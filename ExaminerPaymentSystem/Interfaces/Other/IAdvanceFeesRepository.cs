using ExaminerPaymentSystem.Models.Major;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IAdvanceFeesRepository
    {
        Task AddAdvanceFees(TandSAdvanceFees fees);
        Task UpdateAdvanceFees(TandSAdvanceFees fees, string userId);
        Task DeleteAdvanceFees(int id);
        Task<TandSAdvanceFees> GetAdvanceFees();

        Task<TandSAdvanceFees> GetFirstAdvanceFee();


    }
}
