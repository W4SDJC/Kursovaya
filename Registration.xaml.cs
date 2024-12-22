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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kursovaya2
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        DataBase dataBase = new DataBase();

        public Registration()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (PassTextBox.Text == ConfPassTextBox.Text)
            {
                string loginUser = LoginTextBox.Text;
                string password = PassTextBox.Text;
                SqlDataAdapter adapter = new SqlDataAdapter();
                DataTable dt = new DataTable();

                string querystring = $"select id_user, login from Accounts where login='{loginUser}'";

                SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());

                adapter.SelectCommand = command;
                adapter.Fill(dt);

                if (dt.Rows.Count == 1)
                {
                    ErrorLabel.Foreground = Brushes.Red;
                    ErrorLabel.Content = "Login is already exist";
                }
                else {

                    string AddUser = $"INSERT INTO Account (login, password) VALUES ({loginUser},{password})";

                    SqlCommand AddCommand = new SqlCommand(AddUser,dataBase.GetConnection());

                    ErrorLabel.Foreground = Brushes.Green;
                    ErrorLabel.Content = "Succesfull";
                }
            }
            else {
                ErrorLabel.Foreground = Brushes.Red;
                ErrorLabel.Content = "Passwords don't match";
            }
        }
    }
}
