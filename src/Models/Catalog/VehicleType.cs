using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MicroJack.API.Models.Core;

namespace MicroJack.API.Models.Catalog
{
    public class VehicleType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}