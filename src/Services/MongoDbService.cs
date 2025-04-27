// Services/MongoDbService.cs

using MongoDB.Driver;
using MongoDB.Bson; // Necesario para BsonRegularExpression
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroJack.API.Models; // Ajusta el namespace si es necesario

// Asegúrate que el namespace coincida con el nombre de tu proyecto
namespace MicroJack.API.Services
{
    // Clase para mapear la configuración
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }

    public class MongoDbService
    {
        private readonly IMongoCollection<Registration> _registrationsCollection;
        private readonly IMongoCollection<PreRegistration> _preRegistrationsCollection; // <-- NUEVA COLECCIÓN

        private readonly ILogger<MongoDbService> _logger;

        public MongoDbService(IOptions<MongoDbSettings> mongoDbSettings, ILogger<MongoDbService> logger)
        {
            _logger = logger;
            var settings = mongoDbSettings.Value;

            if (string.IsNullOrEmpty(settings.ConnectionString) || string.IsNullOrEmpty(settings.DatabaseName))
            {
                _logger.LogError("ConnectionString o DatabaseName no configurados correctamente.");
                throw new InvalidOperationException("Configuración de MongoDB incompleta.");
            }

            try
            {
                _logger.LogInformation("Conectando a MongoDB...");
                var mongoClientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
                // Recomendado: Usar ServerApi para compatibilidad futura con Atlas
                mongoClientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
                var mongoClient = new MongoClient(mongoClientSettings);

                var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
                // Nombre exacto de tu colección
                _registrationsCollection = mongoDatabase.GetCollection<Registration>("registrations");
                _preRegistrationsCollection = mongoDatabase.GetCollection<PreRegistration>("preregistrations"); // <-- INICIALIZAR COLECCIÓN

                _logger.LogInformation($"Conectado a MongoDB. Database: {settings.DatabaseName}");

                // Verificar conexión con Ping (opcional)
                // Podrías hacer un ping aquí para asegurar la conexión al inicio
                //  try {
                //      mongoDatabase.RunCommandAsync((Command<BsonDocument>)"{ping: 1}").Wait(TimeSpan.FromSeconds(5)); // Espera corta
                //      _logger.LogInformation("Ping a MongoDB inicial exitoso.");
                //  } catch (TimeoutException) {
                //       _logger.LogWarning("Ping inicial a MongoDB excedió el tiempo límite.");
                //  } catch (Exception ex) {
                //       _logger.LogError(ex, "Error en el ping inicial a MongoDB.");
                //  }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar la conexión con MongoDB.");
                throw;
            }
        }

        // --- Métodos CRUD ---

        public async Task<List<Registration>> GetRegistrationsAsync(string? plate = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(plate))
                {
                    _logger.LogInformation("Buscando registros por placa: {Plate}", plate);
                    // Búsqueda insensible a mayúsculas/minúsculas exacta
                    var filter = Builders<Registration>.Filter.Regex(r => r.Plates,
                        new BsonRegularExpression($"^{plate}$", "i"));
                    return await _registrationsCollection.Find(filter).SortByDescending(r => r.CreatedAt).ToListAsync();
                }
                else
                {
                    _logger.LogInformation("Obteniendo todos los registros.");
                    // Devolver ordenados por fecha de creación descendente
                    return await _registrationsCollection.Find(_ => true).SortByDescending(r => r.CreatedAt)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo registros.");
                // Podrías devolver una lista vacía o relanzar una excepción específica de API
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

        // En Services/MongoDbService.cs
        public async Task<Registration> CreateRegistrationAsync(Registration newRegistration)
        {
            // Las fechas pueden seguir siendo asignadas por el backend
            var now = DateTime.UtcNow;
            newRegistration.CreatedAt = now;
            newRegistration.UpdatedAt = now;
            newRegistration.EntryTimestamp = now;

            // !! IMPORTANTE: El backend YA NO genera el folio/UUID !!
            // Se confía en que newRegistration.Folio viene con el UUID del frontend.
            if (string.IsNullOrEmpty(newRegistration.Folio))
            {
                _logger.LogWarning("Se recibió un intento de registro sin Folio (UUID esperado desde el frontend).");
                // Podrías lanzar un error si es estrictamente requerido
                // throw new ArgumentException("El Folio (UUID) es requerido y no fue proporcionado por el cliente.");
            }
            else if (!Guid.TryParse(newRegistration.Folio, out _)) // Validación opcional de formato UUID
            {
                _logger.LogWarning("El Folio recibido '{Folio}' no parece ser un UUID válido.", newRegistration.Folio);
                // Podrías lanzar un error si quieres forzar el formato UUID
                // throw new ArgumentException("El Folio proporcionado no tiene el formato de UUID válido.");
            }


            if (string.IsNullOrEmpty(newRegistration.Id))
            {
                newRegistration.Id = ObjectId.GenerateNewId().ToString(); // ID de MongoDB
            }

            _logger.LogInformation("Intentando crear registro con ID: {Id}, Folio (UUID recibido): {Folio}",
                newRegistration.Id, newRegistration.Folio ?? "N/A");
            try
            {
                await _registrationsCollection.InsertOneAsync(newRegistration);
                _logger.LogInformation("Registro creado exitosamente: ID={Id}, Folio={Folio}", newRegistration.Id,
                    newRegistration.Folio ?? "N/A");
                return newRegistration; // Devuelve el objeto como se guardó
            }
            // ... (Catch blocks como estaban antes, especialmente el de DuplicateKey si tienes un índice único en 'folio') ...
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear registro ID: {Id}", newRegistration.Id);
                throw new ApplicationException("Error inesperado al crear el registro.", ex);
            }
        }
        
        
        public async Task<PreRegistration> CreatePreRegistrationAsync(PreRegistration newPreRegistration)
        {
            // Asignar valores por defecto si es necesario
            newPreRegistration.CreatedAt = DateTime.UtcNow;
            newPreRegistration.Status = "PENDIENTE"; // Asegurar estado inicial
            if (string.IsNullOrEmpty(newPreRegistration.Id))
            {
                newPreRegistration.Id = ObjectId.GenerateNewId().ToString();
            }

            _logger.LogInformation("Intentando crear pre-registro para placas: {Plates}", newPreRegistration.Plates);
            try
            {
                await _preRegistrationsCollection.InsertOneAsync(newPreRegistration);
                _logger.LogInformation("Pre-registro creado exitosamente: ID={Id}, Placas={Plates}", newPreRegistration.Id, newPreRegistration.Plates);
                return newPreRegistration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear pre-registro para placas: {Plates}", newPreRegistration.Plates);
                throw new ApplicationException("Error inesperado al crear el pre-registro.", ex);
            }
        }

        public async Task<PreRegistration?> GetPendingPreRegistrationByPlateAsync(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate)) return null;

            _logger.LogInformation("Buscando pre-registro PENDIENTE por placa: {Plate}", plate);
            try
            {
                // Busca por placa (insensible a mayúsculas) Y que el estado sea PENDIENTE
                var filterBuilder = Builders<PreRegistration>.Filter;
                var filter = filterBuilder.And(
                    filterBuilder.Regex(pr => pr.Plates, new BsonRegularExpression($"^{plate}$", "i")),
                    filterBuilder.Eq(pr => pr.Status, "PENDIENTE")
                );

                // Podrías ordenar por fecha de llegada esperada si es relevante
                // var sort = Builders<PreRegistration>.Sort.Ascending(pr => pr.ArrivalDateTime);

                // Devolver el primer pre-registro pendiente que coincida
                return await _preRegistrationsCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando pre-registro por placa: {Plate}", plate);
                return null; // O relanzar si prefieres
            }
        }
        
        // --- NUEVO MÉTODO PARA OBTENER TODOS LOS PRE-REGISTROS ---
        public async Task<List<PreRegistration>> GetPreRegistrationsAsync(string? searchTerm = null) // Añadir búsqueda opcional
        {
            _logger.LogInformation("Obteniendo todos los pre-registros (Término búsqueda: {SearchTerm})", searchTerm ?? "N/A");
            try
            {
                FilterDefinition<PreRegistration> filter = Builders<PreRegistration>.Filter.Empty; // Por defecto, obtener todos

                // Si se proporciona un término de búsqueda (opcional)
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogInformation("Aplicando filtro de búsqueda: {SearchTerm}", searchTerm);
                    // Busca en placas o nombre del visitante (insensible a mayúsculas)
                    var searchRegex = new BsonRegularExpression(searchTerm, "i");
                    filter = Builders<PreRegistration>.Filter.Or(
                        Builders<PreRegistration>.Filter.Regex(pr => pr.Plates, searchRegex),
                        Builders<PreRegistration>.Filter.Regex(pr => pr.VisitorName, searchRegex)
                        // Añade más campos a la búsqueda si lo necesitas
                    );
                }

                // Ordenar por fecha de creación descendente (los más nuevos primero)
                var sort = Builders<PreRegistration>.Sort.Descending(pr => pr.CreatedAt);

                return await _preRegistrationsCollection.Find(filter).Sort(sort).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pre-registros.");
                return new List<PreRegistration>(); // Devuelve lista vacía en caso de error
            }
        }
        
        // --- NUEVO MÉTODO PARA ACTUALIZAR ESTADO DE PRE-REGISTRO ---
        public async Task<bool> UpdatePreRegistrationStatusAsync(string id, string newStatus)
        {
            // Validar ID y nuevo estado
            if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _) || string.IsNullOrWhiteSpace(newStatus))
            {
                _logger.LogWarning("Intento de actualizar estado de pre-registro con ID inválido ({Id}) o estado vacío ({Status}).", id, newStatus);
                return false;
            }

            _logger.LogInformation("Intentando actualizar estado del pre-registro ID: {Id} a '{NewStatus}'", id, newStatus);
            try
            {
                var filter = Builders<PreRegistration>.Filter.Eq(pr => pr.Id, id);
                // Define la actualización para cambiar solo el campo 'Status'
                var update = Builders<PreRegistration>.Update.Set(pr => pr.Status, newStatus.Trim().ToUpperInvariant()); // Guarda en mayúsculas

                var updateResult = await _preRegistrationsCollection.UpdateOneAsync(filter, update);

                if (updateResult.IsAcknowledged && updateResult.ModifiedCount > 0)
                {
                    _logger.LogInformation("Estado del pre-registro ID: {Id} actualizado a '{NewStatus}'.", id, newStatus.ToUpperInvariant());
                    return true;
                }
                else
                {
                    // Puede que el ID no exista o que el estado ya fuera el mismo
                    _logger.LogWarning("No se modificó el estado del pre-registro ID: {Id}. ¿Existe? (Resultado: Matched={MatchedCount}, Modified={ModifiedCount})",
                        id, updateResult.MatchedCount, updateResult.ModifiedCount);
                    return updateResult.MatchedCount > 0; // Devuelve true si lo encontró aunque no lo modificara
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado del pre-registro ID: {Id} a '{NewStatus}'", id, newStatus);
                // No relanzar necesariamente, el registro principal ya se creó. Podríamos retornar false.
                return false;
            }
        }
        
    }
}