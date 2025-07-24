using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public abstract class BaseCatalogService<T> : ICatalogService<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger<BaseCatalogService<T>> _logger;
        protected readonly DbSet<T> _dbSet;

        protected BaseCatalogService(ApplicationDbContext context, ILogger<BaseCatalogService<T>> logger)
        {
            _context = context;
            _logger = logger;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all {EntityType}", typeof(T).Name);
                return new List<T>();
            }
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityType} by ID {Id}", typeof(T).Name, id);
                return null;
            }
        }

        public abstract Task<List<T>> SearchAsync(string searchTerm);

        public virtual async Task<T> CreateAsync(T entity)
        {
            try
            {
                await ValidateEntityAsync(entity);
                
                _dbSet.Add(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("{EntityType} created successfully", typeof(T).Name);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T?> UpdateAsync(int id, T entity)
        {
            try
            {
                var existingEntity = await _dbSet.FindAsync(id);
                if (existingEntity == null)
                {
                    return null;
                }

                await ValidateEntityAsync(entity, id);
                UpdateEntityProperties(existingEntity, entity);

                await _context.SaveChangesAsync();
                _logger.LogInformation("{EntityType} updated successfully: {Id}", typeof(T).Name, id);
                return existingEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {EntityType} {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                {
                    return false;
                }

                await ValidateDeleteAsync(entity, id);

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("{EntityType} deleted successfully: {Id}", typeof(T).Name, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType} {Id}", typeof(T).Name, id);
                throw;
            }
        }

        protected abstract Task ValidateEntityAsync(T entity, int? excludeId = null);
        protected abstract void UpdateEntityProperties(T existingEntity, T newEntity);
        protected abstract Task ValidateDeleteAsync(T entity, int id);
    }
}