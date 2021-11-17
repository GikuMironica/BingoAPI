using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class AddIndexOnEventTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Posts_EndTime",
                table: "Posts",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_EventTime",
                table: "Posts",
                column: "EventTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posts_EndTime",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_EventTime",
                table: "Posts");
        }
    }
}
