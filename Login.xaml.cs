using System;
using System.Data;
using System.Data.SqlClient;
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

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(loginUser) || string.IsNullOrEmpty(passUser))
            {
                ErrorLabel.Content = "Пожалуйста, заполните все поля.";
                return;
            }

            try
            {
                // Используем параметризованный запрос для предотвращения SQL-инъекции
                string query = "SELECT id, Логин, Пароль FROM УчетныеЗаписи WHERE Логин = @Логин";
                using (SqlConnection connection = new SqlConnection(dataBase.GetConnectionString()))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Логин", loginUser);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string hashedPassword = reader["Пароль"].ToString();
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
                    }
                }
            }
            catch (SqlException ex)
            {
                // Обработка ошибок подключения к БД
                ErrorLabel.Content = "Ошибка подключения к базе данных. Пожалуйста, проверьте настройки подключения.";
                // Логирование ошибки (опционально)
                // Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                // Обработка других ошибок
                ErrorLabel.Content = "Произошла ошибка. Пожалуйста, попробуйте еще раз.";
                // Логирование ошибки (опционально)
                // Console.WriteLine(ex.Message);
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