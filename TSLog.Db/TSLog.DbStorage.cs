using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Security;
using TSLog.Common;

namespace TSLog.Db
{
    /// <summary>
    /// Базовый класс для работы с хранилищем БД Oracle
    /// </summary>
    public class DbStorage : IDisposable
    {
        protected OracleConnection _connection = null;
        protected string _dataSource = string.Empty;

        public DbStorage(string dataSource)
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
            using (var command = _connection.CreateCommand())
            {
                command.ExecuteNonQuery();
            }
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