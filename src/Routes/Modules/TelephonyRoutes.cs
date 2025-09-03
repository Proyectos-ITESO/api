using System.ComponentModel.DataAnnotations;
using MicroJack.API.Middleware;
using MicroJack.API.Models.Core;
using MicroJack.API.Models.Enums;
using MicroJack.API.Models.Transaction;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Routes.Modules
{
    public static class TelephonyRoutes
    {
        public static void MapTelephonyRoutes(this WebApplication app)
        {
            var calls = app.MapGroup("/api/calls").WithTags("Telephony");

            // Create/initiate a call (GuardLevel)
            calls.MapPost("/", async (CreateCallRequest req, ICallService callService, HttpContext ctx) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(req.ToNumber))
                    {
                        return Results.BadRequest(new { success = false, message = "toNumber is required" });
                    }

                    int? guardId = null;
                    var guardIdClaim = ctx.User.FindFirst("GuardId");
                    if (guardIdClaim != null && int.TryParse(guardIdClaim.Value, out var gId))
                    {
                        guardId = gId;
                    }

                    var created = await callService.InitiateCallAsync(req.ToNumber, req.FromExtension, guardId, req.ResidentId);
                    return Results.Created($"/api/calls/{created.Id}", new { success = true, data = created });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error initiating call", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("CreateCall")
            .WithSummary("Initiate an outbound IP call")
            .Produces<object>(201)
            .Produces(400)
            .Produces(500);

            // --- UCM integration helpers (AdminLevel) ---
            calls.MapGet("/ucm/accounts", async (ICallService callService, IUcmClient ucm) =>
            {
                try
                {
                    // Use TelephonySettings for credentials
                    var s = await callService.GetSettingsAsync();
                    if ((s.Provider ?? string.Empty).Trim().Equals("Grandstream", StringComparison.OrdinalIgnoreCase) == false
                        && (s.Provider ?? string.Empty).Trim().Equals("GrandstreamUCM", StringComparison.OrdinalIgnoreCase) == false)
                    {
                        return Results.BadRequest(new { success = false, message = "Provider is not Grandstream/UCM" });
                    }

                    if (string.IsNullOrWhiteSpace(s.BaseUrl) || string.IsNullOrWhiteSpace(s.Username) || string.IsNullOrWhiteSpace(s.Password))
                        return Results.BadRequest(new { success = false, message = "Missing BaseUrl/Username/Password in settings" });

                    var (challenge, err1) = await ucm.GetChallengeAsync(s.BaseUrl!, s.Username!);
                    if (challenge == null)
                        return Results.Problem(title: "Challenge error", detail: err1, statusCode: 500);

                    var (cookie, err2) = await ucm.LoginAsync(s.BaseUrl!, s.Username!, s.Password!, challenge);
                    if (cookie == null)
                        return Results.Problem(title: "Login error", detail: err2, statusCode: 500);

                    var (accounts, err3) = await ucm.ListAccountsAsync(s.BaseUrl!, cookie);
                    if (err3 != null)
                        return Results.Problem(title: "List accounts error", detail: err3, statusCode: 500);

                    var result = accounts.Select(a => new { extension = a.Extension, name = a.Fullname, status = a.Status, type = a.Account_Type }).ToList();
                    return Results.Ok(new { success = true, count = result.Count, data = result });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error fetching UCM accounts", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("ListUcmAccounts")
            .WithSummary("Lista extensiones del UCM mediante API HTTP")
            .Produces<object>(200)
            .Produces(400)
            .Produces(500);

            // List calls (AdminLevel)
            calls.MapGet("/", async (ICallService callService, string? status, DateTime? from, DateTime? to) =>
            {
                try
                {
                    CallStatus? st = null;
                    if (!string.IsNullOrEmpty(status) && Enum.TryParse<CallStatus>(status, true, out var parsed))
                    {
                        st = parsed;
                    }
                    var items = await callService.GetCallsAsync(st, from, to);
                    return Results.Ok(new { success = true, data = items });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error listing calls", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("ListCalls")
            .WithSummary("List call records with optional filters")
            .Produces<object>(200)
            .Produces(500);

            // Get call by id (GuardLevel)
            calls.MapGet("/{id:int}", async (int id, ICallService callService) =>
            {
                try
                {
                    var item = await callService.GetCallByIdAsync(id);
                    if (item == null)
                        return Results.NotFound(new { success = false, message = "Call not found" });

                    return Results.Ok(new { success = true, data = item });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting call", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("GetCall")
            .Produces<object>(200)
            .Produces(404)
            .Produces(500);

            // Update call status (AdminLevel) - e.g., cancel
            calls.MapPatch("/{id:int}/status", async (int id, UpdateCallStatusRequest req, ICallService callService) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(req.Status) || !Enum.TryParse<CallStatus>(req.Status, true, out var st))
                    {
                        return Results.BadRequest(new { success = false, message = "Invalid status" });
                    }
                    var updated = await callService.UpdateCallStatusAsync(id, st, req.ErrorMessage);
                    if (updated == null)
                        return Results.NotFound(new { success = false, message = "Call not found" });

                    return Results.Ok(new { success = true, data = updated });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error updating call status", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("UpdateCallStatus")
            .WithSummary("Update call status (cancel/complete/fail)")
            .Produces<object>(200)
            .Produces(400)
            .Produces(404)
            .Produces(500);

            // Delete call (SuperAdminLevel)
            calls.MapDelete("/{id:int}", async (int id, ICallService callService) =>
            {
                try
                {
                    var ok = await callService.DeleteCallAsync(id);
                    if (!ok) return Results.NotFound(new { success = false, message = "Call not found" });
                    return Results.Ok(new { success = true, message = "Call deleted" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error deleting call", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("SuperAdminLevel")
            .WithName("DeleteCall")
            .Produces<object>(200)
            .Produces(404)
            .Produces(500);

            // Settings endpoints (AdminLevel)
            calls.MapGet("/settings", async (ICallService callService) =>
            {
                try
                {
                    var s = await callService.GetSettingsAsync();
                    // Redact password in response
                    return Results.Ok(new
                    {
                        success = true,
                        data = new
                        {
                            id = s.Id,
                            provider = s.Provider,
                            baseUrl = s.BaseUrl,
                            username = s.Username,
                            password = string.IsNullOrEmpty(s.Password) ? null : "***",
                            defaultFromExtension = s.DefaultFromExtension,
                            defaultTrunk = s.DefaultTrunk,
                            enabled = s.Enabled,
                            updatedAt = s.UpdatedAt
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting telephony settings", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("GetTelephonySettings")
            .Produces<object>(200)
            .Produces(500);

            calls.MapPut("/settings", async (UpdateTelephonySettingsRequest req, ICallService callService) =>
            {
                try
                {
                    var updated = await callService.UpdateSettingsAsync(new TelephonySettings
                    {
                        Id = 1,
                        Provider = req.Provider ?? "Simulated",
                        BaseUrl = req.BaseUrl,
                        Username = req.Username,
                        Password = req.Password,
                        DefaultFromExtension = req.DefaultFromExtension,
                        DefaultTrunk = req.DefaultTrunk,
                        Enabled = req.Enabled
                    });
                    return Results.Ok(new { success = true, data = new { updated.Provider, updated.BaseUrl, updated.Username, updated.DefaultFromExtension, updated.DefaultTrunk, updated.Enabled, updated.UpdatedAt } });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error updating telephony settings", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("UpdateTelephonySettings")
            .Produces<object>(200)
            .Produces(500);
        }
    }

    // DTOs
    public class CreateCallRequest
    {
        [Required]
        public string ToNumber { get; set; } = string.Empty;
        public string? FromExtension { get; set; }
        public int? ResidentId { get; set; }
    }

    public class UpdateCallStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // CallStatus as string
        public string? ErrorMessage { get; set; }
    }

    public class UpdateTelephonySettingsRequest
    {
        public string? Provider { get; set; }
        public string? BaseUrl { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? DefaultFromExtension { get; set; }
        public string? DefaultTrunk { get; set; }
        public bool Enabled { get; set; } = false;
    }
}
