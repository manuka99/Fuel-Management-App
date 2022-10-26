/*
    Manuka Yasas (IT19133850)
    Model of the Database class
    Store all properties related to creating a mongo db connection
*/
using System;
namespace FuelManagement.Models
{
    public class MongoDBSettings : IMongoDBSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UserCollectionName { get; set; } = null!;
        public string ShedCollectionName { get; set; } = null!;
        public string FuelRequestCollectionName { get; set; } = null!;
    }
}

