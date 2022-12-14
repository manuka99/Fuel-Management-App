/*
    Manuka Yasas (IT19133850)
    Data access layer of the user entity
*/
using System;
using FuelManagement.Models;
using MongoDB.Driver;

namespace FuelManagement.Services
{
    public class UserService
    {

        private readonly IMongoCollection<User> _database;

        public UserService(IMongoDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _database = database.GetCollection<User>(settings.UserCollectionName);
        }

        // get the total number of users within the system
        public async Task<long> GetTotalCount()
        {
            return await _database.CountDocumentsAsync(s => true);
        }

        // get user by id
        public async Task<User> GetByIdAsync(string id)
        {
            return await _database.Find<User>(s => s.Id == id).FirstOrDefaultAsync();
        }

        //get user by email
        public async Task<User> GetByEmailAsync(string? email)
        {
            return await _database.Find<User>(s => s.email == email).FirstOrDefaultAsync();
        }

        // save a new user in database
        public async Task<bool> CreateAsync(User user)
        {
            bool isUserExist = await checkEmailExists(user.email);
            if (!isUserExist)
            {
                user.Id = null;
                await _database.InsertOneAsync(user);
                return true;
            }
            else
            {
                return false;
            }
        }

        // save/update a existing user
        public async Task<bool> UpdateAsync(string id, User user)
        {
            var dbUser = await GetByIdAsync(id);
            if (dbUser.Id == id)
            {
                user.Id = id;
                await _database.ReplaceOneAsync(s => s.Id == id, user);
                return true;
            }
            else return false;
        }

        // delete a user by id
        public async Task DeleteByIdAsync(string id)
        {
            await _database.DeleteOneAsync(s => s.Id == id);
        }

        // delete a user by email
        public async Task DeleteByEmailAsync(string email)
        {
            await _database.DeleteOneAsync(s => s.email == email);
        }

        // returns true if an account exists with the provided email
        public async Task<bool> checkEmailExists(string? email)
        {
            var dbUser = await this.GetByEmailAsync(email);
            return dbUser != null && dbUser.Id != null; ;
        }

    }
}

