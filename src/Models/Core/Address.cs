using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Models.Core
{
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; } = string.Empty; // Casa número/dirección

        [Required]
        [MaxLength(10)]
        [JsonPropertyName("extension")]
        public string Extension { get; set; } = string.Empty; // Extensión única de la casa

        [JsonPropertyName("representativeResidentId")]
        public int? RepresentativeResidentId { get; set; } // ID del residente representante

        [MaxLength(50)]
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Resident> Residents { get; set; } = new List<Resident>();
        
        [JsonIgnore]
        public virtual Resident? RepresentativeResident { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();
    }
}