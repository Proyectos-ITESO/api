using MicroJack.API.Models.Core;

namespace MicroJack.API.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<List<Vehicle>> GetAllVehiclesAsync();
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        Task<Vehicle?> GetVehicleByLicensePlateAsync(string licensePlate);
        Task<List<Vehicle>> SearchVehiclesAsync(string searchTerm);
        Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
        Task<Vehicle?> UpdateVehicleAsync(int id, Vehicle vehicle);
        Task<bool> DeleteVehicleAsync(int id);
        Task<List<Vehicle>> GetVehiclesByBrandAsync(int brandId);
        Task<List<Vehicle>> GetVehiclesByColorAsync(int colorId);
        Task<List<Vehicle>> GetVehiclesByTypeAsync(int typeId);
    }
}