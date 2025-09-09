using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;

namespace ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface
{
    public interface IExaminerRecruitmentRegisterRepository
    {

        Task<ExaminerRecruitmentRegister> GetByIdAsync(int id);
        Task<IEnumerable<ExaminerRecruitmentRegister>> GetAllAsync();
        Task<(List<ExaminerRecruimentDataTableViewModel> Data, int TotalCount, int TotalGradeA, int TotalGradeB, int TotalGradeC, int TotalGradeD,int  totalPending)> GetAllPresentAsync(
            int skip,
            int take,
            string searchValue,
            string sortColumn,
            string sortDirection,
            string sessionLevelFilter,
            string subjectFilter,
            string paperCodeFilter,
            string regionCodeFilter);


        Task<List<ExaminerRecruitmentAssesmentPresentWithoutGrade>> GetPresentExaminersWithoutGradesAsync(
            string sessionLevelFilter,
            string subjectFilter,
            string paperCodeFilter,
            string regionCodeFilter);

        Task<(List<ExaminerRecruimentDataTableViewModel> Data, int TotalCount, int TotalGradeA, int TotalGradeB, int TotalGradeC, int TotalGradeD, int totalPending)> GetAllPresentForAssesmentAsync
            (int skip, int take, string searchValue, string sortColumn, string sortDirection,
             string experience, string subject, string paperCode, string gradeField);

        Task AddAsync(ExaminerRecruitmentRegister entity);
        Task UpdateAsync(ExaminerRecruitmentRegister entity);
        Task<ExaminerRecruitmentRegister> FindByExaminerIdAsync(int ExaminerRecruitmentId);
        Task DeleteAsync(int id);
        Task SaveAsync();

        Task<List<ExaminerRecruitmentAndSelectionViewModel>> LoadByParametersRegister(
          string subject,
          string paperCode,
          string experience);

    }
}
