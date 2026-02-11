using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InnerG.Api.DTOs;

namespace InnerG.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);

        Task LogoutAsync(string refreshToken);
        Task LogoutAllAsync(string userId);

        //get current user info
        Task<UserInfoResponse> GetCurrentUserInfoAsync(string userId);
        Task ConfirmEmailAsync(string userId, string token);
        Task ResendConfirmEmailAsync(string email);
    }
}