using OneSWebBooking.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSWebBooking.Services.Interfaces
{
    public interface IAreasService
    {
        Task<List<Area>> GetAllAsync();
        Task<(bool Success, string? ErrorMessage)> CreateAsync(Area area, string currentUsername);
        Task<(bool Success, string? ErrorMessage)> EditAsync(int id, Area area, string currentUsername);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int? id);
        Task<bool> AreaExistsAsync(int? id);
    }
}
