using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hopaut.Modules.Announcements.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AnnouncementIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "announcement_ids",
                schema: "announcements");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "announcements",
                table: "announcements",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Existing rows keep their IDs: the sequence continues after the highest one (plan Step 7.6).
            migrationBuilder.Sql("""SELECT setval('announcements.announcement_ids', COALESCE((SELECT MAX("Id") FROM announcements.announcements), 0) + 1, false);""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "announcement_ids",
                schema: "announcements");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "announcements",
                table: "announcements",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // The restored identity continues after the highest ID.
            migrationBuilder.Sql("""SELECT setval(pg_get_serial_sequence('announcements.announcements', 'Id'), COALESCE((SELECT MAX("Id") FROM announcements.announcements), 0) + 1, false);""");
        }
    }
}
