using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface ICategoryCheckInCheckOutRepository
    {
        Task<List<CategoryCheckInCheckOut>> GetAllAsync();
        Task<CategoryCheckInCheckOut?> GetByIdAsync(int id);
        Task AddAsync(CategoryCheckInCheckOut categoryCheckInCheckOut);
        Task UpdateAsync(CategoryCheckInCheckOut categoryCheckInCheckOut);
        Task DeleteAsync(int id);
    }
}
