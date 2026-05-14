using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class Update2Report : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportedHostId",
                table: "Reports",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportedHostId",
                table: "Reports",
                column: "ReportedHostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_ReportedHostId",
                table: "Reports",
                column: "ReportedHostId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_ReportedHostId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ReportedHostId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportedHostId",
                table: "Reports");
        }
    }
}
