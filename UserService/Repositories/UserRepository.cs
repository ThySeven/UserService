using UserService.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserService.Services;

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

        public UserModelDTO CreateUser(UserModel user)
        {
            var existingUser = _users.Find(u => u.Username == user.Username).FirstOrDefault();
            if (existingUser != null)
            {
                // User with the same username already exists, handle the error (e.g., throw an exception)
                throw new Exception("Username already exists. Please choose a different username.");
            }
            user.Id = Guid.NewGuid().ToString();
            // Generate a random salt
            byte[] salt = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(salt);

            // Compute the hash of the password concatenated with the salt
            byte[] hashedPasswordWithSalt = new Rfc2898DeriveBytes(user.Password, salt, 10000).GetBytes(32);

            // Convert the byte array to a base64-encoded string for storage
            string hashedPassword = Convert.ToBase64String(hashedPasswordWithSalt);
    
            // Store the hashed password and the salt in the UserModel
            user.Password = hashedPassword;
            user.Salt = Convert.ToBase64String(salt);
            user.RegistrationDate = DateTime.Now;
            _users.InsertOne(user);

            var mail = new MailModel { ReceiverMail = user.Email, Header = "E-mail Verifikation", Content = $"Klik på dette link for at verificere din email <ahref>{Environment.GetEnvironmentVariable("PUBLIC_IP")}/user/verify/{user.Id}</ahref>" };

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

            return new UserModelDTO(user);
        }

        public void DeleteUser(string id)
        {
            var filter = Builders<UserModel>.Filter.Eq("Id", id);

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
            var filter = Builders<UserModel>.Filter.Eq("Id", id);
            var userDTO = new UserModelDTO(_users.Find(filter).FirstOrDefault());
            return userDTO;
        }

        public UserModelDTO UpdateUser(UserModelDTO newUserData)
        {
            var currentUser = GetById(newUserData.Id);
    
            if(string.IsNullOrEmpty(newUserData.FirstName))
                newUserData.FirstName = currentUser.FirstName;
            if(string.IsNullOrEmpty(newUserData.LastName))
                newUserData.LastName = currentUser.LastName;
            if(string.IsNullOrEmpty(newUserData.Email))
                newUserData.Email = currentUser.Email;
            if(string.IsNullOrEmpty(newUserData.Username))
                newUserData.Username = currentUser.Username;
            if(string.IsNullOrEmpty(newUserData.Address))
                newUserData.Address = currentUser.Address;
            if(string.IsNullOrEmpty(newUserData.PhoneNumber))
                newUserData.PhoneNumber = currentUser.PhoneNumber;

            var filter = Builders<UserModel>.Filter.Eq("Id", newUserData.Id);
            var update = Builders<UserModel>.Update
                .Set(x => x.FirstName, newUserData.FirstName)
                .Set(x => x.LastName, newUserData.LastName)
                .Set(x => x.Email, newUserData.Email)
                .Set(x => x.Username, newUserData.Username)
                .Set(x => x.Address, newUserData.Address)
                .Set(x => x.PhoneNumber, newUserData.PhoneNumber);

            _users.UpdateOne(filter, update);
            return newUserData;
        }

        public void UpdatePassword(LoginModel credentials, string newPassword)
        {
            // Retrieve the user from the database based on the provided username
            var user = _users.Find(u => u.Username == credentials.Username).FirstOrDefault();

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Decode the stored salt from base64
            byte[] salt = Convert.FromBase64String(user.Salt);

            // Compute the hash of the old password concatenated with the salt
            byte[] hashedPasswordWithSalt = new Rfc2898DeriveBytes(credentials.Password, salt, 10000).GetBytes(32);

            // Convert the byte array to a base64-encoded string for comparison
            string hashedPassword = Convert.ToBase64String(hashedPasswordWithSalt);

            // Check if the provided old password matches the stored hash
            if (hashedPassword != user.Password)
            {
                throw new Exception("Invalid old password.");
            }

            // Generate a new random salt
            byte[] newSalt = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(newSalt);

            // Compute the hash of the new password concatenated with the new salt
            byte[] newHashedPasswordWithSalt = new Rfc2898DeriveBytes(newPassword, newSalt, 10000).GetBytes(32);

            // Convert the byte array to a base64-encoded string for storage
            string newHashedPassword = Convert.ToBase64String(newHashedPasswordWithSalt);

            // Update the user's password and salt in the database
            var update = Builders<UserModel>.Update
                .Set(u => u.Password, newHashedPassword)
                .Set(u => u.Salt, Convert.ToBase64String(newSalt));

            _users.UpdateOne(u => u.Username == credentials.Username, update);
        }


        public void VerifyUser(string id)
        {
            var filter = Builders<UserModel>.Filter.Eq("Id", id);
            var update = Builders<UserModel>.Update.Set(u => u.Verified, true);
            _users.UpdateOne(filter, update);
        }
        public UserModelDTO Login(LoginModel credentials)
        {
            // Retrieve the user from the database based on the provided username
            var user = _users.Find(u => u.Username == credentials.Username).FirstOrDefault();

            // If the user is not found, return null indicating authentication failure
            if (user == null || !user.Verified)
            {
                if (!user.Verified)
                {
                    AuctionCoreLogger.Logger.Warn($"User loggin attempt: {user.Username}, is not verified");
                }
                
                return null;
            }

            // Decode the stored salt from base64
            byte[] salt = Convert.FromBase64String(user.Salt);

            // Compute the hash of the provided password concatenated with the salt
            byte[] hashedPasswordWithSalt = new Rfc2898DeriveBytes(credentials.Password, salt, 10000).GetBytes(32);

            // Convert the byte array to a base64-encoded string for comparison
            string hashedPassword = Convert.ToBase64String(hashedPasswordWithSalt);

            // Compare the computed hash with the stored hash
            if (hashedPassword == user.Password)
            {
                var userDTO = new UserModelDTO(user);
                // Passwords match, return the user indicating successful authentication
                return userDTO;
            }
            else
            {
                // Passwords don't match, return null indicating authentication failure
                throw new Exception("Password incorrect");
            }
        }
    }
}
