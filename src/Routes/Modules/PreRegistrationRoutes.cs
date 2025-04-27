// Routes/Modules/PreRegistrationRoutes.cs
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MicroJack.API.Models;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Routes.Modules
{
    public static class PreRegistrationRoutes
    {
        public static void Configure(WebApplication app)
        {
            var preRegistrationsApiGroup = app.MapGroup("/api/preregistrations")
                                              .WithTags("PreRegistrations");

            ConfigureCreatePreRegistration(preRegistrationsApiGroup);
            ConfigureGetPreRegistrationByPlate(preRegistrationsApiGroup);
            ConfigureGetPreRegistrations(preRegistrationsApiGroup);
            ConfigureUpdatePreRegistrationStatus(preRegistrationsApiGroup);
        }

        private static void ConfigureCreatePreRegistration(RouteGroupBuilder group)
        {
            group.MapPost("/", async (
                IPreRegistrationService preRegistrationService, 
                ILogger<Program> logger, 
                [FromBody] PreRegistration preRegistration) =>
            {
                logger.LogInformation("Recibida solicitud POST /api/preregistrations para placas: {Plates}", 
                    preRegistration.Plates);
                
                if (string.IsNullOrWhiteSpace(preRegistration.Plates))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]> {
                        {"plates", new[] { "Las placas son requeridas para el pre-registro." }}
                    });
                }
                
                try
                {
                    var createdPreReg = await preRegistrationService.CreatePreRegistrationAsync(preRegistration);
                    return Results.Created($"/api/preregistrations/{createdPreReg.Id}", createdPreReg);
                }
                catch (ApplicationException appEx)
                {
                    logger.LogError(appEx, "Error de aplicación al crear pre-registro.");
                    return Results.Problem(appEx.Message, statusCode: StatusCodes.Status400BadRequest);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error inesperado procesando POST /api/preregistrations");
                    return Results.Problem("Error inesperado.", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("CreatePreRegistration")
            .Produces<PreRegistration>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static void ConfigureGetPreRegistrationByPlate(RouteGroupBuilder group)
        {
            group.MapGet("/by-plate/{plate}", async (
                IPreRegistrationService preRegistrationService, 
                ILogger<Program> logger, 
                string plate) =>
            {
                logger.LogInformation("Recibida solicitud GET /api/preregistrations/by-plate/{Plate}", plate);
                
                if (string.IsNullOrWhiteSpace(plate))
                {
                    return Results.BadRequest("El parámetro 'plate' es requerido.");
                }
                
                try
                {
                    var preRegistration = await preRegistrationService.GetPendingPreRegistrationByPlateAsync(plate);
                    if (preRegistration == null)
                    {
                        logger.LogWarning("No se encontró pre-registro PENDIENTE para placa: {Plate}", plate);
                        return Results.NotFound(new { 
                            message = $"No se encontró pre-registro pendiente para la placa {plate}." 
                        });
                    }
                    return Results.Ok(preRegistration);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error procesando GET /api/preregistrations/by-plate/{Plate}", plate);
                    return Results.Problem("Error al buscar.", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetPreRegistrationByPlate")
            .Produces<PreRegistration>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static void ConfigureGetPreRegistrations(RouteGroupBuilder group)
        {
            group.MapGet("/", async (
                IPreRegistrationService preRegistrationService, 
                ILogger<Program> logger, 
                [FromQuery] string? search) =>
            {
                logger.LogInformation("Recibida solicitud GET /api/preregistros (Búsqueda: {Search})", 
                    search ?? "N/A");
                try
                {
                    var preRegistrations = await preRegistrationService.GetPreRegistrationsAsync(search);
                    return Results.Ok(preRegistrations);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error procesando GET /api/preregistrations");
                    return Results.Problem("Error al obtener.", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetPreRegistrations")
            .Produces<List<PreRegistration>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static void ConfigureUpdatePreRegistrationStatus(RouteGroupBuilder group)
        {
            group.MapPatch("/{id}/status", async (
                IPreRegistrationService preRegistrationService, 
                ILogger<Program> logger, 
                string id, 
                [FromBody] UpdateStatusRequest request) =>
            {
                logger.LogInformation("Recibida solicitud PATCH /api/preregistrations/{Id}/status a '{NewStatus}'", 
                    id, request?.NewStatus);

                if (string.IsNullOrWhiteSpace(request?.NewStatus))
                {
                    return Results.BadRequest("El campo 'newStatus' es requerido.");
                }
                
                if (!ObjectId.TryParse(id, out _))
                {
                    return Results.BadRequest("El ID proporcionado no es un ObjectId válido.");
                }

                try
                {
                    bool success = await preRegistrationService.UpdatePreRegistrationStatusAsync(id, request.NewStatus);
                    if (success)
                    {
                        return Results.NoContent(); // 204 Éxito sin contenido
                    }
                    else
                    {
                        logger.LogWarning("No se pudo actualizar estado para pre-registro ID: {Id}. ¿No encontrado?", id);
                        return Results.NotFound(new { 
                            message = $"No se pudo encontrar o actualizar el pre-registro con ID {id}." 
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error procesando PATCH /api/preregistrations/{Id}/status", id);
                    return Results.Problem("Error al actualizar.", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("UpdatePreRegistrationStatus")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }

    // Record para el request de actualización de estado
    public record UpdateStatusRequest(string NewStatus);
}