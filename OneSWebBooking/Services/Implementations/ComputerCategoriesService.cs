using Microsoft.EntityFrameworkCore;
using OneSWebBooking.Data;
using OneSWebBooking.Models;
using OneSWebBooking.Services.Interfaces;

namespace OneSWebBooking.Services.Implementations
{
    public class ComputerCategoriesService : IComputerCategoriesService
    {
        private readonly ApplicationDbContext _context;

        public ComputerCategoriesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ComputerCategory>> GetAllAsync()
        {
            return await _context.ComputerCategories.ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(ComputerCategory computercategory, string currentUsername)
        {
            computercategory.CreatedBy = currentUsername;
            computercategory.CreatedDate = DateTime.Now;
            computercategory.ModifiedBy = currentUsername;
            computercategory.ModifiedDate = DateTime.Now;

            if (!string.IsNullOrEmpty(computercategory.CategoryName))
            {
                bool isDuplicate = await _context.ComputerCategories.AnyAsync(c => c.CategoryName.ToLower().Trim() == computercategory.CategoryName.ToLower().Trim());
                if (isDuplicate) return (false, "Tên nhóm máy tính này đã tồn tại trong hệ thống!");
            }

            _context.Add(computercategory);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> EditAsync(int id, ComputerCategory computercategory, string currentUsername)
        {
            var existingCategory = await _context.ComputerCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingCategory == null) return (false, null);

            computercategory.CreatedBy = existingCategory.CreatedBy;
            computercategory.CreatedDate = existingCategory.CreatedDate;
            computercategory.ModifiedBy = currentUsername;
            computercategory.ModifiedDate = DateTime.Now;

            if (!string.IsNullOrEmpty(computercategory.CategoryName))
            {
                bool isDuplicate = await _context.ComputerCategories.AnyAsync(c => c.CategoryName.ToLower().Trim() == computercategory.CategoryName.ToLower().Trim() && c.Id != id);
                if (isDuplicate) return (false, "Tên nhóm máy tính này đã tồn tại trong hệ thống!");
            }

            try
            {
                _context.Update(computercategory);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComputerCategoryExists(computercategory.Id)) return (false, null);
                else throw;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int? id)
        {
            if (id == null) return (false, null);

            bool isUsed = await _context.Computers.AnyAsync(c => c.ComputerCategoryId == id);
            if (isUsed) return (false, "Không thể xóa nhóm máy tính này vì đang có máy tính hoạt động thuộc nhóm!");

            var computercategory = await _context.ComputerCategories.FindAsync(id);
            if (computercategory != null)
            {
                _context.ComputerCategories.Remove(computercategory);
                await _context.SaveChangesAsync();
            }

            return (true, null);
        }

        public Task<bool> ComputerCategoryExistsAsync(int? id)
        {
            return Task.FromResult(ComputerCategoryExists(id));
        }

        private bool ComputerCategoryExists(int? id)
        {
            return _context.ComputerCategories.Any(e => e.Id == id);
        }
    }
}
