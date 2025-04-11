using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteLibrary.Data;
using WebsiteLibrary.Helpers;
using WebsiteLibrary.Models;
using WebsiteLibrary.Models.Entities;
using WebsiteLibrary.ViewModels;
using BCrypt.Net;
using NanoidDotNet;
using System.Globalization; // Thêm namespace cho BCrypt



namespace WebsiteLibrary.Controllers
{
    public class ReaderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReaderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> Index()
        {
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

        public IActionResult About()
        {
            return View();
        }
        // HÀM TÍNH TOÁN SỐ  LƯỢNG SẴN SÀNG CỦA SÁCH
        private async Task<int> CalculateAvailableQuantity(string bookId)
        {
            // Lấy thông tin sách và các bản sao
            var book = await _context.Books
                .Include(b => b.BookCopies)
                .FirstOrDefaultAsync(b => b.OriginalBookID == bookId);

            if (book == null) return 0;

            // Tổng số bản sao
            int totalCopies = book.BookCopies?.Count ?? 0;

            // Số bản sao trong Phiếu yêu cầu "Chờ duyệt"
            int pendingLoanRequestCopies = await _context.LoanRequestDetails
                .Where(lrd => lrd.BookCopy.OriginalBookID == bookId &&
                              lrd.LoanRequest.Status == LoanRequestStatus.Choduyet)
                .CountAsync();

            // Số bản sao trong Phiếu mượn "Đang mượn" hoặc "Quá hạn"
            int borrowingCopies = await _context.BorrowingSlipDetails
                .Where(bsd => bsd.BookCopy.OriginalBookID == bookId &&
                              (bsd.BorrowingSlip.Status == BorrowingSlipStatus.Dangmuon ||
                               bsd.BorrowingSlip.Status == BorrowingSlipStatus.Quahan))
                .CountAsync();

            // Số bản sao trong Phiếu trả với tình trạng "Hỏng" hoặc "Mất"
            int damagedOrLostCopies = await _context.ReturnSlipDetails
                .Where(rsd => rsd.BookCopy.OriginalBookID == bookId &&
                              (rsd.Condition == Condition.Hỏng || rsd.Condition == Condition.Mất))
                .CountAsync();

            // Tính số lượng sẵn sàng
            int availableQuantity = totalCopies - pendingLoanRequestCopies - borrowingCopies - damagedOrLostCopies;
            return availableQuantity > 0 ? availableQuantity : 0;
        }

        // VIEW CỦA GIỎ MƯỢN

        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> LoanCart()
        {
            // Lấy giỏ mượn từ Session
            var cart = HttpContext.Session.GetObject<List<LoanCartItem>>("LoanCart") ?? new List<LoanCartItem>();

            // Cập nhật số lượng sẵn sàng
            foreach (var item in cart)
            {
                item.AvailableQuantity = await CalculateAvailableQuantity(item.BookID);
            }

            // Trả về View với danh sách giỏ mượn
            return View(cart);
        }

        [Authorize(Roles = "Reader")]
        public IActionResult Info()
        {
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            var account = _context.Accounts.FirstOrDefault(a => a.Email == email);
            if (account == null)
            {
                return NotFound("Không tìm thấy thông tin tài khoản.");
            }

            return View(account);
        }

        

        [HttpGet]
        public async Task<IActionResult> GetBooksForHomePage()
        {
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

            return Json(books);
        }

        [Authorize(Roles = "Reader")]
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

        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> Library(string category = "Tất cả", int page = 1)
        {
            const int pageSize = 16; // Số sách trên mỗi trang
            var query = _context.Books
                .Include(b => b.BookCopies)
                .AsQueryable();

            // Lọc theo danh mục nếu không phải "Tất cả"
            if (category != "Tất cả")
            {
                Category enumCategory = MapStringToCategory(category);
                query = query.Where(b => b.Category == enumCategory);
            }

            // Tổng số sách
            int totalBooks = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            // Đảm bảo page không vượt quá giới hạn
            page = Math.Max(1, Math.Min(page, totalPages));

            // Lấy sách cho trang hiện tại
            var books = await query
                .OrderBy(b => b.OriginalBookID) // Sắp xếp theo mã sách (hoặc tiêu chí khác nếu muốn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new
                {
                    id = b.OriginalBookID,
                    title = b.OriginalBookTitle,
                    image = b.ImagePath ?? "/images/default-book.jpg",
                    category = MapCategoryToString(b.Category)
                })
                .ToListAsync();

