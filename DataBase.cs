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
        SqlConnection sqlConnection = new SqlConnection(@"Data Source=W4SD-PC; Initial Catalog=Учет отказа оборудования; Integrated Security=True");
        public void openconnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                sqlConnection.Open();
            }
        }

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