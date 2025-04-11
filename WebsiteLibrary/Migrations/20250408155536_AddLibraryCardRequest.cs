using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryCardRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_LibraryCards_CardID",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "CardID",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "LibraryCardRequests",
                columns: table => new
                {
                    RequestID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccountID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReaderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CardPhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryCardRequests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_LibraryCardRequests_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryCardRequests_AccountID",
                table: "LibraryCardRequests",
                column: "AccountID");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_LibraryCards_CardID",
                table: "Payments",
                column: "CardID",
                principalTable: "LibraryCards",
                principalColumn: "CardID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_LibraryCards_CardID",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "LibraryCardRequests");

            migrationBuilder.AlterColumn<string>(
                name: "CardID",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_LibraryCards_CardID",
                table: "Payments",
                column: "CardID",
                principalTable: "LibraryCards",
                principalColumn: "CardID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
