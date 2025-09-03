using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MicroJack.API.Models.Ucm;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services;

public class UcmClient : IUcmClient
{
    private readonly HttpClient _http;
    private readonly ILogger<UcmClient> _logger;

    public UcmClient(HttpClient http, ILogger<UcmClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<(string? challenge, string? error)> GetChallengeAsync(string baseUrl, string username, CancellationToken ct = default)
    {
        try
        {
            var payload = new UcmApiEnvelope<UcmApiRequest>
            {
                Request = new UcmApiRequest
                {
                    Action = "challenge",
                    User = username,
                    Version = "1.0"
                }
            };

            using var resp = await _http.PostAsJsonAsync(NormalizeBaseUrl(baseUrl), payload, cancellationToken: ct);
            resp.EnsureSuccessStatusCode();

            var doc = await resp.Content.ReadFromJsonAsync<UcmApiResponseEnvelope<UcmChallengeResponse>>(cancellationToken: ct);
            var challenge = doc?.Response?.Challenge;
            if (string.IsNullOrWhiteSpace(challenge))
                return (null, "Empty challenge");

            return (challenge, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UCM challenge error");
            return (null, ex.Message);
        }
    }

    public async Task<(string? cookie, string? error)> LoginAsync(string baseUrl, string username, string password, string challenge, CancellationToken ct = default)
    {
        try
        {
            var token = ComputeMd5(challenge + password);

            var payload = new UcmApiEnvelope<UcmApiRequest>
            {
                Request = new UcmApiRequest
                {
                    Action = "login",
                    User = username,
                    Token = token
                }
            };

            using var resp = await _http.PostAsJsonAsync(NormalizeBaseUrl(baseUrl), payload, cancellationToken: ct);
            resp.EnsureSuccessStatusCode();

            var doc = await resp.Content.ReadFromJsonAsync<UcmApiResponseEnvelope<UcmLoginResponse>>(cancellationToken: ct);
            var cookie = doc?.Response?.Cookie;
            if (string.IsNullOrWhiteSpace(cookie))
                return (null, "Empty cookie");

            return (cookie, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UCM login error");
            return (null, ex.Message);
        }
    }

    public async Task<(List<UcmAccount> accounts, string? error)> ListAccountsAsync(string baseUrl, string cookie, CancellationToken ct = default)
    {
        try
        {
            var payload = new UcmApiEnvelope<UcmApiRequest>
            {
                Request = new UcmApiRequest
                {
                    Action = "listAccount",
                    Cookie = cookie,
                    Options = "extension,fullname,status,account_type"
                }
            };

            using var resp = await _http.PostAsJsonAsync(NormalizeBaseUrl(baseUrl), payload, cancellationToken: ct);
            resp.EnsureSuccessStatusCode();

            var doc = await resp.Content.ReadFromJsonAsync<UcmApiResponseEnvelope<UcmListAccountResponse>>(cancellationToken: ct);
            var list = doc?.Response?.Account ?? new List<UcmAccount>();
            return (list, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UCM listAccount error");
            return (new List<UcmAccount>(), ex.Message);
        }
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        // Accept forms like "https://host:8089/api" or "host:8089" and normalize to full URL
        var url = baseUrl.Trim();
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            // default to https with /api path if missing scheme
            url = $"https://{url}";
        }
        if (!url.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            if (url.EndsWith("/")) url = url.TrimEnd('/');
            url += "/api";
        }
        return url;
    }

    private static string ComputeMd5(string input)
    {
        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = md5.ComputeHash(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}

