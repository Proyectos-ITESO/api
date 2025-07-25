using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MicroJack.API.Models.Core
{
    public class PreRegistration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [JsonPropertyName("plates")]
        public string Plates { get; set; } = string.Empty; // Placas del vehículo

        [Required]
        [MaxLength(100)]
        [JsonPropertyName("visitorName")]
        public string VisitorName { get; set; } = string.Empty;

        [MaxLength(50)]
        [JsonPropertyName("vehicleBrand")]
        public string? VehicleBrand { get; set; }

        [MaxLength(30)]
        [JsonPropertyName("vehicleColor")]
        public string? VehicleColor { get; set; }

        [Required]
        [MaxLength(100)]
        [JsonPropertyName("houseVisited")]
        public string HouseVisited { get; set; } = string.Empty; // Casa/dirección

        [Required]
        [JsonPropertyName("expectedArrivalTime")]
        public DateTime ExpectedArrivalTime { get; set; } // Hora esperada de llegada

        [Required]
        [MaxLength(200)]
        [JsonPropertyName("personVisited")]
        public string PersonVisited { get; set; } = string.Empty; // Nombre libre del visitado (no tiene que ser exacto)

        [MaxLength(20)]
        [JsonPropertyName("status")]
        public string Status { get; set; } = "PENDIENTE"; // PENDIENTE, DENTRO, FUERA

        [MaxLength(500)]
        [JsonPropertyName("comments")]
        public string? Comments { get; set; }

        [Required]
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("expiresAt")]
        public DateTime? ExpiresAt { get; set; }

        [MaxLength(50)]
        [JsonPropertyName("createdBy")]
        public string? CreatedBy { get; set; }
    }
}