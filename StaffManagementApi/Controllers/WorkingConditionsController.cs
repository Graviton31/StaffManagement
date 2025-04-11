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
    public class WorkingConditionsController : ControllerBase
    {

        private readonly ContextStaffManagement _context;

        public WorkingConditionsController(ContextStaffManagement context)
        {
            _context = context;
        }

        /// <summary>
        /// Добавляет новое условие работы для работника и обновляет предыдущее условие.
        /// </summary>
        /// <param name="workingConditionDto">Данные условия работы.</param>
        /// <returns>200 OK при успехе; 400 Bad Request при ошибке.</returns>
        [HttpPost("condition/add")]
        public async Task<IActionResult> AddWorkingCondition(WorkingConditionDto workingConditionDto)
        {
            if (workingConditionDto == null)
            {
                return BadRequest("Недопустимые данные.");
            }

            // Проверка корректности дат, если EndDate указано
            if (workingConditionDto.EndDate.HasValue && workingConditionDto.EndDate <= workingConditionDto.StartDate)
            {
                return BadRequest("Дата окончания должна быть больше даты начала.");
            }

            // Создание нового условия работы
            var workingCondition = new WorkingCondition
            {
                StartDate = workingConditionDto.StartDate ?? DateOnly.FromDateTime(DateTime.Now),
                EndDate = workingConditionDto.EndDate,
                Name = workingConditionDto.Name,
                Description = workingConditionDto.Description,
                IdWorker = workingConditionDto.IdWorker
            };

            _context.WorkingConditions.Add(workingCondition);
            await _context.SaveChangesAsync();

            // Поиск и обновление предыдущего активного условия
            var previousCondition = await _context.WorkingConditions
                .Where(wc => wc.IdWorker == workingCondition.IdWorker
                            && wc.IdWorkCondition != workingCondition.IdWorkCondition
                            && (wc.EndDate == null || wc.EndDate > workingCondition.StartDate))
                .OrderByDescending(wc => wc.StartDate)
                .FirstOrDefaultAsync();

            if (previousCondition != null)
            {
                previousCondition.EndDate = workingCondition.StartDate;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        /// <summary>
        /// Обновляет дату окончания условия работы.
        /// </summary>
        /// <param name="id">ID условия работы</param>
        /// <param name="endDate">Новая дата окончания (null для текущей даты)</param>
        /// <returns>204 No Content при успехе; 404 Not Found если запись не найдена</returns>
        [HttpPut("condition/update-end-date/{id}")]
        public async Task<IActionResult> UpdateWorkingConditionEndDate(int id, DateOnly? endDate = null)
        {
            var condition = await _context.WorkingConditions.FindAsync(id);
            if (condition == null)
            {
                return NotFound();
            }

            // Обновление даты окончания
            condition.EndDate = endDate ?? DateOnly.FromDateTime(DateTime.Now);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("condition/update/{id}")]
        public async Task<IActionResult> UpdateWorkingCondition(
    int id,
    [FromBody] WorkingConditionUpdateDto updatedConditionDto)
        {
            if (updatedConditionDto == null)
                return BadRequest("Данные обновления обязательны.");

            var currentCondition = await _context.WorkingConditions.FindAsync(id);
            if (currentCondition == null)
                return NotFound();

            // Обновление только переданных полей
            currentCondition.Name = updatedConditionDto.Name ?? currentCondition.Name;
            currentCondition.Description = updatedConditionDto.Description ?? currentCondition.Description;

            // Логика обработки дат
            var newStartDate = updatedConditionDto.StartDate ?? currentCondition.StartDate;
            var newEndDate = updatedConditionDto.EndDate ?? currentCondition.EndDate;

            if (newEndDate.HasValue && newEndDate <= newStartDate)
                return BadRequest("Некорректный временной интервал.");

            // Обновление связанных записей только при изменении дат
            if (newStartDate != currentCondition.StartDate || newEndDate != currentCondition.EndDate)
            {
                var originalStart = currentCondition.StartDate;
                var originalEnd = currentCondition.EndDate;

                currentCondition.StartDate = newStartDate;
                currentCondition.EndDate = newEndDate;

                if (newStartDate != originalStart)
                {
                    var previousCondition = await _context.WorkingConditions
                        .Where(wc => wc.IdWorker == currentCondition.IdWorker
                                    && wc.EndDate == originalStart)
                        .FirstOrDefaultAsync();

                    if (previousCondition != null)
                        previousCondition.EndDate = newStartDate;
                }

                if (newEndDate != originalEnd)
                {
                    var nextCondition = await _context.WorkingConditions
                        .Where(wc => wc.IdWorker == currentCondition.IdWorker
                                    && wc.StartDate == originalEnd)
                        .FirstOrDefaultAsync();

                    if (nextCondition != null)
                        nextCondition.StartDate = newEndDate.Value;
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

