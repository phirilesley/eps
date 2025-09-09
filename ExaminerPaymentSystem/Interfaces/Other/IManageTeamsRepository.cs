using ExaminerPaymentSystem.Controllers.Examiners;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.ViewModels.Examiners;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IManageTeamsRepository
    {
        Task<IEnumerable<ExaminerTeam>> GetComponentTeamsAsync(string examCode,string subject,string paperCode,string regionCode);
        Task<IEnumerable<SelectTeamViewModel>> GetTeamsFromMasterAsync(string examCode, string subject, string paperCode, string regionCode,string activity);

        Task<IEnumerable<ExaminerScriptsMarked>> GetSelectedTeamsFromTransactionAsync(string examCode, string subject, string paperCode, string regionCode, string activity);

        Task<OperationResult> UpdateTransactionRecord(ExaminerUpdateModel updatedExaminerData,ApplicationUser applicationUser);

        Task<OperationResult> UpdateCapturingRole(string idNumber, string capturingRole, string examCode, string subjectCode, string paperCode, string regionCode, string activity,string userId);

       Task<OperationResult> UpdateCategory(string idNumber, string category, string examCode, string subjectCode, string paperCode, string regionCode, string activity,string userId);

       Task<OperationResult> UpdateExaminerNumber(string idNumber, string examinerNumber,
                                          string examCode, string subjectCode,
                                          string paperCode, string regionCode,
                                          string activity, string userId);

        Task<OperationResult> UpdateTeam(string idNumber, string team,
                                          string examCode, string subjectCode,
                                          string paperCode, string regionCode,
                                          string activity, string userId);

        Task<OperationResult> ClearTeam(
                                          string examCode, string subjectCode,
                                          string paperCode, string regionCode,
                                          string activity, string userId);
        //bool SaveTeamMember(Team teamMember);

        Task<List<string>> GetSuperordsBySubSubIdAndPaperCodeAsync(string examCode, string subSubId, string paperCode, string activity, string regionCode);

        Task<MarksCaptured> GetComponentMarkCaptured(string examCode, string subjectCode, string paperCode, string regionCode);

        Task<IEnumerable<Examiner>> GetFromMasterAsync(string examCode, string subject, string paperCode, string regionCode, string activity);

        Task<IEnumerable<SelectTeamViewModel>> GetAllFromMasterAsync();
        Task<IEnumerable<Apportioned>> GetApportionedScriptsAsync(string examCode, string subject, string paperCode, string regionCode);


        Task<OperationResult> SaveApportionment(ApportionScriptsViewModel model, string examCode, string subjectCode,
                                        string paperCode, string regionCode,
                                        string activity, string userId);

        Task<OperationResult> SaveExaminerApportionment(ExaminerApportionmentViewModel model, string examCode, string subjectCode, string paperCode, string regionCode, string activity, string userId);


               Task<OperationResult> SaveSummaryScriptApportionment(SummaryScriptApportionmentViewModel model, string examCode, string subjectCode,
                                        string paperCode, string regionCode,
                                        string activity);


        Task<IEnumerable<Apportioned>> GetApportionedScriptsAsync(string examCode, string subject, string paperCode, string regionCode, string activity);

        Task<IEnumerable<ExaminerApportionment>> GetApportionedExaminersAsync(string examCode, string subject, string paperCode, string regionCode, string activity);

        Task<OperationResult> ProcessTeamUpdates(List<TeamMemberImportModel> teamList, string examCode, string subject, string paperCode, string regionCode, string activity, ApplicationUser currentUser);
    }
}
