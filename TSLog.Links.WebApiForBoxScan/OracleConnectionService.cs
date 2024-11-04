using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using TSLog.Links.WebApiForBoxScan.Models;

namespace TSLog.Links.WebApiForBoxScan;

public class OracleConnectionService
{
    private const string DefaultCommonSchema = "common";
    private const string DefaultWmsSchema = "wms";
    private const string DefaultSsccSchema = "sscc";
    private const string DefaultStaffSchema = "staff";
    private const string DefaultPartnersSchema = "partners";

    private readonly OracleConnectionSettings _settings;

    private readonly string CommonSchema = DefaultCommonSchema;
    private readonly string WmsSchema = DefaultWmsSchema;
    private readonly string SsccSchema = DefaultSsccSchema;
    private readonly string StaffSchema = DefaultStaffSchema;
    private readonly string PartnersSchema = DefaultPartnersSchema;

    public OracleConnectionService(IOptions<OracleConnectionSettings> settings)
    {
        if (settings.Value == null)
            throw new ArgumentNullException(nameof(settings));
        _settings = settings.Value;

        if (!string.IsNullOrWhiteSpace(_settings.CommonSchema))
            CommonSchema = _settings.CommonSchema;
        if (!string.IsNullOrWhiteSpace(_settings.WmsSchema))
            WmsSchema = _settings.WmsSchema;
        if (!string.IsNullOrWhiteSpace(_settings.SsccSchema))
            SsccSchema = _settings.SsccSchema;
        if (!string.IsNullOrWhiteSpace(_settings.StaffSchema))
            StaffSchema = _settings.StaffSchema;
        if (!string.IsNullOrWhiteSpace(_settings.PartnersSchema))
            PartnersSchema = _settings.PartnersSchema;
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

    public async Task<bool> ValidateOrderAsync(string orderId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = "SELECT COUNT(1) FROM ECCO.AUFID_FOR_BOX_SCAN_APP#V WHERE AUFID = :AUFID";
        command.Parameters.Add("AUFID", OracleDbType.Varchar2).Value = orderId;

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<int> GetOutgoTusCount(string outgoId)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер заказа (расхода) outgoId не может быть пустым или null.",
                nameof(outgoId));
        outgoId = outgoId.Trim();

        using var connection = CreateConnection();
        await connection.OpenAsync();


        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        // TODO: мне не нравится доступ напрямую к таблице,
        //       надо сделать представление.
        command.CommandText = $"select count(*) from {WmsSchema}.te_t tu where tu.aufid = :order_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId;
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> GetOutgoScannedTusCount(string outgoId)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер заказа (расхода) outgoId не может быть пустым или null.",
                nameof(outgoId));
        outgoId = outgoId.Trim();

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText =
            $"select count(distinct sscc.tu_id) tus_count from {SsccSchema}.outgo_sscc#v sscc where sscc.outgo_id = :outgo_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId;
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> GetOutgoScannedBoxesCount(string outgoId)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер заказа (расхода) outgoId не может быть пустым или null.",
                nameof(outgoId));
        outgoId = outgoId.Trim();

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText =
            $"select count(*) tus_count from {SsccSchema}.outgo_sscc#v sscc where sscc.outgo_id = :outgo_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId;
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task AcceptBoxSSCCasync(string orderId, string ssccBoxCode, string teNum, string user)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Номер заказа orderId не может быть пустым или null.", nameof(orderId));

        if (string.IsNullOrWhiteSpace(teNum))
            throw new ArgumentException("Номер ТЕ teNum не может быть пустым или null.", nameof(teNum));

        if (string.IsNullOrWhiteSpace(user))
            throw new ArgumentException("Пользователь user не может быть пустым или null.", nameof(user));

        ssccBoxCode = ssccBoxCode.Trim();

        if (string.IsNullOrWhiteSpace(ssccBoxCode) || ssccBoxCode.Length != 20)
            throw new ArgumentException("Штрих-код ssccBoxCode должен быть длиной 20 символов.", nameof(ssccBoxCode));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "ECCO.INSERT_SSCC_FOR_BOX_SCAN_APP";

        command.Parameters.Add("p_aufid", OracleDbType.Varchar2).Value = orderId;
        command.Parameters.Add("p_sscc_box_code", OracleDbType.Varchar2).Value = ssccBoxCode;
        command.Parameters.Add("p_te_num", OracleDbType.Varchar2).Value = teNum;
        command.Parameters.Add("p_user", OracleDbType.Varchar2).Value = user;

        try
        {
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("Insert operation completed successfully.");
        }
        catch (OracleException exception)
        {
            if (exception.Number == 1) // Check for unique constraint violation (ORA-00001).
            {
                throw new InvalidOperationException("SSCC_BOX_CODE already exists.");
            }
            Console.WriteLine($"Error: {exception.Message}");
            throw;
        }
    }


    public async Task DeleteBoxSSCCforTEAsync(string teNum, string orderId)
    {
        if (string.IsNullOrWhiteSpace(teNum))
            throw new ArgumentException("Номер ТЕ teNum не может быть пустым или null.", nameof(teNum));

        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Номер заказа orderId не может быть пустым или null.", nameof(orderId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "ECCO.DELETE_SSCC_FOR_BOX_SCAN_APP";

        command.Parameters.Add("p_te_num", OracleDbType.Varchar2).Value = teNum;
        command.Parameters.Add("p_aufid", OracleDbType.Varchar2).Value = orderId;

        try
        {
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("Delete operation completed successfully.");
        }
        catch (OracleException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
