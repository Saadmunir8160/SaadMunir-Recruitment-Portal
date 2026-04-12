using System.Threading.Tasks;

namespace Application.Services
{
    public class UserInfo
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
    }

    public interface IUserQueryService
    {
        Task<UserInfo?> GetUserInfoAsync(string userId);
    }
}