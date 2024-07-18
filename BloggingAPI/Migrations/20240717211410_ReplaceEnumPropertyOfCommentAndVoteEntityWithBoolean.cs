using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BloggingAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEnumPropertyOfCommentAndVoteEntityWithBoolean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentVote_AspNetUsers_UserId",
                table: "CommentVote");

            

            migrationBuilder.DropColumn(
                name: "DownVoteCount",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "UpVoteCount",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "VoteType",
                table: "CommentVote");

            migrationBuilder.AddColumn<bool>(
                name: "IsUpVote",
                table: "CommentVote",
                type: "bit",
                nullable: false,
                defaultValue: false);

          
            migrationBuilder.AddForeignKey(
                name: "FK_CommentVote_AspNetUsers_UserId",
                table: "CommentVote",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentVote_AspNetUsers_UserId",
                table: "CommentVote");

         
            migrationBuilder.DropColumn(
                name: "IsUpVote",
                table: "CommentVote");

            migrationBuilder.AddColumn<int>(
                name: "DownVoteCount",
                table: "PostComments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpVoteCount",
                table: "PostComments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VoteType",
                table: "CommentVote",
                type: "int",
                nullable: false,
                defaultValue: 0);

          
         
            migrationBuilder.AddForeignKey(
                name: "FK_CommentVote_AspNetUsers_UserId",
                table: "CommentVote",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
