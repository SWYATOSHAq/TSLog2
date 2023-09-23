using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data.Common;

namespace TSLog.Yard.DbConnectionTester
{
    internal class Worker
    {
        public enum DisposeMethod
        {
            NotDispose, Direct, Using
        }
        private const int ConnectionsCount = 64;
        private const int DelayTimeMs = 4 * 1024;
        private const bool DefaultPooling = true;
        private const int DefaultMaxPoolSize = 128;
        private const int DefaultConnectionIdleLifetime = 16;
        private const int DefaultConnectionPruningInterval = 16;
        private readonly Random Generator = new();

        private readonly string _host = string.Empty;
        private readonly string _userName = string.Empty;
        private readonly string _password = string.Empty;
        private readonly string _database = string.Empty;
        private readonly string _yardSchema = string.Empty;
        private readonly bool _pooling = DefaultPooling;
        private readonly int _MaxPoolSize = DefaultMaxPoolSize;
        private readonly int _connectionIdleLifetime = DefaultConnectionIdleLifetime;
        private readonly int _connectionPruningInterval = DefaultConnectionPruningInterval;

        public Worker(IConfigurationRoot configuration) 
        {
            var connectionSettings = configuration.GetSection("ConnectionSettings");
            _host = connectionSettings.GetValue<string>("Host") ?? string.Empty;
            _userName = connectionSettings.GetValue<string>("UserName") ?? string.Empty;
            _password = connectionSettings.GetValue<string>("Password") ?? string.Empty;
            _database = connectionSettings.GetValue<string>("Database") ?? string.Empty;
            _yardSchema = connectionSettings.GetValue<string>("YardSchema") ?? string.Empty;
            _pooling = connectionSettings.GetValue<bool>("Pooling");
            _MaxPoolSize = connectionSettings.GetValue<int>("MaxPoolSize");
            _connectionIdleLifetime = connectionSettings.GetValue<int>("ConnectionIdleLifetime");
            _connectionPruningInterval = connectionSettings.GetValue<int>("ConnectionPruningInterval");
        }
        private string CreateConnectionString()
        {
            var stringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = _host,
                Username = _userName,
                Password = _password,
                Database = _database,
                Pooling = _pooling,
                ConnectionIdleLifetime = _connectionIdleLifetime,
                ConnectionPruningInterval = _connectionPruningInterval,
                MaxPoolSize = _MaxPoolSize,
            };
            return stringBuilder.ToString();
        }
        private string CreateSql()
        {
            var gateId = Generator.Next(1000, 1050);
            return $"select count(*) from {_yardSchema}.v_gate g where g.id = {gateId}";
        }
        private void DoConnectionWork(DbConnection connection,
            bool callClose = true,
            bool throwException = false)
        {
            connection.ConnectionString = CreateConnectionString();
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = CreateSql();
                var result = command.ExecuteScalar();
                if (throwException)
                    throw new CustomException();
            }
            if (callClose)
                connection.Close();
        }

        private readonly NpgsqlConnection _connection = new NpgsqlConnection();

        private void UseConnection(bool callClose = true,
            DisposeMethod disposeMethod = DisposeMethod.Using, 
            bool throwException = false)
        {
            if (disposeMethod == DisposeMethod.Using)
            {
                using var connection = new NpgsqlConnection();
                DoConnectionWork(connection, callClose, throwException);
            }
            else
            {
                var connection = new NpgsqlConnection();
                DoConnectionWork(connection, callClose, throwException);
                if (disposeMethod == DisposeMethod.Direct)
                    connection.Dispose();
            }

        }
        private async Task UseConnectionAsync(bool callClose = true,
            DisposeMethod disposeMethod = DisposeMethod.Using,
            bool throwException = false)
        {
            await Task.Delay(DelayTimeMs);
            UseConnection(callClose, disposeMethod, throwException);
        }
        private void UseSingleConnection()
        {
            using var command = _connection.CreateCommand();
            command.CommandText = CreateSql();
            var result = command.ExecuteScalar();
        }
        private async Task UseSingleConnectionAsync()
        {
            await Task.Delay(DelayTimeMs);
            UseSingleConnection();
        }

        public void TestConnections(
            bool callClose = true,
            DisposeMethod disposeMethod = DisposeMethod.Using,
            bool throwException = false)
        {
            var index = 0;
            while (index < ConnectionsCount)
            {
                index++;
                try
                {
                    UseConnection(callClose, disposeMethod, throwException);
                }
                catch (CustomException)
                { }
                catch (Exception) 
                {
                    throw;
                }
            }
        }
        public async Task TestConnectionsAsync(
            bool callClose = true,
            DisposeMethod disposeMethod = DisposeMethod.Using,
            bool throwException = false)
        {
            var index = 0;
            var tasks = new List<Task>();
            while (index < ConnectionsCount)
            {
                index++;
                try
                {
                    var task = UseConnectionAsync(callClose, disposeMethod, throwException);
                    tasks.Add(task);
                }
                catch (CustomException)
                { }
                catch (Exception)
                {
                    throw;
                }
            }
            await Task.WhenAll(tasks);
        }
        public void TestSingleConnection()
        {
            var index = 0;            
            while (index < ConnectionsCount)
            {
                index++;
                try
                {
                    UseSingleConnection();
                }
                catch
                { }
            }
        }
        public async Task TestSingleConnectionAsync()
        {
            var index = 0;
            var tasks = new List<Task>();
            while (index < ConnectionsCount)
            {
                index++;
                try
                {
                    var task = UseSingleConnectionAsync();
                    tasks.Add(task);
                }
                catch
                { }
            }
            await Task.WhenAll(tasks);
        }
    }
}
