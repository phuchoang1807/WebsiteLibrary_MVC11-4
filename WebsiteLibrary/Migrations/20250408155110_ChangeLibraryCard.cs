using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteLibrary.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLibraryCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardPhotoUrl",
                table: "LibraryCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "LibraryCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardPhotoUrl",
                table: "LibraryCards");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LibraryCards");
        }
    }
}
