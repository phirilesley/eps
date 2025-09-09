using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IApportionmentRepository
    {

        Task<IEnumerable<Apportionment>> GetNumberOfCandidatesBySubjectComponent(string subjectCode, string paperCode);
        Task<IEnumerable<ExaminerScriptsMarked>> GetNumberOfScriptsMarked(string subjectCode, string paperCode);

    }
}
