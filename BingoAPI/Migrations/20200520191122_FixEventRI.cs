using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class FixEventRI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Posts_Id",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_EventsLocations_Posts_Id",
                table: "EventsLocations");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Posts");

            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "EventsLocations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "Events",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EventsLocations_PostId",
                table: "EventsLocations",
                column: "PostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_PostId",
                table: "Events",
                column: "PostId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Posts_PostId",
                table: "Events",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventsLocations_Posts_PostId",
                table: "EventsLocations",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Posts_PostId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_EventsLocations_Posts_PostId",
                table: "EventsLocations");

            migrationBuilder.DropIndex(
                name: "IX_EventsLocations_PostId",
                table: "EventsLocations");

            migrationBuilder.DropIndex(
                name: "IX_Events_PostId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "EventsLocations");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "Events");

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
    }
}
