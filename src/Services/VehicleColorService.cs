using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Catalog;

namespace MicroJack.API.Services
{
    public class VehicleColorService : BaseCatalogService<VehicleColor>
    {
        public VehicleColorService(ApplicationDbContext context, ILogger<BaseCatalogService<VehicleColor>> logger)
            : base(context, logger)
        {
        }

        public override async Task<List<VehicleColor>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllAsync();
                }

                return await _dbSet
                    .Where(c => c.Name.Contains(searchTerm))
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching vehicle colors with term {SearchTerm}", searchTerm);
                return new List<VehicleColor>();
            }
        }

        protected override async Task ValidateEntityAsync(VehicleColor entity, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.Name == entity.Name);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            var existingColor = await query.FirstOrDefaultAsync();
            if (existingColor != null)
            {
                throw new ApplicationException($"Vehicle color '{entity.Name}' already exists");
            }
        }

        protected override void UpdateEntityProperties(VehicleColor existingEntity, VehicleColor newEntity)
        {
            existingEntity.Name = newEntity.Name;
        }

        protected override async Task ValidateDeleteAsync(VehicleColor entity, int id)
        {
            var hasVehicles = await _context.Vehicles.AnyAsync(v => v.ColorId == id);
            if (hasVehicles)
            {
                throw new ApplicationException("Cannot delete vehicle color that is associated with vehicles");
            }
        }
    }
}