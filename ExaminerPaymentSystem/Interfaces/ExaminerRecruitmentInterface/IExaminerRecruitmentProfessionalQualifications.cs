using ExaminerPaymentSystem.Models.ExaminerRecruitment;

namespace ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface
{
    public interface IExaminerRecruitmentProfessionalQualifications
    {
        Task<List<ProfessionalQualifications>> GetByExaminerRecruitmentIdAsync(int examinerRecruitmentId);
        Task AddAsync(ProfessionalQualifications teachingExperience);
        Task UpdateAsync(ProfessionalQualifications teachingExperience);
        Task DeleteAsync(int id);
    }
}
