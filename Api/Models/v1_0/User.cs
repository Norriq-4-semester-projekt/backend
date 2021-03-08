using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.v1_0
{
    public class User
    {
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
