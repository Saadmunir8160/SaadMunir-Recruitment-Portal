using Infra.Identity.Seeding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseMaintenanceController : ControllerBase
    {
        private readonly DatabaseCleanupSeeder _cleanupSeeder;
        private readonly DataSeeder _dataSeeder;
        private readonly ILogger<DatabaseMaintenanceController> _logger;

        public DatabaseMaintenanceController(
            DatabaseCleanupSeeder cleanupSeeder,
            DataSeeder dataSeeder,
            ILogger<DatabaseMaintenanceController> logger)
        {
            _cleanupSeeder = cleanupSeeder;
            _dataSeeder = dataSeeder;
            _logger = logger;
        }

        /// <summary>
        /// Cleans up custom role IDs and re-creates roles with proper GUIDs
        /// WARNING: This will remove all custom roles and recreate them!
        /// </summary>
        [HttpPost("cleanup-roles")]
        [Authorize(Roles = "Admin")] // Only admins can run this
        public async Task<ActionResult> CleanupRoles()
        {
            try
            {
                await _cleanupSeeder.CleanupAndReseedAsync();
                await _cleanupSeeder.EnsureAdminUserAsync();
                
                return Ok(new { 
                    success = true, 
                    message = "Database cleanup completed. Roles now have proper GUID IDs." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database cleanup");
                return BadRequest(new { 
                    success = false, 
                    message = $"Cleanup failed: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Alternative endpoint that doesn't require authentication for initial setup
        /// </summary>
        [HttpPost("initial-setup")]
        [AllowAnonymous] // For initial database setup only
        public async Task<ActionResult> InitialSetup()
        {
            try
            {
                await _cleanupSeeder.CleanupAndReseedAsync();
                await _cleanupSeeder.EnsureAdminUserAsync();
                
                return Ok(new { 
                    success = true, 
                    message = "Initial database setup completed. All roles now have proper GUID IDs." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial setup");
                return BadRequest(new { 
                    success = false, 
                    message = $"Initial setup failed: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Check current role status
        /// </summary>
        [HttpGet("role-status")]
        [AllowAnonymous]
        public async Task<ActionResult> GetRoleStatus()
        {
            try
            {
                var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = roleManager.Roles.ToList().Select(r => new { 
                    Id = r.Id, 
                    Name = r.Name,
                    IsGuid = Guid.TryParse(r.Id, out _)
                }).ToList();

                return Ok(new { 
                    success = true, 
                    roles = roles,
                    message = $"Found {roles.Count} roles"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role status");
                return BadRequest(new { 
                    success = false, 
                    message = $"Failed to get role status: {ex.Message}" 
                });
            }
        }
    }
}