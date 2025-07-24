using MicroJack.API.Services.Interfaces;
using MicroJack.API.Models.Core;
using MicroJack.API.Middleware;
using MicroJack.API.Models.Enums;

namespace MicroJack.API.Routes.Modules
{
    public static class GuardRoutes
    {
        public static void MapGuardRoutes(this WebApplication app)
        {
            var guardGroup = app.MapGroup("/api/guards").WithTags("Guards");

            // GET all guards (Admin level required)
            guardGroup.MapGet("/", async (IGuardService guardService) =>
            {
                try
                {
                    var guards = await guardService.GetAllGuardsAsync();
                    return Results.Ok(new { success = true, data = guards });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting guards", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("GetAllGuards")
            .Produces<object>(200);

            // GET guard by ID
            guardGroup.MapGet("/{id:int}", async (int id, IGuardService guardService) =>
            {
                try
                {
                    var guard = await guardService.GetGuardByIdAsync(id);
                    if (guard == null)
                        return Results.NotFound(new { success = false, message = "Guard not found" });

                    return Results.Ok(new { success = true, data = guard });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting guard", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("GetGuardById")
            .Produces<object>(200)
            .Produces(404);

            // POST create new guard (Admin level required) 
            guardGroup.MapPost("/", async (GuardCreateRequest request, IGuardService guardService) =>
            {
                try
                {
                    var guard = new Guard
                    {
                        FullName = request.FullName,
                        Username = request.Username,
                        IsActive = request.IsActive
                    };

                    var createdGuard = await guardService.CreateGuardAsync(guard, request.Password);
                    return Results.Created($"/api/guards/{createdGuard.Id}", new { success = true, data = createdGuard });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating guard", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("CreateGuard")
            .Produces<object>(201)
            .Produces(500);

            // PUT update guard
            guardGroup.MapPut("/{id:int}", async (int id, GuardUpdateRequest request, IGuardService guardService) =>
            {
                try
                {
                    var existingGuard = await guardService.GetGuardByIdAsync(id);
                    if (existingGuard == null)
                        return Results.NotFound(new { success = false, message = "Guard not found" });

                    existingGuard.FullName = request.FullName;
                    existingGuard.IsActive = request.IsActive;
                    
                    if (!string.IsNullOrEmpty(request.Password))
                    {
                        existingGuard.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    }

                    var updatedGuard = await guardService.UpdateGuardAsync(id, existingGuard);
                    return Results.Ok(new { success = true, data = updatedGuard });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error updating guard", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("UpdateGuard")
            .Produces<object>(200)
            .Produces(404);

            // DELETE guard (SuperAdmin level required for destructive operations)
            guardGroup.MapDelete("/{id:int}", async (int id, IGuardService guardService) =>
            {
                try
                {
                    var result = await guardService.DeleteGuardAsync(id);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Guard not found" });

                    return Results.Ok(new { success = true, message = "Guard deleted successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error deleting guard", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("DeleteGuard")
            .Produces<object>(200)
            .Produces(404);
        }
    }

    // DTOs
    public class GuardCreateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class GuardUpdateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? Password { get; set; }
        public bool IsActive { get; set; } = true;
    }
}