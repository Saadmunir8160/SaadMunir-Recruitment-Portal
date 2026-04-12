using System.Data;
using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Repositories.Query.Base;
using Infra.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;



namespace Infra.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<IdentityService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;

        public IdentityService(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            RoleManager<IdentityRole> roleManager, 
            ILogger<IdentityService> logger,
            IHttpContextAccessor httpContextAccessor,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _dealerRepository = dealerRepository;
        }

        public async Task<bool> AssignUserToRole(string userName, IList<string> roles)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            var result = await _userManager.AddToRolesAsync(user, roles);
            return result.Succeeded;
        }

        public async Task<bool> CreateRoleAsync(string roleName)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors);
            }
            return result.Succeeded;
        }


        public async Task<(bool isSucceed, string userId)> CreateUserAsync(string userName, string password, string email, string fullName, string phoneNo, string role)
        {
            var userExists = await _userManager.FindByNameAsync(userName);
            if (userExists != null)
                return (false, "User already exists");
            else if (!await _roleManager.RoleExistsAsync(role))
                return (false, "Create User Role First");

            var user = new ApplicationUser()
            {
                FullName = fullName,
                UserName = userName,
                Email = email,
                PhoneNumber = phoneNo,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors);
            }

            // Update roles
            var existingRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);

            var addUserRole = await _userManager.AddToRoleAsync(user, role);
            if (!addUserRole.Succeeded)
            {
                throw new ValidationException(addUserRole.Errors);
            }
            return (result.Succeeded, user.Id);
        }

        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            var roleDetails = await _roleManager.FindByIdAsync(roleId);
            if (roleDetails == null)
            {
                throw new NotFoundException("Role not found");
            }

            if (roleDetails.Name == "Administrator")
            {
                throw new BadRequestException("You can not delete Administrator Role");
            }
            
            // Note: IdentityRole doesn't inherit from BaseEntity, so we need to handle this differently
            // For now, we'll keep the existing hard delete behavior for roles
            // If you want soft delete for roles, you would need to add IsDeleted property to IdentityRole
            
            var result = await _roleManager.DeleteAsync(roleDetails);
            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors);
            }
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            // Implement soft delete by setting IsDeleted flag
            // Note: ApplicationUser doesn't inherit from BaseEntity, so we need to handle this differently
            // For now, we'll keep the existing hard delete behavior for users
            // If you want soft delete for users, you would need to add IsDeleted property to ApplicationUser
            
            // Remove all roles from user first
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            // Delete user which will cascade delete related data
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<List<(string id, string fullName, string userName, string email)>> GetAllUsersAsync()
        {
            //_logger.LogInformation("Starting operation Get User...");
            var users = await _userManager.Users.Select(x => new
            {
                x.Id,
                x.FullName,
                x.UserName,
                x.Email
            }).ToListAsync();

            return users.Select(user => (user.Id, user.FullName, user.UserName, user.Email)).ToList();
        }

        public async Task<List<(string id, string fullName, string userName, string email, string phoneNumber, IList<string> roles)>> GetAllUsersDetailsAsync()
        {
            _logger.LogInformation("Starting GetAllUsersDetailsAsync");
            
            var users = await _userManager.Users.ToListAsync();
            _logger.LogInformation($"Found {users.Count} users in UserManager.Users");
            
            var result = new List<(string id, string fullName, string userName, string email, string phoneNumber, IList<string> roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add((user.Id, user.FullName, user.UserName, user.Email, user.PhoneNumber, roles));
            }

            _logger.LogInformation($"Returning {result.Count} users with roles");
            return result;
        }

        public async Task<List<(string id, string fullName, string userName, string email, string phoneNumber, IList<string> roles)>> GetUsersByRoleAsync(string roleName)
        {
            _logger.LogInformation($"Starting GetUsersByRoleAsync for role: {roleName}");
            
            // Check if role exists
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                _logger.LogWarning($"Role '{roleName}' not found");
                return new List<(string id, string fullName, string userName, string email, string phoneNumber, IList<string> roles)>();
            }
            
            // Get all users in the specified role
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            _logger.LogInformation($"Found {usersInRole.Count} users in role '{roleName}'");
            
            var result = new List<(string id, string fullName, string userName, string email, string phoneNumber, IList<string> roles)>();

            foreach (var user in usersInRole)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add((user.Id, user.FullName, user.UserName, user.Email, user.PhoneNumber, roles));
            }

            _logger.LogInformation($"Returning {result.Count} users with role '{roleName}'");
            return result;
        }

        public async Task<List<(string id, string roleName)>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.Select(x => new
            {
                x.Id,
                x.Name
            }).ToListAsync();

            return roles.Select(role => (role.Id, role.Name)).ToList();
        }

        public async Task<(string userId, string fullName, string UserName, string email, string phoneNumber, IList<string> roles)> GetUserDetailsAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            var roles = await _userManager.GetRolesAsync(user);
            return (user.Id, user.FullName, user.UserName, user.Email, user.PhoneNumber, roles);
        }

        public async Task<(string userId, string fullName, string UserName, string email, IList<string> roles)> GetUserDetailsByUserNameAsync(string userName)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            var roles = await _userManager.GetRolesAsync(user);
            return (user.Id, user.FullName, user.UserName, user.Email, roles);
        }

        public async Task<string> GetUserIdAsync(string email)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                throw new NotFoundException("User not found");
                //throw new Exception("User not found");
            }
            return await _userManager.GetUserIdAsync(user);
        }

        public async Task<string> GetUserEmailAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
                //throw new Exception("User not found");
            }
            return user.Email ?? string.Empty;
        }

        public async Task<string> GetUserNameAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
                //throw new Exception("User not found");
            }
            return await _userManager.GetUserNameAsync(user);
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<bool> IsInRoleAsync(string userId, string role)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<bool> IsUniqueUserName(string userName)
        {
            return await _userManager.FindByNameAsync(userName) == null;
        }

        public async Task<bool> SigninUserAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, true, false);
            return result.Succeeded;
        }

        public async Task<bool> UpdateUserProfile(string id, string fullName, string email, string phone, IList<string> roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            user.FullName = fullName;
            user.Email = email;
            user.PhoneNumber = phone;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return false;

            // Update roles
            var existingRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
            if (!removeResult.Succeeded)
                return false;
            var addResult = await _userManager.AddToRolesAsync(user, roles);
            return addResult.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<(string id, string roleName)> GetRoleByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            return (role.Id, role.Name);
        }

        public async Task<bool> UpdateRole(string id, string roleName)
        {
            if (roleName != null)
            {
                var role = await _roleManager.FindByIdAsync(id);
                role.Name = roleName;
                var result = await _roleManager.UpdateAsync(role);
                return result.Succeeded;
            }
            return false;
        }

        public async Task<bool> UpdateUsersRole(string userName, IList<string> usersRole)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var existingRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, existingRoles);
            result = await _userManager.AddToRolesAsync(user, usersRole);

            return result.Succeeded;
        }

        public string? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Look for "UserId" claim first (our specific claim name)
                var userIdClaim = user.FindFirst("UserId") ?? 
                                user.FindFirst(ClaimTypes.NameIdentifier) ?? 
                                user.FindFirst("sub") ?? 
                                user.FindFirst("id") ?? 
                                user.FindFirst("user_id") ??
                                user.FindFirst("uid");

                _logger.LogInformation("Found User ID from claims: {UserId}", userIdClaim?.Value ?? "NULL");
                return userIdClaim?.Value;
            }
            _logger.LogWarning("User is not authenticated or HttpContext is null");
            return null;
        }

        public async Task<int?> GetCurrentDealerIdAsync()
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return null;
            }

            var dealer = await _dealerRepository.GetQueryable()
                .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted && d.IsActive);

            return dealer?.DealerId;
        }
    }
}
