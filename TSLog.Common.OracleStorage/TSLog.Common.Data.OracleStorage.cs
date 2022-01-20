using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Security;
using TSLog.Common.BO;
using TSLog.ExceptionHandler;

namespace TSLog.Common.Data
{
    /// <summary>
    /// Базовый класс для работы с хранилищем БД Oracle
    /// </summary>
    public class OracleStorage : IDisposable
    {
        protected OracleConnection _connection = null;
        protected string _dataSource = string.Empty;

        private const string DefaultCommonSchema = "ecco";
        /// <summary>
        /// Схема для "общих" объектов
        /// </summary>
        public string CommonSchema { protected get; set; } = DefaultCommonSchema;

        public OracleStorage(string dataSource)
        {
            _connection = new OracleConnection();
            _dataSource = dataSource;
        }

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
        public void Execute(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("sql is empty");
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        protected static Entity CreateEntity(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var id = Utils.DbValueToLong(reader["id"]);
            var code = Utils.DbValueToString(reader["code"]);
            var name = Utils.DbValueToString(reader["name"]);
            var shortName = Utils.DbValueToString(reader["short_name"]);
            var altName = Utils.DbValueToString(reader["alt_name"]);
            var displayName = Utils.DbValueToString(reader["display_name"]);
            var isActive = Utils.DbValueToShort(reader["is_active"]) == 1;
            var displayOrder = Utils.DbValueToShort(reader["display_order"]);
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
        public Entity GetEntity(int id, string sql, Catalog<Entity> catalog = null)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("sql is empty");
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
                    if (catalog != null)
                        catalog.Add(entity);
                }
                reader.Close();
            }
            if (entity != null && entity != Entity.Null)
                return entity;
            throw new EntityNotFoundException(id);
        }
        public Entity GetEntity(string code, string sql, Catalog<Entity> catalog = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("sql is empty");
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
                    if (catalog != null)
                        catalog.Add(entity);
                }
                reader.Close();
            }
            if (entity != null && entity != Entity.Null)
                return entity;
            throw new EntityNotFoundException(code);
        }
        public Entity[] GetEntities(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("sql is empty");
            var entities = new List<Entity>();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    entities.Add(CreateEntity(reader));
                reader.Close();
            }
            return entities.ToArray();
        }

        private readonly Catalog<MeasureUnit> _measureUnits = new("MEASURE_UNITS", "Единицы измерения");
        private static MeasureUnit CreateMeasureUnit(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

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
        public MeasureUnit GetMeasureUnit(short id)
        {
            throw new NotImplementedException();
        }
        public MeasureUnit GetMeasureUnit(string code)
        {
            code = !string.IsNullOrWhiteSpace(code) ? code.Trim() : string.Empty;
            var measureUnit = _measureUnits[code];
            if (measureUnit != null && measureUnit != MeasureUnit.Null)
                return measureUnit;
            using (var command = _connection.CreateCommand())
            {
                var SelectMeasureUnitByCodeSql = $"select * from {CommonSchema}.v_measure_unit m where m.code = :code";
                command.CommandText = SelectMeasureUnitByCodeSql;
                command.Parameters.Add(new OracleParameter("code", code));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                    measureUnit = CreateMeasureUnit(reader);
                reader.Close();
            }
            return measureUnit ?? MeasureUnit.Null;
        }
        public MeasureUnit[] GetMeasureUnits()
        {
            var measureUnits = new List<MeasureUnit>();
            using (var command = _connection.CreateCommand())
            {
                var SelectMeasureUnitsSql = $"select * from {CommonSchema}.v_measure_unit";
                command.CommandText = SelectMeasureUnitsSql;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var measureUnit = CreateMeasureUnit(reader);
                    measureUnits.Add(measureUnit);
                }
                reader.Close();
            }
            return measureUnits.ToArray();
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
}