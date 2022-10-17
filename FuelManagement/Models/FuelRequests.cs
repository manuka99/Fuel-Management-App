
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FuelManagement.Models
{
    public class FuelRequests
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "User Required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string user { get; set; }

        [Required(ErrorMessage = "Shed Required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string shed { get; set; }

        public string fuelType { get; set; }
        public double fuelQTY { get; set; }
        public bool isApproved { get; set; }
        public bool isCompleted { get; set; }
    }
}

