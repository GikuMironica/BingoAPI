using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class UpdateBugReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Bugs",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "Bugs");
        }
    }
}
