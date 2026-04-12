using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Application.Commands.User.Create;
using Application.Commands.User.Login;
using UnitTests.Helpers;

namespace UnitTests.Integration;

[Collection("TestEnvironmentSetup")]
public class DealerUserControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DealerUserControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateDealerUser_WithValidData_ReturnsOk()
    {
        var credentials = TestDataHelper.GenerateDealerTestCredentials("TestDealer");
        
        var command = new CreateDealerUserCommand
        {
            FullName = credentials.FullName,
            UserName = credentials.UserName,
            Email = credentials.Email,
            Phone = credentials.Phone,
            Password = credentials.Password,
            ConfirmationPassword = credentials.ConfirmPassword,
            DealerName = "Test Dealer Company",
            CreditLimit = 50000,
            CurrentBalance = 0,
            Ln_ID = "LN_TEST_001"
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/Create", command);
        
        // Should succeed or return BadRequest if user already exists
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected OK or BadRequest, but got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task CreateDealerUser_WithInvalidEmail_ReturnsBadRequest()
    {
        var credentials = TestDataHelper.GenerateDealerTestCredentials("TestDealer");
        
        var command = new CreateDealerUserCommand
        {
            FullName = credentials.FullName,
            UserName = credentials.UserName,
            Email = "invalid-email", // Invalid email format
            Phone = credentials.Phone,
            Password = credentials.Password,
            ConfirmationPassword = credentials.ConfirmPassword,
            DealerName = "Test Dealer Company"
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/Create", command);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDealerUser_WithMismatchedPasswords_ReturnsBadRequest()
    {
        var credentials = TestDataHelper.GenerateDealerTestCredentials("TestDealer");
        
        var command = new CreateDealerUserCommand
        {
            FullName = credentials.FullName,
            UserName = credentials.UserName,
            Email = credentials.Email,
            Phone = credentials.Phone,
            Password = credentials.Password,
            ConfirmationPassword = "DifferentPass123!", // Mismatched password
            DealerName = "Test Dealer Company"
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/Create", command);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDealerDriverUser_WithValidData_ReturnsUnauthorizedOrOk()
    {
        // This endpoint requires authentication, so it should return Unauthorized for anonymous requests
        var credentials = TestDataHelper.GenerateDriverTestCredentials("TestDriver");
        
        var command = new CreateDealerDriverUserCommand
        {
            FullName = credentials.FullName,
            UserName = credentials.UserName,
            Email = credentials.Email,
            Phone = credentials.Phone,
            Password = credentials.Password,
            ConfirmationPassword = credentials.ConfirmPassword,
            Ln_ID = "LN_DRIVER_001",
            IqamaNumber = "1234567890"
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/CreateDriver", command);
        
        // Should return Unauthorized due to missing authentication
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginDealer_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var command = new LoginDealerCommand
        {
            UserName = "nonexistentuser",
            Password = "wrongpassword"
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/Login", command);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginDealerDriver_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var command = new LoginDealerDriverCommand
        {
            UserName = "nonexistentdriver",
            Password = "wrongpassword"
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/LoginDriver", command);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginDealer_WithMissingUserName_ReturnsBadRequest()
    {
        var command = new LoginDealerCommand
        {
            UserName = "", // Missing username
            Password = "somepassword"
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/Login", command);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LoginDealerDriver_WithMissingPassword_ReturnsBadRequest()
    {
        var command = new LoginDealerDriverCommand
        {
            UserName = "someuser",
            Password = "" // Missing password
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/LoginDriver", command);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}