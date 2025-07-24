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
                // Check if identifier already exists
                var existingAddress = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Identifier == address.Identifier);
                
                if (existingAddress != null)
                {
                    throw new ApplicationException($"Address with identifier '{address.Identifier}' already exists");
                }

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Address created successfully: {Identifier}", address.Identifier);
                return address;
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

                // Check if new identifier conflicts with another address
                if (address.Identifier != existingAddress.Identifier)
                {
                    var conflictAddress = await _context.Addresses
                        .FirstOrDefaultAsync(a => a.Identifier == address.Identifier);
                    
                    if (conflictAddress != null)
                    {
                        throw new ApplicationException($"Address with identifier '{address.Identifier}' already exists");
                    }
                }

                existingAddress.Identifier = address.Identifier;
                existingAddress.Status = address.Status;
                existingAddress.Message = address.Message;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Address updated successfully: {Id}", id);
                return existingAddress;
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