using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPTCG.Tracker.Core.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DisplayName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginDate { get; set; }

        [MaxLength(1000)]
        public string? Preferences { get; set; }

        [Required]
        [MaxLength(50)]
        public string OAuthProvider { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OAuthProviderUserId { get; set; } = string.Empty;

        [NotMapped]
        public OAuthProvider ProviderEnum => Enum.Parse<OAuthProvider>(OAuthProvider);
    }

    public enum OAuthProvider
    {
        Google,
        Microsoft,
        Discord,
        Twitch
    }
}
