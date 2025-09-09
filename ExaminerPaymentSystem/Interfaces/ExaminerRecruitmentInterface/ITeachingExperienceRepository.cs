using ExaminerPaymentSystem.Models.ExaminerRecruitment;

namespace ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface
{
    public interface ITeachingExperienceRepository
    {
        Task<List<TeachingExperience>> GetByExaminerRecruitmentIdAsync(int examinerRecruitmentId);
        Task AddAsync(TeachingExperience teachingExperience);
        Task UpdateAsync(TeachingExperience teachingExperience);
        Task DeleteAsync(int id);
    }
}
