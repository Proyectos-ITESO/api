using Microsoft.AspNetCore.Mvc;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Routes.Modules
{
    public static class HousesRoutes
    {
        public static void MapHousesRoutes(this WebApplication app)
        {
            var group = app.MapGroup("/api/casas")
                          .WithTags("Houses Management")
                          .RequireAuthorization();

            // Obtener todas las casas
            group.MapGet("/", async (
                IAddressService addressService,
                HttpContext context) =>
            {
                try
                {
                    var userRole = context.User.FindFirst("Role")?.Value ?? "Guard";
                    var addresses = await addressService.GetAllAddressesAsync();

                    var result = addresses.Select(a => new
                    {
                        a.Id,
                        Casa = a.Identifier, // Calle y número
                        a.Extension,
                        RepresentativeName = a.RepresentativeResident?.FullName,
                        // Solo admins y super admins ven el teléfono del representante
                        RepresentativePhone = (userRole == "Admin" || userRole == "SuperAdmin") 
                            ? a.RepresentativeResident?.Phone 
                            : null,
                        a.RepresentativeResidentId,
                        ResidentCount = a.Residents?.Count ?? 0
                    }).OrderBy(x => x.Casa).ToList();

                    return Results.Ok(new
                    {
                        success = true,
                        count = result.Count,
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error obteniendo casas: {ex.Message}");
                }
            })
            .WithName("GetAllHouses")
            .WithSummary("Obtener todas las casas (guardias ven extensión, admins ven teléfono del representante)");

            // Obtener casa específica por ID
            group.MapGet("/{id}", async (
                int id,
                IAddressService addressService,
                HttpContext context) =>
            {
                try
                {
                    var userRole = context.User.FindFirst("Role")?.Value ?? "Guard";
                    var address = await addressService.GetAddressByIdAsync(id);

                    if (address == null)
                    {
                        return Results.NotFound($"Casa con ID {id} no encontrada");
                    }

                    var result = new
                    {
                        address.Id,
                        Casa = address.Identifier,
                        address.Extension,
                        RepresentativeName = address.RepresentativeResident?.FullName,
                        RepresentativePhone = (userRole == "Admin" || userRole == "SuperAdmin") 
                            ? address.RepresentativeResident?.Phone 
                            : null,
                        address.RepresentativeResidentId,
                        Residents = address.Residents?.Select(r => new
                        {
                            r.Id,
                            r.FullName,
                            // Solo admins ven teléfonos de residentes
                            Phone = (userRole == "Admin" || userRole == "SuperAdmin") ? r.Phone : null
                        }).ToList()
                    };

                    return Results.Ok(new
                    {
                        success = true,
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error obteniendo casa: {ex.Message}");
                }
            })
            .WithName("GetHouseById")
            .WithSummary("Obtener información detallada de una casa");

            // Asignar residente representante (solo admins)
            group.MapPatch("/{id}/representante", async (
                int id,
                [FromBody] SetRepresentativeRequest request,
                IAddressService addressService,
                IResidentService residentService,
                HttpContext context) =>
            {
                try
                {
                    var userRole = context.User.FindFirst("Role")?.Value ?? "Guard";
                    
                    if (userRole == "Guard")
                    {
                        return Results.Forbid();
                    }

                    var address = await addressService.GetAddressByIdAsync(id);
                    if (address == null)
                    {
                        return Results.NotFound($"Casa con ID {id} no encontrada");
                    }

                    // Verificar que el residente existe y pertenece a esta casa
                    var resident = await residentService.GetResidentByIdAsync(request.ResidentId);
                    if (resident == null)
                    {
                        return Results.NotFound($"Residente con ID {request.ResidentId} no encontrado");
                    }

                    if (resident.AddressId != id)
                    {
                        return Results.BadRequest("El residente no pertenece a esta casa");
                    }

                    // Actualizar el representante
                    address.RepresentativeResidentId = request.ResidentId;
                    var updated = await addressService.UpdateAddressAsync(id, address);

                    if (updated == null)
                    {
                        return Results.Problem("No se pudo actualizar el representante");
                    }

                    return Results.Ok(new
                    {
                        success = true,
                        message = "Representante asignado exitosamente",
                        data = new
                        {
                            CasaId = id,
                            Casa = updated.Identifier,
                            RepresentativeId = request.ResidentId,
                            RepresentativeName = resident.FullName,
                            RepresentativePhone = resident.Phone
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error asignando representante: {ex.Message}");
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("SetHouseRepresentative")
            .WithSummary("Asignar residente representante a una casa (solo admins)");

            // Buscar casas
            group.MapGet("/buscar", async (
                [FromQuery] string q,
                IAddressService addressService,
                HttpContext context) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(q))
                        return Results.BadRequest("Parámetro 'q' es requerido");

                    var userRole = context.User.FindFirst("Role")?.Value ?? "Guard";
                    var addresses = await addressService.SearchAddressesAsync(q);

                    var result = addresses.Select(a => new
                    {
                        a.Id,
                        Casa = a.Identifier,
                        a.Extension,
                        RepresentativeName = a.RepresentativeResident?.FullName,
                        RepresentativePhone = (userRole == "Admin" || userRole == "SuperAdmin") 
                            ? a.RepresentativeResident?.Phone 
                            : null,
                        a.RepresentativeResidentId,
                        ResidentCount = a.Residents?.Count ?? 0
                    }).OrderBy(x => x.Casa).ToList();

                    return Results.Ok(new
                    {
                        success = true,
                        searchTerm = q,
                        count = result.Count,
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error buscando casas: {ex.Message}");
                }
            })
            .WithName("SearchHouses")
            .WithSummary("Buscar casas por término");
        }
    }

    public record SetRepresentativeRequest(
        int ResidentId
    );
}
