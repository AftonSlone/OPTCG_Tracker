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

        /// <summary>
        /// Add a new round to an event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="request">Round creation request</param>
        /// <returns>Created round</returns>
        [HttpPost("{eventId}/round")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddRound(int eventId, [FromBody] CreateRoundRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId && e.UserId == int.Parse(userId));

            if (eventEntity == null)
            {
                return NotFound();
            }

            if (eventEntity.IsFinalized)
            {
                return BadRequest(new { message = "Cannot add rounds to a finalized event" });
            }

            // Auto-increment round number
            var maxRoundNumber = await _context.Rounds
                .Where(r => r.EventId == eventId)
                .MaxAsync(r => (int?)r.RoundNumber) ?? 0;

            var round = new Round
            {
                EventId = eventId,
                RoundNumber = maxRoundNumber + 1,
                OpponentLeader = request.OpponentLeader,
                DiceRollResult = request.DiceRollResult,
                WentFirst = request.WentFirst,
                IsWin = request.IsWin,
                CreatedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventId },
                new
                {
                    round.Id,
                    round.EventId,
                    round.RoundNumber,
                    round.OpponentLeader,
                    round.DiceRollResult,
                    round.WentFirst,
                    round.IsWin,
                    round.CreatedDate,
                    round.LastModified
                });
        }

        /// <summary>
        /// Update an existing round
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="roundId">Round ID</param>
        /// <param name="request">Round update request</param>
        /// <returns>Updated round</returns>
        [HttpPut("{eventId}/round/{roundId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRound(int eventId, int roundId, [FromBody] UpdateRoundRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId && e.UserId == int.Parse(userId));

            if (eventEntity == null)
            {
                return NotFound();
            }

            if (eventEntity.IsFinalized)
            {
                return BadRequest(new { message = "Cannot update rounds in a finalized event" });
            }

            var round = await _context.Rounds
                .FirstOrDefaultAsync(r => r.Id == roundId && r.EventId == eventId);

            if (round == null)
            {
                return NotFound();
            }

            round.OpponentLeader = request.OpponentLeader;
            round.DiceRollResult = request.DiceRollResult;
            round.WentFirst = request.WentFirst;
            round.IsWin = request.IsWin;
            round.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                round.Id,
                round.EventId,
                round.RoundNumber,
                round.OpponentLeader,
                round.DiceRollResult,
                round.WentFirst,
                round.IsWin,
                round.CreatedDate,
                round.LastModified
            });
        }

        /// <summary>
        /// Delete a round from an event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="roundId">Round ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{eventId}/round/{roundId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteRound(int eventId, int roundId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId && e.UserId == int.Parse(userId));

            if (eventEntity == null)
            {
                return NotFound();
            }

            if (eventEntity.IsFinalized)
            {
                return BadRequest(new { message = "Cannot delete rounds from a finalized event" });
            }

            var round = await _context.Rounds
                .FirstOrDefaultAsync(r => r.Id == roundId && r.EventId == eventId);

            if (round == null)
            {
                return NotFound();
            }

            _context.Rounds.Remove(round);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Finalize an event and calculate final result
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Finalized event with result</returns>
        [HttpPut("{eventId}/finalize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FinalizeEvent(int eventId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var eventEntity = await _context.Events
                .Include(e => e.Rounds)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.UserId == int.Parse(userId));

            if (eventEntity == null)
            {
                return NotFound();
            }

            if (eventEntity.IsFinalized)
            {
                return BadRequest(new { message = "Event is already finalized" });
            }

            // Calculate final result (wins-losses-draws)
            var wins = eventEntity.Rounds?.Count(r => r.IsWin) ?? 0;
            var losses = eventEntity.Rounds?.Count(r => !r.IsWin) ?? 0;
            eventEntity.FinalResult = $"{wins}-{losses}";
            eventEntity.IsFinalized = true;
            eventEntity.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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
                eventEntity.LastModified
            });
        }
    }

    public class CreateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int DeckId { get; set; }
    }

    public class CreateRoundRequest
    {
        public string? OpponentLeader { get; set; }
        public string? DiceRollResult { get; set; }
        public bool WentFirst { get; set; } = false;
        public bool IsWin { get; set; } = false;
    }

    public class UpdateRoundRequest
    {
        public string? OpponentLeader { get; set; }
        public string? DiceRollResult { get; set; }
        public bool WentFirst { get; set; }
        public bool IsWin { get; set; }
    }
}
