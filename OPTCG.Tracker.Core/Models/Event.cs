using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPTCG.Tracker.Core.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        public int UserId { get; set; }

        [Required]
        public int DeckId { get; set; }

        [MaxLength(50)]
        public string? FinalResult { get; set; }

        public bool IsFinalized { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(DeckId))]
        public Deck? Deck { get; set; }

        public ICollection<Round>? Rounds { get; set; }
    }
}
