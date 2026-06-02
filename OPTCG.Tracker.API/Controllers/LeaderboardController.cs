using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OPTCG.Tracker.Data;
using OPTCG.Tracker.Core.Models;

namespace OPTCG.Tracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public LeaderboardController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaderboardEntry>>> GetLeaderboard([FromQuery] string timePeriod = "7d")
        {
            // Temporarily disable cache to ensure thumbnailUrl is included
            // string cacheKey = $"leaderboard_{timePeriod}";
            // 
            // if (_cache.TryGetValue(cacheKey, out List<LeaderboardEntry>? cachedEntries))
            // {
            //     return Ok(cachedEntries);
            // }

            DateTime startDate = GetStartDate(timePeriod);

            var query = from card in _context.Cards
                        where card.CardType == "Leader"
                        join deck in _context.Decks on card.Id equals deck.LeaderId into leaderDecks
                        from deck in leaderDecks.DefaultIfEmpty()
                        join event_ in _context.Events on deck.Id equals event_.DeckId into deckEvents
                        from event_ in deckEvents.DefaultIfEmpty()
                        join round in _context.Rounds on event_.Id equals round.EventId into eventRounds
                        from round in eventRounds.DefaultIfEmpty()
                        where event_ == null || event_.Date >= startDate
                        group new { card, event_, round } by new { card.Id, card.Name, card.ThumbnailUrl } into g
                        select new
                        {
                            LeaderId = g.Key.Id,
                            LeaderName = g.Key.Name,
                            ThumbnailUrl = g.Key.ThumbnailUrl,
                            TotalEvents = g.Count(x => x.event_ != null),
                            TotalRounds = g.Count(x => x.round != null),
                            Wins = g.Count(x => x.round != null && x.round.IsWin)
                        };

            var results = await query.ToListAsync();

            var totalEvents = results.Sum(x => x.TotalEvents);
            var leaderboard = results
                .Where(x => x.TotalEvents > 0)
                .Select(x => new LeaderboardEntry
                {
                    LeaderId = x.LeaderId,
                    LeaderName = x.LeaderName,
                    ThumbnailUrl = x.ThumbnailUrl,
                    PlayRate = totalEvents > 0 ? (double)x.TotalEvents / totalEvents * 100 : 0,
                    WinRate = x.TotalRounds > 0 ? (double)x.Wins / x.TotalRounds * 100 : 0,
                    TotalEvents = x.TotalEvents,
                    TotalRounds = x.TotalRounds,
                    Wins = x.Wins
                })
                .OrderByDescending(x => x.PlayRate)
                .Take(10)
                .ToList();

            // var cacheOptions = new MemoryCacheEntryOptions()
            //     .SetAbsoluteExpiration(_cacheDuration)
            //     .SetSlidingExpiration(TimeSpan.FromHours(12));
            // 
            // _cache.Set(cacheKey, leaderboard, cacheOptions);

            return Ok(leaderboard);
        }

        private DateTime GetStartDate(string timePeriod)
        {
            var now = DateTime.UtcNow;
            return timePeriod.ToLower() switch
            {
                "7d" => now.AddDays(-7),
                "30d" => now.AddDays(-30),
                "ytd" => new DateTime(now.Year, 1, 1),
                "year" => new DateTime(now.Year, 1, 1),
                "all" => DateTime.MinValue,
                _ => now.AddDays(-7)
            };
        }
    }

    public class LeaderboardEntry
    {
        public int LeaderId { get; set; }
        public string LeaderName { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public double PlayRate { get; set; }
        public double WinRate { get; set; }
        public int TotalEvents { get; set; }
        public int TotalRounds { get; set; }
        public int Wins { get; set; }
    }
}
