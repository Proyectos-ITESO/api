using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MicroJack.API.Models
{
    public class Registration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("registrationType")]
        public string RegistrationType { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("house")]
        public string House { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("visitReason")]
        public string VisitReason { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("visitorName")]
        public string VisitorName { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("visitedPerson")]
        public string VisitedPerson { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("guard")]
        public string Guard { get; set; } = string.Empty;

        [JsonPropertyName("comments")]
        public string? Comments { get; set; }

        [JsonPropertyName("folio")]
        public string? Folio { get; set; }

        [JsonPropertyName("entryTimestamp")]
        public DateTime EntryTimestamp { get; set; }

        [JsonPropertyName("plates")]
        public string? Plates { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        public Registration() 
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            EntryTimestamp = DateTime.UtcNow;
        }
    }
}