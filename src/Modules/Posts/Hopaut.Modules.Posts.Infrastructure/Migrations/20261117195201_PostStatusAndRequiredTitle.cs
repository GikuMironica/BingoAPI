using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hopaut.Modules.Posts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PostStatusAndRequiredTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActiveFlag",
                schema: "posts",
                table: "posts",
                newName: "Status");

            // Data conversion (plan Step 8.2, D-025): every stored title must satisfy PostTitle (trimmed, 1-30 characters).
            // The legacy API enforced that; only rows created by the module before this step can be blank or longer.
            migrationBuilder.Sql("""
                UPDATE posts.events
                SET "Title" = CASE WHEN btrim(coalesce("Title", '')) = '' THEN 'Untitled event' ELSE left(btrim("Title"), 30) END
                WHERE "Title" IS NULL OR btrim("Title") = '' OR length(btrim("Title")) > 30;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "posts",
                table: "events",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "posts",
                table: "posts",
                newName: "ActiveFlag");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "posts",
                table: "events",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
