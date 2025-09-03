using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AddressService> _logger;

        public AddressService(ApplicationDbContext context, ILogger<AddressService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Address>> GetAllAddressesAsync()
        {
            try
            {
                return await _context.Addresses
                    .Include(a => a.Residents)
                    .OrderBy(a => a.Identifier)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all addresses");
                return new List<Address>();
            }
        }

        public async Task<Address?> GetAddressByIdAsync(int id)
        {
            try
            {
                return await _context.Addresses
                    .Include(a => a.Residents)
                    .FirstOrDefaultAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address by ID {Id}", id);
                return null;
            }
        }

        public async Task<List<Address>> SearchAddressesAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllAddressesAsync();
                }

                return await _context.Addresses
                    .Include(a => a.Residents)
                    .Where(a => a.Identifier.Contains(searchTerm) ||
                               (a.Status != null && a.Status.Contains(searchTerm)) ||
                               (a.Message != null && a.Message.Contains(searchTerm)))
                    .OrderBy(a => a.Identifier)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching addresses with term {SearchTerm}", searchTerm);
                return new List<Address>();
            }
        }

        public async Task<Address> CreateAddressAsync(Address address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address.Identifier))
                    throw new ApplicationException("Identifier is required");
                if (string.IsNullOrWhiteSpace(address.Extension))
                    throw new ApplicationException("Extension is required");

                address.Identifier = address.Identifier.Trim();
                address.Extension = address.Extension.Trim();

                // Ensure unique by extension
                var extConflict = await _context.Addresses.AnyAsync(a => a.Extension == address.Extension);
                if (extConflict)
                    throw new ApplicationException($"Extension '{address.Extension}' already exists");

                // Optionally ensure identifier uniqueness (current behavior)
                var idConflict = await _context.Addresses.AnyAsync(a => a.Identifier == address.Identifier);
                if (idConflict)
                    throw new ApplicationException($"Address with identifier '{address.Identifier}' already exists");

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Address created successfully: {Identifier}", address.Identifier);
                return address;
            }
            catch (DbUpdateException dbex) when (dbex.InnerException is Microsoft.Data.Sqlite.SqliteException sqlEx)
            {
                // Map common constraint errors to user-friendly messages
                if (sqlEx.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
                {
                    var msg = sqlEx.Message.Contains("Addresses.Extension", StringComparison.OrdinalIgnoreCase)
                        ? "Extension already exists"
                        : sqlEx.Message.Contains("Addresses.Identifier", StringComparison.OrdinalIgnoreCase)
                            ? "Identifier already exists" : "Unique constraint failed";
                    _logger.LogWarning(dbex, "Address create conflict: {Message}", msg);
                    throw new ApplicationException(msg);
                }
                if (sqlEx.Message.Contains("NOT NULL constraint failed", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(dbex, "Address create validation error: {Message}", sqlEx.Message);
                    throw new ApplicationException("Missing required field (Identifier/Extension)");
                }
                _logger.LogError(dbex, "DB error creating address");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                throw;
            }
        }

        public async Task<Address?> UpdateAddressAsync(int id, Address address)
        {
            try
            {
                var existingAddress = await _context.Addresses.FindAsync(id);
                if (existingAddress == null)
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(address.Identifier))
                    throw new ApplicationException("Identifier is required");

                var newIdentifier = address.Identifier.Trim();
                var newExtension = address.Extension?.Trim();

                // Check if new identifier conflicts with another address (current behavior)
                if (!string.Equals(existingAddress.Identifier, newIdentifier, StringComparison.Ordinal))
                {
                    var conflictAddress = await _context.Addresses
                        .FirstOrDefaultAsync(a => a.Identifier == newIdentifier);
                    if (conflictAddress != null)
                        throw new ApplicationException($"Address with identifier '{newIdentifier}' already exists");
                }

                // Check extension uniqueness if changed and provided
                if (!string.IsNullOrWhiteSpace(newExtension) && !string.Equals(existingAddress.Extension, newExtension, StringComparison.Ordinal))
                {
                    var extConflict = await _context.Addresses.AnyAsync(a => a.Extension == newExtension);
                    if (extConflict)
                        throw new ApplicationException($"Extension '{newExtension}' already exists");
                }

                existingAddress.Identifier = newIdentifier;
                if (!string.IsNullOrWhiteSpace(newExtension))
                {
                    existingAddress.Extension = newExtension;
                }
                existingAddress.Status = address.Status;
                existingAddress.Message = address.Message;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Address updated successfully: {Id}", id);
                return existingAddress;
            }
            catch (DbUpdateException dbex) when (dbex.InnerException is Microsoft.Data.Sqlite.SqliteException sqlEx)
            {
                if (sqlEx.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
                {
                    var msg = sqlEx.Message.Contains("Addresses.Extension", StringComparison.OrdinalIgnoreCase)
                        ? "Extension already exists"
                        : sqlEx.Message.Contains("Addresses.Identifier", StringComparison.OrdinalIgnoreCase)
                            ? "Identifier already exists" : "Unique constraint failed";
                    _logger.LogWarning(dbex, "Address update conflict: {Message}", msg);
                    throw new ApplicationException(msg);
                }
                if (sqlEx.Message.Contains("NOT NULL constraint failed", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(dbex, "Address update validation error: {Message}", sqlEx.Message);
                    throw new ApplicationException("Missing required field (Identifier/Extension)");
                }
                _logger.LogError(dbex, "DB error updating address {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAddressAsync(int id)
        {
            try
            {
                var address = await _context.Addresses
                    .Include(a => a.Residents)
                    .Include(a => a.AccessLogs)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (address == null)
                {
                    return false;
                }

                // Check if address has associated records
                if (address.Residents.Any() || address.AccessLogs.Any())
                {
                    throw new ApplicationException("Cannot delete address with associated residents or access logs");
                }

                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Address deleted successfully: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {Id}", id);
                throw;
            }
        }

        public async Task<List<Address>> GetAddressesByStatusAsync(string status)
        {
            try
            {
                return await _context.Addresses
                    .Include(a => a.Residents)
                    .Where(a => a.Status == status)
                    .OrderBy(a => a.Identifier)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses by status {Status}", status);
                return new List<Address>();
            }
        }
    }
}
