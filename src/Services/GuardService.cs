using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class GuardService : IGuardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GuardService> _logger;

        public GuardService(ApplicationDbContext context, ILogger<GuardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Guard>> GetAllGuardsAsync()
        {
            try
            {
                return await _context.Guards
                    .OrderBy(g => g.FullName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all guards");
                return new List<Guard>();
            }
        }

        public async Task<Guard?> GetGuardByIdAsync(int id)
        {
            try
            {
                return await _context.Guards.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guard by ID {Id}", id);
                return null;
            }
        }

        public async Task<Guard?> GetGuardByUsernameAsync(string username)
        {
            try
            {
                return await _context.Guards
                    .FirstOrDefaultAsync(g => g.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guard by username {Username}", username);
                return null;
            }
        }

        public async Task<Guard> CreateGuardAsync(Guard guard, string password)
        {
            try
            {
                // Check if username already exists
                var existingGuard = await GetGuardByUsernameAsync(guard.Username);
                if (existingGuard != null)
                {
                    throw new ApplicationException($"Username '{guard.Username}' already exists");
                }

                // Hash password
                guard.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                
                _context.Guards.Add(guard);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Guard created successfully: {Username}", guard.Username);
                return guard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating guard");
                throw;
            }
        }

        public async Task<Guard?> UpdateGuardAsync(int id, Guard guard)
        {
            try
            {
                var existingGuard = await _context.Guards.FindAsync(id);
                if (existingGuard == null)
                {
                    return null;
                }

                // Check if new username conflicts with another guard
                if (guard.Username != existingGuard.Username)
                {
                    var conflictGuard = await GetGuardByUsernameAsync(guard.Username);
                    if (conflictGuard != null)
                    {
                        throw new ApplicationException($"Username '{guard.Username}' already exists");
                    }
                }

                existingGuard.FullName = guard.FullName;
                existingGuard.Username = guard.Username;
                existingGuard.IsActive = guard.IsActive;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Guard updated successfully: {Id}", id);
                return existingGuard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating guard {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteGuardAsync(int id)
        {
            try
            {
                var guard = await _context.Guards.FindAsync(id);
                if (guard == null)
                {
                    return false;
                }

                // Check if guard has associated records
                var hasAccessLogs = await _context.AccessLogs
                    .AnyAsync(al => al.EntryGuardId == id || al.ExitGuardId == id);
                
                var hasEventLogs = await _context.EventLogs
                    .AnyAsync(el => el.GuardId == id);

                if (hasAccessLogs || hasEventLogs)
                {
                    throw new ApplicationException("Cannot delete guard with associated access or event logs");
                }

                _context.Guards.Remove(guard);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Guard deleted successfully: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting guard {Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdatePasswordAsync(int id, string newPassword)
        {
            try
            {
                var guard = await _context.Guards.FindAsync(id);
                if (guard == null)
                {
                    return false;
                }

                guard.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Password updated for guard {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for guard {Id}", id);
                return false;
            }
        }

        public async Task<bool> ActivateDeactivateGuardAsync(int id, bool isActive)
        {
            try
            {
                var guard = await _context.Guards.FindAsync(id);
                if (guard == null)
                {
                    return false;
                }

                guard.IsActive = isActive;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Guard {Id} {Status}", id, isActive ? "activated" : "deactivated");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing guard status {Id}", id);
                return false;
            }
        }

        public async Task<Guard?> AuthenticateAsync(string username, string password)
        {
            try
            {
                var guard = await GetGuardByUsernameAsync(username);
                if (guard == null || !guard.IsActive)
                {
                    return null;
                }

                if (BCrypt.Net.BCrypt.Verify(password, guard.PasswordHash))
                {
                    _logger.LogInformation("Guard authenticated successfully: {Username}", username);
                    return guard;
                }

                _logger.LogWarning("Failed authentication attempt for: {Username}", username);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for {Username}", username);
                return null;
            }
        }
    }
}