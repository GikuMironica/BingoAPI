using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hopaut.Modules.Moderation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReportIdSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "post_report_ids",
                schema: "moderation");

            migrationBuilder.CreateSequence<int>(
                name: "user_report_ids",
                schema: "moderation");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "moderation",
                table: "user_reports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "moderation",
                table: "post_reports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Existing rows keep their IDs: the sequence continues after the highest one (plan Step 7.6).
            migrationBuilder.Sql("""SELECT setval('moderation.post_report_ids', COALESCE((SELECT MAX("Id") FROM moderation.post_reports), 0) + 1, false);""");
            migrationBuilder.Sql("""SELECT setval('moderation.user_report_ids', COALESCE((SELECT MAX("Id") FROM moderation.user_reports), 0) + 1, false);""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "post_report_ids",
                schema: "moderation");

            migrationBuilder.DropSequence(
                name: "user_report_ids",
                schema: "moderation");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "moderation",
                table: "user_reports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "moderation",
                table: "post_reports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // The restored identity continues after the highest ID.
            migrationBuilder.Sql("""SELECT setval(pg_get_serial_sequence('moderation.post_reports', 'Id'), COALESCE((SELECT MAX("Id") FROM moderation.post_reports), 0) + 1, false);""");
            migrationBuilder.Sql("""SELECT setval(pg_get_serial_sequence('moderation.user_reports', 'Id'), COALESCE((SELECT MAX("Id") FROM moderation.user_reports), 0) + 1, false);""");
        }
    }
}
