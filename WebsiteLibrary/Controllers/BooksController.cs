using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteLibrary.Data;
using WebsiteLibrary.Models.Entities;

namespace WebsiteLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Books.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.BookCopies)
                .Select(b => new
                {
                    id = b.OriginalBookID,
                    title = b.OriginalBookTitle,
                    publisher = b.Publisher,
                    author = b.Author,
                    pages = b.PageCount,
                    quantity = b.BookCopies.Count,
                    year = b.PublicationYear,
                    category = b.Category.ToString(),
                    image = b.ImagePath
                })
                .ToListAsync();

            return Json(books);
        }
        [HttpPost]
        public async Task<IActionResult> AddBook([FromForm] BookRequest bookRequest, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
            }

            if (await _context.Books.AnyAsync(b => b.OriginalBookID == bookRequest.BookId))
            {
                return BadRequest(new { success = false, message = "Mã đầu sách đã tồn tại!" });
            }

            var book = new Book
            {
                OriginalBookID = bookRequest.BookId,
                OriginalBookTitle = bookRequest.OriginalBookTitle,
                Publisher = bookRequest.Publisher,
                Author = bookRequest.Author,
                PageCount = bookRequest.PageCount,
                Quantity = 0,
                PublicationYear = bookRequest.PublicationYear,
                Category = MapCategory(bookRequest.Category),
                BookCopies = new List<BookCopy>()
            };

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                book.ImagePath = "/images/" + uniqueFileName;
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Sách đã được thêm thành công!",
                book = new
                {
                    id = book.OriginalBookID,
                    title = book.OriginalBookTitle,
                    publisher = book.Publisher,
                    author = book.Author,
                    pages = book.PageCount,
                    quantity = book.Quantity,
                    year = book.PublicationYear,
                    category = book.Category.ToString(),
                    image = book.ImagePath
                }
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBook(string id, [FromBody] BookRequest bookRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.OriginalBookID == id);
            if (book == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy sách!" });
            }

            book.OriginalBookID = bookRequest.BookId; // OriginalBookID giờ là string
            book.OriginalBookTitle = bookRequest.OriginalBookTitle;
            book.Publisher = bookRequest.Publisher;
            book.Author = bookRequest.Author;
            book.PageCount = bookRequest.PageCount;
            book.PublicationYear = bookRequest.PublicationYear;
            book.Category = MapCategory(bookRequest.Category);

            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thông tin sách đã được cập nhật!" });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBook(string id)
        {
            var book = await _context.Books
                .Include(b => b.BookCopies)
                .FirstOrDefaultAsync(b => b.OriginalBookID == id);

            if (book == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy sách!" });
            }

            if (book.BookCopies.Any())
            {
                return BadRequest(new { success = false, message = "Không thể xóa sách vì đã có bản sao tồn tại!" });
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Sách đã được xóa!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetBookCopies(string bookId)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.OriginalBookID == bookId);
            if (book == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy sách!" });
            }

            var bookCopies = await _context.BookCopies
                .Where(bc => bc.OriginalBookID == book.OriginalBookID) // OriginalBookID giờ là string
                .Select(bc => new
                {
                    copyId = bc.BookID,
                    bookId = bookId,
                    status = bc.Condition.ToString().Replace("_", " "),
                    createdAt = bc.ImportDate.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(bookCopies);
        }

        private Category MapCategory(string category)
        {
            return category switch
            {
                "van-hoc" => Category.Tailieuhoctap,
                "khoa-hoc" => Category.Tailieulichsu,
                "lich-su" => Category.Sachphattrienbanthan,
                "tam-ly" => Category.Tieuthuyet,
                _ => throw new ArgumentException("Thể loại không hợp lệ")
            };
        }
    }

    public class BookRequest
    {
        public string BookId { get; set; } // Chuỗi mã đầu sách (ví dụ: TLLS0001)
        public string OriginalBookTitle { get; set; }
        public string Publisher { get; set; }
        public string Author { get; set; }
        public int PageCount { get; set; }
        public int PublicationYear { get; set; }
        public string Category { get; set; }
    }
}