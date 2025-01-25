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

    public async Task<bool> ValidateOrderAsync(string outgoId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"select count(1) from {SsccSchema}.aufid#v where aufid = :outgo_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId;

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    /// <summary>
    /// Возвращает количество ТЕ для заданного идентификатора расхода (заказа).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <returns>Количество ТЕ.</returns>
    /// <exception cref="ArgumentException"></exception>
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
        command.CommandText = $"select count(*) " +
            $"from {WmsSchema}.te_t tu " +
            $"where tu.aufid = :outgo_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId;
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Возвращает количество отсканированных ТЕ для заданного идентификатора расхода (заказа).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <returns>Количество отсканированных ТЕ.</returns>
    /// <exception cref="ArgumentException"></exception>
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
        command.CommandText = $"select count(distinct sscc.tu_id) tus_count " +
            $"from {SsccSchema}.outgo_sscc#v sscc " +
            $"where sscc.outgo_id = :outgo_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId;
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Возвращает количество отсканированных коробов для заданного идентификатора расхода (заказа).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <returns>Количество отсканированных коробов.</returns>
    /// <exception cref="ArgumentException"></exception>
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
        command.CommandText = $"select count(*) tus_count " +
            $"from {SsccSchema}.outgo_sscc#v sscc " +
            $"where sscc.outgo_id = :outgo_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId;
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task AcceptBoxSSCCasync(string outgoId, string ssccBoxCode, string tuNumber, string user)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер заказа orderId не может быть пустым или null.", nameof(outgoId));

        if (string.IsNullOrWhiteSpace(tuNumber))
            throw new ArgumentException("Номер ТЕ teNum не может быть пустым или null.", nameof(tuNumber));

        if (string.IsNullOrWhiteSpace(user))
            throw new ArgumentException("Пользователь user не может быть пустым или null.", nameof(user));

        ssccBoxCode = ssccBoxCode.Trim();

        if (string.IsNullOrWhiteSpace(ssccBoxCode) || ssccBoxCode.Length != 20)
            throw new ArgumentException("Штрих-код ssccBoxCode должен быть длиной 20 символов.", nameof(ssccBoxCode));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = $"{SsccSchema}.insert_sscc_for_box_scan_app";

        command.Parameters.Add("p_aufid", OracleDbType.Varchar2).Value = outgoId;
        command.Parameters.Add("p_sscc_box_code", OracleDbType.Varchar2).Value = ssccBoxCode;
        command.Parameters.Add("p_te_num", OracleDbType.Varchar2).Value = tuNumber;
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

    public async Task DeleteBoxSSCCforTEAsync(string tuNumber, string outgoId)
    {
        if (string.IsNullOrWhiteSpace(tuNumber))
            throw new ArgumentException("Номер ТЕ teNum не может быть пустым или null.", nameof(tuNumber));

        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер заказа orderId не может быть пустым или null.", nameof(outgoId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = $"{SsccSchema}.delete_sscc_for_box_scan_app";

        command.Parameters.Add("p_te_num", OracleDbType.Varchar2).Value = tuNumber;
        command.Parameters.Add("p_aufid", OracleDbType.Varchar2).Value = outgoId;

        try
        {
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("Delete operation completed successfully.");
        }
        catch (OracleException exception)
        {
            Console.WriteLine($"Error: {exception.Message}");
        }
    }
}
