using MicroJack.API.Models.Core;

namespace MicroJack.API.Services.Interfaces
{
    public interface IPreRegistrationService
    {
        Task<List<PreRegistration>> GetAllPreRegistrationsAsync();
        Task<PreRegistration?> GetPreRegistrationByIdAsync(int id);
        Task<PreRegistration?> GetPreRegistrationByIdentifierAsync(string plates);
        Task<List<PreRegistration>> GetActivePreRegistrationsAsync(); // Status = "PENDIENTE"
        Task<PreRegistration> CreatePreRegistrationAsync(PreRegistration preRegistration);
        Task<PreRegistration?> UpdatePreRegistrationAsync(int id, PreRegistration preRegistration);
        Task<bool> MarkAsUsedAsync(string plates); // Cambiar a DENTRO
        Task<bool> MarkAsExitAsync(string plates); // Cambiar a FUERA
        Task<bool> DeletePreRegistrationAsync(int id);
        Task<List<PreRegistration>> SearchPreRegistrationsAsync(string searchTerm);
    }
}