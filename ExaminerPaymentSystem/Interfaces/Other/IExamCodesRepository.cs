using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IExamCodesRepository
    {

        Task<IEnumerable<ExamCodes>> GetAllExamCodes();
        Task<ExamCodes> GetExamCodesById(string examcode);

    }
}
