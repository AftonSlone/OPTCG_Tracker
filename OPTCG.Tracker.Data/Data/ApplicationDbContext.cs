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

                // Create index for UserId for faster queries
                entity.HasIndex(e => e.UserId);
            });
        }
    }
}
