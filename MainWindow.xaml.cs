using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
namespace Kursovaya2
{
    public partial class MainWindow : Window
    {
        private string currentUserLogin;

        DataBase dataBase = new DataBase();
        // Новый конструктор, который принимает логин
        public MainWindow(string login)
        {
            InitializeComponent();
            currentUserLogin = login;
            // Устанавливаем логин в Label
            LabelCurrentUser.Content = currentUserLogin;
            ComboBoxData_Loaded(null, null);
        }
        // Существующий конструктор без параметров (можно удалить, если он не нужен)
        public MainWindow()
        {
            InitializeComponent();
            ComboBoxData_Loaded(null, null);
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
                SELECT 
                    ТО.id,
                    ОБ.НомерОборудования,
                    ТО.ДатаОсмотра,
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
                SELECT 
                    ОО.id,
                    ОБ.НомерОборудования,
                    ОО.ДатаОтказа,
                    ОО.Причина,
                    С.ТабельныйНомер,
                    С.Имя,
                    С.Должность
                FROM 
                    ОтказыОборудования ОО
                JOIN 
                    Оборудование ОБ ON ОО.ОборудованиеID = ОБ.id
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



        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
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
                case "сотрудники":
                    queryString = $@"SELECT * FROM сотрудники WHERE имя LIKE '%{searchText}%'";
                    break;

                case "производственныеучастки":
                    queryString = $@"SELECT * FROM производственныеучастки WHERE название LIKE '%{searchText}%'";
                    break;

                case "оборудование":
                    queryString = $@"SELECT * FROM оборудование WHERE название LIKE '%{searchText}%'";
                    break;

                case "ТехническиеОсмотры":
                    // Попытка преобразовать введенный текст в дату
                    DateTime searchDate;
                    if (DateTime.TryParseExact(searchText, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out searchDate))
                    {
                        queryString = $@"SELECT ТО.*, ОБ.НомерОборудования, С.ТабельныйНомер, С.Имя, С.Должность
                                 FROM ТехническиеОсмотры ТО
                                 JOIN Оборудование ОБ ON ТО.ОборудованиеID = ОБ.id
                                 JOIN Сотрудники С ON ТО.СотрудникID = С.id
                                 WHERE ТО.ДатаОсмотра = '{searchDate.ToString("yyyy-MM-dd")}'";
                    }
                    else
                    {
                        // Если дата не распознана, ищем по другим полям
                        queryString = $@"SELECT ТО.*, ОБ.НомерОборудования, С.ТабельныйНомер, С.Имя, С.Должность
                                 FROM ТехническиеОсмотры ТО
                                 JOIN Оборудование ОБ ON ТО.ОборудованиеID = ОБ.id
                                 JOIN Сотрудники С ON ТО.СотрудникID = С.id
                                 WHERE ОБ.НомерОборудования LIKE '%{searchText}%' OR С.ТабельныйНомер LIKE '%{searchText}%' OR С.Имя LIKE '%{searchText}%'";
                    }
                    break;

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
            string query = "SELECT id, название FROM производственныеучастки";
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
            string query = "SELECT id, табельныйномер, имя, должность FROM сотрудники";
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
            string query = "SELECT id, номероборудования, название, тип FROM оборудование";
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