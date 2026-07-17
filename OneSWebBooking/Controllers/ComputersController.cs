using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Models;
using OneSWebBooking.Data;
using OneSWebBooking.Services.Interfaces;

namespace OneSWebBooking.Controllers
{
    [Authorize] 
    public class ComputersController : Controller
    {
        private readonly IComputerService _computerService;

        public ComputersController(IComputerService computerService)
        {
            _computerService = computerService;
        }

        // GET: Computers
        public async Task<IActionResult> Index()
        {
            return View(await _computerService.GetAllWithIncludesAsync());
        }

        // API GET: Lấy danh sách dropdown động trả về dạng JSON cho AJAX
        [HttpGet]
        public async Task<IActionResult> GetDropdowns(int? computerId, int? currentCatId, int? currentAreaId)
        {
            var data = await _computerService.GetDropdownsAsync(computerId, currentCatId, currentAreaId);
            return Json(data);
        }

        // POST: Computers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ComputerName,CounterName,IpAddress,MacAddress,ServiceName,ComputerCategoryId,AreaId,PortType,SwallowedCardCount,Status")] Computer computer)
        {
            string currentUsername = HttpContext.User.Identity?.Name ?? "system";
            var (success, error) = await _computerService.CreateAsync(computer, currentUsername);
            if (success) return Json(new { success = true });
            return BadRequest(error ?? "Dữ liệu không hợp lệ!");
        }

        // POST: Computers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,ComputerName,CounterName,IpAddress,MacAddress,ServiceName,ComputerCategoryId,AreaId,PortType,SwallowedCardCount,Status")] Computer computer)
        {
            if (id != computer.Id) return NotFound();
            string currentUsername = HttpContext.User.Identity?.Name ?? "system";
            var (success, error) = await _computerService.EditAsync(id ?? 0, computer, currentUsername);
            if (success) return Json(new { success = true });
            if (error == null) return NotFound();
            return BadRequest(error);
        }

        // POST: Computers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // 5. PHÂN QUYỀN NÂNG CAO (Tùy chọn): Chỉ có Admin mới được xóa máy tính
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var (success, error) = await _computerService.DeleteAsync(id);
            if (success) return Json(new { success = true });
            if (error != null) return BadRequest(error);
            return NotFound();
        }

       
    }
}