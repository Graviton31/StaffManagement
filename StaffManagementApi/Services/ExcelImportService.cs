using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Data;
using Microsoft.EntityFrameworkCore;
using StaffManagementApi.Models;
using StaffManagementApi.Data;
using System.Globalization;

public class ExcelImportService
{
    private readonly ContextStaffManagement _context;

    public ExcelImportService(ContextStaffManagement context)
    {
        _context = context;
    }

    public async Task ImportWorkersFromExcel(IFormFile file)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            IWorkbook workbook;
            using (var stream = file.OpenReadStream())
            {
                workbook = file.FileName.EndsWith(".xlsx")
                    ? new XSSFWorkbook(stream)
                    : new HSSFWorkbook(stream);
            }

            ISheet sheet = workbook.GetSheetAt(0);

            for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                // Функция для корректного чтения дат из Excel
                DateOnly ParseExcelDate(ICell cell)
            {
                if (cell == null)
                    return default;

                switch (cell.CellType)
                {
                    case CellType.Numeric:
                        if (DateUtil.IsCellDateFormatted(cell))
                        {
                            // Проверяем, что значение не null, иначе используем минимальную дату
                            return cell.DateCellValue.HasValue
                                ? DateOnly.FromDateTime(cell.DateCellValue.Value)
                                : default;
                        }
                        // Если это число, но не дата (может быть серийным номером даты)
                        return DateOnly.FromDateTime(DateUtil.GetJavaDate(cell.NumericCellValue));

                    case CellType.String:
                        if (DateTime.TryParseExact(
                            cell.StringCellValue,
                            new[] { "dd.MM.yyyy", "dd/MM/yyyy", "yyyy-MM-dd" }, // поддерживаемые форматы
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var date))
                        {
                            return DateOnly.FromDateTime(date);
                        }
                        return default;

                    default:
                        return default;
                }
            }

            // Чтение данных из Excel с правильной обработкой дат
            var workerData = new
            {
                Name = row.GetCell(0)?.ToString(),
                Surname = row.GetCell(1)?.ToString(),
                Patronymic = row.GetCell(2)?.ToString(),
                BirthDate = ParseExcelDate(row.GetCell(3)),
                WorkEmail = row.GetCell(4)?.ToString(),
                Phone = row.GetCell(5)?.ToString(),
                PcNumber = row.GetCell(6)?.ToString(),
                StartDate = ParseExcelDate(row.GetCell(7)),
                PostName = row.GetCell(8)?.ToString(),
                DepartmentName = row.GetCell(9)?.ToString(),
                Duties = row.GetCell(10)?.ToString(),
                Salary = int.TryParse(row.GetCell(11)?.ToString(), out var salary) ? salary : 0,
                WorkspaceName = row.GetCell(12)?.ToString()
            };

                // Проверка обязательных полей
                if (string.IsNullOrEmpty(workerData.Name))
                    throw new Exception($"Строка {rowIndex}: Имя не может быть пустым.");

                // Обработка Worker
                var worker = await _context.Workers
                .FirstOrDefaultAsync(w =>
                    w.Name == workerData.Name &&
                    w.Surname == workerData.Surname &&
                    w.Patronymic == workerData.Patronymic &&
                    w.BirthDate == workerData.BirthDate);

            if (worker == null)
            {
                worker = new Worker
                {
                    Name = workerData.Name,
                    Surname = workerData.Surname,
                    Patronymic = workerData.Patronymic,
                    BirthDate = workerData.BirthDate,
                    WorkEmail = workerData.WorkEmail,
                    Phone = workerData.Phone,
                    PcNumber = workerData.PcNumber,
                    IsDeleted = 0,
                };
                _context.Workers.Add(worker);
            }
            else
            {
                worker.WorkEmail = workerData.WorkEmail;
                worker.Phone = workerData.Phone;
                worker.PcNumber = workerData.PcNumber;
                _context.Workers.Update(worker);
            }

            await _context.SaveChangesAsync();

            // Обработка Post
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Name == workerData.PostName);

            if (post == null)
            {
                post = new Post
                {
                    Name = workerData.PostName,
                    Descriptions = workerData.Duties
                };
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
            }

            // Обработка Department
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Name == workerData.DepartmentName);

            if (department == null)
            {
                department = new Department
                {
                    Name = workerData.DepartmentName
                };
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
            }

            // Обработка StatusesWorker
            var status = await _context.StatusesWorkers
                .FirstOrDefaultAsync(s =>
                    s.IdWorker == worker.IdWorker &&
                    s.StartDate == workerData.StartDate);

            if (status == null)
            {
                status = new StatusesWorker
                {
                    StartDate = workerData.StartDate,
                    Duties = workerData.Duties,
                    Salary = workerData.Salary,
                    IdWorker = worker.IdWorker,
                    IdPost = post.IdPost,
                    IdDepartment = department.IdDepartment
                };
                _context.StatusesWorkers.Add(status);
            }
            else
            {
                status.Duties = workerData.Duties;
                status.Salary = workerData.Salary;
                status.IdPost = post.IdPost;
                status.IdDepartment = department.IdDepartment;
                _context.StatusesWorkers.Update(status);
            }

            // Обработка Workspace
            if (!string.IsNullOrEmpty(workerData.WorkspaceName))
            {
                var workspace = await _context.Workspaces
                    .FirstOrDefaultAsync(w => w.Name == workerData.WorkspaceName);

                if (workspace != null)
                {
                    workspace.IdWorker = worker.IdWorker;
                    _context.Workspaces.Update(workspace);
                }
                else
                {
                    // Создаем новое рабочее место с минимальными данными
                    workspace = new Workspace
                    {
                        Name = workerData.WorkspaceName,
                        IdWorker = worker.IdWorker
                    };
                    _context.Workspaces.Add(workspace);
                }
            }

            await _context.SaveChangesAsync();
        }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Ошибка импорта: " + ex.InnerException?.Message);
        }
    }
}