/*
    Manuka Yasas (IT19133850)
    Data access layer of the fuel request entity
*/
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

        // next token number would be the number of incompleted items in the database
        public async Task<long> GetNextTokenId(string shedId)
        {
            return await _database.CountDocumentsAsync(s => s.shed == shedId && s.isCompleted == false);
        }

        // get fuel request by document id
        public async Task<FuelRequests> GetByIdAsync(string id)
        {
            return await _database.Find<FuelRequests>(s => s.Id == id).FirstOrDefaultAsync();
        }

        // get incompleted fuel request by token id and shed id
        public async Task<FuelRequests> GetByTokenIdAndShed(int tokenId, string shedId)
        {
            return await _database.Find<FuelRequests>(s => s.tokenId == tokenId && s.shed == shedId).FirstOrDefaultAsync();
        }

        // get incompleted fuel request by shed id
        public async Task<List<FuelRequests>> GetByShedAsync(string? shedId)
        {
            return await _database.Find<FuelRequests>(s => s.shed == shedId && s.isCompleted == false).ToListAsync();
        }

        // get incompleted fuel request by shed id and fuel type
        public async Task<List<FuelRequests>> GetByShedAndFuelType(string? shedId, string fuelType)
        {
            return await _database.Find<FuelRequests>(s => s.shed == shedId && s.isCompleted == false && s.fuelType == fuelType).ToListAsync();
        }

        // get incompleted fuel request by shed id and user id
        public async Task<List<FuelRequests>> GetByShedUserAsync(string? shedId, string? userId)
        {
            return await _database.Find<FuelRequests>(s => s.shed == shedId && s.user == userId && s.isCompleted == false).ToListAsync();
        }

        // save a new fuel request in db
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

        // save/update a existing fuel request in db
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

        // delete a fuel request by request id
        public async Task DeleteByIdAsync(string id)
        {
            await _database.DeleteOneAsync(s => s.Id == id);
        }

        // delete all incompleted fuel requests by shed id
        public async Task DeleteAllIncompleteRequestsPerShedAsync(string shedId)
        {
            await _database.DeleteManyAsync(s => s.shed == shedId && s.isCompleted == false);
        }

        // validate if a user has incompleted fuel requests made for a given shed id
        public async Task<bool> checkUserRequestedShed(string? shed, string? user)
        {
            var dbData = await this.GetByShedUserAsync(shed, user);
            return dbData.Count != 0;
        }

    }
}

