using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MicroJack.API.Models
{
    public class PreRegistration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("plates")]
        public string Plates { get; set; } = string.Empty;

        [JsonPropertyName("visitorName")]
        public string? VisitorName { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("houseVisited")]
        public string? HouseVisited { get; set; }

        [JsonPropertyName("arrivalDateTime")]
        public DateTime? ArrivalDateTime { get; set; }

        [JsonPropertyName("personVisited")]
        public string? PersonVisited { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "PENDIENTE";

        [JsonPropertyName("createdBy")]
        public string? CreatedBy { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        public PreRegistration()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}