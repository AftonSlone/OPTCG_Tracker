using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPTCG.Tracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCardsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Color2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CardNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Set = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Rarity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Effect = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Power = table.Column<int>(type: "int", nullable: true),
                    Life = table.Column<int>(type: "int", nullable: true),
                    Cost = table.Column<int>(type: "int", nullable: true),
                    Attribute = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CardType",
                table: "Cards",
                column: "CardType");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CardType_Set",
                table: "Cards",
                columns: new[] { "CardType", "Set" });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Name",
                table: "Cards",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Set",
                table: "Cards",
                column: "Set");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cards");
        }
    }
}
