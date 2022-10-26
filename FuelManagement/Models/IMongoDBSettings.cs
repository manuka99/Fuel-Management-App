/*
    Manuka Yasas (IT19133850)
    Model of the interface class
    Store all properties related to creating a mongo db connection
*/
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

