using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StaffManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ExcelImportService _importService;

        public ImportController(ExcelImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран");

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                return BadRequest("Поддерживаются только файлы Excel (.xlsx, .xls)");

            try
            {
                await _importService.ImportWorkersFromExcel(file);
                return Ok("Данные успешно импортированы");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при импорте: {ex.Message}");
            }
        }
    }
}
