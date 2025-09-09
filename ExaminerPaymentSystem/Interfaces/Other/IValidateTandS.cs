using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IValidateTandS
    {
        Task<ValidateTandS> GetValidateTandSById(int Id);


        Task<ValidateTandS> SaveValidateTandS(ValidateTandS model);
        Task<bool> UpdateValidateTandS(ValidateTandS model);
        Task<IEnumerable<ValidateTandS>> ValidateTandSsGetAll();
    }
}
