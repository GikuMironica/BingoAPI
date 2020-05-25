using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BingoAPI.Migrations
{
    public partial class UpdatedDomainModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventsLocations");

            migrationBuilder.DropIndex(
                name: "IX_EventLocation_PostId",
                table: "EventLocation");

            migrationBuilder.DropColumn(
                name: "Contry",
                table: "EventLocation");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "EventLocation",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "EventLocation",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventLocation_PostId",
                table: "EventLocation",
                column: "PostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventLocation_Coords",
                table: "EventLocation",
                column: "Location");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventLocation_PostId",
                table: "EventLocation");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "EventLocation");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "EventLocation");

            migrationBuilder.AddColumn<string>(
                name: "Contry",
                table: "EventLocation",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventsLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Logitude = table.Column<double>(type: "double precision", nullable: true),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    Region = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventsLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventsLocations_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventLocation_PostId",
                table: "EventLocation",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_EventsLocations_PostId",
                table: "EventsLocations",
                column: "PostId",
                unique: true);
        }
    }
}
