using OneSWebBooking.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSWebBooking.Services.Interfaces
{
    public interface IComputerCategoriesService
    {
        Task<List<ComputerCategory>> GetAllAsync();
        Task<(bool Success, string? ErrorMessage)> CreateAsync(ComputerCategory category, string currentUsername);
        Task<(bool Success, string? ErrorMessage)> EditAsync(int id, ComputerCategory category, string currentUsername);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int? id);
        Task<bool> ComputerCategoryExistsAsync(int? id);
    }
}
