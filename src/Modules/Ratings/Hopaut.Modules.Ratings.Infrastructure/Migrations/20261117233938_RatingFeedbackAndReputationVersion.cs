using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hopaut.Modules.Ratings.Infrastructure.Migrations
{
    /// <summary>
    /// Plan Step 8.4 (D-031): <c>Rating.Feedback</c> becomes the <c>RatingFeedback</c> value object (trimmed, 1–300 characters,
    /// the legacy rule) stored as <c>character varying(300)</c>, and the reputation projection gets an <c>xmin</c> concurrency
    /// token (no SQL). Data conversion before the type change: feedback is trimmed and cut to 300 characters, and blank feedback
    /// becomes NULL, so every stored value passes the factory. <c>Down</c> restores the column type; the normalisation is one-way.
    /// </summary>
    public partial class RatingFeedbackAndReputationVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "ratings",
                table: "user_reputations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.Sql("""
                UPDATE ratings.ratings
                SET "Feedback" = NULLIF(btrim(left(btrim("Feedback"), 300)), '')
                WHERE "Feedback" IS NOT NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Feedback",
                schema: "ratings",
                table: "ratings",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "ratings",
                table: "user_reputations");

            migrationBuilder.AlterColumn<string>(
                name: "Feedback",
                schema: "ratings",
                table: "ratings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);
        }
    }
}
