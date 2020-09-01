using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class ReportChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.Sql("ALTER TABLE public.\"UserReports\" ALTER COLUMN \"Reason\" TYPE integer USING \"Reason\"::integer");
            migrationBuilder.Sql("ALTER TABLE public.\"Reports\" ALTER COLUMN \"Reason\" TYPE integer USING \"Reason\"::integer");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "UserReports",
                type: "text",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Reports",
                type: "text",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
