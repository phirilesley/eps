using ExaminerPaymentSystem.Controllers;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Major
{
    public interface ITandSRepository
    {

        //Add T and S
        Task<TandS> AddTandS(TandS tands,string userId);



        //Update T and S
        Task ApproveTandS(TandS tands, string role,string userid);
        Task RejectTandS(TandS tands, string role, ApplicationUser applicationUser, string comment);

        Task ChangeTandS(string tandscode, string idnumber, string subKey, string examinerCode);
        Task UpdateTandS(TandS tands, string userid);
        Task<TandS> AdjustTandS(TandS data, string userId);
        Task AdjustAdvance(TandS data,  string code, string userId);
        Task AdjustTandsDetails(TandS data,  string code, string userId);
        Task AdjustTandSFiles(TandS data,  string code, string userId);


        //Remove T and S

        Task DeleteTandS(string nationalId, string subkey, ApplicationUser applicationUser);

        Task DeleteAllExaminerTandS(string nationalId, ApplicationUser applicationUser);
        //Filter
        Task<IEnumerable<TandS>> GetAllTandS();

        Task<IEnumerable<TandS>> GetAllTandSByComponent(string examCode, string subjectCode, string paperCode, string venue, string regionCode, string activity, List<string> roles);

        Task<IEnumerable<TandS>> GetAllAppliedTandS(string examCode, string subjectCode, string paperCode, string venue, string regionCode, string activity, List<string> roles);

        Task<IEnumerable<TandS>> GetAllTandSByComponent2(string examCode, string subjectCode, string paperCode, string venue, string regionCode, string activity, List<string> roles);

        Task<IEnumerable<DeletedTandS>> GetAllDeleted();
        Task<TandS> GetUserTandS(string idNumber,string subKey);

        Task<TandS> GetOneTandS(string idNumber, string tandsCode, string subKey);
        Task<List<TandS>> GetTandSsByNationalID(string idNumber);



        Task<IEnumerable<TandS>> GetTandSComponentReport(string examcode, string subjectcode, string papercode, string venue, string activity, string regionCode);

        Task<IEnumerable<TandS>> GetVenueTandSReport(string examcode, string venue, string activity, List<string> year);

        Task<IEnumerable<TandS>> GetActivityTandSReport(string examCode,string activity, List<string> year);

    }

    public interface ITandSDetailsRepository
    {
        Task AddTandSDetail(TandSDetail tands,string userid);
        Task UpdateTandSDetail(List<TandSDetail> tands,string userid);
        Task DeleteTandSDetail(int id);

        Task<TandSDetail> GetTandSDetailById(int id);
        Task<IEnumerable<TandSDetail>> GetAllTandSDetails();
        Task<List<TandSDetail>> GetTandSDetails(string nationalId, string tandsCode, string subKey, string examinerCode);




        Task<TandSAdvance> GetTandSAdvance(string nationalId, string tandsCode, string subKey, string examinerCode);

        Task AddTandSAdvance(TandSAdvance tandsAdvance,string userid);

        Task UpdateTandSAdvance(TandSAdvance tands,string userid);
    }
}
