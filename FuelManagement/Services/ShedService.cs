using System;
using FuelManagement.Models;
using MongoDB.Driver;

namespace FuelManagement.Services
{
    public class ShedService
    {

        private readonly IMongoCollection<Shed> _database;

        public ShedService(IMongoDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _database = database.GetCollection<Shed>(settings.ShedCollectionName);
        }

        public async Task<List<Shed>> GetAllAsync()
        {
            return await _database.Find(s => true).ToListAsync();
        }

        public async Task<Shed> GetByIdAsync(string id)
        {
            return await _database.Find<Shed>(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Shed> CreateAsync(Shed shed)
        {
            await _database.InsertOneAsync(shed);
            return shed;
        }

        public async Task UpdateAsync(string id, Shed shed)
        {
            await _database.ReplaceOneAsync(s => s.Id == id, shed);
        }

        public async Task DeleteAsync(string id)
        {
            await _database.DeleteOneAsync(s => s.Id == id);
        }

    }
}

