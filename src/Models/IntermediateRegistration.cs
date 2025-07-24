using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MicroJack.API.Models
{
    public class IntermediateRegistration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("plates")]
        public string Plates { get; set; } = string.Empty;

        [JsonPropertyName("visitorName")]
        public string? VisitorName { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("cotoId")]
        public string CotoId { get; set; } = string.Empty;

        [JsonPropertyName("cotoName")]
        public string CotoName { get; set; } = string.Empty;

        [JsonPropertyName("houseNumber")]
        public string HouseNumber { get; set; } = string.Empty;

        [JsonPropertyName("housePhone")]
        public string HousePhone { get; set; } = string.Empty;

        [JsonPropertyName("arrivalDateTime")]
        public DateTime? ArrivalDateTime { get; set; }

        [JsonPropertyName("personVisited")]
        public string? PersonVisited { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "AWAITING_APPROVAL";

        [JsonPropertyName("whatsappSent")]
        public bool WhatsappSent { get; set; } = false;

        [JsonPropertyName("whatsappSentAt")]
        public DateTime? WhatsappSentAt { get; set; }

        [JsonPropertyName("approvalToken")]
        public string? ApprovalToken { get; set; }

        [JsonPropertyName("approvedAt")]
        public DateTime? ApprovedAt { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        public IntermediateRegistration()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}