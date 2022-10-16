using System;
namespace FuelManagement.Models
{
    public class MongoDBSettings : IMongoDBSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UserCollectionName { get; set; } = null!;
        public string ShedCollectionName { get; set; } = null!;
    }
}

