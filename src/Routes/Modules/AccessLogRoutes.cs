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