            // Truyền dữ liệu qua ViewBag
            ViewBag.Books = books;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View();
        }

        // Hàm ánh xạ Category enum sang chuỗi (static)
        private static string MapCategoryToString(Category category)
        {
            return category switch
            {
                Category.Tailieuhoctap => "Tài liệu học tập",
                Category.Tailieulichsu => "Tài liệu lịch sử",
                Category.Sachphattrienbanthan => "Sách phát triển bản thân",
                Category.Tieuthuyet => "Tiểu thuyết",
                _ => "Không xác định"
            };
        }

        // Hàm ánh xạ chuỗi sang Category enum (static)
        private static Category MapStringToCategory(string category)
        {
            return category switch
            {
                "Tài liệu học tập" => Category.Tailieuhoctap,
                "Tài liệu lịch sử" => Category.Tailieulichsu,
                "Sách phát triển bản thân" => Category.Sachphattrienbanthan,
                "Tiểu thuyết" => Category.Tieuthuyet,
                _ => throw new ArgumentException("Danh mục không hợp lệ")
            };
        }


        // THÊM VÀO GIỎ
        [Authorize(Roles = "Reader")]
        [HttpPost]
        public async Task<IActionResult> AddToCart(string bookId, int quantity = 1)
        {
            // Lấy thông tin sách từ DB (giữ logic cũ)
            var book = await _context.Books
                .Include(b => b.BookCopies)
                .FirstOrDefaultAsync(b => b.OriginalBookID == bookId);

            if (book == null)
            {
                return NotFound("Không tìm thấy sách.");
            }

            // Tính số lượng sẵn sàng (dùng CalculateAvailableQuantity)
            int availableQuantity = await CalculateAvailableQuantity(bookId);

            // Kiểm tra số lượng sẵn sàng (giữ logic cũ, nhưng không thông báo)
            if (availableQuantity == 0 || availableQuantity < quantity)
            {
                // Không thông báo, chỉ chuyển hướng về BookDetails
                return RedirectToAction("BookDetails", new { id = bookId });
            }

            // Lấy giỏ mượn từ Session (giữ logic cũ)
            var cart = HttpContext.Session.GetObject<List<LoanCartItem>>("LoanCart") ?? new List<LoanCartItem>();

            // Kiểm tra sách đã có trong giỏ chưa (giữ logic cũ)
            if (!cart.Any(item => item.BookID == bookId))
            {
                // Thêm sách vào giỏ (giữ logic cũ)
                cart.Add(new LoanCartItem
                {
                    BookID = book.OriginalBookID,
                    Title = book.OriginalBookTitle,
                    AvailableQuantity = availableQuantity,
                    ImagePath = book.ImagePath ?? "/images/default-book.jpg"
                });

                // Cập nhật lại Session (giữ logic cũ)
                HttpContext.Session.SetObject("LoanCart", cart);
            }

            // Chuyển hướng về trang chi tiết sách (giữ logic cũ)
            return RedirectToAction("BookDetails", new { id = bookId });
        }

        [Authorize(Roles = "Reader")]
        [HttpPost]
        public IActionResult RemoveFromCart(string bookId)
        {
            if (string.IsNullOrEmpty(bookId))
            {
                return Json(new { success = false, message = "Mã sách không hợp lệ." });
            }

            // Lấy giỏ mượn từ Session
            var cart = HttpContext.Session.GetObject<List<LoanCartItem>>("LoanCart") ?? new List<LoanCartItem>();

            // Tìm và xóa mục khỏi giỏ
            var itemToRemove = cart.FirstOrDefault(item => item.BookID == bookId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);

                // Cập nhật lại Session
                HttpContext.Session.SetObject("LoanCart", cart);
                return Json(new { success = true, message = "Đã xóa sách khỏi giỏ mượn." });
            }

