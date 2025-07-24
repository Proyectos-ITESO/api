using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MicroJack.API.Models.Core;
using MicroJack.API.Models.Catalog;

namespace MicroJack.API.Models.Transaction
{
    public class AccessLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; } // Este es el FOLIO

        [Required]
        [JsonPropertyName("entryTimestamp")]
        public DateTime EntryTimestamp { get; set; }

        [JsonPropertyName("exitTimestamp")]
        public DateTime? ExitTimestamp { get; set; }

        [Required]
        [MaxLength(20)]
        [JsonPropertyName("status")]
        public string Status { get; set; } = "DENTRO"; // DENTRO, FUERA, PRE-REGISTRO

        [JsonPropertyName("comments")]
        public string? Comments { get; set; }

        [Required]
        [JsonPropertyName("visitorId")]
        public int VisitorId { get; set; }

        [JsonPropertyName("vehicleId")]
        public int? VehicleId { get; set; } // Nullable para peatones

        [Required]
        [JsonPropertyName("addressId")]
        public int AddressId { get; set; }

        [JsonPropertyName("residentVisitedId")]
        public int? ResidentVisitedId { get; set; }

        [Required]
        [JsonPropertyName("entryGuardId")]
        public int EntryGuardId { get; set; }

        [JsonPropertyName("exitGuardId")]
        public int? ExitGuardId { get; set; }

        [JsonPropertyName("visitReasonId")]
        public int? VisitReasonId { get; set; }

        [MaxLength(20)]
        [JsonPropertyName("gafeteNumber")]
        public string? GafeteNumber { get; set; } // Para peatones

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Visitor Visitor { get; set; } = null!;
        
        [JsonIgnore]
        public virtual Vehicle? Vehicle { get; set; }
        
        [JsonIgnore]
        public virtual Address Address { get; set; } = null!;
        
        [JsonIgnore]
        public virtual Resident? ResidentVisited { get; set; }
        
        [JsonIgnore]
        public virtual Guard EntryGuard { get; set; } = null!;
        
        [JsonIgnore]
        public virtual Guard? ExitGuard { get; set; }
        
        [JsonIgnore]
        public virtual VisitReason? VisitReason { get; set; }

        public AccessLog()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            EntryTimestamp = DateTime.UtcNow;
        }
    }
}