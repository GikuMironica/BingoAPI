using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class UpdateRI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Events_EventId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_EventsLocations_LocationId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_EventId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_LocationId",
                table: "Posts");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Posts_Id",
                table: "Events",
                column: "Id",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventsLocations_Posts_Id",
                table: "EventsLocations",
                column: "Id",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Posts_Id",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_EventsLocations_Posts_Id",
                table: "EventsLocations");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_EventId",
                table: "Posts",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_LocationId",
                table: "Posts",
                column: "LocationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Events_EventId",
                table: "Posts",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_EventsLocations_LocationId",
                table: "Posts",
                column: "LocationId",
                principalTable: "EventsLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
