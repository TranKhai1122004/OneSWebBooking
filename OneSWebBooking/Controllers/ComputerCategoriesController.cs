using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Models;
using OneSWebBooking.Data;

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

    // GET: ComputerCategories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var computercategory = await _context.ComputerCategories
            .FirstOrDefaultAsync(m => m.Id == id);
        if (computercategory == null) return NotFound();

        return View(computercategory);
    }

    // GET: ComputerCategories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ComputerCategories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,CategoryName,Description,Status")] ComputerCategory computercategory)
    {
        if (!string.IsNullOrEmpty(computercategory.CategoryName))
        {
            bool isDuplicate = await _context.ComputerCategories.AnyAsync(c => c.CategoryName.ToLower().Trim() == computercategory.CategoryName.ToLower().Trim());
            if (isDuplicate)
            {
                ModelState.AddModelError("CategoryName", "Tên nhóm máy tính này đã tồn tại!");
            }
        }

        if (ModelState.IsValid)
        {
            computercategory.CreatedBy = "admin";
            computercategory.CreatedDate = DateTime.Now;

            _context.Add(computercategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(computercategory);
    }

    // GET: ComputerCategories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var computercategory = await _context.ComputerCategories.FindAsync(id);
        if (computercategory == null) return NotFound();
        return View(computercategory);
    }

    // POST: ComputerCategories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,CategoryName,Description,Status")] ComputerCategory computercategory)
    {
        if (id != computercategory.Id) return NotFound();

        if (!string.IsNullOrEmpty(computercategory.CategoryName))
        {
            bool isDuplicate = await _context.ComputerCategories.AnyAsync(c => c.CategoryName.ToLower().Trim() == computercategory.CategoryName.ToLower().Trim() && c.Id != id);
            if (isDuplicate)
            {
                ModelState.AddModelError("CategoryName", "Tên nhóm máy tính này đã tồn tại!");
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Entry(computercategory).Property(x => x.CreatedBy).IsModified = false;
                _context.Entry(computercategory).Property(x => x.CreatedDate).IsModified = false;

                computercategory.ModifiedBy = "admin_edit";
                computercategory.ModifiedDate = DateTime.Now;

                _context.Update(computercategory);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComputerCategoryExists(computercategory.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(computercategory);
    }

    // GET: ComputerCategories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var computercategory = await _context.ComputerCategories
            .FirstOrDefaultAsync(m => m.Id == id);
        if (computercategory == null) return NotFound();

        return View(computercategory);
    }

    // POST: ComputerCategories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null) return NotFound();

        bool isUsed = await _context.Computers.AnyAsync(c => c.ComputerCategoryId == id);
        if (isUsed)
        {
            TempData["ErrorMessage"] = "Không thể xóa nhóm máy tính này vì đang có máy tính hoạt động thuộc nhóm!";
            return RedirectToAction(nameof(Index));
        }

        var computercategory = await _context.ComputerCategories.FindAsync(id);
        if (computercategory != null)
        {
            _context.ComputerCategories.Remove(computercategory);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool ComputerCategoryExists(int? id)
    {
        return _context.ComputerCategories.Any(e => e.Id == id);
    }
}