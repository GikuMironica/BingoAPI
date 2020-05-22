using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class FixBug : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Club_EntrancePrice",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "HouseParty_EntrancePrice",
                table: "Events");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Club_EntrancePrice",
                table: "Events",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HouseParty_EntrancePrice",
                table: "Events",
                type: "double",
                nullable: true);
        }
    }
}
