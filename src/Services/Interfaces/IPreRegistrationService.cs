using MicroJack.API.Models;

namespace MicroJack.API.Services.Interfaces
{
    public interface IPreRegistrationService
    {
        Task<PreRegistration> CreatePreRegistrationAsync(PreRegistration newPreRegistration);
        Task<PreRegistration?> GetPendingPreRegistrationByPlateAsync(string plate);
        Task<List<PreRegistration>> GetPreRegistrationsAsync(string? searchTerm = null);
        Task<bool> UpdatePreRegistrationStatusAsync(int id, string newStatus);
    }
}