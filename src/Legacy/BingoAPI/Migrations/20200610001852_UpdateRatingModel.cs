using Microsoft.EntityFrameworkCore.Migrations;

namespace BingoAPI.Migrations
{
    public partial class UpdateRatingModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Frequency",
                table: "RepeatableProperties",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "Rating",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Rating_PostId",
                table: "Rating",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rating_Posts_PostId",
                table: "Rating",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rating_Posts_PostId",
                table: "Rating");

            migrationBuilder.DropIndex(
                name: "IX_Rating_PostId",
                table: "Rating");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "Rating");

            migrationBuilder.AlterColumn<long>(
                name: "Frequency",
                table: "RepeatableProperties",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
