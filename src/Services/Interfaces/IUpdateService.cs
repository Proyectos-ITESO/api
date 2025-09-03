using MicroJack.API.Models.Update;

namespace MicroJack.API.Services.Interfaces;

public interface IUpdateService
{
    Task<UpdateCheckResponse> CheckForUpdatesAsync();
    Task<bool> DownloadAndInstallUpdateAsync(UpdateRequest updateRequest);
    string GetCurrentVersion();
    Task<UpdateInfo?> GetLatestVersionInfoAsync();
}
