using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kursovaya2
{
    public partial class MainWindow : Window
    {
        DataBase dataBase = new DataBase();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ComboBoxData_Loaded(object sender, RoutedEventArgs e)
        {
            string Sql = "SELECT TableName FROM TablesName";
            SqlConnection conn = new SqlConnection(@"Data Source=W4SD-PC; Initial Catalog=Учет отказа оборудования; Integrated Security=True");
            conn.Open();
            SqlCommand cmd = new SqlCommand(Sql, conn);
            SqlDataReader DR = cmd.ExecuteReader();

            while (DR.Read())
            {
                TNComboBox.Items.Add(DR[0]);
            }
        }

        private void TNComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            string selectedTable = TNComboBox.SelectedItem.ToString().Trim();

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable dt = new DataTable();

            string querystring = $"SELECT * FROM {selectedTable}";

            SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());

            adapter.SelectCommand = command;
            adapter.Fill(dt);
            MyDataGrid.ItemsSource = dt.DefaultView;
            command.Dispose();
            MWindow.Title = $"Редактирование таблицы {selectedTable}";
        }
    }
}
