using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class ExtendModelAddActiveEventEndTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveFlag",
                table: "Posts",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "EndTime",
                table: "Posts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveFlag",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Posts");
        }
    }
}
