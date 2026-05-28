using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OPTCG.Tracker.Core.Models;
using OPTCG.Tracker.Data;

namespace OPTCG.Tracker.Tests
{
    public class DeckTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IDbContextTransaction _transaction;

        public DeckTests()
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
        public void Deck_Model_HasRequiredProperties()
        {
            // Arrange & Act
            var deck = new Deck
            {
                Name = "Test Deck",
                UserId = 1
            };

            // Assert
            Assert.Equal("Test Deck", deck.Name);
            Assert.Equal(1, deck.UserId);
            Assert.True(deck.CreatedDate > DateTime.MinValue);
            Assert.True(deck.LastModified > DateTime.MinValue);
        }

        [Fact]
        public async Task Deck_NameMaxLength_Enforced()
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

            var deck = new Deck
            {
                Name = new string('A', 101), // Exceeds 100 char limit
                UserId = user.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Decks.Add(deck);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task Deck_RequiresName()
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

            var deck = new Deck
            {
                Name = null, // Required field
                UserId = user.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Decks.Add(deck);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task Deck_RequiresUserId()
        {
            // Arrange
            var deck = new Deck
            {
                Name = "Test Deck",
                UserId = 999 // Non-existent user
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Decks.Add(deck);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task Deck_CanBeCreatedAndRetrieved()
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

            var deck = new Deck
            {
                Name = "Test Deck",
                UserId = user.Id
            };

            // Act
            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

            var retrievedDeck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == deck.Id);

            // Assert
            Assert.NotNull(retrievedDeck);
            Assert.Equal("Test Deck", retrievedDeck.Name);
            Assert.Equal(user.Id, retrievedDeck.UserId);
        }

        [Fact]
        public async Task Deck_CanBeUpdated()
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

            var deck = new Deck
            {
                Name = "Test Deck",
                UserId = user.Id
            };
            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

            // Act
            deck.Name = "Updated Deck";
            await _context.SaveChangesAsync();

            var retrievedDeck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == deck.Id);

            // Assert
            Assert.NotNull(retrievedDeck);
            Assert.Equal("Updated Deck", retrievedDeck.Name);
        }

        [Fact]
        public async Task Deck_CanBeDeleted()
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

            var deck = new Deck
            {
                Name = "Test Deck",
                UserId = user.Id
            };
            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

            // Act
            _context.Decks.Remove(deck);
            await _context.SaveChangesAsync();

            var retrievedDeck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == deck.Id);

            // Assert
            Assert.Null(retrievedDeck);
        }

        [Fact]
        public async Task Deck_CanHaveSpecialCharactersInName()
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

            var deck = new Deck
            {
                Name = "Deck @#$%^&*()_+-=[]{}|;':\",./<>?",
                UserId = user.Id
            };

            // Act
            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

            var retrievedDeck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == deck.Id);

            // Assert
            Assert.NotNull(retrievedDeck);
            Assert.Equal("Deck @#$%^&*()_+-=[]{}|;':\",./<>?", retrievedDeck.Name);
        }

        [Fact]
        public async Task Deck_LastModified_CanBeManuallyUpdated()
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

            var deck = new Deck
            {
                Name = "Test Deck",
                UserId = user.Id
            };
            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

            var originalLastModified = deck.LastModified;

            // Act
            await Task.Delay(10); // Small delay to ensure time difference
            deck.Name = "Updated Deck";
            deck.LastModified = DateTime.UtcNow; // Manually update
            await _context.SaveChangesAsync();

            var retrievedDeck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == deck.Id);

            // Assert
            Assert.NotNull(retrievedDeck);
            Assert.True(retrievedDeck.LastModified > originalLastModified);
        }

        [Fact]
        public async Task User_CanHaveMultipleDecks()
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

            var deck1 = new Deck
            {
                Name = "Deck 1",
                UserId = user.Id
            };

            var deck2 = new Deck
            {
                Name = "Deck 2",
                UserId = user.Id
            };

            var deck3 = new Deck
            {
                Name = "Deck 3",
                UserId = user.Id
            };

            // Act
            _context.Decks.AddRange(deck1, deck2, deck3);
            await _context.SaveChangesAsync();

            var userDecks = await _context.Decks.Where(d => d.UserId == user.Id).ToListAsync();

            // Assert
            Assert.Equal(3, userDecks.Count);
        }

        [Fact]
        public async Task DeletingUser_DeletesAssociatedDecks()
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

            var deck1 = new Deck
            {
                Name = "Deck 1",
                UserId = user.Id
            };

            var deck2 = new Deck
            {
                Name = "Deck 2",
                UserId = user.Id
            };

            _context.Decks.AddRange(deck1, deck2);
            await _context.SaveChangesAsync();

            // Act
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var remainingDecks = await _context.Decks.Where(d => d.UserId == user.Id).ToListAsync();

            // Assert
            Assert.Empty(remainingDecks);
        }
    }
}
