using Microsoft.EntityFrameworkCore;
using OPTCG.Tracker.Core.Models;

namespace OPTCG.Tracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Card> Cards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(100);

                entity.Property(e => e.Preferences)
                    .HasMaxLength(1000);

                entity.Property(e => e.OAuthProvider)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.OAuthProviderUserId)
                    .IsRequired()
                    .HasMaxLength(255);

                // Create unique indexes
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => new { e.OAuthProvider, e.OAuthProviderUserId }).IsUnique();
            });

            // Configure Deck entity
            modelBuilder.Entity<Deck>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.UserId)
                    .IsRequired();

                // Create foreign key relationship
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Leader)
                    .WithMany()
                    .HasForeignKey(d => d.LeaderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Create index for UserId for faster queries
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.LeaderId);
            });

            // Configure Event entity
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Date)
                    .IsRequired();

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.DeckId)
                    .IsRequired();

                entity.Property(e => e.FinalResult)
                    .HasMaxLength(50);

                // Foreign key relationships
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Deck)
                    .WithMany()
                    .HasForeignKey(e => e.DeckId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Rounds)
                    .WithOne(r => r.Event)
                    .HasForeignKey(r => r.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Create indexes for faster queries
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.DeckId);
            });

            // Configure Round entity
            modelBuilder.Entity<Round>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.EventId)
                    .IsRequired();

                entity.Property(e => e.RoundNumber)
                    .IsRequired();

                entity.Property(e => e.OpponentLeader)
                    .HasMaxLength(100);

                entity.Property(e => e.DiceRollResult)
                    .HasMaxLength(10);

                // Create index for EventId and RoundNumber for faster queries
                entity.HasIndex(e => e.EventId);
                entity.HasIndex(e => new { e.EventId, e.RoundNumber });
            });

            // Configure Card entity
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CardType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Color1)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Color2)
                    .HasMaxLength(20);

                entity.Property(e => e.CardNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.Set)
                    .HasMaxLength(20);

                entity.Property(e => e.Rarity)
                    .HasMaxLength(20);

                entity.Property(e => e.Effect)
                    .HasMaxLength(1000);

                entity.Property(e => e.Attribute)
                    .HasMaxLength(20);

                entity.Property(e => e.Type)
                    .HasMaxLength(100);

                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.ThumbnailUrl)
                    .HasMaxLength(500);

                // Create indexes for faster queries
                entity.HasIndex(e => e.CardType);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Set);
                entity.HasIndex(e => new { e.CardType, e.Set });
            });
        }
    }
}
