using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPTCG.Tracker.Core.Models
{
    public class Round
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int RoundNumber { get; set; }

        [MaxLength(100)]
        public string? OpponentLeader { get; set; }

        [MaxLength(10)]
        public string? DiceRollResult { get; set; }

        public bool WentFirst { get; set; } = false;

        public bool IsWin { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }
    }
}
