/*
    Manuka Yasas (IT19133850)
    Data access layer of the shed entity
*/
using System;
using System.Threading;
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
            var logBuilder = Builders<Shed>.IndexKeys.Ascending(x => x.city).Ascending(x => x.tags);
            var indexModel = new CreateIndexModel<Shed>(logBuilder);
            _database.Indexes.CreateOne(indexModel);
        }

        // search for sheds by given text string
        public async Task<List<Shed>> search(string text)
        {
            return await _database.Find(Builders<Shed>.Filter.Text(text)).ToListAsync();
        }

        //get all saved sheds
        public async Task<List<Shed>> GetAllAsync()
        {
            return await _database.Find(s => true).ToListAsync();
        }

        // get shed by id 
        public async Task<Shed> GetByIdAsync(string id)
        {
            return await _database.Find<Shed>(s => s.Id == id).FirstOrDefaultAsync();
        }

        // get sheds by owner's user id
        public async Task<List<Shed>> GetByOwnerAsync(string ownerId)
        {
            return await _database.Find<Shed>(s => s.owner == ownerId).ToListAsync();
        }

        // save a new shed
        public async Task<Shed> CreateAsync(Shed shed)
        {
            shed.Id = null;
            await _database.InsertOneAsync(shed);
            return shed;
        }

        // save/update a existing shed
        public async Task UpdateAsync(string id, Shed shed)
        {
            await _database.ReplaceOneAsync(s => s.Id == id, shed);
        }

        // delete a shed by id
        public async Task DeleteAsync(string id)
        {
            await _database.DeleteOneAsync(s => s.Id == id);
        }

    }
}

