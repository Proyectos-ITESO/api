using MicroJack.API.Services;

namespace MicroJack.API.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> LoginAsync(string username, string password);
        Task<bool> LogoutAsync(int guardId);
        Task<bool> ChangePasswordAsync(int guardId, string currentPassword, string newPassword);
        Task<bool> ValidateTokenAsync(string token);
    }
}