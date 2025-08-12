using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class PreRegistrationService : IPreRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PreRegistrationService> _logger;
        private readonly IWhatsAppService _whatsAppService;

        public PreRegistrationService(ApplicationDbContext context, ILogger<PreRegistrationService> logger, IWhatsAppService whatsAppService)
        {
            _context = context;
            _logger = logger;
            _whatsAppService = whatsAppService;
        }

        public async Task<List<PreRegistration>> GetAllPreRegistrationsAsync()
        {
            try
            {
                return await _context.PreRegistrations
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all pre-registrations");
                throw;
            }
        }

        public async Task<PreRegistration?> GetPreRegistrationByIdAsync(int id)
        {
            try
            {
                return await _context.PreRegistrations
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pre-registration by ID: {Id}", id);
                throw;
            }
        }

        public async Task<PreRegistration?> GetPreRegistrationByIdentifierAsync(string plates)
        {
            try
            {
                var preReg = await _context.PreRegistrations
                    .FirstOrDefaultAsync(p => p.Plates == plates && p.Status == "PENDIENTE");

                // Validar ventana de tiempo de ±2 horas
                if (preReg != null)
                {
                    var now = DateTime.UtcNow;
                    var timeDiff = Math.Abs((now - preReg.ExpectedArrivalTime).TotalHours);
                    
                    if (timeDiff > 2)
                    {
                        _logger.LogWarning("Pre-registration for plates {Plates} is outside time window. Expected: {Expected}, Current: {Current}, Diff: {Diff}hrs", 
                            plates, preReg.ExpectedArrivalTime, now, timeDiff);
                        return null; // Fuera de ventana de tiempo
                    }
                }

                return preReg;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pre-registration by plates: {Plates}", plates);
                throw;
            }
        }

        public async Task<List<PreRegistration>> GetActivePreRegistrationsAsync()
        {
            try
            {
                return await _context.PreRegistrations
                    .Where(p => p.Status == "PENDIENTE")
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active pre-registrations");
                throw;
            }
        }

        public async Task<PreRegistration> CreatePreRegistrationAsync(PreRegistration preRegistration)
        {
            try
            {
                // Check if plates already exists with PENDIENTE status
                var existing = await _context.PreRegistrations
                    .FirstOrDefaultAsync(p => p.Plates == preRegistration.Plates && p.Status == "PENDIENTE");
                
                if (existing != null)
                {
                    throw new InvalidOperationException($"Pre-registration with plates '{preRegistration.Plates}' already exists");
                }

                preRegistration.CreatedAt = DateTime.UtcNow;
                preRegistration.Status = "PENDIENTE";

                _context.PreRegistrations.Add(preRegistration);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created pre-registration with ID {Id} for plates: {Plates}", preRegistration.Id, preRegistration.Plates);

                // After creating, send WhatsApp notification
                try
                {
                    var address = await _context.Addresses
                        .Include(a => a.RepresentativeResident)
                        .FirstOrDefaultAsync(a => a.Identifier == preRegistration.HouseVisited);

                    if (address?.RepresentativeResident != null && !string.IsNullOrEmpty(address.RepresentativeResident.Phone))
                    {
                        _logger.LogInformation("Found representative resident {ResidentName} for address {Address}. Sending notification.",
                            address.RepresentativeResident.FullName, address.Identifier);

                        await _whatsAppService.SendPreRegistrationNotificationAsync(
                            address.RepresentativeResident.Phone,
                            address.RepresentativeResident.FullName,
                            preRegistration.VisitorName,
                            preRegistration.ExpectedArrivalTime
                        );
                    }
                    else
                    {
                        _logger.LogWarning("Could not find a representative resident with a phone number for address identifier '{Identifier}' to send notification.",
                            preRegistration.HouseVisited);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't let it crash the main operation
                    _logger.LogError(ex, "Failed to send WhatsApp notification for pre-registration {Id}.", preRegistration.Id);
                }

                return preRegistration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pre-registration");
                throw;
            }
        }

        public async Task<PreRegistration?> UpdatePreRegistrationAsync(int id, PreRegistration preRegistration)
        {
            try
            {
                var existing = await _context.PreRegistrations.FindAsync(id);
                if (existing == null)
                    return null;

                existing.VisitorName = preRegistration.VisitorName;
                existing.HouseVisited = preRegistration.HouseVisited;
                existing.PersonVisited = preRegistration.PersonVisited;
                existing.VehicleBrand = preRegistration.VehicleBrand;
                existing.VehicleColor = preRegistration.VehicleColor;
                existing.Comments = preRegistration.Comments;
                existing.ExpiresAt = preRegistration.ExpiresAt;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated pre-registration ID: {Id}", id);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pre-registration ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> MarkAsUsedAsync(string plates)
        {
            try
            {
                var preReg = await _context.PreRegistrations
                    .FirstOrDefaultAsync(p => p.Plates == plates && p.Status == "PENDIENTE");

                if (preReg == null)
                    return false;

                preReg.Status = "DENTRO";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Marked pre-registration as DENTRO: {Plates}", plates);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking pre-registration as DENTRO: {Plates}", plates);
                throw;
            }
        }

        public async Task<bool> MarkAsExitAsync(string plates)
        {
            try
            {
                var preReg = await _context.PreRegistrations
                    .FirstOrDefaultAsync(p => p.Plates == plates && p.Status == "DENTRO");

                if (preReg == null)
                    return false;

                preReg.Status = "FUERA";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Marked pre-registration as FUERA: {Plates}", plates);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking pre-registration as FUERA: {Plates}", plates);
                throw;
            }
        }

        public async Task<bool> DeletePreRegistrationAsync(int id)
        {
            try
            {
                var preReg = await _context.PreRegistrations.FindAsync(id);
                if (preReg == null)
                    return false;

                _context.PreRegistrations.Remove(preReg);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted pre-registration ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting pre-registration ID: {Id}", id);
                throw;
            }
        }

        public async Task<List<PreRegistration>> SearchPreRegistrationsAsync(string searchTerm)
        {
            try
            {
                return await _context.PreRegistrations
                    .Where(p => p.Plates.Contains(searchTerm) ||
                               p.VisitorName.Contains(searchTerm) ||
                               p.HouseVisited.Contains(searchTerm) ||
                               p.PersonVisited.Contains(searchTerm))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching pre-registrations with term: {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
}