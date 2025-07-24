using System.ComponentModel.DataAnnotations;
using MicroJack.API.Services.Interfaces;
using MicroJack.API.Models.Core;
using MicroJack.API.Models.Transaction;
using MicroJack.API.Models.Enums;

namespace MicroJack.API.Routes.Modules
{
    public static class UnifiedAccessRoutes
    {
        public static void MapUnifiedAccessRoutes(this WebApplication app)
        {
            var accessGroup = app.MapGroup("/api/access").WithTags("Unified Access Control");

            // POST unified entry registration
            accessGroup.MapPost("/register-entry", async (
                UnifiedEntryRequest request, 
                IVisitorService visitorService,
                IVehicleService vehicleService,
                IResidentService residentService,
                IAccessLogService accessLogService,
                HttpContext context) =>
            {
                try
                {
                    // Get guard ID from JWT token
                    var guardIdClaim = context.User.FindFirst("GuardId");
                    if (guardIdClaim == null || !int.TryParse(guardIdClaim.Value, out int guardId))
                    {
                        return Results.BadRequest(new { success = false, message = "Invalid authentication token" });
                    }

                    // 1. Handle Visitor (create or find existing by name)
                    Visitor visitor;
                    var existingVisitors = await visitorService.GetAllVisitorsAsync();
                    visitor = existingVisitors.FirstOrDefault(v => 
                        v.FullName.Equals(request.Visitor.FullName, StringComparison.OrdinalIgnoreCase));
                    
                    if (visitor == null)
                    {
                        // Create new visitor
                        visitor = new Visitor
                        {
                            FullName = request.Visitor.FullName,
                            IneImageUrl = request.Visitor.IneImageUrl,
                            FaceImageUrl = request.Visitor.FaceImageUrl
                        };
                        visitor = await visitorService.CreateVisitorAsync(visitor);
                    }
                    else
                    {
                        // Update existing visitor info if needed
                        visitor.FullName = request.Visitor.FullName;
                        if (!string.IsNullOrEmpty(request.Visitor.IneImageUrl))
                            visitor.IneImageUrl = request.Visitor.IneImageUrl;
                        if (!string.IsNullOrEmpty(request.Visitor.FaceImageUrl))
                            visitor.FaceImageUrl = request.Visitor.FaceImageUrl;
                        
                        visitor = await visitorService.UpdateVisitorAsync(visitor.Id, visitor);
                    }

                    // 2. Handle Vehicle (create or find existing)
                    Vehicle? vehicle = null;
                    if (!string.IsNullOrEmpty(request.Vehicle?.LicensePlate))
                    {
                        // Try to find existing vehicle by license plate
                        vehicle = await vehicleService.GetVehicleByLicensePlateAsync(
                            request.Vehicle.LicensePlate.ToUpper());
                        
                        if (vehicle == null)
                        {
                            // Create new vehicle
                            vehicle = new Vehicle
                            {
                                LicensePlate = request.Vehicle.LicensePlate.ToUpper(),
                                PlateImageUrl = request.Vehicle.PlateImageUrl,
                                BrandId = request.Vehicle.BrandId,
                                ColorId = request.Vehicle.ColorId,
                                TypeId = request.Vehicle.TypeId
                            };
                            vehicle = await vehicleService.CreateVehicleAsync(vehicle);
                        }
                        else
                        {
                            // Update vehicle info if needed
                            if (!string.IsNullOrEmpty(request.Vehicle.PlateImageUrl))
                                vehicle.PlateImageUrl = request.Vehicle.PlateImageUrl;
                            if (request.Vehicle.BrandId.HasValue)
                                vehicle.BrandId = request.Vehicle.BrandId;
                            if (request.Vehicle.ColorId.HasValue)
                                vehicle.ColorId = request.Vehicle.ColorId;
                            if (request.Vehicle.TypeId.HasValue)
                                vehicle.TypeId = request.Vehicle.TypeId;
                            
                            vehicle = await vehicleService.UpdateVehicleAsync(vehicle.Id, vehicle);
                        }
                    }

                    // 3. Validate resident if specified
                    Resident? resident = null;
                    if (request.ResidentId.HasValue)
                    {
                        resident = await residentService.GetResidentByIdAsync(request.ResidentId.Value);
                        if (resident == null)
                        {
                            return Results.BadRequest(new { 
                                success = false, 
                                message = $"Resident with ID {request.ResidentId} not found" 
                            });
                        }
                    }

                    // 4. Create access log entry
                    var accessLog = new AccessLog
                    {
                        VisitorId = visitor.Id,
                        VehicleId = vehicle?.Id,
                        AddressId = resident?.AddressId ?? 1, // Default address if no resident
                        ResidentVisitedId = resident?.Id,
                        EntryTimestamp = request.EntryTime ?? DateTime.UtcNow,
                        Comments = request.Notes,
                        EntryGuardId = guardId,
                        Status = "DENTRO"
                    };

                    var createdAccessLog = await accessLogService.CreateAccessLogAsync(accessLog);

                    // 5. Return comprehensive response
                    return Results.Created($"/api/accesslogs/{createdAccessLog.Id}", new
                    {
                        success = true,
                        message = "Entry registered successfully",
                        data = new
                        {
                            accessLog = createdAccessLog,
                            visitor = visitor,
                            vehicle = vehicle,
                            resident = resident,
                            entryTime = createdAccessLog.EntryTimestamp,
                            guardId = guardId
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Error registering entry",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("RegisterUnifiedEntry")
            .WithSummary("Register a complete entry with visitor, vehicle, and access log in one operation")
            .Produces<object>(201)
            .Produces(400)
            .Produces(500);

            // POST unified exit registration
            accessGroup.MapPost("/register-exit/{accessLogId}", async (
                int accessLogId,
                UnifiedExitRequest request,
                IAccessLogService accessLogService,
                HttpContext context) =>
            {
                try
                {
                    // Get guard ID from JWT token
                    var guardIdClaim = context.User.FindFirst("GuardId");
                    if (guardIdClaim == null || !int.TryParse(guardIdClaim.Value, out int guardId))
                    {
                        return Results.BadRequest(new { success = false, message = "Invalid authentication token" });
                    }

                    var accessLog = await accessLogService.GetAccessLogByIdAsync(accessLogId);
                    if (accessLog == null)
                    {
                        return Results.NotFound(new { 
                            success = false, 
                            message = "Access log entry not found" 
                        });
                    }

                    if (accessLog.ExitTimestamp.HasValue)
                    {
                        return Results.BadRequest(new { 
                            success = false, 
                            message = "Visitor has already exited" 
                        });
                    }

                    // Update access log with exit information
                    accessLog.ExitTimestamp = request.ExitTime ?? DateTime.UtcNow;
                    accessLog.Comments = string.IsNullOrEmpty(accessLog.Comments) ? 
                        request.ExitNotes : $"{accessLog.Comments}. Exit: {request.ExitNotes}";
                    accessLog.Status = "FUERA";
                    accessLog.ExitGuardId = guardId;

                    var updatedAccessLog = await accessLogService.UpdateAccessLogAsync(accessLogId, accessLog);

                    return Results.Ok(new
                    {
                        success = true,
                        message = "Exit registered successfully",
                        data = updatedAccessLog
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Error registering exit",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("RegisterUnifiedExit")
            .WithSummary("Register visitor exit by access log ID")
            .Produces<object>(200)
            .Produces(400)
            .Produces(404)
            .Produces(500);

            // GET active visits summary
            accessGroup.MapGet("/active-visits", async (IAccessLogService accessLogService) =>
            {
                try
                {
                    var allLogs = await accessLogService.GetAllAccessLogsAsync();
                    var activeVisits = allLogs.Where(log => log.Status == "DENTRO");
                    
                    return Results.Ok(new
                    {
                        success = true,
                        count = activeVisits.Count(),
                        data = activeVisits.Select(log => new
                        {
                            id = log.Id,
                            visitorName = log.Visitor?.FullName,
                            vehiclePlate = log.Vehicle?.LicensePlate,
                            entryTime = log.EntryTimestamp,
                            comments = log.Comments,
                            minutesInside = (int)(DateTime.UtcNow - log.EntryTimestamp).TotalMinutes
                        })
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Error getting active visits",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("GetActiveVisits")
            .WithSummary("Get summary of all currently active visits")
            .Produces<object>(200)
            .Produces(500);
        }
    }

    // Request DTOs
    public class UnifiedEntryRequest
    {
        [Required]
        public VisitorRequest Visitor { get; set; } = new VisitorRequest();
        
        public VehicleRequest? Vehicle { get; set; }
        
        public int? ResidentId { get; set; }
        
        [Required]
        public string Purpose { get; set; } = string.Empty;
        
        public DateTime? EntryTime { get; set; }
        
        public string? Notes { get; set; }
    }

    public class VisitorRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        public string? IneImageUrl { get; set; }
        
        public string? FaceImageUrl { get; set; }
    }

    public class VehicleRequest
    {
        [Required]
        public string LicensePlate { get; set; } = string.Empty;
        
        public string? PlateImageUrl { get; set; }
        
        public int? BrandId { get; set; }
        
        public int? ColorId { get; set; }
        
        public int? TypeId { get; set; }
    }

    public class UnifiedExitRequest
    {
        public DateTime? ExitTime { get; set; }
        
        public string? ExitNotes { get; set; }
    }
}