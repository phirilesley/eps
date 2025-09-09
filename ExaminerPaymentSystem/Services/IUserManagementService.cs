using ExaminerPaymentSystem.Controllers.Examiners;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Major;

namespace ExaminerPaymentSystem.Services
{


        public interface IUserManagementService
        {
            Task CreateNewUser(ExaminerUpdateModel model, Examiner examiner, string subkey);

   
            Task UpdateExistingUser(ApplicationUser user, ExaminerUpdateModel model, string subkey);
        }
    
}
