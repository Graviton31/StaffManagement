using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffManagementApi.Data;
using StaffManagementApi.Interfaces;

namespace StaffManagementApi.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportController : ControllerBase
    {
        private readonly ContextStaffManagement _context;
        private readonly IExcelReportService _excelService;

        public ReportController(
            ContextStaffManagement context,
            IExcelReportService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        [HttpGet("workers-excel")]
        public async Task<IActionResult> ExportWorkersToExcel()
        {
            var workers = await _context.VwWorkerDetails.ToListAsync();

            var fileBytes = _excelService.GenerateWorkersReport(workers);

            var fileName = $"workers_report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes,
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       fileName);
        }
    }
}
