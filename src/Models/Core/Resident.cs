using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Models.Core
{
    public class Resident
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
        [MaxLength(15)]
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty; // Teléfono real del residente

        [Required]
        [JsonPropertyName("addressId")]
        public int AddressId { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Address Address { get; set; } = null!;
        
        [JsonIgnore]
        public virtual ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();

        [JsonIgnore]
        public virtual ICollection<CallRecord> CallRecords { get; set; } = new List<CallRecord>();
    }
}
