using ExaminerPaymentSystem.Controllers;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Repositories.Examiners;
using System.Threading.Tasks;

namespace ExaminerPaymentSystem.Interfaces.Major
{
    public interface IExaminerRepository
    {
        //Create data to master
        Task<OperationResult> AddOrReplaceExaminer(Examiner examiner,string userId);

        Task<OperationResult> AddNewExaminer(Examiner examiner,string userId);

        Task<List<ApplicationUser>> GetUsersBySubkeys(IEnumerable<string> subkeys);

        Task<List<Examiner>> GetExaminersByIdNumbers(List<string> idNumbers);

        //Alter data in master
        Task<OperationResult> ChangeSubject(Examiner examiner, string userId);

        Task<OperationResult> UpdateExaminerRecord(Examiner updatedExaminerData,string userId);
        Task<OperationResult> ChangeRoleAndTeam(Examiner examiner, string userId);

        Task<OperationResult> AdminUpdateExaminerDetails(Examiner examiner, string examCode, string userId, string activity);

        Task<OperationResult> EditExaminer(Examiner examiner, string examCode,string userId, string activity, string attendance);

        //Filter Master
        Task<Examiner> GetExaminer(string nationalId, string subKey, string examinerCode);




        Task<Examiner> GetExaminerRecord(string nationalId);
        Task<IEnumerable<Examiner>> GetAllExaminers();
        Task<IEnumerable<Examiner>> GetComponentExaminers(string examCode, string subsbuid, string papercode, string activity, string regionCode);
        Task<IEnumerable<Examiner>> GetPresentComponentExaminers(string examCode, string subsbuid, string papercode, string activity, string regionCode);

        Task<IEnumerable<Examiner>> GetComponentExaminersTransaction(string examCode, string subsbuid, string papercode, string activity, string regionCode);

        Task<IEnumerable<Examiner>> GetAllComponentExaminersTransaction(string examCode, string activity);


        Task<List<ExaminerScriptsMarked>> GetPresentExaminersFromRegister(string examcode, string subjectCode, string paperCode);
        Task<List<ExaminerDto>> GetAbsentExaminersByCategoryAsync(string categoryCode, string examCode,string subSubId, string paperCode, string activity, string regionCode);

        Task<List<string>> GetSuperordsBySubSubIdAndPaperCodeAsync(string examCode,string subSubId, string paperCode, string activity, string regionCode);
        Task<Examiner> GetExaminerWithSUBKEY(string subKey, string examinerCode);

   
        Task<IEnumerable<Examiner>> GetExaminersByParameters(string subjectCode, string paperCode);

        Task<Examiner> GetLatestExaminerAsync();



        //Remove from Master
        Task<OperationResult> RemoveExaminer(string nationalId,string username);
        Task<bool> ImportExaminersFromOracle(string examLevel);
        Task<bool> ExportExaminersToOracle(string examLevel);

        



        //Transcribers
























        //Task DeleteExaminer(int id);

        //Task<Examiner> GetExaminerByNationalId(string nationalId,string examinercode);

        //Task<Examiner> CheckExaminerExist(string nationalId,string firstname,string lastname,string examcode,string subject,string papercode, string examinercode);

        //Task<Examiner> GetExaminerByParameters(string nationalId,string examinerNumber);

        //Task<Examiner> GetExaminerByExaminerCode(string examinerCode, string subjectcode, string papercode);



        //Task DeleteExaminer(string examinerCode, string subjectCode, string paperCode);

        //Task<IEnumerable<Examiner>> GetComponentExaminer(string examinercode, string subject, string papercode, string bmscode);





        //int GetNumberOfScriptsMarked(string subjectCode, string paperCode);

        //Task UpdateExaminerScripts(Examiner examiner);

        //Task UpdateExaminerScriptsMarked(RootObject rootObject);

        //Task<IEnumerable<ExaminerScriptsMarked>> GetTeamByBmsForExaminerCategory(String formExaminerCode, String SubjectCode, String paperCode, String searchBmsCode);


        // Add a method to check for records in EXAMINER_TRANSACTIONS

        //Task<List<ExaminerScriptsMarked>> CheckExaminerTransactionsApproval(string examinerCode, string subjectCode, string paperCode);
        //Task ApproveAllScripts(IEnumerable<string> userRoles, string currentUser, string subject, string examcode, string papercode);


        //Task RejectAllScripts(IEnumerable<string> userRoles, string currentUser,string subject, string examcode, string papercode);

        //Task InsertExaminerTransactions(RootObject rootObject,ApplicationUser applicationUser);
    }
}
