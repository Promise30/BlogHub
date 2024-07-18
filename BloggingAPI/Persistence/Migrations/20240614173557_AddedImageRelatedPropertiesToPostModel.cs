using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BloggingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageRelatedPropertiesToPostModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<string>(
                name: "ImageFormat",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "");

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.DropColumn(
                name: "ImageFormat",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "BlogPosts");

            
        }
    }
}
