using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebsiteLibrary.Data;
using WebsiteLibrary.Models.Entities;
using WebsiteLibrary.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; // Thêm namespace cho BCrypt


namespace WebsiteLibrary.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = _context.Accounts
                    .FirstOrDefault(a => a.Email == model.Email);

                if (account == null || !BCrypt.Net.BCrypt.Verify(model.Password, account.Password))
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, account.Email),
                    new Claim(ClaimTypes.Role, account.Role),
                    new Claim("FullName", account.Name)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                if (account.Role == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (account.Role == "Reader")
                {
                    return RedirectToAction("Index", "Reader");
                }
                else
                {
                    ModelState.AddModelError("", "Vai trò không hợp lệ.");
                    return View(model);
                }
            }

            return View(model);
        }

        // GET: /Account/Signup
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        // POST: /Account/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại chưa
                if (_context.Accounts.Any(a => a.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                // Tạo đối tượng Reader
                var reader = new Reader
                {
                    Name = model.Name,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password), // Băm mật khẩu
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    EducationLevel = model.EducationLevel,
                    Address = model.Address,
                    Role = "Reader" // Gán vai trò mặc định là "Reader"
                };

                // Tạo mã độc giả mới (DGxxxxx)
                var lastReader = await _context.Readers
                    .OrderByDescending(r => r.ReaderCode)
                    .FirstOrDefaultAsync();
                int newNumber = lastReader == null ? 1 : int.Parse(lastReader.ReaderCode.Replace("DG", "")) + 1;
                reader.ReaderCode = $"DG{newNumber.ToString().PadLeft(5, '0')}";

                // Lưu vào database
                _context.Readers.Add(reader);
                await _context.SaveChangesAsync();

                // Tạo claims cho xác thực
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, reader.Email),
                    new Claim(ClaimTypes.Role, reader.Role),
                    new Claim("FullName", reader.Name),
                    new Claim("ReaderCode", reader.ReaderCode)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                // Đăng nhập ngay sau khi đăng ký
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Reader");
            }

            return View(model);
        }

        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập email.";
                return View();
            }

            var account = _context.Accounts.FirstOrDefault(a => a.Email == email);
            if (account == null)
            {
                ViewBag.ErrorMessage = "Email không tồn tại.";
                return View();
            }

            // Logic gửi email đặt lại mật khẩu (giả lập)
            // Trong thực tế, bạn cần tích hợp dịch vụ email như SendGrid hoặc SMTP
            ViewBag.SuccessMessage = "Một liên kết đặt lại mật khẩu đã được gửi đến email của bạn.";
            return View();
        }

        // POST: /Account/ResetPassword (Ví dụ cho đặt lại mật khẩu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string newPassword)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.Email == email);
            if (account == null)
            {
                return NotFound();
            }

            account.Password = BCrypt.Net.BCrypt.HashPassword(newPassword); // Băm mật khẩu mới
            _context.Update(account);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }
    }

}