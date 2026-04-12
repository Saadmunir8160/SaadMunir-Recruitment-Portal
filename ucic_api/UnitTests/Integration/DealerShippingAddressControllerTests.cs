using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Application.DTOs;

namespace UnitTests.Integration;

[Collection("TestEnvironmentSetup")]
public class DealerShippingAddressControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DealerShippingAddressControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateDealerShippingAddress_ReturnsOk()
    {
        var address = new CreateDealerShippingAddressDTO
        {
            DealerID = 1,
            Ln_ID = "LN123",
            AddressLine1 = "Line 1",
            AddressLine2 = "Line 2",
            City = "City",
            State = "State",
            Country = "Country",
            PostalCode = "12345",
            IsActive = true
        };
        var response = await _client.PostAsJsonAsync("/api/DealerShippingAddress/Create", address);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllDealerShippingAddresses_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/DealerShippingAddress/GetAll");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDealerShippingAddressById_ReturnsOkOrNotFound()
    {
        var response = await _client.GetAsync("/api/DealerShippingAddress/GetById/1");
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDealerShippingAddress_ReturnsOk()
    {
        var update = new UpdateDealerShippingAddressDTO
        {
            AddressID = 1,
            DealerID = 1,
            Ln_ID = "LN999",
            AddressLine1 = "Updated Line 1",
            AddressLine2 = "Updated Line 2",
            City = "Updated City",
            State = "Updated State",
            Country = "Updated Country",
            PostalCode = "54321",
            IsActive = true
        };
        var response = await _client.PutAsJsonAsync("/api/DealerShippingAddress/Update/1", update);
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDealerShippingAddress_ReturnsOk()
    {
        var response = await _client.DeleteAsync("/api/DealerShippingAddress/Delete/1");
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }
}
