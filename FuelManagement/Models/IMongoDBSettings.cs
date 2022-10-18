using System;
namespace FuelManagement.Models
{
    public interface IMongoDBSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string ShedCollectionName { get; set; }
        string UserCollectionName { get; set; }
        string FuelRequestCollectionName { get; set; }
    }
}

