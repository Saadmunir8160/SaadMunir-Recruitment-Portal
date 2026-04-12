using System.Threading.Tasks;

namespace Application.Services
{
    public interface IUserUpdateService
    {
        Task<bool> UpdateUserAsync(string userId, string? fullName, string? email, string? phoneNumber);
    }
}