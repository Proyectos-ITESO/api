using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BitacoraService> _logger;

        public BitacoraService(ApplicationDbContext context, ILogger<BitacoraService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BitacoraNote>> GetAllNotesAsync()
        {
            try
            {
                return await _context.BitacoraNotes
                    .Include(b => b.Guard)
                    .OrderByDescending(b => b.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all bitacora notes");
                throw;
            }
        }

        public async Task<BitacoraNote?> GetNoteByIdAsync(int id)
        {
            try
            {
                return await _context.BitacoraNotes
                    .Include(b => b.Guard)
                    .FirstOrDefaultAsync(b => b.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bitacora note by ID: {Id}", id);
                throw;
            }
        }

        public async Task<List<BitacoraNote>> GetNotesByGuardAsync(int guardId)
        {
            try
            {
                return await _context.BitacoraNotes
                    .Include(b => b.Guard)
                    .Where(b => b.GuardId == guardId)
                    .OrderByDescending(b => b.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bitacora notes by guard ID: {GuardId}", guardId);
                throw;
            }
        }

        public async Task<List<BitacoraNote>> GetNotesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.BitacoraNotes
                    .Include(b => b.Guard)
                    .Where(b => b.Timestamp >= startDate && b.Timestamp <= endDate)
                    .OrderByDescending(b => b.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bitacora notes by date range: {Start} - {End}", startDate, endDate);
                throw;
            }
        }

        public async Task<List<BitacoraNote>> GetNotesFilteredAsync(int? guardId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.BitacoraNotes
                    .Include(b => b.Guard)
                    .AsQueryable();

                if (guardId.HasValue)
                {
                    query = query.Where(b => b.GuardId == guardId.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(b => b.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(b => b.Timestamp <= endDate.Value);
                }

                return await query
                    .OrderByDescending(b => b.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered bitacora notes");
                throw;
            }
        }

        public async Task<BitacoraNote> CreateNoteAsync(BitacoraNote note)
        {
            try
            {
                note.Timestamp = DateTime.UtcNow;

                _context.BitacoraNotes.Add(note);
                await _context.SaveChangesAsync();

                // Reload with Guard information
                await _context.Entry(note)
                    .Reference(b => b.Guard)
                    .LoadAsync();

                _logger.LogInformation("Created bitacora note by guard ID: {GuardId}", note.GuardId);
                return note;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bitacora note");
                throw;
            }
        }

        public async Task<BitacoraNote?> UpdateNoteAsync(int id, BitacoraNote note)
        {
            try
            {
                var existing = await _context.BitacoraNotes.FindAsync(id);
                if (existing == null)
                    return null;

                existing.Note = note.Note;
                // No permitir cambiar GuardId o Timestamp en updates

                await _context.SaveChangesAsync();

                // Reload with Guard information
                await _context.Entry(existing)
                    .Reference(b => b.Guard)
                    .LoadAsync();

                _logger.LogInformation("Updated bitacora note ID: {Id}", id);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bitacora note ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteNoteAsync(int id)
        {
            try
            {
                var note = await _context.BitacoraNotes.FindAsync(id);
                if (note == null)
                    return false;

                _context.BitacoraNotes.Remove(note);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted bitacora note ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bitacora note ID: {Id}", id);
                throw;
            }
        }

        public async Task<List<BitacoraNote>> SearchNotesAsync(string searchTerm)
        {
            try
            {
                return await _context.BitacoraNotes
                    .Include(b => b.Guard)
                    .Where(b => b.Note.Contains(searchTerm) ||
                               b.Guard.Username.Contains(searchTerm))
                    .OrderByDescending(b => b.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching bitacora notes with term: {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
}