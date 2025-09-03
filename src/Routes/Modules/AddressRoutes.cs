using MicroJack.API.Services.Interfaces;
using MicroJack.API.Models.Core;
using MicroJack.API.Middleware;
using MicroJack.API.Models.Enums;

namespace MicroJack.API.Routes.Modules
{
    public static class AddressRoutes
    {
        public static void MapAddressRoutes(this WebApplication app)
        {
            var addressGroup = app.MapGroup("/api/addresses").WithTags("Addresses");

            // GET all addresses
            addressGroup.MapGet("/", async (IAddressService addressService) =>
            {
                try
                {
                    var addresses = await addressService.GetAllAddressesAsync();
                    return Results.Ok(new { success = true, data = addresses });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting addresses", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAllAddresses")
            .Produces<object>(200);

            // GET address by ID
            addressGroup.MapGet("/{id:int}", async (int id, IAddressService addressService) =>
            {
                try
                {
                    var address = await addressService.GetAddressByIdAsync(id);
                    if (address == null)
                        return Results.NotFound(new { success = false, message = "Address not found" });

                    return Results.Ok(new { success = true, data = address });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting address", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAddressById")
            .Produces<object>(200)
            .Produces(404);

            // POST create new address (AdminLevel)
            addressGroup.MapPost("/", async (AddressCreateRequest request, IAddressService addressService) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(request.Identifier))
                        return Results.BadRequest(new { success = false, message = "Identifier is required" });
                    if (string.IsNullOrWhiteSpace(request.Extension))
                        return Results.BadRequest(new { success = false, message = "Extension is required" });

                    var address = new Address
                    {
                        Identifier = request.Identifier.Trim(),
                        Extension = request.Extension.Trim(),
                        Status = request.Status,
                        Message = request.Message
                    };

                    var createdAddress = await addressService.CreateAddressAsync(address);
                    return Results.Created($"/api/addresses/{createdAddress.Id}", new { success = true, data = createdAddress });
                }
                catch (ApplicationException aex)
                {
                    // Conflictos/validación
                    return Results.Conflict(new { success = false, message = aex.Message });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating address", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("CreateAddress")
            .Produces<object>(201)
            .Produces(500);

            // PUT update address (AdminLevel)
            addressGroup.MapPut("/{id:int}", async (int id, AddressUpdateRequest request, IAddressService addressService) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(request.Identifier))
                        return Results.BadRequest(new { success = false, message = "Identifier is required" });
                    if (string.IsNullOrWhiteSpace(request.Extension))
                        return Results.BadRequest(new { success = false, message = "Extension is required" });

                    var address = new Address
                    {
                        Identifier = request.Identifier.Trim(),
                        Extension = request.Extension.Trim(),
                        Status = request.Status,
                        Message = request.Message
                    };

                    var updatedAddress = await addressService.UpdateAddressAsync(id, address);
                    if (updatedAddress == null)
                        return Results.NotFound(new { success = false, message = "Address not found" });

                    return Results.Ok(new { success = true, data = updatedAddress });
                }
                catch (ApplicationException aex)
                {
                    return Results.Conflict(new { success = false, message = aex.Message });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error updating address", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("UpdateAddress")
            .Produces<object>(200)
            .Produces(404);

            // DELETE address (SuperAdminLevel)
            addressGroup.MapDelete("/{id:int}", async (int id, IAddressService addressService) =>
            {
                try
                {
                    var result = await addressService.DeleteAddressAsync(id);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Address not found" });

                    return Results.Ok(new { success = true, message = "Address deleted successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error deleting address", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("DeleteAddress")
            .Produces<object>(200)
            .Produces(404);
        }
    }

    // DTOs
    public class AddressCreateRequest
    {
        public string Identifier { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? Message { get; set; }
    }

    public class AddressUpdateRequest
    {
        public string Identifier { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? Message { get; set; }
    }
}
