// Services/Interfaces/IRegistrationService.cs
using MicroJack.API.Models;

namespace MicroJack.API.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<List<Registration>> GetRegistrationsAsync(string? plate = null);
        Task<Registration?> GetRegistrationByIdAsync(string id);
        Task<Registration> CreateRegistrationAsync(Registration newRegistration);
    }
}