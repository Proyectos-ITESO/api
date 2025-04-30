// Services/Interfaces/IPhidgetService.cs
namespace MicroJack.API.Services.Interfaces
{
    public interface IPhidgetService
    {
        bool IsInitialized { get; }
        Task<bool> InitializeAsync();
        Task<bool> SetRelayStateAsync(int channel, bool state);
        bool? GetRelayState(int channel);
        Dictionary<int, bool> GetAllRelayStates();
        void Close();
    }
}