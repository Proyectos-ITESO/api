using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class VisitorService : IVisitorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VisitorService> _logger;

        public VisitorService(ApplicationDbContext context, ILogger<VisitorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Visitor>> GetAllVisitorsAsync()
        {
            try
            {
                return await _context.Visitors
                    .OrderBy(v => v.FullName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all visitors");
                return new List<Visitor>();
            }
        }

        public async Task<Visitor?> GetVisitorByIdAsync(int id)
        {
            try
            {
                return await _context.Visitors.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visitor by ID {Id}", id);
                return null;
            }
        }

        public async Task<List<Visitor>> SearchVisitorsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllVisitorsAsync();
                }

                return await _context.Visitors
                    .Where(v => v.FullName.Contains(searchTerm))
                    .OrderBy(v => v.FullName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching visitors with term {SearchTerm}", searchTerm);
                return new List<Visitor>();
            }
        }

        public async Task<Visitor> CreateVisitorAsync(Visitor visitor)
        {
            try
            {
                _context.Visitors.Add(visitor);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Visitor created successfully: {Name}", visitor.FullName);
                return visitor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating visitor");
                throw;
            }
        }

        public async Task<Visitor?> UpdateVisitorAsync(int id, Visitor visitor)
        {
            try
            {
                var existingVisitor = await _context.Visitors.FindAsync(id);
                if (existingVisitor == null)
                {
                    return null;
                }

                existingVisitor.FullName = visitor.FullName;
                existingVisitor.IneImageUrl = visitor.IneImageUrl;
                existingVisitor.FaceImageUrl = visitor.FaceImageUrl;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Visitor updated successfully: {Id}", id);
                return existingVisitor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating visitor {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteVisitorAsync(int id)
        {
            try
            {
                var visitor = await _context.Visitors.FindAsync(id);
                if (visitor == null)
                {
                    return false;
                }

                // Check if visitor has associated access logs
                var hasAccessLogs = await _context.AccessLogs
                    .AnyAsync(al => al.VisitorId == id);

                if (hasAccessLogs)
                {
                    throw new ApplicationException("Cannot delete visitor with associated access logs");
                }

                _context.Visitors.Remove(visitor);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Visitor deleted successfully: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting visitor {Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateVisitorImagesAsync(int id, string? ineImageUrl, string? faceImageUrl)
        {
            try
            {
                var visitor = await _context.Visitors.FindAsync(id);
                if (visitor == null)
                {
                    return false;
                }

                visitor.IneImageUrl = ineImageUrl;
                visitor.FaceImageUrl = faceImageUrl;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Visitor images updated: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating visitor images {Id}", id);
                return false;
            }
        }
    }
}