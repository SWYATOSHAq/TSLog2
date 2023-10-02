using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Npgsql;
using Xunit.Abstractions;
using TSLog.Yard.WebUI;

namespace TSLog.Yard.IntegrationTest
{
    public class ApiIntegrationTest : IClassFixture<WebApplicationFactory<TSLog.Yard.WebUI.Program>>
    {
        private readonly WebApplicationFactory<TSLog.Yard.WebUI.Program> _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public ApiIntegrationTest(WebApplicationFactory<TSLog.Yard.WebUI.Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task TestYourApiEndpoint()
        {
            var connectionString = "Host=10.50.254.14;Database=tslog;Pooling=True;Maximum Pool Size=200;Connection Lifetime=120;Connection Pruning Interval=10;Username=yarddispatcher;Password=sdfdsSFS38&%^(#__Sdhgsdgy_sDDG22;Persist Security Info=True";

            // Act
            // Check for idle connections before making API call
            await CheckForIdleConnections(connectionString);
            _output.WriteLine("Test Output");

            // Call your API endpoint
            var response = await _client.GetAsync("https://localhost:7295/api/v1/yard/ActiveOperations");
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine(content); // Log the content of the response

            // Assert
            response.EnsureSuccessStatusCode();

            // Optionally, check for idle connections again after making API call
            await CheckForIdleConnections(connectionString);
        }

        private async Task CheckForIdleConnections(string connectionString)
        {
            _output.WriteLine("Test Output");
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand("SELECT * FROM pg_stat_activity WHERE state = 'idle'", connection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // Check the type of the first column and convert it appropriately
                var oidString = reader.GetValue(0).ToString();
                _output.WriteLine(oidString);
            }
        }
    }
}