using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IMaterialTransactionRepository
    {
        Task<List<MaterialTransaction>> GetAllAsync();
        Task<MaterialTransaction?> GetByIdAsync(int id);
        Task AddAsync(MaterialTransaction materialTransaction);
        Task UpdateAsync(MaterialTransaction materialTransaction);
        Task DeleteAsync(int id);
    }
}
