using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Application.DTOs;
using UnitTests.Helpers;

namespace UnitTests.Integration;

[Collection("TestEnvironmentSetup")]
public class DealerControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DealerControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // NOTE: Dealer creation is now handled by DealerUserController
    // This test suite focuses on dealer entity management

    [Fact]
    public async Task GetAllDealers_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Dealer/GetAll");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDealerById_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Dealer/GetById/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDealer_WithoutAuth_ReturnsUnauthorized()
    {
        // Generate proper test GUID instead of hardcoded value
        var testUserId = TestDataHelper.GenerateTestUserId();
        
        var update = new UpdateDealerDTO
        {
            DealerId = 1,
            UserId = testUserId, // Using proper GUID format
            DealerName = "Updated Dealer",
            CreditLimit = 2000,
            CurrentBalance = 100,
            Ln_ID = "LN999",
            IsActive = true
        };
        var response = await _client.PutAsJsonAsync("/api/Dealer/Update/1", update);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDealer_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.DeleteAsync("/api/Dealer/Delete/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
