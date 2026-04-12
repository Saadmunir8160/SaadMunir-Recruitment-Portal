using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Application.DTOs;
using UnitTests.Helpers;

namespace UnitTests.Integration;

[Collection("TestEnvironmentSetup")]
public class DealerDriverControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DealerDriverControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // NOTE: Dealer driver creation is now handled by DealerUserController
    // This test suite focuses on dealer driver entity management

    [Fact]
    public async Task GetAllDealerDrivers_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/DealerDriver/GetAll");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDealerDriverById_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/DealerDriver/GetById/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDealerDriver_WithoutAuth_ReturnsUnauthorized()
    {
        // Generate proper test GUID instead of hardcoded value
        var testUserId = TestDataHelper.GenerateTestUserId();
        
        var update = new UpdateDealerDriverDTO
        {
            DriverID = 1,
            UserId = testUserId, // Changed from UserID to UserId for consistency
            Ln_ID = "LN999",
            IqamaNumber = "IQ999",
            IsActive = true
        };
        var response = await _client.PutAsJsonAsync("/api/DealerDriver/Update/1", update);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDealerDriver_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.DeleteAsync("/api/DealerDriver/Delete/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
