using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Transaction;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class AccessLogService : IAccessLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccessLogService> _logger;

        public AccessLogService(ApplicationDbContext context, ILogger<AccessLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AccessLog>> GetAllAccessLogsAsync()
        {
            try
            {
                return await _context.AccessLogs
                    .Include(al => al.Visitor)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Brand)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Color)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Type)
                    .Include(al => al.Address)
                    .Include(al => al.ResidentVisited)
                    .Include(al => al.EntryGuard)
                    .Include(al => al.ExitGuard)
                    .Include(al => al.VisitReason)
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all access logs");
                return new List<AccessLog>();
            }
        }

        public async Task<AccessLog?> GetAccessLogByIdAsync(int id)
        {
            try
            {
                return await _context.AccessLogs
                    .Include(al => al.Visitor)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Brand)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Color)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Type)
                    .Include(al => al.Address)
                    .Include(al => al.ResidentVisited)
                    .Include(al => al.EntryGuard)
                    .Include(al => al.ExitGuard)
                    .Include(al => al.VisitReason)
                    .FirstOrDefaultAsync(al => al.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access log by ID {Id}", id);
                return null;
            }
        }

        public async Task<List<AccessLog>> GetAccessLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.AccessLogs
                    .Include(al => al.Visitor)
                    .Include(al => al.Vehicle)
                    .Include(al => al.Address)
                    .Include(al => al.EntryGuard)
                    .Where(al => al.EntryTimestamp >= startDate && al.EntryTimestamp <= endDate)
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access logs by date range");
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetAccessLogsByVisitorAsync(int visitorId)
        {
            try
            {
                return await _context.AccessLogs
                    .Include(al => al.Vehicle)
                    .Include(al => al.Address)
                    .Include(al => al.EntryGuard)
                    .Where(al => al.VisitorId == visitorId)
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access logs by visitor {VisitorId}", visitorId);
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetAccessLogsByVehicleAsync(int vehicleId)
        {
            try
            {
                return await _context.AccessLogs
                    .Include(al => al.Visitor)
                    .Include(al => al.Address)
                    .Include(al => al.EntryGuard)
                    .Where(al => al.VehicleId == vehicleId)
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access logs by vehicle {VehicleId}", vehicleId);
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetAccessLogsByAddressAsync(int addressId)
        {
            try
            {
                return await _context.AccessLogs
                    .Include(al => al.Visitor)
                    .Include(al => al.Vehicle)
                    .Include(al => al.EntryGuard)
                    .Where(al => al.AddressId == addressId)
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access logs by address {AddressId}", addressId);
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetAccessLogsByStatusAsync(string status)
        {
            try
            {
                return await _context.AccessLogs
                    .Include(al => al.Visitor)
                    .Include(al => al.Vehicle)
                    .Include(al => al.Address)
                    .Include(al => al.EntryGuard)
                    .Where(al => al.Status == status)
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access logs by status {Status}", status);
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetActiveAccessLogsAsync()
        {
            return await GetAccessLogsByStatusAsync("DENTRO");
        }

        public async Task<AccessLog> CreateAccessLogAsync(AccessLog accessLog)
        {
            try
            {
                // Validate foreign key references
                await ValidateAccessLogReferencesAsync(accessLog);

                _context.AccessLogs.Add(accessLog);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Access log created successfully with ID: {Id}", accessLog.Id);
                return accessLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating access log");
                throw;
            }
        }

        public async Task<AccessLog?> UpdateAccessLogAsync(int id, AccessLog accessLog)
        {
            try
            {
                var existingLog = await _context.AccessLogs.FindAsync(id);
                if (existingLog == null)
                {
                    return null;
                }

                // Validate foreign key references
                await ValidateAccessLogReferencesAsync(accessLog);

                existingLog.Status = accessLog.Status;
                existingLog.Comments = accessLog.Comments;
                existingLog.ExitTimestamp = accessLog.ExitTimestamp;
                existingLog.ExitGuardId = accessLog.ExitGuardId;
                existingLog.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Access log updated successfully: {Id}", id);
                return existingLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating access log {Id}", id);
                throw;
            }
        }

        public async Task<bool> RegisterExitAsync(int id, int exitGuardId)
        {
            try
            {
                var accessLog = await _context.AccessLogs.FindAsync(id);
                if (accessLog == null)
                {
                    return false;
                }

                // Validate guard exists
                var guardExists = await _context.Guards.AnyAsync(g => g.Id == exitGuardId);
                if (!guardExists)
                {
                    throw new ApplicationException($"Guard with ID {exitGuardId} does not exist");
                }

                accessLog.ExitTimestamp = DateTime.UtcNow;
                accessLog.ExitGuardId = exitGuardId;
                accessLog.Status = "FUERA";
                accessLog.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Exit registered successfully for access log {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering exit for access log {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAccessLogAsync(int id)
        {
            try
            {
                var accessLog = await _context.AccessLogs.FindAsync(id);
                if (accessLog == null)
                {
                    return false;
                }

                _context.AccessLogs.Remove(accessLog);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Access log deleted successfully: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting access log {Id}", id);
                throw;
            }
        }

        public async Task<List<AccessLog>> SearchAccessLogsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllAccessLogsAsync();
                }

                return await _context.AccessLogs
                    .Include(al => al.Visitor)
                    .Include(al => al.Vehicle)
                    .Include(al => al.Address)
                    .Include(al => al.EntryGuard)
                    .Where(al => al.Visitor.FullName.Contains(searchTerm) ||
                               (al.Vehicle != null && al.Vehicle.LicensePlate.Contains(searchTerm)) ||
                               al.Address.Identifier.Contains(searchTerm) ||
                               al.Status.Contains(searchTerm) ||
                               (al.Comments != null && al.Comments.Contains(searchTerm)))
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching access logs with term {SearchTerm}", searchTerm);
                return new List<AccessLog>();
            }
        }

        private async Task ValidateAccessLogReferencesAsync(AccessLog accessLog)
        {
            // Validate visitor exists
            var visitorExists = await _context.Visitors.AnyAsync(v => v.Id == accessLog.VisitorId);
            if (!visitorExists)
            {
                throw new ApplicationException($"Visitor with ID {accessLog.VisitorId} does not exist");
            }

            // Validate vehicle exists (if provided)
            if (accessLog.VehicleId.HasValue)
            {
                var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == accessLog.VehicleId);
                if (!vehicleExists)
                {
                    throw new ApplicationException($"Vehicle with ID {accessLog.VehicleId} does not exist");
                }
            }

            // Validate address exists
            var addressExists = await _context.Addresses.AnyAsync(a => a.Id == accessLog.AddressId);
            if (!addressExists)
            {
                throw new ApplicationException($"Address with ID {accessLog.AddressId} does not exist");
            }

            // Validate resident exists (if provided)
            if (accessLog.ResidentVisitedId.HasValue)
            {
                var residentExists = await _context.Residents.AnyAsync(r => r.Id == accessLog.ResidentVisitedId);
                if (!residentExists)
                {
                    throw new ApplicationException($"Resident with ID {accessLog.ResidentVisitedId} does not exist");
                }
            }

            // Validate entry guard exists
            var entryGuardExists = await _context.Guards.AnyAsync(g => g.Id == accessLog.EntryGuardId);
            if (!entryGuardExists)
            {
                throw new ApplicationException($"Entry guard with ID {accessLog.EntryGuardId} does not exist");
            }

            // Validate exit guard exists (if provided)
            if (accessLog.ExitGuardId.HasValue)
            {
                var exitGuardExists = await _context.Guards.AnyAsync(g => g.Id == accessLog.ExitGuardId);
                if (!exitGuardExists)
                {
                    throw new ApplicationException($"Exit guard with ID {accessLog.ExitGuardId} does not exist");
                }
            }

            // Validate visit reason exists (if provided)
            if (accessLog.VisitReasonId.HasValue)
            {
                var visitReasonExists = await _context.VisitReasons.AnyAsync(vr => vr.Id == accessLog.VisitReasonId);
                if (!visitReasonExists)
                {
                    throw new ApplicationException($"Visit reason with ID {accessLog.VisitReasonId} does not exist");
                }
            }
        }
    }
}