using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Transaction;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class EventLogService : IEventLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventLogService> _logger;

        public EventLogService(ApplicationDbContext context, ILogger<EventLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<EventLog>> GetAllEventLogsAsync()
        {
            try
            {
                return await _context.EventLogs
                    .Include(el => el.Guard)
                    .OrderByDescending(el => el.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all event logs");
                return new List<EventLog>();
            }
        }

        public async Task<EventLog?> GetEventLogByIdAsync(int id)
        {
            try
            {
                return await _context.EventLogs
                    .Include(el => el.Guard)
                    .FirstOrDefaultAsync(el => el.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event log by ID {Id}", id);
                return null;
            }
        }

        public async Task<List<EventLog>> GetEventLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.EventLogs
                    .Include(el => el.Guard)
                    .Where(el => el.Timestamp >= startDate && el.Timestamp <= endDate)
                    .OrderByDescending(el => el.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs by date range");
                return new List<EventLog>();
            }
        }

        public async Task<List<EventLog>> GetEventLogsByGuardAsync(int guardId)
        {
            try
            {
                return await _context.EventLogs
                    .Include(el => el.Guard)
                    .Where(el => el.GuardId == guardId)
                    .OrderByDescending(el => el.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs by guard {GuardId}", guardId);
                return new List<EventLog>();
            }
        }

        public async Task<EventLog> CreateEventLogAsync(EventLog eventLog)
        {
            try
            {
                // Validate guard exists
                var guardExists = await _context.Guards.AnyAsync(g => g.Id == eventLog.GuardId);
                if (!guardExists)
                {
                    throw new ApplicationException($"Guard with ID {eventLog.GuardId} does not exist");
                }

                _context.EventLogs.Add(eventLog);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Event log created successfully with ID: {Id}", eventLog.Id);
                return eventLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event log");
                throw;
            }
        }

        public async Task<EventLog> CreateEventLogAsync(int guardId, string description)
        {
            var eventLog = new EventLog
            {
                GuardId = guardId,
                Description = description,
                Timestamp = DateTime.UtcNow
            };

            return await CreateEventLogAsync(eventLog);
        }

        public async Task<bool> DeleteEventLogAsync(int id)
        {
            try
            {
                var eventLog = await _context.EventLogs.FindAsync(id);
                if (eventLog == null)
                {
                    return false;
                }

                _context.EventLogs.Remove(eventLog);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Event log deleted successfully: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event log {Id}", id);
                throw;
            }
        }

        public async Task<List<EventLog>> SearchEventLogsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllEventLogsAsync();
                }

                return await _context.EventLogs
                    .Include(el => el.Guard)
                    .Where(el => el.Description.Contains(searchTerm) ||
                               el.Guard.FullName.Contains(searchTerm) ||
                               el.Guard.Username.Contains(searchTerm))
                    .OrderByDescending(el => el.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching event logs with term {SearchTerm}", searchTerm);
                return new List<EventLog>();
            }
        }
    }
}