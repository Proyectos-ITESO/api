using MicroJack.API.Services.Interfaces;
using MicroJack.API.Models.Catalog;

namespace MicroJack.API.Routes.Modules
{
    public static class CatalogRoutes
    {
        public static void MapCatalogRoutes(this WebApplication app)
        {
            var catalogGroup = app.MapGroup("/api/catalogs").WithTags("Catalogs");

            // Vehicle Brands
            catalogGroup.MapGet("/vehicle-brands", async (ICatalogService<VehicleBrand> brandService) =>
            {
                try
                {
                    var brands = await brandService.GetAllAsync();
                    return Results.Ok(new { success = true, data = brands });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting vehicle brands", detail: ex.Message, statusCode: 500);
                }
            })
            .WithName("GetVehicleBrands")
            .Produces<object>(200);

            // Vehicle Colors
            catalogGroup.MapGet("/vehicle-colors", async (ICatalogService<VehicleColor> colorService) =>
            {
                try
                {
                    var colors = await colorService.GetAllAsync();
                    return Results.Ok(new { success = true, data = colors });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting vehicle colors", detail: ex.Message, statusCode: 500);
                }
            })
            .WithName("GetVehicleColors")
            .Produces<object>(200);

            // Vehicle Types
            catalogGroup.MapGet("/vehicle-types", async (ICatalogService<VehicleType> typeService) =>
            {
                try
                {
                    var types = await typeService.GetAllAsync();
                    return Results.Ok(new { success = true, data = types });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting vehicle types", detail: ex.Message, statusCode: 500);
                }
            })
            .WithName("GetVehicleTypes")
            .Produces<object>(200);

            // Visit Reasons
            catalogGroup.MapGet("/visit-reasons", async (ICatalogService<VisitReason> reasonService) =>
            {
                try
                {
                    var reasons = await reasonService.GetAllAsync();
                    return Results.Ok(new { success = true, data = reasons });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting visit reasons", detail: ex.Message, statusCode: 500);
                }
            })
            .WithName("GetVisitReasons")
            .Produces<object>(200);

            // CREATE endpoints for catalog management
            catalogGroup.MapPost("/vehicle-brands", async (VehicleBrandRequest request, ICatalogService<VehicleBrand> brandService) =>
            {
                try
                {
                    var brand = new VehicleBrand { Name = request.Name };
                    var created = await brandService.CreateAsync(brand);
                    return Results.Created($"/api/catalogs/vehicle-brands/{created.Id}", new { success = true, data = created });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating vehicle brand", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("CreateVehicleBrand")
            .Produces<object>(201);

            catalogGroup.MapPost("/vehicle-colors", async (VehicleColorRequest request, ICatalogService<VehicleColor> colorService) =>
            {
                try
                {
                    var color = new VehicleColor { Name = request.Name };
                    var created = await colorService.CreateAsync(color);
                    return Results.Created($"/api/catalogs/vehicle-colors/{created.Id}", new { success = true, data = created });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating vehicle color", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("CreateVehicleColor")
            .Produces<object>(201);
        }
    }

    // DTOs for catalog requests
    public class VehicleBrandRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class VehicleColorRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class VehicleTypeRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class VisitReasonRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}