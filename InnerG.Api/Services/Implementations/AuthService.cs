using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InnerG.Api.Data;
using InnerG.Api.DTOs;
using InnerG.Api.Exceptions;
using InnerG.Api.Exceptions.Helpers;
using InnerG.Api.Models;
using InnerG.Api.Services.Interfaces;

namespace InnerG.Api.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        AppDbContext context,
        IEmailService emailService,
        IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task RegisterAsync(RegisterRequest register)
        {
            if (await _userManager.FindByNameAsync(register.UserName) != null)
                throw new ConflictException("Username already exists");

            if (await _userManager.FindByEmailAsync(register.Email) != null)
                throw new ConflictException("Email already exists");

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new AppUser
                {
                    UserName = register.UserName,
                    Email = register.Email,
                    EmailConfirmed = false
                };

                var result =
                    await _userManager.CreateAsync(user, register.Password);


                if (!result.Succeeded)
                    throw IdentityErrorMapper.ToValidationException(result);

                if (!await _roleManager.RoleExistsAsync("User"))
                    throw new BadRequestException(
                        "Default role User is not configured");

                await _userManager.AddToRoleAsync(user, "User");
                await SendConfirmEmailAsync(user);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        private async Task<AppUser?> FindUserAsync(string acccount)
        {
            var userByEmail = await _userManager.FindByEmailAsync(acccount);
            if (userByEmail != null) return userByEmail;
            return await _userManager.FindByNameAsync(acccount);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await FindUserAsync(request.EmailOrUsername) ?? throw new UnauthorizedException("User not found");
            if (!user.EmailConfirmed)
                throw new UnauthorizedException("Email is not confirmed");

            var validatePassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!validatePassword) throw new UnauthorizedException("Password is incorrect");

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken.Token,
                UserName = user.UserName ?? "",
                Email = user.Email ?? ""

            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token == null)
                return; // idempotent

            token.IsRevoked = true;
            await _context.SaveChangesAsync();
        }

        public async Task LogoutAllAsync(string userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId && !x.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }




        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var refresh = await _context.RefreshTokens
                .Include(x => x.AppUser)
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (refresh == null)
                throw new UnauthorizedException("Invalid refresh token");

            if (refresh.Expires <= DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token expired");

            if (refresh.IsRevoked)
            {
                //close all tokens of user
                // var userId = refresh.AppUser.Id;
                // var userTokens = _context.RefreshTokens
                //     .Where(x => x.UserId == userId && !x.IsRevoked)
                //     .ToList();
                // foreach (var token in userTokens)
                // {
                //     token.IsRevoked = true;
                // }
                // await _context.SaveChangesAsync();

                throw new UnauthorizedException("Revoked refresh token");
            }


            var user = refresh.AppUser;

            var roles = await _userManager.GetRolesAsync(user);

            refresh.IsRevoked = true;

            var newAccessToken =
                _tokenService.GenerateAccessToken(user, roles);

            var newRefreshToken =
                _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                UserName = user.UserName!,
                Email = user.Email!
            };
        }

        public async Task<UserInfoResponse> GetCurrentUserInfoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            return new UserInfoResponse
            {
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                Roles = roles
            };
        }
        private async Task SendConfirmEmailAsync(AppUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmBaseUrl = _configuration["Frontend:ConfirmEmailUrl"]!;

            var confirmLink =
                $"{confirmBaseUrl}?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailService.SendEmailConfirmationAsync(
                user.Email!,
                "Confirm your email",
                $"""
        <h3>Welcome!</h3>
        <p>Please confirm your email by clicking the link below:</p>
        <a href="{confirmLink}">Confirm Email</a>
        """
            );
        }



        public async Task ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            // IMPORTANT: Decode token!
            var decodedToken = Uri.UnescapeDataString(token);
            Console.WriteLine($"Decoded token: {decodedToken}"); // Debug

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                // Log errors for debugging
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Code} - {error.Description}");
                }
                throw new BadRequestException("Invalid or expired confirmation token");
            }

            // VERIFY: Check if EmailConfirmed is now true
            Console.WriteLine($"Email confirmed: {user.EmailConfirmed}");
        }

        public async Task ResendConfirmEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new NotFoundException("User not found");

            if (user.EmailConfirmed)
                throw new BadRequestException("Email already confirmed");

            await SendConfirmEmailAsync(user);
        }





    }
}