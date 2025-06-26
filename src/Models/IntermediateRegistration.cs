using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

namespace MicroJack.API.Models
{
    public class IntermediateRegistration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [BsonElement("plates")]
        [BsonRequired]
        [JsonPropertyName("plates")]
        public string Plates { get; set; }

        [BsonElement("visitorName")]
        [JsonPropertyName("visitorName")]
        public string? VisitorName { get; set; }

        [BsonElement("brand")]
        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [BsonElement("color")]
        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [BsonElement("cotoId")]
        [JsonPropertyName("cotoId")]
        public int CotoId { get; set; }

        [BsonElement("cotoName")]
        [JsonPropertyName("cotoName")]
        public string CotoName { get; set; }

        [BsonElement("houseNumber")]
        [JsonPropertyName("houseNumber")]
        public string HouseNumber { get; set; }

        [BsonElement("housePhone")]
        [JsonPropertyName("housePhone")]
        public string HousePhone { get; set; }

        [BsonElement("arrivalDateTime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [JsonPropertyName("arrivalDateTime")]
        public DateTime? ArrivalDateTime { get; set; }

        [BsonElement("personVisited")]
        [JsonPropertyName("personVisited")]
        public string? PersonVisited { get; set; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = "AWAITING_APPROVAL";

        [BsonElement("whatsappSent")]
        [JsonPropertyName("whatsappSent")]
        public bool WhatsappSent { get; set; } = false;

        [BsonElement("whatsappSentAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [JsonPropertyName("whatsappSentAt")]
        public DateTime? WhatsappSentAt { get; set; }

        [BsonElement("approvalToken")]
        [JsonPropertyName("approvalToken")]
        public string? ApprovalToken { get; set; }

        [BsonElement("approvedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [JsonPropertyName("approvedAt")]
        public DateTime? ApprovedAt { get; set; }

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}