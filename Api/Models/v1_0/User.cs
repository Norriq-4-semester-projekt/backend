using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.v1_0
{
    public class User
    {
        public String Username { get; set; }
        public String Password { get; set; }
        public String PasswordHash { get; set; }
        public String Salt { get; set; }

        public User(String Username)
        {
            this.Username = Username;
        }
    }
}
