using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IExaminerCodesRepository
    {

        Task<IEnumerable<ExaminerCodes>> GetAllExaminerCodes();
        Task<ExaminerCodes> GetExaminerCodesById(string examinercode);
        Task<IEnumerable<string>> GetExaminerCategories();
    }
}

