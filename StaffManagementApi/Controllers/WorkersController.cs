using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using StaffManagementApi.Data;
using StaffManagementApi.Models;
using StaffManagement.ModelsDto;

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
        /// Получает данные всех работников.
        /// </summary>
        /// <returns>Список объектов <see cref="VwWorkerInfos"/>.</returns>
        // GET: api/Workers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VwWorkerInfo>>> GetWorkers()
        {
            // Получаем отсортированный список работников
            var workers = await _context.VwWorkerInfos
                                         .OrderBy(w => w.FullWorkerName) 
                                         .ToListAsync(); 

            return workers; // Возвращаем список работников
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

        // Остальные методы остаются без изменений
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
            worker.Name =  workerDto.Name ?? worker.Name;
            worker.Surname =  workerDto.Surname ?? worker.Surname;
            worker.Patronymic = workerDto.Patronymic; // Устанавливаем значение, даже если оно null
            worker.BirthDate = workerDto.BirthDate ?? worker.BirthDate; // Обновляем дату рождения
            worker.WorkEmail = workerDto.WorkEmail; // Обновляем рабочий email
            worker.Phone = workerDto.Phone; // Обновляем телефон
            worker.PcNumber = workerDto.PcNumber; // Обновляем номер ПК

            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// <summary>
        /// Добавляет статус для конкретного работника и обновляет время окончания предыдущего статуса.
        /// </summary>
        /// <param name="statusWorkerDto">Данные статуса для добавления.</param>
        /// <returns>200 OK, если успешно; иначе 400 Bad Request.</returns>
        [HttpPost("status/add")]
        public async Task<IActionResult> AddStatusWorker(StatusWorkerDto statusWorkerDto)
        {
            // Проверка на валидность входных данных
            if (statusWorkerDto == null)
            {
                return BadRequest("Недопустимые данные.");
            }

            // Проверка на корректность дат
            if (statusWorkerDto.EndDate <= statusWorkerDto.StartDate)
            {
                return BadRequest("Дата окончания должна быть больше даты начала.");
            }

            // Создание нового статуса рабочего
            var statusWorker = new StatusesWorker
            {
                StartDate = statusWorkerDto.StartDate ?? DateOnly.FromDateTime(DateTime.Now),
                EndDate = statusWorkerDto.EndDate,
                IdPost = statusWorkerDto.IdPost.Value,
                IdDepartment = statusWorkerDto.IdDepartment.Value,
                IdWorker = statusWorkerDto.IdWorker,
                Duties = statusWorkerDto.Duties,
                Salary = statusWorkerDto.Salary,
            };

            _context.StatusesWorkers.Add(statusWorker);
            await _context.SaveChangesAsync();

            // Проверка на существование статуса рабочего
            var status = await _context.StatusesWorkers.FindAsync(statusWorkerDto.IdStatusWorker);
            if (status != null)
            {
                // Обновление даты окончания предыдущего статуса
                await UpdateEndDate(statusWorkerDto.IdStatusWorker, statusWorker.StartDate);
            }

            return Ok();
        }

        /// <summary>
        /// Обновляет дату окончания статуса конкретного работника.
        /// </summary>
        /// <param name="id">ID статуса для обновления.</param>
        /// <param name="endDate">Новая дата окончания; если null, будет использована текущая дата.</param>
        /// <returns>204 No Content, если успешно; иначе 404 Not Found.</returns>
        [HttpPut("update-end-date/{id}")]
        public async Task<IActionResult> UpdateEndDate(int id, DateOnly? endDate = null)
        {
            // Поиск статуса рабочего по ID
            var statusWorker = await _context.StatusesWorkers.FindAsync(id);
            if (statusWorker == null)
            {
                return NotFound();
            }

            // Обновление даты окончания статуса, если она не указана, устанавливается текущая дата
            statusWorker.EndDate = endDate ?? DateOnly.FromDateTime(DateTime.Now);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Обновляет статус конкретного работника.
        /// </summary>
        /// <param name="id">ID статуса для обновления.</param>
        /// <param name="updatedStatusDto">Обновленные данные статуса.</param>
        /// <returns>204 No Content, если успешно; иначе 400 Bad Request или 404 Not Found.</returns>
        [HttpPut("status/update/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusWorkerDto updatedStatusDto)
        {
            // Проверка на валидность входных данных
            if (updatedStatusDto == null)
            {
                return BadRequest("Данные обновленного статуса обязательны.");
            }

            // Поиск текущего статуса по ID
            var currentStatus = await _context.StatusesWorkers.FindAsync(id);
            if (currentStatus == null)
            {
                return NotFound();
            }

            // Проверка на корректность дат
            if (updatedStatusDto.EndDate <= updatedStatusDto.StartDate)
            {
                return BadRequest("Дата окончания должна быть больше даты начала.");
            }

            // Обновление даты окончания предыдущего статуса, если дата начала нового статуса отличается
            if (updatedStatusDto.StartDate != null && currentStatus.StartDate != updatedStatusDto.StartDate)
            {
                var previousStatus = await _context.StatusesWorkers
                    .Where(s => s.IdWorker == currentStatus.IdWorker && (s.EndDate == null || s.EndDate == currentStatus.StartDate))
                    .FirstOrDefaultAsync(s => s.StartDate < currentStatus.StartDate);

                if (previousStatus != null)
                {
                    previousStatus.EndDate = updatedStatusDto.StartDate; // Обновление даты окончания предыдущего статуса
                }
            }

            // Обновление даты начала следующего статуса, если дата окончания текущего статуса отличается
            if (updatedStatusDto.EndDate != null && currentStatus.EndDate != updatedStatusDto.EndDate)
            {
                var nextStatus = await _context.StatusesWorkers
                    .Where(s => s.IdWorker == currentStatus.IdWorker && s.StartDate > currentStatus.StartDate)
                    .FirstOrDefaultAsync();

                if (nextStatus != null)
                {
                    nextStatus.StartDate = updatedStatusDto.EndDate.Value; // Обновление даты начала следующего статуса
                }
            }

            // Обновление текущего статуса новыми значениями или оставление существующих
            currentStatus.StartDate = updatedStatusDto.StartDate ?? currentStatus.StartDate;
            currentStatus.EndDate = updatedStatusDto.EndDate ?? currentStatus.EndDate;
            currentStatus.IdPost = updatedStatusDto.IdPost ?? currentStatus.IdPost;
            currentStatus.IdDepartment = updatedStatusDto.IdDepartment ?? currentStatus.IdDepartment;
            currentStatus.Duties = updatedStatusDto.Duties ?? currentStatus.Duties;
            currentStatus.Salary = updatedStatusDto.Salary ?? currentStatus.Salary;

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