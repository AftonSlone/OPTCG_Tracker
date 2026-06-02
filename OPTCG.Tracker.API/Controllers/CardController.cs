using Microsoft.AspNetCore.Mvc;
using OPTCG.Tracker.API.Services;

namespace OPTCG.Tracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardController : ControllerBase
    {
        private readonly CardImportService _cardImportService;

        public CardController(CardImportService cardImportService)
        {
            _cardImportService = cardImportService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportCards([FromQuery] string? set = null)
        {
            try
            {
                var (imported, updated, errors) = await _cardImportService.ImportCardsAsync(set);

                return Ok(new
                {
                    message = "Card import completed",
                    imported,
                    updated,
                    errors,
                    total = imported + updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during card import", error = ex.Message });
            }
        }
    }
}
