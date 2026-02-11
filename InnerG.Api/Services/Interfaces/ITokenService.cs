using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InnerG.Api.Models;

namespace InnerG.Api.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(AppUser user, IList<string> roles);
        RefreshToken GenerateRefreshToken(string userId);
    }
}