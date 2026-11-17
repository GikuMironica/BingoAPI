using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hopaut.Modules.Posts.Infrastructure.Migrations
{
    /// <summary>
    /// Plan Step 8.2 (6.2): the post's one-to-one children (events, event_locations, repeatable_properties) are keyed by
    /// "PostId" instead of a surrogate "Id" that nothing referenced; the unique "PostId" index becomes the primary key. No data
    /// conversion is needed ("PostId" was already unique and required). Down re-adds identity keys, numbering existing rows anew.
    /// </summary>
    public partial class ChildrenKeyedByPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_repeatable_properties",
                schema: "posts",
                table: "repeatable_properties");

            migrationBuilder.DropIndex(
                name: "IX_repeatable_properties_PostId",
                schema: "posts",
                table: "repeatable_properties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_events",
                schema: "posts",
                table: "events");

            migrationBuilder.DropIndex(
                name: "IX_events_PostId",
                schema: "posts",
                table: "events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_event_locations",
                schema: "posts",
                table: "event_locations");

            migrationBuilder.DropIndex(
                name: "IX_event_locations_PostId",
                schema: "posts",
                table: "event_locations");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "posts",
                table: "repeatable_properties");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "posts",
                table: "events");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "posts",
                table: "event_locations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_repeatable_properties",
                schema: "posts",
                table: "repeatable_properties",
                column: "PostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_events",
                schema: "posts",
                table: "events",
                column: "PostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_event_locations",
                schema: "posts",
                table: "event_locations",
                column: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_repeatable_properties",
                schema: "posts",
                table: "repeatable_properties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_events",
                schema: "posts",
                table: "events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_event_locations",
                schema: "posts",
                table: "event_locations");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "posts",
                table: "repeatable_properties",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "posts",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "posts",
                table: "event_locations",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_repeatable_properties",
                schema: "posts",
                table: "repeatable_properties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_events",
                schema: "posts",
                table: "events",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_event_locations",
                schema: "posts",
                table: "event_locations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_repeatable_properties_PostId",
                schema: "posts",
                table: "repeatable_properties",
                column: "PostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_PostId",
                schema: "posts",
                table: "events",
                column: "PostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_locations_PostId",
                schema: "posts",
                table: "event_locations",
                column: "PostId",
                unique: true);
        }
    }
}
