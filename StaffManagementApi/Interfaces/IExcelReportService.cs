using StaffManagementApi.Models;

namespace StaffManagementApi.Interfaces
{
    public interface IExcelReportService
    {
        byte[] GenerateWorkersReport(IEnumerable<VwWorkerDetail> workers);
    }
}
