using ExaminerPaymentSystem.Models.Common;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IMaxExaminerCodeRepository
    {
        Task<LastNumberDatabase> SaveMaxExaminerCode(LastNumberDatabase model);
        Task<LastNumberDatabase> UpdateMaxExaminerCode(LastNumberDatabase updatedBankData);
        Task<IEnumerable<LastNumberDatabase>> GetAllMaxExaminerCodes();
        Task<LastNumberDatabase> GetMaxExaminerCodeById(int id);

        Task<string> GetMaxExaminerCodeFromDatabase();
    }
}
