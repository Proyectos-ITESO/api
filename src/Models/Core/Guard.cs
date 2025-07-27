using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Models.Core
{
    public class Guard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [JsonPropertyName("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("lastLogin")]
        public DateTime? LastLogin { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<GuardRole> GuardRoles { get; set; } = new List<GuardRole>();
        
        [JsonIgnore]
        public virtual ICollection<AccessLog> EntryLogs { get; set; } = new List<AccessLog>();
        
        [JsonIgnore]
        public virtual ICollection<AccessLog> ExitLogs { get; set; } = new List<AccessLog>();
        
        [JsonIgnore]
        public virtual ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();

        // Helper properties
        [NotMapped]
        [JsonPropertyName("roles")]
        public List<string> RoleNames => GuardRoles?.Select(gr => gr.Role.Name).ToList() ?? new List<string>();

        [NotMapped]
        [JsonPropertyName("isAdmin")]
        public bool IsAdmin => RoleNames.Contains("Admin") || RoleNames.Contains("SuperAdmin");

        public Guard()
        {
            CreatedAt = DateTime.Now; // Usar hora local de la máquina
        }
    }
}