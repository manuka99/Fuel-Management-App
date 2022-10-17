using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FuelManagement.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [Required(ErrorMessage = "Name Required")]
        public string? name { get; set; }
        [Required(ErrorMessage = "Email Required")]
        public string? email { get; set; }
        public string? phone { get; set; }
        public bool? isStationOwner { get; set; }
        [Required(ErrorMessage = "Password Required")]
        public string? password {get; set; }
        public DateTime? createdDate { get; set; }
    }
}

