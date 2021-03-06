using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations.ErrorData
{
    public partial class ServeName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Server",
                table: "ErrorLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Server",
                table: "ErrorLogs");
        }
    }
}
