using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OPTCG.Tracker.Core.Models;
using OPTCG.Tracker.Data;

namespace OPTCG.Tracker.Tests
{
    public class UserTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IDbContextTransaction _transaction;

        public UserTests()
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

        [Fact]
        public void User_Model_HasRequiredProperties()
        {
            // Arrange & Act
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Assert
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("testuser", user.Username);
            Assert.Equal("Google", user.OAuthProvider);
            Assert.Equal("google123", user.OAuthProviderUserId);
            Assert.True(user.CreatedDate > DateTime.MinValue);
            Assert.True(user.LastModified > DateTime.MinValue);
        }

        [Fact]
        public void User_Model_CanSetOptionalProperties()
        {
            // Arrange & Act
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                DisplayName = "Test User",
                Preferences = "some preferences",
                LastLoginDate = DateTime.UtcNow,
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Assert
            Assert.Equal("Test User", user.DisplayName);
            Assert.Equal("some preferences", user.Preferences);
            Assert.NotNull(user.LastLoginDate);
        }

        [Fact]
        public async Task User_EmailMaxLength_Enforced()
        {
            // Arrange
            var user = new User
            {
                Email = new string('a', 256), // Exceeds 255 char limit
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_UsernameMaxLength_Enforced()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = new string('a', 101), // Exceeds 100 char limit
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_DisplayNameMaxLength_Enforced()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                DisplayName = new string('a', 101), // Exceeds 100 char limit
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_PreferencesMaxLength_Enforced()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                Preferences = new string('a', 1001), // Exceeds 1000 char limit
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_OAuthProviderMaxLength_Enforced()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = new string('a', 51), // Exceeds 50 char limit
                OAuthProviderUserId = "google123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_OAuthProviderUserIdMaxLength_Enforced()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = new string('a', 256) // Exceeds 255 char limit
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_RequiresEmail()
        {
            // Arrange
            var user = new User
            {
                Email = null, // Required field
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_RequiresUsername()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = null, // Required field
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_RequiresOAuthProvider()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = null, // Required field
                OAuthProviderUserId = "google123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_RequiresOAuthProviderUserId()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = null // Required field
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_EmailMustBeUnique()
        {
            // Arrange
            var user1 = new User
            {
                Email = "test@example.com",
                Username = "user1",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google1"
            };
            _context.Users.Add(user1);
            await _context.SaveChangesAsync();

            var user2 = new User
            {
                Email = "test@example.com", // Duplicate email
                Username = "user2",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google2"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user2);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_UsernameMustBeUnique()
        {
            // Arrange
            var user1 = new User
            {
                Email = "user1@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google1"
            };
            _context.Users.Add(user1);
            await _context.SaveChangesAsync();

            var user2 = new User
            {
                Email = "user2@example.com",
                Username = "testuser", // Duplicate username
                OAuthProvider = "Google",
                OAuthProviderUserId = "google2"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user2);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_OAuthProviderCombinationMustBeUnique()
        {
            // Arrange
            var user1 = new User
            {
                Email = "user1@example.com",
                Username = "user1",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };
            _context.Users.Add(user1);
            await _context.SaveChangesAsync();

            var user2 = new User
            {
                Email = "user2@example.com",
                Username = "user2",
                OAuthProvider = "Google", // Same provider
                OAuthProviderUserId = "google123" // Same provider user ID
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Users.Add(user2);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task User_CanHaveNullOptionalFields()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                DisplayName = null,
                Preferences = null,
                LastLoginDate = null,
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Act
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Null(retrievedUser.DisplayName);
            Assert.Null(retrievedUser.Preferences);
            Assert.Null(retrievedUser.LastLoginDate);
        }

        [Fact]
        public async Task User_CanHaveSpecialCharactersInUsername()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "user_123-test",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Act
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal("user_123-test", retrievedUser.Username);
        }

        [Fact]
        public async Task User_LastModified_CanBeManuallyUpdated()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var originalLastModified = user.LastModified;

            // Act
            await Task.Delay(10); // Small delay to ensure time difference
            user.DisplayName = "Updated Name";
            user.LastModified = DateTime.UtcNow; // Manually update
            await _context.SaveChangesAsync();

            var retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.True(retrievedUser.LastModified > originalLastModified);
        }

        [Fact]
        public void User_ProviderEnum_ParsesCorrectly()
        {
            // Arrange & Act
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "google123"
            };

            // Assert
            Assert.Equal(OAuthProvider.Google, user.ProviderEnum);
        }

        [Fact]
        public void User_ProviderEnum_ParsesMicrosoft()
        {
            // Arrange & Act
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Microsoft",
                OAuthProviderUserId = "ms123"
            };

            // Assert
            Assert.Equal(OAuthProvider.Microsoft, user.ProviderEnum);
        }

        [Fact]
        public void User_ProviderEnum_ParsesDiscord()
        {
            // Arrange & Act
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Discord",
                OAuthProviderUserId = "discord123"
            };

            // Assert
            Assert.Equal(OAuthProvider.Discord, user.ProviderEnum);
        }

        [Fact]
        public void User_ProviderEnum_ParsesTwitch()
        {
            // Arrange & Act
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Twitch",
                OAuthProviderUserId = "twitch123"
            };

            // Assert
            Assert.Equal(OAuthProvider.Twitch, user.ProviderEnum);
        }
    }
}
