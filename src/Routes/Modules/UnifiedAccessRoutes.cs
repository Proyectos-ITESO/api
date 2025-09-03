using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
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

                    // 3. Resolve Address and Resident
                    Resident? resident = null;
                    int addressId;

                    // Resident is optional, but if provided, we validate it.
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

                    // Address is resolved via the 'House' string or AddressId. At least one is required.
                    if (string.IsNullOrWhiteSpace(request.House) && !request.AddressId.HasValue)
                    {
                        return Results.BadRequest(new { success = false, message = "House identifier or AddressId is required." });
                    }

                    var addressService = context.RequestServices.GetRequiredService<IAddressService>();

                    // If AddressId is provided, use it directly
                    if (request.AddressId.HasValue)
                    {
                        var addressById = await addressService.GetAddressByIdAsync(request.AddressId.Value);
                        if (addressById == null)
                        {
                            return Results.BadRequest(new { 
                                success = false, 
                                message = $"Address with ID {request.AddressId} not found" 
                            });
                        }
                        addressId = addressById.Id;
                    }
                    else
                    {
                        // Use House string to find or create address
                        var addresses = await addressService.GetAllAddressesAsync();
                        var targetAddress = addresses.FirstOrDefault(a => a.Identifier.Equals(request.House, StringComparison.OrdinalIgnoreCase));

                        if (targetAddress != null)
                        {
                            addressId = targetAddress.Id;
                        }
                        else
                        {
                            // Create a new address since it doesn't exist
                            var newAddress = new MicroJack.API.Models.Core.Address
                            {
                                Identifier = request.House,
                                Extension = "000", // Default value
                                Status = "Active"
                            };
                            var createdAddress = await addressService.CreateAddressAsync(newAddress);
                            addressId = createdAddress.Id;
                        }
                    }

                    // 4. Create access log entry
                    var accessLog = new AccessLog
                    {
                        VisitorId = visitor.Id,
                        VehicleId = vehicle?.Id,
                        AddressId = addressId, // Use the resolved address ID
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

            // POST unified entry registration with multiple image upload
            accessGroup.MapPost("/register-entry-with-images", async (
                HttpRequest request,
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

                    // Parse form data
                    var form = await request.ReadFormAsync();
                    
                    // Extract basic visitor information
                    var visitorFullName = form["visitorFullName"].ToString();
                    if (string.IsNullOrEmpty(visitorFullName))
                    {
                        return Results.BadRequest(new { success = false, message = "Visitor full name is required" });
                    }

                    // Handle vehicle information
                    string? licensePlate = form["licensePlate"].ToString();
                    Vehicle? vehicle = null;
                    
                    if (!string.IsNullOrEmpty(licensePlate))
                    {
                        // Try to find existing vehicle by license plate
                        vehicle = await vehicleService.GetVehicleByLicensePlateAsync(licensePlate.ToUpper());
                        
                        if (vehicle == null)
                        {
                            // Create new vehicle
                            vehicle = new Vehicle
                            {
                                LicensePlate = licensePlate.ToUpper(),
                                PlateImageUrl = null, // Will be set after image upload
                                BrandId = null,
                                ColorId = null,
                                TypeId = null
                            };
                        }
                    }

                    // Handle address and resident
                    Resident? resident = null;
                    int addressId;

                    if (int.TryParse(form["residentId"], out int residentIdValue))
                    {
                        resident = await residentService.GetResidentByIdAsync(residentIdValue);
                        if (resident == null)
                        {
                            return Results.BadRequest(new { 
                                success = false, 
                                message = $"Resident with ID {residentIdValue} not found" 
                            });
                        }
                    }

                    var house = form["house"].ToString();
                    if (string.IsNullOrWhiteSpace(house) && !int.TryParse(form["addressId"], out int addressIdValue))
                    {
                        return Results.BadRequest(new { success = false, message = "House identifier or AddressId is required." });
                    }

                    var addressService = context.RequestServices.GetRequiredService<IAddressService>();

                    if (int.TryParse(form["addressId"], out addressIdValue))
                    {
                        var addressById = await addressService.GetAddressByIdAsync(addressIdValue);
                        if (addressById == null)
                        {
                            return Results.BadRequest(new { 
                                success = false, 
                                message = $"Address with ID {addressIdValue} not found" 
                            });
                        }
                        addressId = addressById.Id;
                    }
                    else
                    {
                        var addresses = await addressService.GetAllAddressesAsync();
                        var targetAddress = addresses.FirstOrDefault(a => a.Identifier.Equals(house, StringComparison.OrdinalIgnoreCase));

                        if (targetAddress != null)
                        {
                            addressId = targetAddress.Id;
                        }
                        else
                        {
                            var newAddress = new MicroJack.API.Models.Core.Address
                            {
                                Identifier = house,
                                Extension = "000",
                                Status = "Active"
                            };
                            var createdAddress = await addressService.CreateAddressAsync(newAddress);
                            addressId = createdAddress.Id;
                        }
                    }

                    // Handle file uploads
                    string? ineImageUrl = null;
                    string? faceImageUrl = null;
                    string? plateImageUrl = null;

                    var uploadsPath = Path.Combine(context.RequestServices.GetRequiredService<IWebHostEnvironment>().ContentRootPath, "uploads");

                    if (form.Files["ineImage"] != null)
                    {
                        var ineFile = form.Files["ineImage"];
                        var ineFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N[0..8]}{Path.GetExtension(ineFile.FileName)}";
                        var ineFilePath = Path.Combine(uploadsPath, "ine", ineFileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(ineFilePath));
                        
                        using (var stream = new FileStream(ineFilePath, FileMode.Create))
                        {
                            await ineFile.CopyToAsync(stream);
                        }
                        ineImageUrl = $"/uploads/ine/{ineFileName}";
                    }

                    if (form.Files["faceImage"] != null)
                    {
                        var faceFile = form.Files["faceImage"];
                        var faceFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N[0..8]}{Path.GetExtension(faceFile.FileName)}";
                        var faceFilePath = Path.Combine(uploadsPath, "faces", faceFileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(faceFilePath));
                        
                        using (var stream = new FileStream(faceFilePath, FileMode.Create))
                        {
                            await faceFile.CopyToAsync(stream);
                        }
                        faceImageUrl = $"/uploads/faces/{faceFileName}";
                    }

                    if (form.Files["plateImage"] != null && vehicle != null)
                    {
                        var plateFile = form.Files["plateImage"];
                        var plateFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N[0..8]}{Path.GetExtension(plateFile.FileName)}";
                        var plateFilePath = Path.Combine(uploadsPath, "plates", plateFileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(plateFilePath));
                        
                        using (var stream = new FileStream(plateFilePath, FileMode.Create))
                        {
                            await plateFile.CopyToAsync(stream);
                        }
                        plateImageUrl = $"/uploads/plates/{plateFileName}";
                        vehicle.PlateImageUrl = plateImageUrl;
                    }

                    // Create or update visitor
                    Visitor visitor;
                    var existingVisitors = await visitorService.GetAllVisitorsAsync();
                    visitor = existingVisitors.FirstOrDefault(v => 
                        v.FullName.Equals(visitorFullName, StringComparison.OrdinalIgnoreCase));
                    
                    if (visitor == null)
                    {
                        visitor = new Visitor
                        {
                            FullName = visitorFullName,
                            IneImageUrl = ineImageUrl,
                            FaceImageUrl = faceImageUrl
                        };
                        visitor = await visitorService.CreateVisitorAsync(visitor);
                    }
                    else
                    {
                        visitor.FullName = visitorFullName;
                        if (!string.IsNullOrEmpty(ineImageUrl))
                            visitor.IneImageUrl = ineImageUrl;
                        if (!string.IsNullOrEmpty(faceImageUrl))
                            visitor.FaceImageUrl = faceImageUrl;
                        
                        visitor = await visitorService.UpdateVisitorAsync(visitor.Id, visitor);
                    }

                    // Create or update vehicle
                    if (vehicle != null)
                    {
                        if (vehicle.Id == 0)
                        {
                            // New vehicle
                            vehicle = await vehicleService.CreateVehicleAsync(vehicle);
                        }
                        else
                        {
                            // Update existing vehicle
                            vehicle = await vehicleService.UpdateVehicleAsync(vehicle.Id, vehicle);
                        }
                    }

                    // Create access log
                    var purpose = form["purpose"].ToString() ?? "Visit";
                    var notes = form["notes"].ToString();

                    var accessLog = new AccessLog
                    {
                        VisitorId = visitor.Id,
                        VehicleId = vehicle?.Id,
                        AddressId = addressId,
                        ResidentVisitedId = resident?.Id,
                        EntryTimestamp = DateTime.UtcNow,
                        Comments = notes,
                        EntryGuardId = guardId,
                        Status = "DENTRO"
                    };

                    var createdAccessLog = await accessLogService.CreateAccessLogAsync(accessLog);

                    return Results.Created($"/api/accesslogs/{createdAccessLog.Id}", new
                    {
                        success = true,
                        message = "Entry registered successfully with images",
                        data = new
                        {
                            accessLog = createdAccessLog,
                            visitor = new
                            {
                                visitor.Id,
                                visitor.FullName,
                                visitor.IneImageUrl,
                                visitor.FaceImageUrl
                            },
                            vehicle = vehicle != null ? new
                            {
                                vehicle.Id,
                                vehicle.LicensePlate,
                                vehicle.PlateImageUrl,
                                vehicle.BrandId,
                                vehicle.ColorId,
                                vehicle.TypeId
                            } : null,
                            resident = resident,
                            entryTime = createdAccessLog.EntryTimestamp,
                            guardId = guardId
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Error registering entry with images",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .RequireAuthorization("GuardLevel")
            .DisableAntiforgery()
            .WithName("RegisterUnifiedEntryWithImages")
            .WithSummary("Register a complete entry with visitor, vehicle, and image upload in one operation")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<object>(201)
            .Produces(400)
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
        
        public int? AddressId { get; set; } // Direct address ID
        
        public string? House { get; set; } // House/Address as string
        
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