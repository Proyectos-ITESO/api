using Microsoft.AspNetCore.Mvc;
using MicroJack.API.Models;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Routes.Modules
{
    public static class IntermediateRegistrationRoutes
    {
        private static readonly Dictionary<int, CotoInfo> Cotos = new()
        {
            { 1, new CotoInfo("Coto Las Palmas", new Dictionary<string, string> 
                { {"1", "+523326688810"}, {"2", "+523317984651"}, {"3", "+523312345673"} }) },
            { 2, new CotoInfo("Coto Los Pinos", new Dictionary<string, string> 
                { {"1", "+523312345674"}, {"2", "+523312345675"}, {"3", "+523312345676"} }) },
            { 3, new CotoInfo("Coto El Bosque", new Dictionary<string, string> 
                { {"1", "+523312345677"}, {"2", "+523312345678"}, {"3", "+523312345679"} }) }
        };

        public static void Configure(WebApplication app)
        {
            var intermediateApiGroup = app.MapGroup("/api/intermediate")
                                         .WithTags("IntermediateRegistrations");

            ConfigureCreateIntermediateRegistration(intermediateApiGroup);
            ConfigureGetCotos(intermediateApiGroup);
            ConfigureGetPendingRegistrations(intermediateApiGroup);
            ConfigureApproveRegistration(intermediateApiGroup);
        }

        private static void ConfigureCreateIntermediateRegistration(RouteGroupBuilder group)
        {
            group.MapPost("/", async (
                IIntermediateRegistrationService intermediateService,
                IWhatsAppService whatsAppService,
                ILogger<Program> logger,
                [FromBody] CreateIntermediateRequest request) =>
            {
                logger.LogInformation("Creating intermediate registration for plates: {Plates}", request.Plates);

                if (string.IsNullOrWhiteSpace(request.Plates) || request.CotoId <= 0 || 
                    string.IsNullOrWhiteSpace(request.HouseNumber))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]> {
                        {"request", new[] { "Plates, CotoId and HouseNumber are required." }}
                    });
                }

                if (!Cotos.TryGetValue(request.CotoId, out var coto))
                {
                    return Results.BadRequest("Invalid CotoId");
                }

                if (!coto.Houses.TryGetValue(request.HouseNumber, out var housePhone))
                {
                    return Results.BadRequest("Invalid house number for the selected coto");
                }

                try
                {
                    var intermediate = new IntermediateRegistration
                    {
                        Plates = request.Plates,
                        VisitorName = request.VisitorName,
                        Brand = request.Brand,
                        Color = request.Color,
                        CotoId = request.CotoId.ToString(),
                        CotoName = coto.Name,
                        HouseNumber = request.HouseNumber,
                        HousePhone = housePhone,
                        ArrivalDateTime = request.ArrivalDateTime,
                        PersonVisited = request.PersonVisited
                    };

                    var created = await intermediateService.CreateIntermediateRegistrationAsync(intermediate);

                    // Send WhatsApp in background
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var sent = await whatsAppService.SendApprovalWhatsAppAsync(
                                housePhone, 
                                created.ApprovalToken!, 
                                created.VisitorName ?? "Visitante", 
                                created.Plates);
                            
                            if (sent)
                            {
                                logger.LogInformation("WhatsApp sent successfully for registration {Id}", created.Id);
                            }
                            else
                            {
                                logger.LogWarning("Failed to send WhatsApp for registration {Id}", created.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error sending WhatsApp for registration {Id}", created.Id);
                        }
                    });

                    return Results.Created($"/api/intermediate/{created.Id}", created);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error creating intermediate registration");
                    return Results.Problem("Error creating registration", statusCode: 500);
                }
            })
            .WithName("CreateIntermediateRegistration")
            .Produces<IntermediateRegistration>(201)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);
        }

        private static void ConfigureGetCotos(RouteGroupBuilder group)
        {
            group.MapGet("/cotos", (ILogger<Program> logger) =>
            {
                logger.LogInformation("Getting available cotos");
                
                var cotoList = Cotos.Select(c => new
                {
                    Id = c.Key,
                    Name = c.Value.Name,
                    Houses = c.Value.Houses.Keys.ToList()
                }).ToList();

                return Results.Ok(cotoList);
            })
            .WithName("GetCotos")
            .Produces(200);
        }

        private static void ConfigureGetPendingRegistrations(RouteGroupBuilder group)
        {
            group.MapGet("/pending", async (
                IIntermediateRegistrationService intermediateService,
                ILogger<Program> logger) =>
            {
                logger.LogInformation("Getting pending intermediate registrations");
                
                try
                {
                    var pending = await intermediateService.GetPendingIntermediateRegistrationsAsync();
                    return Results.Ok(pending);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error getting pending registrations");
                    return Results.Problem("Error getting pending registrations", statusCode: 500);
                }
            })
            .WithName("GetPendingIntermediateRegistrations")
            .Produces(200);
        }

        private static void ConfigureApproveRegistration(RouteGroupBuilder group)
        {
            group.MapPost("/approve/{token}", async (
                IIntermediateRegistrationService intermediateService,
                ILogger<Program> logger,
                string token) =>
            {
                logger.LogInformation("Approving registration with token: {Token}", token);

                if (string.IsNullOrWhiteSpace(token))
                {
                    return Results.BadRequest("Token is required");
                }

                try
                {
                    var approved = await intermediateService.ApproveIntermediateRegistrationAsync(token);
                    
                    if (approved)
                    {
                        return Results.Ok(new { message = "Registration approved successfully" });
                    }
                    else
                    {
                        return Results.NotFound(new { message = "Registration not found or already approved" });
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error approving registration with token: {Token}", token);
                    return Results.Problem("Error approving registration", statusCode: 500);
                }
            })
            .WithName("ApproveRegistration")
            .Produces(200)
            .ProducesProblem(404)
            .ProducesProblem(500);
        }
    }

    public record CreateIntermediateRequest(
        string Plates,
        string? VisitorName,
        string? Brand,
        string? Color,
        int CotoId,
        string HouseNumber,
        DateTime? ArrivalDateTime,
        string? PersonVisited
    );

    public record CotoInfo(string Name, Dictionary<string, string> Houses);
}