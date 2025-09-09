using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Models.ExamMonitors.Dtos;
using ExaminerPaymentSystem.Repositories.Common;

namespace ExaminerPaymentSystem.Interfaces.ExamMonitors
{
    public interface IExamMonitorRepository : IBaseRepository<ExamMonitor>
    {
        Task<IEnumerable<ExamMonitor>> GetByRegionAsync(string region);
        Task<IEnumerable<ExamMonitor>> GetByCentreAsync(string centre);

        Task<ExamMonitor> GetMonitorByRecordAsync(string nationalId);
    }
}
