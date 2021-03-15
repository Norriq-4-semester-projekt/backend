using System;

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

        private String _password;

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
    }
}