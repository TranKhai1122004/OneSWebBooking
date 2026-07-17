using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Data;
using OneSWebBooking.Helpers;
using OneSWebBooking.Services.Interfaces;
using System.Security.Claims;

namespace OneSWebBooking.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login()
        {
         
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
            var (success, error) = await _accountService.LoginAsync(username, password, site, noPersistent);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Lỗi đăng nhập");
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return RedirectToAction("Login");
        }
    }
}