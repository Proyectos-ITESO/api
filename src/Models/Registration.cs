// Models/Registration.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

// Asegúrate que el namespace coincida con el nombre de tu proyecto
namespace MicroJack.API.Models
{
    public class Registration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")] // Asegura 'id' en minúscula en JSON
        public string Id { get; set; }

        [BsonElement("registrationType")]
        [JsonPropertyName("registrationType")]
        public string RegistrationType { get; set; }

        [BsonElement("house")]
        [BsonRequired]
        [JsonPropertyName("house")]
        public string House { get; set; }

        [BsonElement("visitReason")]
        [BsonRequired]
        [JsonPropertyName("visitReason")]
        public string VisitReason { get; set; }

        [BsonElement("visitorName")]
        [BsonRequired]
        [JsonPropertyName("visitorName")]
        public string VisitorName { get; set; }

        [BsonElement("visitedPerson")]
        [BsonRequired]
        [JsonPropertyName("visitedPerson")]
        public string VisitedPerson { get; set; }

        [BsonElement("guard")]
        [BsonRequired]
        [JsonPropertyName("guard")]
        public string Guard { get; set; }

        [BsonElement("comments")]
        [JsonPropertyName("comments")]
        public string? Comments { get; set; }

        [BsonElement("folio")]
        [JsonPropertyName("folio")]
        public string? Folio { get; set; }

        [BsonElement("entryTimestamp")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [JsonPropertyName("entryTimestamp")]
        public DateTime EntryTimestamp { get; set; }

        [BsonElement("plates")]
        [JsonPropertyName("plates")]
        public string? Plates { get; set; }

        [BsonElement("brand")]
        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [BsonElement("color")]
        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        
        [BsonElement("status")] 
        [JsonPropertyName("status")]
        public string? Status { get; set; } 

        public Registration() {}
    }
}