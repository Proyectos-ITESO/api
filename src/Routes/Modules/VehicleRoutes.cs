using MicroJack.API.Services.Interfaces;
using MicroJack.API.Models.Core;
using MicroJack.API.Middleware;
using MicroJack.API.Models.Enums;

namespace MicroJack.API.Routes.Modules
{
    public static class VehicleRoutes
    {
        public static void MapVehicleRoutes(this WebApplication app)
        {
            var vehicleGroup = app.MapGroup("/api/vehicles").WithTags("Vehicles");

            // GET all vehicles
            vehicleGroup.MapGet("/", async (IVehicleService vehicleService) =>
            {
                try
                {
                    var vehicles = await vehicleService.GetAllVehiclesAsync();
                    return Results.Ok(new { success = true, data = vehicles });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting vehicles", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("GetAllVehicles")
            .Produces<object>(200);

            // GET vehicle by ID
            vehicleGroup.MapGet("/{id:int}", async (int id, IVehicleService vehicleService) =>
            {
                try
                {
                    var vehicle = await vehicleService.GetVehicleByIdAsync(id);
                    if (vehicle == null)
                        return Results.NotFound(new { success = false, message = "Vehicle not found" });

                    return Results.Ok(new { success = true, data = vehicle });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting vehicle", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetVehicleById")
            .Produces<object>(200)
            .Produces(404);

            // GET vehicle by license plate
            vehicleGroup.MapGet("/plate/{licensePlate}", async (string licensePlate, IVehicleService vehicleService) =>
            {
                try
                {
                    var vehicle = await vehicleService.GetVehicleByLicensePlateAsync(licensePlate);
                    if (vehicle == null)
                        return Results.NotFound(new { success = false, message = "Vehicle not found" });

                    return Results.Ok(new { success = true, data = vehicle });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting vehicle", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetVehicleByPlate")
            .Produces<object>(200)
            .Produces(404);

            // POST create new vehicle
            vehicleGroup.MapPost("/", async (VehicleCreateRequest request, IVehicleService vehicleService) =>
            {
                try
                {
                    var vehicle = new Vehicle
                    {
                        LicensePlate = request.LicensePlate.ToUpper(),
                        PlateImageUrl = request.PlateImageUrl,
                        BrandId = request.BrandId,
                        ColorId = request.ColorId,
                        TypeId = request.TypeId
                    };

                    var createdVehicle = await vehicleService.CreateVehicleAsync(vehicle);
                    return Results.Created($"/api/vehicles/{createdVehicle.Id}", new { success = true, data = createdVehicle });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating vehicle", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("CreateVehicle")
            .Produces<object>(201)
            .Produces(500);

            // PUT update vehicle
            vehicleGroup.MapPut("/{id:int}", async (int id, VehicleUpdateRequest request, IVehicleService vehicleService) =>
            {
                try
                {
                    var vehicle = new Vehicle
                    {
                        LicensePlate = request.LicensePlate.ToUpper(),
                        PlateImageUrl = request.PlateImageUrl,
                        BrandId = request.BrandId,
                        ColorId = request.ColorId,
                        TypeId = request.TypeId
                    };

                    var updatedVehicle = await vehicleService.UpdateVehicleAsync(id, vehicle);
                    if (updatedVehicle == null)
                        return Results.NotFound(new { success = false, message = "Vehicle not found" });

                    return Results.Ok(new { success = true, data = updatedVehicle });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error updating vehicle", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("UpdateVehicle")
            .Produces<object>(200)
            .Produces(404);

            // DELETE vehicle
            vehicleGroup.MapDelete("/{id:int}", async (int id, IVehicleService vehicleService) =>
            {
                try
                {
                    var result = await vehicleService.DeleteVehicleAsync(id);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Vehicle not found" });

                    return Results.Ok(new { success = true, message = "Vehicle deleted successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error deleting vehicle", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("DeleteVehicle")
            .Produces<object>(200)
            .Produces(404);
        }
    }

    // DTOs
    public class VehicleCreateRequest
    {
        [Required]
        public string LicensePlate { get; set; } = string.Empty;
        public string? PlateImageUrl { get; set; }
        public int? BrandId { get; set; }
        public int? ColorId { get; set; }
        public int? TypeId { get; set; }
    }

    public class VehicleUpdateRequest
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string? PlateImageUrl { get; set; }
        public int? BrandId { get; set; }
        public int? ColorId { get; set; }
        public int? TypeId { get; set; }
    }
}