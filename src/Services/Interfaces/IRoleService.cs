using MicroJack.API.Models.Core;
using MicroJack.API.Models.Enums;

namespace MicroJack.API.Services.Interfaces
{
    public interface IRoleService
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int id);
        Task<Role?> GetRoleByNameAsync(string name);
        Task<Role> CreateRoleAsync(Role role);
        Task<Role?> UpdateRoleAsync(int id, Role role);
        Task<bool> DeleteRoleAsync(int id);
        
        // Role-Permission management
        Task<bool> AddPermissionToRoleAsync(int roleId, Permission permission);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, Permission permission);
        Task<List<Permission>> GetRolePermissionsAsync(int roleId);
        
        // Guard-Role management
        Task<bool> AssignRoleToGuardAsync(int guardId, int roleId, int assignedBy);
        Task<bool> RemoveRoleFromGuardAsync(int guardId, int roleId);
        Task<List<Role>> GetGuardRolesAsync(int guardId);
        Task<bool> GuardHasPermissionAsync(int guardId, Permission permission);
        Task<bool> GuardHasRoleAsync(int guardId, string roleName);
    }
}