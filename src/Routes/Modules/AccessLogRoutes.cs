using MicroJack.API.Services.Interfaces;
using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Routes.Modules
{
    public static class AccessLogRoutes
    {
        public static void MapAccessLogRoutes(this WebApplication app)
        {
            var accessLogGroup = app.MapGroup("/api/accesslogs").WithTags("Access Logs");

            // GET all access logs
            accessLogGroup.MapGet("/", async (IAccessLogService accessLogService) =>
            {
                try
                {
                    var accessLogs = await accessLogService.GetAllAccessLogsAsync();
                    return Results.Ok(new { success = true, data = accessLogs });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting access logs", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("GetAllAccessLogs")
            .Produces<object>(200);

            // GET access log by ID
            accessLogGroup.MapGet("/{id:int}", async (int id, IAccessLogService accessLogService) =>
            {
                try
                {
                    var accessLog = await accessLogService.GetAccessLogByIdAsync(id);
                    if (accessLog == null)
                        return Results.NotFound(new { success = false, message = "Access log not found" });

                    return Results.Ok(new { success = true, data = accessLog });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting access log", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAccessLogById")
            .Produces<object>(200)
            .Produces(404);

            // GET active access logs (no exit time)
            accessLogGroup.MapGet("/active", async (IAccessLogService accessLogService) =>
            {
                try
                {
                    var activeLogs = await accessLogService.GetActiveAccessLogsAsync();
                    return Results.Ok(new { success = true, data = activeLogs });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting active access logs", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetActiveAccessLogs")
            .Produces<object>(200);

            // POST create new access log (entry)
            accessLogGroup.MapPost("/", async (AccessLogCreateRequest request, IAccessLogService accessLogService) =>
            {
                try
                {
                    var accessLog = new AccessLog
                    {
                        VisitorId = request.VisitorId,
                        VehicleId = request.VehicleId,
                        AddressId = request.AddressId,
                        ResidentVisitedId = request.ResidentVisitedId,
                        EntryGuardId = request.EntryGuardId,
                        VisitReasonId = request.VisitReasonId,
                        GafeteNumber = request.GafeteNumber,
                        Comments = request.Comments,
                        Status = "Active"
                    };

                    var createdAccessLog = await accessLogService.CreateAccessLogAsync(accessLog);
                    return Results.Created($"/api/accesslogs/{createdAccessLog.Id}", new { success = true, data = createdAccessLog });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating access log", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("CreateAccessLog")
            .Produces<object>(201)
            .Produces(500);

            // PUT register exit
            accessLogGroup.MapPut("/{id:int}/exit", async (int id, ExitRequest request, IAccessLogService accessLogService) =>
            {
                try
                {
                    var result = await accessLogService.RegisterExitAsync(id, request.ExitGuardId);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Access log not found or already completed" });

                    return Results.Ok(new { success = true, message = "Exit registered successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error registering exit", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("RegisterExit")
            .Produces<object>(200)
            .Produces(404);

            // DELETE access log
            accessLogGroup.MapDelete("/{id:int}", async (int id, IAccessLogService accessLogService) =>
            {
                try
                {
                    var result = await accessLogService.DeleteAccessLogAsync(id);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Access log not found" });

                    return Results.Ok(new { success = true, message = "Access log deleted successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error deleting access log", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("DeleteAccessLog")
            .Produces<object>(200)
            .Produces(404);

            // === NUEVOS ENDPOINTS DE BÚSQUEDA AVANZADA ===

            // GET access logs by specific date
            accessLogGroup.MapGet("/by-date/{date:datetime}", async (DateTime date, IAccessLogService accessLogService) =>
            {
                try
                {
                    var accessLogs = await accessLogService.GetAccessLogsByDateAsync(date);
                    return Results.Ok(new { success = true, data = accessLogs });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting access logs by date", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAccessLogsByDate")
            .Produces<object>(200);

            // GET access logs by visitor name
            accessLogGroup.MapGet("/by-visitor/{visitorName}", async (string visitorName, IAccessLogService accessLogService) =>
            {
                try
                {
                    var accessLogs = await accessLogService.GetAccessLogsByVisitorNameAsync(visitorName);
                    return Results.Ok(new { success = true, data = accessLogs });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting access logs by visitor name", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAccessLogsByVisitorName")
            .Produces<object>(200);

            // GET access logs by license plate
            accessLogGroup.MapGet("/by-plate/{licensePlate}", async (string licensePlate, IAccessLogService accessLogService) =>
            {
                try
                {
                    var accessLogs = await accessLogService.GetAccessLogsByLicensePlateAsync(licensePlate);
                    return Results.Ok(new { success = true, data = accessLogs });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting access logs by license plate", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAccessLogsByLicensePlate")
            .Produces<object>(200);

            // GET access logs by vehicle characteristics
            accessLogGroup.MapGet("/by-vehicle", async (int? brandId, int? colorId, int? typeId, IAccessLogService accessLogService) =>
            {
                try
                {
                    var accessLogs = await accessLogService.GetAccessLogsByVehicleCharacteristicsAsync(brandId, colorId, typeId);
                    return Results.Ok(new { success = true, data = accessLogs });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting access logs by vehicle characteristics", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAccessLogsByVehicleCharacteristics")
            .Produces<object>(200);

            // GET access logs by address identifier
            accessLogGroup.MapGet("/by-address/{addressIdentifier}", async (string addressIdentifier, IAccessLogService accessLogService) =>
            {
                try
                {
                    var accessLogs = await accessLogService.GetAccessLogsByAddressIdentifierAsync(addressIdentifier);
                    return Results.Ok(new { success = true, data = accessLogs });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting access logs by address identifier", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAccessLogsByAddressIdentifier")
            .Produces<object>(200);

            // GET visitor history
            accessLogGroup.MapGet("/history/visitor/{visitorId:int}", async (int visitorId, IAccessLogService accessLogService) =>
            {
                try
                {
                    var history = await accessLogService.GetVisitorHistoryAsync(visitorId);
                    return Results.Ok(new { success = true, data = history });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting visitor history", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetVisitorHistory")
            .Produces<object>(200);

            // GET vehicle history
            accessLogGroup.MapGet("/history/vehicle/{licensePlate}", async (string licensePlate, IAccessLogService accessLogService) =>
            {
                try
                {
                    var history = await accessLogService.GetVehicleHistoryAsync(licensePlate);
                    return Results.Ok(new { success = true, data = history });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting vehicle history", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetVehicleHistory")
            .Produces<object>(200);

            // GET address history
            accessLogGroup.MapGet("/history/address/{addressId:int}", async (int addressId, IAccessLogService accessLogService) =>
            {
                try
                {
                    var history = await accessLogService.GetAddressHistoryAsync(addressId);
                    return Results.Ok(new { success = true, data = history });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting address history", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAddressHistory")
            .Produces<object>(200);

            // POST advanced search
            accessLogGroup.MapPost("/search", async (AccessLogSearchRequest request, IAccessLogService accessLogService) =>
            {
                try
                {
                    var results = await accessLogService.AdvancedSearchAsync(request);
                    return Results.Ok(new { success = true, data = results });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error in advanced search", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("AdvancedSearch")
            .Produces<object>(200);
        }
    }

    // DTOs
    public class AccessLogCreateRequest
    {
        public int VisitorId { get; set; }
        public int? VehicleId { get; set; }
        public int AddressId { get; set; }
        public int? ResidentVisitedId { get; set; }
        public int EntryGuardId { get; set; }
        public int? VisitReasonId { get; set; }
        public string? GafeteNumber { get; set; }
        public string? Comments { get; set; }
    }

    public class ExitRequest
    {
        public int ExitGuardId { get; set; }
        public string? Comments { get; set; }
    }

    }