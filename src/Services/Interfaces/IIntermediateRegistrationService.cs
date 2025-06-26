using MicroJack.API.Models;

namespace MicroJack.API.Services.Interfaces
{
    public interface IIntermediateRegistrationService
    {
        Task<IntermediateRegistration> CreateIntermediateRegistrationAsync(IntermediateRegistration registration);
        Task<IntermediateRegistration?> GetIntermediateRegistrationByTokenAsync(string token);
        Task<bool> ApproveIntermediateRegistrationAsync(string token);
        Task<List<IntermediateRegistration>> GetPendingIntermediateRegistrationsAsync();
    }
}