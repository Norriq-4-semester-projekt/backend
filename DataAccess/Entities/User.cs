using System;
using System.Text.Json.Serialization;

namespace DataAccess.Entities
{
    public class User
    {
        private string _id;

        public String Id
        {
            get => _id;
            set => _id = value;
        }

        private String _username;

        public String Username
        {
            get => _username;
            set => _username = value;
        }
        [JsonIgnore]

        private String _password;

        [JsonIgnore]
        public String Password
        {
            get => _password;
            set => _password = value;
        }

        private String _passwordHash;

        public String PasswordHash
        {
            get => _passwordHash;
            set => _passwordHash = value;
        }

        private String _salt;

        public String Salt
        {
            get => _salt;
            set => _salt = value;
        }

        public User(String Username)
        {
            this.Username = Username;
        }

        public User(String Username, string Password)
        {
            this.Username = Username;
            this.Password = Password;
        }
    }
}