            return Json(new { success = false, message = "Không tìm thấy sách trong giỏ mượn." });
        }

        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> ConfirmBorrow()
        {
            // Lấy email của người dùng đã đăng nhập
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin độc giả
            var reader = await _context.Readers
                .Include(r => r.LibraryCards)
                .FirstOrDefaultAsync(r => r.Email == email);

            if (reader == null)
            {
                return NotFound("Không tìm thấy thông tin độc giả.");
            }

            // Lấy giỏ mượn từ Session
            var cart = HttpContext.Session.GetObject<List<LoanCartItem>>("LoanCart") ?? new List<LoanCartItem>();
            if (!cart.Any())
            {
                return RedirectToAction("LoanCart");
            }

            // Kiểm tra số lượng sẵn sàng
            foreach (var item in cart)
            {
                item.AvailableQuantity = await CalculateAvailableQuantity(item.BookID);
                if (item.AvailableQuantity == 0)
                {
                    TempData["ErrorMessage"] = $"Sách {item.Title} hiện không có sẵn để mượn.";
                    return RedirectToAction("LoanCart");
                }
            }

            // Tạo view model
            var viewModel = new ConfirmBorrowViewModel
            {
                Reader = reader,
                CartItems = cart,
                BorrowDeadline = DateTime.Now.AddDays(2)
            };

            return View(viewModel);
        }


        // LOAN REQUEST
        [Authorize(Roles = "Reader")]
        [HttpPost]
        public async Task<IActionResult> CreateLoanRequest()
        {
            // Lấy email của người dùng đã đăng nhập
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để tiếp tục." });
            }

            // Lấy thông tin độc giả
            var reader = await _context.Readers
                .Include(r => r.LibraryCards)
                .FirstOrDefaultAsync(r => r.Email == email);

            if (reader == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin độc giả." });
            }

            // Lấy mã thẻ thư viện
            var libraryCard = reader.LibraryCards?.FirstOrDefault();
            if (libraryCard == null)
            {
                return Json(new { success = false, message = "Bạn chưa có thẻ thư viện." });
            }

            // Kiểm tra phiếu yêu cầu "Chờ duyệt"
            var existingLoanRequest = await _context.LoanRequests
                .AnyAsync(lr => lr.CardID == libraryCard.CardID && lr.Status == LoanRequestStatus.Choduyet);

            if (existingLoanRequest)
            {
                return Json(new { success = false, message = "Bạn đã có một yêu cầu mượn đang chờ duyệt." });
            }

            // Kiểm tra phiếu m  "Đang mượn" hoặc "Quá hạn"
            var existingBorrowingSlip = await _context.BorrowingSlips
                .AnyAsync(bs => bs.CardID == libraryCard.CardID &&
                                (bs.Status == BorrowingSlipStatus.Dangmuon || bs.Status == BorrowingSlipStatus.Quahan));

            if (existingBorrowingSlip)
            {
                return Json(new { success = false, message = "Bạn đang có phiếu mượn chưa trả hoặc quá hạn." });
            }

            // Lấy giỏ mượn từ Session
            var cart = HttpContext.Session.GetObject<List<LoanCartItem>>("LoanCart") ?? new List<LoanCartItem>();
            if (!cart.Any())
            {
                return Json(new { success = false, message = "Giỏ mượn trống. Vui lòng thêm sách để mượn." });
            }

            // Kiểm tra số lượng sẵn sàng
            foreach (var item in cart)
            {
                var availableQuantity = await CalculateAvailableQuantity(item.BookID);
                if (availableQuantity == 0)
                {
                    return Json(new { success = false, message = $"Sách {item.Title} hiện không có sẵn để mượn." });
                }
            }

            // Tạo LoanRequest
            var loanRequest = new LoanRequest
            {
                RequestID = Guid.NewGuid(),
                CardID = libraryCard.CardID,
                RequestDate = DateTime.Now,
                Status = LoanRequestStatus.Choduyet,
                LoanRequestDetails = new List<LoanRequestDetail>()
            };

            // Tạo LoanRequestDetail
            foreach (var item in cart)
            {
                var usedBookCopyIds = await _context.LoanRequestDetails
                    .Where(lrd => lrd.LoanRequest.Status == LoanRequestStatus.Choduyet)
                    .Select(lrd => lrd.BookCopyId)
                    .ToListAsync();

                var bookCopy = await _context.BookCopies
                    .Where(bc => bc.OriginalBookID == item.BookID
                              && bc.Condition == Condition.Nguyên_vẹn
                              && !usedBookCopyIds.Contains(bc.BookID))
                    .FirstOrDefaultAsync();

                if (bookCopy == null)
                {
                    return Json(new { success = false, message = $"Không còn bản sao nào của sách {item.Title} để mượn." });
                }

                loanRequest.LoanRequestDetails.Add(new LoanRequestDetail
                {
                    RequestId = loanRequest.RequestID,
                    BookCopyId = bookCopy.BookID,
                    RequestFK = loanRequest.RequestID,
                    BookCopyFK = bookCopy.BookID
                });
            }

            // Lưu vào database
            _context.LoanRequests.Add(loanRequest);
            await _context.SaveChangesAsync();

            // Xóa giỏ mượn
            HttpContext.Session.Remove("LoanCart");

            return Json(new { success = true, message = "Yêu cầu mượn đã được tạo thành công." });
        }

        [Authorize(Roles = "Reader")]
        [HttpPut]
        public async Task<IActionResult> EditInfo([FromBody] ReaderUpdateRequest updatedReaderRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
            }

            var email = User.Identity.Name;
            var reader = await _context.Readers.FirstOrDefaultAsync(r => r.Email == email);
            if (reader == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy độc giả!" });
            }

            // Chỉ cập nhật các trường mà độc giả được phép chỉnh sửa
            reader.DateOfBirth = DateTime.Parse(updatedReaderRequest.DateOfBirth);
            reader.Gender = updatedReaderRequest.Gender;
            reader.PhoneNumber = updatedReaderRequest.PhoneNumber;
            reader.EducationLevel = updatedReaderRequest.EducationLevel;
            reader.Address = updatedReaderRequest.Address;

            _context.Readers.Update(reader);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thông tin cá nhân đã được cập nhật!" });
        }

        [Authorize(Roles = "Reader")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest passwordRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
            }

            var email = User.Identity.Name;
            var reader = await _context.Readers.FirstOrDefaultAsync(r => r.Email == email);
            if (reader == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy độc giả!" });
            }

            // Kiểm tra mật khẩu cũ bằng BCrypt
            if (!BCrypt.Net.BCrypt.Verify(passwordRequest.OldPassword, reader.Password))
            {
                return Json(new { success = false, message = "Mật khẩu cũ không đúng!" });
            }

            // Cập nhật mật khẩu mới (được băm bằng BCrypt)
            reader.Password = BCrypt.Net.BCrypt.HashPassword(passwordRequest.NewPassword);
            _context.Readers.Update(reader);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Mật khẩu đã được thay đổi thành công!" });
        }

        //THẺ THƯ VIỆN //////////////////////////////////////////////////

        //KIỂM TRA TRẠNG THÁI THẺ

        [Authorize(Roles = "Reader")]
        [HttpGet]
        public IActionResult CheckCardStatus()
        {
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { hasCard = false, isPending = false });
            }

            var reader = _context.Readers
                .Include(r => r.LibraryCards)
                .FirstOrDefault(r => r.Email == email);

            if (reader == null)
            {
                return Json(new { hasCard = false, isPending = false });
            }

            // Kiểm tra thẻ thư viện chính thức
            var libraryCard = reader.LibraryCards?.FirstOrDefault();
            if (libraryCard != null && (libraryCard.Status == "Đang hoạt động" || libraryCard.Status == "Bị khóa"))
            {
                return Json(new
                {
                    hasCard = true,
                    isPending = false,
                    cardId = libraryCard.CardID,
                    registrationYear = libraryCard.RegistrationDate.Year
                });
            }

            // Kiểm tra yêu cầu đăng ký thẻ
            var cardRequest = _context.LibraryCardRequests
                .FirstOrDefault(r => r.AccountID == reader.ID);
            if (cardRequest != null)
            {
                return Json(new
                {
                    hasCard = false,
                    isPending = true,
                    requestId = cardRequest.RequestID
                });
            }

            return Json(new { hasCard = false, isPending = false });
        }


        // lấy THÔNG TIN THẺ
        [Authorize(Roles = "Reader")]
        [HttpGet]
        public IActionResult GetCardDetails()
        {
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            var reader = _context.Readers
                .Include(r => r.LibraryCards)
                .FirstOrDefault(r => r.Email == email);

            if (reader == null || reader.LibraryCards == null || !reader.LibraryCards.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy thẻ thư viện!" });
            }

            var card = reader.LibraryCards.First();
            return Json(new
            {
                success = true,
                cardPhotoUrl = card.CardPhotoUrl,
                status = card.Status,
                registrationYear = card.RegistrationDate.Year
            });
        }

        // ĐĂNG KÝ THẺ THƯ VIỆN

        [Authorize(Roles = "Reader")]
        public IActionResult LibraryRegisterCard()
        {
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            var reader = _context.Readers.FirstOrDefault(r => r.Email == email);
            if (reader == null)
            {
                return View(null); // Trả về view với model null nếu không tìm thấy
            }

            return View(reader);
        }

        // GỬI YÊU CẦU TẠO THẺ
        [Authorize(Roles = "Reader")]
        [HttpPost]
        public async Task<IActionResult> SubmitCardRequest(IFormFile cardPhoto)
        {
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            var reader = _context.Readers.FirstOrDefault(r => r.Email == email);
            if (reader == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin độc giả!" });
            }

            // Kiểm tra xem đã có yêu cầu hoặc thẻ chưa
            var existingRequest = _context.LibraryCardRequests.FirstOrDefault(r => r.AccountID == reader.ID);
            var existingCard = _context.LibraryCards.FirstOrDefault(c => c.ID == reader.ID);
            if (existingRequest != null || existingCard != null)
            {
                return Json(new { success = false, message = "Bạn đã có yêu cầu hoặc thẻ thư viện!" });
            }

            // Xử lý upload ảnh
            string filePath = null;
            if (cardPhoto != null && cardPhoto.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(cardPhoto.FileName);
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await cardPhoto.CopyToAsync(stream);
                }
                filePath = "/uploads/" + fileName;
            }
            else
            {
                return Json(new { success = false, message = "Vui lòng upload ảnh thẻ!" });
            }

            // Không tạo LibraryCardRequest ở đây nữa
            // Trả về đường dẫn ảnh và redirectUrl
            return Json(new { success = true, message = "Ảnh thẻ đã được upload thành công!", filePath = filePath, redirectUrl = "/Reader/LibraryPayment" });
        }

        // HIỂN THỊ VIEW
        [Authorize(Roles = "Reader")]
        public IActionResult LibraryPayment()
        {
            return View();
        }

        // XÁC NHẬN THANH TOÁN
        [Authorize(Roles = "Reader")]
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment([FromBody] CardRequestData cardRequestData)
        {
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            var reader = _context.Readers.FirstOrDefault(r => r.Email == email);
            if (reader == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin độc giả!" });
            }

            // Kiểm tra lại xem đã có yêu cầu hoặc thẻ chưa
            var existingRequest = _context.LibraryCardRequests.FirstOrDefault(r => r.AccountID == reader.ID);
            var existingCard = _context.LibraryCards.FirstOrDefault(c => c.ID == reader.ID);
            if (existingRequest != null || existingCard != null)
            {
                return Json(new { success = false, message = "Bạn đã có yêu cầu hoặc thẻ thư viện!" });
            }

            // Parse ngày từ string
            DateTime registrationDate;
            DateTime expirationDate;
            try
            {
                registrationDate = DateTime.ParseExact(cardRequestData.registerDate, "dd/MM/yyyy", null);
                expirationDate = DateTime.ParseExact(cardRequestData.expiryDate, "dd/MM/yyyy", null);
            }
            catch
            {
                return Json(new { success = false, message = "Định dạng ngày không hợp lệ!" });
            }

            // Tạo bản ghi LibraryCardRequest
            var cardRequest = new LibraryCardRequest
            {
                AccountID = cardRequestData.accountId,
                ReaderName = cardRequestData.readerName,
                RegistrationDate = registrationDate,
                ExpirationDate = expirationDate,
                CardPhotoUrl = cardRequestData.cardPhotoUrl
            };

            _context.LibraryCardRequests.Add(cardRequest);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thanh toán thành công! Yêu cầu của bạn đã được gửi đến thủ thư để phê duyệt." });
        }

        // HISTORY
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> History(int page = 1, int pageSize = 5)
        {
            // Lấy email của người dùng đã đăng nhập
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin độc giả từ cơ sở dữ liệu
            var reader = await _context.Readers
                .Include(r => r.LibraryCards)
                .FirstOrDefaultAsync(r => r.Email == email);

            if (reader == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy mã thẻ thư viện
            var libraryCard = reader.LibraryCards?.FirstOrDefault();
            if (libraryCard == null)
            {
                return View(new List<LoanRequest>());
            }

            // Kiểm tra và cập nhật trạng thái LoanRequest
            var now = DateTime.Now;
            var pendingRequests = await _context.LoanRequests
                .Where(lr => lr.CardID == libraryCard.CardID && lr.Status == LoanRequestStatus.Choduyet)
                .ToListAsync();

            foreach (var request in pendingRequests)
            {
                var timeElapsed = now - request.RequestDate;
                if (timeElapsed.TotalHours >= 48) // Quá 2 ngày
                {
                    request.Status = LoanRequestStatus.Dahuy;
                    _context.LoanRequests.Update(request);
                }
            }
            await _context.SaveChangesAsync();

            // Lấy tổng số LoanRequest
            var totalRequests = await _context.LoanRequests
                .Where(lr => lr.CardID == libraryCard.CardID)
                .CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling((double)totalRequests / pageSize);

            // Đảm bảo page hợp lệ
            page = Math.Max(1, Math.Min(page, totalPages));

            // Lấy danh sách LoanRequest cho trang hiện tại
            var loanRequests = await _context.LoanRequests
                .Where(lr => lr.CardID == libraryCard.CardID)
                .OrderByDescending(lr => lr.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(loanRequests);
        }

        // Lấy thông tin của LoanRequest - để hiển thị View hợp lý
        [Authorize(Roles = "Reader")]
        [HttpGet]
        public async Task<IActionResult> GetLoanRequestDetails(Guid requestId)
        {
            try
            {
                // Lấy LoanRequest
                var loanRequest = await _context.LoanRequests
                    .Include(lr => lr.LoanRequestDetails)
                    .ThenInclude(lrd => lrd.BookCopy)
                    .ThenInclude(bc => bc.Book)
                    .FirstOrDefaultAsync(lr => lr.RequestID == requestId);

                if (loanRequest == null)
                {
                    return Json(new LoanRequestDetailsResponse { Success = false, Status = "Không tìm thấy yêu cầu mượn." });
                }

                // Kiểm tra LoanRequestDetails
                if (loanRequest.LoanRequestDetails == null || !loanRequest.LoanRequestDetails.Any())
                {
                    return Json(new LoanRequestDetailsResponse { Success = false, Status = "Yêu cầu mượn không có sách." });
                }

                // Lấy danh sách sách từ LoanRequestDetails
                var bookList = loanRequest.LoanRequestDetails
                    .Where(lrd => lrd.BookCopy != null && lrd.BookCopy.Book != null)
                    .Select(lrd => new BookItem
                    {
                        Stt = loanRequest.LoanRequestDetails.ToList().IndexOf(lrd) + 1,
                        BookTitle = lrd.BookCopy.Book.OriginalBookTitle ?? "Không xác định"
                    })
                    .ToList();

                // Chuẩn bị dữ liệu trả về
                var response = new LoanRequestDetailsResponse
                {
                    Success = true,
                    Status = loanRequest.Status.ToString(),
                    RequestDate = loanRequest.RequestDate.ToString("dd/MM/yyyy"),
                    Books = bookList,
                    BorrowingSlip = null
                };

                // Nếu trạng thái là "Đã duyệt", lấy thông tin từ BorrowingSlip
                if (loanRequest.Status == LoanRequestStatus.Daduyet)
                {
                    // Tìm BorrowingSlip dựa trên CardID và BorrowDate >= RequestDate
                    var borrowingSlip = await _context.BorrowingSlips
                        .Include(bs => bs.BorrowingSlipDetails)
                        .ThenInclude(bsd => bsd.BookCopy)
                        .ThenInclude(bc => bc.Book)
                        .Where(bs => bs.CardID == loanRequest.CardID && bs.BorrowDate >= loanRequest.RequestDate)
                        .OrderBy(bs => bs.BorrowDate) // Lấy phiếu mượn gần nhất
                        .FirstOrDefaultAsync();

                    if (borrowingSlip != null && borrowingSlip.BorrowingSlipDetails != null && borrowingSlip.BorrowingSlipDetails.Any())
                    {
                        // So sánh danh sách sách trong LoanRequestDetails và BorrowingSlipDetails
                        var loanRequestBookCopyIds = loanRequest.LoanRequestDetails
                            .Where(lrd => lrd.BookCopyId != null)
                            .Select(lrd => lrd.BookCopyId)
                            .OrderBy(id => id)
                            .ToList();

                        var borrowingSlipBookCopyIds = borrowingSlip.BorrowingSlipDetails
                            .Where(bsd => bsd.BookCopyId != null)
                            .Select(bsd => bsd.BookCopyId)
                            .OrderBy(id => id)
                            .ToList();

                        // Kiểm tra xem danh sách sách có khớp không
                        if (loanRequestBookCopyIds.Any() && borrowingSlipBookCopyIds.Any() && loanRequestBookCopyIds.SequenceEqual(borrowingSlipBookCopyIds))
                        {
                            // Tìm ReturnSlip (nếu có)
                            var returnSlip = await _context.ReturnSlips
                                .FirstOrDefaultAsync(rs => rs.BorrowingSlipID == borrowingSlip.BorrowingSlipID);

                            // Tính DueDate từ BorrowDuration
                            int borrowDays = 30; // Giá trị mặc định
                            if (!string.IsNullOrEmpty(borrowingSlip.BorrowDuration))
                            {
                                // Giả sử BorrowDuration có dạng "30 ngày"
                                var durationParts = borrowingSlip.BorrowDuration.Split(' ');
                                if (durationParts.Length > 0 && int.TryParse(durationParts[0], out int days))
                                {
                                    borrowDays = days;
                                }
                            }
                            var dueDate = borrowingSlip.BorrowDate.AddDays(borrowDays);

                            response.BorrowingSlip = new BorrowingSlipDetails
                            {
                                BorrowDate = borrowingSlip.BorrowDate.ToString("dd/MM/yyyy"),
                                ReturnDate = returnSlip != null ? returnSlip.ReturnDate.ToString("dd/MM/yyyy") : "Chưa có",
                                DueDate = dueDate.ToString("dd/MM/yyyy"),
                                Status = borrowingSlip.Status.ToString()
                            };
                        }
                    }
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                System.Diagnostics.Debug.WriteLine($"Error in GetLoanRequestDetails: {ex.Message}");
                return Json(new LoanRequestDetailsResponse { Success = false, Status = "Đã xảy ra lỗi khi lấy chi tiết: " + ex.Message });
            }
        }

        // CHỨC NĂNG HỦY YÊU CẦU MƯỢN NÈ
        [Authorize(Roles = "Reader")]
        [HttpPost]
        public async Task<IActionResult> CancelLoanRequest(Guid requestId)
        {
            // Lấy email của người dùng đang đăng nhập
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để tiếp tục." });
            }

            // Lấy thông tin độc giả
            var reader = await _context.Readers
                .Include(r => r.LibraryCards)
                .FirstOrDefaultAsync(r => r.Email == email);

            if (reader == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin độc giả." });
            }

            // Lấy LoanRequest cần hủy
            var loanRequest = await _context.LoanRequests
                .FirstOrDefaultAsync(lr => lr.RequestID == requestId && lr.CardID == reader.LibraryCards.First().CardID);

            if (loanRequest == null)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu mượn hoặc bạn không có quyền hủy." });
            }

            // Kiểm tra trạng thái
            if (loanRequest.Status != LoanRequestStatus.Choduyet)
            {
                return Json(new { success = false, message = "Chỉ có thể hủy yêu cầu khi đang ở trạng thái 'Chờ duyệt'." });
            }

            // Cập nhật trạng thái thành "Đã hủy"
            loanRequest.Status = LoanRequestStatus.Dahuy;
            _context.LoanRequests.Update(loanRequest);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Yêu cầu mượn đã được hủy thành công." });
        }



    }
    // Lớp DTO cho chỉnh sửa thông tin độc giả
    public class ReaderUpdateRequest
    {
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string EducationLevel { get; set; }
        public string Address { get; set; }
    }

    // Lớp DTO cho đổi mật khẩu
    public class PasswordChangeRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class CardRequestData
    {
        public string accountId { get; set; }
        public string readerName { get; set; }
        public string registerDate { get; set; }
        public string expiryDate { get; set; }
        public string cardPhotoUrl { get; set; }
        public int amount { get; set; }
    }


}
