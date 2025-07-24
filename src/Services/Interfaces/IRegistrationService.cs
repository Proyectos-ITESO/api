using MicroJack.API.Models;

namespace MicroJack.API.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<List<Registration>> GetRegistrationsAsync(string? plate = null);
        Task<Registration?> GetRegistrationByIdAsync(int id);
        Task<Registration> CreateRegistrationAsync(Registration newRegistration);
    }
}