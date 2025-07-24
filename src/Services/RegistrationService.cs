using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(ApplicationDbContext context, ILogger<RegistrationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Registration>> GetRegistrationsAsync(string? plate = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(plate))
                {
                    _logger.LogInformation("Buscando registros por placa: {Plate}", plate);
                    return await _context.Registrations
                        .Where(r => r.Plates == plate)
                        .OrderByDescending(r => r.CreatedAt)
                        .ToListAsync();
                }
                else
                {
                    _logger.LogInformation("Obteniendo todos los registros.");
                    return await _context.Registrations
                        .OrderByDescending(r => r.CreatedAt)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo registros.");
                return new List<Registration>();
            }
        }

        public async Task<Registration?> GetRegistrationByIdAsync(int id)
        {
            _logger.LogInformation("Buscando registro por ID: {Id}", id);
            try
            {
                return await _context.Registrations.FindAsync(id);
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

            _logger.LogInformation("Intentando crear registro con Folio (UUID recibido): {Folio}",
                newRegistration.Folio ?? "N/A");
            try
            {
                _context.Registrations.Add(newRegistration);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Registro creado exitosamente: ID={Id}, Folio={Folio}", 
                    newRegistration.Id, newRegistration.Folio ?? "N/A");
                return newRegistration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear registro");
                throw new ApplicationException("Error inesperado al crear el registro.", ex);
            }
        }
    }
}