// Services/PreRegistrationService.cs
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using MicroJack.API.Models;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class PreRegistrationService : IPreRegistrationService
    {
        private readonly IMongoCollection<PreRegistration> _preRegistrationsCollection;
        private readonly ILogger<PreRegistrationService> _logger;

        public PreRegistrationService(IMongoService mongoService, ILogger<PreRegistrationService> logger)
        {
            _logger = logger;
            _preRegistrationsCollection = mongoService.Database.GetCollection<PreRegistration>("preregistrations");
        }

        public async Task<PreRegistration> CreatePreRegistrationAsync(PreRegistration newPreRegistration)
        {
            newPreRegistration.CreatedAt = DateTime.UtcNow;
            newPreRegistration.Status = "PENDIENTE";
            
            if (string.IsNullOrEmpty(newPreRegistration.Id))
            {
                newPreRegistration.Id = ObjectId.GenerateNewId().ToString();
            }

            _logger.LogInformation("Intentando crear pre-registro para placas: {Plates}", newPreRegistration.Plates);
            try
            {
                await _preRegistrationsCollection.InsertOneAsync(newPreRegistration);
                _logger.LogInformation("Pre-registro creado exitosamente: ID={Id}, Placas={Plates}", 
                    newPreRegistration.Id, newPreRegistration.Plates);
                return newPreRegistration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear pre-registro para placas: {Plates}", 
                    newPreRegistration.Plates);
                throw new ApplicationException("Error inesperado al crear el pre-registro.", ex);
            }
        }

        public async Task<PreRegistration?> GetPendingPreRegistrationByPlateAsync(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate)) return null;

            _logger.LogInformation("Buscando pre-registro PENDIENTE por placa: {Plate}", plate);
            try
            {
                var filterBuilder = Builders<PreRegistration>.Filter;
                var filter = filterBuilder.And(
                    filterBuilder.Regex(pr => pr.Plates, new BsonRegularExpression($"^{plate}$", "i")),
                    filterBuilder.Eq(pr => pr.Status, "PENDIENTE")
                );

                return await _preRegistrationsCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando pre-registro por placa: {Plate}", plate);
                return null;
            }
        }

        public async Task<List<PreRegistration>> GetPreRegistrationsAsync(string? searchTerm = null)
        {
            _logger.LogInformation("Obteniendo todos los pre-registros (Término búsqueda: {SearchTerm})", 
                searchTerm ?? "N/A");
            try
            {
                FilterDefinition<PreRegistration> filter = Builders<PreRegistration>.Filter.Empty;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogInformation("Aplicando filtro de búsqueda: {SearchTerm}", searchTerm);
                    var searchRegex = new BsonRegularExpression(searchTerm, "i");
                    filter = Builders<PreRegistration>.Filter.Or(
                        Builders<PreRegistration>.Filter.Regex(pr => pr.Plates, searchRegex),
                        Builders<PreRegistration>.Filter.Regex(pr => pr.VisitorName, searchRegex)
                    );
                }

                var sort = Builders<PreRegistration>.Sort.Descending(pr => pr.CreatedAt);
                return await _preRegistrationsCollection.Find(filter).Sort(sort).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pre-registros.");
                return new List<PreRegistration>();
            }
        }

        public async Task<bool> UpdatePreRegistrationStatusAsync(string id, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _) || 
                string.IsNullOrWhiteSpace(newStatus))
            {
                _logger.LogWarning("Intento de actualizar estado de pre-registro con ID inválido ({Id}) o estado vacío ({Status}).", 
                    id, newStatus);
                return false;
            }

            _logger.LogInformation("Intentando actualizar estado del pre-registro ID: {Id} a '{NewStatus}'", 
                id, newStatus);
            try
            {
                var filter = Builders<PreRegistration>.Filter.Eq(pr => pr.Id, id);
                var update = Builders<PreRegistration>.Update.Set(pr => pr.Status, 
                    newStatus.Trim().ToUpperInvariant());

                var updateResult = await _preRegistrationsCollection.UpdateOneAsync(filter, update);

                if (updateResult.IsAcknowledged && updateResult.ModifiedCount > 0)
                {
                    _logger.LogInformation("Estado del pre-registro ID: {Id} actualizado a '{NewStatus}'.", 
                        id, newStatus.ToUpperInvariant());
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se modificó el estado del pre-registro ID: {Id}. ¿Existe? (Resultado: Matched={MatchedCount}, Modified={ModifiedCount})",
                        id, updateResult.MatchedCount, updateResult.ModifiedCount);
                    return updateResult.MatchedCount > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado del pre-registro ID: {Id} a '{NewStatus}'", 
                    id, newStatus);
                return false;
            }
        }
    }
}