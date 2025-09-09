using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.ViewModels.Examiners;

namespace ExaminerPaymentSystem.Interfaces.Transcribers
{
    public interface ITranscribersRepository
    {
        Task<OperationResult> AddNewTranscriber(Examiner examiner,string userid);
        Task<OperationResult> EditTranscribers(Examiner examiner, string attendance, string activity, string userIdd);

        //Transcribers 
        Task<IEnumerable<TandS>> GetAllTandSForTranscribers(string venue, string examCode, string activity);
        Task<IEnumerable<TandS>> GetFullListTandSForTranscribers();
        Task<IEnumerable<Examiner>> GetAllTranscribers();
        Task<IEnumerable<ExaminerScriptsMarked>> GetTranscribersRegister();



        Task<List<ExaminersListModel>> GetTranscribersFromMaster();
        Task<List<ExaminersListModel>> GetSelectedTranscribersFromTransaction(string examCode, string activity);

        Task<IEnumerable<SelectTeamViewModel>> GetTeamsFromMasterAsync();

    }
}
