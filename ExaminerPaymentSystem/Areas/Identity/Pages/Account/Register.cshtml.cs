// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Models.Major;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace ExaminerPaymentSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUserRepository _userData;
        private readonly IExaminerRepository _examinerRepository;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, IUserRepository UserData, IExaminerRepository examinerRepository)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _userData = UserData;
            _examinerRepository = examinerRepository;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [Required]
            [StringLength(50, MinimumLength = 3)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

          
            [StringLength(50, MinimumLength = 3)]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [StringLength(50, MinimumLength = 3)]
            [Display(Name = "Last Name")]
            public string Surname { get; set; }


            [Required]
            [StringLength(50, MinimumLength = 3)]
            [Display(Name = "IDNumber")]
            public string IDNumber { get; set; }

            [Required]
            [StringLength(10, MinimumLength = 10)]
            [Display(Name = "Mobile")]
            [DataType(DataType.PhoneNumber)]
            public string Mobile { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
 
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null,string examCode = "",string subject= "",string paperCode = "")
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();




            if (ModelState.IsValid)
            {

                var subKey = subject + paperCode + Input.IDNumber;

                var checkexaminer = new Examiner();
                
                if (checkexaminer == null)
                {
                    ModelState.AddModelError(string.Empty, "Examiner record not available Contact Subject Manager or enter correct details.");
                    return Page();
                }
                string role = "Examiner";
                if (checkexaminer.EMS_ECT_EXAMINER_CAT_CODE == "E")
                {
                    role = "Examiner";
                }
            
                else if (checkexaminer.EMS_ECT_EXAMINER_CAT_CODE == "BMS")
                {
                    role = "BMS";
                }
                else if (checkexaminer.EMS_ECT_EXAMINER_CAT_CODE == "PMS")
                {
                    role = "PMS";
                }
                else if (checkexaminer.EMS_ECT_EXAMINER_CAT_CODE == "DPMS")
                {
                    role = "DPMS";
                }
                else if (checkexaminer.EMS_ECT_EXAMINER_CAT_CODE == "RPMS")
                {
                    role = "RPMS";
                }
                else if (checkexaminer.EMS_ECT_EXAMINER_CAT_CODE == "A")
                {
                    role = "A";
                }
                else if (checkexaminer.EMS_ECT_EXAMINER_CAT_CODE == "PBT")
                {
                    role = "PBT";
                }
                else if (checkexaminer.EMS_ECT_EXAMINER_CAT_CODE == "BT")
                {
                    role = "BT";
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Examiner Category Is Missing for this examiner contact Subject Manager.");
                    return Page();
                }


                if(checkexaminer.EMS_SUB_SUB_ID != subject && checkexaminer.EMS_PAPER_CODE != paperCode)
                {
                    ModelState.AddModelError(string.Empty, "Please select the apropriate subject the examiner account is not linked to the selected subject or Contact Admin.");
                    return Page();
                }

                //var checkuser = await _userData.GetUserByNationalID(Input.IDNumber);
                //if (checkuser != null)
                //{
                //    ModelState.AddModelError(string.Empty, "User with this National ID already exist.");
                //    return Page();
                //}

             
                // Take the first 3 letters of the first name and surname
                string firstNamePrefix = Input.FirstName.Substring(0, Math.Min(3, Input.FirstName.Length)).ToLower();
                string surnamePrefix = Input.Surname.ToLower();
                string subjectcode = subject.Substring(3);
                string papercode = paperCode;
             

                // Generate the username by concatenating the prefixes
                string username = $"{firstNamePrefix}{surnamePrefix}";

                var checkusername = await _userManager.FindByNameAsync(username);
                if (checkusername != null)
                {
                    ModelState.AddModelError(string.Empty, "Username Already Taken.");
                    return Page();
                }


                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = Input.Email,
                    EMS_SUBKEY = subKey,
                    PhoneNumber = Input.Mobile,
                    IDNumber = Input.IDNumber,
                 
                    ExaminerCode = checkexaminer.EMS_EXAMINER_CODE,
                    Activated = true,
                    LockoutEnabled = true,
                    EmailConfirmed = true
                };
                string defaultPassword = await GenerateDefaultPassword(user,subjectcode,papercode);

                await _userStore.SetUserNameAsync(user, username, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, defaultPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var fullName = $"{Input.FirstName}{Input.Surname}";
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, fullName));
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.MobilePhone, Input.Mobile));
                    await _userManager.AddToRoleAsync(user, role);
                        var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync("ems@zimsec.co.zw", "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { username = username, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task<string> GenerateDefaultPassword(ApplicationUser user, string subjectcode, string papercode)
        {

            var subKey = subjectcode + papercode + Input.IDNumber;

            var checkexaminer = await _examinerRepository.GetExaminer(Input.IDNumber, subKey, null);
            string firstNamePrefix = checkexaminer.EMS_EXAMINER_NAME.Substring(0, Math.Min(3, checkexaminer.EMS_EXAMINER_NAME.Length)).ToLower();
            string surnamePrefix = checkexaminer.EMS_LAST_NAME.ToLower();
            int currentYear = DateTime.Now.Year;

            // Combine the parts of the password
            string password = $"{firstNamePrefix}{surnamePrefix}{subjectcode}{papercode}.*";

            // Capitalize the first letter
            password = char.ToUpper(password[0]) + password.Substring(1);

            return password;
        }



        static string GetPrefix(string name)
        {
            // Ensure the first letter is capitalized and the rest are lowercase
            string prefix = name.Substring(0, Math.Min(3, name.Length)).ToLower();
            return char.ToUpper(prefix[0]) + prefix.Substring(1);
        }
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}



