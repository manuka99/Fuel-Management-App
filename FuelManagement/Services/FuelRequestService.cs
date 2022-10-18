using System;
using FuelManagement.Models;
using MongoDB.Driver;

namespace FuelManagement.Services
{
    public class FuelRequestService
    {

        private readonly IMongoCollection<FuelRequests> _database;

        public FuelRequestService(IMongoDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _database = database.GetCollection<FuelRequests>(settings.FuelRequestCollectionName);
        }

        public async Task<long> GetNextTokenId(string shedId)
        {
            return await _database.CountDocumentsAsync(s => s.shed == shedId && s.isCompleted == false);
        }

        public async Task<FuelRequests> GetByIdAsync(string id)
        {
            return await _database.Find<FuelRequests>(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<FuelRequests>> GetByShedAsync(string? shedId)
        {
            return await _database.Find<FuelRequests>(s => s.shed == shedId && s.isCompleted == false).ToListAsync();
        }

        public async Task<List<FuelRequests>> GetByShedUserAsync(string? shedId, string? userId)
        {
            return await _database.Find<FuelRequests>(s => s.shed == shedId && s.user == userId && s.isCompleted == false).ToListAsync();
        }

        public async Task<FuelRequests?> CreateAsync(FuelRequests fuelRequest)
        {
            bool isRequestExist = await checkUserRequestedShed(fuelRequest.shed, fuelRequest.user);
            if (!isRequestExist)
            {
                var tokenID = await this.GetNextTokenId(fuelRequest.shed);
                fuelRequest.Id = null;
                fuelRequest.tokenId = (int)tokenID + 1;
                await _database.InsertOneAsync(fuelRequest);
                return fuelRequest;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> UpdateAsync(string id, FuelRequests fuelRequest)
        {
            var dbData = await GetByIdAsync(id);
            if (dbData.Id == id)
            {
                fuelRequest.Id = id;
                await _database.ReplaceOneAsync(s => s.Id == id, fuelRequest);
                return true;
            }
            else return false;
        }

        public async Task DeleteByIdAsync(string id)
        {
            await _database.DeleteOneAsync(s => s.Id == id);
        }

        public async Task<bool> checkUserRequestedShed(string? shed, string? user)
        {
            var dbData = await this.GetByShedUserAsync(shed, user);
            return dbData.Count != 0;
        }

    }
}

