using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class ModelsAlteredPostTimeToLong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                table: "Tags",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "PostTime",
                table: "Posts",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<long>(
                name: "EventTime",
                table: "Posts",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                table: "Tags",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PostTime",
                table: "Posts",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventTime",
                table: "Posts",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(long));
        }
    }
}
