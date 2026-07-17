using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Data;
using OneSWebBooking.Models;
using OneSWebBooking.Services.Interfaces;

namespace OneSWebBooking.Services.Implementations
{
    public class AreasService : IAreasService
    {
        private readonly ApplicationDbContext _context; // Khai báo _context có kiểu dữ liệu ApplicationDbContext để truy cập cơ sở dữ liệu.

        public AreasService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Area>> GetAllAsync()
        {
            return await _context.Areas.ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(Area area, string currentUsername)
        {
            area.CreatedBy = currentUsername;
            area.CreatedDate = DateTime.Now;
            area.ModifiedBy = currentUsername;
            area.ModifiedDate = DateTime.Now;

            if (!string.IsNullOrEmpty(area.AreaName))
            {
                bool isDuplicate = await _context.Areas.AnyAsync(a => a.AreaName.ToLower().Trim() == area.AreaName.ToLower().Trim());
                if (isDuplicate) return (false, "Tên khu vực này đã tồn tại trong hệ thống!");
            }

            _context.Add(area);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> EditAsync(int id, Area area, string currentUsername)
        {
            var existingArea = await _context.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (existingArea == null) return (false, null);

            area.CreatedBy = existingArea.CreatedBy;
            area.CreatedDate = existingArea.CreatedDate;
            area.ModifiedBy = currentUsername;
            area.ModifiedDate = DateTime.Now;

            if (!string.IsNullOrEmpty(area.AreaName))
            {
                bool isDuplicate = await _context.Areas.AnyAsync(a => a.AreaName.ToLower().Trim() == area.AreaName.ToLower().Trim() && a.Id != id);
                if (isDuplicate) return (false, "Tên khu vực này đã tồn tại trong hệ thống!");
            }

            try
            {
                _context.Update(area);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AreaExists(id)) return (false, null);
                else throw;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int? id)
        {
            if (id == null) return (false, null);

            bool isUsed = await _context.Computers.AnyAsync(c => c.AreaId == id);
            if (isUsed) return (false, "Không thể xóa khu vực này vì đang có máy tính hoạt động tại đây!");

            var area = await _context.Areas.FindAsync(id);
            if (area != null)
            {
                _context.Areas.Remove(area);
                await _context.SaveChangesAsync();
            }

            return (true, null);
        }

        public Task<bool> AreaExistsAsync(int? id)
        {
            return Task.FromResult(AreaExists(id));
        }

        private bool AreaExists(int? id)
        {
            return _context.Areas.Any(e => e.Id == id);
        }
    }
}
