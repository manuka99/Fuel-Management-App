using System;
using System.ComponentModel.DataAnnotations;
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

        [Required(ErrorMessage = "Owner Required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string owner { get; set; }

        [Required(ErrorMessage = "Name Required")]
        public string name { get; set; }

        [Required(ErrorMessage = "City Required")]
        public string city { get; set; }

        [Required(ErrorMessage = "Address Required")]
        public string address { get; set; }

        public string? tags { get; set; }
        public string? description { get; set; }
        public double petrolQTY { get; set; }
        public double petrolHoldQTY { get; set; }
        public int petrolDispenserQTY { get; set; }
        public double dieselQTY { get; set; }
        public double dieselHoldQTY { get; set; }
        public int dieselDispenserQTY { get; set; }
    }
}

