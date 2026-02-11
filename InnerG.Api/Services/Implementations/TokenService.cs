using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using InnerG.Api.Exceptions;
using InnerG.Api.Models;
using InnerG.Api.Services.Interfaces;

namespace InnerG.Api.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateAccessToken(AppUser user, IList<string> roles)
        {
            var claims = new List<Claim>
    {
        // new(JwtRegisteredClaimNames.Sub, user.Id),
        // new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
        // new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        // new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            // new Claim("uid", user.Id),
            // new Claim("un", user.UserName!),
            // new Claim("em", user.Email!),
            // new Claim("jti", Guid.NewGuid().ToString())
    };

            // claims.AddRange(roles.Select(role => new Claim("role", role)));
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtKey = _config["JWT_KEY"]
                ?? throw new ConfigurationException("JWT_KEY");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_config.GetRequiredSection("Jwt:AccessTokenMinutes").Value!)
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public RefreshToken GenerateRefreshToken(string userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = userId,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config.GetRequiredSection("Jwt:RefreshTokenDays").Value!)),
                Created = DateTime.UtcNow
            };

            return refreshToken;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = _config["JWT_KEY"] ?? throw new ConfigurationException("JWT_KEY");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // QUAN TRỌNG: bỏ check expiry
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key)
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out SecurityToken securityToken
            );

            if (securityToken is not JwtSecurityToken jwtToken ||
    !jwtToken.Header.Alg.Equals(
        SecurityAlgorithms.HmacSha256,
        StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UnauthorizedException("Invalid token");
            }


            return principal;
        }

    }
}