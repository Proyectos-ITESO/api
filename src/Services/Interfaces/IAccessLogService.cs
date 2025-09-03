using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Services.Interfaces
{
    public interface IAccessLogService
    {
        // Métodos básicos existentes
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

        // Nuevos métodos para búsqueda avanzada
        Task<List<AccessLog>> GetAccessLogsByDateAsync(DateTime date);
        Task<List<AccessLog>> GetAccessLogsByVisitorNameAsync(string visitorName);
        Task<List<AccessLog>> GetAccessLogsByLicensePlateAsync(string licensePlate);
        Task<List<AccessLog>> GetAccessLogsByVehicleCharacteristicsAsync(int? brandId = null, int? colorId = null, int? typeId = null);
        Task<List<AccessLog>> GetAccessLogsByAddressIdentifierAsync(string addressIdentifier);
        
        // Métodos para historial
        Task<List<AccessLog>> GetVisitorHistoryAsync(int visitorId);
        Task<List<AccessLog>> GetVehicleHistoryAsync(string licensePlate);
        Task<List<AccessLog>> GetAddressHistoryAsync(int addressId);
        
        // Método para búsqueda combinada
        Task<List<AccessLog>> AdvancedSearchAsync(AccessLogSearchRequest request);
    }

    // Request DTO para búsqueda avanzada
    public class AccessLogSearchRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? VisitorName { get; set; }
        public string? LicensePlate { get; set; }
        public int? BrandId { get; set; }
        public int? ColorId { get; set; }
        public int? TypeId { get; set; }
        public string? AddressIdentifier { get; set; }
        public string? Status { get; set; }
        public int? ResidentId { get; set; }
        public bool IncludePhotos { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}