using System;
using System.IO;
using Xunit;

namespace UnitTests.Integration
{
    // xUnit does not support [AssemblyInitialize], so use a collection fixture
    [CollectionDefinition("TestEnvironmentSetup")] 
    public class TestEnvironmentSetupCollection : ICollectionFixture<TestEnvironmentSetup> { }

    public class TestEnvironmentSetup : IDisposable
    {
        public TestEnvironmentSetup()
        {
            // Ensure all required static folders exist in the test output directory
            string outputRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            string[] folders = { "News", "Payment", "Vendors", "Product", "Resume" };
            foreach (var folder in folders)
            {
                string dir = Path.Combine(outputRoot, folder);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }
        public void Dispose() { }
    }
}
