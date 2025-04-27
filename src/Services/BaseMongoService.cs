// Services/BaseMongoService.cs

using MicroJack.API.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class BaseMongoService : IMongoService
    {
        protected readonly ILogger<BaseMongoService> _logger;
        protected readonly IMongoDatabase _database;

        public IMongoDatabase Database => _database;

        public BaseMongoService(IOptions<MongoDbSettings> mongoDbSettings, ILogger<BaseMongoService> logger)
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
                mongoClientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
                var mongoClient = new MongoClient(mongoClientSettings);

                _database = mongoClient.GetDatabase(settings.DatabaseName);
                _logger.LogInformation($"Conectado a MongoDB. Database: {settings.DatabaseName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar la conexión con MongoDB.");
                throw;
            }
        }
    }
}