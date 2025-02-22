using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Security;
using TSLog.Common.BO;
using TSLog.ExceptionHandler;

namespace TSLog.Common.Data;

/// <summary>
/// Базовый класс для работы с хранилищем БД Oracle.
/// </summary>
public class OracleStorage : IDisposable
{
    protected OracleConnection _connection = null;
    protected string _dataSource = string.Empty;

    private const string DefaultCommonSchema = "common";
    /// <summary>
    /// Схема для "общих" объектов.
    /// </summary>
    public string CommonSchema { protected get; set; } = DefaultCommonSchema;

    public OracleStorage(string dataSource)
    {
        if (string.IsNullOrWhiteSpace(dataSource))
            throw new ArgumentException($"{nameof(dataSource)} is empty");
        _connection = new OracleConnection();
        _dataSource = dataSource;
    }

    /// <summary>
    /// Подключиться к БД.
    /// </summary>
    /// <param name="user">Имя пользователя БД.</param>
    /// <param name="password">Пароль пользователя БД.</param>
    public void Connect(string user, SecureString password)
    {
        var stringBuilder = new OracleConnectionStringBuilder
        {
            DataSource = _dataSource,
            UserID = user,
            Password = Utils.SecureStringToString(password)
        };

        _connection.ConnectionString = stringBuilder.ConnectionString;
        _connection.Open();
        stringBuilder.Clear();
    }
    /// <summary>
    /// Выполнить произвольный SQL код.
    /// </summary>
    /// <param name="sql">SQL код.</param>
    /// <exception cref="ArgumentException"></exception>
    public void Execute(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException($"{nameof(sql)} is empty");
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    public static bool HasColumn(IDataRecord dataRecord, string columnName)
    {
        for (int i = 0; i < dataRecord.FieldCount; i++)
        {
            if (dataRecord.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                return true;
        }
        return false;
    }
    protected static Entity CreateEntity(DbDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var id = Utils.DbValueToLong(reader["id"]);
        var code = Utils.DbValueToString(reader["code"]);
        var name = Utils.DbValueToString(reader["name"]);

        var shortName = string.Empty;
        if (HasColumn(reader, "short_name"))
            shortName = Utils.DbValueToString(reader["short_name"]);

        var altName = string.Empty;
        if (HasColumn(reader, "alt_name"))
            altName = Utils.DbValueToString(reader["alt_name"]);

        var displayName = string.Empty;
        if (HasColumn(reader, "display_name"))
            displayName = Utils.DbValueToString(reader["display_name"]);

        var isActive = true;
        if (HasColumn(reader, "is_active"))
            isActive = Utils.DbValueToShort(reader["is_active"]) == 1;

        var displayOrder = -1;
        if (HasColumn(reader, "display_order"))
            displayOrder = Utils.DbValueToShort(reader["display_order"]);
        var note = Utils.DbValueToString(reader["note"]);
        var entity = new Entity(id,
            code,
            name,
            shortName,
            altName,
            displayName,
            isActive,
            displayOrder,
            note);
        return entity;
    }
    /// <summary>
    /// Возвращает базовую сущность по идентификатору (с указанием SQL запроса и каталога каэша).
    /// Сначала сущность ищется в кэше, в каталоге,
    /// потом в БД по указанному SQL запросу.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="sql">SQL запрос.</param>
    /// <param name="catalog">Каталог (кэш).</param>
    /// <returns>Сущность.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="EntityNotFoundException"></exception>
    public Entity GetEntity(int id, string sql, Catalog<Entity> catalog = null)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException($"{nameof(sql)} is empty");
        id = id > 0 ? id : -1;
        var entity = Entity.Null;
        if (catalog != null)
        {
            entity = catalog[id];
            if (entity != null && entity != Entity.Null)
                return entity;
        }
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.Add(new OracleParameter("id", id));
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                entity = CreateEntity(reader);
                catalog?.Add(entity);
            }
            reader.Close();
        }
        if (entity != null && entity != Entity.Null)
            return entity;
        throw new EntityNotFoundException(id);
    }
    /// <summary>
    /// Возвращает базовую сущность по коду (с указанием SQL запроса и каталога каэша).
    /// Сначала сущность ищется в кэше, в каталоге,
    /// потом в БД по указанному SQL запросу.
    /// </summary>
    /// <param name="code">Код.</param>
    /// <param name="sql">SQL запрос.</param>
    /// <param name="catalog">Каталог (кэш).</param>
    /// <returns>Сущность.</returns>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="EntityNotFoundException"></exception>
    public Entity GetEntity(string code, string sql, Catalog<Entity> catalog = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException($"{nameof(code)} is empty");
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException($"{nameof(sql)} is empty");
        code = code.Trim().ToUpper();
        var entity = Entity.Null;
        if (catalog != null)
        {
            entity = catalog[code];
            if (entity != null && entity != Entity.Null)
                return entity;
        }
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.Add(new OracleParameter("code", code));
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                entity = CreateEntity(reader);
                catalog?.Add(entity);
            }
            reader.Close();
        }
        if (entity != null && entity != Entity.Null)
            return entity;
        throw new EntityNotFoundException(code);
    }
    /// <summary>
    /// Возвращает набор базовых сущностей из БД по указанному SQL запросу.
    /// </summary>
    /// <param name="sql">SQL запрос.</param>
    /// <returns>Массив базовых сущностей.</returns>
    /// <exception cref="ArgumentException"></exception>
    public Entity[] GetEntities(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException($"{nameof(sql)} is empty");
        var entities = new List<Entity>();
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = sql;
            using var reader = command.ExecuteReader();
            while (reader.Read())
                entities.Add(CreateEntity(reader));
            reader.Close();
        }
        return [.. entities];
    }

    private readonly Catalog<MeasureUnit> _measureUnits = new("MEASURE_UNITS", "Единицы измерения");
    private static MeasureUnit CreateMeasureUnit(DbDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var id = Utils.DbValueToShort(reader["id"]);
        var code = Utils.DbValueToString(reader["code"]);
        var name = Utils.DbValueToString(reader["name"]); 
        var shortName = Utils.DbValueToString(reader["short_name"]);
        var altName = Utils.DbValueToString(reader["alt_name"]);
        var displayName = Utils.DbValueToString(reader["display_name"]);
        var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
        var displayOrder = Utils.DbValueToShort(reader["display_order"]);
        var category = Utils.DbValueToString(reader["category"]);
        var isBasic = Utils.DbValueToShort(reader["is_basic"]) == 1;
        var coefficient = Utils.DbValueToDouble(reader["coefficient"]);
        var isDefault = Utils.DbValueToShort(reader["is_default"]) == 1;
        var note = Utils.DbValueToString(reader["note"]);

        var measureUnit = new MeasureUnit(id, code, name, shortName, altName, displayName, isActive, displayOrder, 
            category, isBasic, coefficient, isDefault, note);
        return measureUnit;
    }
    /// <summary>
    /// Возвращает единицу измерения по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <returns>Единица измерения.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public MeasureUnit GetMeasureUnit(short id)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Возвращает единицу измерения по коду.
    /// </summary>
    /// <param name="code">Код.</param>
    /// <returns>Единица измерения.</returns>
    /// <exception cref="EntityNotFoundException"></exception>
    public MeasureUnit GetMeasureUnit(string code)
    {
        code = !string.IsNullOrWhiteSpace(code) ? code.Trim() : string.Empty;
        var measureUnit = _measureUnits[code];
        if (measureUnit != null && measureUnit != MeasureUnit.Null && measureUnit != MeasureUnit.Empty)
            return measureUnit;
        using (var command = _connection.CreateCommand())
        {
            var SelectMeasureUnitByCodeSql = 
                $"select m.id, m.code,\n" +
                $"       m.name, m.short_name, m.alt_name, m.display_name,\n" +
                $"       m.is_active, m.display_order,\n" +
                $"       m.category, m.is_basic, m.coefficient, m.is_default,\n" +
                $"       m.note\n" +
                $"  from {CommonSchema}.measure_unit#v m\n" +
                $" where m.code = :code";
            command.CommandText = SelectMeasureUnitByCodeSql;
            command.Parameters.Add(new OracleParameter("code", code));
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                measureUnit = CreateMeasureUnit(reader);
                _measureUnits.Add(measureUnit);
            }                    
            reader.Close();
        }
        if (measureUnit != null && measureUnit != MeasureUnit.Null && measureUnit != MeasureUnit.Empty)
            return measureUnit;
        throw new EntityNotFoundException(code);
    }
    /// <summary>
    /// Возвращает набор единиц измерения.
    /// </summary>
    /// <returns>Массив единиц измерения.</returns>
    public MeasureUnit[] GetMeasureUnits()
    {
        var measureUnits = new List<MeasureUnit>();
        using (var command = _connection.CreateCommand())
        {
            var SelectMeasureUnitsSql =
                $"select m.id, m.code,\n" +
                $"       m.name, m.short_name, m.alt_name, m.display_name,\n" +
                $"       m.is_active, m.display_order,\n" +
                $"       m.category, m.is_basic, m.coefficient, m.is_default,\n" +
                $"       m.note\n" +
                $"  from {CommonSchema}.measure_unit#v";
            command.CommandText = SelectMeasureUnitsSql;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var measureUnit = CreateMeasureUnit(reader);
                measureUnits.Add(measureUnit);
            }
            reader.Close();
        }
        return [.. measureUnits];
    }

    public void Disconnect()
    {
        if (_connection.State != ConnectionState.Closed && _connection.State != ConnectionState.Broken)
            _connection.Close();
    }
    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
