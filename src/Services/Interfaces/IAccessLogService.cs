using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Services.Interfaces
{
    public interface IAccessLogService
    {
        Task<List<AccessLog>> GetAllAccessLogsAsync();
        Task<AccessLog?> GetAccessLogByIdAsync(int id);
        Task<List<AccessLog>> GetAccessLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<AccessLog>> GetAccessLogsByVisitorAsync(int visitorId);
        Task<List<AccessLog>> GetAccessLogsByVehicleAsync(int vehicleId);
        Task<List<AccessLog>> GetAccessLogsByAddressAsync(int addressId);
        Task<List<AccessLog>> GetAccessLogsByStatusAsync(string status);
        Task<List<AccessLog>> GetActiveAccessLogsAsync(); // Status = "DENTRO"
        Task<AccessLog> CreateAccessLogAsync(AccessLog accessLog);
        Task<AccessLog?> UpdateAccessLogAsync(int id, AccessLog accessLog);
        Task<bool> RegisterExitAsync(int id, int exitGuardId);
        Task<bool> DeleteAccessLogAsync(int id);
        Task<List<AccessLog>> SearchAccessLogsAsync(string searchTerm);
    }
}