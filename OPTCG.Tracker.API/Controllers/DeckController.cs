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
    public class DeckController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DeckController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeck([FromBody] CreateDeckRequest request)
        {
            var userId = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var deck = new Deck
            {
                Name = request.Name,
                UserId = int.Parse(userId),
                LeaderId = request.LeaderId,
                CreatedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                deck.Id,
                deck.Name,
                deck.UserId,
                deck.LeaderId,
                deck.CreatedDate,
                deck.LastModified
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetDecks()
        {
            var userId = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var decks = await _context.Decks
                .Include(d => d.Leader)
                .Where(d => d.UserId == int.Parse(userId))
                .OrderBy(d => d.Name)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.UserId,
                    d.LeaderId,
                    Leader = d.Leader != null ? new
                    {
                        d.Leader.Id,
                        d.Leader.Name,
                        d.Leader.CardNumber,
                        d.Leader.Color1,
                        d.Leader.Color2
                    } : null,
                    d.CreatedDate,
                    d.LastModified
                })
                .ToListAsync();

            return Ok(decks);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeck(int id, [FromBody] UpdateDeckRequest request)
        {
            var userId = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var deck = await _context.Decks
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == int.Parse(userId));

            if (deck == null)
            {
                return NotFound();
            }

            deck.Name = request.Name;
            deck.LeaderId = request.LeaderId;
            deck.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                deck.Id,
                deck.Name,
                deck.UserId,
                deck.LeaderId,
                deck.CreatedDate,
                deck.LastModified
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeck(int id)
        {
            var userId = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var deck = await _context.Decks
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == int.Parse(userId));

            if (deck == null)
            {
                return NotFound();
            }

            _context.Decks.Remove(deck);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deck deleted successfully" });
        }
    }

    public class CreateDeckRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? LeaderId { get; set; }
    }

    public class UpdateDeckRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? LeaderId { get; set; }
    }
}
