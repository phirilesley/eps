using ExaminerPaymentSystem.Controllers.Examiners;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Major;
using Microsoft.AspNetCore.Identity;

namespace ExaminerPaymentSystem.Services
{
    public class UserManagementService: IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task CreateNewUser(ExaminerUpdateModel model, Examiner examiner, string subkey)
        {
            var username = GenerateUsername(examiner);
           

            var newUser = new ApplicationUser
            {
                UserName = username,
                Email = $"{username}@ems.com",
                EMS_SUBKEY = subkey,
                PhoneNumber = examiner.EMS_PHONE_HOME ?? "0000000000",
                IDNumber = examiner.EMS_NATIONAL_ID,
                ExaminerCode = examiner.EMS_EXAMINER_CODE,
                Activated = true,
                LockoutEnabled = true,
                EmailConfirmed = true,
                Activity = model.Activity
            };
            var password = GenerateDefaultPassword(newUser,model.SubjectCode, model.PaperCode);
            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                await AssignRole(newUser, model.Category);
            }
            else
            {
                throw new ApplicationException($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task UpdateExistingUser(ApplicationUser user, ExaminerUpdateModel model, string subkey)
        {
            user.EMS_SUBKEY = subkey;
            user.Activity = model.Activity;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await ReassignRole(user, model.Category);
            }
            else
            {
                throw new ApplicationException($"User update failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        private async Task AssignRole(ApplicationUser user, string category)
        {
            var roleName = GetRoleName(category);
            await _userManager.AddToRoleAsync(user, roleName);
        }

        private async Task ReassignRole(ApplicationUser user, string category)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var roleName = GetRoleName(category);
            await _userManager.AddToRoleAsync(user, roleName);
        }

        private string GetRoleName(string category)
        {
            return category switch
            {
                "E" => "Examiner",
                "A" => "A",
                "BT" => "BT",
                "PBT" => "PBT",
                "PMS" => "PMS",
                "BMS" => "BMS",
                "DPMS" => "DPMS",
                "RPMS" => "RPMS",
                "S" => "S", 
                "I" => "I",
                _ => "Examiner" // Default role
            };
        }

        private string GenerateUsername(Examiner examiner)
        {
            string cleanFirstName = RemoveMiddleName(examiner.EMS_EXAMINER_NAME);
            string cleanSurname = RemoveMiddleName(examiner.EMS_LAST_NAME);

            string firstNamePrefix = cleanFirstName.Substring(0, Math.Min(3, cleanFirstName.Length)).ToLower();
            string surnamePrefix = cleanSurname.ToLower();

            string username = $"{firstNamePrefix}{surnamePrefix}";
            string originalUsername = username;

            int counter = 1;
            while (_userManager.FindByNameAsync(username).Result != null)
            {
                username = $"{originalUsername}{counter++}";
            }

            return username;
        }


        private string GenerateDefaultPassword(ApplicationUser user, string subjectcode, string papercode)
        {
            //string cleanFirstName = RemoveMiddleName(firstname);
            //string cleanSurname = RemoveMiddleName(surname);

            //// Generate prefixes
            //string firstNamePrefix = cleanFirstName.Substring(0, Math.Min(3, cleanFirstName.Length)).ToLower();
            //string surnamePrefix = cleanSurname.ToLower();
            //int currentYear = DateTime.Now.Year;

            // Combine the parts of the password
            string password = $"{user.UserName.ToLower()}{subjectcode}{papercode}.*";

            // Capitalize the first letter
            //password = char.ToUpper(password[0]) + password.Substring(1);

            return password;
        }
        private string RemoveMiddleName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var nameParts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return nameParts.Length > 0 ? nameParts[0] : string.Empty;
        }
    }
}
