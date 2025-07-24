using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MicroJack.API.Models.Catalog;
using MicroJack.API.Models.Transaction;

namespace MicroJack.API.Models.Core
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        [JsonPropertyName("licensePlate")]
        public string LicensePlate { get; set; } = string.Empty;

        [MaxLength(500)]
        [JsonPropertyName("plateImageUrl")]
        public string? PlateImageUrl { get; set; }

        [JsonPropertyName("brandId")]
        public int? BrandId { get; set; }

        [JsonPropertyName("colorId")]
        public int? ColorId { get; set; }

        [JsonPropertyName("typeId")]
        public int? TypeId { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual VehicleBrand? Brand { get; set; }
        
        [JsonIgnore]
        public virtual VehicleColor? Color { get; set; }
        
        [JsonIgnore]
        public virtual VehicleType? Type { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();
    }
}