using System.Data;
using System.Windows;
using System.Windows.Controls;
using BCrypt.Net;

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
            string loginUser = textboxLogin.Text.Trim();
            string passUser = passwordBox.Password;

            if (string.IsNullOrEmpty(loginUser) || string.IsNullOrEmpty(passUser))
            {
                ErrorLabel.Content = "Пожалуйста, заполните все поля.";
                return;
            }

            // Используем русские названия таблицы и полей
            DataTable dt = dataBase.GetDataTable($"SELECT id, Логин, Пароль FROM УчетныеЗаписи WHERE Логин='{loginUser}'");

            if (dt.Rows.Count == 1)
            {
                string hashedPassword = dt.Rows[0]["Пароль"].ToString();
                if (BCrypt.Net.BCrypt.Verify(passUser, hashedPassword))
                {
                    MainWindow frm = new MainWindow(loginUser); // Передаем логин в MainWindow
                    this.Hide();
                    frm.ShowDialog();
                    this.Show();
                }
                else
                {
                    ErrorLabel.Content = "Неверный пароль.";
                }
            }
            else
            {
                ErrorLabel.Content = "Логин не найден.";
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

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            textboxPass.Text = passwordBox.Password;
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            passwordBox.Visibility = Visibility.Collapsed;
            textboxPass.Visibility = Visibility.Visible;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            passwordBox.Visibility = Visibility.Visible;
            textboxPass.Visibility = Visibility.Collapsed;
            passwordBox.Password = textboxPass.Text;
        }

        private void textboxPass_TextChanged(object sender, TextChangedEventArgs e)
        {
            textboxPass.Text = passwordBox.Password.ToString();
        }

        private void textboxPass_Loaded(object sender, RoutedEventArgs e)
        {
            textboxPass.Visibility = Visibility.Collapsed;
        }
    }
}