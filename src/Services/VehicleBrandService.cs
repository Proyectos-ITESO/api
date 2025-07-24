using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Catalog;

namespace MicroJack.API.Services
{
    public class VehicleBrandService : BaseCatalogService<VehicleBrand>
    {
        public VehicleBrandService(ApplicationDbContext context, ILogger<BaseCatalogService<VehicleBrand>> logger)
            : base(context, logger)
        {
        }

        public override async Task<List<VehicleBrand>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllAsync();
                }

                return await _dbSet
                    .Where(b => b.Name.Contains(searchTerm))
                    .OrderBy(b => b.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching vehicle brands with term {SearchTerm}", searchTerm);
                return new List<VehicleBrand>();
            }
        }

        protected override async Task ValidateEntityAsync(VehicleBrand entity, int? excludeId = null)
        {
            var query = _dbSet.Where(b => b.Name == entity.Name);
            if (excludeId.HasValue)
            {
                query = query.Where(b => b.Id != excludeId.Value);
            }

            var existingBrand = await query.FirstOrDefaultAsync();
            if (existingBrand != null)
            {
                throw new ApplicationException($"Vehicle brand '{entity.Name}' already exists");
            }
        }

        protected override void UpdateEntityProperties(VehicleBrand existingEntity, VehicleBrand newEntity)
        {
            existingEntity.Name = newEntity.Name;
        }

        protected override async Task ValidateDeleteAsync(VehicleBrand entity, int id)
        {
            var hasVehicles = await _context.Vehicles.AnyAsync(v => v.BrandId == id);
            if (hasVehicles)
            {
                throw new ApplicationException("Cannot delete vehicle brand that is associated with vehicles");
            }
        }
    }
}