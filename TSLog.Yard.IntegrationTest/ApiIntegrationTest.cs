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
            var connectionString = "Host=10.50.254.14;Database=tslog;Pooling=True;Maximum Pool Size=200;Connection Lifetime=200;Connection Pruning Interval=200;Username=yarddispatcher;Password=sdfdsSFS38&%^(#__Sdhgsdgy_sDDG22;Persist Security Info=True;Application Name='TEST'";

            // Act
            // Call your API endpoint in a loop
            _output.WriteLine("Check for idle connections before making all API calls");
            await CheckForIdleConnections(connectionString);
            for (int i = 0; i < 50; i++)
            {
                // Create a new database connection for each API call
                //using (var connection = new NpgsqlConnection(connectionString))
                //{
                var connection = new NpgsqlConnection(connectionString);
                    await connection.OpenAsync();

                    // Perform API call using the new connection
                    //var response = await _client.GetAsync("https://localhost:7295/api/v1/yard/ActiveOperations");
                    var response = await _client.GetAsync("https://yd.tslog.ru/api/v1/yard/ActiveOperations");
                    //var content = await response.Content.ReadAsStringAsync();
                    //_output.WriteLine(content); // Log the content of the response

                    // Assert
                    response.EnsureSuccessStatusCode();
                //}
            }

            // Check for idle connections after making all API calls
            _output.WriteLine("Check for idle connections after making all API calls");
            await CheckForIdleConnections(connectionString);
        }

        private async Task CheckForIdleConnections(string connectionString)
        {
            _output.WriteLine("CheckForIdleConnections method execute!");

            //using (var connection = new NpgsqlConnection(connectionString))
            //{
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

                // Filter connections by application name
                using (var cmd = new NpgsqlCommand("SELECT * FROM pg_stat_activity WHERE application_name = 'TEST'", connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Check the type of the first column and convert it appropriately
                            var oidString = reader.GetValue(0).ToString();
                            _output.WriteLine(oidString);
                        }
                    }
                }
            //}
        }
    }
}