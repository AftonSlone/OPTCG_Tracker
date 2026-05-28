using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OPTCG.Tracker.API.Controllers;
using OPTCG.Tracker.API.DTOs;
using OPTCG.Tracker.Core.Models;
using OPTCG.Tracker.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OPTCG.Tracker.Tests
{
    public class UserControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IDbContextTransaction _transaction;

        public UserControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=OPTCGTrackerTests;Trusted_Connection=true;MultipleActiveResultSets=true")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
            _transaction = _context.Database.BeginTransaction();
        }

        public void Dispose()
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _context.Dispose();
        }

        private string GenerateJwtToken(int userId)
        {
            var key = Encoding.ASCII.GetBytes("your-super-secret-jwt-key-that-is-at-least-32-characters-long");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, "test@example.com")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Fact]
        public async Task GetProfile_ReturnsUnauthorized_WhenNoUserId()
        {
            // Arrange
            var controller = new UserController(_context);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };

            // Act
            var result = await controller.GetProfile();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetProfile_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            var controller = new UserController(_context);
            
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "999") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await controller.GetProfile();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetProfile_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                DisplayName = "Test User",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var controller = new UserController(_context);
            
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await controller.GetProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateProfile_ReturnsUnauthorized_WhenNoUserId()
        {
            // Arrange
            var controller = new UserController(_context);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };

            var request = new UpdateProfileRequest
            {
                Username = "newusername",
                DisplayName = "New Display Name"
            };

            // Act
            var result = await controller.UpdateProfile(request);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateProfile_ReturnsBadRequest_WhenUsernameTaken()
        {
            // Arrange
            var user1 = new User
            {
                Email = "user1@example.com",
                Username = "user1",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google1"
            };
            var user2 = new User
            {
                Email = "user2@example.com",
                Username = "user2",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google2"
            };
            
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var controller = new UserController(_context);
            
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user1.Id.ToString()) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = principal
                }
            };

            var request = new UpdateProfileRequest
            {
                Username = "user2", // Already taken by user2
                DisplayName = "New Display Name"
            };

            // Act
            var result = await controller.UpdateProfile(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateProfile_UpdatesDisplayName_WhenValid()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                DisplayName = "Old Name",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var controller = new UserController(_context);
            
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = principal
                }
            };

            var request = new UpdateProfileRequest
            {
                Username = "",
                DisplayName = "New Display Name"
            };

            // Act
            var result = await controller.UpdateProfile(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Verify the update
            var updatedUser = await _context.Users.FindAsync(user.Id);
            Assert.Equal("New Display Name", updatedUser.DisplayName);
        }
    }
}
