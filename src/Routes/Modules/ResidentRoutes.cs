using MicroJack.API.Services.Interfaces;
using MicroJack.API.Models.Core;

namespace MicroJack.API.Routes.Modules
{
    public static class ResidentRoutes
    {
        public static void MapResidentRoutes(this WebApplication app)
        {
            var residentGroup = app.MapGroup("/api/residents").WithTags("Residents");

            // GET all residents
            residentGroup.MapGet("/", async (IResidentService residentService) =>
            {
                try
                {
                    var residents = await residentService.GetAllResidentsAsync();
                    return Results.Ok(new { success = true, data = residents });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting residents", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAllResidents")
            .Produces<object>(200);

            // GET resident by ID
            residentGroup.MapGet("/{id:int}", async (int id, IResidentService residentService) =>
            {
                try
                {
                    var resident = await residentService.GetResidentByIdAsync(id);
                    if (resident == null)
                        return Results.NotFound(new { success = false, message = "Resident not found" });

                    return Results.Ok(new { success = true, data = resident });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting resident", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetResidentById")
            .Produces<object>(200)
            .Produces(404);

            // GET residents by address
            residentGroup.MapGet("/address/{addressId:int}", async (int addressId, IResidentService residentService) =>
            {
                try
                {
                    var residents = await residentService.GetResidentsByAddressAsync(addressId);
                    return Results.Ok(new { success = true, data = residents });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting residents by address", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetResidentsByAddress")
            .Produces<object>(200);

            // POST create new resident
            residentGroup.MapPost("/", async (ResidentCreateRequest request, IResidentService residentService) =>
            {
                try
                {
                    var resident = new Resident
                    {
                        FullName = request.FullName,
                        PhoneExtension = request.PhoneExtension,
                        AddressId = request.AddressId
                    };

                    var createdResident = await residentService.CreateResidentAsync(resident);
                    return Results.Created($"/api/residents/{createdResident.Id}", new { success = true, data = createdResident });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating resident", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("CreateResident")
            .Produces<object>(201)
            .Produces(500);

            // PUT update resident
            residentGroup.MapPut("/{id:int}", async (int id, ResidentUpdateRequest request, IResidentService residentService) =>
            {
                try
                {
                    var resident = new Resident
                    {
                        FullName = request.FullName,
                        PhoneExtension = request.PhoneExtension,
                        AddressId = request.AddressId
                    };

                    var updatedResident = await residentService.UpdateResidentAsync(id, resident);
                    if (updatedResident == null)
                        return Results.NotFound(new { success = false, message = "Resident not found" });

                    return Results.Ok(new { success = true, data = updatedResident });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error updating resident", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("UpdateResident")
            .Produces<object>(200)
            .Produces(404);

            // DELETE resident
            residentGroup.MapDelete("/{id:int}", async (int id, IResidentService residentService) =>
            {
                try
                {
                    var result = await residentService.DeleteResidentAsync(id);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Resident not found" });

                    return Results.Ok(new { success = true, message = "Resident deleted successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error deleting resident", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("DeleteResident")
            .Produces<object>(200)
            .Produces(404);
        }
    }

    // DTOs
    public class ResidentCreateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneExtension { get; set; }
        public int AddressId { get; set; }
    }

    public class ResidentUpdateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneExtension { get; set; }
        public int AddressId { get; set; }
    }
}