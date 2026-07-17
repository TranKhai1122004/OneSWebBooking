using OneSWebBooking.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSWebBooking.Services.Interfaces
{
    public interface IComputerService
    {
        Task<List<Computer>> GetAllWithIncludesAsync();
        Task<object> GetDropdownsAsync(int? computerId, int? currentCatId, int? currentAreaId);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(Computer computer, string currentUsername);
        Task<(bool Success, string? ErrorMessage)> EditAsync(int id, Computer computer, string currentUsername);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int? id);
        Task<bool> ComputerExistsAsync(int? id);
    }
}
