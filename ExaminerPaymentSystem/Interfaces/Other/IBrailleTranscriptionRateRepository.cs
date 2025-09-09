using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IBrailleTranscriptionRateRepository
    {
        Task<IEnumerable<BrailleTranscriptionRate>> GetAllAsync();
        Task<BrailleTranscriptionRate> GetByIdAsync(int id);
        Task AddAsync(BrailleTranscriptionRate rate);
        Task UpdateAsync(BrailleTranscriptionRate rate);
        Task DeleteAsync(int id);
    }

}
