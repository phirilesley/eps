using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Threading.Tasks;

namespace ExaminerPaymentSystem.Interfaces.Major
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> UsersGetAllRegionalMonitors(string examSession, string phase, string regionCode);
        Task<ApplicationUser> GetUserById(string Id);

        Task DeleteUserWithKey(string idNumber, string key);
        Task<ApplicationUser> GetUserNationalId(string idNumber);
        Task<ApplicationUser> GetCurrentUser(string idNumber, string subKey, string activity);
        Task<DateRange> GetDate();
        Task<IEnumerable<ApplicationUser>> UsersGetAllComponent(string examCode, string subjectCode, string paperCode, string regionCode, string activity);

        Task<IEnumerable<ApplicationUser>> UsersGetAllZiMSEC();
        Task<ApplicationUser> GetUser(string idNumber, string subKey);

        Task<ApplicationUser> SaveUser(ApplicationUser model, string password);
        Task<bool> UpdateUser(ApplicationUser model);
        Task<IEnumerable<ApplicationUser>> UsersGetAll();

        Task DeleteUser(string idNumber,string key);

        Task<ApplicationUserDTO> SaveZimsecStaffUser(ApplicationUserDTO model, string userId);

        Task<ApplicationUserDTO> GetZimsecStaffById(string Idnumber);

        Task<IEnumerable<ApplicationUserDTO>> ZimsecStaffGetAll();

        Task UpdateZimsecStaff(ApplicationUserDTO model);

        Task<OperationResult> AddNewUser(ApplicationUser user, string userId);
    }
}
