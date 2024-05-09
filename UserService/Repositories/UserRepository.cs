using UserService.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
namespace UserService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserModel> _users;
        private string _connectionString = Environment.GetEnvironmentVariable("MongoDBConnectionString");

        public UserRepository()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("AuctionCoreServices");
            _users = database.GetCollection<UserModel>("Users");
        }
        public UserRepository(IMongoDatabase db)
        {
            _users = db.GetCollection<UserModel>("Users");
        }

        public void CreateUser(UserModel user)
        {
            _users.InsertOne(user);
        }

        public void DeleteUser(string id)
        {
            var filter = Builders<UserModel>.Filter.Eq("id", id);

            _users.DeleteOne(filter);
        }

        public IEnumerable<UserModel> GetAll()
        {
            return _users.Find(_ => true).ToList();
        }


        public UserModel GetById(string id)
        {
            var filter = Builders<UserModel>.Filter.Eq("id", id);
            return _users.Find(filter).SingleOrDefault();
        }

        public UserModel UpdateUser(UserModel newUserData)
        {
            var filter = Builders<UserModel>.Filter.Eq("id", newUserData.id);
            var update = Builders<UserModel>.Update
                            .Set(x => x.address, newUserData.address); // Example of updating the address
                                                                       // Add other properties to update as required

            _users.UpdateOne(filter, update);
            return newUserData;
        }

        public void VerifyUser(string id)
        {
            var filter = Builders<UserModel>.Filter.Eq("id", id);
            var update = Builders<UserModel>.Update.Set(u => u.verified, true);
            _users.UpdateOne(filter, update);
        }
        //MANGLER!
        public void ValidateUser(string userName, string password)
        {
            throw new NotImplementedException();
        }
    }
}
