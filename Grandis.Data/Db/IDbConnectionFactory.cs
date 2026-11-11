using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Grandis.Data.Db
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }

    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        //public IDbConnection CreateConnection()
        //    => new SqlConnection(_connectionString);

        public SqlConnection CreateConnection() => new(_connectionString);
    }
}
