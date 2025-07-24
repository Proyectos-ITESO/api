using Microsoft.EntityFrameworkCore;
using MicroJack.API.Models;
using MicroJack.API.Services.Interfaces;
using MicroJack.API.Data;

namespace MicroJack.API.Services
{
    public class IntermediateRegistrationService : IIntermediateRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPreRegistrationService _preRegistrationService;
        private readonly ILogger<IntermediateRegistrationService> _logger;

        public IntermediateRegistrationService(
            ApplicationDbContext context, 
            IPreRegistrationService preRegistrationService,
            ILogger<IntermediateRegistrationService> logger)
        {
            _context = context;
            _preRegistrationService = preRegistrationService;
            _logger = logger;
        }

        public async Task<IntermediateRegistration> CreateIntermediateRegistrationAsync(IntermediateRegistration registration)
        {
            registration.CreatedAt = DateTime.UtcNow;
            registration.Status = "AWAITING_APPROVAL";
            registration.ApprovalToken = Guid.NewGuid().ToString();
            
            // Entity Framework will auto-generate the ID
            registration.Id = 0;

            _logger.LogInformation("Creating intermediate registration for plates: {Plates}", registration.Plates);
            
            try
            {
                _context.IntermediateRegistrations.Add(registration);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Intermediate registration created: ID={Id}, Token={Token}", 
                    registration.Id, registration.ApprovalToken);
                return registration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating intermediate registration for plates: {Plates}", 
                    registration.Plates);
                throw new ApplicationException("Error creating intermediate registration.", ex);
            }
        }

        public async Task<IntermediateRegistration?> GetIntermediateRegistrationByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            _logger.LogInformation("Getting intermediate registration by token: {Token}", token);
            
            try
            {
                return await _context.IntermediateRegistrations
                    .FirstOrDefaultAsync(ir => ir.ApprovalToken == token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting intermediate registration by token: {Token}", token);
                return null;
            }
        }

        public async Task<bool> ApproveIntermediateRegistrationAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            _logger.LogInformation("Approving intermediate registration with token: {Token}", token);
            
            try
            {
                var intermediate = await GetIntermediateRegistrationByTokenAsync(token);
                if (intermediate == null)
                {
                    _logger.LogWarning("Intermediate registration not found for token: {Token}", token);
                    return false;
                }

                // Check if already approved
                if (intermediate.Status == "APPROVED")
                {
                    _logger.LogInformation("Intermediate registration already approved for token: {Token}", token);
                    return true; // Consider it successful since it was already approved
                }

                // Check if a pre-registration with the same plates already exists to prevent duplicates
                var existingPreReg = await _preRegistrationService.GetPendingPreRegistrationByPlateAsync(intermediate.Plates);
                if (existingPreReg != null)
                {
                    _logger.LogWarning("Pre-registration already exists for plates: {Plates}. Marking intermediate as approved.", intermediate.Plates);
                    
                    // Update intermediate status to approved without creating duplicate
                    intermediate.Status = "APPROVED";
                    intermediate.ApprovedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

                // Create PreRegistration from intermediate
                var preRegistration = new PreRegistration
                {
                    Plates = intermediate.Plates,
                    VisitorName = intermediate.VisitorName,
                    Brand = intermediate.Brand,
                    Color = intermediate.Color,
                    HouseVisited = $"{intermediate.CotoName} - Casa {intermediate.HouseNumber}",
                    ArrivalDateTime = intermediate.ArrivalDateTime,
                    PersonVisited = intermediate.PersonVisited,
                    Status = "PENDIENTE"
                };

                await _preRegistrationService.CreatePreRegistrationAsync(preRegistration);

                // Update intermediate status
                intermediate.Status = "APPROVED";
                intermediate.ApprovedAt = DateTime.UtcNow;
                var changeCount = await _context.SaveChangesAsync();
                
                _logger.LogInformation("Intermediate registration approved and converted to pre-registration. Token: {Token}", token);
                return changeCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving intermediate registration with token: {Token}", token);
                return false;
            }
        }

        public async Task<List<IntermediateRegistration>> GetPendingIntermediateRegistrationsAsync()
        {
            _logger.LogInformation("Getting pending intermediate registrations");
            
            try
            {
                return await _context.IntermediateRegistrations
                    .Where(ir => ir.Status == "AWAITING_APPROVAL")
                    .OrderByDescending(ir => ir.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending intermediate registrations");
                return new List<IntermediateRegistration>();
            }
        }
    }
}