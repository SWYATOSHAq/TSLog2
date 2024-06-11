using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using TSLog.Links.WebApiForBoxScan.Models;

namespace TSLog.Links.WebApiForBoxScan;

public class OracleConnectionService
{
    private readonly OracleConnectionSettings _settings;

    public OracleConnectionService(IOptions<OracleConnectionSettings> settings)
    {
        _settings = settings.Value;
    }

    private OracleConnection CreateConnection()
    {
        var connectionString = new OracleConnectionStringBuilder
        {
            DataSource = _settings.DataSource,
            UserID = _settings.UserName,
            Password = _settings.Password
        }.ConnectionString;

        return new OracleConnection(connectionString);
    }

    public async Task SubmitOrderAsync(ScanData scanData)
    {
        using (var connection = CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "YOUR_STORED_PROCEDURE_NAME"; // Replace with your stored procedure name

                command.Parameters.Add("P_ORDER_ID", OracleDbType.Varchar2).Value = scanData.OrderId;
                command.Parameters.Add("P_BOX_ID", OracleDbType.Varchar2).Value = scanData.BoxId;

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<List<string>> GetAllDataFromViewAsync()
    {
        var results = new List<string>();

        using (var connection = CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT AUFID FROM AUFID_MNDR_0024#V";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(reader.GetString(0));
                    }
                }
            }
        }

        return results;
    }
}
