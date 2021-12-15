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
        protected readonly DbConnection _connection = null;
        protected readonly string _host = string.Empty;
        protected readonly string _database = string.Empty;
        public PgSqlStorage(string host, string database)
        {
            _connection = new NpgsqlConnection();
            _host = host;
            _database = database;
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

        private Entity CreateEntity(DbDataReader reader)
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
        public Entity GetEntity(int id, string sql, Catalog<Entity> catalog = null) 
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("sql is empty");
            id = id > 0 ? id : -1;
            if (catalog != null)
            {
                var entity = catalog[id];
                if (entity != null)
                    return entity;
            }
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.Add(new NpgsqlParameter("id", id));
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var entity = CreateEntity(reader);
                    if (catalog != null)
                        catalog.Add(entity);
                    return entity;
                }
            }
            throw new EntityNotFoundException(id);
        }
        public Entity GetEntity(string code, string sql, Catalog<Entity> catalog = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code is empty");
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("sql is empty");
            code = code.Trim().ToUpper();
            if (catalog != null)
            {
                var entity = catalog[code];
                if (entity != null)
                    return entity;
            }
            using (var command = _connection.CreateCommand()) 
            { 
                command.CommandText = sql;
                command.Parameters.Add(new NpgsqlParameter("code", code));
                using var reader = command.ExecuteReader();
                if (reader.Read()) 
                {
                    var entity = CreateEntity(reader);
                    if (catalog != null)
                        catalog.Add(entity);
                    return entity;
                }                    
            }
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
