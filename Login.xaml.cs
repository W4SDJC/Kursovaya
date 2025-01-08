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
            try
            {
                SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());

                adapter.SelectCommand = command;
                adapter.Fill(dt);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 1225)
                {
                    MessageBox.Show(
                        "Server connection error. Incorrect data may have been entered.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(
                        "An unknown error has occurred.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            finally
            {
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
        }
        private void RegButton(object sender, RoutedEventArgs e)
        {
            Registration regForm = new Registration();
            this.Hide();
            regForm.ShowDialog();
            this.Show();
        }

        private void textboxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorLabel.Content = "";
        }

        private void textboxPass_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorLabel.Content = "";
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Устанавливаем текст в TextBox, показывая пароль
            var password = passwordBox.Password;
            passwordBox.Visibility = Visibility.Collapsed; // Скрываем PasswordBox
            var textBox = new TextBox { Text = password, Width = 200 }; // Создаем новый TextBox для отображения пароля
            textBox.LostFocus += (s, args) =>
            {
                passwordBox.Password = textBox.Text; // Обновляем PasswordBox при потере фокуса
                passwordBox.Visibility = Visibility.Visible; // Возвращаем PasswordBox

            };
            textboxPass.Text = password;
            textboxPass.Visibility = Visibility.Visible;

        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Возвращаем PasswordBox и скрываем текстовый элемент

            passwordBox.Visibility = Visibility.Visible; // Показываем PasswordBox
            textboxPass.Visibility = Visibility.Collapsed;
            passwordBox.Password = textboxPass.Text;
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            textboxPass.Text = passwordBox.Password.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void textboxPass_Loaded(object sender, RoutedEventArgs e)
        {
            textboxPass.Visibility = Visibility.Collapsed;

        }

        private void ChangeIPButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeServerIP frm = new ChangeServerIP();
            this.Hide();
            frm.ShowDialog();
            this.Show();
        }
    }
}
