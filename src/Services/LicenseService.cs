using System.Security.Cryptography;
using System.Text;
using MicroJack.API.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;

namespace MicroJack.API.Services;

public class LicenseService : ILicenseService
{
    private readonly ILogger<LicenseService> _logger;
    private readonly LicenseSettings _licenseSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _cachePath;

    public LicenseService(ILogger<LicenseService> logger, IOptions<LicenseSettings> licenseSettings, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _licenseSettings = licenseSettings.Value;
        _httpClientFactory = httpClientFactory;
        _cachePath = Path.Combine(env.ContentRootPath, "license.cache");
    }

    public void ValidateLicense()
    {
        _logger.LogInformation("License validation process started.");
        try
        {
            ValidateOnlineAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Online license validation failed. Attempting offline validation.");
            ValidateOffline();
        }
    }

    private async Task ValidateOnlineAsync()
    {
        _logger.LogInformation("Attempting online license validation...");
        var machineId = GenerateMachineId();
        var client = _httpClientFactory.CreateClient();
        var validationUrl = $"{_licenseSettings.UpdateServerUrl}?licenseKey={_licenseSettings.LicenseKey}&machineId={machineId}";

        HttpResponseMessage response = await client.GetAsync(validationUrl);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var licenseCache = JsonConvert.DeserializeObject<LicenseCache>(jsonResponse);

        if (licenseCache == null || string.IsNullOrEmpty(licenseCache.Signature))
        {
            throw new Exception("Invalid response from license server.");
        }

        var dataToVerify = $"{licenseCache.LicenseKey}{licenseCache.ExpirationDate:o}{string.Join(",", licenseCache.EnabledFeatures)}{licenseCache.NextVerificationDate:o}{licenseCache.LatestVersion}{licenseCache.MinimumRequiredVersion}";
        if (!VerifySignature(dataToVerify, licenseCache.Signature, _licenseSettings.PublicKey))
        {
            throw new Exception("Invalid signature from license server.");
        }
        
        _logger.LogInformation("License signature is valid.");
        PerformVersionCheck(licenseCache);

        SaveLicenseCache(licenseCache);
        _logger.LogInformation("Online license validation successful. Cache updated.");
    }

    private void ValidateOffline()
    {
        _logger.LogInformation("Attempting offline license validation...");
        var cache = LoadLicenseCache();
        if (cache == null)
        {
            _logger.LogError("Offline validation failed: No license cache found.");
            throw new Exception("License validation failed. No cached license available.");
        }

        if (DateTime.UtcNow > cache.NextVerificationDate)
        {
            _logger.LogError("Offline validation failed: License cache has expired. Next verification was on {NextVerificationDate}", cache.NextVerificationDate);
            throw new Exception("License validation failed. Please connect to the internet to refresh your license.");
        }

        var dataToVerify = $"{cache.LicenseKey}{cache.ExpirationDate:o}{string.Join(",", cache.EnabledFeatures)}{cache.NextVerificationDate:o}{cache.LatestVersion}{cache.MinimumRequiredVersion}";
        if (!VerifySignature(dataToVerify, cache.Signature, _licenseSettings.PublicKey))
        {
            _logger.LogError("Offline validation failed: Invalid signature.");
            throw new Exception("License validation failed. The license signature is invalid.");
        }
        
        _logger.LogInformation("Offline license signature is valid.");
        PerformVersionCheck(cache);

        _logger.LogInformation("Offline license validation successful.");
    }

    private void PerformVersionCheck(LicenseCache cache)
    {
        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (assemblyVersion == null)
        {
            _logger.LogWarning("Could not determine application version. Skipping version check.");
            return;
        }

        var currentVersion = assemblyVersion;
        var minRequiredVersion = new Version(cache.MinimumRequiredVersion);
        var latestVersion = new Version(cache.LatestVersion);

        _logger.LogInformation("Version Check: Current='{currentVersion}', Minimum='{minRequiredVersion}', Latest='{latestVersion}'", currentVersion, minRequiredVersion, latestVersion);

        if (currentVersion < minRequiredVersion)
        {
            _logger.LogCritical("This application version ({currentVersion}) is obsolete. Version {minRequiredVersion} or higher is required. Shutting down.", currentVersion, minRequiredVersion);
            throw new Exception($"Application version {currentVersion} is too old. Please update to version {minRequiredVersion} or newer.");
        }

        if (currentVersion < latestVersion)
        {
            _logger.LogWarning("A newer version of the application is available ({latestVersion}). Please consider updating.", latestVersion);
        }
    }

    private LicenseCache LoadLicenseCache()
    {
        if (!File.Exists(_cachePath)) return null;
        var json = File.ReadAllText(_cachePath);
        return JsonConvert.DeserializeObject<LicenseCache>(json);
    }

    private void SaveLicenseCache(LicenseCache cache)
    {
        var json = JsonConvert.SerializeObject(cache, Formatting.Indented);
        File.WriteAllText(_cachePath, json);
    }

    private bool VerifySignature(string data, string signature, string publicKeyPem)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);

            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromBase64String(signature);

            return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signature verification.");
            return false;
        }
    }

    private string GenerateMachineId()
    {
        try
        {
            var machineInfo = $"{Environment.MachineName}-{Environment.OSVersion.VersionString}";
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineInfo));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not generate machine ID.");
            return "unknown-machine";
        }
    }
}