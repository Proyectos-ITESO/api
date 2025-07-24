using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Models.Enums;
using MicroJack.API.Services.Interfaces;
using System.Text.Json;

namespace MicroJack.API.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoleService> _logger;

        public RoleService(ApplicationDbContext context, ILogger<RoleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            try
            {
                return await _context.Roles
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return new List<Role>();
            }
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            try
            {
                return await _context.Roles.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by ID {Id}", id);
                return null;
            }
        }

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            try
            {
                return await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by name {Name}", name);
                return null;
            }
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            try
            {
                // Check if role name already exists
                var existingRole = await GetRoleByNameAsync(role.Name);
                if (existingRole != null)
                {
                    throw new ApplicationException($"Role '{role.Name}' already exists");
                }

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Role created successfully: {Name}", role.Name);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                throw;
            }
        }

        public async Task<Role?> UpdateRoleAsync(int id, Role role)
        {
            try
            {
                var existingRole = await _context.Roles.FindAsync(id);
                if (existingRole == null)
                {
                    return null;
                }

                // Check if new name conflicts with another role
                if (role.Name != existingRole.Name)
                {
                    var conflictRole = await GetRoleByNameAsync(role.Name);
                    if (conflictRole != null)
                    {
                        throw new ApplicationException($"Role '{role.Name}' already exists");
                    }
                }

                existingRole.Name = role.Name;
                existingRole.Description = role.Description;
                existingRole.Permissions = role.Permissions;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Role updated successfully: {Id}", id);
                return existingRole;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.GuardRoles)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    return false;
                }

                // Check if role is assigned to guards
                if (role.GuardRoles.Any())
                {
                    throw new ApplicationException("Cannot delete role that is assigned to guards");
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Role deleted successfully: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {Id}", id);
                throw;
            }
        }

        public async Task<bool> AddPermissionToRoleAsync(int roleId, Permission permission)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    return false;
                }

                var permissions = GetPermissionsFromJson(role.Permissions);
                if (!permissions.Contains(permission))
                {
                    permissions.Add(permission);
                    role.Permissions = JsonSerializer.Serialize(permissions);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Permission {Permission} added to role {RoleId}", permission, roleId);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding permission to role {RoleId}", roleId);
                return false;
            }
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, Permission permission)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    return false;
                }

                var permissions = GetPermissionsFromJson(role.Permissions);
                if (permissions.Remove(permission))
                {
                    role.Permissions = JsonSerializer.Serialize(permissions);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Permission {Permission} removed from role {RoleId}", permission, roleId);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing permission from role {RoleId}", roleId);
                return false;
            }
        }

        public async Task<List<Permission>> GetRolePermissionsAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    return new List<Permission>();
                }

                return GetPermissionsFromJson(role.Permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for role {RoleId}", roleId);
                return new List<Permission>();
            }
        }

        public async Task<bool> AssignRoleToGuardAsync(int guardId, int roleId, int assignedBy)
        {
            try
            {
                // Check if assignment already exists
                var existingAssignment = await _context.GuardRoles
                    .FirstOrDefaultAsync(gr => gr.GuardId == guardId && gr.RoleId == roleId);

                if (existingAssignment != null)
                {
                    return true; // Already assigned
                }

                var guardRole = new GuardRole
                {
                    GuardId = guardId,
                    RoleId = roleId,
                    AssignedBy = assignedBy
                };

                _context.GuardRoles.Add(guardRole);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Role {RoleId} assigned to guard {GuardId}", roleId, guardId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to guard");
                return false;
            }
        }

        public async Task<bool> RemoveRoleFromGuardAsync(int guardId, int roleId)
        {
            try
            {
                var guardRole = await _context.GuardRoles
                    .FirstOrDefaultAsync(gr => gr.GuardId == guardId && gr.RoleId == roleId);

                if (guardRole == null)
                {
                    return false;
                }

                _context.GuardRoles.Remove(guardRole);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Role {RoleId} removed from guard {GuardId}", roleId, guardId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from guard");
                return false;
            }
        }

        public async Task<List<Role>> GetGuardRolesAsync(int guardId)
        {
            try
            {
                return await _context.GuardRoles
                    .Include(gr => gr.Role)
                    .Where(gr => gr.GuardId == guardId)
                    .Select(gr => gr.Role)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for guard {GuardId}", guardId);
                return new List<Role>();
            }
        }

        public async Task<bool> GuardHasPermissionAsync(int guardId, Permission permission)
        {
            try
            {
                var guardRoles = await _context.GuardRoles
                    .Include(gr => gr.Role)
                    .Where(gr => gr.GuardId == guardId)
                    .Select(gr => gr.Role)
                    .ToListAsync();

                foreach (var role in guardRoles)
                {
                    var permissions = GetPermissionsFromJson(role.Permissions);
                    if (permissions.Contains(Permission.SuperAdmin) || permissions.Contains(permission))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission for guard {GuardId}", guardId);
                return false;
            }
        }

        public async Task<bool> GuardHasRoleAsync(int guardId, string roleName)
        {
            try
            {
                return await _context.GuardRoles
                    .Include(gr => gr.Role)
                    .AnyAsync(gr => gr.GuardId == guardId && gr.Role.Name == roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role for guard {GuardId}", guardId);
                return false;
            }
        }

        private List<Permission> GetPermissionsFromJson(string permissionsJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionsJson))
                {
                    return new List<Permission>();
                }

                return JsonSerializer.Deserialize<List<Permission>>(permissionsJson) ?? new List<Permission>();
            }
            catch
            {
                return new List<Permission>();
            }
        }
    }
}