using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPTCG.Tracker.Data;
using OPTCG.Tracker.Core.Models;
using OPTCG.Tracker.API.DTOs;

namespace OPTCG.Tracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.Username,
                user.DisplayName,
                user.CreatedDate,
                user.LastModified,
                user.LastLoginDate,
                user.OAuthProvider
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            // Update username if provided and different
            if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
            {
                // Check if username is already taken
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username && u.Id != user.Id);

                if (existingUser != null)
                {
                    return BadRequest(new { message = "Username is already taken" });
                }

                user.Username = request.Username;
            }

            // Update display name if provided
            if (!string.IsNullOrEmpty(request.DisplayName))
            {
                user.DisplayName = request.DisplayName;
            }

            user.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                user.Id,
                user.Email,
                user.Username,
                user.DisplayName,
                user.CreatedDate,
                user.LastModified,
                user.LastLoginDate,
                user.OAuthProvider
            });
        }
    }
}
