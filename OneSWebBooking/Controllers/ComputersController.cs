using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Models;
using OneSWebBooking.Data;

namespace OneSWebBooking.Controllers
{
    [Authorize] 
    public class ComputersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComputersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Computers
        public async Task<IActionResult> Index()
        {
            var computers = _context.Computers
                .Include(c => c.ComputerCategory)
                .Include(c => c.Area);
            return View(await computers.ToListAsync());
        }

        // API GET: Lấy danh sách dropdown động trả về dạng JSON cho AJAX
        [HttpGet]
        public async Task<IActionResult> GetDropdowns(int? computerId, int? currentCatId, int? currentAreaId)
        {
            // Lấy nhóm máy: Lấy danh sách đang hoạt động (Status == true) cộng thêm chính nó nếu đang xem/sửa
            var categories = await _context.ComputerCategories
                .Where(x => x.Status == true || x.Id == currentCatId)
                .Select(x => new { id = x.Id, name = x.CategoryName })
                .ToListAsync();

            // Lấy khu vực: Lấy danh sách đang hoạt động (Status == true) cộng thêm chính nó nếu đang xem/sửa
            var areas = await _context.Areas
                .Where(x => x.Status == true || x.Id == currentAreaId)
                .Select(x => new { id = x.Id, name = x.AreaName })
                .ToListAsync();

            return Json(new { categories, areas });
        }

        // POST: Computers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ComputerName,CounterName,IpAddress,MacAddress,ServiceName,ComputerCategoryId,AreaId,PortType,SwallowedCardCount,Status")] Computer computer)
        {
            // 3. ĐỒNG BỘ: Lấy Username từ HttpContext của tài khoản đăng nhập thực tế
            string currentUsername = HttpContext.User.Identity?.Name ?? "system";

            // Điền mặc định thông tin Audit hệ thống
            computer.CreatedBy = currentUsername;
            computer.CreatedDate = DateTime.Now;
            computer.ModifiedBy = currentUsername;
            computer.ModifiedDate = DateTime.Now;

            // FIX BUG 2: Chạy validate model mặc định trước
            ModelState.Clear();
            TryValidateModel(computer);

            // Kiểm tra logic ACM đặc thù và thêm lỗi sau khi đã Clear
            var category = await _context.ComputerCategories.FindAsync(computer.ComputerCategoryId);
            bool isAcm = category != null && category.CategoryName.ToUpper().Contains("ACM");

            if (isAcm)
            {
                if (string.IsNullOrEmpty(computer.PortType))
                {
                    ModelState.AddModelError("PortType", "Nhóm máy ACM yêu cầu phải nhập Loại cổng!");
                }
                if (computer.SwallowedCardCount < 0)
                {
                    ModelState.AddModelError("SwallowedCardCount", "Nhóm máy ACM yêu cầu nhập Số lượng thẻ nuốt hợp lệ!");
                }
            }
            else
            {
                // Nếu không phải ACM, xóa trắng 2 trường này để tránh lưu rác vào DB
                computer.PortType = null;
                computer.SwallowedCardCount = 0;
            }

            // Check trùng tên máy
            if (!string.IsNullOrEmpty(computer.ComputerName))
            {
                bool isNameDuplicate = await _context.Computers.AnyAsync(c => c.ComputerName.ToLower().Trim() == computer.ComputerName.ToLower().Trim());
                if (isNameDuplicate)
                {
                    ModelState.AddModelError("ComputerName", "Tên máy tính này đã tồn tại!");
                }
            }

            // Check trùng IP
            if (!string.IsNullOrEmpty(computer.IpAddress))
            {
                bool isIpDuplicate = await _context.Computers.AnyAsync(c => c.IpAddress.Trim() == computer.IpAddress.Trim());
                if (isIpDuplicate)
                {
                    ModelState.AddModelError("IpAddress", "Địa chỉ IP này đã được gán cho máy khác!");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(computer);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ!";
            return BadRequest(firstError);
        }

        // POST: Computers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,ComputerName,CounterName,IpAddress,MacAddress,ServiceName,ComputerCategoryId,AreaId,PortType,SwallowedCardCount,Status")] Computer computer)
        {
            if (id != computer.Id) return NotFound();

            var existingComputer = await _context.Computers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingComputer == null) return NotFound();

            // 4. ĐỒNG BỘ: Sửa đổi ở đây từ "admin_edit" thành "HttpContext.User.Identity" và giữ lại audit cũ
            string currentUsername = HttpContext.User.Identity?.Name ?? "system";

            computer.CreatedBy = existingComputer.CreatedBy;
            computer.CreatedDate = existingComputer.CreatedDate;
            computer.ModifiedBy = currentUsername;
            computer.ModifiedDate = DateTime.Now;

            // FIX BUG 2: Chạy validate model mặc định trước
            ModelState.Clear();
            TryValidateModel(computer);

            // Kiểm tra logic ACM đặc thù khi sửa sau khi đã Clear
            var category = await _context.ComputerCategories.FindAsync(computer.ComputerCategoryId);
            bool isAcm = category != null && category.CategoryName.ToUpper().Contains("ACM");

            if (isAcm)
            {
                if (string.IsNullOrEmpty(computer.PortType))
                {
                    ModelState.AddModelError("PortType", "Nhóm máy ACM yêu cầu phải nhập Loại cổng!");
                }
                if (computer.SwallowedCardCount < 0)
                {
                    ModelState.AddModelError("SwallowedCardCount", "Nhóm máy ACM yêu cầu nhập Số lượng thẻ nuốt hợp lệ!");
                }
            }
            else
            {
                computer.PortType = null;
                computer.SwallowedCardCount = 0;
            }

            // Check trùng tên khi sửa (loại trừ chính nó)
            if (!string.IsNullOrEmpty(computer.ComputerName))
            {
                bool isNameDuplicate = await _context.Computers.AnyAsync(c => c.ComputerName.ToLower().Trim() == computer.ComputerName.ToLower().Trim() && c.Id != id);
                if (isNameDuplicate)
                {
                    ModelState.AddModelError("ComputerName", "Tên máy tính này đã tồn tại!");
                }
            }

            // FIX BUG 1: Thêm check trùng IP khi sửa (loại trừ chính nó)
            if (!string.IsNullOrEmpty(computer.IpAddress))
            {
                bool isIpDuplicate = await _context.Computers.AnyAsync(c => c.IpAddress.Trim() == computer.IpAddress.Trim() && c.Id != id);
                if (isIpDuplicate)
                {
                    ModelState.AddModelError("IpAddress", "Địa chỉ IP này đã được gán cho máy khác!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(computer);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComputerExists(computer.Id)) return NotFound();
                    else throw;
                }
            }

            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ!";
            return BadRequest(firstError);
        }

        // POST: Computers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // 5. PHÂN QUYỀN NÂNG CAO (Tùy chọn): Chỉ có Admin mới được xóa máy tính
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null) return NotFound();

            var computer = await _context.Computers.FindAsync(id);
            if (computer == null) return NotFound();

            // Ràng buộc nghiệp vụ cũ: Máy đang hoạt động (Status = true) thì không được xóa
            if (computer.Status)
            {
                return BadRequest("Không thể xóa máy tính này vì máy đang ở trạng thái 'Đang hoạt động'!");
            }

            _context.Computers.Remove(computer);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private bool ComputerExists(int? id)
        {
            return _context.Computers.Any(e => e.Id == id);
        }
    }
}