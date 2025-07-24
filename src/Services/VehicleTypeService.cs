using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Catalog;

namespace MicroJack.API.Services
{
    public class VehicleTypeService : BaseCatalogService<VehicleType>
    {
        public VehicleTypeService(ApplicationDbContext context, ILogger<BaseCatalogService<VehicleType>> logger)
            : base(context, logger)
        {
        }

        public override async Task<List<VehicleType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllAsync();
                }

                return await _dbSet
                    .Where(t => t.Name.Contains(searchTerm))
                    .OrderBy(t => t.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching vehicle types with term {SearchTerm}", searchTerm);
                return new List<VehicleType>();
            }
        }

        protected override async Task ValidateEntityAsync(VehicleType entity, int? excludeId = null)
        {
            var query = _dbSet.Where(t => t.Name == entity.Name);
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            var existingType = await query.FirstOrDefaultAsync();
            if (existingType != null)
            {
                throw new ApplicationException($"Vehicle type '{entity.Name}' already exists");
            }
        }

        protected override void UpdateEntityProperties(VehicleType existingEntity, VehicleType newEntity)
        {
            existingEntity.Name = newEntity.Name;
        }

        protected override async Task ValidateDeleteAsync(VehicleType entity, int id)
        {
            var hasVehicles = await _context.Vehicles.AnyAsync(v => v.TypeId == id);
            if (hasVehicles)
            {
                throw new ApplicationException("Cannot delete vehicle type that is associated with vehicles");
            }
        }
    }
}