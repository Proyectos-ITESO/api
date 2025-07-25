using Microsoft.AspNetCore.Mvc;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Routes.Modules
{
    public static class SimplePreRegistrationRoutes
    {
        public static void MapSimplePreRegistrationRoutes(this WebApplication app)
        {
            var group = app.MapGroup("/api/preregistro")
                          .WithTags("Simple Pre-Registration");

            // ENDPOINT PRINCIPAL: Buscar por placas
            group.MapGet("/buscar/{plates}", async (
                string plates,
                IPreRegistrationService preRegistrationService) =>
            {
                try
                {
                    var preReg = await preRegistrationService.GetPreRegistrationByIdentifierAsync(plates.ToUpper());
                    
                    if (preReg == null)
                    {
                        return Results.Ok(new
                        {
                            found = false,
                            message = $"No se encontró preregistro para placas: {plates} o fuera de ventana de tiempo (±2hrs)"
                        });
                    }

                    return Results.Ok(new
                    {
                        found = true,
                        data = preReg
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error buscando preregistro: {ex.Message}");
                }
            })
            .WithName("BuscarPreRegistro")
            .WithSummary("Busca un preregistro por placas (incluye validación de tiempo ±2hrs)")
            .AllowAnonymous(); // Sin autenticación para búsqueda rápida

            // Crear preregistro
            group.MapPost("/", async (
                [FromBody] CreatePreRegistroRequest request,
                IPreRegistrationService preRegistrationService) =>
            {
                try
                {
                    var preReg = new PreRegistration
                    {
                        Plates = request.Plates.ToUpper(),
                        VisitorName = request.VisitorName,
                        VehicleBrand = request.VehicleBrand,
                        VehicleColor = request.VehicleColor,
                        HouseVisited = request.HouseVisited,
                        ExpectedArrivalTime = request.ExpectedArrivalTime,
                        PersonVisited = request.PersonVisited,
                        Comments = request.Comments,
                        CreatedBy = request.CreatedBy
                    };

                    var created = await preRegistrationService.CreatePreRegistrationAsync(preReg);

                    return Results.Created($"/api/preregistro/{created.Id}", new
                    {
                        success = true,
                        message = "Preregistro creado exitosamente",
                        data = created
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { success = false, message = ex.Message });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error creando preregistro: {ex.Message}");
                }
            })
            .WithName("CrearPreRegistro")
            .WithSummary("Crear un nuevo preregistro")
            .RequireAuthorization();

            // Marcar entrada (PENDIENTE -> DENTRO)
            group.MapPatch("/entrada/{plates}", async (
                string plates,
                IPreRegistrationService preRegistrationService) =>
            {
                try
                {
                    var success = await preRegistrationService.MarkAsUsedAsync(plates.ToUpper());
                    
                    if (!success)
                    {
                        return Results.NotFound(new
                        {
                            success = false,
                            message = $"No se encontró preregistro pendiente para placas: {plates}"
                        });
                    }

                    return Results.Ok(new
                    {
                        success = true,
                        message = $"Preregistro marcado como DENTRO para placas: {plates}"
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error marcando entrada: {ex.Message}");
                }
            })
            .WithName("EntradaPreRegistro")
            .WithSummary("Marcar entrada de un preregistro (PENDIENTE -> DENTRO)")
            .RequireAuthorization();

            // Marcar salida (DENTRO -> FUERA)
            group.MapPatch("/salida/{plates}", async (
                string plates,
                IPreRegistrationService preRegistrationService) =>
            {
                try
                {
                    var success = await preRegistrationService.MarkAsExitAsync(plates.ToUpper());
                    
                    if (!success)
                    {
                        return Results.NotFound(new
                        {
                            success = false,
                            message = $"No se encontró preregistro DENTRO para placas: {plates}"
                        });
                    }

                    return Results.Ok(new
                    {
                        success = true,
                        message = $"Preregistro marcado como FUERA para placas: {plates}"
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error marcando salida: {ex.Message}");
                }
            })
            .WithName("SalidaPreRegistro")
            .WithSummary("Marcar salida de un preregistro (DENTRO -> FUERA)")
            .RequireAuthorization();

            // Listar todos los pendientes
            group.MapGet("/pendientes", async (
                IPreRegistrationService preRegistrationService) =>
            {
                try
                {
                    var pendientes = await preRegistrationService.GetActivePreRegistrationsAsync();
                    
                    return Results.Ok(new
                    {
                        success = true,
                        count = pendientes.Count,
                        data = pendientes
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error obteniendo preregistros pendientes: {ex.Message}");
                }
            })
            .WithName("PreRegistrosPendientes")
            .WithSummary("Obtener todos los preregistros pendientes")
            .RequireAuthorization();

            // Buscar por término
            group.MapGet("/buscar", async (
                [FromQuery] string q,
                IPreRegistrationService preRegistrationService) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(q))
                        return Results.BadRequest("Parámetro 'q' es requerido");

                    var results = await preRegistrationService.SearchPreRegistrationsAsync(q);
                    
                    return Results.Ok(new
                    {
                        success = true,
                        count = results.Count,
                        searchTerm = q,
                        data = results
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error buscando preregistros: {ex.Message}");
                }
            })
            .WithName("BuscarPreRegistros")
            .WithSummary("Buscar preregistros por término")
            .RequireAuthorization();
        }
    }

    public record CreatePreRegistroRequest(
        string Plates,
        string VisitorName,
        string? VehicleBrand,
        string? VehicleColor,
        string HouseVisited,
        DateTime ExpectedArrivalTime,
        string PersonVisited,
        string? Comments,
        string? CreatedBy
    );
}