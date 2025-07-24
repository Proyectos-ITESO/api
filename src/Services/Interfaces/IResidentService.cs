using MicroJack.API.Models.Core;

namespace MicroJack.API.Services.Interfaces
{
    public interface IResidentService
    {
        Task<List<Resident>> GetAllResidentsAsync();
        Task<Resident?> GetResidentByIdAsync(int id);
        Task<List<Resident>> GetResidentsByAddressAsync(int addressId);
        Task<List<Resident>> SearchResidentsAsync(string searchTerm);
        Task<Resident> CreateResidentAsync(Resident resident);
        Task<Resident?> UpdateResidentAsync(int id, Resident resident);
        Task<bool> DeleteResidentAsync(int id);
    }
}