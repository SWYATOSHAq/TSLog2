using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using TSLog.Links.WebApiForBoxScan.Models;

namespace TSLog.Links.WebApiForBoxScan;

public class OracleConnectionService
{
    private readonly OracleConnectionSettings _settings;
    private readonly Dictionary<string, HashSet<string>> _orderBoxes;

    public OracleConnectionService(IOptions<OracleConnectionSettings> settings)
    {
        _settings = settings.Value;
        _orderBoxes = new Dictionary<string, HashSet<string>>();
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

    public async Task<bool> ValidateOrderAsync(string orderId)
    {
        using (var connection = CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT COUNT(1) FROM ECCO.AUFID_FOR_BOX_SCAN_APP#V WHERE AUFID = :AUFID";
                command.Parameters.Add("AUFID", OracleDbType.Varchar2).Value = orderId;

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
        }
    }

    public void AcceptOrderBoxes(string orderId, List<string> boxSSCCCodes)
    {
        if (_orderBoxes.ContainsKey(orderId))
        {
            foreach (var code in boxSSCCCodes)
            {
                _orderBoxes[orderId].Add(code);
            }
        }
        else
        {
            _orderBoxes[orderId] = new HashSet<string>(boxSSCCCodes);
        }
    }

    public async Task RegisterOrderAsync(string orderId, string userName)
    {
        if (!_orderBoxes.ContainsKey(orderId))
        {
            throw new Exception("Order not found");
        }

        var boxSSCCCodes = _orderBoxes[orderId];
        var ssccCodesArray = boxSSCCCodes.ToArray();

        using (var connection = CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SSCC_SCAN_APP.Insert_SSCC_For_Box_Scan_App";
                command.BindByName = true;

                command.Parameters.Add("p_AUFID", OracleDbType.Varchar2).Value = orderId;
                command.Parameters.Add("p_TE_NUM", OracleDbType.Varchar2).Value = "1001"; // Example TE_NUM as VARCHAR2
                command.Parameters.Add("p_USER", OracleDbType.Varchar2).Value = userName;

                var ssccCodesParam = new OracleParameter("p_SSCC_CODES", OracleDbType.Varchar2)
                {
                    CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                    Size = ssccCodesArray.Length,
                    Value = ssccCodesArray
                };
                command.Parameters.Add(ssccCodesParam);

                await command.ExecuteNonQueryAsync();
            }
        }
    }


    //public async Task RegisterOrderAsync(string orderId, string userName)
    //{
    //    if (!_orderBoxes.ContainsKey(orderId))
    //    {
    //        throw new Exception("Order not found");
    //    }

    //    var boxSSCCCodes = _orderBoxes[orderId];

    //    using (var connection = CreateConnection())
    //    {
    //        await connection.OpenAsync();

    //        using (var command = connection.CreateCommand())
    //        {
    //            command.CommandType = CommandType.StoredProcedure;
    //            command.CommandText = "ECCO.INSERT_SSCC_FOR_BOX_SCAN_APP";

    //            command.Parameters.Add("P_AUFID", OracleDbType.Varchar2).Value = orderId;
    //            var ssccCodesArray = new OracleParameter("P_SSCC_CODES", OracleDbType.Array)
    //            {
    //                UdtTypeName = "ECCO.SSCC_CODE_ARRAY#T",
    //                Value = boxSSCCCodes.ToArray()
    //            };
    //            command.Parameters.Add(ssccCodesArray);
    //            command.Parameters.Add("P_TE_NUM", OracleDbType.Varchar2).Value = "1001"; // Example TE_NUM as VARCHAR2
    //            command.Parameters.Add("P_USER", OracleDbType.Varchar2).Value = userName;

    //            await command.ExecuteNonQueryAsync();
    //        }
    //    }

    //    // Clear the stored box codes for the order
    //    _orderBoxes.Remove(orderId);
    //}

    //public async Task<List<string>> GetAllDataFromViewAsync()
    //{
    //    var results = new List<string>();

    //    using (var connection = CreateConnection())
    //    {
    //        await connection.OpenAsync();

    //        using (var command = connection.CreateCommand())
    //        {
    //            command.CommandType = CommandType.Text;
    //            command.CommandText = "SELECT AUFID FROM AUFID_MNDR_0024#V";

    //            using (var reader = await command.ExecuteReaderAsync())
    //            {
    //                while (await reader.ReadAsync())
    //                {
    //                    results.Add(reader.GetString(0));
    //                }
    //            }
    //        }
    //    }

    //    return results;
    //}
}
