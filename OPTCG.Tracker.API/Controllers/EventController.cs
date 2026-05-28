using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPTCG.Tracker.Data;
using OPTCG.Tracker.Core.Models;

namespace OPTCG.Tracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new event with auto-created Round 1
        /// </summary>
        /// <param name="request">Event creation request</param>
        /// <returns>Created event with Round 1 data</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Validate that deck belongs to the user
            var deck = await _context.Decks
                .FirstOrDefaultAsync(d => d.Id == request.DeckId && d.UserId == int.Parse(userId));

            if (deck == null)
            {
                return BadRequest(new { message = "Deck not found or does not belong to user" });
            }

            var eventEntity = new Event
            {
                Name = request.Name,
                Date = request.Date,
                UserId = int.Parse(userId),
                DeckId = request.DeckId,
                CreatedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            // Auto-create Round 1 with default values
            var round1 = new Round
            {
                EventId = eventEntity.Id,
                RoundNumber = 1,
                OpponentLeader = null,
                DiceRollResult = null,
                WentFirst = false,
                IsWin = false,
                CreatedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _context.Rounds.Add(round1);
            await _context.SaveChangesAsync();

            // Reload event with rounds
            await _context.Entry(eventEntity)
                .Collection(e => e.Rounds)
                .LoadAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventEntity.Id },
                new
                {
                    eventEntity.Id,
                    eventEntity.Name,
                    eventEntity.Date,
                    eventEntity.UserId,
                    eventEntity.DeckId,
                    eventEntity.FinalResult,
                    eventEntity.IsFinalized,
                    eventEntity.CreatedDate,
                    eventEntity.LastModified,
                    Rounds = eventEntity.Rounds?.Select(r => new
                    {
                        r.Id,
                        r.EventId,
                        r.RoundNumber,
                        r.OpponentLeader,
                        r.DiceRollResult,
                        r.WentFirst,
                        r.IsWin,
                        r.CreatedDate,
                        r.LastModified
                    }).ToList()
                });
        }

        /// <summary>
        /// Get event by ID with all rounds
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>Event with all rounds</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEvent(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var eventEntity = await _context.Events
                .Include(e => e.Rounds)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == int.Parse(userId));

            if (eventEntity == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                eventEntity.Id,
                eventEntity.Name,
                eventEntity.Date,
                eventEntity.UserId,
                eventEntity.DeckId,
                eventEntity.FinalResult,
                eventEntity.IsFinalized,
                eventEntity.CreatedDate,
                eventEntity.LastModified,
                Rounds = eventEntity.Rounds?.Select(r => new
                {
                    r.Id,
                    r.EventId,
                    r.RoundNumber,
                    r.OpponentLeader,
                    r.DiceRollResult,
                    r.WentFirst,
                    r.IsWin,
                    r.CreatedDate,
                    r.LastModified
                }).OrderBy(r => r.RoundNumber).ToList()
            });
        }
    }

    public class CreateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int DeckId { get; set; }
    }
}
