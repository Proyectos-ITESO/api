// Services/RegistrationService.cs
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using MicroJack.API.Models;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IMongoCollection<Registration> _registrationsCollection;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(IMongoService mongoService, ILogger<RegistrationService> logger)
        {
            _logger = logger;
            _registrationsCollection = mongoService.Database.GetCollection<Registration>("registrations");
        }

        public async Task<List<Registration>> GetRegistrationsAsync(string? plate = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(plate))
                {
                    _logger.LogInformation("Buscando registros por placa: {Plate}", plate);
                    var filter = Builders<Registration>.Filter.Regex(r => r.Plates,
                        new BsonRegularExpression($"^{plate}$", "i"));
                    return await _registrationsCollection.Find(filter)
                        .SortByDescending(r => r.CreatedAt)
                        .ToListAsync();
                }
                else
                {
                    _logger.LogInformation("Obteniendo todos los registros.");
                    return await _registrationsCollection.Find(_ => true)
                        .SortByDescending(r => r.CreatedAt)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo registros.");
                return new List<Registration>();
            }
        }

        public async Task<Registration?> GetRegistrationByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                _logger.LogWarning("ID inválido para búsqueda: {Id}", id);
                return null;
            }

            _logger.LogInformation("Buscando registro por ID: {Id}", id);
            try
            {
                return await _registrationsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando registro por ID: {Id}", id);
                return null;
            }
        }

        public async Task<Registration> CreateRegistrationAsync(Registration newRegistration)
        {
            var now = DateTime.UtcNow;
            newRegistration.CreatedAt = now;
            newRegistration.UpdatedAt = now;
            newRegistration.EntryTimestamp = now;

            if (string.IsNullOrEmpty(newRegistration.Folio))
            {
                _logger.LogWarning("Se recibió un intento de registro sin Folio (UUID esperado desde el frontend).");
            }
            else if (!Guid.TryParse(newRegistration.Folio, out _))
            {
                _logger.LogWarning("El Folio recibido '{Folio}' no parece ser un UUID válido.", newRegistration.Folio);
            }

            if (string.IsNullOrEmpty(newRegistration.Id))
            {
                newRegistration.Id = ObjectId.GenerateNewId().ToString();
            }

            _logger.LogInformation("Intentando crear registro con ID: {Id}, Folio (UUID recibido): {Folio}",
                newRegistration.Id, newRegistration.Folio ?? "N/A");
            try
            {
                await _registrationsCollection.InsertOneAsync(newRegistration);
                _logger.LogInformation("Registro creado exitosamente: ID={Id}, Folio={Folio}", 
                    newRegistration.Id, newRegistration.Folio ?? "N/A");
                return newRegistration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear registro ID: {Id}", newRegistration.Id);
                throw new ApplicationException("Error inesperado al crear el registro.", ex);
            }
        }
    }
}