using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MicroJack.API.Models.Core
{
    public class GuardRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("guardId")]
        public int GuardId { get; set; }

        [Required]
        [JsonPropertyName("roleId")]
        public int RoleId { get; set; }

        [JsonPropertyName("assignedAt")]
        public DateTime AssignedAt { get; set; }

        [JsonPropertyName("assignedBy")]
        public int? AssignedBy { get; set; } // ID del admin que asignó el rol

        // Navigation properties
        [JsonIgnore]
        public virtual Guard Guard { get; set; } = null!;
        
        [JsonIgnore]
        public virtual Role Role { get; set; } = null!;

        public GuardRole()
        {
            AssignedAt = DateTime.UtcNow;
        }
    }
}