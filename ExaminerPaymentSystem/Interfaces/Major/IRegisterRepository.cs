using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;

namespace ExaminerPaymentSystem.Interfaces.Major
{
    public interface IRegisterRepository
    {
        //filter
       

        Task<ExaminerScriptsMarked> GetExaminer(string subKey);
        Task<IEnumerable<ExaminerScriptsMarked>> GetAllRegister();
        Task<ExaminerScriptsMarked> CheckExaminerRegister(string subKey);
        Task<IEnumerable<ExaminerScriptsMarked>> GetComponentRegister(string examCode, string subjectCode, string paperCode, string activity, string regionCode);

        Task<IEnumerable<ExaminerScriptsMarked>> GetComponentRegister2(string examCode, string subjectCode, string paperCode, string activity, string regionCode, string venue);

        //update
        //Task UpdateRegister(ExaminerScriptsMarked register, string userId);

        Task<OperationResult> ConfirmRegister(ExaminerScriptsMarked register,string userId);
        //Task UpdateRegisterList(List<ExaminerScriptsMarked> registers,ApplicationUser applicationUser);

        Task MarkPresent(ExaminerScriptsMarked register,string userid);






        //delete

        //Task DeleteRegister(string IdNumber,string userId);

    }
}
