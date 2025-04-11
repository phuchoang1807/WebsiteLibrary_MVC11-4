using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteLibrary.Data;
using WebsiteLibrary.Models.Entities;

namespace WebsiteLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ImportReceiptController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportReceiptController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ImportReceipt()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetImportReceipts()
        {
            var receipts = await _context.ImportReceipts
                .Include(ir => ir.ImportDetails)
                .ThenInclude(id => id.Book)
                .Select(ir => new
                {
                    receiptId = ir.ImportReceiptID.ToString(),
                    createdAt = ir.ImportDate.ToString("dd/MM/yyyy"),
                    supplier = ir.Supplier,
                    details = ir.ImportDetails.Select(d => new
                    {
                        bookId = d.OriginalBookId,
                        quantity = d.Quantity,
                        price = d.Price
                    }).ToList()
                })
                .ToListAsync();

            return Json(receipts);
        }


        [HttpPost]
        public async Task<IActionResult> AddImportReceipt([FromBody] ImportReceiptRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors });
            }

            if (!DateTime.TryParseExact(request.CreatedAt, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var createdAt))
            {
                return BadRequest(new { success = false, message = "Ngày nhập không hợp lệ! Định dạng phải là DD/MM/YYYY." });
            }

            if (createdAt.Year != 2025)
            {
                return BadRequest(new { success = false, message = "Năm nhập phải là 2025!" });
            }

            if (request.Details == null || !request.Details.Any())
            {
                return BadRequest(new { success = false, message = "Phải có ít nhất một đầu sách!" });
            }

            var bookIds = request.Details.Select(d => d.BookId).ToList();
            var booksExist = await _context.Books
                .Where(b => bookIds.Contains(b.OriginalBookID))
                .Select(b => b.OriginalBookID)
                .ToListAsync();

            foreach (var detail in request.Details)
            {
                if (!booksExist.Contains(detail.BookId))
                {
                    return BadRequest(new { success = false, message = $"Mã đầu sách {detail.BookId} không tồn tại!" });
                }
                if (detail.Quantity < 1)
                {
                    return BadRequest(new { success = false, message = "Số lượng phải lớn hơn 0!" });
                }
                if (detail.Price < 0)
                {
                    return BadRequest(new { success = false, message = "Giá nhập không được âm!" });
                }
            }

            var importReceipt = new ImportReceipt
            {
                ImportReceiptID = Guid.NewGuid(),
                ImportDate = createdAt,
                Supplier = request.Supplier,
                ImportDetails = new List<ImportDetail>()
            };

            foreach (var detail in request.Details)
            {
                importReceipt.ImportDetails.Add(new ImportDetail
                {
                    ImportReceiptId = importReceipt.ImportReceiptID,
                    OriginalBookId = detail.BookId,
                    ImportReceiptFK = importReceipt.ImportReceiptID,
                    OriginalBookFK = detail.BookId,
                    Quantity = detail.Quantity,
                    Price = detail.Price
                });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Bước 1: Thêm ImportReceipt và ImportDetails
                    _context.ImportReceipts.Add(importReceipt);
                    await _context.SaveChangesAsync();

                    // Bước 2: Cập nhật Books và thêm BookCopies
                    foreach (var detail in request.Details)
                    {
                        var book = await _context.Books
                            .AsNoTracking()
                            .FirstOrDefaultAsync(b => b.OriginalBookID == detail.BookId);

                        if (book == null)
                        {
                            throw new Exception($"Mã đầu sách {detail.BookId} không tồn tại trong database!");
                        }

                        book.Quantity += detail.Quantity;
                        _context.Books.Attach(book);
                        _context.Entry(book).Property(b => b.Quantity).IsModified = true;

                        // Thêm BookCopies với giá từ ImportDetail
                        for (int i = 0; i < detail.Quantity; i++)
                        {
                            var bookCopy = new BookCopy
                            {
                                BookID = Guid.NewGuid(),
                                OriginalBookID = book.OriginalBookID,
                                Condition = Condition.Nguyên_vẹn,
                                ImportDate = createdAt,
                                Price = detail.Price // Đảm bảo gán giá từ phiếu nhập
                            };
                            _context.BookCopies.Add(bookCopy);
                        }

                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Phiếu nhập đã được thêm thành công!",
                        receipt = new
                        {
                            receiptId = importReceipt.ImportReceiptID.ToString(),
                            createdAt = importReceipt.ImportDate.ToString("dd/MM/yyyy"),
                            supplier = importReceipt.Supplier,
                            details = importReceipt.ImportDetails.Select(d => new
                            {
                                bookId = d.OriginalBookId,
                                quantity = d.Quantity,
                                price = d.Price
                            }).ToList()
                        }
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
                }
            }
        }


    }


    public class ImportReceiptRequest
    {
        public string CreatedAt { get; set; }
        public string Supplier { get; set; }
        public List<ImportDetailRequest> Details { get; set; }
    }

    public class ImportDetailRequest
    {
        public string BookId { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}