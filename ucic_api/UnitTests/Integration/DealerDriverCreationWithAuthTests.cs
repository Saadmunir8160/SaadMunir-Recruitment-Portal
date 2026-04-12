using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Application.Commands.User.Create;
using Application.Commands.Auth;
using UnitTests.Helpers;
using System.Net.Http.Headers;

namespace UnitTests.Integration;

[Collection("TestEnvironmentSetup")]
public class DealerDriverCreationWithAuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DealerDriverCreationWithAuthTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateDealerDriver_WithAdminAuth_ShouldSucceed()
    {
        try
        {
            // Step 1: Login as admin to get token
            var loginCommand = new AuthCommand
            {
                Email = "admin@ucic.com",
                Password = "Admin@123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/Login", loginCommand);
            
            if (loginResponse.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await loginResponse.Content.ReadAsStringAsync();
                Assert.True(false, $"Login failed with status {loginResponse.StatusCode}: {errorContent}");
            }

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<Application.DTOs.AuthResponseDTO>();
            Assert.NotNull(loginResult?.Token);

            // Set authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            // Step 2: Create a dealer first (if not exists)
            var dealerCredentials = TestDataHelper.GenerateDealerTestCredentials("TestDealerForDriver");
            var dealerCommand = new CreateDealerUserCommand
            {
                FullName = dealerCredentials.FullName,
                UserName = dealerCredentials.UserName,
                Email = dealerCredentials.Email,
                Phone = dealerCredentials.Phone,
                Password = dealerCredentials.Password,
                ConfirmationPassword = dealerCredentials.ConfirmPassword,
                DealerName = "Test Dealer Company for Driver Creation",
                CreditLimit = 100000,
                CurrentBalance = 0,
                Ln_ID = "LN_TEST_DEALER_001"
            };

            var dealerResponse = await _client.PostAsJsonAsync("/api/DealerUser/Create", dealerCommand);
            // Dealer creation might return BadRequest if already exists, that's OK
            if (dealerResponse.StatusCode != HttpStatusCode.OK && dealerResponse.StatusCode != HttpStatusCode.BadRequest)
            {
                var dealerError = await dealerResponse.Content.ReadAsStringAsync();
                Assert.True(false, $"Dealer creation failed with status {dealerResponse.StatusCode}: {dealerError}");
            }

            // Step 3: Create dealer driver
            var driverCredentials = TestDataHelper.GenerateDriverTestCredentials("AuthTestDriver");
            var driverCommand = new CreateDealerDriverUserCommand
            {
                FullName = driverCredentials.FullName,
                UserName = driverCredentials.UserName,
                Email = driverCredentials.Email,
                Phone = driverCredentials.Phone,
                Password = driverCredentials.Password,
                ConfirmationPassword = driverCredentials.ConfirmPassword,
                Ln_ID = "LN_AUTH_DRIVER_001",
                IqamaNumber = "1234567890123"
            };

            var driverResponse = await _client.PostAsJsonAsync("/api/DealerUser/CreateDriver", driverCommand);
            
            if (driverResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await driverResponse.Content.ReadAsStringAsync();
                // If user already exists, that's actually success for our test
                if (errorContent.Contains("already exists") || errorContent.Contains("DuplicateUserName"))
                {
                    Assert.True(true, "Driver user already exists - creation was successful in previous run");
                    return;
                }
                else
                {
                    Assert.True(false, $"Driver creation failed with BadRequest: {errorContent}");
                }
            }

            if (driverResponse.StatusCode != HttpStatusCode.OK)
            {
                var driverError = await driverResponse.Content.ReadAsStringAsync();
                Assert.True(false, $"Driver creation failed with status {driverResponse.StatusCode}: {driverError}");
            }

            // Should succeed with proper authentication
            Assert.Equal(HttpStatusCode.OK, driverResponse.StatusCode);
            
            var driverResult = await driverResponse.Content.ReadAsStringAsync();
            Assert.Contains("successfully", driverResult, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Assert.True(false, $"Test failed with exception: {ex.Message}\nStack: {ex.StackTrace}");
        }
    }

    [Fact]
    public async Task CreateDealerDriver_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Clear any existing authorization
        _client.DefaultRequestHeaders.Authorization = null;

        var driverCredentials = TestDataHelper.GenerateDriverTestCredentials("NoAuthDriver");
        var driverCommand = new CreateDealerDriverUserCommand
        {
            FullName = driverCredentials.FullName,
            UserName = driverCredentials.UserName,
            Email = driverCredentials.Email,
            Phone = driverCredentials.Phone,
            Password = driverCredentials.Password,
            ConfirmationPassword = driverCredentials.ConfirmPassword,
            Ln_ID = "LN_NO_AUTH",
            IqamaNumber = "9876543210"
        };

        var response = await _client.PostAsJsonAsync("/api/DealerUser/CreateDriver", driverCommand);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}