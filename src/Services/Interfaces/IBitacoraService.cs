using MicroJack.API.Models.Core;

namespace MicroJack.API.Services.Interfaces
{
    public interface IBitacoraService
    {
        Task<List<BitacoraNote>> GetAllNotesAsync();
        Task<BitacoraNote?> GetNoteByIdAsync(int id);
        Task<List<BitacoraNote>> GetNotesByGuardAsync(int guardId);
        Task<List<BitacoraNote>> GetNotesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<BitacoraNote>> GetNotesFilteredAsync(int? guardId, DateTime? startDate, DateTime? endDate);
        Task<BitacoraNote> CreateNoteAsync(BitacoraNote note);
        Task<BitacoraNote?> UpdateNoteAsync(int id, BitacoraNote note);
        Task<bool> DeleteNoteAsync(int id);
        Task<List<BitacoraNote>> SearchNotesAsync(string searchTerm);
    }
}