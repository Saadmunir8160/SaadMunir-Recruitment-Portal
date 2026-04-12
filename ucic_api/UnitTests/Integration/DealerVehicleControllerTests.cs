using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Application.DTOs;

namespace UnitTests.Integration;

[Collection("TestEnvironmentSetup")]
public class DealerVehicleControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DealerVehicleControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateDealerVehicle_ReturnsOk()
    {
        var vehicle = new CreateDealerVehicleDTO
        {
            DealerID = 1,
            PlateNumber = "ABC-123",
            Type = "Truck",
            Capacity = 5.5m,
            RegistrationDate = DateTime.UtcNow.AddDays(-30),
            RegistrationExpiryDate = DateTime.UtcNow.AddYears(1),
            InsuranceExpiryDate = DateTime.UtcNow.AddMonths(6),
            Ln_ID = "LN123",
            RegistrationNumber = "REG123",
            IsActive = true
        };
        var response = await _client.PostAsJsonAsync("/api/dealer/vehicles", vehicle);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected OK but got {response.StatusCode}. Response: {content}");
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllDealerVehicles_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/dealer/vehicles");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected OK but got {response.StatusCode}. Response: {content}");
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDealerVehicleById_ReturnsOkOrNotFound()
    {
        var response = await _client.GetAsync("/api/dealer/vehicles/1");
        if (!(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound))
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected OK or NotFound but got {response.StatusCode}. Response: {content}");
        }
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDealerVehicle_ReturnsOk()
    {
        var update = new UpdateDealerVehicleDTO
        {
            VehicleID = 1,
            DealerID = 1,
            PlateNumber = "XYZ-999",
            Type = "Van",
            Capacity = 3.0m,
            RegistrationDate = DateTime.UtcNow.AddDays(-60),
            RegistrationExpiryDate = DateTime.UtcNow.AddYears(2),
            InsuranceExpiryDate = DateTime.UtcNow.AddMonths(12),
            Ln_ID = "LN999",
            RegistrationNumber = "REG999",
            IsActive = true
        };
        var response = await _client.PutAsJsonAsync("/api/dealer/vehicles/1", update);
        if (!(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound))
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected OK or NotFound but got {response.StatusCode}. Response: {content}");
        }
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDealerVehicle_ReturnsOk()
    {
        var response = await _client.DeleteAsync("/api/dealer/vehicles/1");
        if (!(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound))
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected OK or NotFound but got {response.StatusCode}. Response: {content}");
        }
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }
}
