using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface ISubjectsRepository
    {
        Task<IEnumerable<Subjects>> GetAllPaperCodes();
        Task<IEnumerable<Subjects>> GetPaperCodeById(string subjectCode);
        Task<IEnumerable<Subjects>> GetSubjectsByExamSession(string examcode);
        Task<List<Subjects>> GetSubjectsByPrefix(string prefix);
        Task<IEnumerable<Subjects>> GetSubjectsByPrefixes(string[] prefixes);

        Task<IEnumerable<(string SubSubjectCode, string SubSubjectDesc)>> GetSubjectByLevel(string prefix);
        Task<IEnumerable<(string SubSubjectCode, string SubSubjectDesc)>> GetSubjectByLevels(string[] prefixes);

        Task<IEnumerable<string>> GetPaperCodesBySubjectCode(string subjectCode);

        Task<Subjects> GetSubjectCode(string subjectCode);


    }
}
