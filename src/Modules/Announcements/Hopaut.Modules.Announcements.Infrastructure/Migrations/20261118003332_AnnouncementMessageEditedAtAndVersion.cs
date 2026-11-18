using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hopaut.Modules.Announcements.Infrastructure.Migrations
{
    /// <summary>
    /// Plan Step 8.5 (D-033, D-034): <c>Announcement.Message</c> becomes the <c>AnnouncementMessage</c> value object (trimmed,
    /// 1–500 characters) stored as <c>character varying(500)</c>, <c>EditedAt</c> records the last edit, and the aggregate gets
    /// an <c>xmin</c> concurrency token (no SQL). Data conversion before the type change, so that every stored message passes
    /// the factory: announcements whose message is blank are deleted (only the module's unvalidated endpoint could store them;
    /// the legacy API required a message), messages are trimmed of every character .NET treats as white space (PostgreSQL's
    /// <c>btrim</c> alone removes spaces only), and messages longer than 500 UTF-16 code units (the unit of
    /// <c>string.Length</c>; PostgreSQL counts code points) are cut to the longest prefix that fits and trimmed again.
    /// <c>Down</c> restores the column type and drops the new columns; the normalisation and the deletions are one-way.
    /// </summary>
    public partial class AnnouncementMessageEditedAtAndVersion : Migration
    {
        private const int MaxLength = 500;

        /// <summary>The characters <see cref="string.Trim()"/> removes (<see cref="char.IsWhiteSpace(char)"/>) as a PostgreSQL text expression.</summary>
        private static readonly string WhiteSpace = string.Join(
            " || ", Enumerable.Range(0, char.MaxValue + 1).Where(c => char.IsWhiteSpace((char)c)).Select(c => $"chr({c})"));

        /// <summary>The length .NET gives a text: one per character plus one per character outside the Basic Multilingual Plane (a surrogate pair).</summary>
        private static string Utf16Length(string text) =>
            $"(char_length({text}) + char_length(regexp_replace({text}, '[^' || chr(65536) || '-' || chr(1114111) || ']', '', 'g')))";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string message = "a.\"Message\"";

            migrationBuilder.Sql($"""
                DELETE FROM announcements.announcements AS a
                WHERE btrim({message}, {WhiteSpace}) = '';
                """);

            migrationBuilder.Sql($"""
                UPDATE announcements.announcements AS a
                SET "Message" = btrim({message}, {WhiteSpace})
                WHERE {message} <> btrim({message}, {WhiteSpace});
                """);

            migrationBuilder.Sql($"""
                UPDATE announcements.announcements AS a
                SET "Message" = btrim(left({message}, (
                        SELECT max(k) FROM generate_series(1, {MaxLength}) AS k
                        WHERE {Utf16Length($"left({message}, k)")} <= {MaxLength})), {WhiteSpace})
                WHERE {Utf16Length(message)} > {MaxLength};
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                schema: "announcements",
                table: "announcements",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EditedAt",
                schema: "announcements",
                table: "announcements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "announcements",
                table: "announcements",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditedAt",
                schema: "announcements",
                table: "announcements");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "announcements",
                table: "announcements");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                schema: "announcements",
                table: "announcements",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
