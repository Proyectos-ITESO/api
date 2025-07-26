namespace LicensingServer.Models;

public class License
{
    public string LicenseKey { get; set; }
    public string? MachineId { get; set; }
    public DateTime ExpirationDate { get; set; }
    public List<string> EnabledFeatures { get; set; }
    public string LatestVersion { get; set; }
    public string MinimumRequiredVersion { get; set; }
}