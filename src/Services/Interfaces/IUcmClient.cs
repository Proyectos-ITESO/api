using MicroJack.API.Models.Ucm;

namespace MicroJack.API.Services.Interfaces;

public interface IUcmClient
{
    Task<(string? challenge, string? error)> GetChallengeAsync(string baseUrl, string username, CancellationToken ct = default);
    Task<(string? cookie, string? error)> LoginAsync(string baseUrl, string username, string password, string challenge, CancellationToken ct = default);
    Task<(List<UcmAccount> accounts, string? error)> ListAccountsAsync(string baseUrl, string cookie, CancellationToken ct = default);
}

