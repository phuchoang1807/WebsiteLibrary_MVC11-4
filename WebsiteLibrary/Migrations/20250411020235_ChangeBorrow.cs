using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteLibrary.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBorrow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CardID",
                table: "BorrowingSlips",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingSlips_CardID",
                table: "BorrowingSlips",
                column: "CardID");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowingSlips_LibraryCards_CardID",
                table: "BorrowingSlips",
                column: "CardID",
                principalTable: "LibraryCards",
                principalColumn: "CardID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowingSlips_LibraryCards_CardID",
                table: "BorrowingSlips");

            migrationBuilder.DropIndex(
                name: "IX_BorrowingSlips_CardID",
                table: "BorrowingSlips");

            migrationBuilder.AlterColumn<string>(
                name: "CardID",
                table: "BorrowingSlips",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
