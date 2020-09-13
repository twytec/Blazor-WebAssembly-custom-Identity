using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class Auth
    {
        public const string AdminRoleName = "Admin";
        public const string UserRoleName = "User";

        public const string JwtKey = "Ein geheimer Schlüssel z.B. ein Guid";
        public const string JwtIssuer = "https://localhost5001";
        public const int JwtExpireDays = 30;
    }
}
