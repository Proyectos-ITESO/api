namespace MicroJack.API.Models;

public class LicenseCache
{
    public string LicenseKey { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public string ExpirationDateString { get; set; } = string.Empty;
    public List<string> EnabledFeatures { get; set; } = new();
    public DateTime NextVerificationDate { get; set; }
    public string NextVerificationDateString { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public string MinimumRequiredVersion { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
}