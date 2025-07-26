using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LicensingServer.Models;

namespace LicensingServer.Services;

public class LicenseValidationService
{
    private readonly ILogger<LicenseValidationService> _logger;
    private readonly string _privateKey;
    private readonly string _dbPath = "licenses.json";

    public LicenseValidationService(IConfiguration configuration, ILogger<LicenseValidationService> logger)
    {
        _privateKey = configuration["LicenseSettings:PrivateKey"]!;
        _logger = logger;
    }

    public LicenseResponse? ValidateAndSign(string licenseKey, string machineId)
    {
        _logger.LogInformation("Validating license {LicenseKey} for machine {MachineId}", licenseKey, machineId);

        var licenses = JsonSerializer.Deserialize<List<License>>(File.ReadAllText(_dbPath));
        var license = licenses?.FirstOrDefault(l => l.LicenseKey == licenseKey);

        if (license == null)
        {
            _logger.LogWarning("License key {LicenseKey} not found.", licenseKey);
            return null;
        }

        if (license.MachineId != null && license.MachineId != machineId)
        {
            _logger.LogWarning("Machine ID mismatch for license {LicenseKey}. Expected {Expected}, got {Actual}", licenseKey, license.MachineId, machineId);
            return null;
        }

        var response = new LicenseResponse
        {
            LicenseKey = license.LicenseKey,
            ExpirationDate = license.ExpirationDate,
            EnabledFeatures = license.EnabledFeatures,
            NextVerificationDate = DateTime.UtcNow.AddDays(7),
            LatestVersion = license.LatestVersion,
            MinimumRequiredVersion = license.MinimumRequiredVersion
        };

        var dataToSign = $"{response.LicenseKey}{response.ExpirationDate:o}{string.Join(",", response.EnabledFeatures)}{response.NextVerificationDate:o}{response.LatestVersion}{response.MinimumRequiredVersion}";
        response.Signature = SignData(dataToSign);

        return response;
    }

    private string SignData(string data)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(_privateKey);

        var dataBytes = Encoding.UTF8.GetBytes(data);
        var signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signatureBytes);
    }
}