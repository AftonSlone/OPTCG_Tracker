using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPTCG.Tracker.Core.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string CardType { get; set; } = string.Empty; // "Leader", "Character", "Event", "Stage"

        // Common fields
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Color1 { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Color2 { get; set; }

        [MaxLength(20)]
        public string? CardNumber { get; set; }

        [MaxLength(20)]
        public string? Set { get; set; }

        [MaxLength(20)]
        public string? Rarity { get; set; }

        [MaxLength(1000)]
        public string? Effect { get; set; }

        // Type-specific fields (nullable based on card type)
        public int? Power { get; set; }  // Leader/Character
        public int? Life { get; set; }  // Leader/Stage
        public int? Cost { get; set; }  // Character/Stage
        public string? Attribute { get; set; }  // Leader/Character
        public string? Type { get; set; }  // Character/Event

        // Image fields
        public string? ImageUrl { get; set; }

        public string? ThumbnailUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
