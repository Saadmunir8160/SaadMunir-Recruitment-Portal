using Microsoft.AspNetCore.Identity;
using System;

namespace UnitTests.Helpers
{
    /// <summary>
    /// Helper class for generating test data with proper GUIDs instead of hardcoded values
    /// </summary>
    public static class TestDataHelper
    {
        /// <summary>
        /// Generates a test user ID using proper GUID format
        /// This mimics how ASP.NET Identity generates user IDs in production
        /// </summary>
        public static string GenerateTestUserId()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Generates a unique username for testing to avoid conflicts
        /// </summary>
        public static string GenerateTestUserName(string prefix = "testuser")
        {
            return $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..6]}";
        }

        /// <summary>
        /// Generates a unique email for testing to avoid conflicts
        /// </summary>
        public static string GenerateTestEmail(string prefix = "test")
        {
            return $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}@example.com";
        }

        /// <summary>
        /// Creates test credentials for dealer user creation
        /// </summary>
        public static (string FullName, string UserName, string Email, string Phone, string Password, string ConfirmPassword) 
            GenerateDealerTestCredentials(string namePrefix = "TestDealer")
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString()[..6];
            
            return (
                FullName: $"{namePrefix} User {uniqueId}",
                UserName: $"{namePrefix.ToLower()}{timestamp}{uniqueId}",
                Email: $"{namePrefix.ToLower()}{timestamp}@example.com",
                Phone: "+1234567890",
                Password: "TestPass123!",
                ConfirmPassword: "TestPass123!"
            );
        }

        /// <summary>
        /// Creates test credentials for dealer driver user creation
        /// </summary>
        public static (string FullName, string UserName, string Email, string Phone, string Password, string ConfirmPassword) 
            GenerateDriverTestCredentials(string namePrefix = "TestDriver")
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString()[..6];
            
            return (
                FullName: $"{namePrefix} User {uniqueId}",
                UserName: $"{namePrefix.ToLower()}{timestamp}{uniqueId}",
                Email: $"{namePrefix.ToLower()}{timestamp}@example.com",
                Phone: "+1234567891",
                Password: "TestPass123!",
                ConfirmPassword: "TestPass123!"
            );
        }

        /// <summary>
        /// Creates a simple test dealer command for quick testing
        /// </summary>
        public static Application.Commands.User.Create.CreateDealerUserCommand CreateTestDealerCommand(string namePrefix = "QuickTestDealer")
        {
            var credentials = GenerateDealerTestCredentials(namePrefix);
            
            return new Application.Commands.User.Create.CreateDealerUserCommand
            {
                FullName = credentials.FullName,
                UserName = credentials.UserName,
                Email = credentials.Email,
                Phone = credentials.Phone,
                Password = credentials.Password,
                ConfirmationPassword = credentials.ConfirmPassword,
                DealerName = $"{namePrefix} Company",
                CreditLimit = 25000,
                CurrentBalance = 0,
                Ln_ID = $"LN_{namePrefix.ToUpper()}"
            };
        }

        /// <summary>
        /// Creates a simple test dealer driver command for quick testing
        /// </summary>
        public static Application.Commands.User.Create.CreateDealerDriverUserCommand CreateTestDriverCommand(string namePrefix = "QuickTestDriver")
        {
            var credentials = GenerateDriverTestCredentials(namePrefix);
            
            return new Application.Commands.User.Create.CreateDealerDriverUserCommand
            {
                FullName = credentials.FullName,
                UserName = credentials.UserName,
                Email = credentials.Email,
                Phone = credentials.Phone,
                Password = credentials.Password,
                ConfirmationPassword = credentials.ConfirmPassword,
                Ln_ID = $"LN_{namePrefix.ToUpper()}",
                IqamaNumber = "1234567890"
            };
        }
    }
}