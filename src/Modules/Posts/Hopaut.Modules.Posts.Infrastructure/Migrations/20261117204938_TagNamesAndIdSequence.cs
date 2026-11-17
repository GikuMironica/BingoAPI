using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hopaut.Modules.Posts.Infrastructure.Migrations
{
    /// <summary>
    /// Plan Step 8.2 (F-065): tags become an aggregate with IDs from the <c>posts.tag_ids</c> sequence (D-019) and a unique,
    /// normalised name (<c>TagName</c>: trimmed, lower-case, 1–20 characters). Data conversion before the column change:
    /// names are trimmed, lower-cased and cut to 20 characters, tags left blank are removed with their links, and duplicates
    /// collapse into the tag with the lowest ID (their posts are re-linked to it). Down restores the identity column and the
    /// text type; the normalisation itself cannot be undone.
    /// </summary>
    public partial class TagNamesAndIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE posts.tags
                SET "TagName" = left(lower(regexp_replace("TagName", '^[[:space:]]+|[[:space:]]+$', '', 'g')), 20);

                DELETE FROM posts.tags WHERE "TagName" = '';

                WITH canonical AS (
                    SELECT "Id", min("Id") OVER (PARTITION BY "TagName") AS "KeepId" FROM posts.tags)
                INSERT INTO posts.post_tags ("PostId", "TagId")
                SELECT link."PostId", canonical."KeepId"
                FROM posts.post_tags AS link
                JOIN canonical ON canonical."Id" = link."TagId"
                WHERE canonical."Id" <> canonical."KeepId"
                ON CONFLICT DO NOTHING;

                DELETE FROM posts.tags AS tag
                USING (SELECT "Id", min("Id") OVER (PARTITION BY "TagName") AS "KeepId" FROM posts.tags) AS canonical
                WHERE tag."Id" = canonical."Id" AND canonical."Id" <> canonical."KeepId";
                """);

            migrationBuilder.CreateSequence<int>(
                name: "tag_ids",
                schema: "posts");

            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                schema: "posts",
                table: "tags",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "posts",
                table: "tags",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Existing tags keep their IDs: the sequence continues after the highest one (D-019).
            migrationBuilder.Sql("""SELECT setval('posts.tag_ids', COALESCE((SELECT MAX("Id") FROM posts.tags), 0) + 1, false);""");

            migrationBuilder.CreateIndex(
                name: "IX_tags_TagName",
                schema: "posts",
                table: "tags",
                column: "TagName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tags_TagName",
                schema: "posts",
                table: "tags");

            migrationBuilder.DropSequence(
                name: "tag_ids",
                schema: "posts");

            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                schema: "posts",
                table: "tags",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "posts",
                table: "tags",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // The restored identity continues after the highest ID.
            migrationBuilder.Sql("""SELECT setval(pg_get_serial_sequence('posts.tags', 'Id'), COALESCE((SELECT MAX("Id") FROM posts.tags), 0) + 1, false);""");
        }
    }
}
