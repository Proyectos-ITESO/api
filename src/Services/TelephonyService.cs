using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Models.Enums;
using MicroJack.API.Models.Transaction;
using System.Net.Sockets;
using System.Text;

namespace MicroJack.API.Services
{
    public class TelephonyService : Interfaces.ICallService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TelephonyService> _logger;

        public TelephonyService(ApplicationDbContext context, ILogger<TelephonyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TelephonySettings> GetSettingsAsync()
        {
            var settings = await _context.TelephonySettings.FirstOrDefaultAsync(s => s.Id == 1);
            if (settings == null)
            {
                settings = new TelephonySettings
                {
                    Id = 1,
                    Provider = "Simulated",
                    Enabled = false
                };
                _context.TelephonySettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        public async Task<TelephonySettings> UpdateSettingsAsync(TelephonySettings settings)
        {
            var current = await GetSettingsAsync();
            current.Provider = settings.Provider ?? current.Provider;
            current.BaseUrl = settings.BaseUrl;
            current.Username = settings.Username;
            if (!string.IsNullOrEmpty(settings.Password))
            {
                current.Password = settings.Password;
            }
            current.DefaultFromExtension = settings.DefaultFromExtension;
            current.DefaultTrunk = settings.DefaultTrunk;
            current.Enabled = settings.Enabled;
            current.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return current;
        }

        public async Task<CallRecord> InitiateCallAsync(string toNumber, string? fromExtension, int? requestedByGuardId, int? residentId = null)
        {
            var settings = await GetSettingsAsync();
            var now = DateTime.Now;

            var record = new CallRecord
            {
                ToNumber = toNumber,
                FromExtension = fromExtension ?? settings.DefaultFromExtension,
                Direction = CallDirection.Outbound,
                Status = CallStatus.Pending,
                Provider = settings.Provider,
                RequestedByGuardId = requestedByGuardId,
                ResidentId = residentId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.CallRecords.Add(record);
            await _context.SaveChangesAsync();

            if (!settings.Enabled)
            {
                record.Status = CallStatus.Failed;
                record.ErrorMessage = "Telephony provider disabled";
                record.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return record;
            }

            try
            {
                // Basic flow: mark initiated, then attempt provider operation
                record.Status = CallStatus.Initiated;
                record.StartedAt = DateTime.Now;
                record.UpdatedAt = record.StartedAt;
                await _context.SaveChangesAsync();

                // Provider handling
                switch ((settings.Provider ?? "Simulated").Trim())
                {
                    case "Simulated":
                        // Simulate quick call success
                        record.Status = CallStatus.Ringing;
                        record.UpdatedAt = DateTime.Now;
                        await _context.SaveChangesAsync();

                        // Simulate answer and completion
                        record.Status = CallStatus.Completed;
                        record.EndedAt = DateTime.Now;
                        record.UpdatedAt = record.EndedAt;
                        await _context.SaveChangesAsync();
                        break;

                    case "Grandstream":
                        // Try Asterisk AMI originate on UCM
                        var host = ExtractHost(settings.BaseUrl);
                        if (string.IsNullOrWhiteSpace(host))
                        {
                            record.Status = CallStatus.Failed;
                            record.ErrorMessage = "TelephonySettings.BaseUrl (AMI host) is required for Grandstream";
                            record.UpdatedAt = DateTime.Now;
                            await _context.SaveChangesAsync();
                            break;
                        }

                        var port = 5038; // default AMI port
                        string username = settings.Username ?? string.Empty;
                        string secret = settings.Password ?? string.Empty;
                        string from = record.FromExtension ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(from))
                        {
                            record.Status = CallStatus.Failed;
                            record.ErrorMessage = "Missing AMI credentials or fromExtension";
                            record.UpdatedAt = DateTime.Now;
                            await _context.SaveChangesAsync();
                            break;
                        }

                        // Tech preference (SIP or PJSIP) can be set in DefaultTrunk; fallback to SIP
                        var techPref = (settings.DefaultTrunk ?? string.Empty).Trim().ToUpperInvariant();
                        var tech = (techPref == "PJSIP" || techPref == "SIP") ? techPref : "SIP";
                        var context = GetAmiContext();

                        var (ok, actionId, err) = await OriginateViaAmiAsync(host, port, username, secret, from, toNumber, tech, context);
                        if (!ok)
                        {
                            // Try fallback tech once
                            var fallback = tech == "SIP" ? "PJSIP" : "SIP";
                            (ok, actionId, err) = await OriginateViaAmiAsync(host, port, username, secret, from, toNumber, fallback, context);
                        }
                        if (ok)
                        {
                            record.Status = CallStatus.Ringing;
                            record.ExternalId = actionId;
                            record.UpdatedAt = DateTime.Now;
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            record.Status = CallStatus.Failed;
                            record.ErrorMessage = err ?? "AMI originate failed";
                            record.UpdatedAt = DateTime.Now;
                            await _context.SaveChangesAsync();
                        }
                        break;

                    default:
                        _logger.LogWarning("Unknown telephony provider: {Provider}", settings.Provider);
                        record.Status = CallStatus.Failed;
                        record.ErrorMessage = "Unknown provider";
                        record.UpdatedAt = DateTime.Now;
                        await _context.SaveChangesAsync();
                        break;
                }

                return record;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating call to {ToNumber}", toNumber);
                record.Status = CallStatus.Failed;
                record.ErrorMessage = ex.Message;
                record.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return record;
            }
        }

        private async Task<(bool success, string? actionId, string? error)> OriginateViaAmiAsync(string host, int port, string username, string secret, string fromExtension, string toNumber, string tech, string context)
        {
            try
            {
                using var client = new TcpClient();
                client.ReceiveTimeout = 5000;
                client.SendTimeout = 5000;
                await client.ConnectAsync(host, port);

                using var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.ASCII);
                var writer = new StreamWriter(stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };

                // Read AMI banner
                await reader.ReadLineAsync();

                // Login
                await writer.WriteLineAsync("Action: Login");
                await writer.WriteLineAsync($"Username: {username}");
                await writer.WriteLineAsync($"Secret: {secret}");
                await writer.WriteLineAsync();

                var loginResp = await ReadAmiResponseAsync(reader);
                if (!loginResp.Contains("Response: Success"))
                {
                    return (false, null, "AMI login failed");
                }

                var actionId = $"microjack-{Guid.NewGuid():N}";

                // Originate
                await writer.WriteLineAsync("Action: Originate");
                await writer.WriteLineAsync($"ActionID: {actionId}");
                await writer.WriteLineAsync($"Channel: {tech}/{fromExtension}");
                await writer.WriteLineAsync($"Exten: {toNumber}");
                await writer.WriteLineAsync($"Context: {context}");
                await writer.WriteLineAsync("Priority: 1");
                await writer.WriteLineAsync("Async: true");
                await writer.WriteLineAsync($"CallerID: {fromExtension}");
                await writer.WriteLineAsync();

                var origResp = await ReadAmiResponseAsync(reader);
                var ok = origResp.Contains("Response: Success") || origResp.Contains("Message: Originate successfully queued");

                // Logoff best effort
                try
                {
                    await writer.WriteLineAsync("Action: Logoff");
                    await writer.WriteLineAsync();
                }
                catch { }

                return ok ? (true, actionId, null) : (false, null, "AMI originate not accepted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AMI originate error");
                return (false, null, ex.Message);
            }
        }

        private static async Task<string> ReadAmiResponseAsync(StreamReader reader)
        {
            var sb = new StringBuilder();
            var emptyCount = 0;
            for (int i = 0; i < 200; i++)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                    break;
                sb.Append(line).Append('\n');
                if (string.IsNullOrWhiteSpace(line))
                {
                    emptyCount++;
                    if (emptyCount >= 1) // one blank line ends the frame
                        break;
                }
            }
            return sb.ToString();
        }

        private static string GetAmiContext()
        {
            var ctx = Environment.GetEnvironmentVariable("MICROJACK_TEL_AMI_CONTEXT");
            return string.IsNullOrWhiteSpace(ctx) ? "from-internal" : ctx.Trim();
        }

        private static string ExtractHost(string? baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) return string.Empty;
            var s = baseUrl.Trim();
            // Try parse as URI
            if (Uri.TryCreate(s, UriKind.Absolute, out var uri))
            {
                return uri.Host;
            }
            // If contains scheme-less host:port format
            if (s.Contains(":"))
            {
                var parts = s.Split(':');
                return parts[0];
            }
            // Could be host only
            return s;
        }

        public async Task<CallRecord?> GetCallByIdAsync(int id)
        {
            return await _context.CallRecords.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<CallRecord>> GetCallsAsync(CallStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.CallRecords.AsQueryable();
            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }
            if (from.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= to.Value);
            }
            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<CallRecord?> UpdateCallStatusAsync(int id, CallStatus status, string? errorMessage = null)
        {
            var record = await _context.CallRecords.FirstOrDefaultAsync(c => c.Id == id);
            if (record == null)
                return null;

            record.Status = status;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                record.ErrorMessage = errorMessage;
            }
            if (status == CallStatus.Completed || status == CallStatus.Failed || status == CallStatus.Cancelled)
            {
                record.EndedAt = DateTime.Now;
            }
            record.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<bool> DeleteCallAsync(int id)
        {
            var record = await _context.CallRecords.FirstOrDefaultAsync(c => c.Id == id);
            if (record == null)
                return false;

            _context.CallRecords.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
