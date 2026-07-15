using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Models;
using OneSWebBooking.Data;

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

    // GET: Computers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var computer = await _context.Computers
            .Include(c => c.ComputerCategory)
            .Include(c => c.Area)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (computer == null) return NotFound();

        return View(computer);
    }

    // GET: Computers/Create
    public IActionResult Create()
    {
        ViewData["ComputerCategoryId"] = new SelectList(_context.ComputerCategories.Where(x => x.Status == true), "Id", "CategoryName");
        ViewData["AreaId"] = new SelectList(_context.Areas.Where(x => x.Status == true), "Id", "AreaName");
        return View();
    }

    // POST: Computers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ComputerName,CounterName,IpAddress,MacAddress,ServiceName,ComputerCategoryId,AreaId,PortType,SwallowedCardCount,Status")] Computer computer)
    {
        if (!string.IsNullOrEmpty(computer.ComputerName))
        {
            // Check trùng tên máy
            bool isNameDuplicate = await _context.Computers.AnyAsync(c => c.ComputerName.ToLower().Trim() == computer.ComputerName.ToLower().Trim());
            if (isNameDuplicate)
            {
                ModelState.AddModelError("ComputerName", "Tên máy tính này đã tồn tại!");
            }
        }

        if (!string.IsNullOrEmpty(computer.IpAddress))
        {
            // Check trùng IP
            bool isIpDuplicate = await _context.Computers.AnyAsync(c => c.IpAddress.Trim() == computer.IpAddress.Trim());
            if (isIpDuplicate)
            {
                ModelState.AddModelError("IpAddress", "Địa chỉ IP này đã được gán cho máy khác!");
            }
        }

        if (ModelState.IsValid)
        {
            computer.CreatedBy = "admin";
            computer.CreatedDate = DateTime.Now;

            _context.Add(computer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["ComputerCategoryId"] = new SelectList(_context.ComputerCategories.Where(x => x.Status == true), "Id", "CategoryName", computer.ComputerCategoryId);
        ViewData["AreaId"] = new SelectList(_context.Areas.Where(x => x.Status == true), "Id", "AreaName", computer.AreaId);
        return View(computer);
    }

    // GET: Computers/Edit/5 (HÀM GET - ĐÃ SỬA DROPDOWN)
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var computer = await _context.Computers.FindAsync(id);
        if (computer == null) return NotFound();

        // Nạp Dropdown lấy cả Group/Area hiện tại dù nó đã bị khóa (Status == false)
        ViewData["ComputerCategoryId"] = new SelectList(
            _context.ComputerCategories.Where(x => x.Status == true || x.Id == computer.ComputerCategoryId),
            "Id", "CategoryName", computer.ComputerCategoryId);

        ViewData["AreaId"] = new SelectList(
            _context.Areas.Where(x => x.Status == true || x.Id == computer.AreaId),
            "Id", "AreaName", computer.AreaId);

        return View(computer);
    }

    // POST: Computers/Edit/5 (HÀM POST - ĐÃ SỬA DROPDOWN)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,ComputerName,CounterName,IpAddress,MacAddress,ServiceName,ComputerCategoryId,AreaId,PortType,SwallowedCardCount,Status")] Computer computer)
    {
        if (id != computer.Id) return NotFound();

        if (!string.IsNullOrEmpty(computer.ComputerName))
        {
            // Check trùng tên khi sửa (loại trừ chính nó)
            bool isNameDuplicate = await _context.Computers.AnyAsync(c => c.ComputerName.ToLower().Trim() == computer.ComputerName.ToLower().Trim() && c.Id != id);
            if (isNameDuplicate)
            {
                ModelState.AddModelError("ComputerName", "Tên máy tính này đã tồn tại!");
            }
        }

        if (!string.IsNullOrEmpty(computer.IpAddress))
        {
            // Check trùng IP khi sửa (loại trừ chính nó)
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
                _context.Entry(computer).Property(x => x.CreatedBy).IsModified = false;
                _context.Entry(computer).Property(x => x.CreatedDate).IsModified = false;

                computer.ModifiedBy = "admin_edit";
                computer.ModifiedDate = DateTime.Now;

                _context.Update(computer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComputerExists(computer.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // Nếu lỗi form, nạp lại Dropdown an toàn chứa cả phần tử hiện tại
        ViewData["ComputerCategoryId"] = new SelectList(
            _context.ComputerCategories.Where(x => x.Status == true || x.Id == computer.ComputerCategoryId),
            "Id", "CategoryName", computer.ComputerCategoryId);

        ViewData["AreaId"] = new SelectList(
            _context.Areas.Where(x => x.Status == true || x.Id == computer.AreaId),
            "Id", "AreaName", computer.AreaId);

        return View(computer);
    }

    // GET: Computers/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var computer = await _context.Computers
            .Include(c => c.ComputerCategory)
            .Include(c => c.Area)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (computer == null) return NotFound();

        return View(computer);
    }

    // POST: Computers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null) return NotFound();

        var computer = await _context.Computers.FindAsync(id);
        if (computer == null) return NotFound();

        if (computer.Status == true)
        {
            TempData["ErrorMessage"] = "Không thể xóa máy tính này vì máy đang ở trạng thái 'Đang hoạt động'!";
            return RedirectToAction(nameof(Index));
        }

        _context.Computers.Remove(computer);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private bool ComputerExists(int? id)
    {
        return _context.Computers.Any(e => e.Id == id);
    }
}