using MicroJack.API.Models.Core;

namespace MicroJack.API.Services.Interfaces
{
    public interface IGuardService
    {
        Task<List<Guard>> GetAllGuardsAsync();
        Task<Guard?> GetGuardByIdAsync(int id);
        Task<Guard?> GetGuardByUsernameAsync(string username);
        Task<Guard> CreateGuardAsync(Guard guard, string password);
        Task<Guard?> UpdateGuardAsync(int id, Guard guard);
        Task<bool> DeleteGuardAsync(int id);
        Task<bool> UpdatePasswordAsync(int id, string newPassword);
        Task<bool> ActivateDeactivateGuardAsync(int id, bool isActive);
        Task<Guard?> AuthenticateAsync(string username, string password);
    }
}