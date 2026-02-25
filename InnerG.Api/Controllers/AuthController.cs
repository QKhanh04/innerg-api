using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InnerG.Api.Data;
using InnerG.Api.DTOs;
using InnerG.Api.Exceptions;
using InnerG.Api.Exceptions.Helpers;
using InnerG.Api.Services;
using InnerG.Api.Services.Interfaces;
using static InnerG.Api.DTOs.GoogleAuthDTO;

namespace InnerG.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            ValidationHelper.FromModelState(ModelState);
            var result = await _authService.LoginAsync(request);

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new
            {
                token = result.Token,
                userName = result.UserName,
                email = result.Email
            });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLoginAsync([FromBody] GoogleLoginRequest request)
        {
            System.Console.WriteLine("Google ID Token: " + request.IdToken);
            ValidationHelper.FromModelState(ModelState);
            var result = await _authService.LoginWithGoogleAsync(request.IdToken);

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new
            {
                token = result.Token,
                userName = result.UserName,
                email = result.Email
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            ValidationHelper.FromModelState(ModelState);
            var result = await _authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshTokenAsync()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized();
            }
            var result = await _authService.RefreshTokenAsync(refreshToken);
            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new
            {
                token = result.Token,
                userName = result.UserName,
                email = result.Email
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return NoContent(); // idempotent

            await _authService.LogoutAsync(refreshToken);

            DeleteRefreshTokenCookie();

            return NoContent();
        }
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            await _authService.LogoutAllAsync(userId);
            DeleteRefreshTokenCookie();
            return NoContent();
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(
    [FromQuery] string userId,
    [FromQuery] string token)
        {
            await _authService.ConfirmEmailAsync(userId, token);
            return Ok(new { message = "Email verified successfully" });
        }

        [HttpPost("resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] string email)
        {
            await _authService.ResendConfirmEmailAsync(email);
            return Ok(new { message = "Confirmation email resent" });
        }


        [Authorize]
        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserInfo(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != userId)
            {
                return Forbid();
            }
            var result = await _authService.GetCurrentUserInfoAsync(userId);
            return Ok(result);

        }



        [Authorize]
        [HttpGet("claims")]
        public IActionResult Claims()
        {
            return Ok(User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            }));
        }


        [HttpGet("debug")]
        public IActionResult Debug()
        {
            // log Cookie refresh_token
            var refreshToken = Request.Cookies["refresh_token"];
            return Ok(new { RefreshToken = refreshToken });
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,        // true khi dùng HTTPS
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/api/auth"
            };

            Response.Cookies.Append("refresh_token", refreshToken, options);
        }

        private void DeleteRefreshTokenCookie()
        {
            var options = new CookieOptions
            {
                SameSite = SameSiteMode.None,
                Secure = true,
                Path = "/api/auth"
            };
            Response.Cookies.Delete("refresh_token", options);
        }


    }
}