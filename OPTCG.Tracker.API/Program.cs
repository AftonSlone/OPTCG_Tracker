using Microsoft.EntityFrameworkCore;
using OPTCG.Tracker.Data;
using OPTCG.Tracker.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register JWT service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var authSettings = builder.Configuration.GetSection("Authentication");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddGoogle(options =>
{
    options.ClientId = authSettings["Google:ClientId"] ?? "";
    options.ClientSecret = authSettings["Google:ClientSecret"] ?? "";
})
.AddOAuth("GitHub", options =>
{
    options.ClientId = authSettings["GitHub:ClientId"] ?? "";
    options.ClientSecret = authSettings["GitHub:ClientSecret"] ?? "";
    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";
    options.CallbackPath = "/signin-github";
    // Claim mapping will be handled manually in the callback
})
.AddMicrosoftAccount(options =>
{
    options.ClientId = authSettings["Microsoft:ClientId"] ?? "";
    options.ClientSecret = authSettings["Microsoft:ClientSecret"] ?? "";
})
.AddOAuth("Discord", options =>
{
    options.ClientId = authSettings["Discord:ClientId"] ?? "";
    options.ClientSecret = authSettings["Discord:ClientSecret"] ?? "";
    options.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
    options.TokenEndpoint = "https://discord.com/api/oauth2/token";
    options.UserInformationEndpoint = "https://discord.com/api/users/@me";
    options.CallbackPath = "/signin-discord";
    // Claim mapping will be handled manually in the callback
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAll");

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
