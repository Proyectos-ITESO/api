using Microsoft.AspNetCore.Authorization;
using MicroJack.API.Controllers;
using MicroJack.API.Models.Update;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Routes.Modules
{
    public static class UpdateRoutes
    {
        public static void MapUpdateRoutes(this WebApplication app)
        {
            var updateGroup = app.MapGroup("/api/update").WithTags("Update");

            // Get current version (no auth required)
            updateGroup.MapGet("/version", async (IUpdateService updateService) =>
            {
                var version = updateService.GetCurrentVersion();
                return Results.Ok(new { version = version });
            }).AllowAnonymous();

            // Check for updates (admin required)
            updateGroup.MapGet("/check", async (IUpdateService updateService) =>
            {
                var result = await updateService.CheckForUpdatesAsync();
                return Results.Ok(result);
            }).RequireAuthorization("AdminLevel");

            // Get latest version info (admin required)
            updateGroup.MapGet("/latest", async (IUpdateService updateService) =>
            {
                var latestVersion = await updateService.GetLatestVersionInfoAsync();

                if (latestVersion == null)
                {
                    return Results.NotFound(new { message = "No se pudo obtener información de la última versión" });
                }

                return Results.Ok(latestVersion);
            }).RequireAuthorization("AdminLevel");

            // Install update (admin required)
            updateGroup.MapPost("/install", async (UpdateRequest request, IUpdateService updateService, HttpContext context) =>
            {
                if (string.IsNullOrWhiteSpace(request.Version) ||
                    string.IsNullOrWhiteSpace(request.DownloadUrl) ||
                    string.IsNullOrWhiteSpace(request.Checksum))
                {
                    return Results.BadRequest(new { message = "Datos de actualización incompletos" });
                }

                try
                {
                    var success = await updateService.DownloadAndInstallUpdateAsync(request);

                    if (success)
                    {
                        return Results.Ok(new {
                            message = "Actualización iniciada. La aplicación se reiniciará automáticamente.",
                            version = request.Version
                        });
                    }
                    else
                    {
                        return Results.Problem("Error al iniciar el proceso de actualización", statusCode: 500);
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error durante la actualización: {ex.Message}", statusCode: 500);
                }
            }).RequireAuthorization("AdminLevel");

            // Auto-update (admin required)
            updateGroup.MapPost("/auto-update", async (IUpdateService updateService) =>
            {
                try
                {
                    // Verificar si hay actualizaciones disponibles
                    var updateCheck = await updateService.CheckForUpdatesAsync();

                    if (!updateCheck.UpdateAvailable || updateCheck.LatestVersion == null)
                    {
                        return Results.Ok(new {
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

                    var success = await updateService.DownloadAndInstallUpdateAsync(updateRequest);

                    if (success)
                    {
                        return Results.Ok(new {
                            message = $"Actualización a versión {updateRequest.Version} iniciada. La aplicación se reiniciará automáticamente.",
                            version = updateRequest.Version
                        });
                    }
                    else
                    {
                        return Results.Problem("Error al iniciar la actualización automática", statusCode: 500);
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error durante la auto-actualización: {ex.Message}", statusCode: 500);
                }
            }).RequireAuthorization("AdminLevel");
        }
    }
}
