using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FuelManagement.Models
{
    public class Shed
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string owner { get; set; } = null!;
        public string name { get; set; }

        [BsonElement("tokens")]
        [JsonPropertyName("tokens")]
        public string tokenIds { get; set; }
    }
}

