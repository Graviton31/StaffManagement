using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.Globalization;
using StaffManagementApi.Interfaces;
using StaffManagementApi.Models;

public class ExcelReportService : IExcelReportService
{
    public byte[] GenerateWorkersReport(IEnumerable<VwWorkerDetail> workers)
    {
        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Сотрудники");

        // Стиль для заголовков
        var headerStyle = workbook.CreateCellStyle();
        var headerFont = workbook.CreateFont();
        headerFont.IsBold = true;
        headerStyle.SetFont(headerFont);
        headerStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
        headerStyle.FillPattern = FillPattern.SolidForeground;
        headerStyle.Alignment = HorizontalAlignment.Center;

        // Стиль для дат
        var dateStyle = workbook.CreateCellStyle();
        var dateFormat = workbook.CreateDataFormat();
        dateStyle.DataFormat = dateFormat.GetFormat("dd.MM.yyyy");

        // Новые заголовки
        var headerRow = sheet.CreateRow(0);
        string[] headers =
        {
            "ФИО",
            "Дата рождения",
            "Почта",
            "Телефон",
            "Должность",
            "Отдел",
            "Оклад",
            "Дата начала статуса",
            "Дата окончания статуса",
            "Номер ПК",
            "Тип особых условий",
            "Начало особых условий",
            "Окончание особых условий"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = headerRow.CreateCell(i);
            cell.SetCellValue(headers[i]);
            cell.CellStyle = headerStyle;
        }

        // Заполнение данных
        int rowNumber = 1;
        foreach (var worker in workers)
        {
            var row = sheet.CreateRow(rowNumber++);

            row.CreateCell(0).SetCellValue(worker.FullWorkerName);
            SetDateCell(row, 1, worker.BirthDate, dateStyle);
            row.CreateCell(2).SetCellValue(worker.WorkEmail);
            row.CreateCell(3).SetCellValue(worker.Phone);
            row.CreateCell(4).SetCellValue(worker.Post);
            row.CreateCell(5).SetCellValue(worker.Department);
            row.CreateCell(6).SetCellValue(worker.Salary ?? 0);
            SetDateCell(row, 7, worker.StartDateStatus, dateStyle);
            SetDateCell(row, 8, worker.EndDateStatus, dateStyle);    
            row.CreateCell(9).SetCellValue(worker.PcNumber);
            row.CreateCell(10).SetCellValue(worker.Name);
            SetDateCell(row, 11, worker.StartDateWorkingConditions, dateStyle);
            SetDateCell(row, 12, worker.EndDateWorkingConditions, dateStyle);
        }

        // Автоподбор ширины столбцов
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.AutoSizeColumn(i);
            // Добавляем небольшой отступ
            sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + 1024);
        }

        using (var stream = new MemoryStream())
        {
            workbook.Write(stream);
            return stream.ToArray();
        }
    }

    private void SetDateCell(IRow row, int column, DateOnly? date, ICellStyle style)
    {
        var cell = row.CreateCell(column);
        if (date.HasValue)
        {
            cell.SetCellValue(date.Value.ToDateTime(TimeOnly.MinValue));
            cell.CellStyle = style;
        }
    }
}