using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infra.Data;
using Domain.Entities;

namespace Infra.Identity.Seeding
{
    public class DataSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DataSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Create all required roles
            await EnsureRoleExistsAsync("Admin");
            await EnsureRoleExistsAsync("User");
            await EnsureRoleExistsAsync("Dealer");
            await EnsureRoleExistsAsync("DealerDriver");

            // Recruitment Management System roles
            await EnsureRoleExistsAsync("HR");                 // General HR — vacancies, RMS (name must match JWT)
            await EnsureRoleExistsAsync("Recruiter");          // HR staff — manage candidates, verify, shortlist
            await EnsureRoleExistsAsync("HRSupervisor");       // Senior HR / Section Head — conducts HR interviews, approves vacancies
            await EnsureRoleExistsAsync("HR Manager");         // Vacancy approval step 1 (RMS)
            await EnsureRoleExistsAsync("HR Section Head");   // Vacancy approval step 2 (RMS)
            await EnsureRoleExistsAsync("HiringManager");      // Department manager/lead — technical interviews, shortlist
            await EnsureRoleExistsAsync("VPO");                // VP Operations — offer approval chain
            await EnsureRoleExistsAsync("MedicalProvider");    // External provider — portal access + upload medical reports
            await EnsureRoleExistsAsync("Candidate");          // Job seekers — register, apply, view status

            // Create admin user
            var adminEmail = "admin@ucic.com";
            var adminUserName = "admin";
            var adminPassword = "Admin@123";

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };

                var result = await _userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newAdmin, "Admin");
                }
                else
                {
                    throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Create test dealer user
            var dealerEmail = "dealer@ucic.com";
            var dealerUserName = "testdealer";
            var dealerPassword = "Dealer@123";

            var dealerUser = await _userManager.FindByEmailAsync(dealerEmail);
            if (dealerUser == null)
            {
                var newDealerUser = new ApplicationUser
                {
                    UserName = dealerUserName,
                    Email = dealerEmail,
                    EmailConfirmed = true,
                    FullName = "Test Dealer User",
                    PhoneNumber = "+91-9876543210"
                };

                var result = await _userManager.CreateAsync(newDealerUser, dealerPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newDealerUser, "Dealer");

                    // Create associated dealer record
                    var existingDealer = _context.Dealers.FirstOrDefault(d => d.UserId == newDealerUser.Id);
                    if (existingDealer == null)
                    {
                        var dealer = new Dealer
                        {
                            UserId = newDealerUser.Id,
                            DealerName = "ABC Construction Company",
                            DealerCode = "DLR001",
                            CreditLimit = 500000.00m,
                            CurrentBalance = 0.00m,
                            Ln_ID = "LN001",
                            IsVerified = true,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = "System"
                        };

                        _context.Dealers.Add(dealer);
                        await _context.SaveChangesAsync();

                        // Note: DealerAddress and DealerNotification seeding removed
                        // Use DealerShippingAddress for address management instead
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    throw new Exception($"Failed to create dealer user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Seed sample FAQs
            await SeedSampleFAQs();
        }

        private async Task SeedSampleFAQs()
        {
            // Note: FAQ seeding removed as FAQ entities were deleted from the system
            // FAQ functionality is now handled through simplified support responses
            await Task.CompletedTask;
        }

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                await _roleManager.CreateAsync(role);
            }
        }
    }
}
