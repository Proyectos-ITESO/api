using MicroJack.API.Models.Core;

namespace MicroJack.API.Services.Interfaces
{
    public interface IVisitorService
    {
        Task<List<Visitor>> GetAllVisitorsAsync();
        Task<Visitor?> GetVisitorByIdAsync(int id);
        Task<List<Visitor>> SearchVisitorsAsync(string searchTerm);
        Task<Visitor> CreateVisitorAsync(Visitor visitor);
        Task<Visitor?> UpdateVisitorAsync(int id, Visitor visitor);
        Task<bool> DeleteVisitorAsync(int id);
        Task<bool> UpdateVisitorImagesAsync(int id, string? ineImageUrl, string? faceImageUrl);
    }
}