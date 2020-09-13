using System;
using System.Collections.Generic;
using System.Text;

namespace SharedModels
{
    public class Auth
    {
        public const string AdminRoleName = "Admin";
        public const string UserRoleName = "User";

        public const string JwtKey = "A very secret key Guid?!";
        public const string JwtIssuer = "https://localhost:5001";
        public const int JwtExpireDays = 30;
    }
}
