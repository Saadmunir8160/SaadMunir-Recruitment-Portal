using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infra.Identity.Seeding
{
    /// <summary>
    /// Database cleanup and proper re-seeding with GUID role IDs
    /// </summary>
    public class DatabaseCleanupSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DatabaseCleanupSeeder> _logger;

        public DatabaseCleanupSeeder(
            RoleManager<IdentityRole> roleManager, 
            UserManager<ApplicationUser> userManager,
            ILogger<DatabaseCleanupSeeder> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task CleanupAndReseedAsync()
        {
            _logger.LogInformation("Starting database cleanup and re-seeding...");

            // Step 1: Remove roles with custom IDs
            await RemoveCustomRolesAsync();

            // Step 2: Create roles with proper GUIDs
            await CreateRolesWithGuidAsync();

            _logger.LogInformation("Database cleanup and re-seeding completed.");
        }

        private async Task RemoveCustomRolesAsync()
        {
            var customRoleIds = new[]
            {
                "customer-role-id",
                "dealer-role-id", 
                "dealerdriver-role-id"
            };

            foreach (var customId in customRoleIds)
            {
                var role = await _roleManager.FindByIdAsync(customId);
                if (role != null)
                {
                    _logger.LogInformation($"Removing role with custom ID: {customId} (Name: {role.Name})");
                    
                    // Remove all users from this role first
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                    foreach (var user in usersInRole)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                        _logger.LogInformation($"Removed user {user.UserName} from role {role.Name}");
                    }
                    
                    // Delete the role
                    await _roleManager.DeleteAsync(role);
                    _logger.LogInformation($"Deleted role: {role.Name}");
                }
            }
        }

        private async Task CreateRolesWithGuidAsync()
        {
            var roles = new[] { "Admin", "User", "Customer", "Dealer", "DealerDriver" };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName)
                    {
                        // Let ASP.NET Identity generate proper GUID
                        // Don't set Id manually - it will be auto-generated
                    };
                    
                    var result = await _roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Created role '{roleName}' with GUID ID: {role.Id}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    var existingRole = await _roleManager.FindByNameAsync(roleName);
                    _logger.LogInformation($"Role '{roleName}' already exists with ID: {existingRole?.Id}");
                }
            }
        }

        /// <summary>
        /// Re-create admin user if needed
        /// </summary>
        public async Task EnsureAdminUserAsync()
        {
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
                    _logger.LogInformation($"Created admin user with ID: {newAdmin.Id}");
                }
                else
                {
                    _logger.LogError($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Ensure admin has proper role
                if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Added Admin role to existing admin user");
                }
            }
        }
    }
}