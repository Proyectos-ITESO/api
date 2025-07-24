using System.ComponentModel.DataAnnotations;
using MicroJack.API.Services.Interfaces;
using MicroJack.API.Models.Core;
using MicroJack.API.Models.Transaction;
using MicroJack.API.Models.Enums;

namespace MicroJack.API.Routes.Modules
{
    public static class EventLogRoutes
    {
        public static void MapEventLogRoutes(this WebApplication app)
        {
            var eventLogGroup = app.MapGroup("/api/eventlogs").WithTags("Event Logs (Bitácora)");

            // GET all event logs
            eventLogGroup.MapGet("/", async (IEventLogService eventLogService, 
                DateTime? fromDate, DateTime? toDate, string? eventType, int? guardId) =>
            {
                try
                {
                    var eventLogs = await eventLogService.GetAllEventLogsAsync();

                    // Apply filters if provided
                    var filteredLogs = eventLogs.AsEnumerable();
                    
                    if (fromDate.HasValue)
                        filteredLogs = filteredLogs.Where(e => e.Timestamp >= fromDate.Value);
                    
                    if (toDate.HasValue)
                        filteredLogs = filteredLogs.Where(e => e.Timestamp <= toDate.Value);
                    
                    if (!string.IsNullOrEmpty(eventType))
                        filteredLogs = filteredLogs.Where(e => e.Description.Contains(eventType, StringComparison.OrdinalIgnoreCase));
                    
                    if (guardId.HasValue)
                        filteredLogs = filteredLogs.Where(e => e.GuardId == guardId.Value);

                    var orderedLogs = filteredLogs.OrderByDescending(e => e.Timestamp);

                    return Results.Ok(new { 
                        success = true, 
                        count = orderedLogs.Count(),
                        data = orderedLogs
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting event logs", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("GetAllEventLogs")
            .WithSummary("Get all event logs with optional filters")
            .Produces<object>(200);

            // GET event log by ID
            eventLogGroup.MapGet("/{id:int}", async (int id, IEventLogService eventLogService) =>
            {
                try
                {
                    var eventLog = await eventLogService.GetEventLogByIdAsync(id);
                    if (eventLog == null)
                        return Results.NotFound(new { success = false, message = "Event log not found" });

                    return Results.Ok(new { success = true, data = eventLog });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting event log", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("GetEventLogById")
            .WithSummary("Get specific event log by ID")
            .Produces<object>(200)
            .Produces(404);

            // POST create new event log
            eventLogGroup.MapPost("/", async (EventLogCreateRequest request, 
                IEventLogService eventLogService, HttpContext context) =>
            {
                try
                {
                    // Get guard ID from JWT token
                    var guardIdClaim = context.User.FindFirst("GuardId");
                    if (guardIdClaim == null || !int.TryParse(guardIdClaim.Value, out int guardId))
                    {
                        return Results.BadRequest(new { success = false, message = "Invalid authentication token" });
                    }

                    var eventLog = new EventLog
                    {
                        Description = $"[{request.EventType}] {request.Description} (Severity: {request.Severity})",
                        GuardId = guardId,
                        Timestamp = request.Timestamp ?? DateTime.UtcNow
                    };

                    var createdEventLog = await eventLogService.CreateEventLogAsync(eventLog);
                    return Results.Created($"/api/eventlogs/{createdEventLog.Id}", 
                        new { success = true, data = createdEventLog });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating event log", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("CreateEventLog")
            .WithSummary("Create new event log entry")
            .Produces<object>(201)
            .Produces(400)
            .Produces(500);

            // GET recent events (last 24 hours)
            eventLogGroup.MapGet("/recent", async (IEventLogService eventLogService, int? hours) =>
            {
                try
                {
                    var hoursBack = hours ?? 24;
                    var cutoffTime = DateTime.UtcNow.AddHours(-hoursBack);
                    
                    var eventLogs = await eventLogService.GetAllEventLogsAsync();
                    var recentLogs = eventLogs
                        .Where(e => e.Timestamp >= cutoffTime)
                        .OrderByDescending(e => e.Timestamp)
                        .Take(50); // Limit to 50 most recent

                    return Results.Ok(new { 
                        success = true, 
                        hoursBack = hoursBack,
                        count = recentLogs.Count(),
                        data = recentLogs 
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting recent events", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("GetRecentEventLogs")
            .WithSummary("Get recent event logs (last 24 hours by default)")
            .Produces<object>(200);

            // GET events by type
            eventLogGroup.MapGet("/type/{eventType}", async (string eventType, IEventLogService eventLogService) =>
            {
                try
                {
                    var eventLogs = await eventLogService.GetAllEventLogsAsync();
                    var filteredLogs = eventLogs
                        .Where(e => e.Description.Contains(eventType, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(e => e.Timestamp);

                    return Results.Ok(new { 
                        success = true, 
                        eventType = eventType,
                        count = filteredLogs.Count(),
                        data = filteredLogs 
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting events by type", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("GetEventLogsByType")
            .WithSummary("Get event logs filtered by event type")
            .Produces<object>(200);

            // GET events by severity
            eventLogGroup.MapGet("/severity/{severity}", async (string severity, IEventLogService eventLogService) =>
            {
                try
                {
                    var eventLogs = await eventLogService.GetAllEventLogsAsync();
                    var filteredLogs = eventLogs
                        .Where(e => e.Description.Contains(severity, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(e => e.Timestamp);

                    return Results.Ok(new { 
                        success = true, 
                        severity = severity,
                        count = filteredLogs.Count(),
                        data = filteredLogs 
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting events by severity", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("GetEventLogsBySeverity")
            .WithSummary("Get event logs filtered by severity level")
            .Produces<object>(200);

            // POST quick event (predefined event types)
            eventLogGroup.MapPost("/quick", async (QuickEventRequest request, 
                IEventLogService eventLogService, HttpContext context) =>
            {
                try
                {
                    // Get guard ID from JWT token
                    var guardIdClaim = context.User.FindFirst("GuardId");
                    if (guardIdClaim == null || !int.TryParse(guardIdClaim.Value, out int guardId))
                    {
                        return Results.BadRequest(new { success = false, message = "Invalid authentication token" });
                    }

                    // Predefined quick events
                    var quickEvents = new Dictionary<string, (string eventType, string description, string severity)>
                    {
                        { "shift_start", ("Shift Change", "Guard shift started", "Info") },
                        { "shift_end", ("Shift Change", "Guard shift ended", "Info") },
                        { "security_check", ("Security", "Routine security check performed", "Info") },
                        { "maintenance", ("Maintenance", "Maintenance activity recorded", "Info") },
                        { "incident", ("Security", "Security incident reported", "Warning") },
                        { "emergency", ("Emergency", "Emergency situation", "Critical") },
                        { "visitor_issue", ("Visitor", "Issue with visitor access", "Warning") },
                        { "equipment_check", ("Equipment", "Equipment status check", "Info") },
                        { "gate_malfunction", ("Equipment", "Gate malfunction reported", "Error") },
                        { "unauthorized_access", ("Security", "Unauthorized access attempt", "Critical") }
                    };

                    if (!quickEvents.ContainsKey(request.QuickEventType.ToLowerInvariant()))
                    {
                        return Results.BadRequest(new { 
                            success = false, 
                            message = "Invalid quick event type",
                            availableTypes = quickEvents.Keys
                        });
                    }

                    var (eventType, description, severity) = quickEvents[request.QuickEventType.ToLowerInvariant()];

                    var eventLog = new EventLog
                    {
                        Description = string.IsNullOrEmpty(request.Notes) ? 
                            $"[{eventType}] {description} (Severity: {severity})" : 
                            $"[{eventType}] {description} (Severity: {severity}). Notes: {request.Notes}",
                        GuardId = guardId,
                        Timestamp = DateTime.UtcNow
                    };

                    var createdEventLog = await eventLogService.CreateEventLogAsync(eventLog);
                    return Results.Created($"/api/eventlogs/{createdEventLog.Id}", 
                        new { success = true, data = createdEventLog });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating quick event", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("CreateQuickEvent")
            .WithSummary("Create event log using predefined quick event types")
            .Produces<object>(201)
            .Produces(400)
            .Produces(500);

            // DELETE event log (admin only)
            eventLogGroup.MapDelete("/{id:int}", async (int id, IEventLogService eventLogService) =>
            {
                try
                {
                    var result = await eventLogService.DeleteEventLogAsync(id);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Event log not found" });

                    return Results.Ok(new { success = true, message = "Event log deleted successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error deleting event log", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("DeleteEventLog")
            .WithSummary("Delete event log entry (admin only)")
            .Produces<object>(200)
            .Produces(404);
        }
    }

    // Request DTOs
    public class EventLogCreateRequest
    {
        [Required]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Severity { get; set; } = "Info"; // Info, Warning, Error, Critical

        public string? RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }

        public string? AdditionalData { get; set; }

        public DateTime? Timestamp { get; set; }
    }

    public class QuickEventRequest
    {
        [Required]
        public string QuickEventType { get; set; } = string.Empty; // shift_start, incident, etc.

        public string? Notes { get; set; }

        public string? AdditionalData { get; set; }
    }
}