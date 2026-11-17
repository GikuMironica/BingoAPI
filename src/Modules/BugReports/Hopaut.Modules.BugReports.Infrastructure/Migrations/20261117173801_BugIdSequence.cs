using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hopaut.Modules.BugReports.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BugIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "bug_ids",
                schema: "bug_reports");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "bug_reports",
                table: "bugs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Existing rows keep their IDs: the sequence continues after the highest one (plan Step 7.6).
            migrationBuilder.Sql("""SELECT setval('bug_reports.bug_ids', COALESCE((SELECT MAX("Id") FROM bug_reports.bugs), 0) + 1, false);""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "bug_ids",
                schema: "bug_reports");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "bug_reports",
                table: "bugs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // The restored identity continues after the highest ID.
            migrationBuilder.Sql("""SELECT setval(pg_get_serial_sequence('bug_reports.bugs', 'Id'), COALESCE((SELECT MAX("Id") FROM bug_reports.bugs), 0) + 1, false);""");
        }
    }
}
