using ExaminerPaymentSystem.Controllers.Examiners;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IMarksCapturedRepository
    {
        Task UpdateMarksCaptured(MarksCaptured capturedMarks);

        Task<MarksCaptured> GetComponentMarkCaptured(string examCode, string subjectCode, string paperCode, string regionCode);

        Task<IEnumerable<MarksCaptured>> GetAllMarksCaptured();
        Task<MarksCaptured> GetMarkCapturedByParameters(string examcode, string subjectcode, string papercode);
        

        Task<IEnumerable<MarksCaptured>> GetMarkCapturedGrade7List(string examcode, string subjectcode);
        Task UpdateExaminerScripts(MarksCaptured markscaptured);

        Task UpdateExaminerScriptsMarked(RootObjectMarksCaptured rootObject);

        Task<List<MarksCaptured>> CheckExaminerTransactions(string examinerCode, string subjectCode, string paperCode, string searchBmsCode);



        Task<OperationResult> InsertExaminerTransactions(MarksCaptured rootObject, ApplicationUser applicationUser);

        Task<IEnumerable<MarksCaptured>> GetMarksCaptured(string ExaminerCode, string subjectCode, string paperCode);
    }
}
