using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPTCG.Tracker.Data;
using OPTCG.Tracker.Core.Models;

namespace OPTCG.Tracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Leader>>> GetLeaders()
        {
            var leaders = await _context.Leaders
                .OrderBy(l => l.Name)
                .ToListAsync();

            return Ok(leaders);
        }
    }
}
