using System.Threading.Tasks;

namespace OneSWebBooking.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password, string site, bool noPersistent);
        Task LogoutAsync();
    }
}
