using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class ResidentService : IResidentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResidentService> _logger;

        public ResidentService(ApplicationDbContext context, ILogger<ResidentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Resident>> GetAllResidentsAsync()
        {
            return await _context.Residents
                .Include(r => r.Address)
                .ToListAsync();
        }

        public async Task<Resident?> GetResidentByIdAsync(int id)
        {
            return await _context.Residents
                .Include(r => r.Address)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Resident>> GetResidentsByAddressAsync(int addressId)
        {
            return await _context.Residents
                .Include(r => r.Address)
                .Where(r => r.AddressId == addressId)
                .ToListAsync();
        }

        public async Task<Resident> CreateResidentAsync(Resident resident)
        {
            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();
            return resident;
        }

        public async Task<Resident?> UpdateResidentAsync(int id, Resident resident)
        {
            var existingResident = await _context.Residents.FindAsync(id);
            if (existingResident == null)
                return null;

            existingResident.FullName = resident.FullName;
            existingResident.PhoneExtension = resident.PhoneExtension;
            existingResident.AddressId = resident.AddressId;

            await _context.SaveChangesAsync();
            return existingResident;
        }

        public async Task<bool> DeleteResidentAsync(int id)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null)
                return false;

            _context.Residents.Remove(resident);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}