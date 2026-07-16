using Microsoft.AspNetCore.Authorization; // 1. THÊM THƯ VIỆN NÀY ĐỂ SỬ DỤNG [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Data;
using OneSWebBooking.Models;
using System.Collections;

namespace OneSWebBooking.Controllers
{
    [Authorize] // 2. CHẶN TRUY CẬP: Bắt buộc đăng nhập mới được thao tác với nhóm máy tính
    public class ComputerCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComputerCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ComputerCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.ComputerCategories.ToListAsync());
        }

        // POST: ComputerCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CategoryName,Description,Status")] ComputerCategory computercategory)
        {
            // 3. ĐỒNG BỘ: Lấy Username từ HttpContext của tài khoản đang đăng nhập thực tế
            string currentUsername = HttpContext.User.Identity?.Name ?? "system";

            computercategory.CreatedBy = currentUsername;
            computercategory.CreatedDate = DateTime.Now;
            computercategory.ModifiedBy = currentUsername;
            computercategory.ModifiedDate = DateTime.Now;

            // Xóa cache và validate lại thực thể sạch
            ModelState.Clear();
            TryValidateModel(computercategory);

            if (!string.IsNullOrEmpty(computercategory.CategoryName))
            {
                bool isDuplicate = await _context.ComputerCategories.AnyAsync(c => c.CategoryName.ToLower().Trim() == computercategory.CategoryName.ToLower().Trim());
                if (isDuplicate)
                {
                    ModelState.AddModelError("CategoryName", "Tên nhóm máy tính này đã tồn tại trong hệ thống!");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(computercategory);
                await _context.SaveChangesAsync();
                return Json(new { success = true }); // Trả về JSON cho AJAX
            }

            // Trả về lỗi 400 kèm câu lỗi đầu tiên
            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ!";
            return BadRequest(firstError);
        }

        // POST: ComputerCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,CategoryName,Description,Status")] ComputerCategory computercategory)
        {
            if (id != computercategory.Id) return NotFound();

            // Lấy thông tin bản ghi cũ từ DB để giữ nguyên ngày tạo/người tạo
            var existingCategory = await _context.ComputerCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingCategory == null) return NotFound();

            // 4. ĐỒNG BỘ: Sửa đổi ở đây từ "User.Identity" thành "HttpContext.User.Identity" cho đồng bộ và tường minh
            string currentUsername = HttpContext.User.Identity?.Name ?? "system";

            computercategory.CreatedBy = existingCategory.CreatedBy;
            computercategory.CreatedDate = existingCategory.CreatedDate;
            computercategory.ModifiedBy = currentUsername;
            computercategory.ModifiedDate = DateTime.Now;

            ModelState.Clear();
            TryValidateModel(computercategory);

            if (!string.IsNullOrEmpty(computercategory.CategoryName))
            {
                bool isDuplicate = await _context.ComputerCategories.AnyAsync(c => c.CategoryName.ToLower().Trim() == computercategory.CategoryName.ToLower().Trim() && c.Id != id);
                if (isDuplicate)
                {
                    ModelState.AddModelError("CategoryName", "Tên nhóm máy tính này đã tồn tại trong hệ thống!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(computercategory);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true }); // Trả về JSON thành công
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComputerCategoryExists(computercategory.Id)) return NotFound();
                    else throw;
                }
            }

            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ!";
            return BadRequest(firstError);
        }

        // POST: ComputerCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // 5. PHÂN QUYỀN NÂNG CAO (Tùy chọn): Chỉ có Admin mới được xóa danh mục
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null) return NotFound();

            // Kiểm tra xem nhóm này đã được dùng ở máy tính nào chưa
            bool isUsed = await _context.Computers.AnyAsync(c => c.ComputerCategoryId == id);
            if (isUsed)
            {
                return BadRequest("Không thể xóa nhóm máy tính này vì đang có máy tính hoạt động thuộc nhóm!");
            }
            
            var computercategory = await _context.ComputerCategories.FindAsync(id);
            if (computercategory != null)
            {
                _context.ComputerCategories.Remove(computercategory);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true }); // Trả về JSON cho AJAX xóa
        }

        private bool ComputerCategoryExists(int? id)
        {
            return _context.ComputerCategories.Any(e => e.Id == id);
        }
    }
}