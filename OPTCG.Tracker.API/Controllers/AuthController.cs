using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPTCG.Tracker.Data;
using OPTCG.Tracker.Core.Models;
using OPTCG.Tracker.Core.Services;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OPTCG.Tracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(ApplicationDbContext context, IJwtTokenService jwtTokenService, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("login/{provider}")]
        public IActionResult Login(string provider)
        {
            var redirectUrl = Url.Action("Callback", "Auth", new { provider });
            var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(properties, provider);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string provider)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(provider);
            
            if (!authenticateResult.Succeeded)
            {
                return BadRequest("Authentication failed");
            }

            var claims = authenticateResult.Principal?.Identities?.FirstOrDefault()?.Claims;
            if (claims == null)
            {
                return BadRequest("No claims found");
            }

            var email = "";
            var username = "";
            var providerUserId = "";

            // Debug: Log all claims for Discord
            if (provider == "Discord")
            {
                Console.WriteLine("Discord Claims:");
                foreach (var claim in claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }

                // For Discord, manually fetch user info from API using the token from the result
                var accessToken = "";
                if (authenticateResult.Properties?.Items != null && authenticateResult.Properties.Items.TryGetValue("access_token", out var tokenValue))
                {
                    accessToken = tokenValue?.ToString();
                    Console.WriteLine($"Discord Access Token from Items: {accessToken?.Substring(0, Math.Min(20, accessToken?.Length ?? 0))}...");
                }
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = authenticateResult.Properties?.GetTokenValue("access_token");
                    Console.WriteLine($"Discord Access Token from GetTokenValue: {accessToken?.Substring(0, Math.Min(20, accessToken?.Length ?? 0))}...");
                }
                
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var (discordEmail, discordUsername, discordId) = await GetDiscordUserInfoAsync(accessToken);
                    Console.WriteLine($"Discord User Info - Email: {discordEmail}, Username: {discordUsername}, ID: {discordId}");
                    if (!string.IsNullOrEmpty(discordEmail) && !string.IsNullOrEmpty(discordId))
                    {
                        email = discordEmail;
                        username = discordUsername;
                        providerUserId = discordId;
                    }
                }
                else
                {
                    Console.WriteLine("Discord access token is null or empty from both methods");
                }
            }
            else
            {
                // Extract user information based on provider
                var (providerEmail, providerUsername, providerId) = ExtractUserInfo(claims, provider);
                email = providerEmail;
                username = providerUsername;
                providerUserId = providerId;
            }
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerUserId))
            {
                return BadRequest("Required user information not found");
            }

            // Find or create user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.OAuthProvider == provider && u.OAuthProviderUserId == providerUserId);

            if (user == null)
            {
                // Check if user with same email already exists (linking multiple OAuth providers)
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (existingUser != null)
                {
                    // Update existing user with new OAuth provider
                    existingUser.OAuthProvider = provider;
                    existingUser.OAuthProviderUserId = providerUserId;
                    existingUser.LastModified = DateTime.UtcNow;
                    existingUser.LastLoginDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    user = existingUser;
                }
                else
                {
                    // Create new user
                    user = new User
                    {
                        Email = email,
                        Username = username ?? GenerateUsernameFromEmail(email),
                        DisplayName = username ?? GenerateUsernameFromEmail(email),
                        OAuthProvider = provider,
                        OAuthProviderUserId = providerUserId,
                        CreatedDate = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow,
                        LastLoginDate = DateTime.UtcNow,
                        Preferences = "{}"
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Update existing user
                user.LastModified = DateTime.UtcNow;
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);

            // Redirect to React dashboard with token
            // In development, redirect to frontend on port 3000
            // In production, this should be configured to the frontend URL
            var frontendUrl = "http://localhost:3000";
            return Redirect($"{frontendUrl}/dashboard?token={token}");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        private (string email, string username, string providerUserId) ExtractUserInfo(IEnumerable<Claim> claims, string provider)
        {
            var email = "";
            var username = "";
            var providerUserId = "";

            switch (provider)
            {
                case "Google":
                    email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
                    username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                    providerUserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
                    break;
                case "GitHub":
                    email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
                    username = claims.FirstOrDefault(c => c.Type == "username")?.Value ?? "";
                    providerUserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
                    break;
                case "Microsoft":
                    email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
                    username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                    providerUserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
                    break;
                case "Discord":
                    email = claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
                    username = claims.FirstOrDefault(c => c.Type == "username")?.Value ?? "";
                    var discriminator = claims.FirstOrDefault(c => c.Type == "discriminator")?.Value ?? "";
                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(discriminator))
                    {
                        username += $"#{discriminator}";
                    }
                    providerUserId = claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "";
                    break;
                case "Twitch":
                    email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
                    username = claims.FirstOrDefault(c => c.Type == "username")?.Value ?? "";
                    providerUserId = claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "";
                    break;
            }

            return (email, username, providerUserId);
        }

        private string GenerateUsernameFromEmail(string email)
        {
            var localPart = email.Split('@')[0];
            // Clean up the username to remove special characters
            var cleanUsername = new string(localPart.Where(char.IsLetterOrDigit).ToArray());
            
            // Ensure it's not empty and within reasonable length
            if (string.IsNullOrEmpty(cleanUsername))
            {
                cleanUsername = "user";
            }
            
            return cleanUsername.Length > 50 ? cleanUsername.Substring(0, 50) : cleanUsername;
        }

        private async Task<(string email, string username, string id)> GetDiscordUserInfoAsync(string accessToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync("https://discord.com/api/users/@me");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<JsonElement>(content);

                    var email = userInfo.TryGetProperty("email", out var emailProp) ? emailProp.GetString() ?? "" : "";
                    var username = userInfo.TryGetProperty("username", out var usernameProp) ? usernameProp.GetString() ?? "" : "";
                    var discriminator = userInfo.TryGetProperty("discriminator", out var discriminatorProp) ? discriminatorProp.GetString() ?? "" : "";
                    var id = userInfo.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";

                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(discriminator))
                    {
                        username += $"#{discriminator}";
                    }

                    return (email, username, id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Discord user info: {ex.Message}");
            }

            return ("", "", "");
        }
    }
}
