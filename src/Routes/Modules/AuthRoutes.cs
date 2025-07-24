using System.ComponentModel.DataAnnotations;
using MicroJack.API.Services.Interfaces;
using MicroJack.API.Middleware;
using MicroJack.API.Models.Enums;

namespace MicroJack.API.Routes.Modules
{
    public static class AuthRoutes
    {
        public static void MapAuthRoutes(this WebApplication app)
        {
            var authGroup = app.MapGroup("/api/auth").WithTags("Authentication");

            // Login endpoint
            authGroup.MapPost("/login", async (LoginRequest request, IAuthenticationService authService) =>
            {
                try
                {
                    // Validate request
                    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = "Username and password are required"
                        });
                    }

                    var result = await authService.LoginAsync(request.Username, request.Password);

                    if (result.Success)
                    {
                        return Results.Ok(new
                        {
                            success = true,
                            message = result.Message,
                            token = result.Token,
                            guard = new
                            {
                                id = result.Guard?.Id,
                                username = result.Guard?.Username,
                                fullName = result.Guard?.FullName,
                                isActive = result.Guard?.IsActive,
                                roles = result.Roles,
                                isAdmin = result.Roles.Contains("Admin") || result.Roles.Contains("SuperAdmin")
                            }
                        });
                    }
                    else
                    {
                        return Results.Unauthorized();
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Login error",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .WithName("Login")
            .WithSummary("Authenticate guard and get JWT token")
            .Produces<LoginResponse>(200)
            .Produces(400)
            .Produces(401)
            .Produces(500);

            // Logout endpoint
            authGroup.MapPost("/logout", async (IAuthenticationService authService, HttpContext context) =>
            {
                try
                {
                    var guardIdClaim = context.User.FindFirst("GuardId");
                    if (guardIdClaim != null && int.TryParse(guardIdClaim.Value, out int guardId))
                    {
                        await authService.LogoutAsync(guardId);
                        return Results.Ok(new
                        {
                            success = true,
                            message = "Successfully logged out"
                        });
                    }

                    return Results.BadRequest(new
                    {
                        success = false,
                        message = "Invalid token"
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Logout error",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .WithName("Logout")
            .WithSummary("Logout current guard")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(500);

            // Change password endpoint
            authGroup.MapPost("/change-password", async (ChangePasswordRequest request, IAuthenticationService authService, HttpContext context) =>
            {
                try
                {
                    var guardIdClaim = context.User.FindFirst("GuardId");
                    if (guardIdClaim == null || !int.TryParse(guardIdClaim.Value, out int guardId))
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = "Invalid token"
                        });
                    }

                    // Validate request
                    if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = "Current password and new password are required"
                        });
                    }

                    if (request.NewPassword.Length < 6)
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = "New password must be at least 6 characters long"
                        });
                    }

                    var success = await authService.ChangePasswordAsync(guardId, request.CurrentPassword, request.NewPassword);

                    if (success)
                    {
                        return Results.Ok(new
                        {
                            success = true,
                            message = "Password changed successfully"
                        });
                    }
                    else
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = "Current password is incorrect"
                        });
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Change password error",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .WithName("ChangePassword")
            .WithSummary("Change guard password")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(500);

            // Health check endpoint (no auth required)
            authGroup.MapGet("/health", async () =>
            {
                return Results.Ok(new 
                { 
                    success = true, 
                    message = "Authentication service is healthy", 
                    timestamp = DateTime.UtcNow,
                    policies = new[]
                    {
                        "GuardLevel: Guard, Admin, SuperAdmin",
                        "AdminLevel: Admin, SuperAdmin", 
                        "SuperAdminLevel: SuperAdmin"
                    }
                });
            })
            .WithName("AuthHealthCheck")
            .WithSummary("Check authentication service health and available policies")
            .Produces(200);

            // Debug endpoint to check users (temporary)
            authGroup.MapGet("/debug/users", async (IGuardService guardService) =>
            {
                try
                {
                    var guards = await guardService.GetAllGuardsAsync();
                    return Results.Ok(new
                    {
                        success = true,
                        count = guards.Count(),
                        users = guards.Select(g => new { 
                            id = g.Id, 
                            username = g.Username, 
                            fullName = g.FullName, 
                            isActive = g.IsActive,
                            hasPassword = !string.IsNullOrEmpty(g.PasswordHash)
                        })
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting users", detail: ex.Message, statusCode: 500);
                }
            })
            .WithName("DebugUsers")
            .WithSummary("Debug endpoint to check existing users")
            .Produces(200);

            // Get current user info
            authGroup.MapGet("/me", async (IRoleService roleService, HttpContext context) =>
            {
                try
                {
                    var guardIdClaim = context.User.FindFirst("GuardId");
                    var usernameClaim = context.User.FindFirst("Username");
                    var fullNameClaim = context.User.FindFirst("FullName");

                    if (guardIdClaim == null || !int.TryParse(guardIdClaim.Value, out int guardId))
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = "Invalid token"
                        });
                    }

                    var roles = await roleService.GetGuardRolesAsync(guardId);
                    var roleNames = roles.Select(r => r.Name).ToList();

                    return Results.Ok(new
                    {
                        success = true,
                        guard = new
                        {
                            id = guardId,
                            username = usernameClaim?.Value,
                            fullName = fullNameClaim?.Value,
                            roles = roleNames,
                            isAdmin = roleNames.Contains("Admin") || roleNames.Contains("SuperAdmin")
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Get user info error",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .WithName("GetCurrentUser")
            .WithSummary("Get current authenticated guard information")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(500);
        }
    }

    // Request/Response DTOs
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public GuardInfo? Guard { get; set; }
    }

    public class GuardInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsAdmin { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}