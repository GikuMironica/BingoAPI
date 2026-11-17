using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hopaut.Modules.Posts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PostIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "post_ids",
                schema: "posts");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "posts",
                table: "posts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Existing rows keep their IDs: the sequence continues after the highest one (plan Step 7.6).
            migrationBuilder.Sql("""SELECT setval('posts.post_ids', COALESCE((SELECT MAX("Id") FROM posts.posts), 0) + 1, false);""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "post_ids",
                schema: "posts");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "posts",
                table: "posts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // The restored identity continues after the highest ID.
            migrationBuilder.Sql("""SELECT setval(pg_get_serial_sequence('posts.posts', 'Id'), COALESCE((SELECT MAX("Id") FROM posts.posts), 0) + 1, false);""");
        }
    }
}
