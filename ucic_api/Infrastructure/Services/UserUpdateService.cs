using Application.Services;
using Infra.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Infra.Services
{
    public class UserUpdateService : IUserUpdateService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserUpdateService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> UpdateUserAsync(string userId, string? fullName, string? email, string? phoneNumber)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            bool hasChanges = false;

            if (!string.IsNullOrEmpty(fullName) && user.FullName != fullName)
            {
                user.FullName = fullName;
                hasChanges = true;
            }

            if (!string.IsNullOrEmpty(email) && user.Email != email)
            {
                user.Email = email;
                user.UserName = email; // Update username to match email
                hasChanges = true;
            }

            if (!string.IsNullOrEmpty(phoneNumber) && user.PhoneNumber != phoneNumber)
            {
                user.PhoneNumber = phoneNumber;
                hasChanges = true;
            }

            if (hasChanges)
            {
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }

            return true; // No changes needed
        }
    }
}