using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Npgsql;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Security;
using TSLog.Common.BO;
using TSLog.ExceptionHandler;

namespace TSLog.Common.Data
{
    /// <summary>
    /// Класс для работы с данными из БД PostgreSQL.
    /// </summary>
    public class PgSqlStorage : IDisposable
    {
        private const bool DefaultPooling = true;
        private const int DefaultMaxPoolSize = 100;
        private const int DefaultConnectionIdleLifetime = 300;
        private const int DefaultonnectionPruningInterval = 10;
        protected readonly DbConnection _connection = new NpgsqlConnection();
        protected readonly string _host = string.Empty;
        public string Host
        {
            get { return _host; }
        }
        protected readonly string _database = string.Empty;
        // Использовать пулинг соединений
        // (соединения не уничтожаются при закрытии в переходят в idle состояния и могут использоваться повторно).
        protected readonly bool _pooling = DefaultPooling;

        // Максимальный размер пула соединений (работает только при Pooling=true),
        // не может быть больше чем анаогичный параметр на сервере.
        protected readonly int _maxPoolSize = DefaultMaxPoolSize;

        // Время ожидания idle соединения (в секундах).
        protected readonly int _connectionIdleLifetime = DefaultConnectionIdleLifetime;

        // Время ожидания idle соединения (в секундах), пока пул его не закроет.
        protected readonly int _connectionPruningInterval = DefaultonnectionPruningInterval;

        public PgSqlStorage(string host, string database,
            IDictionary<string, object>? extensionParameters = null)
        {
            _host = host;
            _database = database;
            if (extensionParameters != null)
            {
                if (extensionParameters.ContainsKey("Pooling"))
                    _pooling = (bool)extensionParameters["Pooling"];
                if (extensionParameters.ContainsKey("MaxPoolSize"))
                    _maxPoolSize = (int)extensionParameters["MaxPoolSize"];
                if (extensionParameters.ContainsKey("ConnectionIdleLifetime"))
                    _connectionIdleLifetime = (int)extensionParameters["ConnectionIdleLifetime"];
                if (extensionParameters.ContainsKey("ConnectionPruningInterval"))
                    _connectionPruningInterval = (int)extensionParameters["ConnectionPruningInterval"];
            }
        }
        public DbConnection CloneConnection()
        {
            var connection = new NpgsqlConnection
            {
                ConnectionString = _connection.ConnectionString
            };
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Установить соединение с БД.
        /// </summary>
        /// <param name="user">Имя пользователя БД</param>
        /// <param name="password">Пароль</param>
        public void Connect(string user, SecureString password)
        {
            var stringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = _host,
                Database = _database,
                Pooling = _pooling,
                MaxPoolSize = _maxPoolSize,
                ConnectionLifetime = _connectionIdleLifetime,
                ConnectionPruningInterval = _connectionPruningInterval,
                Username = user,
                Password = Utils.SecureStringToString(password)
            };
            stringBuilder.PersistSecurityInfo = true;
            _connection.ConnectionString = stringBuilder.ConnectionString;
            _connection.Open();
            stringBuilder.Clear();
        }
        /// <summary>
        /// Выполнить SQL запрос.
        /// </summary>
        /// <param name="sql">Текст SQL запроса</param>
        /// <exception cref="ArgumentException"></exception>
        public void Execute(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("sql is null");
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
        /// <summary>
        /// Возвращает сущность по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="sql">SQL запрос для получения сущности</param>
        /// <param name="catalog">Каталог для поиска сущости в кэше</param>
        /// <returns>Сущность</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="EntityNotFoundException"></exception>
        public Entity GetEntity(int id, string sql, Catalog<Entity>? catalog = null)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("sql is empty");
            id = id > 0 ? id : -1;
            var entity = Entity.Null;
            if (catalog != null)
            {
                entity = catalog[id];
                if (entity != null && entity != Entity.Null && entity != Entity.Empty)
                    return entity;
            }
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.Add(new NpgsqlParameter("id", id));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        entity = CreateEntity(reader);
                        if (catalog != null)
                            catalog.Add(entity);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (entity != null && entity != Entity.Null && entity != Entity.Empty)
                return entity;
            throw new EntityNotFoundException(id);
        }
        /// <summary>
        /// Возвращает сущность по коду.
        /// </summary>
        /// <param name="code">Код</param>
        /// <param name="sql">SQL запрос для получеия сущности</param>
        /// <param name="catalog">Каталог для поиска сущности в кэше</param>
        /// <returns>Сущность</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="EntityNotFoundException"></exception>
        public Entity GetEntity(string code, string sql, Catalog<Entity>? catalog = null)
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
                if (entity != null && entity != Entity.Null && entity != Entity.Empty)
                    return entity;
            }
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.Add(new NpgsqlParameter("code", code));
                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        entity = CreateEntity(reader);
                        if (catalog != null)
                            catalog.Add(entity);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            if (entity != null && entity != Entity.Null && entity != Entity.Empty)
                return entity;
            throw new EntityNotFoundException(code);
        }
        /// <summary>
        /// Возвращает сущности, которые возвращаются указанным SQL запросом.
        /// </summary>
        /// <param name="sql">Текст SQL запроса</param>
        /// <returns>Массив сущностей</returns>
        /// <exception cref="ArgumentException"></exception>
        public Entity[] GetEntities(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("sql is empty");
            var entities = new List<Entity>();
            using (var connection = CloneConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                        entities.Add(CreateEntity(reader));
                    reader.Close();
                }
                connection.Close();
            }
            entities.Sort();
            return entities.ToArray();
        }

        /// <summary>
        /// Отсоединиться от БД.
        /// </summary>
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
