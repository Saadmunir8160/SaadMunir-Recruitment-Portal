using Application.Services;
using Infra.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Infra.Services
{
    public class UserQueryService : IUserQueryService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserQueryService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserInfo?> GetUserInfoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            return new UserInfo
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName
            };
        }
    }
}