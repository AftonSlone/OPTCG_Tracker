using Microsoft.EntityFrameworkCore;
using OPTCG.Tracker.Data;
using OPTCG.Tracker.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Cookies;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Enable static files
builder.Services.AddDirectoryBrowser();

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

var authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
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
});

// Add Google OAuth if credentials are configured
var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? authSettings["Google:ClientId"];
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? authSettings["Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authenticationBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

// Add Twitch OAuth if credentials are configured
var twitchClientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID") ?? authSettings["Twitch:ClientId"];
var twitchClientSecret = Environment.GetEnvironmentVariable("TWITCH_CLIENT_SECRET") ?? authSettings["Twitch:ClientSecret"];
if (!string.IsNullOrEmpty(twitchClientId) && !string.IsNullOrEmpty(twitchClientSecret))
{
    authenticationBuilder.AddOAuth("Twitch", options =>
    {
        options.ClientId = twitchClientId;
        options.ClientSecret = twitchClientSecret;
        options.AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize";
        options.TokenEndpoint = "https://id.twitch.tv/oauth2/token";
        options.UserInformationEndpoint = "https://api.twitch.tv/helix/users";
        options.CallbackPath = "/signin-twitch";
        options.Scope.Add("user:read:email");
        options.SaveTokens = true;
        
        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                using var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                request.Headers.Add("Client-Id", twitchClientId);
                
                using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var user = System.Text.Json.JsonDocument.Parse(content);
                
                var data = user.RootElement.GetProperty("data")[0];
                var id = data.GetProperty("id").GetString();
                var login = data.GetProperty("login").GetString();
                var email = data.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : "";
                
                context.Identity.AddClaim(new System.Security.Claims.Claim("id", id));
                context.Identity.AddClaim(new System.Security.Claims.Claim("username", login));
                if (!string.IsNullOrEmpty(email))
                {
                    context.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email));
                }
            }
        };
    });
}

// Add Microsoft OAuth if credentials are configured
var microsoftClientId = Environment.GetEnvironmentVariable("MICROSOFT_CLIENT_ID") ?? authSettings["Microsoft:ClientId"];
var microsoftClientSecret = Environment.GetEnvironmentVariable("MICROSOFT_CLIENT_SECRET") ?? authSettings["Microsoft:ClientSecret"];
if (!string.IsNullOrEmpty(microsoftClientId) && !string.IsNullOrEmpty(microsoftClientSecret))
{
    authenticationBuilder.AddMicrosoftAccount(options =>
    {
        options.ClientId = microsoftClientId;
        options.ClientSecret = microsoftClientSecret;
    });
}

// Add Discord OAuth if credentials are configured
var discordClientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID") ?? authSettings["Discord:ClientId"];
var discordClientSecret = Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET") ?? authSettings["Discord:ClientSecret"];
if (!string.IsNullOrEmpty(discordClientId) && !string.IsNullOrEmpty(discordClientSecret))
{
    authenticationBuilder.AddOAuth("Discord", options =>
    {
        options.ClientId = discordClientId;
        options.ClientSecret = discordClientSecret;
        options.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
        options.TokenEndpoint = "https://discord.com/api/oauth2/token";
        options.UserInformationEndpoint = "https://discord.com/api/users/@me";
        options.CallbackPath = "/signin-discord";
        options.Scope.Add("identify");
        options.Scope.Add("email");
        options.SaveTokens = true;
        
        // Custom event handler to extract user info from token response
        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                using var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                
                using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var user = System.Text.Json.JsonDocument.Parse(content);
                
                var id = user.RootElement.GetProperty("id").GetString();
                var username = user.RootElement.GetProperty("username").GetString();
                var discriminator = user.RootElement.GetProperty("discriminator").GetString();
                var email = user.RootElement.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : "";
                
                context.Identity.AddClaim(new System.Security.Claims.Claim("id", id));
                context.Identity.AddClaim(new System.Security.Claims.Claim("username", $"{username}#{discriminator}"));
                if (!string.IsNullOrEmpty(email))
                {
                    context.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email));
                }
            }
        };
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Enable static files for React app
app.UseStaticFiles();

// Serve React app as SPA
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api") && 
        !context.Request.Path.StartsWithSegments("/swagger") &&
        !context.Request.Path.Value.Contains("."))
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(builder.Environment.WebRootPath, "index.html"));
        return;
    }
    await next();
});

app.Run();
