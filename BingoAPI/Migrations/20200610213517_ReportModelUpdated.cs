using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class ReportModelUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReporterEmail",
                table: "Reports");

            migrationBuilder.AddColumn<string>(
                name: "ReporterId",
                table: "Reports",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterId",
                table: "Reports",
                column: "ReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_ReporterId",
                table: "Reports",
                column: "ReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_ReporterId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ReporterId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReporterId",
                table: "Reports");

            migrationBuilder.AddColumn<string>(
                name: "ReporterEmail",
                table: "Reports",
                type: "text",
                nullable: true);
        }
    }
}
