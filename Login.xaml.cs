using System;
using System.Collections.Generic;
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
using System.Reflection.Emit;

namespace Kursovaya2
{

    public partial class Login : Window
    {
        DataBase dataBase = new DataBase();

        public Login()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var loginUser = textboxLogin.Text;
            var passUser = textboxPass.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable dt = new DataTable();

            string querystring = $"select id_user, login, password from Accounts where login='{loginUser}' and password = '{passUser}'";

            SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());

            adapter.SelectCommand = command;
            adapter.Fill(dt);

            if (dt.Rows.Count == 1)
            {
                MainWindow frm = new MainWindow();
                this.Hide();
                frm.ShowDialog();
                this.Show();
            }
            else
            {
                ErrorLabel.Content = "Login failed, account not finded!";
            }
        }

        private void RegButton(object sender, RoutedEventArgs e)
        {
            var loginUser = textboxLogin.Text;
            var passUser = textboxPass.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable dt = new DataTable();

            string querystring = $"select id_user, login, password from Accounts where login='{loginUser}' and password = '{passUser}'";

            SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());

            adapter.SelectCommand = command;
            adapter.Fill(dt);

            if (dt.Rows.Count == 1)
            {
                MessageBox.Show("Login succesfull!", "Succesfull!");
                MainWindow frm = new MainWindow();
                this.Hide();
                frm.ShowDialog();
                this.Show();
            }
            else
            {
                


            }
        }

        private void textboxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorLabel.Content = "";
        }

        private void textboxPass_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorLabel.Content = "";
        }
    }
}
