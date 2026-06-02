using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OPTCG.Tracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeckLeaderForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Leaders_LeaderId",
                table: "Decks");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Cards_LeaderId",
                table: "Decks",
                column: "LeaderId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Cards_LeaderId",
                table: "Decks");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Leaders_LeaderId",
                table: "Decks",
                column: "LeaderId",
                principalTable: "Leaders",
                principalColumn: "Id");
        }
    }
}
