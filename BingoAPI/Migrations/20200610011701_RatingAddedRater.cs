using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class RatingAddedRater : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RaterId",
                table: "Rating",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rating_RaterId",
                table: "Rating",
                column: "RaterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rating_AspNetUsers_RaterId",
                table: "Rating",
                column: "RaterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rating_AspNetUsers_RaterId",
                table: "Rating");

            migrationBuilder.DropIndex(
                name: "IX_Rating_RaterId",
                table: "Rating");

            migrationBuilder.DropColumn(
                name: "RaterId",
                table: "Rating");
        }
    }
}
