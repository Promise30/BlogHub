using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BloggingAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEnumPropertyOfCommentAndVoteEntityWithNullBooleanField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AlterColumn<bool>(
                name: "IsUpVote",
                table: "CommentVote",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {;

            migrationBuilder.AlterColumn<bool>(
                name: "IsUpVote",
                table: "CommentVote",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);


        }
    }
}
