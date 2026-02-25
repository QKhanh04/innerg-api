using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InnerG.Api.DTOs
{
    public class RegisterRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

    }

    public class LoginRequest
    {
        public string EmailOrUsername { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        public bool RequiresEmailConfirmation { get; set; }
    }


    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UserInfoResponse
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }

}