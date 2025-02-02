using System.Data;
using System.Windows;
using System.Windows.Media;
using BCrypt.Net;

namespace Kursovaya2
{
    public partial class Registration : Window
    {
        DataBase dataBase = new DataBase();

        public Registration()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string loginUser = LoginTextBox.Text.Trim();
            string password = PassTextBox.Password;
            string confirmPassword = ConfPassTextBox.Password;

            if (string.IsNullOrEmpty(loginUser) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ErrorLabel.Foreground = Brushes.Red;
                ErrorLabel.Content = "Пожалуйста, заполните все поля.";
                return;
            }

            if (password != confirmPassword)
            {
                ErrorLabel.Foreground = Brushes.Red;
                ErrorLabel.Content = "Пароли не совпадают.";
                return;
            }

            DataTable dt = dataBase.GetDataTable($"SELECT id, Логин FROM Учетныезаписи WHERE Логин='{loginUser}'");

            if (dt.Rows.Count == 1)
            {
                ErrorLabel.Foreground = Brushes.Red;
                ErrorLabel.Content = "Логин уже существует.";
            }
            else
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password); // Хэширование пароля
                dataBase.ExecuteQuery($"INSERT INTO Учетныезаписи (Логин, Пароль, Роль) VALUES ('{loginUser}', '{hashedPassword}', 'user')");
                ErrorLabel.Foreground = Brushes.Green;
                ErrorLabel.Content = "Регистрация успешна.";
            }
        }
    }
}