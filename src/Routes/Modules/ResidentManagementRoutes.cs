using Microsoft.AspNetCore.Mvc;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;
using System.Security.Claims;

namespace MicroJack.API.Routes.Modules
{
    public static class ResidentManagementRoutes
    {
        public static void MapResidentManagementRoutes(this WebApplication app)
        {
            var group = app.MapGroup("/api/residentes")
                          .WithTags("Resident Management")
                          .RequireAuthorization();

            // Obtener residente por casa (guardias ven solo extensión)
            group.MapGet("/casa/{houseIdentifier}", async (
                string houseIdentifier,
                IResidentService residentService,
                IAddressService addressService,
                HttpContext context) =>
            {
                try
                {
                    var userRole = context.User.FindFirst("Role")?.Value ?? "Guard";
                    
                    // Buscar dirección por identificador
                    var addresses = await addressService.SearchAddressesAsync(houseIdentifier);
                    var address = addresses.FirstOrDefault();
                    
                    if (address == null)
                    {
                        return Results.NotFound($"No se encontró la casa: {houseIdentifier}");
                    }

                    var residents = await residentService.GetResidentsByAddressAsync(address.Id);
                    
                    // Filtrar información según rol
                    var result = residents.Select(r => new
                    {
                        r.Id,
                        r.FullName,
                        r.AddressId,
                        Casa = address.Identifier,
                        Extension = address.Extension,
                        // Solo admins y super admins ven el teléfono
                        Phone = (userRole == "Admin" || userRole == "SuperAdmin") ? r.Phone : null
                    }).ToList();

                    return Results.Ok(new
                    {
                        success = true,
                        houseInfo = new
                        {
                            address.Id,
                            address.Identifier,
                            address.Extension
                        },
                        residents = result
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error obteniendo residentes: {ex.Message}");
                }
            })
            .WithName("GetResidentsByCasaWithRole")
            .WithSummary("Obtener residentes por casa (guardias ven extensión, admins ven teléfono)");

            // Crear residente (solo admins y super admins)
            group.MapPost("/", async (
                [FromBody] CreateResidentRequest request,
                IResidentService residentService,
                IAddressService addressService,
                HttpContext context) =>
            {
                try
                {
                    var userRole = context.User.FindFirst("Role")?.Value ?? "Guard";
                    
                    if (userRole == "Guard")
                    {
                        return Results.Forbid();
                    }

                    // Buscar o crear dirección
                    var addresses = await addressService.SearchAddressesAsync(request.HouseIdentifier);
                    var address = addresses.FirstOrDefault();
                    
                    if (address == null)
                    {
                        // Crear nueva dirección
                        address = new Address
                        {
                            Identifier = request.HouseIdentifier,
                            Extension = request.Extension
                        };
                        address = await addressService.CreateAddressAsync(address);
                    }

                    var resident = new Resident
                    {
                        FullName = request.FullName,
                        Phone = request.Phone,
                        AddressId = address.Id
                    };

                    var created = await residentService.CreateResidentAsync(resident);

                    return Results.Created($"/api/residentes/{created.Id}", new
                    {
                        success = true,
                        message = "Residente creado exitosamente",
                        data = new
                        {
                            created.Id,
                            created.FullName,
                            created.Phone,
                            created.AddressId,
                            Casa = address.Identifier,
                            Extension = address.Extension
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error creando residente: {ex.Message}");
                }
            })
            .WithName("CreateResidentWithRole")
            .WithSummary("Crear residente (solo admins y super admins)");

            // Actualizar teléfono de residente (solo admins y super admins)
            group.MapPatch("/{id}/telefono", async (
                int id,
                [FromBody] UpdatePhoneRequest request,
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

                    var resident = await residentService.GetResidentByIdAsync(id);
                    if (resident == null)
                    {
                        return Results.NotFound($"Residente con ID {id} no encontrado");
                    }

                    resident.Phone = request.Phone;
                    var updated = await residentService.UpdateResidentAsync(id, resident);

                    if (updated == null)
                    {
                        return Results.Problem("No se pudo actualizar el residente");
                    }

                    return Results.Ok(new
                    {
                        success = true,
                        message = "Teléfono actualizado exitosamente",
                        data = new
                        {
                            updated.Id,
                            updated.FullName,
                            updated.Phone
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error actualizando teléfono: {ex.Message}");
                }
            })
            .WithName("UpdateResidentPhoneWithRole")
            .WithSummary("Actualizar teléfono de residente (solo admins y super admins)");

            // Buscar residentes (con filtro de información por rol)
            group.MapGet("/buscar", async (
                [FromQuery] string q,
                IResidentService residentService,
                HttpContext context) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(q))
                        return Results.BadRequest("Parámetro 'q' es requerido");

                    var userRole = context.User.FindFirst("Role")?.Value ?? "Guard";
                    var residents = await residentService.SearchResidentsAsync(q);

                    var result = residents.Select(r => new
                    {
                        r.Id,
                        r.FullName,
                        r.AddressId,
                        Casa = r.Address?.Identifier,
                        Extension = r.Address?.Extension,
                        // Solo admins y super admins ven el teléfono
                        Phone = (userRole == "Admin" || userRole == "SuperAdmin") ? r.Phone : null
                    }).ToList();

                    return Results.Ok(new
                    {
                        success = true,
                        count = result.Count,
                        searchTerm = q,
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error buscando residentes: {ex.Message}");
                }
            })
            .WithName("SearchResidentsWithRole")
            .WithSummary("Buscar residentes (guardias ven extensión, admins ven teléfono)");
        }
    }

    public record CreateResidentRequest(
        string FullName,
        string Phone,
        string HouseIdentifier,
        string Extension
    );

    public record UpdatePhoneRequest(
        string Phone
    );
}