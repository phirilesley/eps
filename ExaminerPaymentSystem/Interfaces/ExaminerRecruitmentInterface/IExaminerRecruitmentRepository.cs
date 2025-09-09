using System.Collections.Generic;
using System.Threading.Tasks;
using ExaminerPaymentSystem.Models;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;

namespace ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface
{
    public interface IExaminerRecruitmentRepository
    {
        Task<IEnumerable<ExaminerRecruitment>> GetAllAsync();
        Task<int> GetFilteredCountAsync(string searchValue);
        Task<int> GetTotalCountAsync();
        Task<(List<ExaminerRecruimentDataTableViewModel> Data, int TotalCount)> GetPaginatedAsync(
                     int skip,
                     int take,
                     string searchValue,
                     string sortColumn,
                     string sortDirection,
                     string sessionLevelFilter,
                     string subjectFilter,
                     string paperCodeFilter,
                     string regionCodeFilter);
        Task<ExaminerRecruitment> GetByIdAsync(int id);
        Task AddAsync(ExaminerRecruitment examinerRecruitment);
        Task UpdateAsync(ExaminerRecruitment examinerRecruitment);
        Task DeleteAsync(int id);
    }
}
