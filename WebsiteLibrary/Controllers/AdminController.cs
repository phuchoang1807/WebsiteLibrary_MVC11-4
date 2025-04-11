using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
using WebsiteLibrary.Data;
using WebsiteLibrary.Models.Entities;
using WebsiteLibrary.Models.ViewModels;
using WebsiteLibrary.ViewModels;

namespace WebsiteLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Readers()
        {
            return View();
        }

        // Lấy danh sách độc giả
        [HttpGet]
        public async Task<IActionResult> GetReaders()
        {
            var readers = await _context.Readers
                .Select(r => new
                {
                    id = r.ReaderCode,
                    name = r.Name,
                    dob = r.DateOfBirth.ToString("dd/MM/yyyy"),
                    gender = r.Gender,
                    phone = r.PhoneNumber,
                    email = r.Email,
                    address = r.Address,
                    education = r.EducationLevel
                })
                .ToListAsync();

            return Json(readers);
        }

        // Thêm độc giả
        [HttpPost]
        public async Task<IActionResult> AddReader([FromBody] ReaderRequest readerRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors, receivedData = readerRequest });
            }

            var reader = new Reader
            {
                Name = readerRequest.Name,
                DateOfBirth = DateTime.Parse(readerRequest.DateOfBirth), // Parse YYYY-MM-DD
                Gender = readerRequest.Gender,
                PhoneNumber = readerRequest.PhoneNumber,
                Email = readerRequest.Email,
                Address = readerRequest.Address,
                EducationLevel = readerRequest.EducationLevel
            };

            // Tạo mã độc giả mới (DGxxxxx)
            var lastReader = await _context.Readers
                .OrderByDescending(r => r.ReaderCode)
                .FirstOrDefaultAsync();
            int newNumber = lastReader == null ? 0 : int.Parse(lastReader.ReaderCode.Replace("DG", "")) + 1;
            reader.ReaderCode = $"DG{newNumber.ToString().PadLeft(5, '0')}";

            reader.Role = "Reader";
            reader.Password = "defaultPassword";
            if (string.IsNullOrEmpty(reader.EducationLevel))
            {
                reader.EducationLevel = "Trung học";
            }

            _context.Readers.Add(reader);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Độc giả đã được thêm thành công!",
                reader = new
                {
                    id = reader.ReaderCode,
                    name = reader.Name,
                    dob = reader.DateOfBirth.ToString("dd/MM/yyyy"),
                    gender = reader.Gender,
                    phone = reader.PhoneNumber,
                    email = reader.Email,
                    address = reader.Address,
                    education = reader.EducationLevel
                }
            });
        }

        // Sửa độc giả
        [HttpPut]
        public async Task<IActionResult> UpdateReader(string id, [FromBody] ReaderRequest updatedReaderRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
            }

            var reader = await _context.Readers.FirstOrDefaultAsync(r => r.ReaderCode == id);
            if (reader == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy độc giả!" });
            }

            // Cập nhật thông tin
            reader.Name = updatedReaderRequest.Name;
            reader.DateOfBirth = DateTime.Parse(updatedReaderRequest.DateOfBirth);
            reader.Gender = updatedReaderRequest.Gender;
            reader.PhoneNumber = updatedReaderRequest.PhoneNumber;
            reader.Email = updatedReaderRequest.Email;
            reader.Address = updatedReaderRequest.Address;
            reader.EducationLevel = updatedReaderRequest.EducationLevel;

            _context.Readers.Update(reader);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thông tin độc giả đã được cập nhật!" });
        }

        // Xóa độc giả
        [HttpDelete]
        public async Task<IActionResult> DeleteReader(string id)
        {
            var reader = await _context.Readers.FirstOrDefaultAsync(r => r.ReaderCode == id);
            if (reader == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy độc giả!" });
            }

            _context.Readers.Remove(reader);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Độc giả đã được xóa!" });
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Lấy ngày hiện tại
            var today = DateTime.Today;

            // 1. Hoạt động hôm nay
            // Số lượt phiếu mượn tạo hôm nay
            int todayBorrows = await _context.BorrowingSlips
                .CountAsync(bs => bs.BorrowDate.Date == today);

            // Số lượt phiếu trả tạo hôm nay
            int todayReturns = await _context.ReturnSlips
                .CountAsync(rs => rs.ReturnDate.Date == today);

            // 2. Tình trạng sách
            // Tổng số sách (đếm tất cả BookCopies)
            int totalBooks = await _context.BookCopies.CountAsync();

            // Tổng số sách đang mượn (đếm số BookCopies trong phiếu mượn "Đang mượn" hoặc "Quá hạn")
            int borrowedBooks = await _context.BorrowingSlipDetails
                .Where(bsd => bsd.BorrowingSlip.Status == BorrowingSlipStatus.Dangmuon ||
                              bsd.BorrowingSlip.Status == BorrowingSlipStatus.Quahan)
                .CountAsync();

            // 3. Đặt mượn
            // Số yêu cầu mượn chờ duyệt
            int pendingReservations = await _context.LoanRequests
                .CountAsync(lr => lr.Status == LoanRequestStatus.Choduyet);

            // Số phiếu mượn quá hạn
            int overdueBooks = await _context.BorrowingSlips
                .CountAsync(bs => bs.Status == BorrowingSlipStatus.Quahan);

            // 4. Dữ liệu biểu đồ (lượt mượn/trả theo ngày trong tháng)
            var currentDate = DateTime.Now;
            var daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            var borrowData = new int[daysInMonth];
            var returnData = new int[daysInMonth];

            // Lượt mượn theo ngày
            var borrowCounts = await _context.BorrowingSlips
                .Where(bs => bs.BorrowDate.Year == currentDate.Year && bs.BorrowDate.Month == currentDate.Month)
                .GroupBy(bs => bs.BorrowDate.Day)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToListAsync();

            // Lượt trả theo ngày
            var returnCounts = await _context.ReturnSlips
                .Where(rs => rs.ReturnDate.Year == currentDate.Year && rs.ReturnDate.Month == currentDate.Month)
                .GroupBy(rs => rs.ReturnDate.Day)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToListAsync();

            // Điền dữ liệu vào mảng
            foreach (var borrow in borrowCounts)
            {
                borrowData[borrow.Day - 1] = borrow.Count;
            }

            foreach (var returnCount in returnCounts)
            {
                returnData[returnCount.Day - 1] = returnCount.Count;
            }

            // 5. Hoạt động gần đây (lấy 3 phiếu mượn gần nhất)
            var recentActivities = await _context.BorrowingSlips
                .Include(bs => bs.BorrowingSlipDetails)
                .ThenInclude(bsd => bsd.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Include(bs => bs.LibraryCard) // Sửa lại: Include LibraryCard
                .ThenInclude(lc => lc.Reader) // Sau đó Include Reader
                .Where(bs => bs.Status == BorrowingSlipStatus.Dangmuon || bs.Status == BorrowingSlipStatus.Quahan)
                .OrderByDescending(bs => bs.BorrowDate)
                .Take(3)
                .Select(bs => new
                {
                    BookTitle = bs.BorrowingSlipDetails.FirstOrDefault().BookCopy.Book.OriginalBookTitle,
                    Category = bs.BorrowingSlipDetails.FirstOrDefault().BookCopy.Book.Category.ToString(),
                    ReaderName = bs.LibraryCard != null ? bs.LibraryCard.Reader.Name : "Không xác định",
                    BorrowDate = bs.BorrowDate
                })
                .ToListAsync();

            // Tạo view model để truyền dữ liệu vào view
            var viewModel = new DashboardViewModel
            {
                TodayBorrows = todayBorrows,
                TodayReturns = todayReturns,
                TotalBooks = totalBooks,
                BorrowedBooks = borrowedBooks,
                PendingReservations = pendingReservations,
                OverdueBooks = overdueBooks,
                BorrowData = borrowData,
                ReturnData = returnData,
                RecentActivities = recentActivities.Select(ra => new RecentActivityViewModel
                {
                    BookTitle = ra.BookTitle,
                    Category = ra.Category,
                    ReaderName = ra.ReaderName,
                    BorrowDate = ra.BorrowDate
                }).ToList()
            };

            return View(viewModel);
        }


        // THỐNG KÊ

        [HttpGet]
        public async Task<IActionResult> Stats()
        {
            // Lấy ngày hiện tại
            var currentDate = DateTime.Now;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // 1. Hoạt động tháng này
            int monthlyBorrows = await _context.BorrowingSlips
                .CountAsync(bs => bs.BorrowDate >= startOfMonth && bs.BorrowDate <= endOfMonth);

            int monthlyReturns = await _context.ReturnSlips
                .CountAsync(rs => rs.ReturnDate >= startOfMonth && rs.ReturnDate <= endOfMonth);

            // 2. Tình trạng sách
            int totalBooks = await _context.BookCopies.CountAsync();

            int borrowedBooks = await _context.BorrowingSlipDetails
                .Where(bsd => bsd.BorrowingSlip.Status == BorrowingSlipStatus.Dangmuon)
                .CountAsync();

            // 3. Hoạt động trả sách
            int totalReturns = await _context.ReturnSlips.CountAsync();

            int overdueBooks = await _context.BorrowingSlipDetails
                .Where(bsd => bsd.BorrowingSlip.Status == BorrowingSlipStatus.Quahan)
                .CountAsync();

            // 4. Thống kê lượt mượn/lượt trả sách (biểu đồ đường)
            var borrowData = new int[12];
            var returnData = new int[12];

            var borrowCounts = await _context.BorrowingSlips
                .Where(bs => bs.BorrowDate.Year == currentDate.Year)
                .GroupBy(bs => bs.BorrowDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            var returnCounts = await _context.ReturnSlips
                .Where(rs => rs.ReturnDate.Year == currentDate.Year)
                .GroupBy(rs => rs.ReturnDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var borrow in borrowCounts)
            {
                borrowData[borrow.Month - 1] = borrow.Count;
            }

            foreach (var returnCount in returnCounts)
            {
                returnData[returnCount.Month - 1] = returnCount.Count;
            }

            // 5. Thống kê sách theo thể loại
            var booksByCategory = await _context.Books
                .GroupBy(b => b.Category)
                .Select(g => new
                {
                    Category = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToDictionaryAsync(g => g.Category, g => g.Count);

            // 6. Thể loại sách được mượn nhiều (tính tỷ lệ)
            var totalBorrowedBooks = await _context.BorrowingSlipDetails.CountAsync();
            var borrowedBooksByCategory = await _context.BorrowingSlipDetails
                .Include(bsd => bsd.BookCopy)
                .ThenInclude(bc => bc.Book)
                .GroupBy(bsd => bsd.BookCopy.Book.Category)
                .Select(g => new
                {
                    Category = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToDictionaryAsync(g => g.Category, g => totalBorrowedBooks > 0 ? (double)g.Count / totalBorrowedBooks * 100 : 0);

            // 7. Danh sách phiếu cần bồi thường
            var compensations = await _context.ReturnSlips
                .Include(rs => rs.BorrowingSlip)
                .ThenInclude(bs => bs.LibraryCard)
                .ThenInclude(lc => lc.Reader)
                .Include(rs => rs.ReturnSlipDetails)
                .Where(rs => rs.ReturnSlipDetails.Any(rsd => rsd.Condition == Condition.Hỏng || rsd.Condition == Condition.Mất))
                .Select(rs => new
                {
                    BorrowingSlipId = rs.BorrowingSlip.BorrowingSlipID.ToString(),
                    ReaderName = rs.BorrowingSlip.LibraryCard.Reader.Name,
                    PhoneNumber = rs.BorrowingSlip.LibraryCard.Reader.PhoneNumber,
                    DamagedBooks = rs.ReturnSlipDetails.Count(rsd => rsd.Condition == Condition.Hỏng),
                    LostBooks = rs.ReturnSlipDetails.Count(rsd => rsd.Condition == Condition.Mất)
                })
                .ToListAsync();

            var compensationList = compensations.Select(c => new CompensationViewModel
            {
                BorrowingSlipId = c.BorrowingSlipId,
                ReaderName = c.ReaderName,
                PhoneNumber = c.PhoneNumber ?? "Không có",
                DamagedBooks = c.DamagedBooks,
                LostBooks = c.LostBooks,
                CompensationAmount = (c.DamagedBooks * 50000) + (c.LostBooks * 100000)
            }).ToList();

            // Tạo view model
            var viewModel = new StatsViewModel
            {
                MonthlyBorrows = monthlyBorrows,
                MonthlyReturns = monthlyReturns,
                TotalBooks = totalBooks,
                BorrowedBooks = borrowedBooks,
                TotalReturns = totalReturns,
                OverdueBooks = overdueBooks,
                BorrowData = borrowData,
                ReturnData = returnData,
                BooksByCategory = booksByCategory,
                BorrowedBooksByCategory = borrowedBooksByCategory,
                Compensations = compensationList
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult LibraryCards()
        {
            return View();
        }
        // Các action mới cho quản lý thẻ thư viện

        // Lấy danh sách thẻ đã phê duyệt (trang chính)
        [HttpGet]
        public async Task<IActionResult> GetApprovedCards()
        {
            var approvedCards = await _context.LibraryCards
                .Include(c => c.Reader)
                .Where(c => c.Status == "Đang hoạt động")
                .Select(c => new
                {
                    cardId = c.CardID,
                    readerId = c.ID,
                    name = c.Reader.Name,
                    createdDate = c.RegistrationDate.ToString("dd/MM/yyyy"),
                    duration = c.ExpirationDate.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(approvedCards);
        }

        // Lấy danh sách thẻ chờ duyệt
        [HttpGet]
        public async Task<IActionResult> GetPendingCards()
        {
            var pendingCards = await _context.LibraryCardRequests
                .Include(r => r.Account)
                .Select(r => new
                {
                    requestId = r.RequestID,
                    cardId = r.RequestID, // Sử dụng RequestID làm CardID tạm thời
                    readerId = r.AccountID,
                    name = r.ReaderName,
                    createdDate = r.RegistrationDate.ToString("dd/MM/yyyy"),
                    duration = r.ExpirationDate.ToString("dd/MM/yyyy"),
                    email = r.Account.Email,
                    phone = r.Account.PhoneNumber,
                    birthday = r.Account.DateOfBirth.ToString("dd/MM/yyyy"),
                    gender = r.Account.Gender,
                    education = r.Account.EducationLevel,
                    address = r.Account.Address
                })
                .ToListAsync();

            return Json(pendingCards);
        }

        // Lấy danh sách thẻ bị khóa
        [HttpGet]
        public async Task<IActionResult> GetLockedCards()
        {
            var lockedCards = await _context.LibraryCards
                .Include(c => c.Reader)
                .Where(c => c.Status == "Unactive")
                .Select(c => new
                {
                    cardId = c.CardID,
                    readerId = c.ID,
                    name = c.Reader.Name,
                    createdDate = c.RegistrationDate.ToString("dd/MM/yyyy"),
                    duration = c.ExpirationDate.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(lockedCards);
        }

        // Lấy chi tiết độc giả từ LibraryCardRequest (khi xem chi tiết từ danh sách chờ duyệt)
        [HttpGet]
        public async Task<IActionResult> GetPendingCardDetails(string requestId)
        {
            var request = await _context.LibraryCardRequests
                .Include(r => r.Account)
                .FirstOrDefaultAsync(r => r.RequestID == requestId);

            if (request == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy yêu cầu đăng ký thẻ." });
            }

            return Json(new
            {
                success = true,
                card = new
                {
                    requestId = request.RequestID,
                    readerId = request.AccountID,
                    name = request.ReaderName,
                    birthday = request.Account.DateOfBirth.ToString("dd/MM/yyyy"),
                    gender = request.Account.Gender,
                    phone = request.Account.PhoneNumber,
                    email = request.Account.Email,
                    education = request.Account.EducationLevel,
                    address = request.Account.Address,
                    createdDate = request.RegistrationDate.ToString("dd/MM/yyyy"),
                    duration = request.ExpirationDate.ToString("dd/MM/yyyy")
                }
            });
        }

        // Lấy chi tiết thẻ thư viện từ LibraryCard (khi xem chi tiết từ danh sách đã phê duyệt hoặc bị khóa)
        [HttpGet]
        public async Task<IActionResult> GetApprovedCardDetails(string cardId)
        {
            var card = await _context.LibraryCards
                .Include(c => c.Reader)
                .FirstOrDefaultAsync(c => c.CardID == cardId);

            if (card == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy thẻ thư viện." });
            }

            return Json(new
            {
                success = true,
                card = new
                {
                    cardId = card.CardID,
                    readerId = card.ID,
                    name = card.Reader.Name,
                    createdDate = card.RegistrationDate.ToString("dd/MM/yyyy"),
                    duration = card.ExpirationDate.ToString("dd/MM/yyyy")
                }
            });
        }

        // Duyệt thẻ (chuyển từ LibraryCardRequest sang LibraryCard, cập nhật Payment)
        [HttpPost]
        public async Task<IActionResult> ApproveCard(string requestId)
        {
            var request = await _context.LibraryCardRequests
                .Include(r => r.Account)
                .FirstOrDefaultAsync(r => r.RequestID == requestId);

            if (request == null)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu đăng ký thẻ." });
            }

            try
            {
                // Tạo thẻ thư viện mới từ yêu cầu
                var libraryCard = new LibraryCard
                {
                    CardID = Nanoid.Generate(size: 10),
                    ID = request.AccountID,
                    RegistrationDate = request.RegistrationDate,
                    ExpirationDate = request.ExpirationDate,
                    Status = "Đang hoạt động",
                    CardPhotoUrl = request.CardPhotoUrl
                };

                _context.LibraryCards.Add(libraryCard);

                // Cập nhật CardID trong bảng Payments
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.Status == "Paid" && p.CardID == null && p.PaymentDate >= request.RegistrationDate);
                if (payment != null)
                {
                    payment.CardID = libraryCard.CardID;
                }

                // Xóa yêu cầu sau khi duyệt
                _context.LibraryCardRequests.Remove(request);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Yêu cầu đã được duyệt thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi duyệt yêu cầu: " + ex.Message });
            }
        }

        // Loại bỏ yêu cầu đăng ký thẻ
        [HttpPost]
        public async Task<IActionResult> RejectCard(string requestId)
        {
            var request = await _context.LibraryCardRequests
                .FirstOrDefaultAsync(r => r.RequestID == requestId);

            if (request == null)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu đăng ký thẻ." });
            }

            try
            {
                _context.LibraryCardRequests.Remove(request);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Yêu cầu đã bị loại bỏ." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi loại bỏ yêu cầu: " + ex.Message });
            }
        }

        // Khóa thẻ
        [HttpPost]
        public async Task<IActionResult> LockCard(string cardId)
        {
            var card = await _context.LibraryCards
                .FirstOrDefaultAsync(c => c.CardID == cardId);

            if (card == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thẻ thư viện." });
            }

            try
            {
                card.Status = "Unactive";
                _context.LibraryCards.Update(card);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Thẻ đã bị khóa." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi khóa thẻ: " + ex.Message });
            }
        }

        // Mở khóa thẻ
        [HttpPost]
        public async Task<IActionResult> UnlockCard(string cardId)
        {
            var card = await _context.LibraryCards
                .FirstOrDefaultAsync(c => c.CardID == cardId);

            if (card == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thẻ thư viện." });
            }

            try
            {
                card.Status = "Đang hoạt động";
                _context.LibraryCards.Update(card);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Thẻ đã được mở khóa." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi mở khóa thẻ: " + ex.Message });
            }
        }




        [HttpGet]
        public IActionResult Books()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ImportReceipt()
        {
            return View();
        }

        // Hiển thị view LoanRequests
        [HttpGet]
        public IActionResult LoanRequests()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetLoanRequests()
        {
            var loanRequests = await _context.LoanRequests
                .Include(lr => lr.LoanRequestDetails)
                .ThenInclude(lrd => lrd.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(lr => lr.Status == LoanRequestStatus.Choduyet)
                .Select(lr => new
                {
                    requestId = lr.RequestID,
                    cardId = lr.CardID,
                    requestDate = lr.RequestDate.ToString("dd/MM/yyyy"),
                    expirationDate = lr.RequestDate.AddDays(2).ToString("dd/MM/yyyy"),
                    status = lr.Status.ToString(),
                    details = lr.LoanRequestDetails.Select(lrd => new
                    {
                        bookCopyId = lrd.BookCopyId,
                        bookTitle = lrd.BookCopy.Book.OriginalBookTitle
                    }).ToList()
                })
                .ToListAsync();

            return Json(loanRequests);
        }
        [HttpGet]
        public async Task<IActionResult> GetProcessedLoanRequests()
        {
            var processedLoanRequests = await _context.LoanRequests
                .Include(lr => lr.LoanRequestDetails)
                .ThenInclude(lrd => lrd.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Where(lr => lr.Status != LoanRequestStatus.Choduyet) // Sửa: So sánh với giá trị enum
                .Select(lr => new
                {
                    requestId = lr.RequestID.ToString(), // Chuyển Guid thành string
                    cardId = lr.CardID.ToString(), // Chuyển Guid thành string
                    requestDate = lr.RequestDate.ToString("dd/MM/yyyy"),
                    status = lr.Status.ToString(), // Chuyển enum thành string
                    details = lr.LoanRequestDetails.Select(lrd => new
                    {
                        bookCopyId = lrd.BookCopyId.ToString(), // Chuyển Guid thành string
                        bookTitle = lrd.BookCopy.Book.OriginalBookTitle
                    }).ToList()
                })
                .ToListAsync();

            return Json(processedLoanRequests);
        }
        [HttpPost]
        public async Task<IActionResult> ApproveLoanRequest(string requestId, string borrowDuration)
        {
            if (!Guid.TryParse(requestId, out Guid parsedRequestId))
                return Json(new { success = false, message = "Mã yêu cầu không hợp lệ." });

            var loanRequest = await _context.LoanRequests
                .Include(lr => lr.LoanRequestDetails)
                .ThenInclude(lrd => lrd.BookCopy)
                .FirstOrDefaultAsync(lr => lr.RequestID == parsedRequestId);

            if (loanRequest == null)
                return Json(new { success = false, message = "Không tìm thấy yêu cầu." });

            if (loanRequest.LoanRequestDetails == null || !loanRequest.LoanRequestDetails.Any())
                return Json(new { success = false, message = "Yêu cầu không có chi tiết sách." });

            try
            {
                var libraryCardExists = await _context.LibraryCards.AnyAsync(lc => lc.CardID == loanRequest.CardID);
                if (!libraryCardExists)
                    return Json(new { success = false, message = "Thẻ thư viện không tồn tại." });

                var bookCopyIds = loanRequest.LoanRequestDetails.Select(lrd => lrd.BookCopyId).ToList();
                var bookCopiesExist = await _context.BookCopies
                    .Where(bc => bookCopyIds.Contains(bc.BookID))
                    .CountAsync() == bookCopyIds.Count;
                if (!bookCopiesExist)
                    return Json(new { success = false, message = "Một hoặc nhiều bản sao sách không tồn tại." });

                var borrowingSlip = new BorrowingSlip
                {
                    BorrowingSlipID = Guid.NewGuid(),
                    CardID = loanRequest.CardID,
                    BorrowDate = DateTime.Now,
                    BorrowDuration = borrowDuration,
                    Status = BorrowingSlipStatus.Dangmuon,
                    BorrowingSlipDetails = loanRequest.LoanRequestDetails.Select(lrd => new BorrowingSlipDetail
                    {
                        BorrowingSlipId = Guid.Empty,
                        BookCopyId = lrd.BookCopyId,
                        BorrowingSlipFK = Guid.Empty,
                        BookCopyFK = lrd.BookCopyId,
                        Condition = Condition.Nguyên_vẹn // Sửa thành enum
                    }).ToList()
                };

                foreach (var detail in borrowingSlip.BorrowingSlipDetails)
                {
                    detail.BorrowingSlipId = borrowingSlip.BorrowingSlipID;
                    detail.BorrowingSlipFK = borrowingSlip.BorrowingSlipID;
                }

                _context.BorrowingSlips.Add(borrowingSlip);
                loanRequest.Status = LoanRequestStatus.Daduyet;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã tạo phiếu mượn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tạo phiếu mượn: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CancelLoanRequest(string requestId)
        {
            if (!Guid.TryParse(requestId, out Guid parsedRequestId))
            {
                return Json(new { success = false, message = "Mã yêu cầu không hợp lệ." });
            }

            var loanRequest = await _context.LoanRequests.FindAsync(parsedRequestId); // Sửa: Tìm với Guid
            if (loanRequest == null)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu." });
            }

            loanRequest.Status = LoanRequestStatus.Bituchoi; // Sửa: Gán giá trị enum
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã hủy yêu cầu thành công!" });
        }

        //BORROW RECORD

        [HttpGet]
        public IActionResult BorrowRecords()
        {
            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> GetBorrowRecords()
        {
            var borrowRecords = await _context.BorrowingSlips
                .Where(bs => bs.Status == BorrowingSlipStatus.Dangmuon)
                .Include(bs => bs.BorrowingSlipDetails)
                .ThenInclude(bsd => bsd.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Join(
                    _context.LibraryCards,
                    bs => bs.CardID,
                    lc => lc.CardID,
                    (bs, lc) => new { bs, lc }
                )
                .Join(
                    _context.Readers,
                    bsLc => bsLc.lc.ID,
                    r => r.ID,
                    (bsLc, r) => new
                    {
                        borrowId = bsLc.bs.BorrowingSlipID,
                        cardId = bsLc.bs.CardID,
                        readerName = r.Name,
                        borrowDate = bsLc.bs.BorrowDate.ToString("dd/MM/yyyy"),
                        duration = bsLc.bs.BorrowDuration,
                        dueDate = bsLc.bs.BorrowDate.AddDays(int.Parse(bsLc.bs.BorrowDuration.Replace(" ngày", ""))).ToString("dd/MM/yyyy"),
                        books = bsLc.bs.BorrowingSlipDetails.Select(bsd => new
                        {
                            bookId = bsd.BookCopyId,
                            name = bsd.BookCopy.Book.OriginalBookTitle,
                            condition = bsd.Condition
                        }).ToList()
                    }
                )
                .ToListAsync();

            return Json(borrowRecords);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReturnSlip([FromBody] ReturnSlipViewModel request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors = errors });
            }

            // Log request để kiểm tra
            Console.WriteLine($"Request: {System.Text.Json.JsonSerializer.Serialize(request)}");

            // Tìm phiếu mượn
            var borrowingSlip = await _context.BorrowingSlips
                .Include(bs => bs.BorrowingSlipDetails)
                .ThenInclude(bsd => bsd.BookCopy)
                .FirstOrDefaultAsync(bs => bs.BorrowingSlipID == request.BorrowingSlipID);

            if (borrowingSlip == null)
            {
                return BadRequest(new { message = $"Không tìm thấy phiếu mượn với ID: {request.BorrowingSlipID}" });
            }

            if (borrowingSlip.Status != BorrowingSlipStatus.Dangmuon)
            {
                return BadRequest(new { message = $"Phiếu mượn không ở trạng thái 'Đang mượn'. Trạng thái hiện tại: {borrowingSlip.Status}" });
            }

            // Tạo phiếu trả
            var returnSlip = new ReturnSlip
            {
                ReturnSlipID = Guid.NewGuid(),
                BorrowingSlipID = borrowingSlip.BorrowingSlipID,
                ReturnDate = request.ReturnDate,
                ReturnSlipDetails = new List<ReturnSlipDetail>()
            };

            // Tạo chi tiết phiếu trả và cập nhật tình trạng sách
            foreach (var detail in request.ReturnDetails)
            {
                var bookCopy = await _context.BookCopies.FindAsync(detail.BookCopyId);
                if (bookCopy == null)
                {
                    return BadRequest(new { message = $"Không tìm thấy bản sao sách với ID: {detail.BookCopyId}" });
                }

                // Cập nhật tình trạng sách
                bookCopy.Condition = detail.Condition;

                var returnSlipDetail = new ReturnSlipDetail
                {
                    ReturnSlipId = returnSlip.ReturnSlipID,
                    BookId = detail.BookCopyId,
                    ReturnSlipFK = returnSlip.ReturnSlipID,
                    BookCopyFK = detail.BookCopyId,
                    Condition = detail.Condition
                };
                returnSlip.ReturnSlipDetails.Add(returnSlipDetail);
            }

            // Cập nhật trạng thái phiếu mượn
            borrowingSlip.Status = BorrowingSlipStatus.Datra;

            // Lưu vào CSDL
            _context.ReturnSlips.Add(returnSlip);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo phiếu trả thành công!" });
        }
        // TÌM KIẾM SÁCH ĐỂ TẠO PHIẾU MƯỢN
        [HttpGet]
        public async Task<IActionResult> GetBookById(string bookId)
        {
            if (string.IsNullOrEmpty(bookId))
            {
                return BadRequest(new { message = "Mã sách không được để trống!" });
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.OriginalBookID == bookId);
            if (book == null)
            {
                return NotFound(new { message = "Không tìm thấy sách!" });
            }

            return Ok(new
            {
                originalBookId = book.OriginalBookID,
                originalBookTitle = book.OriginalBookTitle
            });
        }


        [HttpPost]
        public async Task<IActionResult> CreateBorrowingSlip([FromBody] CreateBorrowingSlipViewModel request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors = errors });
            }

            // Kiểm tra mã thẻ thư viện
            var libraryCard = await _context.LibraryCards
                .Include(lc => lc.Reader)
                .FirstOrDefaultAsync(lc => lc.CardID == request.CardID);
            if (libraryCard == null)
            {
                return BadRequest(new { message = $"Không tìm thấy thẻ thư viện với mã: {request.CardID}" });
            }

            // Kiểm tra độc giả có phiếu mượn "Đang mượn" hoặc "Quá hạn" không
            var existingBorrowingSlip = await _context.BorrowingSlips
                .AnyAsync(bs => bs.CardID == request.CardID &&
                                (bs.Status == BorrowingSlipStatus.Dangmuon || bs.Status == BorrowingSlipStatus.Quahan));
            if (existingBorrowingSlip)
            {
                return BadRequest(new { message = "Độc giả đang có phiếu mượn chưa trả hoặc quá hạn. Vui lòng hoàn thành trước khi tạo phiếu mượn mới." });
            }

            // Kiểm tra số lượng sẵn sàng cho từng sách
            foreach (var bookId in request.BookIds)
            {
                int availableQuantity = await CalculateAvailableQuantity(bookId);
                if (availableQuantity <= 0)
                {
                    var book = await _context.Books.FirstOrDefaultAsync(b => b.OriginalBookID == bookId);
                    return BadRequest(new { message = $"Sách {book?.OriginalBookTitle ?? bookId} hiện không có sẵn để mượn." });
                }
            }

            // Tạo phiếu mượn
            var borrowingSlip = new BorrowingSlip
            {
                BorrowingSlipID = Guid.NewGuid(),
                CardID = request.CardID,
                BorrowDate = request.BorrowDate,
                BorrowDuration = request.BorrowDuration,
                Status = BorrowingSlipStatus.Dangmuon,
                BorrowingSlipDetails = new List<BorrowingSlipDetail>()
            };

            // Tạo chi tiết phiếu mượn
            foreach (var bookId in request.BookIds)
            {
                // Tìm bản sao sách có sẵn (Condition = Nguyên_vẹn và chưa được mượn)
                var usedBookCopyIds = await _context.BorrowingSlipDetails
                    .Where(bsd => _context.BorrowingSlips
                        .Where(bs => bs.Status == BorrowingSlipStatus.Dangmuon || bs.Status == BorrowingSlipStatus.Quahan)
                        .Select(bs => bs.BorrowingSlipID)
                        .Contains(bsd.BorrowingSlipId))
                    .Select(bsd => bsd.BookCopyId)
                    .ToListAsync();

                var bookCopy = await _context.BookCopies
                    .Include(bc => bc.Book)
                    .FirstOrDefaultAsync(bc => bc.OriginalBookID == bookId
                                            && bc.Condition == Condition.Nguyên_vẹn
                                            && !usedBookCopyIds.Contains(bc.BookID));

                if (bookCopy == null)
                {
                    return BadRequest(new { message = $"Không tìm thấy bản sao sách có sẵn cho mã sách: {bookId}" });
                }

                var borrowingSlipDetail = new BorrowingSlipDetail
                {
                    BorrowingSlipId = borrowingSlip.BorrowingSlipID,
                    BookCopyId = bookCopy.BookID,
                    BorrowingSlipFK = borrowingSlip.BorrowingSlipID,
                    BookCopyFK = bookCopy.BookID,
                    Condition = Condition.Nguyên_vẹn
                };
                borrowingSlip.BorrowingSlipDetails.Add(borrowingSlipDetail);
            }

            // Lưu vào CSDL
            _context.BorrowingSlips.Add(borrowingSlip);
            await _context.SaveChangesAsync();

            // Tính ngày đến hạn từ BorrowDuration
            if (string.IsNullOrEmpty(borrowingSlip.BorrowDuration))
            {
                return BadRequest(new { message = "Thời hạn mượn không hợp lệ!" });
            }

            int days;
            try
            {
                days = int.Parse(borrowingSlip.BorrowDuration.Split(' ')[0]);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Thời hạn mượn không hợp lệ: {borrowingSlip.BorrowDuration}", error = ex.Message });
            }

            // Trả về thông tin phiếu mượn để cập nhật giao diện
            var response = new
            {
                borrowId = borrowingSlip.BorrowingSlipID.ToString(),
                cardId = borrowingSlip.CardID,
                readerName = libraryCard.Reader.Name,
                books = borrowingSlip.BorrowingSlipDetails.Select(d => new
                {
                    bookId = d.BookCopyId.ToString(),
                    name = d.BookCopy.Book.OriginalBookTitle,
                    condition = d.Condition.ToString()
                }).ToList(),
                borrowDate = borrowingSlip.BorrowDate.ToString("dd/MM/yyyy"),
                dueDate = borrowingSlip.BorrowDate.AddDays(days).ToString("dd/MM/yyyy"),
                duration = borrowingSlip.BorrowDuration
            };

            return Ok(new { message = "Tạo phiếu mượn thành công!", data = response });
        }

        // HÀM TÍNH SỐ LƯỢNG SÁCH SẴN SÀNG
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


        [HttpGet]
        public async Task<IActionResult> GetBorrowReturnHistory()
        {
            var history = await _context.ReturnSlips
                .Include(rs => rs.BorrowingSlip)
                .Include(rs => rs.ReturnSlipDetails)
                .ThenInclude(rsd => rsd.BookCopy)
                .ThenInclude(bc => bc.Book)
                .Select(rs => new
                {
                    borrowId = rs.BorrowingSlipID.ToString(),
                    returnId = rs.ReturnSlipID.ToString(),
                    cardId = rs.BorrowingSlip.CardID,
                    readerName = _context.LibraryCards
                        .Where(lc => lc.CardID == rs.BorrowingSlip.CardID)
                        .Select(lc => lc.Reader)
                        .Select(r => r.Name)
                        .FirstOrDefault(),
                    borrowDate = rs.BorrowingSlip.BorrowDate.ToString("dd/MM/yyyy"),
                    returnDate = rs.ReturnDate.ToString("dd/MM/yyyy"),
                    duration = rs.BorrowingSlip.BorrowDuration,
                    books = rs.ReturnSlipDetails.Select(rsd => new
                    {
                        bookId = rsd.BookId.ToString(),
                        name = rsd.BookCopy.Book.OriginalBookTitle,
                        condition = rsd.Condition.ToString()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(history);
        }

    }

    // DTO cho yêu cầu thêm/cập nhật độc giả
    public class ReaderRequest
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string EducationLevel { get; set; }
    }

    // DTO cho yêu cầu thêm thẻ thư viện
    public class LibraryCardRequestDTO
    {
        public string CardId { get; set; }
        public string ID { get; set; }
        public string RegistrationDate { get; set; }
        public string ExpirationDate { get; set; }
        public string Status { get; set; }
        public string CardPhotoUrl { get; set; }
    }

}