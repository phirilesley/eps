using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IPaperMarkingRateRepository
    {
        Task<IEnumerable<PaperMarkingRate>> GetAllPaperCodes();
        Task<IEnumerable<PaperMarkingRate>> GetPaperCodeById(string subjectCode);
        Task<IEnumerable<PaperMarkingRate>> GetPaperCodesBySubject(string subject, string examcode);
        Task<IEnumerable<PaperMarkingRate>> GetSubjectsByExamSession(string examcode);

        Task UpdatePaperMarkingRate(PaperMarkingRate papermarkingrate);
        Task UpdatePaperMarkingRates(List<PaperMarkingRate> rates, string PPR_SUB_SUB_ID);

        Task<PaperMarkingRate> GetPaperMarkingRate(string subject, string papercode, string examcode);

        Task<List<PaperMarkingRate>> GetPaperCodeBySub(string subjectId);

    }
}
