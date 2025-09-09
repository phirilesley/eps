using ExaminerPaymentSystem.Models.ExaminerRecruitment;

namespace ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface
{
    public interface IExaminerRecruitmentAttachmentsRepository
    {
        // Get all attachments for a specific recruitment
        Task<IEnumerable<ExaminerRecruitmentAttachements>> GetAllByRecruitmentId(int recruitmentId);

        // Get one attachment by Id
        Task<ExaminerRecruitmentAttachements> GetByIdAsync(int id);

        // Add new attachment
        Task AddAsync(ExaminerRecruitmentAttachements attachment);

        // Update existing attachment
        Task UpdateAsync(ExaminerRecruitmentAttachements attachment);

        // Remove attachment
        Task DeleteAsync(int id);
    }
}
