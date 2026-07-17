using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Data;
using OneSWebBooking.Models;
using OneSWebBooking.Services.Interfaces;
using System.Collections;

namespace OneSWebBooking.Controllers
{
    [Authorize] 
    public class ComputerCategoriesController : Controller
    {
        private readonly IComputerCategoriesService _categoriesService;

        public ComputerCategoriesController(IComputerCategoriesService categoriesService)
        {
            _categoriesService = categoriesService;
        }

        // GET: ComputerCategories
        public async Task<IActionResult> Index()
        {
            return View(await _categoriesService.GetAllAsync());
        }

        // POST: ComputerCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CategoryName,Description,Status")] ComputerCategory computercategory)
        {
            string currentUsername = HttpContext.User.Identity?.Name ?? "system";
            var (success, error) = await _categoriesService.CreateAsync(computercategory, currentUsername);
            if (success) return Json(new { success = true });
            return BadRequest(error ?? "Dữ liệu không hợp lệ!");
        }

        // POST: ComputerCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,CategoryName,Description,Status")] ComputerCategory computercategory)
        {
            if (id != computercategory.Id) return NotFound();
            string currentUsername = HttpContext.User.Identity?.Name ?? "system";
            var (success, error) = await _categoriesService.EditAsync(id ?? 0, computercategory, currentUsername);
            if (success) return Json(new { success = true });
            if (error == null) return NotFound();
            return BadRequest(error);
        }

        // POST: ComputerCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // 5. PHÂN QUYỀN NÂNG CAO (Tùy chọn): Chỉ có Admin mới được xóa danh mục
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var (success, error) = await _categoriesService.DeleteAsync(id);
            if (success) return Json(new { success = true });
            if (error != null) return BadRequest(error);
            return NotFound();
        }


    }
}