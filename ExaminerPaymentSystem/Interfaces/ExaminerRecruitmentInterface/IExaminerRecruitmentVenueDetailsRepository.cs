using ExaminerPaymentSystem.Models.ExaminerRecruitment;

namespace ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface
{
    public interface IExaminerRecruitmentVenueDetailsRepository
    {
        Task<ExaminerRecruitmentVenueDetails> GetByIdAsync(int id);
        Task<IEnumerable<ExaminerRecruitmentVenueDetails>> GetAllAsync();
        Task AddAsync(ExaminerRecruitmentVenueDetails venueDetails);
        Task UpdateAsync(ExaminerRecruitmentVenueDetails venueDetails);
        Task<(IEnumerable<ExaminerRecruitmentVenueDetails> data, int totalRecords, int filteredRecords)>
        GetAllDataTableAsync(int start, int length, string searchValue, string sortColumn, string sortDirection);
        Task DeleteAsync(int id);
    }
}
