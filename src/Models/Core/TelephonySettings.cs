using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MicroJack.API.Models.Core
{
    public class TelephonySettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [JsonPropertyName("id")]
        public int Id { get; set; } = 1; // Singleton row

        [Required]
        [MaxLength(50)]
        [JsonPropertyName("provider")]
        public string Provider { get; set; } = "Simulated"; // Grandstream | Simulated | Other

        [MaxLength(200)]
        [JsonPropertyName("baseUrl")]
        public string? BaseUrl { get; set; }

        [MaxLength(100)]
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [MaxLength(200)]
        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [MaxLength(50)]
        [JsonPropertyName("defaultFromExtension")]
        public string? DefaultFromExtension { get; set; }

        [MaxLength(100)]
        [JsonPropertyName("defaultTrunk")]
        public string? DefaultTrunk { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}

