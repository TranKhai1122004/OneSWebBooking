using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Data;
using OneSWebBooking.Models;
using OneSWebBooking.Services.Interfaces;
using System.Diagnostics;

namespace OneSWebBooking.Controllers
{
    [Authorize] 
    public class AreasController : Controller
    {
        private readonly IAreasService _areasService;

        public AreasController(IAreasService areasService)
        {
            _areasService = areasService;
        }

        // GET: Areas (trang duy nhất có giao diện, Create/Edit xử lý qua modal + AJAX)
        public async Task<IActionResult> Index()
        {
            return View(await _areasService.GetAllAsync());
        }

        // POST: Areas/Create (AJAX, trả JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AreaName,Description,Status")] Area area)
        {
            var currentUsername = HttpContext.User.Identity?.Name ?? "system";
            var (success, error) = await _areasService.CreateAsync(area, currentUsername);
            if (success) return Json(new { success = true });
            return BadRequest(error ?? "Dữ liệu không hợp lệ!");
        }

        // POST: Areas/Edit/5 (AJAX, trả JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,AreaName,Description,Status")] Area area)
        {
            if (id != area.Id) return NotFound();
            var currentUsername = HttpContext.User.Identity?.Name ?? "system";
            var (success, error) = await _areasService.EditAsync(id ?? 0, area, currentUsername);
            if (success) return Json(new { success = true });
            if (error == null) return NotFound();
            return BadRequest(error);
        }

        // POST: Areas/Delete/5 (AJAX, trả JSON)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // 5. PHÂN QUYỀN NÂNG CAO (Tùy chọn): Nếu bạn chỉ muốn role "Admin" mới được phép Xóa khu vực
        // [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var (success, error) = await _areasService.DeleteAsync(id);
            if (success) return Json(new { success = true });
            if (error != null) return BadRequest(error);
            return NotFound();
        }

        
    }
}