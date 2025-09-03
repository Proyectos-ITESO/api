using MicroJack.API.Services.Interfaces;
using MicroJack.API.Models.Core;
using MicroJack.API.Models.Enums;
using System.Text.Json;

namespace MicroJack.API.Routes.Modules
{
    public static class AdminRoutes
    {
        public static void MapAdminRoutes(this WebApplication app)
        {
            var adminGroup = app.MapGroup("/api/admin").WithTags("Administration");

            // === ROLES MANAGEMENT ===

            // GET all roles
            adminGroup.MapGet("/roles", async (IRoleService roleService) =>
            {
                try
                {
                    var roles = await roleService.GetAllRolesAsync();
                    return Results.Ok(new { success = true, data = roles });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting roles", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("GetAllRoles")
            .Produces<object>(200);

            // GET role by ID
            adminGroup.MapGet("/roles/{id:int}", async (int id, IRoleService roleService) =>
            {
                try
                {
                    var role = await roleService.GetRoleByIdAsync(id);
                    if (role == null)
                        return Results.NotFound(new { success = false, message = "Role not found" });

                    return Results.Ok(new { success = true, data = role });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting role", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("GetRoleById")
            .Produces<object>(200)
            .Produces(404);

            // POST create new role
            adminGroup.MapPost("/roles", async (RoleCreateRequest request, IRoleService roleService) =>
            {
                try
                {
                    var role = new Role
                    {
                        Name = request.Name,
                        Description = request.Description,
                        Permissions = request.Permissions ?? "[]"
                    };

                    var createdRole = await roleService.CreateRoleAsync(role);
                    return Results.Created($"/api/admin/roles/{createdRole.Id}", new { success = true, data = createdRole });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating role", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("CreateRole")
            .Produces<object>(201);

            // PUT update role
            adminGroup.MapPut("/roles/{id:int}", async (int id, RoleUpdateRequest request, IRoleService roleService) =>
            {
                try
                {
                    var existingRole = await roleService.GetRoleByIdAsync(id);
                    if (existingRole == null)
                        return Results.NotFound(new { success = false, message = "Role not found" });

                    existingRole.Name = request.Name;
                    existingRole.Description = request.Description;
                    existingRole.Permissions = request.Permissions ?? "[]";

                    var updatedRole = await roleService.UpdateRoleAsync(id, existingRole);
                    return Results.Ok(new { success = true, data = updatedRole });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error updating role", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("UpdateRole")
            .Produces<object>(200)
            .Produces(404);

            // DELETE role
            adminGroup.MapDelete("/roles/{id:int}", async (int id, IRoleService roleService) =>
            {
                try
                {
                    var result = await roleService.DeleteRoleAsync(id);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Role not found" });

                    return Results.Ok(new { success = true, message = "Role deleted successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error deleting role", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("DeleteRole")
            .Produces<object>(200)
            .Produces(404);

            // === ROLE PERMISSIONS MANAGEMENT ===

            // GET role permissions
            adminGroup.MapGet("/roles/{id:int}/permissions", async (int id, IRoleService roleService) =>
            {
                try
                {
                    var permissions = await roleService.GetRolePermissionsAsync(id);
                    return Results.Ok(new { success = true, data = permissions });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting role permissions", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("GetRolePermissions")
            .Produces<object>(200);

            // POST add permission to role
            adminGroup.MapPost("/roles/{id:int}/permissions", async (int id, PermissionEnumRequest request, IRoleService roleService) =>
            {
                try
                {
                    var result = await roleService.AddPermissionToRoleAsync(id, request.Permission);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Role or permission not found" });

                    return Results.Ok(new { success = true, message = "Permission added to role successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error adding permission to role", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("AddPermissionToRole")
            .Produces<object>(200)
            .Produces(404);

            // DELETE remove permission from role
            adminGroup.MapDelete("/roles/{id:int}/permissions/{permission:int}", async (int id, int permission, IRoleService roleService) =>
            {
                try
                {
                    var permissionEnum = (Permission)permission;
                    var result = await roleService.RemovePermissionFromRoleAsync(id, permissionEnum);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Role or permission not found" });

                    return Results.Ok(new { success = true, message = "Permission removed from role successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error removing permission from role", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("RemovePermissionFromRole")
            .Produces<object>(200)
            .Produces(404);

            // === USER ROLE MANAGEMENT ===

            // GET user roles
            adminGroup.MapGet("/users/{guardId:int}/roles", async (int guardId, IRoleService roleService) =>
            {
                try
                {
                    var roles = await roleService.GetGuardRolesAsync(guardId);
                    return Results.Ok(new { success = true, data = roles });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting user roles", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("GetUserRoles")
            .Produces<object>(200);

            // POST assign role to user
            adminGroup.MapPost("/users/{guardId:int}/roles", async (int guardId, RoleAssignmentRequest request, IRoleService roleService, HttpContext context) =>
            {
                try
                {
                    // Get current user ID from token claims
                    var currentUserIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("nameid");
                    if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                    {
                        return Results.Unauthorized();
                    }

                    var result = await roleService.AssignRoleToGuardAsync(guardId, request.RoleId, currentUserId);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "User or role not found" });

                    return Results.Ok(new { success = true, message = "Role assigned to user successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error assigning role to user", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("AssignRoleToUser")
            .Produces<object>(200)
            .Produces(404);

            // DELETE remove role from user
            adminGroup.MapDelete("/users/{guardId:int}/roles/{roleId:int}", async (int guardId, int roleId, IRoleService roleService) =>
            {
                try
                {
                    var result = await roleService.RemoveRoleFromGuardAsync(guardId, roleId);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "User or role not found" });

                    return Results.Ok(new { success = true, message = "Role removed from user successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error removing role from user", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("RemoveRoleFromUser")
            .Produces<object>(200)
            .Produces(404);

            // === ADVANCED USER MANAGEMENT ===

            // POST activate/deactivate user
            adminGroup.MapPost("/users/{guardId:int}/toggle-active", async (int guardId, bool isActive, IGuardService guardService) =>
            {
                try
                {
                    var result = await guardService.ActivateDeactivateGuardAsync(guardId, isActive);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "User not found" });

                    return Results.Ok(new { success = true, message = $"User {(isActive ? "activated" : "deactivated")} successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error updating user status", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("ToggleUserActive")
            .Produces<object>(200)
            .Produces(404);

            // POST reset user password (admin only)
            adminGroup.MapPost("/users/{guardId:int}/reset-password", async (int guardId, PasswordResetRequest request, IGuardService guardService) =>
            {
                try
                {
                    var result = await guardService.UpdatePasswordAsync(guardId, request.NewPassword);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "User not found" });

                    return Results.Ok(new { success = true, message = "Password reset successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error resetting password", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("ResetUserPassword")
            .Produces<object>(200)
            .Produces(404);

            // GET user effective permissions
            adminGroup.MapGet("/users/{guardId:int}/permissions", async (int guardId, IRoleService roleService) =>
            {
                try
                {
                    var hasPermission = await Task.WhenAll(
                        roleService.GuardHasRoleAsync(guardId, "Guard"),
                        roleService.GuardHasRoleAsync(guardId, "Admin"),
                        roleService.GuardHasRoleAsync(guardId, "SuperAdmin")
                    );

                    var permissions = new List<string>();
                    if (hasPermission[0]) permissions.Add("Guard");
                    if (hasPermission[1]) permissions.Add("Admin");
                    if (hasPermission[2]) permissions.Add("SuperAdmin");

                    return Results.Ok(new { success = true, data = new { roles = permissions } });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting user permissions", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("GetUserPermissions")
            .Produces<object>(200);

            // === UTILITY ENDPOINTS ===

            // GET all available permissions
            adminGroup.MapGet("/permissions", async () =>
            {
                try
                {
                    var permissions = new
                    {
                        Access = new[] { "access:read", "access:create", "access:update", "access:delete" },
                        Guards = new[] { "guards:read", "guards:create", "guards:update", "guards:delete" },
                        Visitors = new[] { "visitors:read", "visitors:create", "visitors:update", "visitors:delete" },
                        Vehicles = new[] { "vehicles:read", "vehicles:create", "vehicles:update", "vehicles:delete" },
                        Admin = new[] { "admin:read", "admin:create", "admin:update", "admin:delete" },
                        SuperAdmin = new[] { "superadmin:all" }
                    };

                    return Results.Ok(new { success = true, data = permissions });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting permissions", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("GetAllPermissions")
            .Produces<object>(200);

            // GET search users
            adminGroup.MapGet("/users/search", async (string? searchTerm, IGuardService guardService) =>
            {
                try
                {
                    var guards = await guardService.GetAllGuardsAsync();
                    
                    var filteredGuards = string.IsNullOrWhiteSpace(searchTerm) 
                        ? guards 
                        : guards.Where(g => 
                            g.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            g.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

                    return Results.Ok(new { success = true, data = filteredGuards });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error searching users", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("SearchUsers")
            .Produces<object>(200);
        }
    }

    // DTOs
    public class RoleCreateRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Permissions { get; set; }
    }

    public class RoleUpdateRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Permissions { get; set; }
    }

    public class PermissionEnumRequest
    {
        public Permission Permission { get; set; }
    }

    public class RoleAssignmentRequest
    {
        public int RoleId { get; set; }
    }

    public class PasswordResetRequest
    {
        public required string NewPassword { get; set; }
    }
}