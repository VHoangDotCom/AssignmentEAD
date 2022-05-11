﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotReceiver
{
    class ConnectDB
    {
        private static SqlConnection _connection;
        //private const string ConnectionString = @"Server=tcp:testwebserverclientdbserver.database.windows.net,1433;Initial Catalog=RabbitMQAssignment_db;Persist Security Info=False;User ID=sqladmin;Password=Bach@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewsFeed;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public static SqlConnection GetConnectSql()
        {
            if (_connection == null || _connection.State == System.Data.ConnectionState.Closed)
            {
                _connection = new SqlConnection(
                    string.Format(ConnectionString));
            }
            return _connection;
        }
    }
}
