using System;
using System.Data;
using System.Windows;
using ClosedXML.Excel;
using System.IO;

namespace Kursovaya2
{
    public partial class ReportWindow : Window
    {
        DataBase dataBase = new DataBase();

        public ReportWindow(string selectedTable)
        {
            InitializeComponent();
            LoadTables();
        }

        private void LoadTables()
        {
            string Sql = @"
        SELECT TABLE_NAME 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_SCHEMA = 'УчетОтказовОборудования' 
          AND TABLE_NAME <> 'Учетныезаписи'";

            try
            {
                DataTable dt = dataBase.GetDataTable(Sql);

                foreach (DataRow row in dt.Rows)
                {
                    ReportTableComboBox.Items.Add(row["TABLE_NAME"].ToString());
                }

                if (dt.Rows.Count > 0)
                {
                    ReportTableComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке таблиц: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedTable = ReportTableComboBox.SelectedItem.ToString().Trim();

            try
            {
                // Получение данных из выбранной таблицы
                DataTable reportData = dataBase.GetDataTable($"SELECT * FROM {selectedTable}");

                // Получение пути к рабочему столу
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                // Указываем имя файла и путь для сохранения
                string fileName = Path.Combine(desktopPath, $"{selectedTable}_Report.xlsx");

                // Создание Excel файла
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.AddWorksheet(selectedTable);

                    // Получение имен столбцов для выбранной таблицы
                    string columnsSql = $@"
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = '{selectedTable}'";

                    // Получение имен столбцов
                    DataTable columnsData = dataBase.GetDataTable(columnsSql);

                    // Добавляем заголовки (имена столбцов)
                    for (int i = 0; i < columnsData.Rows.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = columnsData.Rows[i]["COLUMN_NAME"].ToString();
                    }

                    // Добавление данных из DataTable в Excel
                    for (int i = 0; i < reportData.Rows.Count; i++)
                    {
                        for (int j = 0; j < reportData.Columns.Count; j++)
                        {
                            worksheet.Cell(i + 2, j + 1).Value = reportData.Rows[i][j].ToString();
                        }
                    }

                    // Сохранение файла на рабочем столе
                    workbook.SaveAs(fileName);
                    MessageBox.Show($"Отчет по таблице {selectedTable} успешно сохранен на рабочем столе.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при генерации отчета: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
