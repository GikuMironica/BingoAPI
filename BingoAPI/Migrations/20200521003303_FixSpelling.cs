using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class FixSpelling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntracePrice",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Club_EntracePrice",
                table: "Events");

            migrationBuilder.AddColumn<double>(
                name: "Club_EntrancePrice",
                table: "Events",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HouseParty_EntrancePrice",
                table: "Events",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Club_EntrancePrice",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "HouseParty_EntrancePrice",
                table: "Events");

            migrationBuilder.AddColumn<double>(
                name: "EntracePrice",
                table: "Events",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Club_EntracePrice",
                table: "Events",
                type: "double",
                nullable: true);
        }
    }
}
