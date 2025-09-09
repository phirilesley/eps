using ExaminerPaymentSystem.Controllers.Examiners;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using System.Transactions;

namespace ExaminerPaymentSystem.Interfaces.Major
{
    public interface ITransactionRepository
    {

        Task DeleteTranscation(string nationalId, ApplicationUser applicationUser);
        Task<IEnumerable<ExaminerScriptsMarked>> GetInvitedExaminers(string examCode, string activity);
        Task<OperationResult> AddNewExaminerToTransaction(ExaminerScriptsMarked examiner, string userId);

        Task<OperationResult> UpdateExaminerToTransaction(ExaminerScriptsMarked examiner, string userId);


        Task<IEnumerable<ExaminerScriptsMarked>> GetExaminersFromTransaction(string examCode, string SubjectCode, string paperCode);


        Task<List<ExaminerScriptsMarked>> CheckExaminerTransactions(string examinerCode, string subjectCode, string paperCode, string searchBmsCode);
        Task<IEnumerable<ExaminerScriptsMarked>> GetTeamByBms(string formExaminerCode, string SubjectCode, string paperCode, string searchBmsCode);

        Task<IEnumerable<ExaminerScriptsMarked>> GetExaminerTransctionsAsync(string idNumber);

        Task<ExaminerScriptsMarked> GetExaminerTransactionAsync(string idNumber,string examCode,string subjectCode,string paperCode,string activity);

        
        Task<ExaminerScriptsMarked> CheckExaminerTransactions(string subkey, string idNumber, string examinerCode, string subid, string papercode);
        Task<IEnumerable<ExaminerScriptsMarked>> CheckPresentExaminersInTransactions(string examCode, string subjectCode, string paperCode,string regionCode);
        Task<List<ExaminerScriptsMarked>> CheckExaminersPresentButNoScriptMarked(string examcode, string subject, string papercode,string regionCode);
        Task<List<ExaminerScriptsMarked>> CheckExaminersPresentButNotApproved(string examcode,
            string subject, string papercode, string regionCode);
        Task<ExaminerScriptsMarked> CheckExaminerTransactionsT(string subkey, string idNumber, string examinerCode);
        Task<EntriesData> GetEntries(string examcode, string subject, string papercode, string bms);
        Task<EntriesData> GetEntriesTrascribers();
        Task<List<ExaminerScriptsMarked>> GetComponentExaminer(string examcode, string subject, string papercode,string regionCode);
        Task<List<ExaminerScriptsMarked>> GetMasterExaminers(List<string> examCodes);
        Task<List<ExaminerScriptsMarked>> GetLevelComponentExaminer(string examcode);

        Task<bool> ApproveAllTrans(IEnumerable<string> userRoles, ApplicationUser currentUser);

        Task<OperationResult> ApproveExaminers(ApprovalRequestModel request, IEnumerable<string> userRoles, ApplicationUser currentUser);

        Task<List<ExaminerScriptsMarked>> GetAllTransactionExaminer();

        Task UpdateExaminerScriptMarked(List<ExaminerScriptsMarked> examiners,ApplicationUser applicationUser);

        Task UpdatePresent(UpdateTranscactionPresenceRequest request, string userId);
        Task<OperationResult> PerformOperationsOnExceptMultipleChoicePapers(string examcode, string subject, string papercode, string bmscode, string? regioncode,ApplicationUser applicationUser);

        Task PerformOperationsOnMultipleChoicePapers(string examcode, string subject, string papercode, string bmscode, string? regioncode);

        Task InsertScriptMarked(List<ExaminerScriptsMarked> examiners,string applicationUser);

        Task<IEnumerable<ExaminerScriptsMarked>> CheckExaminerInTransactions(string examCode, string subjectCode, string paperCode,string regionCode);

        Task<IEnumerable<ExaminerScriptsMarked>> CheckTranscribersInTransactions();

        Task<bool> ApproveExaminers(string examCode, string subjectCode, string paperCode, IEnumerable<string> userRoles, ApplicationUser currentUser);

        Task<bool> ApproveTranscribers(IEnumerable<string> userRoles, ApplicationUser currentUser);
        Task<bool> ExportExaminerTransactionToOracle(string examCode, string subjectCode, string paperCode, string? regionCode);
        Task<bool> ImportExaminerTransactionFromOracle(string examCode, string subjectCode, string paperCode, string? regionCode);

        Task<bool> ExportRefCatToOracle(string examCode, string subjectCode, string paperCode, string? regionCode);
    }


}
