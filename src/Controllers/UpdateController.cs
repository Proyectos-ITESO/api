using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroJack.API.Models.Update;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminLevel")] // Solo administradores pueden manejar actualizaciones
public class UpdateController : ControllerBase
{
    private readonly IUpdateService _updateService;
    private readonly ILogger<UpdateController> _logger;

    public UpdateController(IUpdateService updateService, ILogger<UpdateController> logger)
    {
        _updateService = updateService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la versión actual de la API
    /// </summary>
    [HttpGet("version")]
    [AllowAnonymous] // Permitir consulta de versión sin autenticación
    public ActionResult<string> GetCurrentVersion()
    {
        var version = _updateService.GetCurrentVersion();
        return Ok(new { version = version });
    }

    /// <summary>
    /// Verifica si hay actualizaciones disponibles
    /// </summary>
    [HttpGet("check")]
    public async Task<ActionResult<UpdateCheckResponse>> CheckForUpdates()
    {
        _logger.LogInformation("Usuario {Username} verificando actualizaciones", User.Identity?.Name);

        var result = await _updateService.CheckForUpdatesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene información detallada de la última versión disponible
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<UpdateInfo>> GetLatestVersion()
    {
        var latestVersion = await _updateService.GetLatestVersionInfoAsync();

        if (latestVersion == null)
        {
            return NotFound(new { message = "No se pudo obtener información de la última versión" });
        }

        return Ok(latestVersion);
    }

    /// <summary>
    /// Inicia el proceso de actualización
    /// </summary>
    [HttpPost("install")]
    public async Task<IActionResult> InstallUpdate([FromBody] UpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Version) ||
            string.IsNullOrWhiteSpace(request.DownloadUrl) ||
            string.IsNullOrWhiteSpace(request.Checksum))
        {
            return BadRequest(new { message = "Datos de actualización incompletos" });
        }

        _logger.LogWarning("Usuario {Username} iniciando actualización a versión {Version}",
                         User.Identity?.Name, request.Version);

        try
        {
            var success = await _updateService.DownloadAndInstallUpdateAsync(request);

            if (success)
            {
                _logger.LogInformation("Actualización iniciada exitosamente. La aplicación se cerrará para completar la actualización.");

                return Ok(new {
                    message = "Actualización iniciada. La aplicación se reiniciará automáticamente.",
                    version = request.Version
                });
            }
            else
            {
                return StatusCode(500, new { message = "Error al iniciar el proceso de actualización" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la instalación de la actualización");
            return StatusCode(500, new { message = $"Error durante la actualización: {ex.Message}" });
        }
    }

    /// <summary>
    /// Endpoint para realizar actualización automática (verifica e instala si hay actualizaciones)
    /// </summary>
    [HttpPost("auto-update")]
    public async Task<IActionResult> AutoUpdate()
    {
        _logger.LogInformation("Usuario {Username} solicitando auto-actualización", User.Identity?.Name);

        try
        {
            // Verificar si hay actualizaciones disponibles
            var updateCheck = await _updateService.CheckForUpdatesAsync();

            if (!updateCheck.UpdateAvailable || updateCheck.LatestVersion == null)
            {
                return Ok(new {
                    message = "No hay actualizaciones disponibles",
                    currentVersion = updateCheck.CurrentVersion
                });
            }

            // Si hay actualizaciones, proceder con la instalación
            var updateRequest = new UpdateRequest
            {
                Version = updateCheck.LatestVersion.Version,
                DownloadUrl = updateCheck.LatestVersion.DownloadUrl,
                Checksum = updateCheck.LatestVersion.Checksum,
                ForceRestart = true
            };

            var success = await _updateService.DownloadAndInstallUpdateAsync(updateRequest);

            if (success)
            {
                return Ok(new {
                    message = $"Actualización a versión {updateRequest.Version} iniciada. La aplicación se reiniciará automáticamente.",
                    version = updateRequest.Version
                });
            }
            else
            {
                return StatusCode(500, new { message = "Error al iniciar la actualización automática" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la auto-actualización");
            return StatusCode(500, new { message = $"Error durante la auto-actualización: {ex.Message}" });
        }
    }
}
