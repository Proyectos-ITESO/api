using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Models.Core
{
    public class Visitor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(500)]
        [JsonPropertyName("ineImageUrl")]
        public string? IneImageUrl { get; set; }

        [MaxLength(500)]
        [JsonPropertyName("faceImageUrl")]
        public string? FaceImageUrl { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();
    }
}