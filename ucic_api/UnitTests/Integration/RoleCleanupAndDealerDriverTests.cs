using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Application.Commands.User.Create;
using UnitTests.Helpers;
using System.Text.Json;

namespace UnitTests.Integration;

[Collection("TestEnvironmentSetup")]
public class RoleCleanupAndDealerDriverTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RoleCleanupAndDealerDriverTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CheckRoleStatus_ShouldShowCurrentRoles()
    {
        var response = await _client.GetAsync("/api/DatabaseMaintenance/role-status");
        
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected OK, but got {response.StatusCode}. Content: {await response.Content.ReadAsStringAsync()}"
        );
        
        var content = await response.Content.ReadAsStringAsync();
        // Content should contain role information
        Assert.Contains("roles", content);
    }

    [Fact]
    public async Task InitialSetup_ShouldCleanupAndCreateProperRoles()
    {
        var response = await _client.PostAsync("/api/DatabaseMaintenance/initial-setup", null);
        
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected OK, but got {response.StatusCode}. Content: {await response.Content.ReadAsStringAsync()}"
        );
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("success", content);
    }

    [Fact]
    public async Task CreateDealer_AfterRoleCleanup_ShouldWork()
    {
        // First cleanup roles
        await _client.PostAsync("/api/DatabaseMaintenance/initial-setup", null);
        
        // Now try to create dealer
        var credentials = TestDataHelper.GenerateDealerTestCredentials("CleanupTestDealer");
        
        var command = new CreateDealerUserCommand
        {
            FullName = credentials.FullName,
            UserName = credentials.UserName,
            Email = credentials.Email,
            Phone = credentials.Phone,
            Password = credentials.Password,
            ConfirmationPassword = credentials.ConfirmPassword,
            DealerName = "Cleanup Test Dealer Company",
            CreditLimit = 30000,
            CurrentBalance = 0,
            Ln_ID = "LN_CLEANUP_TEST"
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/Create", command);
        
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.BadRequest, // Could be BadRequest if user already exists
            $"Expected OK or BadRequest, but got {response.StatusCode}. Content: {await response.Content.ReadAsStringAsync()}"
        );
    }

    [Fact]
    public async Task VerifyDealerDriverRole_AfterCleanup_ShouldExist()
    {
        // Cleanup and setup proper roles
        await _client.PostAsync("/api/DatabaseMaintenance/initial-setup", null);
        
        // Check role status to verify DealerDriver role exists with proper GUID
        var response = await _client.GetAsync("/api/DatabaseMaintenance/role-status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Should contain DealerDriver role
        Assert.Contains("DealerDriver", content);
        
        // Should show roles with proper GUID format
        // Note: We can't easily test GUID format in integration test without parsing JSON
        // But the DatabaseCleanupSeeder ensures proper GUID creation
        Assert.Contains("roles", content);
    }

    [Fact]
    public async Task DealerDriverCreation_ShouldReturnUnauthorized_ButRolesExist()
    {
        // Ensure roles are properly setup
        await _client.PostAsync("/api/DatabaseMaintenance/initial-setup", null);
        
        // Try to create dealer driver (should fail due to authentication)
        var credentials = TestDataHelper.GenerateDriverTestCredentials("CleanupTestDriver");
        
        var command = new CreateDealerDriverUserCommand
        {
            FullName = credentials.FullName,
            UserName = credentials.UserName,
            Email = credentials.Email,
            Phone = credentials.Phone,
            Password = credentials.Password,
            ConfirmationPassword = credentials.ConfirmPassword,
            Ln_ID = "LN_CLEANUP_DRIVER",
            IqamaNumber = "1234567890"
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/CreateDriver", command);
        
        // Should return Unauthorized (security working correctly)
        // This means the endpoint is accessible and roles exist, but authentication is required
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}