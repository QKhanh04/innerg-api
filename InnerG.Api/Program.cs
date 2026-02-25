using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using InnerG.Api.Data;
using InnerG.Api.Data.Seed;
using InnerG.Api.Exceptions;
using InnerG.Api.Exceptions.Handlers;
using InnerG.Api.Models;
using InnerG.Api.Repositories.Backgrounds;
using InnerG.Api.Services.Backgrounds;
using InnerG.Api.Services.Implementations;
using InnerG.Api.Services.Interfaces;
using InnerG.Api.Validators;
using Scalar.AspNetCore;

/* =========================
   LOAD ENV (FAIL FAST)
   ========================= */

Env.Load(); // load .env trước TẤT CẢ

var builder = WebApplication.CreateBuilder(args);

/* =========================
   FAIL-FAST CONFIG VALIDATION
   ========================= */

string Require(string key) =>
    builder.Configuration[key]
    ?? throw new ConfigurationException(key);

// Database
var dbConnection = Require("DB_CONNECTION");

// JWT
var jwtKey = Require("JWT_KEY");
var jwtIssuer = Require("Jwt:Issuer");
var jwtAudience = Require("Jwt:Audience");

// SMTP
var smtpHost = Require("SMTP_HOST");
var smtpPort = Require("SMTP_PORT");
var smtpUser = Require("SMTP_USERNAME");
var smtpPass = Require("SMTP_PASSWORD");
var smtpFromName = Require("SMTP_FROM_NAME");
var googleClientId = Require("GOOGLE_CLIENT_ID");
var googleClientSecret = Require("GOOGLE_CLIENT_SECRET");

// Frontend
var frontendUrls =
    builder.Configuration.GetSection("Frontend:Urls").Get<string[]>()
    ?? throw new ConfigurationException("Frontend:Urls");

/* =========================
   MVC & VALIDATION
   ========================= */

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

/* =========================
   EXCEPTION HANDLING
   ========================= */

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

/* =========================
   OPEN API
   ========================= */

builder.Services.AddOpenApi();

/* =========================
   DATABASE
   ========================= */

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbConnection)
);

/* =========================
   CORS
   ========================= */

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins(frontendUrls)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

/* =========================
   IDENTITY
   ========================= */

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    // options.User.RequireUniqueEmail = true;
    // options.SignIn.RequireConfirmedEmail = true;

})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

/* =========================
   JWT AUTH
   ========================= */

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
});



builder.Services.AddAuthorization();

/* =========================
   APPLICATION SERVICES
   ========================= */

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddHostedService<RefreshTokenCleanupService>();

/* =========================
   BUILD APP
   ========================= */

var app = builder.Build();

/* =========================
   MIDDLEWARE PIPELINE
   ========================= */

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
