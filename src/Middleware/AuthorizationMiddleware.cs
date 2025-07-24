using System.Security.Claims;
using System.Text.Json;
using MicroJack.API.Models.Enums;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthorizationMiddleware> _logger;

        public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the endpoint requires authorization
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var requiresPermissionAttribute = endpoint.Metadata.GetMetadata<RequiresPermissionAttribute>();
                if (requiresPermissionAttribute != null)
                {
                    await CheckPermissionAsync(context, requiresPermissionAttribute.Permission);
                    return;
                }

                var requiresRoleAttribute = endpoint.Metadata.GetMetadata<RequiresRoleAttribute>();
                if (requiresRoleAttribute != null)
                {
                    await CheckRoleAsync(context, requiresRoleAttribute.Role);
                    return;
                }
            }

            // Continue to next middleware if no authorization required
            await _next(context);
        }

        private async Task CheckPermissionAsync(HttpContext context, Permission requiredPermission)
        {
            try
            {
                // Get guard ID from claims
                var guardIdClaim = context.User.FindFirst("GuardId");
                if (guardIdClaim == null || !int.TryParse(guardIdClaim.Value, out int guardId))
                {
                    await HandleUnauthorized(context, "No valid guard ID found in token");
                    return;
                }

                // Get role service
                var roleService = context.RequestServices.GetRequiredService<IRoleService>();

                // Check if guard has the required permission
                bool hasPermission = await roleService.GuardHasPermissionAsync(guardId, requiredPermission);
                if (!hasPermission)
                {
                    await HandleForbidden(context, $"Guard {guardId} lacks required permission: {requiredPermission}");
                    return;
                }

                _logger.LogDebug("Guard {GuardId} authorized for permission {Permission}", guardId, requiredPermission);
                
                // Continue to next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission authorization");
                await HandleInternalError(context, "Authorization check failed");
            }
        }

        private async Task CheckRoleAsync(HttpContext context, string requiredRole)
        {
            try
            {
                // Get guard ID from claims
                var guardIdClaim = context.User.FindFirst("GuardId");
                if (guardIdClaim == null || !int.TryParse(guardIdClaim.Value, out int guardId))
                {
                    await HandleUnauthorized(context, "No valid guard ID found in token");
                    return;
                }

                // Get role service
                var roleService = context.RequestServices.GetRequiredService<IRoleService>();

                // Check if guard has the required role
                bool hasRole = await roleService.GuardHasRoleAsync(guardId, requiredRole);
                if (!hasRole)
                {
                    await HandleForbidden(context, $"Guard {guardId} lacks required role: {requiredRole}");
                    return;
                }

                _logger.LogDebug("Guard {GuardId} authorized for role {Role}", guardId, requiredRole);
                
                // Continue to next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role authorization");
                await HandleInternalError(context, "Authorization check failed");
            }
        }

        private async Task HandleUnauthorized(HttpContext context, string message)
        {
            _logger.LogWarning("Unauthorized request: {Message}", message);
            
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                success = false,
                message = "Unauthorized",
                details = message
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private async Task HandleForbidden(HttpContext context, string message)
        {
            _logger.LogWarning("Forbidden request: {Message}", message);
            
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                success = false,
                message = "Forbidden",
                details = message
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private async Task HandleInternalError(HttpContext context, string message)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                success = false,
                message = "Internal server error",
                details = message
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

    // Authorization attributes
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequiresPermissionAttribute : Attribute
    {
        public Permission Permission { get; }

        public RequiresPermissionAttribute(Permission permission)
        {
            Permission = permission;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequiresRoleAttribute : Attribute
    {
        public string Role { get; }

        public RequiresRoleAttribute(string role)
        {
            Role = role;
        }
    }

    // Extension methods for easier registration
    public static class AuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizationMiddleware>();
        }
    }
}