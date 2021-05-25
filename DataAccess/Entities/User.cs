using System;
using System.Text.Json.Serialization;

namespace DataAccess.Entities
{
    public class User
    {
        public string Username { get; }
        
        public string Password { get; set; }

        public string Salt { get; set; }

        public User(string username)
        {
            Username = username;
        }

        public User(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}