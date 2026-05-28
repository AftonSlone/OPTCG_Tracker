using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPTCG.Tracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Color2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Life = table.Column<int>(type: "int", nullable: false),
                    Power = table.Column<int>(type: "int", nullable: false),
                    Attribute = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Set = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Rarity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Effect = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leaders_Name",
                table: "Leaders",
                column: "Name");

            migrationBuilder.Sql(@"
                INSERT INTO Leaders (Name, Color1, Color2, Life, Power, Attribute, Type, CardNumber, [Set], Rarity, Effect, CreatedDate, LastModified)
                VALUES
                ('Monkey D. Luffy', 'Red', 'Green', 5, 5000, 'Strike', 'Straw Hat Crew', 'OP01-003', 'OP01', 'Common', '[DON!! x1] [Your Turn] All of your Characters gain +1000 power.', GETDATE(), GETDATE()),
                ('Roronoa Zoro', 'Red', NULL, 5, 5000, 'Slash', 'Straw Hat Crew', 'OP01-001', 'OP01', 'Common', '[DON!! x1] [Your Turn] All of your Characters gain +1000 power.', GETDATE(), GETDATE()),
                ('Trafalgar Law', 'Red', 'Green', 4, 5000, 'Slash', 'Supernovas/Heart Pirates', 'OP01-002', 'OP01', 'Common', '[DON!! x1] [Your Turn] Look at 5 cards from the top of your deck.', GETDATE(), GETDATE()),
                ('Kouzuki Oden', 'Green', NULL, 5, 5000, 'Slash', 'Wano Country', 'OP01-031', 'OP01', 'Super Rare', '[DON!! x1] [Your Turn] Give up to 1 of your opponent''s Characters -2000 power.', GETDATE(), GETDATE()),
                ('Donquixote Doflamingo', 'Blue', NULL, 5, 5000, 'Strike', 'Seven Warlords of the Sea', 'OP01-060', 'OP01', 'Super Rare', '[DON!! x1] [Main] Reveal the top card of your deck.', GETDATE(), GETDATE()),
                ('Kaido', 'Purple', 'Blue', 4, 6000, 'Strike', 'Four Emperors/Beast Pirates', 'OP01-061', 'OP01', 'Super Rare', '[DON!! x1] [Your Turn] When your Leader attacks, add 1 DON!! card.', GETDATE(), GETDATE()),
                ('Crocodile', 'Blue', 'Purple', 4, 5000, 'Strike', 'Seven Warlords of the Sea/Baroque Works', 'OP01-062', 'OP01', 'Super Rare', '[DON!! x1] [Main] Look at the top 3 cards of your deck.', GETDATE(), GETDATE()),
                ('Monkey D. Garp', 'Red', 'Black', 4, 5000, 'Strike', 'Navy', 'OP02-002', 'OP02', 'Common', '[DON!! x1] [Your Turn] Give up to 1 of your opponent''s Characters -2000 power.', GETDATE(), GETDATE()),
                ('Kin''emon', 'Green', NULL, 5, 5000, 'Slash', 'Wano Country', 'OP02-025', 'OP02', 'Common', '[DON!! x1] [Your Turn] Play up to 1 Wano Character card.', GETDATE(), GETDATE()),
                ('Magellan', 'Purple', NULL, 5, 5000, 'Strike', 'Impel Down', 'OP02-071', 'OP02', 'Common', '[DON!! x1] [Your Turn] Give up to 1 of your opponent''s Characters -2000 power.', GETDATE(), GETDATE()),
                ('Zephyr', 'Black', 'Purple', 4, 5000, 'Strike', 'Neo Navy', 'OP02-072', 'OP02', 'Common', '[DON!! x1] [Your Turn] All of your Characters gain +1000 power.', GETDATE(), GETDATE()),
                ('Smoker', 'Black', NULL, 5, 5000, 'Strike', 'Navy', 'OP02-093', 'OP02', 'Common', '[DON!! x1] [Your Turn] Give up to 1 of your opponent''s Characters -2000 power.', GETDATE(), GETDATE()),
                ('Sanji', 'Green', 'Blue', 4, 5000, 'Strike', 'Straw Hat Crew', 'OP02-026', 'OP02', 'Common', '[DON!! x1] [Your Turn] All of your Characters gain +1000 power.', GETDATE(), GETDATE()),
                ('Emporio Ivankov', 'Blue', NULL, 5, 5000, 'Wisdom', 'Revolutionary Army', 'OP02-049', 'OP02', 'Common', '[DON!! x1] [Your Turn] Look at 5 cards from the top of your deck.', GETDATE(), GETDATE()),
                ('Portgas D. Ace', 'Red', NULL, 5, 5000, 'Strike', 'Whitebeard Pirates', 'OP02-013', 'OP02', 'Common', '[DON!! x1] [On Play] Give up to 2 of your opponent''s Characters -3000 power.', GETDATE(), GETDATE()),
                ('Edward Newgate', 'Red', NULL, 5, 6000, 'Strike', 'Four Emperors/Whitebeard Pirates', 'OP02-001', 'OP02', 'Super Rare', '[DON!! x1] [Your Turn] All of your Characters gain +1000 power.', GETDATE(), GETDATE()),
                ('Nami', 'Blue', NULL, 5, 5000, 'Wisdom', 'Straw Hat Crew', 'OP03-040', 'OP03', 'Common', '[DON!! x1] [Your Turn] Look at 5 cards from the top of your deck.', GETDATE(), GETDATE()),
                ('Usopp', 'Green', NULL, 5, 5000, 'Ranged', 'Straw Hat Crew', 'OP03-041', 'OP03', 'Common', '[DON!! x1] [Your Turn] Give up to 1 of your opponent''s Characters -2000 power.', GETDATE(), GETDATE()),
                ('Tony Tony Chopper', 'Green', NULL, 5, 5000, 'Strike', 'Straw Hat Crew', 'OP03-042', 'OP03', 'Common', '[DON!! x1] [Your Turn] All of your Characters gain +1000 power.', GETDATE(), GETDATE()),
                ('Nico Robin', 'Purple', NULL, 5, 5000, 'Wisdom', 'Straw Hat Crew', 'OP03-043', 'OP03', 'Common', '[DON!! x1] [Your Turn] Look at 5 cards from the top of your deck.', GETDATE(), GETDATE())
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leaders");
        }
    }
}
