using MicroJack.API.Models.Core;
using MicroJack.API.Models.Enums;
using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Services.Interfaces
{
    public interface ICallService
    {
        // Settings
        Task<TelephonySettings> GetSettingsAsync();
        Task<TelephonySettings> UpdateSettingsAsync(TelephonySettings settings);

        // Calls
        Task<CallRecord> InitiateCallAsync(string toNumber, string? fromExtension, int? requestedByGuardId, int? residentId = null);
        Task<CallRecord?> GetCallByIdAsync(int id);
        Task<List<CallRecord>> GetCallsAsync(CallStatus? status = null, DateTime? from = null, DateTime? to = null);
        Task<CallRecord?> UpdateCallStatusAsync(int id, CallStatus status, string? errorMessage = null);
        Task<bool> DeleteCallAsync(int id);
    }
}

