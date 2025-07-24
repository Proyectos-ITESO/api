using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Catalog;

namespace MicroJack.API.Services
{
    public class VisitReasonService : BaseCatalogService<VisitReason>
    {
        public VisitReasonService(ApplicationDbContext context, ILogger<BaseCatalogService<VisitReason>> logger)
            : base(context, logger)
        {
        }

        public override async Task<List<VisitReason>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllAsync();
                }

                return await _dbSet
                    .Where(r => r.Reason.Contains(searchTerm))
                    .OrderBy(r => r.Reason)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching visit reasons with term {SearchTerm}", searchTerm);
                return new List<VisitReason>();
            }
        }

        protected override async Task ValidateEntityAsync(VisitReason entity, int? excludeId = null)
        {
            var query = _dbSet.Where(r => r.Reason == entity.Reason);
            if (excludeId.HasValue)
            {
                query = query.Where(r => r.Id != excludeId.Value);
            }

            var existingReason = await query.FirstOrDefaultAsync();
            if (existingReason != null)
            {
                throw new ApplicationException($"Visit reason '{entity.Reason}' already exists");
            }
        }

        protected override void UpdateEntityProperties(VisitReason existingEntity, VisitReason newEntity)
        {
            existingEntity.Reason = newEntity.Reason;
        }

        protected override async Task ValidateDeleteAsync(VisitReason entity, int id)
        {
            var hasAccessLogs = await _context.AccessLogs.AnyAsync(al => al.VisitReasonId == id);
            if (hasAccessLogs)
            {
                throw new ApplicationException("Cannot delete visit reason that is associated with access logs");
            }
        }
    }
}