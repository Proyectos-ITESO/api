using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MicroJack.API.Models.Core
{
    public class BitacoraNote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("note")]
        public string Note { get; set; } = string.Empty; // Texto largo o corto de la nota

        [Required]
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [JsonPropertyName("guardId")]
        public int GuardId { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Guard Guard { get; set; } = null!;
    }
}