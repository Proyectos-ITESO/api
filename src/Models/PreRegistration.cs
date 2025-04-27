// Models/PreRegistration.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

namespace MicroJack.API.Models
{
    public class PreRegistration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [BsonElement("plates")]
        [BsonRequired]
        [JsonPropertyName("plates")]
        public string Plates { get; set; } // Placas son clave aquí

        [BsonElement("visitorName")]
        [JsonPropertyName("visitorName")]
        public string? VisitorName { get; set; } // Nombre del visitante pre-registrado

        [BsonElement("brand")]
        [JsonPropertyName("brand")]
        public string? Brand { get; set; } // Marca

        [BsonElement("color")]
        [JsonPropertyName("color")]
        public string? Color { get; set; } // Color

        [BsonElement("houseVisited")] // Casa a visitar
        [JsonPropertyName("houseVisited")]
        public string? HouseVisited { get; set; }

        [BsonElement("arrivalDateTime")] // Fecha/Hora esperada de llegada
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] // O local si prefieres manejar zonas horarias
        [JsonPropertyName("arrivalDateTime")]
        public DateTime? ArrivalDateTime { get; set; }

        [BsonElement("personVisited")] // Persona a visitar
        [JsonPropertyName("personVisited")]
        public string? PersonVisited { get; set; }

        [BsonElement("status")] // Estado: PENDIENTE, INGRESADO, CERRADO, CANCELADO?
        [JsonPropertyName("status")]
        public string Status { get; set; } = "PENDIENTE"; // Por defecto al crear

        [BsonElement("createdBy")] // Quién lo pre-registró (opcional)
        [JsonPropertyName("createdBy")]
        public string? CreatedBy { get; set; }

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Fecha de creación del pre-registro
    }
}