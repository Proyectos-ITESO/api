using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using MicroJack.API.Models.Update;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services;

public class UpdateService : IUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UpdateService> _logger;
    private readonly string _updateServerUrl;
    private readonly string _installPath;
    private readonly string _updaterPath;

    public UpdateService(HttpClient httpClient, IConfiguration configuration, ILogger<UpdateService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _updateServerUrl = _configuration["UpdateSettings:ServerUrl"] ?? "https://updates.example.com/api";
        _installPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
        _updaterPath = Path.Combine(_installPath, "MicroJack.Updater");

        // Verificar que el updater existe
        if (!File.Exists(_updaterPath))
        {
            _logger.LogWarning("MicroJack.Updater no encontrado en: {UpdaterPath}", _updaterPath);
        }
    }

    public string GetCurrentVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "1.0.0.0";
    }

    public async Task<UpdateCheckResponse> CheckForUpdatesAsync()
    {
        try
        {
            var currentVersion = GetCurrentVersion();
            _logger.LogInformation("Verificando actualizaciones. Versión actual: {CurrentVersion}", currentVersion);

            var latestVersionInfo = await GetLatestVersionInfoAsync();

            if (latestVersionInfo == null)
            {
                return new UpdateCheckResponse
                {
                    UpdateAvailable = false,
                    CurrentVersion = currentVersion,
                    Message = "No se pudo verificar la disponibilidad de actualizaciones"
                };
            }

            var updateAvailable = IsNewerVersion(currentVersion, latestVersionInfo.Version);

            return new UpdateCheckResponse
            {
                UpdateAvailable = updateAvailable,
                CurrentVersion = currentVersion,
                LatestVersion = updateAvailable ? latestVersionInfo : null,
                Message = updateAvailable
                    ? $"Actualización disponible: v{latestVersionInfo.Version}"
                    : "Su aplicación está actualizada"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar actualizaciones");
            return new UpdateCheckResponse
            {
                UpdateAvailable = false,
                CurrentVersion = GetCurrentVersion(),
                Message = $"Error al verificar actualizaciones: {ex.Message}"
            };
        }
    }

    public async Task<UpdateInfo?> GetLatestVersionInfoAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_updateServerUrl}/backend/latest");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("No se pudo obtener información de la última versión. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return updateInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información de la última versión");
            return null;
        }
    }

    public async Task<bool> DownloadAndInstallUpdateAsync(UpdateRequest updateRequest)
    {
        try
        {
            if (!File.Exists(_updaterPath))
            {
                _logger.LogError("MicroJack.Updater no encontrado en: {UpdaterPath}", _updaterPath);
                return false;
            }

            _logger.LogInformation("Iniciando proceso de actualización a versión {Version}", updateRequest.Version);

            // Obtener el PID del proceso actual
            var currentPid = Process.GetCurrentProcess().Id;

            // Preparar argumentos para el updater
            var arguments = $"\"{updateRequest.DownloadUrl}\" \"{updateRequest.Checksum}\" \"{_installPath}\" {currentPid}";

            _logger.LogInformation("Ejecutando updater con argumentos: {Arguments}", arguments);

            // Crear proceso del updater
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _updaterPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Hacer el ejecutable runnable en Linux
            if (OperatingSystem.IsLinux())
            {
                try
                {
                    var chmodProcess = Process.Start("chmod", $"+x \"{_updaterPath}\"");
                    await chmodProcess!.WaitForExitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo hacer ejecutable el updater");
                }
            }

            // Iniciar el updater
            var updaterProcess = Process.Start(processStartInfo);

            if (updaterProcess == null)
            {
                _logger.LogError("No se pudo iniciar el proceso de actualización");
                return false;
            }

            _logger.LogInformation("Proceso de actualización iniciado con PID: {UpdaterPid}", updaterProcess.Id);

            // Programar el cierre de la aplicación después de un breve delay
            // para dar tiempo al updater de detectar que debe esperar
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000); // Esperar 2 segundos
                _logger.LogInformation("Cerrando aplicación para permitir la actualización...");
                Environment.Exit(0);
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el proceso de actualización");
            return false;
        }
    }

    private bool IsNewerVersion(string currentVersion, string latestVersion)
    {
        try
        {
            var current = Version.Parse(currentVersion);
            var latest = Version.Parse(latestVersion);
            return latest > current;
        }
        catch
        {
            // En caso de error en el parsing, usar comparación de strings
            return string.CompareOrdinal(latestVersion, currentVersion) > 0;
        }
    }
}
