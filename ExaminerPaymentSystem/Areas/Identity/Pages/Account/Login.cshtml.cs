// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ExaminerPaymentSystem.Data;
using DocumentFormat.OpenXml.Spreadsheet;
using ExaminerPaymentSystem.Repositories;
using ExaminerPaymentSystem.Models;
using DocumentFormat.OpenXml.Office2013.Excel;
using System.Numerics;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Interfaces.ExamMonitors;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IExaminerRepository _examinerRepository;
        private readonly IRegisterRepository _registerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IExamCodesRepository _examCodesRepository;
       
        private readonly ApplicationDbContext _context;


        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, UserManager<ApplicationUser> userManager, IExaminerRepository examinerRepository, IRegisterRepository registerRepository,IUserRepository userRepository,IExamCodesRepository examCodesRepository,ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
        
            _userManager = userManager;
            _examinerRepository = examinerRepository;
            _registerRepository = registerRepository;
            _userRepository = userRepository;
            _examCodesRepository = examCodesRepository;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
       
            [EmailAddress]
            public string Email { get; set; }


            [Required(ErrorMessage = "Username is required")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Username must be between 3 and 50 characters")]
            [Display(Name = "Username")]
            public string Username { get; set; }


            //[Required]

            //public string Activity { get; set; }



            public string IDNumber { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 5, ErrorMessage = "Password must be at least 6 characters")]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByNameAsync(Input.Username);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "The username you entered doesn't match our records. Please try again.");
                    return Page();
                }

        

             

                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                        // Check if the user is a CentreSupervisor
                        if (roles.Contains("CentreSupervisor"))
                        {

                        var dateCheckin = await _userRepository.GetDate();

                        if (dateCheckin != null)
                        {
                            // Check if the current date is before the StartDate
                            // Check if the current date is before the StartDate
                            if (DateTime.Now < dateCheckin.StartDate)
                            {
                                ModelState.AddModelError(string.Empty,
    $"Your account access begins on {dateCheckin.StartDate:dddd, dd MMMM yyyy}. " +
    "If you believe this is an error, please contact support.");
                                return Page();
                            }


                            // Check if the current date is after the EndDate
                            if (DateTime.Now > dateCheckin.EndDate)
                            {
                                ModelState.AddModelError(string.Empty, $"Login not allowed: Your assigned period ended on {dateCheckin.EndDate:dddd, dd MMMM yyyy}.");
                                return Page();
                            }


                        }
                    }
                   

                    // Check if the user does not have 'accounts', 'admin', or 'subjectmanager' roles
                    var excludedRoles = new[] { "Accounts", "Admin", "SubjectManager", "CentreSupervisor", "PeerReviewer", "AssistantAccountant", "OfficerSpecialNeeds","SuperAdmin","HR", "HRCapturer", "HRVerifier","ExamsAdmin", "ResidentMonitor", "ClusterManager", "AssistantClusterManager", "RegionalManager" };
                    bool isExcluded = roles.Any(role => excludedRoles.Contains(role));

                    if (!isExcluded)
                    {

                        var examiner = await _examinerRepository.GetExaminerRecord(user.IDNumber);
                        var checkregister = examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == user.EMS_SUBKEY && a.EMS_NATIONAL_ID == user.IDNumber);


                        if (checkregister == null)
                        {
                            ModelState.AddModelError(string.Empty, "Access Denied: Your account is not authorized for this activity.");
                            return Page();
                        }


                        var passwordCorrect = await _userManager.CheckPasswordAsync(user, Input.Password);

                       
                        var subjectcode = "";
                        var papercode = "";
                        if (examiner == null)
                        {
                            subjectcode = user.EMS_SUBKEY.Substring(3, 4);
                            papercode = user.EMS_SUBKEY.Substring(7, 2);
                        }
                        else
                        {
                            subjectcode = checkregister.EMS_SUB_SUB_ID.Substring(3);
                            papercode = checkregister.EMS_PAPER_CODE;
                        }



                        if (passwordCorrect)
                        {
                            string defaultPassword = await GenerateDefaultPassword(user, subjectcode, papercode);
                            if (Input.Password == defaultPassword)
                            {
                                // User provided the default password, redirect to change password page
                                return RedirectToAction("ChangePassword", "Users", new { username = Input.Username });
                            }
                            //else
                            //{
                            //    // Sign the user in and redirect to their dashboard or the next appropriate page
                            //    await _signInManager.SignInAsync(user, isPersistent: false);
                            //    return RedirectToAction("Index", "Home");
                            //}
                        }


                    }
                    else
                    {

                        var excludedRolesMonitor = new[] {  "ResidentMonitor", "ClusterManager", "AssistantClusterManager" };
                        bool isExcludedMonitor = roles.Any(role => excludedRolesMonitor.Contains(role));
                        if (isExcludedMonitor)
                        {
                            var passwordCorrect2 = await _userManager.CheckPasswordAsync(user, Input.Password);
                            var userRecord = await _context.ExamMonitorTransactions.FirstOrDefaultAsync(a => a.SubKey == user.EMS_SUBKEY);
                            if (userRecord == null)
                            {
                                ModelState.AddModelError(string.Empty, "The username you entered is not invited yet contact Regional Office to check your correct username. Please try again.");
                            }

                            if (passwordCorrect2)
                            {
                                string defaultPassword2 = $"{user.UserName.ToLower()}{userRecord.CentreAttached}.*";

                                //defaultPassword1 = char.ToUpper(defaultPassword1[0]) + defaultPassword1.Substring(1);
                                if (Input.Password == defaultPassword2)
                                {
                                    // User provided the default password, redirect to change password page
                                    return RedirectToAction("ChangePassword", "Users", new { username = Input.Username });
                                }
                            }
                            }
                        else
                        {
                                var passwordCorrect1 = await _userManager.CheckPasswordAsync(user, Input.Password);
                                if (passwordCorrect1)
                                {
                                    string defaultPassword1 = $"{user.UserName.ToLower()}100000.*";

                                    //defaultPassword1 = char.ToUpper(defaultPassword1[0]) + defaultPassword1.Substring(1);
                                    if (Input.Password == defaultPassword1)
                                    {
                                        // User provided the default password, redirect to change password page
                                        return RedirectToAction("ChangePassword", "Users", new { username = Input.Username });
                                    }
                                }

                         
                        }
                    }


                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {

                        if (!isExcluded)
                        {
                            var examiner = await _examinerRepository.GetExaminerRecord(user.IDNumber);
                            var checkregister = examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == user.EMS_SUBKEY && a.EMS_NATIONAL_ID == user.IDNumber);
                            
                          if(checkregister != null)
                            {
                                if (checkregister.AttendanceStatus == "Pending")
                                {

                                    if (checkregister.AttendanceStatus == "Pending")
                                    {
                                        var examSession = await _examCodesRepository.GetExamCodesById(checkregister.EMS_SUBKEY.Substring(0,3));
                                        string username = user.UserName;
                                        string firstname = examiner.EMS_EXAMINER_NAME;
                                        string lastname = examiner.EMS_LAST_NAME;
                                        string subject = checkregister.EMS_SUB_SUB_ID.Substring(3);
                                        string papercode = checkregister.EMS_PAPER_CODE;

                                        string subkey = user.EMS_SUBKEY;
                                        string examinerCode = user.ExaminerCode;
                                        string idNumber = user.IDNumber;
                                        string year = examSession.EXM_EXAM_YEAR;
                                        string month = examSession.EXM_EXAM_SESSION;


                                        return RedirectToAction("ConfirmAttendance", "ExaminerRegister", new
                                        {
                                            username,
                                            firstname,
                                            lastname,
                                            subject,
                                            papercode,
                                            subkey,
                                            examinerCode,
                                            idNumber,
                                            year,
                                            month
                                        });
                                    }
                                }
                          
                            }
                        }


                    _logger.LogInformation("User logged in.");
                        return LocalRedirect(returnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Wrong Username or Password.");
                        return Page();
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task<string> GenerateDefaultPassword(ApplicationUser user, string subjectcode, string papercode)
        {
       
            string password = $"{user.UserName.ToLower()}{subjectcode}{papercode}.*";

            return password;
        }

    }
}
