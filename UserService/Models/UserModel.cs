﻿namespace UserService.Models
{
    public class UserModel
    { 
        public string id {  get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string userName { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public bool verified { get; set; } = false;
    }
}