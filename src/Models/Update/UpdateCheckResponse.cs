namespace MicroJack.API.Models.Update;

public class UpdateCheckResponse
{
    public bool UpdateAvailable { get; set; }
    public string CurrentVersion { get; set; } = string.Empty;
    public UpdateInfo? LatestVersion { get; set; }
    public string Message { get; set; } = string.Empty;
}
