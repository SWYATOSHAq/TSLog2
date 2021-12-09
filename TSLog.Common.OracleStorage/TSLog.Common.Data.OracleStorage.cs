using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Security;

namespace TSLog.Common.Data
{
    /// <summary>
    /// Базовый класс для работы с хранилищем БД Oracle
    /// </summary>
    public class OracleStorage : IDisposable
    {
        protected OracleConnection _connection = null;
        protected string _dataSource = string.Empty;

        public OracleStorage(string dataSource)
        {
            _connection = new OracleConnection();
            _dataSource = dataSource;
        }

        public void Connect(string user, SecureString password)
        {
            var stringBuilder = new OracleConnectionStringBuilder();
            stringBuilder.DataSource = _dataSource;
            stringBuilder.UserID = user;
            stringBuilder.Password = Utils.SecureStringToString(password);

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