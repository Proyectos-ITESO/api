using MongoDB.Driver;
using MongoDB.Bson;
using MicroJack.API.Models;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class IntermediateRegistrationService : IIntermediateRegistrationService
    {
        private readonly IMongoCollection<IntermediateRegistration> _intermediateRegistrationsCollection;
        private readonly IPreRegistrationService _preRegistrationService;
        private readonly ILogger<IntermediateRegistrationService> _logger;

        public IntermediateRegistrationService(
            IMongoService mongoService, 
            IPreRegistrationService preRegistrationService,
            ILogger<IntermediateRegistrationService> logger)
        {
            _logger = logger;
            _intermediateRegistrationsCollection = mongoService.Database.GetCollection<IntermediateRegistration>("intermediateregistrations");
            _preRegistrationService = preRegistrationService;
        }

        public async Task<IntermediateRegistration> CreateIntermediateRegistrationAsync(IntermediateRegistration registration)
        {
            registration.CreatedAt = DateTime.UtcNow;
            registration.Status = "AWAITING_APPROVAL";
            registration.ApprovalToken = Guid.NewGuid().ToString();
            
            if (string.IsNullOrEmpty(registration.Id))
            {
                registration.Id = ObjectId.GenerateNewId().ToString();
            }

            _logger.LogInformation("Creating intermediate registration for plates: {Plates}", registration.Plates);
            
            try
            {
                await _intermediateRegistrationsCollection.InsertOneAsync(registration);
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
                var filter = Builders<IntermediateRegistration>.Filter.Eq(ir => ir.ApprovalToken, token);
                return await _intermediateRegistrationsCollection.Find(filter).FirstOrDefaultAsync();
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
                var filter = Builders<IntermediateRegistration>.Filter.Eq(ir => ir.ApprovalToken, token);
                var update = Builders<IntermediateRegistration>.Update
                    .Set(ir => ir.Status, "APPROVED")
                    .Set(ir => ir.ApprovedAt, DateTime.UtcNow);

                var result = await _intermediateRegistrationsCollection.UpdateOneAsync(filter, update);
                
                _logger.LogInformation("Intermediate registration approved and converted to pre-registration. Token: {Token}", token);
                return result.ModifiedCount > 0;
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
                var filter = Builders<IntermediateRegistration>.Filter.Eq(ir => ir.Status, "AWAITING_APPROVAL");
                var sort = Builders<IntermediateRegistration>.Sort.Descending(ir => ir.CreatedAt);
                return await _intermediateRegistrationsCollection.Find(filter).Sort(sort).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending intermediate registrations");
                return new List<IntermediateRegistration>();
            }
        }
    }
}