using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Threading;
using DocumentFormat.OpenXml.Vml;
namespace Kursovaya2
{
    public partial class MainWindow : Window
    {
        private string currentUserLogin;

        DataBase dataBase = new DataBase();
        // Новый конструктор, который принимает логин
        private DispatcherTimer _timer;

        public MainWindow(string login)
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.5); // Задержка 1 секунда
            _timer.Tick += Timer_Tick;

            currentUserLogin = login;
            // Устанавливаем логин в Label
            LabelCurrentUser.Content = currentUserLogin;
            ComboBoxData_Loaded(null, null);
            MyDataGrid.Focus();

            // Подписываемся на событие PreviewKeyDown
            MyDataGrid.PreviewKeyDown += MyDataGrid_PreviewKeyDown;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Сбрасываем таймер при каждом изменении текста
            _timer.Stop();
            _timer.Start();
        }

        // Обработчик срабатывания таймера
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Останавливаем таймер
            _timer.Stop();

            // Выполняем нужное действие
            ProcessTextChange();
        }
        private void ProcessTextChange()
        {
            // Ваш код для обработки изменения текста
            string text = SearchTextBox.Text;
            Search();
            SearchTextBox.Focus();
        }

        // Существующий конструктор без параметров (можно удалить, если он не нужен)
        public MainWindow()
        {
            InitializeComponent();
            ComboBoxData_Loaded(null, null);
            MyDataGrid.Focus();
            // Подписываемся на событие PreviewKeyDown
            MyDataGrid.PreviewKeyDown += MyDataGrid_PreviewKeyDown;
        }
        private void MyDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (MyDataGrid.SelectedItem != null)
                {
                    // Вызываем метод удаления
                    DeleteButton_Click(this, new RoutedEventArgs());
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите строку для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                e.Handled = true; // Предотвращаем дальнейшую обработку события
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Отладочное сообщение для проверки, что метод вызывается
            System.Diagnostics.Debug.WriteLine($"Key pressed: {e.Key}");

            if (e.Key == Key.F5)
            {
                // Вызываем метод обновления
                RefreshButton_Click(this, new RoutedEventArgs());
                e.Handled = true; // Предотвращаем дальнейшую обработку события
            }
            else if (e.Key == Key.Delete)
            {
                // Отладочное сообщение для проверки, что Delete нажата
                System.Diagnostics.Debug.WriteLine("Delete key pressed");

                // Вызываем метод удаления
                DeleteButton_Click(this, new RoutedEventArgs());
                e.Handled = true; // Предотвращаем дальнейшую обработку события
            }
        }
        private void ComboBoxData_Loaded(object sender, RoutedEventArgs e)
        {
            string Sql = @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'УчетОтказовОборудования' AND TABLE_NAME <> 'УчетныеЗаписи'";

            try
            {
                DataTable dt = dataBase.GetDataTable(Sql);

                foreach (DataRow row in dt.Rows)
                {
                    TNComboBox.Items.Add(row["TABLE_NAME"].ToString());
                }

                if (dt.Rows.Count > 0)
                {
                    TNComboBox.SelectedIndex = 0;
                    TNComboBox.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке таблиц: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TNComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            string selectedTable = TNComboBox.SelectedItem.ToString().Trim();

            string queryString = string.Empty;

            switch (selectedTable)
            {
                case "ТехническиеОсмотры":
                    queryString = @"
                SET lc_time_names = 'ru_RU';
                SELECT 
                    ТО.id,
                    ОБ.НомерОборудования,
                    DATE_FORMAT( ТО.ДатаОсмотра , '%d %M %Y' ) as 'Дата Осмотра',
                    ТО.Результат,
                    ТО.Причина,
                    С.ТабельныйНомер,
                    С.Имя,
                    С.Должность
                FROM 
                    ТехническиеОсмотры ТО
                JOIN 
                    Оборудование ОБ ON ТО.ОборудованиеID = ОБ.id
                JOIN 
                    Сотрудники С ON ТО.СотрудникID = С.id";
                    break;

                case "ОтказыОборудования":
                    queryString = @"
                SET lc_time_names = 'ru_RU';
                SELECT 
                    ОО.id,
                    ОБ.Название,
                    DATE_FORMAT( ОО.ДатаОтказа , '%d %M %Y' ) as 'Дата Отказа',
                    ОО.Причина,
                    С.ТабельныйНомер,
                    С.Имя,
                    С.Должность                    
                FROM 
                    ОтказыОборудования ОО
                JOIN 
                    Оборудование ОБ ON ОО.ОборудованиеID = ОБ.ID
                JOIN 
                    Сотрудники С ON ОО.СотрудникID = С.id";
                
                    break;

                // Добавьте другие таблицы и их соответствующие запросы
                default:
                    queryString = $"SELECT * FROM {selectedTable}";
                    break;
            }

            try
            {
                DataTable dt = dataBase.GetDataTable(queryString);
                MyDataGrid.ItemsSource = dt.DefaultView;
                this.Title = $"Редактирование таблицы {selectedTable}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            MyDataGrid.Focus();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            TNComboBox_SelectionChanged_1(null, null);
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Логика для добавления данных
            string selectedTable = TNComboBox.SelectedItem.ToString().Trim();
            AddEditWindow addEditWindow = new AddEditWindow(selectedTable, null);
            addEditWindow.ShowDialog();
            TNComboBox_SelectionChanged_1(null, null);
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Логика для редактирования данных
            if (MyDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)MyDataGrid.SelectedItem;
                string selectedTable = TNComboBox.SelectedItem.ToString().Trim();
                AddEditWindow addEditWindow = new AddEditWindow(selectedTable, selectedRow);
                addEditWindow.ShowDialog();
                TNComboBox_SelectionChanged_1(null, null);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите строку для редактирования.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Логика для удаления данных
            if (MyDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)MyDataGrid.SelectedItem;
                string selectedTable = TNComboBox.SelectedItem.ToString().Trim();

                string primaryKey = GetPrimaryKey(selectedTable);
                if (primaryKey != null)
                {
                    string id = selectedRow[primaryKey].ToString();
                    string confirmMessage = $"Вы уверены, что хотите удалить запись с ID {id}?";
                    var result = MessageBox.Show(confirmMessage, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        string deleteQuery = $"DELETE FROM {selectedTable} WHERE {primaryKey} = {id}";
                        dataBase.ExecuteQuery(deleteQuery);
                        TNComboBox_SelectionChanged_1(null, null);
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось определить первичный ключ для таблицы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите строку для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика для добавления данных
            string selectedTable = TNComboBox.SelectedItem.ToString().Trim();
            AddEditWindow addEditWindow = new AddEditWindow(selectedTable, null);
            addEditWindow.ShowDialog();
            TNComboBox_SelectionChanged_1(null, null);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика для редактирования данных
            if (MyDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)MyDataGrid.SelectedItem;
                string selectedTable = TNComboBox.SelectedItem.ToString().Trim();
                AddEditWindow addEditWindow = new AddEditWindow(selectedTable, selectedRow);
                addEditWindow.ShowDialog();
                TNComboBox_SelectionChanged_1(null, null);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите строку для редактирования.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)MyDataGrid.SelectedItem;
                string selectedTable = TNComboBox.SelectedItem?.ToString().Trim();

                if (string.IsNullOrEmpty(selectedTable))
                {
                    MessageBox.Show("Не выбрана таблица для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string primaryKey = GetPrimaryKey(selectedTable);
                if (primaryKey != null)
                {
                    string id = selectedRow[primaryKey].ToString();
                    string confirmMessage = $"Вы уверены, что хотите удалить запись с ID {id}?";
                    var result = MessageBox.Show(confirmMessage, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    // Отладочное сообщение
                    System.Diagnostics.Debug.WriteLine($"SelectedTable: {selectedTable}, PrimaryKey: {primaryKey}, ID: {id}, Confirmation Result: {result}");

                    if (result == MessageBoxResult.Yes)
                    {
                        string deleteQuery = $"DELETE FROM {selectedTable} WHERE {primaryKey} = {id}";
                        try
                        {
                            dataBase.ExecuteQuery(deleteQuery);
                            MessageBox.Show("Запись успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            TNComboBox_SelectionChanged_1(null, null);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось определить первичный ключ для таблицы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите строку для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string GetPrimaryKey(string tableName)
        {
            // Логика для получения первичного ключа таблицы
            // Здесь можно использовать SQL-запрос для получения информации о первичном ключе
            // Например:
            // SELECT COLUMN_NAME
            // FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            // WHERE TABLE_NAME = 'your_table' AND CONSTRAINT_NAME LIKE 'PK_%'
            // Возвращаемое значение - имя столбца первичного ключа
            return "id"; // Пример: предполагаем, что первичный ключ - это 'id'
        }

        private void GenerateReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Логика для генерации отчетов
            string selectedTable = TNComboBox.SelectedItem.ToString().Trim();
            ReportWindow reportWindow = new ReportWindow(selectedTable);
            reportWindow.ShowDialog();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void Search()
        {
            string searchText = SearchTextBox.Text.Trim();
            string selectedTable = TNComboBox.SelectedItem?.ToString().Trim();

            if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(selectedTable))
            {
                TNComboBox_SelectionChanged_1(null, null);
                return;
            }

            string queryString = string.Empty;

            switch (selectedTable)
            {
                case "Сотрудники":
                    queryString = $@"SELECT * FROM Сотрудники 
                 WHERE Имя LIKE '%{searchText}%' 
                 OR ТабельныйНомер LIKE '%{searchText}%' 
                 OR Должность LIKE '%{searchText}%'";
                    break;

                case "ПроизводственныеУчастки":
                    queryString = $@"SELECT * FROM ПроизводственныеУчастки 
                    WHERE Название LIKE '%{searchText}%' 
                    OR НомерУчастка LIKE '%{searchText}%'";
                    break;

                case "Оборудование":
                    queryString = $@"SELECT ОБ.id, ОБ.НомерОборудования, ОБ.Название, ОБ.Тип, ПУ.Название AS 'Участок'
	             FROM Оборудование ОБ
	             JOIN ПроизводственныеУчастки ПУ ON ОБ.УчастокID = ПУ.id
                 WHERE ОБ.Название LIKE '%{searchText}%' 
                 OR ОБ.id LIKE '%{searchText}%' 
                 OR ОБ.НомерОборудования LIKE '%{searchText}%' 
                 OR ОБ.Тип LIKE '%{searchText}%' 
                 OR ПУ.Название LIKE '%{searchText}%'";
                    break;

                case "ТехническиеОсмотры":
                    // Попытка преобразовать введенный текст в дату
                    DateTime searchDate;
                    if (DateTime.TryParseExact(searchText, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out searchDate))
                    {
                        queryString = $@"
                         SET lc_time_names = 'ru_RU';
                         SELECT ОБ.НомерОборудования, ТО.*,С.ТабельныйНомер, С.Имя, С.Должность
                         FROM ТехническиеОсмотры ТО
                         JOIN Оборудование ОБ ON ТО.ОборудованиеID = ОБ.id
                         JOIN Сотрудники С ON ТО.СотрудникID = С.id
                         WHERE DATE_FORMAT( ТО.ДатаОсмотра , '%d %M %Y' ) = '{searchDate.ToString("yyyy-MM-dd")}'";
                    }
                    else
                    {
                        // Если дата не распознана, ищем по другим полям
                        queryString = $@"
                SET lc_time_names = 'ru_RU';
                 SELECT
                 ТО.id, ТО.ОборудованиеID, DATE_FORMAT( ТО.ДатаОсмотра , '%d %M %Y' ) AS ДатаОсмотра, ТО.Результат, ТО.Причина, ТО.СотрудникID
                 FROM ТехническиеОсмотры ТО
                 JOIN Оборудование ОБ ON ТО.ОборудованиеID = ОБ.id
                 JOIN Сотрудники С ON ТО.СотрудникID = С.id
                 WHERE ОБ.НомерОборудования LIKE '%{searchText}%' 
                 OR С.ТабельныйНомер LIKE '%{searchText}%' 
                 OR С.Имя LIKE '%{searchText}%' 
                 OR CAST(ТО.id AS CHAR) LIKE '%{searchText}%' 
                 OR CAST(ТО.ОборудованиеID AS CHAR) LIKE '%{searchText}%' 
                 OR DATE_FORMAT( ТО.ДатаОсмотра , '%d %M %Y' ) LIKE '%{searchText}%' 
                 OR ТО.Результат LIKE '%{searchText}%' 
                 OR ТО.Причина LIKE '%{searchText}%' 
                 OR CAST(ТО.СотрудникID AS CHAR) LIKE '%{searchText}%'";
                    }
                    break;
                case "ОтказыОборудования":
                    queryString = $@"
                    SELECT
                    ОО.id,
                    ОБ.Название,
                    DATE_FORMAT( ОО.ДатаОтказа , '%d %M %Y' ) as 'Дата Отказа',
                    ОО.Причина,
                    С.ТабельныйНомер,
                    С.Имя,
                    С.Должность
                    FROM                  
                    ОтказыОборудования ОО
                JOIN 
                    Оборудование ОБ ON ОО.ОборудованиеID = ОБ.ID
                JOIN 
                    Сотрудники С ON ОО.СотрудникID = С.id 
                 WHERE CAST(ОО.id AS CHAR) LIKE '%{searchText}%' 
                 OR CAST(ОБ.Название AS CHAR) LIKE '%{searchText}%' 
                 OR CAST(ДатаОтказа AS CHAR) LIKE '%{searchText}%' 
                 OR Причина LIKE '%{searchText}%' 
                 OR CAST(СотрудникID AS CHAR) LIKE '%{searchText}%'"; break;

                default:
                    // Для других таблиц поиск не предусмотрен
                    return;
                    
            }

            try
            {
                DataTable dt = dataBase.GetDataTable(queryString);
                MyDataGrid.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении поиска: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void MyDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string propertyName = e.PropertyName;

            if (propertyName == "УчастокID")
            {
                e.Column = CreateComboBoxColumn(
                    propertyName,
                    "Участок",
                    GetProductionAreas(),
                    "Название",
                    "id"
                );
            }
            else if (propertyName == "СотрудникID")
            {
                e.Column = CreateComboBoxColumn(
                    propertyName,
                    "Сотрудник",
                    GetEmployees(),
                    "Имя",
                    "id"
                );
            }
            else if (propertyName == "ОборудованиеID")
            {
                e.Column = CreateComboBoxColumn(
                    propertyName,
                    "Оборудование",
                    GetEquipment(),
                    "Название",
                    "id"
                );
            }
        }
        private DataGridComboBoxColumn CreateComboBoxColumn(string propertyName, string header, IEnumerable<object> items, string displayMember, string selectedValuePath)
        {
            DataGridComboBoxColumn comboBoxColumn = new DataGridComboBoxColumn();
            comboBoxColumn.Header = header;
            comboBoxColumn.ItemsSource = items;
            comboBoxColumn.SelectedValueBinding = new System.Windows.Data.Binding(propertyName);
            comboBoxColumn.DisplayMemberPath = displayMember;
            comboBoxColumn.SelectedValuePath = selectedValuePath;
            return comboBoxColumn;
        }
        private List<ProductionArea> GetProductionAreas()
        {
            List<ProductionArea> areas = new List<ProductionArea>();
            string query = "SELECT id, название FROM ПроизводственныеУчастки";
            DataTable dt = dataBase.GetDataTable(query);
            foreach (DataRow row in dt.Rows)
            {
                areas.Add(new ProductionArea
                {
                    id = Convert.ToInt32(row["id"]),
                    Название = row["Название"].ToString()
                });
            }
            return areas;
        }

        private List<Employee> GetEmployees()
        {
            List<Employee> employees = new List<Employee>();
            string query = "SELECT id, табельныйномер, имя, должность FROM Сотрудники";
            DataTable dt = dataBase.GetDataTable(query);
            foreach (DataRow row in dt.Rows)
            {
                employees.Add(new Employee
                {
                    id = Convert.ToInt32(row["id"]),
                    ТабельныйНомер = row["ТабельныйНомер"].ToString(),
                    Имя = row["Имя"].ToString(),
                    Должность = row["Должность"].ToString()
                });
            }
            return employees;
        }

        private List<Equipment> GetEquipment()
        {
            List<Equipment> equipmentList = new List<Equipment>();
            string query = "SELECT id, НомерОборудования, Название, Тип FROM Оборудование";
            DataTable dt = dataBase.GetDataTable(query);
            foreach (DataRow row in dt.Rows)
            {
                equipmentList.Add(new Equipment
                {
                    id = Convert.ToInt32(row["id"]),
                    НомерОборудования = row["НомерОборудования"].ToString(),
                    Название = row["Название"].ToString(),
                    Тип = row["Тип"].ToString()
                });
            }
            return equipmentList;
        }

        public class ProductionArea
        {
            public int id { get; set; }
            public string Название { get; set; }
        }

        public class Employee
        {
            public int id { get; set; }
            public string ТабельныйНомер { get; set; }
            public string Имя { get; set; }
            public string Должность { get; set; }
        }

        public class Equipment
        {
            public int id { get; set; }
            public string НомерОборудования { get; set; }
            public string Название { get; set; }
            public string Тип { get; set; }
        }


        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данную работу разработан Замотаев Даниил, группа 21-13 ИСк", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}