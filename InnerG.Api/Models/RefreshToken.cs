using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnerG.Api.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public AppUser AppUser { get; set; } = null!;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public bool IsRevoked { get; set; }
    }
}