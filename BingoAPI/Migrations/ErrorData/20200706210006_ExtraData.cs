using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations.ErrorData
{
    public partial class ExtraData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtraData",
                table: "ErrorLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraData",
                table: "ErrorLogs");
        }
    }
}
