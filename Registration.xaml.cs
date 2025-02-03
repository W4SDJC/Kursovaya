using System;
using System.Data;
using System.Data.SqlClient;
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

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(loginUser) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ErrorLabel.Foreground = Brushes.Red;
                ErrorLabel.Content = "Пожалуйста, заполните все поля.";
                return;
            }

            // Проверка на совпадение паролей
            if (password != confirmPassword)
            {
                ErrorLabel.Foreground = Brushes.Red;
                ErrorLabel.Content = "Пароли не совпадают.";
                return;
            }

            try
            {
                // Проверка на существование логина
                DataTable dt = dataBase.GetDataTable($"SELECT id, Логин FROM Учетныезаписи WHERE Логин='{loginUser}'");

                if (dt.Rows.Count == 1)
                {
                    ErrorLabel.Foreground = Brushes.Red;
                    ErrorLabel.Content = "Логин уже существует.";
                    return;
                }
                else
                {
                    // Хэширование пароля
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                    // Выполнение SQL-запроса
                    bool isSuccess = dataBase.ExecuteQuery($"INSERT INTO УчетныеЗаписи (Логин, Пароль, Роль) VALUES ('{loginUser}', '{hashedPassword}', 'user')");

                    if (isSuccess)
                    {
                        // Успешная регистрация
                        ErrorLabel.Foreground = Brushes.Green;
                        ErrorLabel.Content = "Регистрация успешна.";
                    }
                    else
                    {
                        // Ошибка при выполнении запроса
                        ErrorLabel.Foreground = Brushes.Red;
                        ErrorLabel.Content = "Ошибка при регистрации. Пожалуйста, попробуйте еще раз.";
                    }

                }
            }
            catch (SqlException ex)
            {
                // Обработка ошибок подключения к БД
                ErrorLabel.Foreground = Brushes.Red;
                ErrorLabel.Content = "Ошибка подключения к базе данных. Пожалуйста, проверьте настройки подключения.";
                // Логирование ошибки (опционально)
                // Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                // Обработка других ошибок
                ErrorLabel.Foreground = Brushes.Red;
                ErrorLabel.Content = "Произошла ошибка. Пожалуйста, попробуйте еще раз.";
                // Логирование ошибки (опционально)
                // Console.WriteLine(ex.Message);
            }
        }
    }
}