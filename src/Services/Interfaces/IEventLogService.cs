using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Services.Interfaces
{
    public interface IEventLogService
    {
        Task<List<EventLog>> GetAllEventLogsAsync();
        Task<EventLog?> GetEventLogByIdAsync(int id);
        Task<List<EventLog>> GetEventLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<EventLog>> GetEventLogsByGuardAsync(int guardId);
        Task<EventLog> CreateEventLogAsync(EventLog eventLog);
        Task<EventLog> CreateEventLogAsync(int guardId, string description);
        Task<bool> DeleteEventLogAsync(int id);
        Task<List<EventLog>> SearchEventLogsAsync(string searchTerm);
    }
}