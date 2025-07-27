using System.Security.Cryptography;
using System.Text;
using MicroJack.API.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

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
        _cachePath = Path.Combine(GetDataDirectory(), "license.cache");
    }
    
    private static string GetDataDirectory()
    {
        // Check for environment variable first (for containerized/packaged apps)
        var dataDir = Environment.GetEnvironmentVariable("MICROJACK_DATA_DIR");
        if (!string.IsNullOrEmpty(dataDir) && Directory.Exists(dataDir))
        {
            return dataDir;
        }
        
        // Use current working directory if writable, otherwise use user data directory
        var currentDir = Directory.GetCurrentDirectory();
        try
        {
            var testFile = Path.Combine(currentDir, ".write_test");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return currentDir;
        }
        catch
        {
            // Current directory is not writable, use user data directory
            var userDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDataDir = Path.Combine(userDataDir, "MicroJack");
            Directory.CreateDirectory(appDataDir);
            return appDataDir;
        }
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
        
        var baseUrl = _licenseSettings.UpdateServerUrl;
        var queryString = $"licenseKey={_licenseSettings.LicenseKey}&machineId={machineId}";
        
        var validationUrl = baseUrl.Contains("?") 
            ? $"{baseUrl}&{queryString}" 
            : $"{baseUrl}?{queryString}";

        HttpResponseMessage response = await client.GetAsync(validationUrl);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("License server response: {JsonResponse}", jsonResponse);
        
        // Parse server response first
        var serverResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
        if (serverResponse == null || string.IsNullOrEmpty((string)serverResponse.signature))
        {
            throw new Exception("Invalid response from license server.");
        }

        // Map server response to LicenseCache model
        var licenseCache = new LicenseCache
        {
            LicenseKey = _licenseSettings.LicenseKey,
            MachineId = machineId,
            ExpirationDate = DateTime.Parse((string)serverResponse.expirationDate),
            EnabledFeatures = ((Newtonsoft.Json.Linq.JArray)serverResponse.enabledFeatures).ToObject<List<string>>() ?? new(),
            NextVerificationDate = DateTime.Parse((string)serverResponse.nextVerificationDate, null, System.Globalization.DateTimeStyles.RoundtripKind), // Use exact format from server
            Signature = (string)serverResponse.signature,
            LatestVersion = (string)serverResponse.latestVersion,
            MinimumRequiredVersion = (string)serverResponse.minimumRequiredVersion,
            DownloadUrl = (string)serverResponse.downloadUrl,
            FileHash = (string)serverResponse.fileHash
        };
        
        _logger.LogInformation("Mapped license cache: LicenseKey={LicenseKey}, MachineId={MachineId}, Signature={Signature}", 
            licenseCache.LicenseKey, licenseCache.MachineId, licenseCache.Signature);

        // Use the exact strings from server JSON to preserve formatting without date parsing
        var nextVerificationMatch = System.Text.RegularExpressions.Regex.Match(jsonResponse, @"""nextVerificationDate"":""([^""]+)""");
        var expirationDateMatch = System.Text.RegularExpressions.Regex.Match(jsonResponse, @"""expirationDate"":""([^""]+)""");
        var nextVerificationStr = nextVerificationMatch.Success ? nextVerificationMatch.Groups[1].Value : "";
        var expirationDateStr = expirationDateMatch.Success ? expirationDateMatch.Groups[1].Value : "";
        var dataToVerify = $"{licenseCache.LicenseKey}{expirationDateStr}{string.Join(",", licenseCache.EnabledFeatures)}{nextVerificationStr}{licenseCache.LatestVersion}{licenseCache.MinimumRequiredVersion}{licenseCache.DownloadUrl}{licenseCache.FileHash}";
        _logger.LogInformation("Data to verify: {DataToVerify}", dataToVerify);
        _logger.LogInformation("Signature from server: {Signature}", licenseCache.Signature);
        
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

        // Use the same date format logic as online validation by serializing to JSON and extracting
        var tempCache = new { 
            expirationDate = cache.ExpirationDate, 
            nextVerificationDate = cache.NextVerificationDate 
        };
        var jsonString = System.Text.Json.JsonSerializer.Serialize(tempCache, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        var nextVerificationMatch = System.Text.RegularExpressions.Regex.Match(jsonString, @"""nextVerificationDate"":""([^""]+)""");
        var expirationDateMatch = System.Text.RegularExpressions.Regex.Match(jsonString, @"""expirationDate"":""([^""]+)""");
        var nextVerificationStr = nextVerificationMatch.Success ? nextVerificationMatch.Groups[1].Value : "";
        var expirationDateStr = expirationDateMatch.Success ? expirationDateMatch.Groups[1].Value : "";
        
        var dataToVerify = $"{cache.LicenseKey}{expirationDateStr}{string.Join(",", cache.EnabledFeatures)}{nextVerificationStr}{cache.LatestVersion}{cache.MinimumRequiredVersion}{cache.DownloadUrl}{cache.FileHash}";
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
            _logger.LogCritical("This application version ({currentVersion}) is obsolete. Version {minRequiredVersion} or higher is required. Initiating auto-update.", currentVersion, minRequiredVersion);
            InitiateUpdate(cache);
            // The application will exit after this, so we don't need to throw.
            return;
        }

        if (currentVersion < latestVersion)
        {
            _logger.LogWarning("A newer version of the application is available ({latestVersion}). Please consider updating.", latestVersion);
        }
    }

    private void InitiateUpdate(LicenseCache license)
    {
        _logger.LogInformation("Starting update process...");
        if (string.IsNullOrEmpty(license.DownloadUrl) || string.IsNullOrEmpty(license.FileHash))
        {
            _logger.LogCritical("Update cannot proceed: DownloadUrl or FileHash is missing from license data.");
            // Fallback to old behavior if update info is missing
            throw new Exception("Application version is obsolete, but update information is not available.");
        }

        try
        {
            var updaterPath = Path.Combine(AppContext.BaseDirectory, "MicroJack.Updater");
             if (!File.Exists(updaterPath) && OperatingSystem.IsWindows())
            {
                updaterPath += ".exe";
            }

            if (!File.Exists(updaterPath))
            {
                 _logger.LogCritical($"Updater executable not found at '{updaterPath}'. Cannot proceed with auto-update.");
                 throw new Exception("Update required, but the updater executable is missing.");
            }

            var installPath = AppContext.BaseDirectory;
            var parentPid = Environment.ProcessId;

            var processInfo = new ProcessStartInfo
            {
                FileName = updaterPath,
                Arguments = $"\"{license.DownloadUrl}\" \"{license.FileHash}\" \"{installPath}\" \"{parentPid}\"",
                UseShellExecute = true, // UseShellExecute true for launching as a separate window/process
                CreateNoWindow = false
            };

            Process.Start(processInfo);

            _logger.LogInformation("Updater launched. This application will now exit.");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to launch the updater process.");
            throw; // Re-throw if we can't even launch the updater
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