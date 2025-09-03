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
                        .ThenInclude(v => v!.Brand)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Color)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Type)
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

        // Nuevos métodos para búsqueda avanzada
        public async Task<List<AccessLog>> GetAccessLogsByDateAsync(DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = date.Date.AddDays(1);

                return await _context.AccessLogs
                    .Include(al => al.Visitor)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Brand)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Color)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Type)
                    .Include(al => al.Address)
                    .Include(al => al.EntryGuard)
                    .Where(al => al.EntryTimestamp >= startDate && al.EntryTimestamp < endDate)
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access logs by date {Date}", date);
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetAccessLogsByVisitorNameAsync(string visitorName)
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
                    .Include(al => al.EntryGuard)
                    .Where(al => al.Visitor.FullName.Contains(visitorName))
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access logs by visitor name {VisitorName}", visitorName);
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetAccessLogsByLicensePlateAsync(string licensePlate)
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
                    .Include(al => al.EntryGuard)
                    .Where(al => al.Vehicle != null && al.Vehicle.LicensePlate.Contains(licensePlate))
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access logs by license plate {LicensePlate}", licensePlate);
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetAccessLogsByVehicleCharacteristicsAsync(int? brandId = null, int? colorId = null, int? typeId = null)
        {
            try
            {
                var query = _context.AccessLogs
                    .Include(al => al.Visitor)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Brand)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Color)
                    .Include(al => al.Vehicle)
                        .ThenInclude(v => v!.Type)
                    .Include(al => al.Address)
                    .Include(al => al.EntryGuard)
                    .Where(al => al.Vehicle != null);

                if (brandId.HasValue)
                {
                    query = query.Where(al => al.Vehicle!.BrandId == brandId);
                }

                if (colorId.HasValue)
                {
                    query = query.Where(al => al.Vehicle!.ColorId == colorId);
                }

                if (typeId.HasValue)
                {
                    query = query.Where(al => al.Vehicle!.TypeId == typeId);
                }

                return await query
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access logs by vehicle characteristics");
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetAccessLogsByAddressIdentifierAsync(string addressIdentifier)
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
                    .Include(al => al.EntryGuard)
                    .Where(al => al.Address.Identifier.Contains(addressIdentifier))
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access logs by address identifier {AddressIdentifier}", addressIdentifier);
                return new List<AccessLog>();
            }
        }

        // Métodos para historial
        public async Task<List<AccessLog>> GetVisitorHistoryAsync(int visitorId)
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
                    .Include(al => al.EntryGuard)
                    .Where(al => al.VisitorId == visitorId)
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visitor history for visitor {VisitorId}", visitorId);
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetVehicleHistoryAsync(string licensePlate)
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
                    .Include(al => al.EntryGuard)
                    .Where(al => al.Vehicle != null && al.Vehicle.LicensePlate == licensePlate)
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle history for license plate {LicensePlate}", licensePlate);
                return new List<AccessLog>();
            }
        }

        public async Task<List<AccessLog>> GetAddressHistoryAsync(int addressId)
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
                    .Include(al => al.EntryGuard)
                    .Where(al => al.AddressId == addressId)
                    .OrderByDescending(al => al.EntryTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address history for address {AddressId}", addressId);
                return new List<AccessLog>();
            }
        }

        // Método para búsqueda combinada
        public async Task<List<AccessLog>> AdvancedSearchAsync(AccessLogSearchRequest request)
        {
            try
            {
                var query = _context.AccessLogs
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
                    .AsQueryable();

                // Filtro por rango de fechas
                if (request.StartDate.HasValue)
                {
                    query = query.Where(al => al.EntryTimestamp >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    var endDate = request.EndDate.Value.AddDays(1);
                    query = query.Where(al => al.EntryTimestamp < endDate);
                }

                // Filtro por nombre de visitante
                if (!string.IsNullOrWhiteSpace(request.VisitorName))
                {
                    query = query.Where(al => al.Visitor.FullName.Contains(request.VisitorName));
                }

                // Filtro por placa
                if (!string.IsNullOrWhiteSpace(request.LicensePlate))
                {
                    query = query.Where(al => al.Vehicle != null && al.Vehicle.LicensePlate.Contains(request.LicensePlate));
                }

                // Filtros por características del vehículo
                if (request.BrandId.HasValue)
                {
                    query = query.Where(al => al.Vehicle != null && al.Vehicle.BrandId == request.BrandId);
                }

                if (request.ColorId.HasValue)
                {
                    query = query.Where(al => al.Vehicle != null && al.Vehicle.ColorId == request.ColorId);
                }

                if (request.TypeId.HasValue)
                {
                    query = query.Where(al => al.Vehicle != null && al.Vehicle.TypeId == request.TypeId);
                }

                // Filtro por identificador de dirección
                if (!string.IsNullOrWhiteSpace(request.AddressIdentifier))
                {
                    query = query.Where(al => al.Address.Identifier.Contains(request.AddressIdentifier));
                }

                // Filtro por estado
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    query = query.Where(al => al.Status == request.Status);
                }

                // Filtro por residente visitado
                if (request.ResidentId.HasValue)
                {
                    query = query.Where(al => al.ResidentVisitedId == request.ResidentId);
                }

                // Ordenamiento y paginación
                var result = await query
                    .OrderByDescending(al => al.EntryTimestamp)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced search");
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