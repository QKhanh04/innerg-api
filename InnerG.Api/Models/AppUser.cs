using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace InnerG.Api.Models
{
    public class AppUser : IdentityUser
    {
        // Refresh Token
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
    }
}