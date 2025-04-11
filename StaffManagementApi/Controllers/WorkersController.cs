using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using StaffManagementApi.Data;
using StaffManagementApi.Models;
using StaffManagementApi.ModelsDto;

namespace StaffManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkersController : ControllerBase
    {
        private readonly ContextStaffManagement _context;

        public WorkersController(ContextStaffManagement context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает данные работников с пагинацией
        /// </summary>
        /// <param name="pageNumber">Номер страницы (начиная с 1)</param>
        /// <param name="pageSize">Количество элементов на странице (по умолчанию 10)</param>
        /// <returns>Пагинированный список работников</returns>
        // GET: api/Workers
        [HttpGet]
        public async Task<ActionResult<PagedResponse<VwWorkerInfo>>> GetWorkers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            // Валидация параметров
            if (pageNumber < 1)
                return BadRequest("Номер страницы должен быть больше 0");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Размер страницы должен быть между 1 и 100");

            // Получаем общее количество записей
            var totalRecords = await _context.VwWorkerInfos.CountAsync();

            // Вычисляем смещение
            var skip = (pageNumber - 1) * pageSize;

            // Получаем данные с пагинацией
            var workers = await _context.VwWorkerInfos
                .OrderBy(w => w.FullWorkerName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Создаем ответ
            var response = new PagedResponse<VwWorkerInfo>
            {
                Data = workers,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };

            return Ok(response);
        }

        [HttpGet("birth_date")]
        public async Task<ActionResult<IEnumerable<WorkerBirthDateDto>>> GetWorkersBirthDate()
        {
            // Получаем список работников
            var workers = await _context.VwWorkerInfos
                                        .ToListAsync();

            // Создаем новый список с информацией о дате рождения
            var workersWithBirthDate = workers.Select(w => new WorkerBirthDateDto
            {
                Id = w.IdWorker,
                Avatar = w.Avatar,
                FullWorkerName = w.FullWorkerName,
                // Рассчитываем дату и день недели дня рождения в этом году или следующем
                BirthDayMonthAndDay = GetBirthDayMonthAndDay(w.BirthDate),
                DayOfWeekThisYear = GetDayOfWeekThisYear(w.BirthDate),
                NextBirthDay = GetBirthDayThisYear(w.BirthDate) // Добавляем дату следующего дня рождения для сортировки
            })
            // Сортируем результат по дате следующего дня рождения
            .OrderBy(w => w.NextBirthDay)
            .ToList();

            return workersWithBirthDate;
        }

        // Вспомогательный метод для получения следующего дня рождения
        private DateTime GetBirthDayThisYear(DateOnly birthDate)
        {
            int currentYear = DateTime.Now.Year;
            DateTime birthDayThisYear = new DateTime(currentYear, birthDate.Month, birthDate.Day);

            Console.WriteLine($"Current Date: {DateTime.Now.Date}, BirthDay This Year: {birthDayThisYear}");

            if (birthDayThisYear >= DateTime.Now.Date)
            {
                return birthDayThisYear;
            }
            else
            {
                return new DateTime(currentYear + 1, birthDate.Month, birthDate.Day);
            }
        }

        private string GetDayOfWeekThisYear(DateOnly birthDate)
        {
            DateTime birthDayThisYear = GetBirthDayThisYear(birthDate);
            return birthDayThisYear.ToString("dddd");
        }

        private string GetBirthDayMonthAndDay(DateOnly birthDate)
        {
            DateTime birthDayThisYear = GetBirthDayThisYear(birthDate);
            if (birthDayThisYear.Year == DateTime.Now.Year)
            {
                return birthDayThisYear.ToString("MM.dd");
            }
            else
            {
                return birthDayThisYear.ToString("yyyy.MM.dd");
            }
        }

        /// <summary>
        /// Получает данные конкретного работника по id.
        /// </summary>
        /// <param name="id">id работника для получения.</param>
        /// <returns>Объект <see cref="WorkerDetail"/>, если найден; иначе 404 Not Found.</returns>
        // GET: api/Workers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VwWorkerDetail>> GetWorker(int id)
        {
            var workerDetail = await _context.VwWorkerDetails
                .SingleOrDefaultAsync(w => w.IdWorker == id); // Выполняем запрос и получаем единственный объект

            if (workerDetail == null)
            {
                return NotFound(); // Возвращаем 404, если рабочий не найден
            }

            return Ok(workerDetail); // Возвращаем 200 и детали рабочего
        }

        /// <summary>
        /// Обновляет данные конкретного работника.
        /// </summary>
        /// <param name="id">ID работника для обновления.</param>
        /// <param name="workerDto">Обновленные данные работника.</param>
        /// <returns>204 No Content, если успешно; иначе 400 Bad Request или 404 Not Found.</returns>
        //[Authorize(Roles = "Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateWorker(int id, WorkerDto workerDto)
        {
            // Проверка на валидность входных данных
            if (workerDto == null)
            {
                return BadRequest("Данные работника обязательны.");
            }

            // Поиск работника по ID
            var worker = await _context.Workers.FindAsync(id);
            if (worker == null)
            {
                return NotFound(); // Возврат 404, если работник не найден
            }

            // Обновление данных работника
            worker.Name = workerDto.Name ?? worker.Name;
            worker.Surname = workerDto.Surname ?? worker.Surname;
            worker.Patronymic = workerDto.Patronymic; // Устанавливаем значение, даже если оно null
            worker.BirthDate = workerDto.BirthDate ?? worker.BirthDate; // Обновляем дату рождения
            worker.WorkEmail = workerDto.WorkEmail; // Обновляем рабочий email
            worker.Phone = workerDto.Phone; // Обновляем телефон
            worker.PcNumber = workerDto.PcNumber; // Обновляем номер ПК

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Добавляет нового работника.
        /// </summary>
        /// <param name="workerDto">Данные работника для добавления.</param>
        /// <returns>200 OK, если успешно; иначе 400 Bad Request.</returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddWorker([FromBody] WorkerDto workerDto)
        {
            // Проверка на валидность модели
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Создаем новый объект Worker из DTO
            var worker = new Worker
            {
                Name = workerDto.Name,
                Surname = workerDto.Surname,
                Patronymic = workerDto.Patronymic,
                BirthDate = workerDto.BirthDate.Value, // Обновляем дату рождения
                IdRole = workerDto.IdRole,
                WorkEmail = workerDto.WorkEmail, // Обновляем рабочий email
                Phone = workerDto.Phone, // Обновляем телефон
                PcNumber = workerDto.PcNumber // Обновляем номер ПК
            };

            await _context.Workers.AddAsync(worker);
            await _context.SaveChangesAsync();

            return Ok(new { worker.IdWorker }); // Возвращаем 200 OK после успешного добавления
        }

        /// <summary>
        /// Удаляет конкретного работника по ID (мягкое удаление).
        /// </summary>
        /// <param name="id">ID работника для удаления.</param>
        /// <returns>204 No Content, если успешно; иначе 404 Not Found.</returns>
        // DELETE: api/Workers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker == null)
            {
                return NotFound(); // Возвращаем 404, если работник не найден
            }

            // Устанавливаем статус IsDeleted на 1 для мягкого удаления
            worker.IsDeleted = 1;
            await _context.SaveChangesAsync();

            return NoContent(); // Возвращаем 204 No Content после успешного удаления
        }

        [HttpPost("upload-avatar/{workerId}")]
        public async Task<IActionResult> UploadAvatar(int workerId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не выбран");
            }

            if (!file.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                !file.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                !file.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Допустимы только форматы .jpg, .png, .jpeg");
            }

            // Проверка существования работника
            var worker = await _context.Workers.FindAsync(workerId);
            if (worker == null)
            {
                return NotFound("Работник не найден");
            }

            // Генерация уникального имени файла
            var fileName = $"{workerId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine("wwwroot/images", fileName);

            // Сохранение файла
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Обновление пути к аватарке в базе данных
            worker.Avatar = $"{fileName}";
            await _context.SaveChangesAsync();

            return Ok("Аватарка успешно загружена");
        }
    }
}