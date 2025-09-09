using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;

namespace ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface
{
    public interface IExaminerRecruitmentAssessmentRepository
    {
        Task<IEnumerable<ExaminerRecruitmentAssessment>> GetAllAsync();
        Task<ExaminerRecruitmentAssessment?> GetAssessmentByExaminerIdAsync(int examinerRecruitmentId);
        Task<ExaminerRecruitmentAssessment?> GetByIdAsync(int id);
        Task AddAsync(ExaminerRecruitmentAssessment assessment);
        Task UpdateAsync(ExaminerRecruitmentAssessment assessment);
        Task<bool> SaveExaminerTraineeGradeAsync(ExaminerRecruitmentAssessment examinerGrade);
        Task<List<ExaminerRecruitmentAssessment>> GetPartiallyCapturedGradesAsync(
             string sessionLevelFilter,
             string subjectFilter,
             string paperCodeFilter,
             string regionCodeFilter,
             string reportType);
 
        Task<List<ExaminerRecruitmentAssessmentReportViewModel>> GetExaminerAssessmentReportAsync(
                string sessionLevelFilter,
                string subjectFilter ,
                string paperCodeFilter,
                string regionCodeFilter);

        Task<ExaminerRecruitmentAssessment?> GetByExaminerIdAsync(int id, bool includeRecruitment = false);
        Task DeleteAsync(int id);
    }
}
