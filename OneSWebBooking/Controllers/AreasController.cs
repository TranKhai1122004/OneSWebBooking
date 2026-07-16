using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Data;
using OneSWebBooking.Models;
using System.Diagnostics;

namespace OneSWebBooking.Controllers
{
    [Authorize] 
    public class AreasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AreasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Areas (trang duy nhất có giao diện, Create/Edit xử lý qua modal + AJAX)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Areas.ToListAsync());
        }

        // POST: Areas/Create (AJAX, trả JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AreaName,Description,Status")] Area area)
        {
            // 3. ĐỒNG BỘ: Dùng HttpContext.User.Identity để tránh xung đột với Model User
            var currentUsername = HttpContext.User.Identity?.Name ?? "system";

            area.CreatedBy = currentUsername;
            area.CreatedDate = DateTime.Now;
            area.ModifiedBy = currentUsername;
            area.ModifiedDate = DateTime.Now;

            ModelState.Clear();
            TryValidateModel(area);

            if (!string.IsNullOrEmpty(area.AreaName))
            {
                bool isDuplicate = await _context.Areas.AnyAsync(a => a.AreaName.ToLower().Trim() == area.AreaName.ToLower().Trim());
                if (isDuplicate)
                {
                    ModelState.AddModelError("AreaName", "Tên khu vực này đã tồn tại trong hệ thống!");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(area);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ!";
            return BadRequest(firstError);
        }

        // POST: Areas/Edit/5 (AJAX, trả JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,AreaName,Description,Status")] Area area)
        {
            if (id != area.Id) return NotFound();

            var existingArea = await _context.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (existingArea == null) return NotFound();

            // 4. ĐỒNG BỘ: Sửa đổi ở đây từ "User.Identity" thành "HttpContext.User.Identity"
            var currentUsername = HttpContext.User.Identity?.Name ?? "system";

            area.CreatedBy = existingArea.CreatedBy;
            area.CreatedDate = existingArea.CreatedDate;
            area.ModifiedBy = currentUsername;
            area.ModifiedDate = DateTime.Now;

            ModelState.Clear();
            TryValidateModel(area);

            if (!string.IsNullOrEmpty(area.AreaName))
            {
                bool isDuplicate = await _context.Areas.AnyAsync(a => a.AreaName.ToLower().Trim() == area.AreaName.ToLower().Trim() && a.Id != id);
                if (isDuplicate)
                {
                    ModelState.AddModelError("AreaName", "Tên khu vực này đã tồn tại trong hệ thống!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(area);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AreaExists(area.Id)) return NotFound();
                    else throw;
                }
            }

            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ!";
            return BadRequest(firstError);
        }

        // POST: Areas/Delete/5 (AJAX, trả JSON)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // 5. PHÂN QUYỀN NÂNG CAO (Tùy chọn): Nếu bạn chỉ muốn role "Admin" mới được phép Xóa khu vực
        // [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null) return NotFound();

            bool isUsed = await _context.Computers.AnyAsync(c => c.AreaId == id);
            if (isUsed)
            {
                return BadRequest("Không thể xóa khu vực này vì đang có máy tính hoạt động tại đây!");
            }

            var area = await _context.Areas.FindAsync(id);
            if (area != null)
            {
                _context.Areas.Remove(area);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        private bool AreaExists(int? id)
        {
            return _context.Areas.Any(e => e.Id == id);
        }
    }
}