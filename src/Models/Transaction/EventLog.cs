using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MicroJack.API.Models.Core;

namespace MicroJack.API.Models.Transaction
{
    public class EventLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [Required]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("guardId")]
        public int GuardId { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Guard Guard { get; set; } = null!;

        public EventLog()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}