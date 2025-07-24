using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MicroJack.API.Models.Core
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("permissions")]
        public string Permissions { get; set; } = string.Empty; // JSON string of permissions

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<GuardRole> GuardRoles { get; set; } = new List<GuardRole>();
    }
}