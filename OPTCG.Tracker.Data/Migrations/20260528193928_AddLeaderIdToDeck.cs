using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPTCG.Tracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaderIdToDeck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LeaderId",
                table: "Decks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Decks_LeaderId",
                table: "Decks",
                column: "LeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Leaders_LeaderId",
                table: "Decks",
                column: "LeaderId",
                principalTable: "Leaders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Leaders_LeaderId",
                table: "Decks");

            migrationBuilder.DropIndex(
                name: "IX_Decks_LeaderId",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "LeaderId",
                table: "Decks");
        }
    }
}
