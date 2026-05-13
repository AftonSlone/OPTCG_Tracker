using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPTCG.Tracker.Data;
using OPTCG.Tracker.Core.Models;
using OPTCG.Tracker.Core.Services;
using System.Security.Claims;

namespace OPTCG.Tracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(ApplicationDbContext context, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
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

            // Extract user information based on provider
            var (email, username, providerUserId) = ExtractUserInfo(claims, provider);
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerUserId))
            {
                return BadRequest("Required user information not found");
            }

            // Find or create user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.OAuthProvider == provider && u.OAuthProviderUserId == providerUserId);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = email,
                    Username = username ?? GenerateUsernameFromEmail(email),
                    OAuthProvider = provider,
                    OAuthProviderUserId = providerUserId,
                    CreatedDate = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update existing user
                user.LastModified = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);

            // Redirect to frontend with token
            var redirectUrl = $"http://localhost:3000/auth/callback?token={token}";
            return Redirect(redirectUrl);
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
                    email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
                    username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                    var discriminator = claims.FirstOrDefault(c => c.Type == "discriminator")?.Value ?? "";
                    if (!string.IsNullOrEmpty(discriminator))
                    {
                        username += $"#{discriminator}";
                    }
                    providerUserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
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
    }
}
