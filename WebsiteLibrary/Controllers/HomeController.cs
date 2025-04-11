using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebsiteLibrary.Data;
using WebsiteLibrary.Models;
using WebsiteLibrary.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebsiteLibrary.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Reader"))
                return RedirectToAction("Index", "Reader");
            if (User.IsInRole("Librarian"))
                return RedirectToAction("Dashboard", "Admin");
        }

        var books = await _context.Books
            .Include(b => b.BookCopies)
            .Select(b => new
            {
                id = b.OriginalBookID,
                title = b.OriginalBookTitle,
                image = b.ImagePath ?? "/images/default-book.jpg",
                category = b.Category.ToString()
            })
            .Take(8)
            .ToListAsync();

        ViewBag.Books = books;
        return View();
    }

    public async Task<IActionResult> BookDetails(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Không tìm thấy mã sách.");
            }

            var book = await _context.Books
                .Include(b => b.BookCopies)
                .FirstOrDefaultAsync(b => b.OriginalBookID == id);

            if (book == null)
            {
                return NotFound("Không tìm thấy sách.");
            }

            ViewBag.Stock = book.BookCopies?.Count(bc => bc.Condition == Condition.Nguyên_vẹn) ?? 0;

            var relatedBooks = await _context.Books
                .Where(b => b.Category == book.Category && b.OriginalBookID != book.OriginalBookID)
                .Select(b => new
                {
                    id = b.OriginalBookID,
                    title = b.OriginalBookTitle,
                    image = b.ImagePath ?? "/images/default-book.jpg",
                    category = b.Category.ToString()
                })
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedBooks = relatedBooks;

            return View("BookDetails", book);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Đã xảy ra lỗi khi tải thông tin sách.");
        }
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
