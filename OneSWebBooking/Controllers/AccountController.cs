using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Data;
using OneSWebBooking.Helpers;
using System.Security.Claims;

namespace OneSWebBooking.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (!_context.Users.Any(u => u.Username == "Ones")) // Kiểm tra xem user "Ones" đã tồn tại chưa
            {
                var (hash, salt) = PasswordHelper.HashPassword("123456");
                var adminUser = new OneSWebBooking.Models.User
                {
                    Username = "Ones", // Viết hoa chữ O đúng như bạn muốn
                    Email = "ones@gmail.com",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Role = "Admin",
                    FullName = "Administrator",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(adminUser);
                _context.SaveChanges();
            }
            // ĐỒNG BỘ: Dùng HttpContext.User để tránh xung đột tên gọi với Model User của bạn
            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string site, bool noPersistent)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập và mật khẩu không được để trống.");
                return View();
            }

            // Tìm kiếm tài khoản đang hoạt động trong DB
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View();
            }

            // Thiết lập danh sách Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName ?? user.Username),
                new Claim("SelectedSite", site)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                // noPersistent == true -> Đăng nhập không tạo phiên (IsPersistent = false)[cite: 1]
                IsPersistent = !noPersistent,
                IssuedUtc = DateTimeOffset.UtcNow
            };

            // Thực hiện ghi nhận phiên đăng nhập và tạo Cookie Auth
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Đăng xuất và xóa Cookie phiên
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}