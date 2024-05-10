using UserService.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
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

            var mail = new MailModel { ReceiverMail = user.email, Header = "E-mail Verifikation", Content = $"Klik på dette link for at verificere din email <ahref>http://localhost:5145/user/verify/{user.id}</ahref>" };

            try
            {
                var factory = new ConnectionFactory { HostName = Environment.GetEnvironmentVariable("RabbitMQHostName") };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Serialize the received email data
                var serializedEmail = JsonSerializer.Serialize(mail);

                // Publish the serialized email data to RabbitMQ
                channel.QueueDeclare(queue: "MailQueue",
                                              durable: false,
                                              exclusive: false,
                                              autoDelete: false,
                                              arguments: null);

                channel.BasicPublish(exchange: "",
                                              routingKey: "MailQueue",
                                              basicProperties: null,
                                              body: Encoding.UTF8.GetBytes(serializedEmail));

                Console.WriteLine("Sent email to RabbitMQ: " + serializedEmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email to RabbitMQ: {ex.Message}");
            }
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
            return _users.Find(filter).FirstOrDefault();
        }

        public UserModel UpdateUser(UserModel newUserData)
        {
            var filter = Builders<UserModel>.Filter.Eq("id", newUserData.id);
            var update = Builders<UserModel>.Update
                .Set(x => x.firstName, newUserData.firstName)
                .Set(x => x.lastName, newUserData.lastName)
                .Set(x => x.email, newUserData.email)
                .Set(x => x.userName, newUserData.userName)
                .Set(x => x.address, newUserData.address)
                .Set(x => x.phoneNumber, newUserData.phoneNumber)
                .Set(x => x.verified, newUserData.verified);

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
