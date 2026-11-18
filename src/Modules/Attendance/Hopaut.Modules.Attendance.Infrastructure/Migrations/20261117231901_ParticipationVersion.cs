using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hopaut.Modules.Attendance.Infrastructure.Migrations
{
    /// <summary>
    /// Plan Step 8.3: <c>Participation.Version</c> maps to PostgreSQL's system column <c>xmin</c> for optimistic concurrency.
    /// Npgsql emits no SQL for it (every row already has <c>xmin</c>); the migration only records the model change. No data conversion.
    /// </summary>
    public partial class ParticipationVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "attendance",
                table: "participations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "attendance",
                table: "participations");
        }
    }
}
