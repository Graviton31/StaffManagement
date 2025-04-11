using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffManagementApi.Data;
using StaffManagementApi.Models;
using StaffManagementApi.ModelsDto;

namespace StaffManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusesWorkersController : ControllerBase
    {
        private readonly ContextStaffManagement _context;

        public StatusesWorkersController(ContextStaffManagement context)
        {
            _context = context;
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
    }
}
