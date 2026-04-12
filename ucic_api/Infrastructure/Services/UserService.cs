using Application.Common.Interfaces;
using Application.DTOs;
using Infra.Data;
using Infra.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<DealerUserDTO>> GetAllDealerUsersAsync()
        {
            var dealerUsers = new List<DealerUserDTO>();
            
            // Get all users with Dealer role
            var users = await _userManager.Users
                .Include(u => u.UserType)
                .ToListAsync();
            
            foreach (var user in users)
            {
                // Check if user has "Dealer" role
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Dealer"))
                {
                    dealerUsers.Add(new DealerUserDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        CreditLimit = user.CreditLimit,
                        LnId = user.LnId,
                        UserTypeId = user.UserTypeId,
                        UserTypeName = user.UserType?.Name
                    });
                }
            }
            
            return dealerUsers;
        }
    }
} 