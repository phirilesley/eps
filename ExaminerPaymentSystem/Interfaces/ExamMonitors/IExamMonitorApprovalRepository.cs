using ExaminerPaymentSystem.Models.ExamMonitors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExaminerPaymentSystem.Interfaces.ExamMonitors
{
    public interface IExamMonitorApprovalRepository
    {
      
        Task<List<ExamMonitorRegisterDate>> GetRegisterDatesAsync(string subKey);
        Task UpdateApprovalStatusAsync(ExamMonitorRegisterDate registerDate);

        Task<IEnumerable<ExamMonitorApprovalIndexViewModel>> GetUsersWithRegistersAsync();
        

    }
}
