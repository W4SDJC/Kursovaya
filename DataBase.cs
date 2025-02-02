using System;
using MySql.Data.MySqlClient;
using System.Data;

namespace Kursovaya2
{
    public class DataBase
    {
        private MySqlConnection connection;

        // Конструктор класса, который инициализирует подключение к базе данных
        public DataBase()
        {
            string connectionString = GetConnectionString();
            connection = new MySqlConnection(connectionString);
        }

        // Метод для получения строки подключения
        private string GetConnectionString()
        {
            // Здесь можно использовать конфигурационный файл для хранения строки подключения
            // Но для простоты я использую строку подключения напрямую
            return "server=localhost;user=root;password=yourpassword;database=УчетОтказовОборудования;";
        }

        // Метод для открытия подключения
        public void OpenConnection()
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        // Метод для закрытия подключения
        public void CloseConnection()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        // Метод для выполнения запросов, которые не возвращают данные (например, INSERT, UPDATE, DELETE)
        public void ExecuteQuery(string query)
        {
            try
            {
                OpenConnection();
                MySqlCommand command = new MySqlCommand(query, connection);
                command.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                // Обработка ошибок
                Console.WriteLine("Ошибка при выполнении запроса: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }

        // Метод для выполнения запросов, которые возвращают данные (например, SELECT)
        public DataTable GetDataTable(string query)
        {
            DataTable dataTable = new DataTable();
            try
            {
                OpenConnection();
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataTable);
            }
            catch (MySqlException ex)
            {
                // Обработка ошибок
                Console.WriteLine("Ошибка при выполнении запроса: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return dataTable;
        }

        public DataTable GetSchemaTable(string tableName)
        {
            DataTable schemaTable = new DataTable();
            string query = $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
            try
            {
                OpenConnection();
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(schemaTable);
            }
            catch (MySqlException ex)
            {
                // Обработка ошибок
                Console.WriteLine("Ошибка при получении схемы таблицы: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return schemaTable;
        }
    }
}