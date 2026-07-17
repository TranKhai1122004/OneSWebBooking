using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Data;
using OneSWebBooking.Models;
using OneSWebBooking.Services.Interfaces;

namespace OneSWebBooking.Services.Implementations
{
    public class ComputerService : IComputerService
    {
        private readonly ApplicationDbContext _context;

        public ComputerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Computer>> GetAllWithIncludesAsync()
        {
            var computers = _context.Computers
                .Include(c => c.ComputerCategory)
                .Include(c => c.Area);
            return await computers.ToListAsync();
        }

        public async Task<object> GetDropdownsAsync(int? computerId, int? currentCatId, int? currentAreaId)
        {
            var categories = await _context.ComputerCategories
                .Where(x => x.Status == true || x.Id == currentCatId)
                .Select(x => new { id = x.Id, name = x.CategoryName })
                .ToListAsync();

            var areas = await _context.Areas
                .Where(x => x.Status == true || x.Id == currentAreaId)
                .Select(x => new { id = x.Id, name = x.AreaName })
                .ToListAsync();

            return new { categories, areas };
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(Computer computer, string currentUsername)
        {
            computer.CreatedBy = currentUsername;
            computer.CreatedDate = DateTime.Now;
            computer.ModifiedBy = currentUsername;
            computer.ModifiedDate = DateTime.Now;

            var category = await _context.ComputerCategories.FindAsync(computer.ComputerCategoryId);
            bool isAcm = category != null && category.CategoryName.ToUpper().Contains("ACM");

            if (isAcm)
            {
                if (string.IsNullOrEmpty(computer.PortType))
                {
                    return (false, "Nhóm máy ACM yêu cầu phải nhập Loại cổng!");
                }
                if (computer.SwallowedCardCount < 0)
                {
                    return (false, "Nhóm máy ACM yêu cầu nhập Số lượng thẻ nuốt hợp lệ!");
                }
            }
            else
            {
                computer.PortType = null;
                computer.SwallowedCardCount = 0;
            }

            if (!string.IsNullOrEmpty(computer.ComputerName))
            {
                bool isNameDuplicate = await _context.Computers.AnyAsync(c => c.ComputerName.ToLower().Trim() == computer.ComputerName.ToLower().Trim());
                if (isNameDuplicate) return (false, "Tên máy tính này đã tồn tại!");
            }

            if (!string.IsNullOrEmpty(computer.IpAddress))
            {
                bool isIpDuplicate = await _context.Computers.AnyAsync(c => c.IpAddress.Trim() == computer.IpAddress.Trim());
                if (isIpDuplicate) return (false, "Địa chỉ IP này đã được gán cho máy khác!");
            }

            _context.Add(computer);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> EditAsync(int id, Computer computer, string currentUsername)
        {
            var existingComputer = await _context.Computers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingComputer == null) return (false, null);

            computer.CreatedBy = existingComputer.CreatedBy;
            computer.CreatedDate = existingComputer.CreatedDate;
            computer.ModifiedBy = currentUsername;
            computer.ModifiedDate = DateTime.Now;

            var category = await _context.ComputerCategories.FindAsync(computer.ComputerCategoryId);
            bool isAcm = category != null && category.CategoryName.ToUpper().Contains("ACM");

            if (isAcm)
            {
                if (string.IsNullOrEmpty(computer.PortType))
                {
                    return (false, "Nhóm máy ACM yêu cầu phải nhập Loại cổng!");
                }
                if (computer.SwallowedCardCount < 0)
                {
                    return (false, "Nhóm máy ACM yêu cầu nhập Số lượng thẻ nuốt hợp lệ!");
                }
            }
            else
            {
                computer.PortType = null;
                computer.SwallowedCardCount = 0;
            }

            if (!string.IsNullOrEmpty(computer.ComputerName))
            {
                bool isNameDuplicate = await _context.Computers.AnyAsync(c => c.ComputerName.ToLower().Trim() == computer.ComputerName.ToLower().Trim() && c.Id != id);
                if (isNameDuplicate) return (false, "Tên máy tính này đã tồn tại!");
            }

            if (!string.IsNullOrEmpty(computer.IpAddress))
            {
                bool isIpDuplicate = await _context.Computers.AnyAsync(c => c.IpAddress.Trim() == computer.IpAddress.Trim() && c.Id != id);
                if (isIpDuplicate) return (false, "Địa chỉ IP này đã được gán cho máy khác!");
            }

            try
            {
                _context.Update(computer);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComputerExists(computer.Id)) return (false, null);
                else throw;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int? id)
        {
            if (id == null) return (false, null);

            var computer = await _context.Computers.FindAsync(id);
            if (computer == null) return (false, null);

            if (computer.Status)
            {
                return (false, "Không thể xóa máy tính này vì máy đang ở trạng thái 'Đang hoạt động'!");
            }

            _context.Computers.Remove(computer);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public Task<bool> ComputerExistsAsync(int? id)
        {
            return Task.FromResult(ComputerExists(id));
        }

        private bool ComputerExists(int? id)
        {
            return _context.Computers.Any(e => e.Id == id);
        }
    }
}
