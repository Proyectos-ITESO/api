namespace MicroJack.API.Models;

public class LicenseCache
{
    public string LicenseKey { get; set; }
    public DateTime ExpirationDate { get; set; }
    public List<string> EnabledFeatures { get; set; }
    public DateTime NextVerificationDate { get; set; }
    public string Signature { get; set; }
    public string LatestVersion { get; set; }
    public string MinimumRequiredVersion { get; set; }
}