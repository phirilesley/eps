using DocumentFormat.OpenXml.Spreadsheet;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Extensions;
using ExaminerPaymentSystem.Interfaces.ExamMonitors;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Repositories;
using ExaminerPaymentSystem.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExaminerPaymentSystem.Controllers.Common
{
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IExaminerRepository _examinerRepository;
        private readonly IRegisterRepository _registerRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IExamMonitorRepository _examMonitorRepository;
        private readonly ApplicationDbContext _context;


        public UsersController(IUserRepository userRepository, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IExaminerRepository examinerRepository, IRegisterRepository registerRepository,ITransactionRepository transactionRepository,IExamMonitorRepository examMonitorRepository,ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _examinerRepository = examinerRepository;
            _registerRepository = registerRepository;
            _transactionRepository = transactionRepository;
            _examMonitorRepository = examMonitorRepository;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index(string examCode = "", string subjectCode = "", string paperCode = "",string regionCode = "", string activity = "")
        {
         
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) && !string.IsNullOrEmpty(paperCode) && !string.IsNullOrEmpty(activity))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,
                    SubjectCode = subjectCode.Substring(3),
                    PaperCode = paperCode,
                    Activity = activity,
           

                };

                if (!string.IsNullOrEmpty(examCode))
                {
                    userSession.RegionCode = regionCode;
                }

                    HttpContext.Session.SetObjectAsJson("Session", userSession);
            }
            else
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
          
               
            }

            ViewBag.ExamCode = userSession.ExamCode;
            ViewBag.SubjectCode = userSession.SubjectCode;
            ViewBag.PaperCode = userSession.PaperCode;
            ViewBag.RegionCode = string.IsNullOrEmpty(regionCode) ? "" : userSession.RegionCode;
            ViewBag.Activity = userSession.Activity;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> IndexExamMonitors()
        {
           

            return View();
        }


        [Authorize]
        public async Task<IActionResult> IndexStaff()
        {
          

            return View();
        }

     



        [Authorize]
        public async Task<IActionResult> GetData(string examCode = "", string subjectCode = "", string paperCode = "", string regionCode = "",string activity = "")
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            IEnumerable<ApplicationUserViewModel> model = new List<ApplicationUserViewModel>();
            List<ApplicationUserViewModel> modelList = new List<ApplicationUserViewModel>();
         
        
                
                List<ApplicationUser> users = new List<ApplicationUser>();
                var usersList = await _userRepository.UsersGetAllComponent(examCode,subjectCode,paperCode,regionCode,activity);

               

                var filteredUsers = new List<ApplicationUser>();

                if (userRoles != null && userRoles.Contains("SubjectManager"))
                {
                    var excludedRoles = new List<string> { "Admin", "CentreSupervisor", "SubjectManager", "OfficerSpecialNeeds", "Accounts", "PeerReviewer", "AssistantAccountant","SuperAdmin","HR", "HRCapturer", "HRVerifier","ExamsAdmin" };

                    foreach (var user in usersList)
                    {
                        var userRoles1 = await _userManager.GetRolesAsync(user);

                        // Check if the user does not have any of the excluded roles
                        if (!userRoles1.Any(role => excludedRoles.Contains(role)))
                        {
                         
                                filteredUsers.Add(user);
                            
                            
                        }

                    }

                    foreach (var user in filteredUsers)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                            modelList.Add(new ApplicationUserViewModel
                            {
                                Id = user.Id,
                                FirstName = user.Examiner.EMS_EXAMINER_NAME,
                                Surname = user.Examiner.EMS_LAST_NAME,
                                IDNumber = user.IDNumber,
                                UserName = user.UserName,
                                Email = user.Email,
                                Activated = user.Activated,
                                Roles = roles,
                                SubKey = user.EMS_SUBKEY

                            });
                        }


                    

                }
                else if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                {
                    var excludedRoles = new List<string> { "Admin", "CentreSupervisor", "SubjectManager", "OfficerSpecialNeeds", "Accounts", "PeerReviewer", "AssistantAccountant" ,"SuperAdmin", "HRCapturer", "HRVerifier"};

                    foreach (var user in usersList)
                    {
                        var userRoles1 = await _userManager.GetRolesAsync(user);

                        // Check if the user does not have any of the excluded roles
                        if (!userRoles1.Any(role => excludedRoles.Contains(role)))
                        {
                          
                                filteredUsers.Add(user);
                            
                      
                        }

                    }

                    foreach (var user in filteredUsers)
                    {
                        var roles = await _userManager.GetRolesAsync(user);


                    modelList.Add(new ApplicationUserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.Examiner.EMS_EXAMINER_NAME,
                        Surname = user.Examiner.EMS_LAST_NAME,
                        IDNumber = user.IDNumber,
                        UserName = user.UserName,
                        Email = user.Email,
                        Activated = user.Activated,
                        Roles = roles,
                        SubKey = user.EMS_SUBKEY

                    });
                }


                    
                }
                else if (userRoles != null && userRoles.Contains("PMS"))
                {
                    var excludedRoles = new List<string> { "SubjectManager", "Admin", "CentreSupervisor","SuperAdmin" };

                    foreach (var user in usersList)
                    {

                        var userRoles1 = await _userManager.GetRolesAsync(user);
                        // Check if the user does not have any of the excluded roles
                        if (!userRoles1.Any(role => excludedRoles.Contains(role)))
                        {
                         
                       

                                    filteredUsers.Add(user);
                                
                            

                        }
                    }

                foreach (var user in filteredUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    modelList.Add(new ApplicationUserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.Examiner.EMS_EXAMINER_NAME,
                        Surname = user.Examiner.EMS_LAST_NAME,
                        IDNumber = user.IDNumber,
                        UserName = user.UserName,
                        Email = user.Email,
                        Activated = user.Activated,
                        Roles = roles,
                        SubKey = user.EMS_SUBKEY
                    });


                }

                    
                }
                else if(userRoles != null && userRoles.Contains("Admin"))
                {
                    foreach (var user in usersList)
                    {
                       
                            filteredUsers.Add(user);
                        
                    }

                    foreach (var user in filteredUsers)
                    {
                        var roles = await _userManager.GetRolesAsync(user);



                    modelList.Add(new ApplicationUserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.Examiner.EMS_EXAMINER_NAME,
                        Surname = user.Examiner.EMS_LAST_NAME,
                        IDNumber = user.IDNumber,
                        UserName = user.UserName,
                        Email = user.Email,
                        Activated = user.Activated,
                        Roles = roles,
                    
                        SubKey = user.EMS_SUBKEY
                    });
                }

                    
                    
                }else if (userRoles != null && userRoles.Contains("SuperAdmin"))
            {
                 var filteredUserss = await _userRepository.UsersGetAll();

              

                foreach (var user in filteredUserss)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    var subject = "";
                    var papercode = "";
                    var examcode = "";

                    if (user.EMS_SUBKEY != null)
                    {
                        subject = user.EMS_SUBKEY.Substring(3,4);
                        papercode = user.EMS_SUBKEY.Substring(7,2);
                        examCode = user.EMS_SUBKEY.Substring(0,3);

                    }

                    modelList.Add(new ApplicationUserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.Examiner.EMS_EXAMINER_NAME,
                        Surname = user.Examiner.EMS_LAST_NAME,
                        IDNumber = user.IDNumber,
                        UserName = user.UserName,
                        Email = user.Email,
                        Activated = user.Activated,
                        Roles = roles,
                        Subject = subject + "/" + papercode,
                        PaperCode = papercode,
                        ExamCode = examcode,
                        SubKey = user.EMS_SUBKEY
                    });
                }
            }

              
             

            
            model = modelList;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
     (p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
     (p.Surname?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
     (p.IDNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
     (p.UserName?.ToLower().Contains(searchValue.ToLower()) ?? false)
 );

            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumnDir == "asc")
                {
                    model = model.OrderBy(p => p.GetType().GetProperty(sortColumn).GetValue(p, null));
                }
                else
                {
                    model = model.OrderByDescending(p => p.GetType().GetProperty(sortColumn).GetValue(p, null));
                }
            }

            var totalRecords =  model.Count();

            var data = model.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                data
            };

            return Ok(jsonData);


        }


        [Authorize]
        public async Task<IActionResult> GetDataMonitors(string phase = "", string examSession = "", string regionCode = "")
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            IEnumerable<ApplicationUserViewModel> model = new List<ApplicationUserViewModel>();
            List<ApplicationUserViewModel> modelList = new List<ApplicationUserViewModel>();


            

            List<ApplicationUser> users = new List<ApplicationUser>();
            var usersList = await _context.Users.Include(a => a.ExamMonitor).Where(a => a.Activity == "ExamMonitoring").ToListAsync();

            var filteredUsers = new List<ApplicationUser>();

            if (userRoles != null && userRoles.Contains("RegionalManager"))
            {
                usersList = usersList.Where(a => a.Region == currentUser.Region).ToList();
                var excludedRoles = new List<string> { "Admin","CentreSupervisor", "SubjectManager", "OfficerSpecialNeeds", "Accounts", "PeerReviewer", "AssistantAccountant", "SuperAdmin", "HR", "HRCapturer", "HRVerifier", "ExamsAdmin","Examiner","PMS", "RegionalManager","RPMS","A","BMS","DPMS","S","I","BT","PBT" };

                foreach (var user in usersList)
                {
                    var userRoles1 = await _userManager.GetRolesAsync(user);

                    // Check if the user does not have any of the excluded roles
                    if (!userRoles1.Any(role => excludedRoles.Contains(role)))
                    {

                        filteredUsers.Add(user);


                    }

                }

                foreach (var user in filteredUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    modelList.Add(new ApplicationUserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.ExamMonitor.FirstName,
                        Surname = user.ExamMonitor.LastName,
                        IDNumber = user.IDNumber,
                        UserName = user.UserName,
                        Email = user.Email,
                        Activated = user.Activated,
                        Roles = roles,
                        SubKey = user.EMS_SUBKEY

                    });
                }




            }
            
            else if (userRoles != null && userRoles.Contains("Admin"))
            {
                foreach (var user in usersList)
                {

                    filteredUsers.Add(user);

                }

                foreach (var user in filteredUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);



                    modelList.Add(new ApplicationUserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.ExamMonitor.FirstName,
                        Surname = user.ExamMonitor.LastName,
                        IDNumber = user.IDNumber,
                        UserName = user.UserName,
                        Email = user.Email,
                        Activated = user.Activated,
                        Roles = roles,
                        SubKey = user.EMS_SUBKEY

                    });
                }



            }
            else if (userRoles != null && userRoles.Contains("SuperAdmin"))
            {
                var filteredUserss = await _userRepository.UsersGetAll();



                foreach (var user in filteredUserss)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                  

                    modelList.Add(new ApplicationUserViewModel
                    {
                        Id = user.Id,
                        FirstName = user.Examiner.EMS_EXAMINER_NAME,
                        Surname = user.Examiner.EMS_LAST_NAME,
                        IDNumber = user.IDNumber,
                        UserName = user.UserName,
                        Email = user.Email,
                        Activated = user.Activated,
                        Roles = roles,
                        RegionCode = regionCode,
                       
                        SubKey = user.EMS_SUBKEY
                    });
                }
            }


            if (!string.IsNullOrEmpty(phase))
            {
                modelList = modelList.Where(a => a.SubKey.Substring(2,2) == phase).ToList();
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                modelList = modelList.Where(a => a.SubKey.StartsWith(regionCode)).ToList();
            }

            if (!string.IsNullOrEmpty(examSession))
            {
                modelList = modelList.Where(a => a.SubKey.Substring(4,3) == examSession).ToList();
            }




            model = modelList;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
     (p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
     (p.Surname?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
     (p.IDNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
     (p.UserName?.ToLower().Contains(searchValue.ToLower()) ?? false)
 );

            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumnDir == "asc")
                {
                    model = model.OrderBy(p => p.GetType().GetProperty(sortColumn).GetValue(p, null));
                }
                else
                {
                    model = model.OrderByDescending(p => p.GetType().GetProperty(sortColumn).GetValue(p, null));
                }
            }

            var totalRecords = model.Count();

            var data = model.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                data
            };

            return Ok(jsonData);


        }


        [Authorize]
        public async Task<IActionResult> GetDataa()
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            IEnumerable<ApplicationUserViewModel> model = new List<ApplicationUserViewModel>();
            List<ApplicationUserViewModel> modelList = new List<ApplicationUserViewModel>();
         
               

                // User is authenticated, you can access properties of the current user
                string username = currentUser.UserName;
                string email = currentUser.Email;
                string idnumber = currentUser.IDNumber;
            

                List<ApplicationUser> users = new List<ApplicationUser>();
                var usersList = await _userRepository.UsersGetAllZiMSEC();



                var filteredUsers = new List<ApplicationUser>();

                if (userRoles != null && userRoles.Contains("Admin") || userRoles.Contains("SuperAdmin"))
                {
                   
                    foreach (var user in usersList)
                    {

                        filteredUsers.Add(user);

                    }

                    foreach (var user in filteredUsers)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        var examiner = await _examinerRepository.GetExaminerRecord(user.IDNumber);
                        if (examiner != null)
                        {
                            if (user.ExaminerCode.StartsWith("0"))
                            {

                                modelList.Add(new ApplicationUserViewModel
                                {
                                    Id = user.Id,
                                    FirstName = examiner.EMS_EXAMINER_NAME,
                                    Surname = examiner.EMS_LAST_NAME,
                                    IDNumber = user.IDNumber,
                                    UserName = user.UserName,
                                    Email = user.Email,
                                    Activated = user.Activated,
                                    Roles = roles,
                                    Subject = "0000",
                                    PaperCode = "00",
                                    ExamCode = "00",

                                });
                            }
                        }

                     
                    }
                      
                          

                    

                }


            
            model = modelList;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p => p.FirstName.ToLower().Contains(searchValue.ToLower()) || p.Surname.ToLower().Contains(searchValue.ToLower()) || p.IDNumber.ToLower().Contains(searchValue.ToLower()) || p.UserName.ToLower().Contains(searchValue.ToLower()));
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumnDir == "asc")
                {
                    model = model.OrderBy(p => p.GetType().GetProperty(sortColumn).GetValue(p, null));
                }
                else
                {
                    model = model.OrderByDescending(p => p.GetType().GetProperty(sortColumn).GetValue(p, null));
                }
            }

            var totalRecords = model.Count();

            var data = model.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                data
            };

            return Ok(jsonData);


        }

        public IActionResult ChangePassword(string username)
        {
            ViewBag.Username = username;
            return View();
    
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Retrieve the user by username
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                // Handle user not found case
                return NotFound();
            }

            // Change password logic
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
            if (result.Succeeded)
            {
                // Password changed successfully
                //return RedirectToAction("ChangePasswordConfirmation");
                //return Redirect("/Identity/Account/Login");

                // Password changed successfully
                TempData["SuccessMessage"] = $"{model.UserName} Password changed successfully. Please log in with your new password.";

                return Redirect("/Identity/Account/Login");
            }

            // Handle errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

  
        [HttpGet]
        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ResetPasswordToDefault(string userId)
        {


            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

         

            var roles = await _userManager.GetRolesAsync(user);

            // Check if the user does not have 'accounts', 'admin', or 'subjectmanager' roles
            var excludedRoles = new[] { "Accounts", "Admin", "SubjectManager", "CentreSupervisor", "PeerReviewer", "AssistantAccountant", "OfficerSpecialNeeds","SuperAdmin", "HRCapturer", "HRVerifier","ExamsAdmin" , "ResidentMonitor", "ClusterManager", "AssistantClusterManager", "RegionalManager","HR" };
            bool isExcluded = roles.Any(role => excludedRoles.Contains(role));
            if (!isExcluded)
            {
                var examiner = await _examinerRepository.GetExaminerRecord(user.IDNumber);
                if (examiner == null)
                {
                    return NotFound("Examiner not found.");
                }
                var trans = examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == user.EMS_SUBKEY);

                if (trans == null)
                {
                    return NotFound("User not found in invited contact Admin.");
                }

                var subjectcode = trans.EMS_SUB_SUB_ID.Substring(3);
                var papercode = trans.EMS_PAPER_CODE;
                string defaultPassword = GenerateDefaultPassword(user, subjectcode, papercode);
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, defaultPassword);

                if (result.Succeeded)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Error resetting password.");
                }
            }
            if (isExcluded)
            {
                var excludedMonitorsRoles = new[] {  "ResidentMonitor", "ClusterManager", "AssistantClusterManager"};

                bool isExcludedMonitor = roles.Any(role => excludedMonitorsRoles.Contains(role));

                if (isExcludedMonitor)
                {
                    //var userMaster = await _examMonitorRepository.GetMonitorByRecordAsync(user.IDNumber);
                    var transaction = await _context.ExamMonitorTransactions.FirstOrDefaultAsync(a => a.SubKey == user.EMS_SUBKEY);  
                    var defaultNumber = "100000";
                    if (transaction != null)
                    {
                        defaultNumber = transaction.CentreAttached;
                    }

                    string defaultPassword = $"{user.UserName.ToLower()}{defaultNumber}.*";
                    //defaultPassword = char.ToUpper(defaultPassword[0]) + defaultPassword.Substring(1);
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, resetToken, defaultPassword);

                    if (result.Succeeded)
                    {
                        return Ok(new { message = "Password Reset Succefully" });
                    }
                    else
                    {
                        return StatusCode(500, "Error resetting password.");
                    }
                }
                else {

                    string defaultPassword = $"{user.UserName.ToLower()}100000.*";
                    //defaultPassword = char.ToUpper(defaultPassword[0]) + defaultPassword.Substring(1);
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, resetToken, defaultPassword);

                    if (result.Succeeded)
                    {
                        return Ok(new { message = "Password Reset Succefully" });
                    }
                    else
                    {
                        return StatusCode(500, "Error resetting password.");
                    }

                }

                   
            }

            return Ok(new { message = "Password Reset Succefully" });
        }
        [Authorize]
        [HttpPost]

        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                return BadRequest(new { message = "Failed to remove user roles." });
            }

            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                return BadRequest(new { message = "Role does not exist." });
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                return BadRequest(new { message = "Failed to add user to new role." });
            }

            return Ok(new { message = "User role updated successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentUsername(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { username = user.UserName });
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUsername(string userId, string newUsername)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(newUsername))
                {
                    return BadRequest(new { message = "Username cannot be empty" });
                }

                if (newUsername.Length < 3)
                {
                    return BadRequest(new { message = "Username must be at least 3 characters" });
                }

                // Get user
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Check if username exists (excluding current user)
                var existingUser = await _userManager.FindByNameAsync(newUsername.ToLower());
              

                if (existingUser != null && existingUser.Id != userId)
                {
                    return BadRequest(new { message = "Username already taken" });
                }

                // Update username
                var result = await _userManager.SetUserNameAsync(user, newUsername.ToLower());
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { message = $"Failed to update username: {errors}" });
                }

                // Update normalized username
                user.NormalizedUserName = _userManager.NormalizeName(newUsername);
                await _userManager.UpdateAsync(user);

              

                return Ok(new { message = "Username updated successfully" });
            }
            catch (Exception ex)
            {
             
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        string RemoveMiddleName(string name)
        {
            var nameParts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length > 1)
            {
                // If there's more than one part, assume the first part is the first name and the last part is the last name
                return nameParts[0];
            }
            // If there's only one part, return it as is (could be just the first name or last name)
            return name;
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddOtherUsers()
        {
            return View();
        }

    

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddOtherUsers(ApplicationUserDTO model,string role)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            string firstNamePrefix = model.FirstName.Substring(0, Math.Min(3, model.FirstName.Length)).ToLower();
            string surnamePrefix = model.Surname.ToLower();
            string username = $"{firstNamePrefix}{surnamePrefix}";

            var s = await _examinerRepository.GetAllExaminers();
            var filtered = s.Where(a => a.EMS_EXAMINER_CODE.StartsWith("000") && a.EMS_EXAMINER_CODE.Length == 6);
            var newExaminerCode = "000000";
            // Find the maximum EMS_EXAMINER_CODE
            var maxExaminerCode = filtered.Max(a => a.EMS_EXAMINER_CODE);
            if(maxExaminerCode == null)
            {
                maxExaminerCode = "000010";
            }
            // Convert the maximum code to an integer
            if (int.TryParse(maxExaminerCode, out int maxCodeNumber))
            {
                // Increment the number
                maxCodeNumber += 1;

                // Form the new code with the incremented number, formatted to 6 digits with leading zeros
                newExaminerCode = maxCodeNumber.ToString("D6");

            }
            else
            {
                throw new InvalidOperationException("Invalid examiner code format.");
            }
            var master = new Examiner()
            {
                EMS_EXAMINER_CODE = newExaminerCode,
                EMS_SUBKEY = model.IDNumber,
                EMS_NATIONAL_ID = model.IDNumber,
                EMS_EXAMINER_NAME = model.FirstName,
                EMS_LAST_NAME = model.Surname,
                EMS_SUB_SUB_ID = "0000000",
                EMS_PAPER_CODE = "00",
                EMS_SEX = "M"
                

            };

           

            await _examinerRepository.AddNewExaminer(master,currentUser.Id);



           var checkuser = await _userRepository.GetUserNationalId(model.IDNumber);
            if (checkuser == null)
            {
                var existingUser = await _userManager.FindByNameAsync(username);


                if (existingUser == null)
                {
                    // Create a new user based on the examiner details
                    var user = new ApplicationUser
                    {
                        UserName = username,
                        Email = model.Email,
                   
                        PhoneNumber = model.Mobile,
                        IDNumber = model.IDNumber,
                  
                        EMS_SUBKEY = model.IDNumber,
                        ExaminerCode = newExaminerCode,
                        Activated = true,
                        LockoutEnabled = true,
                        EmailConfirmed = true
                    };

                    if(role == "RegionalManager")
                    {
                        user.Region = model.RegionCode;
                    }
                    else
                    {
                        user.Region = "HQ";
                    }// Generate a default password
                        string defaultPassword = $"{user.UserName.ToLower()}100000.*";

                    //defaultPassword = char.ToUpper(defaultPassword[0]) + defaultPassword.Substring(1);

                    // Create the user with the generated password
                    var result = await _userManager.CreateAsync(user, defaultPassword);

           

                    if (role == "Admin")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "Admin");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }

                    }
                   else if (role == "SuperAdmin")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "SuperAdmin");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }

                    }
                    else if (role == "Accounts")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "Accounts");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else if (role == "RegionalManager")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "RegionalManager");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else if (role == "HRVerifier")  
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "HRVerifier");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else if (role == "HRCapturer")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "HRCapturer");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else if (role == "CentreSupervisor")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "CentreSupervisor");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else if (role == "SubjectManager")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "SubjectManager");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else if (role == "PeerReviewer")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "PeerReviewer");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else if (role == "AssistantAccountant")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "AssistantAccountant");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }else if (role == "OfficerSpecialNeeds")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "OfficerSpecialNeeds");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else if (role == "HR")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "HR");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else if (role == "ExamsAdmin")
                    {
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "ExamsAdmin");
                        }
                        else
                        {
                            // Handle errors if user creation fails
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.HasError = "User with this username already exist";
                    return View();
                }
            }
            else
            {
                ViewBag.HasError = "User with this national id already exist";
                return View();
            }

            ViewBag.HasSuccess = "User Create Successfully";
            return RedirectToAction("IndexStaff");
        }


      


        // Method to generate a username based on first name and surname
        private string GenerateUserName(string firstName, string surname)
        {
            string firstNamePrefix = firstName.Substring(0, Math.Min(3, firstName.Length)).ToLower();
            string surnamePrefix = surname.ToLower();
            return $"{firstNamePrefix}{surnamePrefix}";
        }



        private string GenerateDefaultPassword(ApplicationUser user, string subjectcode, string papercode)
        {
        
            string password = $"{user.UserName.ToLower()}{subjectcode}{papercode}.*";

          
            return password;
        }



        [Authorize]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string userId, string idNumber, string subKey, string username,string examinerCode)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var checkuser = await _userRepository.GetUser(idNumber,"");

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Json(new { success = false, message = errors });
        }



    }
}
