using Npgsql;
using System.Data;
using System.Data.Common;
using System.Security;
using TSLog.Common.BO;
using TSLog.ExceptionHandler;

namespace TSLog.Common.Data
{
    public class PgSqlStorage : IDisposable
    {
        protected readonly DbConnection _connection = new NpgsqlConnection();
        protected readonly string _host = string.Empty;
        protected readonly string _database = string.Empty;
        public PgSqlStorage(string host, string database)
        {
            _host = host;
            _database = database;
        }
        public DbConnection CloneConnection()
        {
            var connection = new NpgsqlConnection();
            connection.ConnectionString = _connection.ConnectionString;
            connection.Open();
            return connection;
        }

        public void Connect(string user, SecureString password)
        {
            var stringBuilder = new NpgsqlConnectionStringBuilder();
            stringBuilder.Host = _host;
            stringBuilder.Database = _database;
            stringBuilder.Username = user;
            stringBuilder.Password = Utils.SecureStringToString(password);

            _connection.ConnectionString = stringBuilder.ConnectionString;
            _connection.Open();
            stringBuilder.Clear();
        }
        public void Execute(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("sql is null");
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        protected Entity CreateEntity(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            return new Entity(Convert.ToInt32(reader["id"]),
                Convert.ToString(reader["code"]),
                Convert.ToString(reader["name"]),
                Convert.ToString(reader["short_name"]),
                Convert.ToString(reader["alt_name"]),
                Convert.ToString(reader["display_name"]),
                Convert.ToString(reader["note"]));
        }
        public Entity GetEntity(int id, string sql, Catalog<Entity>? catalog = null)
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
            if (entity != null && entity != Entity.Null)
                return entity;
            throw new EntityNotFoundException(id);
        }
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
                if (entity != null && entity != Entity.Null)
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
            if (entity != null && entity != Entity.Null)
                return entity;
            throw new EntityNotFoundException(code);
        }
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
            return entities.ToArray();
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
        }
    }
}
