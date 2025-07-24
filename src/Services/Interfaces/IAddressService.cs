using MicroJack.API.Models.Core;

namespace MicroJack.API.Services.Interfaces
{
    public interface IAddressService
    {
        Task<List<Address>> GetAllAddressesAsync();
        Task<Address?> GetAddressByIdAsync(int id);
        Task<List<Address>> SearchAddressesAsync(string searchTerm);
        Task<Address> CreateAddressAsync(Address address);
        Task<Address?> UpdateAddressAsync(int id, Address address);
        Task<bool> DeleteAddressAsync(int id);
        Task<List<Address>> GetAddressesByStatusAsync(string status);
    }
}