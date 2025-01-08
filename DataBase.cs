using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Kursovaya2
{
    class DataBase
    {
        SqlConnection sqlConnection = new SqlConnection(@"Data Source=127.0.0.1,14332; Initial Catalog=Учет отказа оборудования;User ID=sa;Password=123456789; TrustServerCertificate=True");
        public void openconnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                sqlConnection.Open();
            }
        }
        //sql
        public void closeConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Open();
            }
        }

        public SqlConnection GetConnection()
        {
            return sqlConnection;
        }

    }
}