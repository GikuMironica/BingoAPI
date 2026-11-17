using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hopaut.Modules.Attendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ParticipationIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "participation_ids",
                schema: "attendance");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "attendance",
                table: "participations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Existing rows keep their IDs: the sequence continues after the highest one (plan Step 7.6).
            migrationBuilder.Sql("""SELECT setval('attendance.participation_ids', COALESCE((SELECT MAX("Id") FROM attendance.participations), 0) + 1, false);""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "participation_ids",
                schema: "attendance");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "attendance",
                table: "participations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // The restored identity continues after the highest ID.
            migrationBuilder.Sql("""SELECT setval(pg_get_serial_sequence('attendance.participations', 'Id'), COALESCE((SELECT MAX("Id") FROM attendance.participations), 0) + 1, false);""");
        }
    }
}
