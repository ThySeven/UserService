using UserService.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
            var existingUser = _users.Find(u => u.userName == user.userName).FirstOrDefault();
            if (existingUser != null)
            {
                // User with the same username already exists, handle the error (e.g., throw an exception)
                throw new Exception("Username already exists. Please choose a different username.");
            }
            
            // Generate a random salt
            byte[] salt = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(salt);

            // Compute the hash of the password concatenated with the salt
            byte[] hashedPasswordWithSalt = new Rfc2898DeriveBytes(user.password, salt, 10000).GetBytes(32);

            // Convert the byte array to a base64-encoded string for storage
            string hashedPassword = Convert.ToBase64String(hashedPasswordWithSalt);
    
            // Store the hashed password and the salt in the UserModel
            user.password = hashedPassword;
            user.salt = Convert.ToBase64String(salt);
            
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
                channel.QueueDeclare(queue: Environment.GetEnvironmentVariable("RabbitMQQueueName"),
                                              durable: false,
                                              exclusive: false,
                                              autoDelete: false,
                                              arguments: null);

                channel.BasicPublish(exchange: "",
                                              routingKey: Environment.GetEnvironmentVariable("RabbitMQQueueName"),
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

        // GellAll if needed :)
        /*public IEnumerable<UserModelDTO> GetAll()
        {
            var userList = new List<UserModelDTO>();
            var users = _users.Find(_ => true).ToList();
            
            foreach (var user in users)
            {
                userList.Add(new UserModelDTO(user));
            }
            return userList;
        }*/
        
        public UserModelDTO GetById(string id)
        {
            var filter = Builders<UserModel>.Filter.Eq("id", id);
            var userDTO = new UserModelDTO(_users.Find(filter).FirstOrDefault());
            return userDTO;
        }

        public UserModel UpdateUser(UserModel newUserData)
        {
            var filter = Builders<UserModel>.Filter.Eq("id", newUserData.id);
            var update = Builders<UserModel>.Update
                .Set(x => x.firstName, newUserData.firstName)
                .Set(x => x.lastName, newUserData.lastName)
                .Set(x => x.email, newUserData.email)
                .Set(x => x.userName, newUserData.userName)
                .Set(x => x.password, newUserData.password)
                .Set(x => x.address, newUserData.address)
                .Set(x => x.phoneNumber, newUserData.phoneNumber);

            _users.UpdateOne(filter, update);
            return newUserData;
        }

        public void VerifyUser(string id)
        {
            var filter = Builders<UserModel>.Filter.Eq("id", id);
            var update = Builders<UserModel>.Update.Set(u => u.verified, true);
            _users.UpdateOne(filter, update);
        }
        public UserModelDTO Login(LoginModel credentials)
        {
            // Retrieve the user from the database based on the provided username
            var user = _users.Find(u => u.userName == credentials.Username).FirstOrDefault();

            // If the user is not found, return null indicating authentication failure
            if (user == null)
            {
                return null;
            }

            // Decode the stored salt from base64
            byte[] salt = Convert.FromBase64String(user.salt);

            // Compute the hash of the provided password concatenated with the salt
            byte[] hashedPasswordWithSalt = new Rfc2898DeriveBytes(credentials.Password, salt, 10000).GetBytes(32);

            // Convert the byte array to a base64-encoded string for comparison
            string hashedPassword = Convert.ToBase64String(hashedPasswordWithSalt);

            // Compare the computed hash with the stored hash
            if (hashedPassword == user.password)
            {
                var userDTO = new UserModelDTO(user);
                // Passwords match, return the user indicating successful authentication
                return userDTO;
            }
            else
            {
                // Passwords don't match, return null indicating authentication failure
                return null;
            }
        }
    }
}
