using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OPTCG.Tracker.Data;
using OPTCG.Tracker.Core.Models;

namespace OPTCG.Tracker.API.Services
{
    public class CardImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://optcgapi.com/api";

        public CardImportService(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<(int imported, int updated, int errors)> ImportCardsAsync(string? set = null)
        {
            int imported = 0;
            int updated = 0;
            int errors = 0;

            try
            {
                // Fetch all sets from OPTCG API
                var setsResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/allSets/");
                setsResponse.EnsureSuccessStatusCode();
                var setsJson = await setsResponse.Content.ReadAsStringAsync();
                var sets = JsonSerializer.Deserialize<List<OptcgSet>>(setsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (sets == null)
                {
                    throw new Exception("Failed to parse sets from OPTCG API");
                }

                // Filter by specific set if provided
                var setsToImport = string.IsNullOrEmpty(set)
                    ? sets
                    : sets.Where(s => s.set_id.Equals(set, StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var optcgSet in setsToImport)
                {
                    // Fetch cards for this set
                    var cardsResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/sets/{optcgSet.set_id}/");
                    cardsResponse.EnsureSuccessStatusCode();
                    var cardsJson = await cardsResponse.Content.ReadAsStringAsync();
                    var optcgCards = JsonSerializer.Deserialize<List<OptcgCard>>(cardsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (optcgCards == null)
                    {
                        Console.WriteLine($"Failed to parse cards for set {optcgSet.set_id}");
                        continue;
                    }

                    foreach (var optcgCard in optcgCards)
                    {
                        try
                        {
                            var existingCard = await _context.Cards
                                .FirstOrDefaultAsync(c => c.CardNumber == optcgCard.card_set_id);

                            var card = MapOptcgCardToCard(optcgCard, optcgSet.set_id);

                            if (existingCard != null)
                            {
                                // Update existing card
                                existingCard.Name = card.Name;
                                existingCard.Color1 = card.Color1;
                                existingCard.Color2 = card.Color2;
                                existingCard.Power = card.Power;
                                existingCard.Life = card.Life;
                                existingCard.Cost = card.Cost;
                                existingCard.Attribute = card.Attribute;
                                existingCard.Type = card.Type;
                                existingCard.Rarity = card.Rarity;
                                existingCard.Effect = card.Effect;
                                existingCard.ImageUrl = card.ImageUrl;
                                existingCard.ThumbnailUrl = card.ThumbnailUrl;
                                existingCard.LastModified = DateTime.UtcNow;
                                updated++;
                            }
                            else
                            {
                                // Add new card
                                _context.Cards.Add(card);
                                imported++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing card {optcgCard.card_set_id}: {ex.Message}");
                            errors++;
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during import: {ex.Message}");
                errors++;
            }

            return (imported, updated, errors);
        }

        private Card MapOptcgCardToCard(OptcgCard optcgCard, string setCode)
        {
            var card = new Card
            {
                CardType = DetermineCardType(optcgCard),
                Name = Truncate(optcgCard.card_name, 100) ?? string.Empty,
                Color1 = Truncate(optcgCard.card_color, 20) ?? string.Empty,
                Color2 = null, // API doesn't provide Color2 separately
                CardNumber = Truncate(optcgCard.card_set_id, 20),
                Set = Truncate(setCode, 20),
                Rarity = Truncate(optcgCard.rarity, 20),
                Effect = Truncate(optcgCard.card_text, 1000),
                Power = ParseInt(optcgCard.card_power),
                Life = ParseInt(optcgCard.life),
                Cost = ParseInt(optcgCard.card_cost),
                Attribute = Truncate(optcgCard.attribute, 500),
                Type = Truncate(optcgCard.sub_types, 500),
                ImageUrl = Truncate(optcgCard.card_image, 1000), // Truncate to reasonable length
                ThumbnailUrl = Truncate(optcgCard.card_image, 1000), // Use same URL since API doesn't provide separate thumbnails
                CreatedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            return card;
        }

        private string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength);
        }

        private int? ParseInt(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (int.TryParse(value, out int result))
                return result;

            return null;
        }

        private string DetermineCardType(OptcgCard optcgCard)
        {
            // Use the card_type field from API if available
            if (!string.IsNullOrEmpty(optcgCard.card_type))
            {
                return optcgCard.card_type;
            }

            // Fallback to determining type based on properties
            var life = ParseInt(optcgCard.life);
            var cost = ParseInt(optcgCard.card_cost);
            var power = ParseInt(optcgCard.card_power);

            if (life.HasValue && life > 0)
            {
                return "Leader";
            }
            if (cost.HasValue && cost > 0 && power.HasValue)
            {
                return "Character";
            }
            if (cost.HasValue && cost > 0 && !power.HasValue)
            {
                return "Event";
            }
            if (life.HasValue && life > 0 && !power.HasValue)
            {
                return "Stage";
            }
            return "Character"; // Default fallback
        }

    }

    // DTOs for OPTCG API response
    public class OptcgSet
    {
        public string set_id { get; set; } = string.Empty;
        public string set_name { get; set; } = string.Empty;
    }

    public class OptcgCard
    {
        public string? card_set_id { get; set; }
        public string? card_name { get; set; }
        public string? card_color { get; set; }
        public string? card_type { get; set; }
        public string? card_power { get; set; }
        public string? life { get; set; }
        public string? card_cost { get; set; }
        public string? attribute { get; set; }
        public string? sub_types { get; set; }
        public string? rarity { get; set; }
        public string? card_text { get; set; }
        public string? card_image { get; set; }
    }
}
