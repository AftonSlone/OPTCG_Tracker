using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPTCG.Tracker.Core.Models
{
    public class Leader
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Color1 { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Color2 { get; set; }

        [Required]
        public int Life { get; set; }

        [Required]
        public int Power { get; set; }

        [Required]
        [MaxLength(20)]
        public string Attribute { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? CardNumber { get; set; }

        [MaxLength(20)]
        public string? Set { get; set; }

        [MaxLength(20)]
        public string? Rarity { get; set; }

        [MaxLength(1000)]
        public string? Effect { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
