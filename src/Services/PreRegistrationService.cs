// Services/PreRegistrationService.cs
using Microsoft.EntityFrameworkCore;
using MicroJack.API.Models;
using MicroJack.API.Services.Interfaces;
using MicroJack.API.Data;

namespace MicroJack.API.Services
{
    public class PreRegistrationService : IPreRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PreRegistrationService> _logger;

        public PreRegistrationService(ApplicationDbContext context, ILogger<PreRegistrationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PreRegistration> CreatePreRegistrationAsync(PreRegistration newPreRegistration)
        {
            newPreRegistration.CreatedAt = DateTime.UtcNow;
            newPreRegistration.Status = "PENDIENTE";
            
            // Entity Framework will auto-generate the ID
            newPreRegistration.Id = 0;

            _logger.LogInformation("Intentando crear pre-registro para placas: {Plates}", newPreRegistration.Plates);
            try
            {
                _context.PreRegistrations.Add(newPreRegistration);
                await _context.SaveChangesAsync();
                
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
                return await _context.PreRegistrations
                    .Where(pr => pr.Plates.ToLower() == plate.ToLower() && pr.Status == "PENDIENTE")
                    .FirstOrDefaultAsync();
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
                var query = _context.PreRegistrations.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogInformation("Aplicando filtro de búsqueda: {SearchTerm}", searchTerm);
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(pr => 
                        pr.Plates.ToLower().Contains(searchTermLower) ||
                        pr.VisitorName.ToLower().Contains(searchTermLower));
                }

                return await query
                    .OrderByDescending(pr => pr.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pre-registros.");
                return new List<PreRegistration>();
            }
        }

        public async Task<bool> UpdatePreRegistrationStatusAsync(int id, string newStatus)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(newStatus))
            {
                _logger.LogWarning("Intento de actualizar estado de pre-registro con ID inválido ({Id}) o estado vacío ({Status}).", 
                    id, newStatus);
                return false;
            }

            _logger.LogInformation("Intentando actualizar estado del pre-registro ID: {Id} a '{NewStatus}'", 
                id, newStatus);
            try
            {
                var preRegistration = await _context.PreRegistrations
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (preRegistration == null)
                {
                    _logger.LogWarning("Pre-registro con ID: {Id} no encontrado.", id);
                    return false;
                }

                preRegistration.Status = newStatus.Trim().ToUpperInvariant();
                var changeCount = await _context.SaveChangesAsync();

                if (changeCount > 0)
                {
                    _logger.LogInformation("Estado del pre-registro ID: {Id} actualizado a '{NewStatus}'.", 
                        id, newStatus.ToUpperInvariant());
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se modificó el estado del pre-registro ID: {Id}.", id);
                    return false;
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