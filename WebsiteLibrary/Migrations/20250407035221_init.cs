using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteLibrary.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EducationLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    ReaderCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    OriginalBookID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OriginalBookTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Publisher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PageCount = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PublicationYear = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.OriginalBookID);
                });

            migrationBuilder.CreateTable(
                name: "ImportReceipts",
                columns: table => new
                {
                    ImportReceiptID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportReceipts", x => x.ImportReceiptID);
                });

            migrationBuilder.CreateTable(
                name: "LoanRequests",
                columns: table => new
                {
                    RequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRequests", x => x.RequestID);
                });

            migrationBuilder.CreateTable(
                name: "BorrowingSlips",
                columns: table => new
                {
                    BorrowingSlipID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BorrowDuration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReaderID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowingSlips", x => x.BorrowingSlipID);
                    table.ForeignKey(
                        name: "FK_BorrowingSlips_Accounts_ReaderID",
                        column: x => x.ReaderID,
                        principalTable: "Accounts",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "LibraryCards",
                columns: table => new
                {
                    CardID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryCards", x => x.CardID);
                    table.ForeignKey(
                        name: "FK_LibraryCards_Accounts_ID",
                        column: x => x.ID,
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCopies",
                columns: table => new
                {
                    BookID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalBookID = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    ImportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCopies", x => x.BookID);
                    table.ForeignKey(
                        name: "FK_BookCopies_Books_OriginalBookID",
                        column: x => x.OriginalBookID,
                        principalTable: "Books",
                        principalColumn: "OriginalBookID");
                });

            migrationBuilder.CreateTable(
                name: "ImportDetails",
                columns: table => new
                {
                    ImportReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalBookId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ImportReceiptFK = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalBookFK = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportDetails", x => new { x.ImportReceiptId, x.OriginalBookId });
                    table.ForeignKey(
                        name: "FK_ImportDetails_Books_OriginalBookFK",
                        column: x => x.OriginalBookFK,
                        principalTable: "Books",
                        principalColumn: "OriginalBookID");
                    table.ForeignKey(
                        name: "FK_ImportDetails_ImportReceipts_ImportReceiptFK",
                        column: x => x.ImportReceiptFK,
                        principalTable: "ImportReceipts",
                        principalColumn: "ImportReceiptID");
                });

            migrationBuilder.CreateTable(
                name: "ReturnSlips",
                columns: table => new
                {
                    ReturnSlipID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowingSlipID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnSlips", x => x.ReturnSlipID);
                    table.ForeignKey(
                        name: "FK_ReturnSlips_BorrowingSlips_BorrowingSlipID",
                        column: x => x.BorrowingSlipID,
                        principalTable: "BorrowingSlips",
                        principalColumn: "BorrowingSlipID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardRenewals",
                columns: table => new
                {
                    RenewalID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CardID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NewExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardRenewals", x => x.RenewalID);
                    table.ForeignKey(
                        name: "FK_CardRenewals_LibraryCards_CardID",
                        column: x => x.CardID,
                        principalTable: "LibraryCards",
                        principalColumn: "CardID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CardID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<float>(type: "real", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_Payments_LibraryCards_CardID",
                        column: x => x.CardID,
                        principalTable: "LibraryCards",
                        principalColumn: "CardID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BorrowingSlipDetails",
                columns: table => new
                {
                    BorrowingSlipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookCopyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowingSlipFK = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookCopyFK = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowingSlipDetails", x => new { x.BorrowingSlipId, x.BookCopyId });
                    table.ForeignKey(
                        name: "FK_BorrowingSlipDetails_BookCopies_BookCopyFK",
                        column: x => x.BookCopyFK,
                        principalTable: "BookCopies",
                        principalColumn: "BookID");
                    table.ForeignKey(
                        name: "FK_BorrowingSlipDetails_BorrowingSlips_BorrowingSlipFK",
                        column: x => x.BorrowingSlipFK,
                        principalTable: "BorrowingSlips",
                        principalColumn: "BorrowingSlipID");
                });

            migrationBuilder.CreateTable(
                name: "LoanRequestDetails",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookCopyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestFK = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookCopyFK = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRequestDetails", x => new { x.RequestId, x.BookCopyId });
                    table.ForeignKey(
                        name: "FK_LoanRequestDetails_BookCopies_BookCopyFK",
                        column: x => x.BookCopyFK,
                        principalTable: "BookCopies",
                        principalColumn: "BookID");
                    table.ForeignKey(
                        name: "FK_LoanRequestDetails_LoanRequests_RequestFK",
                        column: x => x.RequestFK,
                        principalTable: "LoanRequests",
                        principalColumn: "RequestID");
                });

            migrationBuilder.CreateTable(
                name: "ReturnSlipDetails",
                columns: table => new
                {
                    ReturnSlipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnSlipFK = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookCopyFK = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Condition = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnSlipDetails", x => new { x.ReturnSlipId, x.BookId });
                    table.ForeignKey(
                        name: "FK_ReturnSlipDetails_BookCopies_BookCopyFK",
                        column: x => x.BookCopyFK,
                        principalTable: "BookCopies",
                        principalColumn: "BookID");
                    table.ForeignKey(
                        name: "FK_ReturnSlipDetails_ReturnSlips_ReturnSlipFK",
                        column: x => x.ReturnSlipFK,
                        principalTable: "ReturnSlips",
                        principalColumn: "ReturnSlipID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookCopies_OriginalBookID",
                table: "BookCopies",
                column: "OriginalBookID");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingSlipDetails_BookCopyFK",
                table: "BorrowingSlipDetails",
                column: "BookCopyFK");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingSlipDetails_BorrowingSlipFK",
                table: "BorrowingSlipDetails",
                column: "BorrowingSlipFK");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingSlips_ReaderID",
                table: "BorrowingSlips",
                column: "ReaderID");

            migrationBuilder.CreateIndex(
                name: "IX_CardRenewals_CardID",
                table: "CardRenewals",
                column: "CardID");

            migrationBuilder.CreateIndex(
                name: "IX_ImportDetails_ImportReceiptFK",
                table: "ImportDetails",
                column: "ImportReceiptFK");

            migrationBuilder.CreateIndex(
                name: "IX_ImportDetails_OriginalBookFK",
                table: "ImportDetails",
                column: "OriginalBookFK");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryCards_ID",
                table: "LibraryCards",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequestDetails_BookCopyFK",
                table: "LoanRequestDetails",
                column: "BookCopyFK");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequestDetails_RequestFK",
                table: "LoanRequestDetails",
                column: "RequestFK");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CardID",
                table: "Payments",
                column: "CardID");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnSlipDetails_BookCopyFK",
                table: "ReturnSlipDetails",
                column: "BookCopyFK");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnSlipDetails_ReturnSlipFK",
                table: "ReturnSlipDetails",
                column: "ReturnSlipFK");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnSlips_BorrowingSlipID",
                table: "ReturnSlips",
                column: "BorrowingSlipID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowingSlipDetails");

            migrationBuilder.DropTable(
                name: "CardRenewals");

            migrationBuilder.DropTable(
                name: "ImportDetails");

            migrationBuilder.DropTable(
                name: "LoanRequestDetails");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ReturnSlipDetails");

            migrationBuilder.DropTable(
                name: "ImportReceipts");

            migrationBuilder.DropTable(
                name: "LoanRequests");

            migrationBuilder.DropTable(
                name: "LibraryCards");

            migrationBuilder.DropTable(
                name: "BookCopies");

            migrationBuilder.DropTable(
                name: "ReturnSlips");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "BorrowingSlips");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
