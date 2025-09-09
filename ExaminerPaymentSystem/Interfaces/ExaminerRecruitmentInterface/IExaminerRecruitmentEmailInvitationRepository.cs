using ExaminerPaymentSystem.Models.ExaminerRecruitment;

namespace ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface
{
    public interface IExaminerRecruitmentEmailInvitationRepository
    {
        Task<IEnumerable<ExaminerRecruitmentEmailInvitation>> GetAllAsync();
        Task<ExaminerRecruitmentEmailInvitation> GetByIdAsync(int id);
        Task<ExaminerRecruitmentEmailInvitation> GetByExaminerRecruitmentIdAsync(int id);
    
        Task AddAsync(ExaminerRecruitmentEmailInvitation invitation);
        Task DeleteAsync(int id);
        Task BulkDeleteAsync(IEnumerable<int> ids);
    }

}
