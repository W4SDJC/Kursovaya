using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace Kursovaya2
{
    public partial class AddEditWindow : Window
    {
        DataBase dataBase = new DataBase();
        string selectedTable;
        DataRowView editRow;
        Dictionary<string, UIElement> fields = new Dictionary<string, UIElement>();

        // Define foreign key relationships
        private Dictionary<string, string> foreignKeys = new Dictionary<string, string>()
        {
            { "УчастокID", "ПроизводственныеУчастки" },
            { "ОборудованиеID", "Оборудование" },
            { "СотрудникID", "Сотрудники" }
            // Add more foreign keys as needed
        };

        public AddEditWindow(string tableName, DataRowView row)
        {
            InitializeComponent();
            selectedTable = tableName;
            editRow = row;

            if (editRow != null)
            {
                // Заполнение полей для редактирования
                LoadDataForEdit();
            }
            else
            {
                // Заполнение полей для добавления
                LoadDataForAdd();
            }
        }

        private void LoadDataForEdit()
        {
            DataTable schemaTable = dataBase.GetSchemaTable(selectedTable);
            foreach (DataRow column in schemaTable.Rows)
            {
                string columnName = column["COLUMN_NAME"].ToString();
                string dataType = column["DATA_TYPE"].ToString();
                bool isNullable = column["IS_NULLABLE"].ToString() == "YES";
                bool isAutoIncrement = column["EXTRA"].ToString().Contains("auto_increment");

                if (isAutoIncrement)
                    continue;

                StackPanel horizontalPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                Label label = new Label
                {
                    Content = $"Введите значение для '{columnName}'",
                    FontSize = 12,
                    Width = 300 // Увеличиваем ширину для подписи
                };
                horizontalPanel.Children.Add(label);

                UIElement control = CreateControl(columnName, dataType, isNullable, editRow[columnName].ToString());

                // Add to fields dictionary
                fields[columnName] = control;

                // Добавляем отступ слева для корректного выравнивания
                if (control is Control inputControl)
                {
                    inputControl.Margin = new Thickness(33, 0, 0, 0);
                }

                horizontalPanel.Children.Add(control);
                InputContainer.Children.Add(horizontalPanel);
            }
        }

        private void LoadDataForAdd()
        {
            DataTable schemaTable = dataBase.GetSchemaTable(selectedTable);
            foreach (DataRow column in schemaTable.Rows)
            {
                string columnName = column["COLUMN_NAME"].ToString();
                string dataType = column["DATA_TYPE"].ToString();
                bool isNullable = column["IS_NULLABLE"].ToString() == "YES";
                bool isAutoIncrement = column["EXTRA"].ToString().Contains("auto_increment");

                if (isAutoIncrement)
                    continue;

                StackPanel horizontalPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                Label label = new Label
                {
                    Content = $"Введите значение для '{columnName}'",
                    FontSize = 12,
                    Width = 400 // Увеличиваем ширину для подписи
                };
                horizontalPanel.Children.Add(label);

                UIElement control = CreateControl(columnName, dataType, isNullable, string.Empty);

                // Add to fields dictionary
                fields[columnName] = control;

                if (control is Control inputControl)
                {
                    inputControl.Margin = new Thickness(33, 0, 0, 0);
                }

                horizontalPanel.Children.Add(control);
                InputContainer.Children.Add(horizontalPanel);
            }
        }

        private UIElement CreateControl(string columnName, string dataType, bool isNullable, string value)
        {
            if (foreignKeys.ContainsKey(columnName))
            {
                // It's a foreign key, create ComboBox
                ComboBox comboBox = new ComboBox
                {
                    Name = columnName + "ComboBox",
                    Width = 200,
                    Margin = new Thickness(0, 5, 0, 5),
                    ItemsSource = GetForeignKeyValues(foreignKeys[columnName]),
                    DisplayMemberPath = "DisplayName",
                    SelectedValuePath = "Id",
                    SelectedValue = string.IsNullOrEmpty(value) ? null : (object)Convert.ToInt32(value)
                };
                return comboBox;
            }
            else if (dataType.Equals("varchar", StringComparison.OrdinalIgnoreCase) || dataType.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                TextBox textBox = new TextBox
                {
                    Name = columnName + "TextBox",
                    Width = 200,
                    Margin = new Thickness(0, 5, 0, 5),
                    Text = value
                };
                return textBox;
            }
            else if (dataType.Equals("int", StringComparison.OrdinalIgnoreCase) || dataType.Equals("float", StringComparison.OrdinalIgnoreCase) || dataType.Equals("double", StringComparison.OrdinalIgnoreCase))
            {
                if (foreignKeys.ContainsKey(columnName))
                {
                    // It's a foreign key, create ComboBox
                    ComboBox comboBox = new ComboBox
                    {
                        Name = columnName + "ComboBox",
                        Width = 200,
                        Margin = new Thickness(0, 5, 0, 5),
                        ItemsSource = GetForeignKeyValues(foreignKeys[columnName]),
                        DisplayMemberPath = GetDisplayColumn(foreignKeys[columnName]),
                        SelectedValuePath = "id",
                        SelectedValue = value
                    };
                    return comboBox;
                }
                else
                {
                    TextBox textBox = new TextBox
                    {
                        Name = columnName + "TextBox",
                        Width = 200,
                        Margin = new Thickness(0, 5, 0, 5),
                        Text = value,
                        InputScope = new InputScope
                        {
                            Names = { new InputScopeName(InputScopeNameValue.Number) }
                        }
                    };
                    return textBox;
                }
            }
            else if (dataType.Equals("date", StringComparison.OrdinalIgnoreCase))
            {
                DatePicker datePicker = new DatePicker
                {
                    Name = columnName + "DatePicker",
                    Width = 200,
                    Margin = new Thickness(0, 5, 0, 5),
                    SelectedDate = string.IsNullOrEmpty(value) ? null : (DateTime?)DateTime.Parse(value)
                };
                return datePicker;
            }
            else
            {
                TextBox textBox = new TextBox
                {
                    Name = columnName + "TextBox",
                    Width = 200,
                    Margin = new Thickness(100, 5, 0, 5),
                    Text = value
                };
                return textBox;
            }
        }

        public class ComboBoxItem
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
        }

        private List<ComboBoxItem> GetForeignKeyValues(string tableName)
        {
            List<ComboBoxItem> items = new List<ComboBoxItem>();
            string query = $"SELECT id, {GetDisplayColumn(tableName)} AS DisplayName FROM {tableName}";
            DataTable dt = dataBase.GetDataTable(query);
            foreach (DataRow row in dt.Rows)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Id = Convert.ToInt32(row["id"]),
                    DisplayName = row["DisplayName"].ToString()
                };
                items.Add(item);
            }
            return items;
        }

        private string GetDisplayColumn(string tableName)
        {
            switch (tableName)
            {
                case "ПроизводственныеУчастки":
                    return "Название";
                case "Оборудование":
                    return "Название";
                case "Сотрудники":
                    return "Имя";
                // Add more cases for other tables as needed
                default:
                    return "id";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (editRow != null)
            {
                // Обновление данных
                UpdateData();
            }
            else
            {
                // Добавление новых данных
                AddData();
            }
            this.Close();
        }

        private void AddData()
        {
            try
            {
                // Получение списка столбцов таблицы
                DataTable schemaTable = dataBase.GetSchemaTable(selectedTable);
                List<string> columns = new List<string>();
                List<string> values = new List<string>();

                foreach (DataRow column in schemaTable.Rows)
                {
                    string columnName = column["COLUMN_NAME"].ToString();
                    string dataType = column["DATA_TYPE"].ToString();
                    bool isNullable = column["IS_NULLABLE"].ToString() == "YES";
                    bool isAutoIncrement = column["EXTRA"].ToString().Contains("auto_increment");

                    // Пропускаем автоинкрементные поля
                    if (isAutoIncrement)
                        continue;

                    // Получаем значение из соответствующего элемента управления
                    UIElement control = fields[columnName];
                    string value = GetValueFromControl(columnName);

                    // Проверяем, является ли поле обязательным
                    if (!isNullable && string.IsNullOrEmpty(value))
                    {
                        MessageBox.Show($"Поле '{columnName}' является обязательным.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    columns.Add(columnName);
                    values.Add($"'{value.Replace("'", "''")}'");
                }

                // Проверка уникальности НомерУчастка
                if (selectedTable == "ПроизводственныеУчастки")
                {
                    string номерУчастка = GetValueFromControl("НомерУчастка");
                    if (IsDuplicate("ПроизводственныеУчастки", "НомерУчастка", номерУчастка))
                    {
                        MessageBox.Show("Участок с таким номером уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Проверка уникальности НомерОборудования
                if (selectedTable == "Оборудование")
                {
                    string номерОборудования = GetValueFromControl("НомерОборудования");
                    if (IsDuplicate("Оборудование", "НомерОборудования", номерОборудования))
                    {
                        MessageBox.Show("Оборудование с таким номером уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                // Проверка уникальности Табельного номера
                if (selectedTable == "Сотрудники")
                {
                    string табельныйНомер = GetValueFromControl("ТабельныйНомер");
                    if (IsDuplicate("Сотрудники", "ТабельныйНомер", табельныйНомер))
                    {
                        MessageBox.Show("Сотрудник с таким табельным номером уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                // Проверка отказа оборудования
                if (selectedTable == "ОтказыОборудования")
                {
                    string оборудованиеID = GetValueFromControl("ОборудованиеID");
                    if (IsDuplicateFailure(оборудованиеID))
                    {
                        MessageBox.Show("Оборудование уже имеет активный отказ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Создание SQL-запроса для вставки данных
                string insertQuery = $"INSERT INTO {selectedTable} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
                dataBase.ExecuteQuery(insertQuery);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateData()
        {
            try
            {
                // Получение списка столбцов таблицы
                DataTable schemaTable = dataBase.GetSchemaTable(selectedTable);
                List<string> setClauses = new List<string>();

                foreach (DataRow column in schemaTable.Rows)
                {
                    string columnName = column["COLUMN_NAME"].ToString();
                    string dataType = column["DATA_TYPE"].ToString();
                    bool isNullable = column["IS_NULLABLE"].ToString() == "YES";
                    bool isAutoIncrement = column["EXTRA"].ToString().Contains("auto_increment");

                    // Пропускаем автоинкрементные поля
                    if (isAutoIncrement)
                        continue;

                    // Получаем значение из соответствующего элемента управления
                    UIElement control = fields[columnName];
                    string value = string.Empty;

                    if (control is TextBox textBox)
                    {
                        value = textBox.Text.Trim();
                    }
                    else if (control is ComboBox comboBox)
                    {
                        if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                        {
                            value = selectedItem.Id.ToString();
                        }
                        else
                        {
                            value = string.Empty;
                        }
                    }
                    else if (control is DatePicker datePicker)
                    {
                        if (datePicker.SelectedDate.HasValue)
                        {
                            value = datePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            value = string.Empty;
                        }
                    }

                    // Проверяем, является ли поле обязательным
                    if (!isNullable && string.IsNullOrEmpty(value))
                    {
                        MessageBox.Show($"Поле '{columnName}' является обязательным.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    setClauses.Add($"{columnName} = '{value.Replace("'", "''")}'");
                }

                // Проверка уникальности НомерУчастка
                if (selectedTable == "ПроизводственныеУчастки")
                {
                    string номерУчастка = GetValueFromControl("НомерУчастка");
                    if (IsDuplicate("ПроизводственныеУчастки", "НомерУчастка", номерУчастка))
                    {
                        MessageBox.Show("Участок с таким номером уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Проверка уникальности НомерОборудования
                if (selectedTable == "Оборудование")
                {
                    string номерОборудования = GetValueFromControl("НомерОборудования");
                    if (IsDuplicate("Оборудование", "НомерОборудования", номерОборудования))
                    {
                        MessageBox.Show("Оборудование с таким номером уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Создание SQL-запроса для обновления данных
                string setClause = string.Join(", ", setClauses);
                string updateQuery = $"UPDATE {selectedTable} SET {setClause} WHERE id = {editRow["id"]}";
                dataBase.ExecuteQuery(updateQuery);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetValueFromControl(string columnName)
        {
            if (fields.ContainsKey(columnName))
            {
                UIElement control = fields[columnName];
                string value = string.Empty;

                if (control is TextBox textBox)
                {
                    value = textBox.Text.Trim();
                }
                else if (control is ComboBox comboBox)
                {
                    if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        value = selectedItem.Id.ToString();
                    }
                    else
                    {
                        value = string.Empty;
                    }
                }
                else if (control is DatePicker datePicker)
                {
                    if (datePicker.SelectedDate.HasValue)
                    {
                        value = datePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        value = string.Empty;
                    }
                }

                return value;
            }
            else
            {
                // Логирование или обработка ошибки
                MessageBox.Show($"Ключ '{columnName}' не найден в словаре fields.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        private bool IsDuplicate(string tableName, string columnName, string value)
        {
            string query = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = '{value.Replace("'", "''")}'";
            DataTable dt = dataBase.GetDataTable(query);
            int count = Convert.ToInt32(dt.Rows[0][0]);
            return count > 0;
        }

        private bool IsDuplicateFailure(string equipmentID)
        {
            string query = $"SELECT COUNT(*) FROM ОтказыОборудования WHERE ОборудованиеID = {equipmentID} AND ДатаОтказа IS NOT NULL";
            DataTable dt = dataBase.GetDataTable(query);
            int count = Convert.ToInt32(dt.Rows[0][0]);
            return count > 0;
        }
    }
}