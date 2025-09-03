using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MicroJack.API.Models.Core;
using MicroJack.API.Models.Enums;

namespace MicroJack.API.Models.Transaction
{
    public class CallRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("toNumber")]
        public string ToNumber { get; set; } = string.Empty;

        [JsonPropertyName("fromExtension")]
        public string? FromExtension { get; set; }

        [Required]
        [JsonPropertyName("direction")]
        public CallDirection Direction { get; set; } = CallDirection.Outbound;

        [Required]
        [JsonPropertyName("status")]
        public CallStatus Status { get; set; } = CallStatus.Pending;

        [JsonPropertyName("provider")]
        public string? Provider { get; set; }

        [JsonPropertyName("externalId")]
        public string? ExternalId { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("requestedByGuardId")]
        public int? RequestedByGuardId { get; set; }

        [JsonPropertyName("residentId")]
        public int? ResidentId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("startedAt")]
        public DateTime? StartedAt { get; set; }

        [JsonPropertyName("endedAt")]
        public DateTime? EndedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Guard? RequestedByGuard { get; set; }

        [JsonIgnore]
        public virtual Resident? Resident { get; set; }

        public CallRecord()
        {
            CreatedAt = DateTime.Now;
        }
    }
}

