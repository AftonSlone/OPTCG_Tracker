using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using OPTCG.Tracker.API.Controllers;
using OPTCG.Tracker.Core.Models;
using OPTCG.Tracker.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OPTCG.Tracker.Tests
{
    public class EventControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IDbContextTransaction _transaction;

        public EventControllerTests()
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

        private void SetUserContext(EventController controller, int userId)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        [Fact]
        public async Task CreateEvent_ReturnsUnauthorized_WhenNoUserId()
        {
            // Arrange
            var controller = new EventController(_context);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };

            var request = new CreateEventRequest
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                DeckId = 1
            };

            // Act
            var result = await controller.CreateEvent(request);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task CreateEvent_ReturnsBadRequest_WhenDeckDoesNotExist()
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

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateEventRequest
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                DeckId = 999 // Non-existent deck
            };

            // Act
            var result = await controller.CreateEvent(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateEvent_ReturnsBadRequest_WhenDeckBelongsToDifferentUser()
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

            var deck = new Deck
            {
                Name = "User2's Deck",
                UserId = user2.Id
            };

            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user1.Id);

            var request = new CreateEventRequest
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                DeckId = deck.Id // Belongs to user2
            };

            // Act
            var result = await controller.CreateEvent(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateEvent_CreatesEventWithRound1_WhenValid()
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

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateEventRequest
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                DeckId = deck.Id
            };

            // Act
            var result = await controller.CreateEvent(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);

            // Verify event was created
            var events = await _context.Events.ToListAsync();
            Assert.Single(events);
            Assert.Equal("Test Event", events[0].Name);
            Assert.Equal(deck.Id, events[0].DeckId);
            Assert.Equal(user.Id, events[0].UserId);

            // Verify Round 1 was auto-created
            var rounds = await _context.Rounds.ToListAsync();
            Assert.Single(rounds);
            Assert.Equal(1, rounds[0].RoundNumber);
            Assert.Equal(events[0].Id, rounds[0].EventId);
        }

        [Fact]
        public async Task CreateEvent_ReturnsEventWithRounds_WhenValid()
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

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateEventRequest
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                DeckId = deck.Id
            };

            // Act
            var result = await controller.CreateEvent(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);
            
            var value = createdResult.Value as dynamic;
            var rounds = value.GetType().GetProperty("Rounds")?.GetValue(value);
            Assert.NotNull(rounds);
        }

        [Fact]
        public async Task CreateEvent_ValidationError_NameExceedsMaxLength()
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

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateEventRequest
            {
                Name = new string('A', 101), // Exceeds MaxLength(100)
                Date = DateTime.UtcNow,
                DeckId = deck.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () => await controller.CreateEvent(request));
        }

        [Fact]
        public async Task CreateEvent_ValidationError_NameIsRequired()
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

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateEventRequest
            {
                Name = null, // Null name instead of empty string
                Date = DateTime.UtcNow,
                DeckId = deck.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () => await controller.CreateEvent(request));
        }

        [Fact]
        public async Task GetEvent_ReturnsUnauthorized_WhenNoUserId()
        {
            // Arrange
            var controller = new EventController(_context);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };

            // Act
            var result = await controller.GetEvent(1);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetEvent_ReturnsNotFound_WhenEventDoesNotExist()
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

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            // Act
            var result = await controller.GetEvent(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetEvent_ReturnsNotFound_WhenEventBelongsToDifferentUser()
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

            var deck = new Deck
            {
                Name = "Test Deck",
                UserId = user1.Id
            };
            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

            var eventEntity = new Event
            {
                Name = "User1's Event",
                Date = DateTime.UtcNow,
                UserId = user1.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user2.Id);

            // Act
            var result = await controller.GetEvent(eventEntity.Id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetEvent_ReturnsEventWithRounds_WhenValid()
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

            var eventEntity = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var round1 = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 1,
                OpponentLeader = "Luffy",
                DiceRollResult = "6",
                WentFirst = true,
                IsWin = true
            };
            var round2 = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 2,
                OpponentLeader = "Zoro",
                DiceRollResult = "3",
                WentFirst = false,
                IsWin = false
            };
            _context.Rounds.AddRange(round1, round2);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            // Act
            var result = await controller.GetEvent(eventEntity.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            var value = okResult.Value as dynamic;
            var rounds = value.GetType().GetProperty("Rounds")?.GetValue(value);
            Assert.NotNull(rounds);
        }

        [Fact]
        public async Task GetEvent_RoundsAreOrderedByRoundNumber()
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

            var eventEntity = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var round2 = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 2,
                OpponentLeader = "Zoro"
            };
            var round1 = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 1,
                OpponentLeader = "Luffy"
            };
            _context.Rounds.AddRange(round2, round1); // Add out of order
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            // Act
            var result = await controller.GetEvent(eventEntity.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            var value = okResult.Value as dynamic;
            var rounds = value.GetType().GetProperty("Rounds")?.GetValue(value);
            Assert.NotNull(rounds);
            
            var roundsList = rounds as System.Collections.IList;
            Assert.NotNull(roundsList);
            
            var firstRound = roundsList[0];
            var firstRoundNumber = firstRound.GetType().GetProperty("RoundNumber")?.GetValue(firstRound);
            Assert.Equal(1, firstRoundNumber);
            
            var secondRound = roundsList[1];
            var secondRoundNumber = secondRound.GetType().GetProperty("RoundNumber")?.GetValue(secondRound);
            Assert.Equal(2, secondRoundNumber);
        }

        [Fact]
        public async Task CreateEvent_SpecialCharactersInName()
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

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateEventRequest
            {
                Name = "Test Event @#$%^&*()",
                Date = DateTime.UtcNow,
                DeckId = deck.Id
            };

            // Act
            var result = await controller.CreateEvent(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);

            var events = await _context.Events.ToListAsync();
            Assert.Equal("Test Event @#$%^&*()", events[0].Name);
        }

        [Fact]
        public async Task CreateEvent_EventDateCanBePast()
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

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateEventRequest
            {
                Name = "Past Event",
                Date = DateTime.UtcNow.AddDays(-7), // Past date
                DeckId = deck.Id
            };

            // Act
            var result = await controller.CreateEvent(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);
        }

        [Fact]
        public async Task CreateEvent_EventDateCanBeFuture()
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

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateEventRequest
            {
                Name = "Future Event",
                Date = DateTime.UtcNow.AddDays(7), // Future date
                DeckId = deck.Id
            };

            // Act
            var result = await controller.CreateEvent(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);
        }

        [Fact]
        public async Task AddRound_ReturnsUnauthorized_WhenNoUserId()
        {
            // Arrange
            var controller = new EventController(_context);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };

            var request = new CreateRoundRequest
            {
                OpponentLeader = "Luffy",
                IsWin = true
            };

            // Act
            var result = await controller.AddRound(1, request);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task AddRound_ReturnsNotFound_WhenEventDoesNotExist()
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

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateRoundRequest
            {
                OpponentLeader = "Luffy",
                IsWin = true
            };

            // Act
            var result = await controller.AddRound(999, request);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddRound_ReturnsBadRequest_WhenEventIsFinalized()
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

            var eventEntity = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id,
                IsFinalized = true
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateRoundRequest
            {
                OpponentLeader = "Luffy",
                IsWin = true
            };

            // Act
            var result = await controller.AddRound(eventEntity.Id, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task AddRound_AutoIncrementsRoundNumber()
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

            var eventEntity = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var round1 = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 1
            };
            _context.Rounds.Add(round1);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new CreateRoundRequest
            {
                OpponentLeader = "Zoro",
                IsWin = false
            };

            // Act
            var result = await controller.AddRound(eventEntity.Id, request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);

            var rounds = await _context.Rounds.Where(r => r.EventId == eventEntity.Id).ToListAsync();
            Assert.Equal(2, rounds.Count);
            Assert.Equal(2, rounds[1].RoundNumber);
        }

        [Fact]
        public async Task UpdateRound_ReturnsBadRequest_WhenEventIsFinalized()
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

            var eventEntity = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id,
                IsFinalized = true
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var round = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 1,
                OpponentLeader = "Luffy"
            };
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new UpdateRoundRequest
            {
                OpponentLeader = "Zoro",
                IsWin = true
            };

            // Act
            var result = await controller.UpdateRound(eventEntity.Id, round.Id, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRound_UpdatesRound_WhenValid()
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

            var eventEntity = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var round = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 1,
                OpponentLeader = "Luffy",
                IsWin = false
            };
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            var request = new UpdateRoundRequest
            {
                OpponentLeader = "Zoro",
                DiceRollResult = "6",
                WentFirst = true,
                IsWin = true
            };

            // Act
            var result = await controller.UpdateRound(eventEntity.Id, round.Id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var updatedRound = await _context.Rounds.FirstOrDefaultAsync(r => r.Id == round.Id);
            Assert.Equal("Zoro", updatedRound.OpponentLeader);
            Assert.Equal("6", updatedRound.DiceRollResult);
            Assert.True(updatedRound.WentFirst);
            Assert.True(updatedRound.IsWin);
        }

        [Fact]
        public async Task DeleteRound_ReturnsBadRequest_WhenEventIsFinalized()
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

            var eventEntity = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id,
                IsFinalized = true
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var round = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 1
            };
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            // Act
            var result = await controller.DeleteRound(eventEntity.Id, round.Id);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteRound_DeletesRound_WhenValid()
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

            var eventEntity = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var round = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 1
            };
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            // Act
            var result = await controller.DeleteRound(eventEntity.Id, round.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var remainingRounds = await _context.Rounds.Where(r => r.EventId == eventEntity.Id).ToListAsync();
            Assert.Empty(remainingRounds);
        }

        [Fact]
        public async Task FinalizeEvent_ReturnsBadRequest_WhenAlreadyFinalized()
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

            var eventEntity = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id,
                IsFinalized = true,
                FinalResult = "2-1"
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            // Act
            var result = await controller.FinalizeEvent(eventEntity.Id);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task FinalizeEvent_CalculatesFinalResult_WhenValid()
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

            var eventEntity = new Event
            {
                Name = "Test Event",
                Date = DateTime.UtcNow,
                UserId = user.Id,
                DeckId = deck.Id,
                IsFinalized = false
            };
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            var round1 = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 1,
                IsWin = true
            };
            var round2 = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 2,
                IsWin = true
            };
            var round3 = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 3,
                IsWin = false
            };
            _context.Rounds.AddRange(round1, round2, round3);
            await _context.SaveChangesAsync();

            var controller = new EventController(_context);
            SetUserContext(controller, user.Id);

            // Act
            var result = await controller.FinalizeEvent(eventEntity.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var finalizedEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventEntity.Id);
            Assert.True(finalizedEvent.IsFinalized);
            Assert.Equal("2-1", finalizedEvent.FinalResult);
        }
    }
}
