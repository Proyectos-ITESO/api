// Routes/Modules/RegistrationRoutes.cs
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MicroJack.API.Models;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Routes.Modules
{
    public static class RegistrationRoutes
    {
        public static void Configure(WebApplication app)
        {
            var registrationsApiGroup = app.MapGroup("/api/registrations")
                                           .WithTags("Registrations");

            ConfigureGetRegistrations(registrationsApiGroup);
            ConfigureCreateRegistration(registrationsApiGroup);
            ConfigureGetRegistrationById(registrationsApiGroup);
        }

        private static void ConfigureGetRegistrations(RouteGroupBuilder group)
        {
            group.MapGet("/", async (
                IRegistrationService registrationService, 
                ILogger<Program> logger, 
                [FromQuery] string? search) =>
            {
                logger.LogInformation("Recibida solicitud GET /api/registrations (Búsqueda: {Search})", 
                    search ?? "N/A");
                try
                {
                    var registrations = await registrationService.GetRegistrationsAsync(search);
                    return Results.Ok(registrations);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error procesando GET /api/registrations");
                    return Results.Problem("Ocurrió un error al obtener los registros.", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetRegistrations")
            .Produces<List<Registration>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static void ConfigureCreateRegistration(RouteGroupBuilder group)
        {
            group.MapPost("/", async (
                IRegistrationService registrationService, 
                ILogger<Program> logger, 
                [FromBody] Registration registration) =>
            {
                logger.LogInformation("Recibida solicitud POST /api/registrations para visitante: {VisitorName}, Status: {Status}", 
                    registration.VisitorName, registration.Status ?? "N/A");

                // Validación básica
                if (!ValidateRegistration(registration, logger))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]> {
                        {"RequestBody", new[] { 
                            "Faltan campos requeridos (registrationType, house, visitReason, visitorName, visitedPerson, status)." 
                        }}
                    });
                }

                try
                {
                    var createdRegistration = await registrationService.CreateRegistrationAsync(registration);
                    return Results.CreatedAtRoute("GetRegistrationById", 
                        new { id = createdRegistration.Id }, createdRegistration);
                }
                catch (ApplicationException appEx)
                {
                    logger.LogError(appEx, "Error de aplicación al crear registro.");
                    return Results.Problem(appEx.Message, statusCode: StatusCodes.Status400BadRequest);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error inesperado procesando POST /api/registrations");
                    return Results.Problem("Ocurrió un error inesperado al crear el registro.", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("CreateRegistration")
            .Produces<Registration>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static void ConfigureGetRegistrationById(RouteGroupBuilder group)
        {
            group.MapGet("/{id}", async (
                IRegistrationService registrationService, 
                ILogger<Program> logger, 
                string id) =>
            {
                logger.LogInformation("Recibida solicitud GET /api/registrations/{Id}", id);
                if (!ObjectId.TryParse(id, out _))
                {
                    return Results.BadRequest("El ID proporcionado no es un ObjectId válido.");
                }
                
                try
                {
                    var registration = await registrationService.GetRegistrationByIdAsync(id);
                    if (registration is null)
                    {
                        logger.LogWarning("Registro no encontrado para ID: {Id}", id);
                        return Results.NotFound(new { message = $"Registro con ID {id} no encontrado." });
                    }
                    return Results.Ok(registration);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error procesando GET /api/registrations/{Id}", id);
                    return Results.Problem("Ocurrió un error al obtener el registro.", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetRegistrationById")
            .Produces<Registration>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static bool ValidateRegistration(Registration registration, ILogger<Program> logger)
        {
            if (string.IsNullOrWhiteSpace(registration.RegistrationType) ||
                string.IsNullOrWhiteSpace(registration.House) ||
                string.IsNullOrWhiteSpace(registration.VisitReason) ||
                string.IsNullOrWhiteSpace(registration.VisitorName) ||
                string.IsNullOrWhiteSpace(registration.VisitedPerson) ||
                string.IsNullOrWhiteSpace(registration.Status))
            {
                logger.LogWarning("Solicitud POST inválida: Faltan campos requeridos (incluyendo Status).");
                return false;
            }
            return true;
        }
    }
}