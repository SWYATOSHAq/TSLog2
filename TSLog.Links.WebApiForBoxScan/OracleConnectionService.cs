using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using TSLog.Links.WebApiForBoxScan.Models;

namespace TSLog.Links.WebApiForBoxScan;

/// <summary>
/// Сервис для непосредственной работы с БД Oracle.
/// </summary>
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

    /// <summary>
    /// Проверяет наличие расхода (заказа).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <returns>Признак наличия расхода (заказа).</returns>
    public async Task<bool> CheckOrderAsync(string outgoId)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер заказа (расхода) не может быть пустым или null.",
                nameof(outgoId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"select count(1) " +
            $"from {SsccSchema}.aufid_for_box_scan_app#v " +
            $"where aufid = :outgo_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId.Trim();

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    /// <summary>
    /// Возвращает количество ТЕ для заданного идентификатора расхода (заказа).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <returns>Количество ТЕ.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<int> GetOutgoTusCountAsync(string outgoId)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер заказа (расхода) не может быть пустым или null.",
                nameof(outgoId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        // TODO: мне не нравится доступ напрямую к таблице,
        //       надо сделать представление.
        command.CommandText = $"select count(*) " +
            $"from {WmsSchema}.te_t tu " +
            $"where tu.aufid = :outgo_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId.Trim();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Возвращает количество коробов для заданного идентификатора расхода (заказа).
    /// Речь про собранные короба, которые ещё могли быть не просканированы для кросс-докинга.
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <returns>Количество коробов.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<int> GetOutgoBoxesCountAsync(string outgoId)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер заказа (расхода) outgoId не может быть пустым или null.",
                nameof(outgoId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        
        command.CommandText = $"select sum(tul.boxes_count) " +
            $"from {SsccSchema}.outgo_sscc_tu_line#v tul " +
            $"where tul.outgo_id = :outgo_id";

        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId.Trim();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Возвращает количество коробов для заданного идентификатора транспортной единицы собранного расхода (заказа).
    /// Речь про собранные короба, которые ещё могли быть не просканированы для кросс-докинга.
    /// </summary>
    /// <param name="tuId">Идентификатор транспортной единицы.</param>
    /// <returns>Количество коробов.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<int> GetTuBoxesCountAsync(string tuId)
    {
        if (string.IsNullOrWhiteSpace(tuId))
            throw new ArgumentException("Номер ТЕ tuId не может быть пустым или null.",
                nameof(tuId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;

        command.CommandText = $"select sum(tul.boxes_count) " +
            $"from {SsccSchema}.outgo_sscc_tu_line#v tul " +
            $"where tul.tu_id = :tu_id";

        command.Parameters.Add("tu_id", OracleDbType.Varchar2).Value = tuId.Trim();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Возвращает количество отсканированных ТЕ для заданного идентификатора расхода (заказа).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <returns>Количество отсканированных ТЕ.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<int> GetOutgoScannedTusCountAsync(string outgoId)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер заказа (расхода) outgoId не может быть пустым или null.",
                nameof(outgoId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"select count(distinct sscc.tu_id) tus_count " +
            $"from {SsccSchema}.outgo_sscc#v sscc " +
            $"where sscc.outgo_id = :outgo_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId.Trim();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Возвращает количество отсканированных коробов для заданного идентификатора расхода (заказа).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <returns>Количество отсканированных коробов.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<int> GetOutgoScannedBoxesCountAsync(string outgoId)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер заказа (расхода) не может быть пустым или null.",
                nameof(outgoId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"select count(*) tus_count " +
            $"from {SsccSchema}.outgo_sscc#v sscc " +
            $"where sscc.outgo_id = :outgo_id";
        command.Parameters.Add("outgo_id", OracleDbType.Varchar2).Value = outgoId.Trim();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Возвращает количество отсканированных коробов для заданного идентификатора транспортной единицы.
    /// </summary>
    /// <param name="tuId">Идентификатор транспортной единицы.</param>
    /// <returns>Количество отсканированных коробов.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<int> GetTuScannedBoxesCountAsync(string tuId)
    {
        if (string.IsNullOrWhiteSpace(tuId))
            throw new ArgumentException("Идентификатор транспортной единицы не может быть пустым или null.",
                nameof(tuId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"select count(*) tus_count " +
            $"from {SsccSchema}.outgo_sscc#v sscc " +
            $"where sscc.tu_id = :tu_id";
        command.Parameters.Add("tu_id", OracleDbType.Varchar2).Value = tuId.Trim();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Сохраняет в БД информацию о коробе после сканирования.
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <param name="sscc">SSCC код короба.</param>
    /// <param name="tuId">Идентификатор транспортной единицы.</param>
    /// <param name="user">Имя пользователя.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task AddSsccAsync(string outgoId, string tuId, string sscc, string user)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Номер расхода (заказа) не может быть пустым или null.", nameof(outgoId));

        if (string.IsNullOrWhiteSpace(tuId))
            throw new ArgumentException("Идентификатор ТЕ не может быть пустым или null.", nameof(tuId));

        if (string.IsNullOrWhiteSpace(user))
            throw new ArgumentException("Пользователь не может быть пустым или null.", nameof(user));

        if (string.IsNullOrWhiteSpace(sscc) || sscc.Trim().Length != 20)
            throw new ArgumentException("Штрих-код должен быть длиной 20 символов.", nameof(sscc));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = $"{SsccSchema}.add_sscc";

        command.Parameters.Add("p_outgo_id", OracleDbType.Varchar2).Value = outgoId.Trim();
        command.Parameters.Add("p_tu_id", OracleDbType.Varchar2).Value = tuId.Trim();
        command.Parameters.Add("p_sscc", OracleDbType.Varchar2).Value = sscc.Trim();
        command.Parameters.Add("p_user", OracleDbType.Varchar2).Value = user.Trim();
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Удяляет из БД информацию об отсканированных коробах для конкретного расхода (заказа).
    /// </summary>
    /// <param name="outgoId">Идентификатор расхода (заказа).</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task RemoveOutgoSsccsAsync(string outgoId)
    {
        if (string.IsNullOrWhiteSpace(outgoId))
            throw new ArgumentException("Идентификатор расхода (заказа) не может быть пустым или null.",
                nameof(outgoId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = $"{SsccSchema}.remove_outgo_ssccs";
        command.Parameters.Add("p_outgo_id", OracleDbType.Varchar2).Value = outgoId.Trim();
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Удяляет из БД информацию об отсканированных коробах для конкретной ТЕ.
    /// </summary>
    /// <param name="tuId">Идентификатор транспортной единицы.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task RemoveTuSsccsAsync(string tuId)
    {
        if (string.IsNullOrWhiteSpace(tuId))
            throw new ArgumentException("Идентификатор ТЕ не может быть пустым или null.", nameof(tuId));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = $"{SsccSchema}.remove_tu_ssccs";

        command.Parameters.Add("p_tu_id", OracleDbType.Varchar2).Value = tuId.Trim();
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Удяляет из БД информацию об клнкретном отсканированном коробе.
    /// </summary>
    /// <param name="sscc">SSCC короба.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task RemoveSsccAsync(string sscc)
    {
        if (string.IsNullOrWhiteSpace(sscc))
            throw new ArgumentException("SSCC не может быть пустым или null.", nameof(sscc));

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = $"{SsccSchema}.remove_sscc";

        command.Parameters.Add("p_sscc", OracleDbType.Varchar2).Value = sscc.Trim();
        await command.ExecuteNonQueryAsync();
    }
}
