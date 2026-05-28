using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OPTCG.Tracker.Core.Models;
using OPTCG.Tracker.Data;

namespace OPTCG.Tracker.Tests
{
    public class EventRoundTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IDbContextTransaction _transaction;

        public EventRoundTests()
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
        public void Event_Model_HasRequiredProperties()
        {
            // Arrange & Act
            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = 1,
                DeckId = 1
            };

            // Assert
            Assert.Equal("Test Event", eventModel.Name);
            Assert.Equal(1, eventModel.UserId);
            Assert.Equal(1, eventModel.DeckId);
            Assert.False(eventModel.IsFinalized);
            Assert.True(eventModel.CreatedDate > DateTime.MinValue);
            Assert.True(eventModel.LastModified > DateTime.MinValue);
        }

        [Fact]
        public void Event_Model_CanSetOptionalProperties()
        {
            // Arrange & Act
            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = 1,
                DeckId = 1,
                FinalResult = "3-0",
                IsFinalized = true
            };

            // Assert
            Assert.Equal("3-0", eventModel.FinalResult);
            Assert.True(eventModel.IsFinalized);
        }

        [Fact]
        public void Round_Model_HasRequiredProperties()
        {
            // Arrange & Act
            var round = new Round
            {
                EventId = 1,
                RoundNumber = 1
            };

            // Assert
            Assert.Equal(1, round.EventId);
            Assert.Equal(1, round.RoundNumber);
            Assert.False(round.WentFirst);
            Assert.False(round.IsWin);
            Assert.True(round.CreatedDate > DateTime.MinValue);
            Assert.True(round.LastModified > DateTime.MinValue);
        }

        [Fact]
        public void Round_Model_CanSetOptionalProperties()
        {
            // Arrange & Act
            var round = new Round
            {
                EventId = 1,
                RoundNumber = 1,
                OpponentLeader = "Luffy",
                DiceRollResult = "6",
                WentFirst = true,
                IsWin = true
            };

            // Assert
            Assert.Equal("Luffy", round.OpponentLeader);
            Assert.Equal("6", round.DiceRollResult);
            Assert.True(round.WentFirst);
            Assert.True(round.IsWin);
        }

        [Fact]
        public async Task Event_CanBeCreatedAndRetrieved()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };

            // Act
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var retrievedEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventModel.Id);

            // Assert
            Assert.NotNull(retrievedEvent);
            Assert.Equal("Test Event", retrievedEvent.Name);
            Assert.Equal(user.Id, retrievedEvent.UserId);
            Assert.Equal(deck.Id, retrievedEvent.DeckId);
        }

        [Fact]
        public async Task Round_CanBeCreatedAndRetrieved()
        {
            // Arrange

            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var round = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 1,
                OpponentLeader = "Luffy",
                DiceRollResult = "6",
                WentFirst = true,
                IsWin = true
            };

            // Act
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();

            var retrievedRound = await _context.Rounds.FirstOrDefaultAsync(r => r.Id == round.Id);

            // Assert
            Assert.NotNull(retrievedRound);
            Assert.Equal(eventModel.Id, retrievedRound.EventId);
            Assert.Equal(1, retrievedRound.RoundNumber);
            Assert.Equal("Luffy", retrievedRound.OpponentLeader);
            Assert.True(retrievedRound.WentFirst);
            Assert.True(retrievedRound.IsWin);
        }

        [Fact]
        public async Task Event_CanHaveMultipleRounds()
        {
            // Arrange

            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var round1 = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 1,
                OpponentLeader = "Luffy",
                IsWin = true
            };

            var round2 = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 2,
                OpponentLeader = "Zoro",
                IsWin = false
            };

            var round3 = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 3,
                OpponentLeader = "Nami",
                IsWin = true
            };

            // Act
            _context.Rounds.AddRange(round1, round2, round3);
            await _context.SaveChangesAsync();

            var retrievedEvent = await _context.Events
                .Include(e => e.Rounds)
                .FirstOrDefaultAsync(e => e.Id == eventModel.Id);

            // Assert
            Assert.NotNull(retrievedEvent);
            Assert.NotNull(retrievedEvent.Rounds);
            Assert.Equal(3, retrievedEvent.Rounds.Count);
        }

        [Fact]
        public async Task DeletingEvent_DeletesAssociatedRounds()
        {
            // Arrange

            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var round1 = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 1,
                OpponentLeader = "Luffy"
            };

            var round2 = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 2,
                OpponentLeader = "Zoro"
            };

            _context.Rounds.AddRange(round1, round2);
            await _context.SaveChangesAsync();

            // Act
            _context.Events.Remove(eventModel);
            await _context.SaveChangesAsync();

            var remainingRounds = await _context.Rounds.Where(r => r.EventId == eventModel.Id).ToListAsync();

            // Assert
            Assert.Empty(remainingRounds);
        }


        [Fact]
        public async Task Event_CanBeUpdated()
        {
            // Arrange

            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            // Act
            eventModel.Name = "Updated Event";
            eventModel.FinalResult = "2-1";
            eventModel.IsFinalized = true;
            await _context.SaveChangesAsync();

            var retrievedEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventModel.Id);

            // Assert
            Assert.NotNull(retrievedEvent);
            Assert.Equal("Updated Event", retrievedEvent.Name);
            Assert.Equal("2-1", retrievedEvent.FinalResult);
            Assert.True(retrievedEvent.IsFinalized);
        }

        [Fact]
        public async Task Round_CanBeUpdated()
        {
            // Arrange

            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var round = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 1,
                OpponentLeader = "Luffy"
            };
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();

            // Act
            round.OpponentLeader = "Zoro";
            round.DiceRollResult = "5";
            round.WentFirst = true;
            round.IsWin = true;
            await _context.SaveChangesAsync();

            var retrievedRound = await _context.Rounds.FirstOrDefaultAsync(r => r.Id == round.Id);

            // Assert
            Assert.NotNull(retrievedRound);
            Assert.Equal("Zoro", retrievedRound.OpponentLeader);
            Assert.Equal("5", retrievedRound.DiceRollResult);
            Assert.True(retrievedRound.WentFirst);
            Assert.True(retrievedRound.IsWin);
        }

        [Fact]
        public async Task Event_NameMaxLength_Enforced()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = new string('A', 101), // Exceeds 100 char limit
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Events.Add(eventModel);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task Event_FinalResultMaxLength_Enforced()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id,
                FinalResult = new string('A', 51) // Exceeds 50 char limit
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Events.Add(eventModel);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task Round_OpponentLeaderMaxLength_Enforced()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var round = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 1,
                OpponentLeader = new string('A', 101) // Exceeds 100 char limit
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Rounds.Add(round);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task Round_DiceRollResultMaxLength_Enforced()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var round = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 1,
                DiceRollResult = new string('A', 11) // Exceeds 10 char limit
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Rounds.Add(round);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task Event_RequiresDeck()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = 999 // Non-existent deck
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Events.Add(eventModel);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task Round_RequiresEvent()
        {
            // Arrange
            var round = new Round
            {
                EventId = 999, // Non-existent event
                RoundNumber = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Rounds.Add(round);
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task Event_RequiresName()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = null, // Required field
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Events.Add(eventModel);
                await _context.SaveChangesAsync();
            });
        }


        [Fact]
        public async Task Event_CanHaveEmptyFinalResult()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id,
                FinalResult = null // Optional field can be null
            };

            // Act
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var retrievedEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventModel.Id);

            // Assert
            Assert.NotNull(retrievedEvent);
            Assert.Null(retrievedEvent.FinalResult);
        }

        [Fact]
        public async Task Round_CanHaveEmptyOptionalFields()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var round = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 1,
                OpponentLeader = null,
                DiceRollResult = null
            };

            // Act
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();

            var retrievedRound = await _context.Rounds.FirstOrDefaultAsync(r => r.Id == round.Id);

            // Assert
            Assert.NotNull(retrievedRound);
            Assert.Null(retrievedRound.OpponentLeader);
            Assert.Null(retrievedRound.DiceRollResult);
        }

        [Fact]
        public async Task Event_CanHaveSpecialCharactersInName()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Event @#$%^&*()_+-=[]{}|;':\",./<>?",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };

            // Act
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var retrievedEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventModel.Id);

            // Assert
            Assert.NotNull(retrievedEvent);
            Assert.Equal("Event @#$%^&*()_+-=[]{}|;':\",./<>?", retrievedEvent.Name);
        }

        [Fact]
        public async Task Event_LastModified_CanBeManuallyUpdated()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var originalLastModified = eventModel.LastModified;

            // Act
            await Task.Delay(10); // Small delay to ensure time difference
            eventModel.Name = "Updated Event";
            eventModel.LastModified = DateTime.UtcNow; // Manually update
            await _context.SaveChangesAsync();

            var retrievedEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventModel.Id);

            // Assert
            Assert.NotNull(retrievedEvent);
            Assert.True(retrievedEvent.LastModified > originalLastModified);
        }

        [Fact]
        public async Task Round_LastModified_CanBeManuallyUpdated()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                OAuthProvider = "Google",
                OAuthProviderUserId = "123"
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

            var eventModel = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();

            var round = new Round
            {
                EventId = eventModel.Id,
                RoundNumber = 1,
                OpponentLeader = "Luffy"
            };
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();

            var originalLastModified = round.LastModified;

            // Act
            await Task.Delay(10); // Small delay to ensure time difference
            round.OpponentLeader = "Zoro";
            round.LastModified = DateTime.UtcNow; // Manually update
            await _context.SaveChangesAsync();

            var retrievedRound = await _context.Rounds.FirstOrDefaultAsync(r => r.Id == round.Id);

            // Assert
            Assert.NotNull(retrievedRound);
            Assert.True(retrievedRound.LastModified > originalLastModified);
        }
    }
}
