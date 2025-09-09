using DocumentFormat.OpenXml.Spreadsheet;
using ElmahCore;
using ExaminerPaymentSystem.Controllers.Examiners;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Extensions;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Interfaces.Transcribers;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Repositories;
using ExaminerPaymentSystem.Services;
using ExaminerPaymentSystem.ViewModels.Common;
using ExaminerPaymentSystem.ViewModels.Examiners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Protocol.Core.Types;
using System.Diagnostics;


namespace ExaminerPaymentSystem.Controllers.Transcibers
{
    public class TranscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExaminerRepository _examinerRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPaperMarkingRateRepository _paperMarkingRate;
        private readonly ICategoryRateRepository _categoryMarkingRate;
        private readonly IExamCodesRepository _examCodesRepository;
        private readonly IBanksRepository _banksRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ITandSRepository _andSRepository;
        private readonly IRegisterRepository _registerRepository;
        private readonly IMaxExaminerCodeRepository _maxExaminerCodeRepository;
        private readonly ILogger<TranscribersController> _logger;
        private readonly ITranscribersRepository _transcribersRepository;
        private readonly IAdvanceFeesRepository _advanceFeesRepository;
        private readonly ITandSFilesRepository _tandSFilesRepository;
        private readonly ITandSDetailsRepository _detailRepository;
        private readonly IUserManagementService _userManagementService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IActivityRepository _activityRepository;
        // Constructor to inject the repository dependency
        public TranscribersController(IExaminerRepository examinerRepository, SignInManager<ApplicationUser> signInManager,
           IPaperMarkingRateRepository paperMarkingRate, ICategoryRateRepository categoryMarkingRate, IExamCodesRepository examCodesRepository, IBanksRepository banksRepository, UserManager<ApplicationUser> userManager, IUserRepository userRepository, ITandSRepository tandSRepository, IRegisterRepository registerRepository, IMaxExaminerCodeRepository examinerCodeRepository, ILogger<TranscribersController> logger, ITranscribersRepository transcribersRepository, IAdvanceFeesRepository advanceFeesRepository, ITandSFilesRepository tandSFilesRepository, ITandSDetailsRepository detailRepository, ApplicationDbContext context, IUserManagementService userManagementService, ITransactionRepository transactionRepository, IActivityRepository activityRepository)
        {
            _examinerRepository = examinerRepository;
            _signInManager = signInManager;
            _categoryMarkingRate = categoryMarkingRate;
            _examCodesRepository = examCodesRepository;
            _paperMarkingRate = paperMarkingRate;
            _banksRepository = banksRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _andSRepository = tandSRepository;
            _registerRepository = registerRepository;
            _maxExaminerCodeRepository = examinerCodeRepository;
            _logger = logger;
            _transcribersRepository = transcribersRepository;
            _advanceFeesRepository = advanceFeesRepository;
            _banksRepository = banksRepository;
            _tandSFilesRepository = tandSFilesRepository;
            _detailRepository = detailRepository;
            _context = context;
            _userManagementService = userManagementService;
            _transactionRepository = transactionRepository;
            _activityRepository = activityRepository;
        }

        public async Task<IActionResult> SelectTranscribers(string examCode = "", string activity = "")
        {
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,

                    Activity = activity,
                };


                HttpContext.Session.SetObjectAsJson("Session", userSession);
            }
            else
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
            }

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "Session Expired",
                    text = " You have been logged out for security reasons login again.",
                    showConfirmButton = true,
                    confirmButtonColor = "#ffc107", // Warning color
                                                    //timer = 5000, // Auto-close after 5 seconds
                    timerProgressBar = true
                });



                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
            }

            ViewBag.ExamCode = userSession.ExamCode;

            ViewBag.Activity = userSession.Activity;



            return View();
        }

        [Authorize]
        public async Task<IActionResult> GetData(string examCode = "", string activity = "", string status = "")
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            List<SelectTeamViewModel> availableInRegister = new List<SelectTeamViewModel>();
            IEnumerable<SelectTeamViewModel> model = new List<SelectTeamViewModel>();


            var examinersInTransaction = await _context.EXAMINER_TRANSACTIONS
                  .Where(a => a.EMS_SUB_SUB_ID.StartsWith(examCode) && a.EMS_ACTIVITY == activity && (a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT" || a.EMS_ECT_EXAMINER_CAT_CODE == "S" || a.EMS_ECT_EXAMINER_CAT_CODE == "I" || a.EMS_ECT_EXAMINER_CAT_CODE == "A"))
                  .Include(a => a.Examiner)
                  .Include(a => a.Examiner.ExaminerScriptsMarkeds)
                  .Select(a => a.Examiner)
                  .Where(e => e != null)
                  .ToListAsync();

            // Get all examiners from EXM_EXAMINER_MASTER
            var allComponentExaminers = await _context.EXM_EXAMINER_MASTER
                .Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT" || a.EMS_ECT_EXAMINER_CAT_CODE == "S" || a.EMS_ECT_EXAMINER_CAT_CODE == "I" && a.EMS_ECT_EXAMINER_CAT_CODE == "A")
                .Include(a => a.ExaminerScriptsMarkeds)
                .ToListAsync();

            // Combine both lists, avoiding duplicates based on unique identifier (e.g., ExaminerCode)
            var combined = allComponentExaminers
                .Concat(examinersInTransaction)
                .DistinctBy(a => a.EMS_NATIONAL_ID)
                .ToList();

            foreach (var component in combined)
            {
                string station = string.IsNullOrEmpty(component.EMS_WORK_ADD1)
? string.Empty
: component.EMS_WORK_ADD1.Substring(0, Math.Min(15, component.EMS_WORK_ADD1.Length));
                var subkey = examCode + "9001" + "01" + activity + component.EMS_NATIONAL_ID;
                var transaction = component.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_NATIONAL_ID == component.EMS_NATIONAL_ID && a.EMS_SUBKEY == subkey && a.EMS_ACTIVITY == activity && a.EMS_SUB_SUB_ID.StartsWith(examCode));

                if (transaction != null)
                {
                    var datanow = new SelectTeamViewModel
                    {
                        Name = component.EMS_EXAMINER_NAME + " " + component.EMS_LAST_NAME,
                        IdNumber = transaction.EMS_NATIONAL_ID,
                        Sex = component.EMS_SEX,
                        Category = transaction.EMS_ECT_EXAMINER_CAT_CODE,
                        CapturingRole = "N/A",
                        Station = station,
                        Province = component.EMS_WORK_ADD3,
                        District = component.EMS_WORK_ADD2,
                        ExaminerNumber = "1001",
                        Team = "1001",
                        Selected = "Selected",
                        Status = "Selected",
                        RegisterStatus = transaction.RegisterStatus
                    };

                    availableInRegister.Add(datanow);

                }
                else
                {
                    var datanow = new SelectTeamViewModel
                    {
                        Name = component.EMS_EXAMINER_NAME + " " + component.EMS_LAST_NAME,
                        IdNumber = component.EMS_NATIONAL_ID,
                        Sex = component.EMS_SEX,
                        Category = component.EMS_ECT_EXAMINER_CAT_CODE,
                        CapturingRole = "N/A",
                        Station = station,
                        Province = component.EMS_WORK_ADD3,
                        District = component.EMS_WORK_ADD2,
                        ExaminerNumber = "1001",
                        Team = "1001",
                        Selected = "Pending",
                        Status = "Pending",
                    };

                    availableInRegister.Add(datanow);
                }

            }


            if (!string.IsNullOrEmpty(status))
            {
                availableInRegister = status switch
                {
                    "TotalInvited" => availableInRegister, // No filter for total
                    "Selected" => availableInRegister.Where(e => e.Selected == "Selected").ToList(),
                    "PBT" => availableInRegister.Where(e => e.Category == "PBT" && e.Selected == "Selected").ToList(),
                    "BT" => availableInRegister.Where(e => e.Category == "BT" && e.Selected == "Selected").ToList(),
                    "S" => availableInRegister.Where(e => e.Category == "S" && e.Selected == "Selected").ToList(),
                    "I" => availableInRegister.Where(e => e.Category == "I" && e.Selected == "Selected").ToList(),
                    "A" => availableInRegister.Where(e => e.Category == "I" && e.Selected == "Selected").ToList(),

                    _ => availableInRegister
                };
            }


            model = availableInRegister.ToList();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 50;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
    (p.Name?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.IdNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.ExaminerNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.Category?.ToLower().Contains(searchValue.ToLower()) ?? false)
);

            }



            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                // Special handling for examiner number sorting
                if (sortColumn.Equals("examinerNumber", StringComparison.OrdinalIgnoreCase))
                {
                    if (sortColumnDir == "asc")
                    {
                        model = model.OrderBy(p =>
                            string.IsNullOrEmpty(p.ExaminerNumber) ? "ZZZZZZ" : p.ExaminerNumber
                        );
                    }
                    else
                    {
                        model = model.OrderByDescending(p =>
                            string.IsNullOrEmpty(p.ExaminerNumber) ? "" : p.ExaminerNumber
                        );
                    }
                }
                else // Default sorting for other columns
                {
                    if (sortColumnDir == "asc")
                    {
                        model = model.OrderBy(p =>
                            p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                        );
                    }
                    else
                    {
                        model = model.OrderByDescending(p =>
                            p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                        );
                    }
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

        public async Task<IActionResult> UpdateSelect([FromBody] ExaminerUpdateModel model)
        {
            if (model == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {


                model.Team = "1001";
                model.Category = model.Category;
                model.ExaminerNumber = "1001";
                model.Team = "1001";
                model.CapturingRole = "N/A";
                model.SubjectCode = "9001";
                model.PaperCode = "01";


                var currentUser = await _signInManager.UserManager.GetUserAsync(User);

                var transactionKey = $"{model.ExamCode}{model.SubjectCode}";

                var existing = await _context.EXAMINER_TRANSACTIONS
                    .FirstOrDefaultAsync(a =>
                        a.EMS_SUB_SUB_ID.StartsWith(model.ExamCode) &&
                        a.EMS_ACTIVITY == model.Activity &&
                        a.EMS_NATIONAL_ID == model.IdNumber);

                if (model.IsSelected)
                {
                    if (existing == null)
                    {
                        var examiner = await _context.EXM_EXAMINER_MASTER
                            .FirstOrDefaultAsync(a => a.EMS_NATIONAL_ID == model.IdNumber);

                        if (examiner == null)
                        {
                            return BadRequest("Examiner not found.");
                        }

                        var newTransaction = new ExaminerScriptsMarked
                        {
                            EMS_NATIONAL_ID = model.IdNumber,
                            EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
                            EMS_ECT_EXAMINER_CAT_CODE = model.Category,
                            EMS_CAPTURINGROLE = model.CapturingRole,
                            EMS_EXAMINER_NUMBER = model.ExaminerNumber,
                            EMS_EXM_SUPERORD = model.Team,
                            EMS_SUB_SUB_ID = transactionKey,
                            EMS_PAPER_CODE = model.PaperCode,
                            EMS_ACTIVITY = model.Activity,
                            EMS_SUBKEY = $"{transactionKey}{model.PaperCode}{model.Activity}{model.IdNumber}",
                            EMS_MARKING_REG_CODE = examiner.EMS_MARKING_REG_CODE,
                            IsPresent = false,
                            RegisterStatus = "Absent",
                            RegisterStatusBy = currentUser.UserName,
                            RegisterStatusDate = DateTime.Now.ToString(),
                            RecommendedStatus = "Pending",
                            RecommendedBy = currentUser.UserName,
                            RecommendedDate = DateTime.Now.ToString(),
                            AttendanceStatus = "Pending",
                            AttendanceStatusBy = currentUser.UserName,
                            AttendanceStatusDate = DateTime.Now.ToString(),
                            SCRIPTS_MARKED = 0
                        };

                        await _context.EXAMINER_TRANSACTIONS.AddAsync(newTransaction);
                        await _context.SaveChangesAsync(currentUser.Id);
                        await HandleExaminerSelection(model);
                    }

                }
                else
                {
                    if (existing != null)
                    {
                        _context.EXAMINER_TRANSACTIONS.Remove(existing);
                        await _context.SaveChangesAsync(currentUser.Id);
                        await HandleExaminerRemoval(model);
                        await HandleTandSRemoval(model, currentUser);

                    }
                }



                return Ok();
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An error occurred while updating the presence status.");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CountTransStats(string examCode = "", string activity = "", string regionCode = "")
        {
            var selectedInExaminers = await _context.EXAMINER_TRANSACTIONS
                .Where(a => a.EMS_SUB_SUB_ID.StartsWith(examCode) && a.EMS_ACTIVITY == activity && (a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT" || a.EMS_ECT_EXAMINER_CAT_CODE == "S" || a.EMS_ECT_EXAMINER_CAT_CODE == "I" || a.EMS_ECT_EXAMINER_CAT_CODE == "A"))

                .ToListAsync();

            // Get all examiners from EXM_EXAMINER_MASTER
            var availableInRegister = await _context.EXM_EXAMINER_MASTER
                .Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT" || a.EMS_ECT_EXAMINER_CAT_CODE == "S" || a.EMS_ECT_EXAMINER_CAT_CODE == "I" || a.EMS_ECT_EXAMINER_CAT_CODE == "A")

                .ToListAsync();


            var selectedExaminers = selectedInExaminers.Count();

            var pbtExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PBT");
            var btExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "BT");
            var sExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "S");
            var iExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "I");
            var aExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "A");


            var counts = new { Total = availableInRegister.Count(), SelectedCount = selectedExaminers, PbtCount = pbtExaminers, BtCount = btExaminers, SCount = sExaminers, ICount = iExaminers, ACount = aExaminers };

            return Json(counts);
        }


        private async Task HandleExaminerSelection(ExaminerUpdateModel model)
        {
            var subkey = $"{model.ExamCode}{model.SubjectCode}{model.PaperCode}{model.Activity}{model.IdNumber}";
            var examiner = await _examinerRepository.GetExaminerRecord(model.IdNumber);

            if (examiner == null) return;


            var user = await _userRepository.GetUser(model.IdNumber, subkey);
            model.Category ??= "BT"; // Default category

            if (user == null)
            {
                await _userManagementService.CreateNewUser(model, examiner, subkey);
            }
            else
            {
                await _userManagementService.UpdateExistingUser(user, model, subkey);
            }
        }

        private async Task HandleExaminerRemoval(ExaminerUpdateModel model)
        {
            var key = $"{model.ExamCode}{model.SubjectCode}{model.PaperCode}{model.Activity}{model.IdNumber}";
            await _userRepository.DeleteUserWithKey(model.IdNumber, key);
        }

        private async Task HandleTandSRemoval(ExaminerUpdateModel model, ApplicationUser applicationUser)
        {
            var key = $"{model.ExamCode}{model.SubjectCode}{model.PaperCode}{model.Activity}{model.IdNumber}";
            await _andSRepository.DeleteTandS(model.IdNumber, key, applicationUser);
        }




        [HttpPost]
        public async Task<IActionResult> UpdateCategory(string idNumber, string category, string examCode, string activity)
        {
            if (string.IsNullOrWhiteSpace(idNumber) || string.IsNullOrWhiteSpace(category))
            {
                return BadRequest("Invalid data provided.");
            }
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var existing = await _context.EXAMINER_TRANSACTIONS
                        .FirstOrDefaultAsync(a => a.EMS_SUB_SUB_ID.StartsWith(examCode) &&
                                                  a.EMS_ACTIVITY == activity &&
                                                  a.EMS_NATIONAL_ID == idNumber);

            existing.EMS_ECT_EXAMINER_CAT_CODE = category;
            _context.EXAMINER_TRANSACTIONS.Update(existing);

            await _context.SaveChangesAsync(currentUser.Id);
            return Ok();

        }

        [Authorize]
        public async Task<IActionResult> TranscribersList(string examCode = "", string activity = "", bool isSuccess = false, string message = "")
        {
            var userSession = new SessionModel();

            if (!string.IsNullOrEmpty(examCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,

                    Activity = activity,

                };


                HttpContext.Session.SetObjectAsJson("Session", userSession);
            }
            else
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");


            }


            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "Session Expired",
                    text = " You have been logged out for security reasons login again.",
                    showConfirmButton = true,
                    confirmButtonColor = "#ffc107", // Warning color
                                                    //timer = 5000, // Auto-close after 5 seconds
                    timerProgressBar = true
                });



                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
            }


            ViewBag.ExamCode = userSession.ExamCode;
              
                ViewBag.Activity = userSession.Activity;
            
              



if (!isSuccess && !string.IsNullOrEmpty(message))
{
    TempData["ErrorMessage"] = message;

}

if (isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["SuccessMessage"] = "Record Added Successfully.";

            }

            return View();
        }


        //[Authorize]
        //public async Task<IActionResult> SelectTranscribers(bool isSuccess = false, string message = "")
        //{


        //    if (isSuccess && !string.IsNullOrEmpty(message))
        //    {
        //        TempData["SuccessMessage"] = "Record Added Successfully.";

        //    }

        //    return View();
        //}


        [Authorize]
        public async Task<IActionResult> SelectedTranscribers(string examCode,string activity, bool isSuccess = false, string message = "")
        {
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(activity))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,
                 
                    Activity = activity,

                };

                HttpContext.Session.SetObjectAsJson("Session", userSession);
            }
            else
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");

            }

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "Session Expired",
                    text = " You have been logged out for security reasons login again.",
                    showConfirmButton = true,
                    confirmButtonColor = "#ffc107", // Warning color
                                                    //timer = 5000, // Auto-close after 5 seconds
                    timerProgressBar = true
                });



                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
            }

       
                ViewBag.ExamCode = userSession.ExamCode;
        
                ViewBag.Activity = userSession.Activity;

  
if (isSuccess && !string.IsNullOrEmpty(message))
{
    TempData["SuccessMessage"] = message;

}

if (!isSuccess && !string.IsNullOrEmpty(message))
{
    TempData["Error"] = message;

}

return View();
        }


        [Authorize]
        public async Task<IActionResult> GetTranscribers()
        {
    
            IEnumerable<ExaminersListModel> model = new List<ExaminersListModel>();
  
           var transcribersList = await _transcribersRepository.GetTranscribersFromMaster();
            model = transcribersList;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 50;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
    (p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.LastName?.ToLower().Contains(searchValue.ToLower()) ?? false) /*||*/

);

            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumnDir == "asc")
                {
                    model = model.OrderBy(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
                else
                {
                    model = model.OrderByDescending(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
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
        public async Task<IActionResult> GetSelectedTranscribers(string examCode, string activity)
        {

            IEnumerable<ExaminersListModel> model = new List<ExaminersListModel>();

            var transcribersList = await _transcribersRepository.GetSelectedTranscribersFromTransaction(examCode,activity);

            model = transcribersList;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 50;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
    (p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.LastName?.ToLower().Contains(searchValue.ToLower()) ?? false) /*||*/

);

            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumnDir == "asc")
                {
                    model = model.OrderBy(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
                else
                {
                    model = model.OrderByDescending(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
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
        public async Task<IActionResult> GetSelectTranscribers()
        {

            IEnumerable<SelectTeamViewModel> model = new List<SelectTeamViewModel>();

            var transcribersList = await _transcribersRepository.GetTeamsFromMasterAsync();
            model = transcribersList;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 50;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
    p.Name?.ToLower().Contains(searchValue.ToLower()) ?? false 

);

            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumnDir == "asc")
                {
                    model = model.OrderBy(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
                else
                {
                    model = model.OrderByDescending(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
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
        [HttpGet]
        public async Task<IActionResult> CountStats(string examCode,string activity)
        {
            var availableInRegister =  await _transcribersRepository.GetTranscribersFromMaster();

            var selectedInExaminers = await _transcribersRepository.GetSelectedTranscribersFromTransaction(examCode,activity);

            var selectedExaminers = selectedInExaminers.Count();

            var btExaminers = selectedInExaminers.Count(a => a.Category == "BT");
            var pbtExaminers = selectedInExaminers.Count(a => a.Category == "PBT");
      

            var counts = new { Total = availableInRegister.Count(), SelectedCount = selectedExaminers, BtCount = btExaminers, PbtCount = pbtExaminers };

            return Json(counts);
        }


        [Authorize(Roles = "SuperAdmin,Admin,SubjectManager,CentreSupervisor,OfficerSpecialNeeds")]
        [HttpGet]
        public async Task<IActionResult> AddNewTranscriber(string examCode = "", string activity = "", bool isSuccess = false, string message = "")
        {
            var newexaminercode = await InitializeNextTrainingCode();
          
            var newExaminer = new Examiner();
            newExaminer.EMS_EXAMINER_CODE = newexaminercode.ToString();
            newExaminer.EMS_SUB_SUB_ID = "9001";
            newExaminer.EMS_PAPER_CODE = "01";

            ViewBag.ExamCode = examCode;
            ViewBag.Activity = activity;
            ViewBag.NewExaminerCode = newexaminercode.ToString();

            if (!isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = message;

            }

            return View(newExaminer);
        }


        [HttpGet]
        public async Task<IActionResult> GetSupervisors(string searchTerm)
        {
            try
            {
                // Return empty if search term is too short (reduces unnecessary queries)
                if (!string.IsNullOrWhiteSpace(searchTerm) && searchTerm.Length < 2)
                {
                    return Json(new List<object>());
                }

                IQueryable<Examiner> query = _context.EXM_EXAMINER_MASTER
                    .AsNoTracking(); // Improves performance for read-only operations

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.ToLower();
                    query = query.Where(s =>
                        EF.Functions.Like(s.EMS_EXAMINER_NAME.ToLower(), $"%{term}%") ||
                        EF.Functions.Like(s.EMS_LAST_NAME.ToLower(), $"%{term}%") ||
                        s.EMS_NATIONAL_ID.ToLower().Contains(term));
                }

                var supervisors = await query
                    .OrderBy(s => s.EMS_EXAMINER_NAME)
                    .Select(s => new
                    {
                        id = s.EMS_NATIONAL_ID,
                        text = $"{s.EMS_NATIONAL_ID} - {s.EMS_EXAMINER_NAME} {s.EMS_LAST_NAME}",
                        firstname = s.EMS_EXAMINER_NAME,
                        lastname = s.EMS_LAST_NAME
                    })
                    .Take(10)
                    .ToListAsync();

                return Json(supervisors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervisors");
                return StatusCode(500, new { error = "An error occurred while fetching supervisors" });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin,SubjectManager,CentreSupervisor,OfficerSpecialNeeds")]
        [HttpPost]
        public async Task<IActionResult> AddNewTranscriber(Examiner examiner, string activity,string examCode,string supervisor)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                if(examiner.EMS_ECT_EXAMINER_CAT_CODE == "A" && supervisor == null)
                {
                    TempData["Error"] = "Supervisor is required for Aides";
                    return View(examiner);
                }
                var checksupervisor = await _context.EXM_EXAMINER_MASTER.
                    FirstOrDefaultAsync(a => a.EMS_NATIONAL_ID == supervisor);

                if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "A" && checksupervisor == null) 
                {
                    TempData["Error"] = "Supervisor not in the records check National ID";
                
                    return View(examiner);
                }

                examiner.CreatedBy = currentUser.UserName;
                examiner.CreatedDate = DateTime.Now.ToString();
                examiner.EMS_SUBKEY = examCode + "9001" + "01" + activity + examiner.EMS_NATIONAL_ID ;
                examiner.EMS_SUB_SUB_ID = "9001";
                var result = await _transcribersRepository.AddNewTranscriber(examiner, currentUser.Id);

                

                if (result.Success)
                {
                    if(checksupervisor != null && supervisor != null && examiner.EMS_ECT_EXAMINER_CAT_CODE == "A")
                    {
                        checksupervisor.EMS_AID = "Y";
                        checksupervisor.EMS_SRC_SUPERORD = supervisor;

                        _context.Update(checksupervisor);
                        await _context.SaveChangesAsync(currentUser.Id);
                    }

                    var subKey = examiner.EMS_SUBKEY;
                    string cleanFirstName = RemoveMiddleName(examiner.EMS_EXAMINER_NAME);
                    string cleanSurname = RemoveMiddleName(examiner.EMS_LAST_NAME);
                    var subjectcode = examiner.EMS_SUB_SUB_ID;
                    var papercode = examiner.EMS_PAPER_CODE;
                  

                    // Generate prefixes
                    string firstNamePrefix = cleanFirstName.Substring(0, Math.Min(3, cleanFirstName.Length)).ToLower();
                    string surnamePrefix = cleanSurname.ToLower();

                    // Construct the username
                    string username = $"{firstNamePrefix}{surnamePrefix}";
                    // Check if the examiner already exists as a user

                    var checkuser = await _userRepository.GetUser(examiner.EMS_NATIONAL_ID, examiner.EMS_SUBKEY);
                    if (checkuser == null)
                    {
                        var originalUsername = username;
                        var existingUser = await _userManager.FindByNameAsync(username);
                        int counter = 1;

                        // Check if the existing username already exists and matches the criteria
                        while (existingUser != null && existingUser.UserName == username && existingUser.IDNumber != examiner.EMS_NATIONAL_ID)
                        {
                            // Append the counter to the username
                            username = originalUsername + counter.ToString();

                            // Check if the new username exists
                            existingUser = await _userManager.FindByNameAsync(username);

                            // Increment the counter for the next iteration
                            counter++;
                        }


                        if (existingUser == null)
                        {
                            // Create a new user based on the examiner details
                            var user = new ApplicationUser
                            {
                                UserName = username,
                                Email = $"{username}@ems.com",
                                EMS_SUBKEY = subKey,
                                PhoneNumber = examiner.EMS_PHONE_HOME ?? "0000000000",
                                IDNumber = examiner.EMS_NATIONAL_ID,
                                ExaminerCode = examiner.EMS_EXAMINER_CODE,
                                Activated = true,
                                LockoutEnabled = true,
                                EmailConfirmed = true
                            };


                            // Generate a default password
                            string defaultPassword = GenerateDefaultPassword(user);

                            // Create the user with the generated password
                            var results = await _userManager.CreateAsync(user, defaultPassword);
                            if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "PBT")
                            {
                                if (results.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(user, "PBT");
                                }
                                else
                                {
                                    // Handle errors if user creation fails
                                    foreach (var error in results.Errors)
                                    {
                                        ModelState.AddModelError(string.Empty, error.Description);
                                    }
                                }
                            }
                            else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "BT")
                            {
                                if (results.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(user, "BT");
                                }
                                else
                                {
                                    // Handle errors if user creation fails
                                    foreach (var error in results.Errors)
                                    {
                                        ModelState.AddModelError(string.Empty, error.Description);
                                    }
                                }
                            }
                            else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "S")
                            {
                                if (results.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(user, "S");
                                }
                                else
                                {
                                    // Handle errors if user creation fails
                                    foreach (var error in results.Errors)
                                    {
                                        ModelState.AddModelError(string.Empty, error.Description);
                                    }
                                }
                            }
                            else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "I")
                            {
                                if (results.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(user, "I");
                                }
                                else
                                {
                                    // Handle errors if user creation fails
                                    foreach (var error in results.Errors)
                                    {
                                        ModelState.AddModelError(string.Empty, error.Description);
                                    }
                                }
                            }
                            else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "A")
                            {
                                if (results.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(user, "A");
                                }
                                else
                                {
                                    // Handle errors if user creation fails
                                    foreach (var error in results.Errors)
                                    {
                                        ModelState.AddModelError(string.Empty, error.Description);
                                    }
                                }
                            }

                        }

                    }

                }

                if (!result.Success)
                {
                    return View();
                }

                return RedirectToAction("TranscribersList", new { isSuccess = true, message = result.Message });
            }
            catch (Exception ex)
            {

                ElmahExtensions.RaiseError(ex);
                TempData["Error"] = "Examiner Details could not be Created" + ex.Message;
                return View();
            }

        }

        [Authorize(Roles = "SuperAdmin,Admin,OfficerSpecialNeeds,SubjectManager,CentreSupervisor")]
        [HttpPost]
        public async Task<IActionResult> EditTranscriber(Examiner examiner,string activity,string supervisor,string attendance)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                var checksupervisor = await _context.EXM_EXAMINER_MASTER.
                  FirstOrDefaultAsync(a => a.EMS_NATIONAL_ID == supervisor);

                if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "A" && checksupervisor == null)
                {
                    TempData["Error"] = "Supervisor not in the records check National ID";

                    return View(examiner);
                }

                // Save the changes
                var result = await _transcribersRepository.EditTranscribers(examiner,attendance,activity,currentUser.Id);

                if (result.Success)
                {

                    if (checksupervisor != null && supervisor != null && examiner.EMS_ECT_EXAMINER_CAT_CODE == "A")
                    {
                        checksupervisor.EMS_AID = "Y";
                        checksupervisor.EMS_SRC_SUPERORD = supervisor;

                        _context.Update(checksupervisor);
                        await _context.SaveChangesAsync(currentUser.Id);
                    }
                    var user = await _userRepository.GetUser(examiner.EMS_NATIONAL_ID, examiner.EMS_SUBKEY);
                    if (user != null)
                    {
                        // Update user details

                        user.EMS_SUBKEY = examiner.EMS_SUBKEY;

                        var results = await _userManager.UpdateAsync(user);

                        if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "BT")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "BT");
                            }
                            else
                            {
                                // Handle errors if user creation fails
                                foreach (var error in results.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                            }
                        }
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "PBT")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "PBT");
                            }
                            else
                            {
                                // Handle errors if user creation fails
                                foreach (var error in results.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                            }
                        }
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "S")
                        {
                            if (results.Succeeded)
                            {
                                await _userManager.AddToRoleAsync(user, "S");
                            }
                            else
                            {
                                // Handle errors if user creation fails
                                foreach (var error in results.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                            }
                        }
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "I")
                        {
                            if (results.Succeeded)
                            {
                                await _userManager.AddToRoleAsync(user, "I");
                            }
                            else
                            {
                                // Handle errors if user creation fails
                                foreach (var error in results.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                            }
                        }
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "A")
                        {
                            if (results.Succeeded)
                            {
                                await _userManager.AddToRoleAsync(user, "A");
                            }
                            else
                            {
                                // Handle errors if user creation fails
                                foreach (var error in results.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                            }
                        }
                        

                        }


                }

                if (!result.Success)
                {
                    return RedirectToAction("TranscribersList", new { isSuccess = false, message = result.Message });
                }

                return RedirectToAction("TranscribersList", new { isSuccess = true, message = "Record Updated Successfully" });
            }
            catch (Exception ex)
            {

                ElmahExtensions.RaiseError(ex);
                TempData["Error"] = "Examiner Details could not be Created" + ex.Message;
                return View();
            }


        }


        public async Task<IActionResult> EditTranscriber( string idNumber = "", string examCode = "",string activity = "", bool isSuccess = false, string message = "")
        {
            var examiner = await _examinerRepository.GetExaminerRecord(idNumber);
            if (examiner == null)
            {
                return NotFound();
            }
            var examinerTransaction = examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID  && a.EMS_ACTIVITY == activity);
            if (examinerTransaction != null)
            {
                examiner.EMS_ECT_EXAMINER_CAT_CODE = examinerTransaction.EMS_ECT_EXAMINER_CAT_CODE;
                examiner.EMS_EXAMINER_NUMBER = examinerTransaction.EMS_EXAMINER_NUMBER;
                examiner.EMS_MARKING_REG_CODE = examinerTransaction.EMS_MARKING_REG_CODE;
                examiner.EMS_EXM_SUPERORD = examinerTransaction.EMS_EXM_SUPERORD;
                examiner.EMS_SUB_SUB_ID = examinerTransaction.EMS_SUB_SUB_ID;
                examiner.EMS_PAPER_CODE = examiner.EMS_PAPER_CODE;
                examiner.EMS_SUBKEY = examinerTransaction.EMS_SUBKEY;
                ViewBag.Attendance = examinerTransaction.AttendanceStatus;

                if(examinerTransaction.EMS_ECT_EXAMINER_CAT_CODE == "A")
                {
                    ViewBag.Supervisor = examiner.EMS_SRC_SUPERORD;
                }
            }
            else
            {
                TempData["Error"] = "Examiner not in transaction";
            }
            ViewBag.examCode = examCode;
            ViewBag.activity = activity;


            if (!isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["Error"] = message;

            }
            return View(examiner);

           
        }

        [Authorize]
        public async Task<IActionResult> TranscribersUsers(string examCode = "", string activity = "")
        {
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,

                    Activity = activity,
                };


                HttpContext.Session.SetObjectAsJson("Session", userSession);
            }
            else
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
            }

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "Session Expired",
                    text = " You have been logged out for security reasons login again.",
                    showConfirmButton = true,
                    confirmButtonColor = "#ffc107", // Warning color
                                                    //timer = 5000, // Auto-close after 5 seconds
                    timerProgressBar = true
                });



                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
            }

            ViewBag.ExamCode = userSession.ExamCode;

            ViewBag.Activity = userSession.Activity;


            return View();
        }



        [Authorize]
        public async Task<IActionResult> GetTranscribersRegister(string examCode = "", string activity = "", string status = "")
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            IEnumerable<RegisterViewModel> model = new List<RegisterViewModel>();
            List<RegisterViewModel> modelList = new List<RegisterViewModel>();

            var availableInRegister = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUBKEY.StartsWith(examCode) && a.EMS_ACTIVITY == activity && (a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT" || a.EMS_ECT_EXAMINER_CAT_CODE == "S" || a.EMS_ECT_EXAMINER_CAT_CODE == "I" || a.EMS_ECT_EXAMINER_CAT_CODE == "A"))
             .Include(t => t.Examiner)
             .ToListAsync();

            //var availableInRegister = await _transcribersRepository.GetTranscribersRegister();

            if (!string.IsNullOrEmpty(status))
            {
                availableInRegister = status switch
                {
                    "TotalInvited" => availableInRegister, // No filter for total
                    "ConfirmedAttending" => availableInRegister.Where(e => e.AttendanceStatus == "Yes").ToList(),
                    "Pending" => availableInRegister.Where(e => e.AttendanceStatus == "Pending").ToList(),
                    "NotAttending" => availableInRegister.Where(e => e.AttendanceStatus == "No").ToList(),
                    "Recommended" => availableInRegister.Where(e => e.RecommendedStatus == "Recommended").ToList(),
                    "Absent" => availableInRegister.Where(e => e.RegisterStatus == "Absent").ToList(),
                    _ => availableInRegister
                };
            }

            modelList = availableInRegister.Select(item => new RegisterViewModel
            {
                FirstName = item.Examiner.EMS_EXAMINER_NAME,
                LastName = item.Examiner.EMS_LAST_NAME,
                PaperCode = item.Examiner.EMS_PAPER_CODE,
                Subject = item.Examiner.EMS_SUB_SUB_ID + "/" + item.Examiner.EMS_PAPER_CODE,
                IdNumber = item.EMS_NATIONAL_ID,
                Category = item.EMS_ECT_EXAMINER_CAT_CODE,
                SubKey = item.EMS_SUBKEY,
                ExaminerNumber = item.Examiner.EMS_EXAMINER_NUMBER,
                ExaminerCode = item.EMS_EXAMINER_CODE,
                Status = item.RegisterStatus,
                RecommendedStatus = item.RecommendedStatus,
                AttendanceStatus = item.AttendanceStatus
            }).ToList();



         
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
                model = model.Where(p => p.FirstName.ToLower().Contains(searchValue.ToLower()) || p.LastName.ToLower().Contains(searchValue.ToLower()) || p.IdNumber.ToLower().Contains(searchValue.ToLower()));
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
        [HttpGet]
        public async Task<IActionResult> CountAbsentAndPresent(string examCode = "", string activity = "")
        {


            //var availableInRegister = await _transcribersRepository.GetTranscribersRegister();

            var availableInRegister = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUBKEY.StartsWith(examCode) && a.EMS_ACTIVITY == activity && (a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT" || a.EMS_ECT_EXAMINER_CAT_CODE == "S" || a.EMS_ECT_EXAMINER_CAT_CODE == "I" || a.EMS_ECT_EXAMINER_CAT_CODE == "A"))
        .Include(t => t.Examiner)
        .ToListAsync();

            var presentExaminers = availableInRegister.Count(p => p.RegisterStatus == "Present");
            var absentExaminers = availableInRegister.Count(a => a.RegisterStatus == "Absent");
            var comingExaminers = availableInRegister.Count(a => a.AttendanceStatus == "Yes");
            var notComingExaminers = availableInRegister.Count(a => a.AttendanceStatus == "No");
            var pendingExaminers = availableInRegister.Count(a => a.AttendanceStatus == "Pending");

            var counts = new { Total = availableInRegister.Count(), PresentCount = presentExaminers, AbsentCount = absentExaminers, ComingCount = comingExaminers, NotcomingCount = notComingExaminers, PendingCount = pendingExaminers };

            return Json(counts);
        }



        [Authorize(Roles = "SubjectManager,CentreSupervisor,OfficerSpecialNeeds")]
        public async Task<IActionResult> TranscribersTandS( string venue = "",string activity="",string examCode="")
        {

            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(venue) && !string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(activity))
            {
                userSession = new SessionModel()
                {
              
                    Venue = venue,
                    Activity = activity,
                    ExamCode = examCode 

                };
                HttpContext.Session.SetObjectAsJson("Session", userSession);
            }
            else
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
             
            }


            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "Session Expired",
                    text = " You have been logged out for security reasons login again.",
                    showConfirmButton = true,
                    confirmButtonColor = "#ffc107", // Warning color
                                                    //timer = 5000, // Auto-close after 5 seconds
                    timerProgressBar = true
                });



                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
            }

            ViewBag.Venue = userSession.Venue;
            ViewBag.Activity = userSession.Activity;
            ViewBag.ExamCode = userSession.ExamCode;


            return View();

        }


        [Authorize]
        public async Task<IActionResult> GetAccountsTandS(string venue = "", string examCode = "", string activity = "", string statuss = "")
        {
            var userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
            IEnumerable<TandSListViewModel> model = new List<TandSListViewModel>();
            List<TandSListViewModel> modelList = new List<TandSListViewModel>();
           

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var totalCount = 0;
            var approvedtands = 0;
            var pendingtands = 0;

            var tandsList = await _transcribersRepository.GetAllTandSForTranscribers(venue,examCode,activity);

            var checkPresent = await _transcribersRepository.GetSelectedTranscribersFromTransaction(examCode,activity);

            totalCount =  checkPresent.Where(a => a.Status == "Present").Count();
            IEnumerable<TandS> tands = new List<TandS>();

            var userRoles = await _userManager.GetRolesAsync(currentUser);


            if (userRoles != null && userRoles.Contains("Accounts"))
            {

                approvedtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved").Count();
                pendingtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Pending").Count();
                tandsList = tandsList.Where(a => a.CENTRE_SUPERVISOR_STATUS == "Approved");

              


                if (!string.IsNullOrEmpty(statuss))
                {
                    tandsList = statuss switch
                    {
                        "TotalInvited" => tandsList, // No filter for total
                        "ApprovedCount" => tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved"),
                        "PendingCount" => tandsList.Where(e => e.ACCOUNTS_STATUS == "Pending" ),

                        _ => tandsList
                    };
                }

            }
            else if (userRoles != null && userRoles.Contains("PeerReviewer"))
            {

                approvedtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Approved").Count();
                pendingtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Pending").Count();
                tandsList = tandsList.Where(a => a.ACCOUNTS_STATUS == "Approved");

                if (!string.IsNullOrEmpty(statuss))
                {
                    tandsList = statuss switch
                    {
                        "TotalInvited" => tandsList, // No filter for total
                        "ApprovedCount" => tandsList.Where(e =>  e.ACCOUNTS_REVIEW == "Approved"),
                        "PendingCount" => tandsList.Where(e =>  e.ACCOUNTS_REVIEW == "Pending"),

                        _ => tandsList
                    };
                }

            }
            else if (userRoles != null && userRoles.Contains("AssistantAccountant"))
            {
                approvedtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved").Count();
                pendingtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending").Count();
                tandsList = tandsList.Where(a => a.ACCOUNTS_STATUS == "Approved" && a.ACCOUNTS_REVIEW == "Approved");
               

                if (!string.IsNullOrEmpty(statuss))
                {
                    tandsList = statuss switch
                    {
                        "TotalInvited" => tandsList, // No filter for total
                        "ApprovedCount" => tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved"),
                        "PendingCount" => tandsList.Where(e => e.ACCOUNTS_STATUS == "Pending" || e.ACCOUNTS_REVIEW == "Pending"),

                        _ => tandsList
                    };
                }

            }
            else if (userRoles != null && userRoles.Contains("Admin"))
            {
                approvedtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved").Count();
                pendingtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending" && e.EMS_VENUE == userSession.Venue).Count();
                tandsList = tandsList.Where(a => a.EMS_VENUE == userSession.Venue);

              


                

                if (!string.IsNullOrEmpty(statuss))
                {
                    tandsList = statuss switch
                    {
                        "TotalInvited" => tandsList, // No filter for total
                        "ApprovedCount" => tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved"),
                        "PendingCount" => tandsList.Where(e => e.ACCOUNTS_STATUS == "Pending" || e.ACCOUNTS_REVIEW == "Pending"),

                        _ => tands
                    };
                }
            }

           


            foreach (var ex in tandsList)
            {
               
                modelList.Add(new TandSListViewModel
                {
                    ExaminerCode = ex.EMS_EXAMINER_CODE,
                    FirstName = ex.Examiner.EMS_EXAMINER_NAME,
                    LastName = ex.Examiner.EMS_LAST_NAME,
                    IDNumber = ex.EMS_NATIONAL_ID,
          
                    SubKey = ex.EMS_SUBKEY,
                    AccountsStatus = ex.ACCOUNTS_STATUS,
                    PeerStatus = ex.ACCOUNTS_REVIEW,
                    ClaimId = ex.TANDSCODE,
                    ReturnBackStatus = ex.ReturnBackStatus,
                    ApprovedStatus = ex.CENTRE_SUPERVISOR_STATUS,
                    ApprovedBy = ex.CENTRE_SUPERVISOR_STATUS_BY,
                    ApprovedDate = ex.CENTRE_SUPERVISOR_DATE,
                    RecommendedBy = ex.SUBJECT_MANAGER_STATUS_BY,
                    RecommendedStatus = ex.SUBJECT_MANAGER_STATUS,
                    RecommendedDate = ex.SUBJECT_MANAGER_DATE,
                    CreatedDate = ex.DATE


                });
            }

            var returned = modelList.Where(a => a.ReturnBackStatus == "Returned");

            model = modelList;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 50;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p => p.FirstName.ToLower().Contains(searchValue.ToLower()) || p.LastName.ToLower().Contains(searchValue.ToLower()) || p.IDNumber.ToLower().Contains(searchValue.ToLower()));
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
                totalCount,
                approvedtands,
                pendingtands,
                returned,
                data
            };

            return Ok(jsonData);

        }


        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UpdateTandS(TandSViewModel viewModel)
        {


            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            DateTime currentDate = DateTime.Now;
            string formattedDate = currentDate.ToString();
        
            List<TandSDetail> newadjustedDetails = new List<TandSDetail>();
            foreach (var item in viewModel.TANDSDETAILS)
            {
                var newDetail = new TandSDetail()
                {
                    Id = item.Id,
                    ADJ_BUSFARE = item.EMS_BUSFARE,
                    ADJ_ACCOMMODATION = item.EMS_ACCOMMODATION,
                    ADJ_DINNER = item.EMS_DINNER,
                    ADJ_LUNCH = item.EMS_LUNCH,
                    ADJ_TOTAL = item.EMS_TOTAL,
                    ADJ_BY = currentUser.UserName,
                    ADJ_DATE = DateTime.Now.ToString(),
                    EMS_SUBKEY = viewModel.EMS_SUBKEY,
                    EMS_EXAMINER_CODE = viewModel.EMS_EXAMINER_CODE,
                    EMS_NATIONAL_ID = viewModel.EMS_NATIONAL_ID,
                    TANDSCODE = viewModel.TANDSCODE,

                };

                newadjustedDetails.Add(newDetail);

            }

            await _detailRepository.UpdateTandSDetail(newadjustedDetails, currentUser.Id);

            viewModel.TANDSADVANCE.EMS_NATIONAL_ID = viewModel.EMS_NATIONAL_ID;
            viewModel.TANDSADVANCE.TANDSCODE = viewModel.TANDSCODE;
            viewModel.TANDSADVANCE.EMS_SUBKEY = viewModel.EMS_SUBKEY;
            viewModel.TANDSADVANCE.EMS_EXAMINER_CODE = viewModel.EMS_EXAMINER_CODE;


            var advanceFees = await _advanceFeesRepository.GetAdvanceFees();


            decimal travellingtotal = 0;
            foreach (var detail in viewModel.TANDSDETAILS)
            {
                // Convert string properties to decimal and add them to the total

                travellingtotal += detail.EMS_BUSFARE.GetValueOrDefault();
                travellingtotal += detail.EMS_ACCOMMODATION.GetValueOrDefault();
                travellingtotal += detail.EMS_LUNCH.GetValueOrDefault();
                travellingtotal += detail.EMS_DINNER.GetValueOrDefault();

            }
            decimal totalAdv = 0;

            totalAdv += viewModel.TANDSADVANCE.ADJ_ADV_ACCOMMODATION_NONRES.GetValueOrDefault() * advanceFees.FEE_ACCOMMODATION_NONRES.GetValueOrDefault();

            totalAdv += viewModel.TANDSADVANCE.ADJ_ADV_ACCOMMODATION_RES.GetValueOrDefault() * advanceFees.FEE_ACCOMMODATION_RES.GetValueOrDefault();

            totalAdv += viewModel.TANDSADVANCE.ADJ_ADV_BREAKFAST.GetValueOrDefault() * advanceFees.FEE_BREAKFAST.GetValueOrDefault();

            totalAdv += viewModel.TANDSADVANCE.ADJ_ADV_DINNER.GetValueOrDefault() * advanceFees.FEE_DINNER.GetValueOrDefault();

            totalAdv += viewModel.TANDSADVANCE.ADJ_ADV_LUNCH.GetValueOrDefault() * advanceFees.FEE_LUNCH.GetValueOrDefault();

            totalAdv += viewModel.TANDSADVANCE.ADJ_ADV_OVERNIGHTALLOWANCE.GetValueOrDefault() * advanceFees.FEE_OVERNIGHTALLOWANCE.GetValueOrDefault();

            totalAdv += viewModel.TANDSADVANCE.ADJ_ADV_TEAS.GetValueOrDefault() * advanceFees.FEE_TEA.GetValueOrDefault();

            totalAdv += viewModel.TANDSADVANCE.ADJ_ADV_TRANSPORT.GetValueOrDefault() * advanceFees.FEE_TRANSPORT.GetValueOrDefault();

            var totalTandS = travellingtotal + totalAdv;


            viewModel.TANDSADVANCE.ADJ_ADV_TOTAL = totalAdv;
            await _detailRepository.UpdateTandSAdvance(viewModel.TANDSADVANCE, currentUser.Id);


            var tanstobeupdated = new TandS
            {

                DATE_ADJ = formattedDate,
                EMS_NATIONAL_ID = viewModel.EMS_NATIONAL_ID,
                EMS_EXAMINER_CODE = viewModel.EMS_EXAMINER_CODE,
                EMS_SUBKEY = viewModel.EMS_SUBKEY,
                TANDSCODE = viewModel.TANDSCODE,
                ACCOUNTS_DATE = formattedDate,
                ReturnBackBy = null,
                ReturnComment = null,
                ReturnBackStatus = null,
                ADJ_TOTAL = totalTandS,
                ACCOUNTS_STATUS = "Approved",
                ACCOUNTS_STATUS_BY = currentUser.UserName,
                ACCOUNTS_REVIEW = "Pending",
                ADJ_BY = currentUser.UserName,

            };


            await _andSRepository.UpdateTandS(tanstobeupdated, currentUser.Id);

            //return RedirectToAction("TandSAccountsReview", new { claimId = viewModel.TANDSCODE, nationalId = viewModel.EMS_NATIONAL_ID, examinerCode = viewModel.EMS_EXAMINER_CODE, subKey = viewModel.EMS_SUBKEY, isSuccess = true });
            return Redirect($"/Transcribers/TandSAccountsReview?claimId={viewModel.TANDSCODE}&nationalId={viewModel.EMS_NATIONAL_ID}&examinerCode={viewModel.EMS_EXAMINER_CODE}&subKey={viewModel.EMS_SUBKEY}&isSuccess=true");
        }

        [Authorize]
        public async Task<IActionResult> GetTandS( string venue = "",string examCode = "",string activity="", string statuss = "")
        {

            IEnumerable<TandSListViewModel> model = new List<TandSListViewModel>();
            List<TandSListViewModel> modelList = new List<TandSListViewModel>();
          


            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var totalCount = 0;
            var approvedtands = 0;
            var pendingtands = 0;

     
   

                var tandsList = await _transcribersRepository.GetAllTandSForTranscribers(venue,examCode,activity);

            var checkPresent = await _transcribersRepository.GetSelectedTranscribersFromTransaction(examCode,activity);

            totalCount = checkPresent.Where(a => a.Status == "Present").Count();

 

                var userRoles = await _userManager.GetRolesAsync(currentUser);

                if (userRoles != null && userRoles.Contains("SubjectManager"))
                {
                    approvedtands = tandsList.Where(e => e.SUBJECT_MANAGER_STATUS == "Recommended").Count();
                    pendingtands = tandsList.Where(e => e.SUBJECT_MANAGER_STATUS == "Pending").Count();

                    tandsList = tandsList.Where(a => a.SUBJECT_MANAGER_STATUS == "Pending");
                   

                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tandsList = statuss switch
                        {
                            "TotalInvited" => tandsList, // No filter for total
                            "ApprovedCount" => tandsList.Where(e => e.SUBJECT_MANAGER_STATUS == "Recommended"),
                            "PendingCount" => tandsList.Where(e => e.SUBJECT_MANAGER_STATUS == "Pending"),

                            _ => tandsList
                        };
                    }

                }
                else if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                {

                    approvedtands = tandsList.Where(e => e.CENTRE_SUPERVISOR_STATUS == "Approved").Count();
                    pendingtands = tandsList.Where(e => e.CENTRE_SUPERVISOR_STATUS == "Pending").Count();
                    tandsList = tandsList.Where(a => a.SUBJECT_MANAGER_STATUS == "Pending" && a.CENTRE_SUPERVISOR_STATUS == "Pending");

               

                    

                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tandsList = statuss switch
                        {
                            "TotalInvited" => tandsList, // No filter for total
                            "ApprovedCount" => tandsList.Where(e => e.CENTRE_SUPERVISOR_STATUS == "Approved"),
                            "PendingCount" => tandsList.Where(e => e.CENTRE_SUPERVISOR_STATUS == "Pending"),

                            _ => tandsList
                        };
                    }


                }
                else if (userRoles != null && userRoles.Contains("OfficerSpecialNeeds"))
                {
                    approvedtands = tandsList.Where(e => e.SUBJECT_MANAGER_STATUS == "Recommended").Count();
                    pendingtands = tandsList.Where(e => e.SUBJECT_MANAGER_STATUS == "Pending").Count();
                    tandsList = tandsList.Where(a => a.SUBJECT_MANAGER_STATUS == "Pending");
                 

                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tandsList = statuss switch
                        {
                            "TotalInvited" => tandsList, // No filter for total
                            "ApprovedCount" => tandsList.Where(e => e.ACCOUNTS_STATUS == "Recommended"),
                            "PendingCount" => tandsList.Where(e => e.ACCOUNTS_STATUS == "Pending"),

                            _ => tandsList
                        };
                    }
                }


            modelList = tandsList.Select(ex =>
            {
                var rec = new TandSListViewModel
                {
                    ExaminerCode = ex.EMS_EXAMINER_CODE,
                    FirstName = ex.Examiner.EMS_EXAMINER_NAME,
                    LastName = ex.Examiner.EMS_LAST_NAME,
                    IDNumber = ex.EMS_NATIONAL_ID,
                    Subject = ex.Examiner.EMS_SUB_SUB_ID + "/" + ex.Examiner.EMS_PAPER_CODE,
                    SubKey = ex.EMS_SUBKEY,
                    ClaimId = ex.TANDSCODE,
                    CreatedDate = ex.DATE
                };

                if (userRoles != null && userRoles.Contains("SubjectManager"))
                {
                    rec.Status = ex.SUBJECT_MANAGER_STATUS;
                    rec.ApprovedStatus = ex.CENTRE_SUPERVISOR_STATUS;
                    rec.ApprovedDate = ex.CENTRE_SUPERVISOR_DATE;
                    rec.ApprovedBy = ex.CENTRE_SUPERVISOR_STATUS_BY;
                }
                else if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                {
                    rec.Status = ex.CENTRE_SUPERVISOR_STATUS;

                    var trans = ex.Examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == ex.EMS_SUBKEY);
                    if (trans != null)
                    {
                        if (trans.RegisterStatus == "Present")
                        {
                            rec.RecommendedStatus = "Recommended";
                            rec.RecommendedDate = trans.RegisterStatusDate;
                            rec.RecommendedBy = trans.RegisterStatusBy;
                        }
                        else
                        {
                            rec.RecommendedStatus = "Pending";
                            rec.RecommendedDate = trans.RegisterStatusDate;
                            rec.RecommendedBy = trans.RegisterStatusBy;
                        }
                    }




                }

                return rec;
            }).ToList();


            model = modelList;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 50;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p => p.FirstName.ToLower().Contains(searchValue.ToLower()) || p.LastName.ToLower().Contains(searchValue.ToLower()) || p.IDNumber.ToLower().Contains(searchValue.ToLower()));
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


            ViewBag.totalCount = totalRecords;
            ViewBag.approvedtands = approvedtands;
            ViewBag.pendingtands = pendingtands;


            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                totalCount,
                approvedtands,
                pendingtands,
                data
            };

            return Ok(jsonData);

        }

        [Authorize(Roles = "Accounts,PeerReviewer,Admin,SuperAdmin")]
        public async Task<IActionResult> TandSAccountsList( string venue = "",string activity="",string examCode="")
        {
        

            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(venue) && !string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(activity))
            {
                userSession = new SessionModel()
                {

                    Venue = venue,
                    Activity = activity,
                    ExamCode = examCode

                };
                HttpContext.Session.SetObjectAsJson("Session", userSession);
            }
            else
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");

            }

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "Session Expired",
                    text = " You have been logged out for security reasons login again.",
                    showConfirmButton = true,
                    confirmButtonColor = "#ffc107", // Warning color
                                                    //timer = 5000, // Auto-close after 5 seconds
                    timerProgressBar = true
                });



                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
            }

            ViewBag.Venue = userSession.Venue;
            ViewBag.Activity = userSession.Activity;
            ViewBag.ExamCode = userSession.ExamCode;

            return View();
        }



        [Authorize(Roles = "SubjectManager,CentreSupervisor,OfficerSpecialNeeds")]
        public async Task<IActionResult> TranscriberTandSApproval(string claimId, string nationalId, string examinerCode, string subKey)
        {
            // Fetch TandS record and advance fees
            var checkTandS = await _andSRepository.GetOneTandS(nationalId, claimId, subKey);
            var advanceFees = await _advanceFeesRepository.GetAdvanceFees();

            // Initialize totals
            decimal totalBusfare = 0, totalAccommodation = 0, totalLunch = 0, totalDinner = 0, totalTotal = 0;
            decimal adjTotalBusfare = 0, adjAccommodation = 0, adjLunch = 0, adjDinner = 0, adjTotal = 0;

            // Calculate totals if TandS record exists
            if (checkTandS != null)
            {
                foreach (var detail in checkTandS.TandSDetails)
                {
                    totalBusfare += Convert.ToDecimal(detail.EMS_BUSFARE);
                    totalAccommodation += Convert.ToDecimal(detail.EMS_ACCOMMODATION);
                    totalLunch += Convert.ToDecimal(detail.EMS_LUNCH);
                    totalDinner += Convert.ToDecimal(detail.EMS_DINNER);
                    totalTotal += Convert.ToDecimal(detail.EMS_TOTAL);

                    adjTotalBusfare += Convert.ToDecimal(detail.ADJ_BUSFARE);
                    adjAccommodation += Convert.ToDecimal(detail.ADJ_ACCOMMODATION);
                    adjLunch += Convert.ToDecimal(detail.ADJ_LUNCH);
                    adjDinner += Convert.ToDecimal(detail.ADJ_DINNER);
                    adjTotal += Convert.ToDecimal(detail.ADJ_TOTAL);
                }

                // Assign totals to ViewBag
                ViewBag.TotalBusfare = totalBusfare;
                ViewBag.TotalAccommodation = totalAccommodation;
                ViewBag.TotalLunch = totalLunch;
                ViewBag.TotalDinner = totalDinner;
                ViewBag.TotalTotal = totalTotal;

                ViewBag.TotalAdjBusfare = adjTotalBusfare;
                ViewBag.TotalAdjAccommodation = adjAccommodation;
                ViewBag.TotalAdjLunch = adjLunch;
                ViewBag.TotalAdjDinner = adjDinner;
                ViewBag.TotalAdjTotal = adjTotal;
            }

            // Prepare TandS ViewModel
            TandSViewModel examinerData = null;
            if (checkTandS != null)
            {
                examinerData = new TandSViewModel
                {
                    EMS_EXAMINER_CODE = checkTandS.Examiner.EMS_EXAMINER_CODE,
                    EMS_EXAMINER_NAME = checkTandS.Examiner.EMS_EXAMINER_NAME,
                    EMS_LAST_NAME = checkTandS.Examiner.EMS_LAST_NAME,
                    EMS_NATIONAL_ID = checkTandS.Examiner.EMS_NATIONAL_ID,
                    EMS_ADDRESS = $"{checkTandS.Examiner.EMS_WORK_ADD1}, {checkTandS.Examiner.EMS_WORK_ADD2}",
                    EMS_ACCOUNT_NO_FCA = checkTandS.Examiner.EMS_ACCOUNT_NO_FCA,
                    EMS_ACCOUNT_NO_ZWL = checkTandS.Examiner.EMS_ACCOUNT_NO_ZWL,
                    EMS_BANK_NAME_ZWL = checkTandS.Examiner.EMS_BANK_NAME_ZWL,
                    EMS_BANK_NAME_FCA = checkTandS.Examiner.EMS_BANK_NAME_FCA,
                    EMS_LEVEL_OF_EXAM_MARKED = checkTandS.Examiner.EMS_LEVEL_OF_EXAM_MARKED,
                    EMS_PAPER_CODE = checkTandS.Examiner.EMS_PAPER_CODE,
                    EMS_PHONE_HOME = checkTandS.Examiner.EMS_PHONE_HOME,
                    EMS_SUB_SUB_ID = checkTandS.Examiner.EMS_SUB_SUB_ID,
                    EMS_SUBKEY = checkTandS.EMS_SUBKEY,
                    EMS_WORK_ADD1 = checkTandS.Examiner.EMS_WORK_ADD1,
                    EMS_WORK_ADD2 = checkTandS.Examiner.EMS_WORK_ADD2,
                    EMS_WORK_ADD3 = checkTandS.Examiner.EMS_WORK_ADD3,
                    EMS_PURPOSEOFJOURNEY = checkTandS.EMS_PURPOSEOFJOURNEY,
                    EMS_VENUE = checkTandS.EMS_VENUE,
                    TANDSCODE = checkTandS.TANDSCODE,
                    SUBJECT_MANAGER_STATUS = checkTandS.SUBJECT_MANAGER_STATUS,
                    SUBJECT_MANAGER_DATE = checkTandS.SUBJECT_MANAGER_DATE,
                    SUBJECT_MANAGER_STATUS_BY = checkTandS.SUBJECT_MANAGER_STATUS_BY,
                    CENTRE_SUPERVISOR_DATE = checkTandS.CENTRE_SUPERVISOR_DATE,
                    CENTRE_SUPERVISOR_STATUS = checkTandS.CENTRE_SUPERVISOR_STATUS,
                    CENTRE_SUPERVISOR_STATUS_BY = checkTandS.CENTRE_SUPERVISOR_STATUS_BY,
                    CENTRE_SUPERVISOR_COMMENT = checkTandS.CENTRE_SUPERVISOR_COMMENT,
                    SUBJECT_MANAGER_COMMENT = checkTandS.SUBJECT_MANAGER_COMMENT,
                    EMS_TOTAL = checkTandS.EMS_TOTAL.ToString(),
                    TANDSADVANCE = checkTandS.TandSAdvance,
                    Date = checkTandS.DATE,
                };

                var trans = checkTandS.Examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == checkTandS.EMS_SUBKEY);

                if (trans != null)
                {
                    if (trans != null)
                    {
                        if (trans.RegisterStatus == "Present")
                        {
                            examinerData.SUBJECT_MANAGER_STATUS = "Recommended";
                            examinerData.SUBJECT_MANAGER_DATE = trans.RegisterStatusDate;
                            examinerData.SUBJECT_MANAGER_STATUS_BY = trans.RegisterStatusBy;
                            examinerData.SUBJECT_MANAGER_COMMENT = "Recommended";
                            
                        }
                        else
                        {
                            examinerData.SUBJECT_MANAGER_STATUS = "Pending";
                            examinerData.SUBJECT_MANAGER_DATE = trans.RegisterStatusDate;
                            examinerData.SUBJECT_MANAGER_STATUS_BY = trans.RegisterStatusBy;
                        }
                    }
                }
            }

            // Assign data to ViewBag
            ViewBag.TandSDetails = checkTandS?.TandSDetails;
            ViewBag.TandSFile = checkTandS?.TandSFiles;
            ViewBag.STATUS = checkTandS?.STATUS;
            ViewBag.TandSAdvance = checkTandS?.TandSAdvance;
            ViewBag.FEE_TEA = advanceFees?.FEE_TEA ?? 0.00m;
            ViewBag.FEE_BREAKFAST = advanceFees?.FEE_BREAKFAST ?? 0.00m;
            ViewBag.FEE_TRANSPORT = advanceFees?.FEE_TRANSPORT ?? 0.00m;
            ViewBag.FEE_ACCOMMODATION_RES = advanceFees?.FEE_ACCOMMODATION_RES ?? 0.00m;
            ViewBag.FEE_ACCOMMODATION_NONRES = advanceFees?.FEE_ACCOMMODATION_NONRES ?? 0.00m;
            ViewBag.FEE_LUNCH = advanceFees?.FEE_LUNCH ?? 0.00m;
            ViewBag.FEE_DINNER = advanceFees?.FEE_DINNER ?? 0.00m;
            ViewBag.FEE_OVERNIGHTALLOWANCE = advanceFees?.FEE_OVERNIGHTALLOWANCE ?? 0.00m;

            // Return view with data
            return View(examinerData);
        }


        [Authorize(Roles = "CentreSupervisor,OfficerSpecialNeeds,PeerReviewer")]
        [HttpPost]
        public async Task<IActionResult> Approve(string idNumber, string tandscode, string subKey, string examinerCode, string comment)
        {
            var currentUser = await _signInManager.UserManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var userRoles = await _userManager.GetRolesAsync(currentUser);

            if (userRoles == null)
            {
                return Json(new { success = false, message = "User roles not found" });
            }

            var checkTandS = await _andSRepository.GetOneTandS(idNumber,tandscode,subKey);
            if(checkTandS == null)
            {
                return Json(new { success = false, message = "Something went wrong contact Admin" });
            }

            var tands = new TandS
            {
                EMS_NATIONAL_ID = idNumber,
                EMS_SUBKEY = subKey,
                TANDSCODE = tandscode,
                EMS_EXAMINER_CODE = examinerCode
            };

            if (userRoles.Contains("SubjectManager") || userRoles.Contains("OfficerSpecialNeeds"))
            {
                tands.SUBJECT_MANAGER_STATUS = "Recommended";
                tands.SUBJECT_MANAGER_STATUS_BY = currentUser.UserName;
                tands.SUBJECT_MANAGER_DATE = DateTime.Now.ToString();
                tands.SUBJECT_MANAGER_COMMENT = comment;
                tands.CENTRE_SUPERVISOR_STATUS = "Approved";

                if (comment != null)
                {
                    tands.SUBJECT_MANAGER_COMMENT = comment;
                    tands.CENTRE_SUPERVISOR_COMMENT = "Claim Approved";
                }
                else
                {
                    tands.SUBJECT_MANAGER_COMMENT = "Claim Recommended";
                    tands.CENTRE_SUPERVISOR_COMMENT = "Claim Approved";
                }

                await _andSRepository.ApproveTandS(tands, "SubjectManager", currentUser.Id);
                return Json(new { success = true, message = "Recommended successful" });
            }
            else if (userRoles.Contains("CentreSupervisor"))
            {
                tands.SUBJECT_MANAGER_STATUS = "Recommended";
                tands.CENTRE_SUPERVISOR_STATUS = "Approved";
                tands.CENTRE_SUPERVISOR_STATUS_BY = currentUser.UserName;
                tands.CENTRE_SUPERVISOR_DATE = DateTime.Now.ToString();
                tands.CENTRE_SUPERVISOR_COMMENT = comment;


                if (comment != null)
                {
                    tands.SUBJECT_MANAGER_COMMENT = "Claim Recommended";
                    tands.CENTRE_SUPERVISOR_COMMENT = comment;
                }
                else
                {
                    tands.CENTRE_SUPERVISOR_COMMENT = "Claim Approved";
                    tands.SUBJECT_MANAGER_COMMENT = "Claim Recommended";
                }

                await _andSRepository.ApproveTandS(tands, "CentreSupervisor", currentUser.Id);
                return Json(new { success = true, message = "Approval successful" });
            }
            else if (userRoles.Contains("PeerReviewer"))
            {
                tands.ACCOUNTS_REVIEW = "Approved";
                tands.ACCOUNTS_REVIEW_BY = currentUser.UserName;
                tands.ACCOUNTS_REVIEW_DATE = DateTime.Now.ToString();
                tands.STATUS = "Approved";


                if (comment != null)
                {
                    tands.ACCOUNTS_REVIEW_COMMENT = comment;
                }
                else
                {
                    tands.ACCOUNTS_REVIEW_COMMENT = "Claim Reviewed";
                }

                await _andSRepository.ApproveTandS(tands, "PeerReviewer", currentUser.Id);
                return Json(new { success = true, message = "Reviewed successful" });

            }
            else if (userRoles.Contains("AssistantAccountant"))
            {
                tands.STATUS = "Approved";
                tands.STATUS_BY = currentUser.UserName;
                tands.STATUS_DATE = DateTime.Now.ToString();
                if (comment != null)
                {
                    tands.ACCOUNTS_REVIEW_COMMENT = comment;
                }
                else
                {
                    tands.ACCOUNTS_REVIEW_COMMENT = "Claim Approved";
                }
                await _andSRepository.ApproveTandS(tands, "AssistantAccountant", currentUser.Id);
                return Json(new { success = true, message = "Approved successful" });
            }
            else
            {
                return Json(new { success = false, message = "User does not have the required role" });
            }


        }


      


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reject(string idNumber, string tandscode, string subKey, string examinercode, string comment)
        {
            var currentUser = await _signInManager.UserManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var userRoles = await _userManager.GetRolesAsync(currentUser);

            if (userRoles == null)
            {
                return Json(new { success = false, message = "User roles not found" });
            }

            var tands = new TandS
            {
                EMS_NATIONAL_ID = idNumber,
                EMS_SUBKEY = subKey,
                TANDSCODE = tandscode,
                EMS_EXAMINER_CODE = examinercode
            };



            await _andSRepository.RejectTandS(tands, "SubjectManager", currentUser, comment);



            return Json(new { success = true, message = "Rejection successful" });
        }


        [Authorize]
        public async Task<IActionResult> ChangeTandS(string tandsCode, string idnumber, string subkey, string examinercode)
        {
            await _andSRepository.ChangeTandS(tandsCode, idnumber, subkey, examinercode);
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReturnTandS(string idNumber, string subjectcode, string papercode, string tandscode, string subKey, string examinercode, string comment)
        {
            var currentUser = await _signInManager.UserManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var userRoles = await _userManager.GetRolesAsync(currentUser);

            if (userRoles == null)
            {
                return Json(new { success = false, message = "User roles not found" });
            }

            var tands = new TandS
            {
                EMS_NATIONAL_ID = idNumber,
                EMS_SUBKEY = subKey,
                TANDSCODE = tandscode,
                EMS_EXAMINER_CODE = examinercode
            };

            if (userRoles.Contains("PeerReviewer"))
            {
                tands.ACCOUNTS_REVIEW = "Pending";
                tands.ACCOUNTS_REVIEW_BY = currentUser.UserName;
                tands.ACCOUNTS_REVIEW_DATE = DateTime.Now.ToString();
                tands.ACCOUNTS_STATUS = "Pending";
                tands.ACCOUNTS_DATE = DateTime.Now.ToString();
                tands.ACCOUNTS_STATUS_BY = currentUser.UserName;
                tands.ReturnBackBy = currentUser.UserName;
                tands.ReturnBackStatus = "Returned";
                tands.ReturnDate = DateTime.Now.ToString();


                if (comment != null)
                {
                    tands.ACCOUNTS_REVIEW_COMMENT = comment;
                    tands.ReturnComment = comment;
                }
                else
                {
                    tands.ACCOUNTS_REVIEW_COMMENT = "Returned Claim";
                }

                await _andSRepository.ApproveTandS(tands, "PeerReviewer", currentUser.Id);
                return Json(new { success = true, message = "Returning successful" });

            }

            else
            {
                return Json(new { success = false, message = "User does not have the required role" });
            }


        }

        [Authorize(Roles = "Accounts,PeerReviewer,Admin,SuperAdmin")]
        public async Task<IActionResult> TandSAccountsReview(string claimId, string nationalId, string examinerCode, string subKey, bool isSuccess = false)
        {



            var checktands = await _andSRepository.GetOneTandS(nationalId, claimId,subKey);


            List<TandSDetail> tandSDetails = new List<TandSDetail>();
            IEnumerable<TandSFile> tandSFileCollection = new List<TandSFile>();

            if (checktands != null)
            {


                decimal totalBusfare = 0;
                decimal totalAccommodation = 0;
                decimal totalLunch = 0;
                decimal totalDinner = 0;
                decimal totalTotal = 0;
                decimal adjtotalBusfare = 0;
                decimal adjAccommodation = 0;
                decimal adjLunch = 0;
                decimal adjDiner = 0;
                decimal adjTotal = 0;

                foreach (var detail in checktands.TandSDetails)
                {
                    // Convert the string values to decimal and add them to respective totals
                    totalBusfare += Convert.ToDecimal(detail.EMS_BUSFARE);
                    totalAccommodation += Convert.ToDecimal(detail.EMS_ACCOMMODATION);
                    totalLunch += Convert.ToDecimal(detail.EMS_LUNCH);
                    totalDinner += Convert.ToDecimal(detail.EMS_DINNER);
                    totalTotal += Convert.ToDecimal(detail.EMS_TOTAL);
                    adjtotalBusfare += Convert.ToDecimal(detail.ADJ_BUSFARE);
                    adjAccommodation += Convert.ToDecimal(detail.ADJ_ACCOMMODATION);
                    adjLunch += Convert.ToDecimal(detail.ADJ_LUNCH);
                    adjDiner += Convert.ToDecimal(detail.ADJ_DINNER);
                    adjTotal += Convert.ToDecimal(detail.ADJ_TOTAL);
                }

                // Assign totals to ViewBag
                ViewBag.TotalBusfare = totalBusfare;
                ViewBag.TotalAccommodation = totalAccommodation;
                ViewBag.TotalLunch = totalLunch;
                ViewBag.TotalDinner = totalDinner;
                ViewBag.TotalTotal = totalTotal;
                ViewBag.TotalAjdBusfare = adjtotalBusfare;
                ViewBag.TotalAdjAccommodation = adjAccommodation;
                ViewBag.TotalAdjLunch = adjLunch;
                ViewBag.TotalAdjDiner = adjDiner;
                ViewBag.TotalAdjTotal = adjTotal;
            }


            var advanceFees = await _advanceFeesRepository.GetAdvanceFees();
            decimal totalAdv = 0;

            if (checktands.TandSAdvance != null)
            {

                totalAdv += checktands.TandSAdvance.ADV_ACCOMMODATION_NONRES.GetValueOrDefault() * advanceFees.FEE_ACCOMMODATION_NONRES.GetValueOrDefault();

                totalAdv += checktands.TandSAdvance.ADV_ACCOMMODATION_RES.GetValueOrDefault() * advanceFees.FEE_ACCOMMODATION_RES.GetValueOrDefault();

                totalAdv += checktands.TandSAdvance.ADV_BREAKFAST.GetValueOrDefault() * advanceFees.FEE_BREAKFAST.GetValueOrDefault();

                totalAdv += checktands.TandSAdvance.ADV_DINNER.GetValueOrDefault() * advanceFees.FEE_DINNER.GetValueOrDefault();

                totalAdv += checktands.TandSAdvance.ADV_LUNCH.GetValueOrDefault() * advanceFees.FEE_LUNCH.GetValueOrDefault();

                totalAdv += checktands.TandSAdvance.ADV_OVERNIGHTALLOWANCE.GetValueOrDefault() * advanceFees.FEE_OVERNIGHTALLOWANCE.GetValueOrDefault();

                totalAdv += checktands.TandSAdvance.ADV_TEAS.GetValueOrDefault() * advanceFees.FEE_TEA.GetValueOrDefault();

                totalAdv += checktands.TandSAdvance.ADV_TRANSPORT.GetValueOrDefault() * advanceFees.FEE_TRANSPORT.GetValueOrDefault();


            }


        


            var examinerData = new TandSViewModel()
            {
                EMS_EXAMINER_CODE = checktands.Examiner.EMS_EXAMINER_CODE,
                EMS_EXAMINER_NAME = checktands.Examiner.EMS_EXAMINER_NAME,
                EMS_LAST_NAME = checktands.Examiner.EMS_LAST_NAME,
                EMS_NATIONAL_ID = checktands.Examiner.EMS_NATIONAL_ID,
                EMS_ADDRESS = checktands.Examiner.EMS_WORK_ADD1 + ", " + checktands.Examiner.EMS_WORK_ADD2,
                EMS_ACCOUNT_NO_FCA = checktands.Examiner.EMS_ACCOUNT_NO_FCA,
                EMS_ACCOUNT_NO_ZWL = checktands.Examiner.EMS_ACCOUNT_NO_ZWL,
                EMS_BANK_NAME_ZWL = checktands.Examiner.EMS_BANK_NAME_ZWL,
                EMS_BANK_NAME_FCA = checktands.Examiner.EMS_BANK_NAME_FCA,
                EMS_LEVEL_OF_EXAM_MARKED = checktands.Examiner.EMS_LEVEL_OF_EXAM_MARKED,
                EMS_PAPER_CODE = checktands.Examiner.EMS_PAPER_CODE,
                EMS_PHONE_HOME = checktands.Examiner.EMS_PHONE_HOME,
                EMS_SUB_SUB_ID = checktands.Examiner.EMS_SUB_SUB_ID,
                EMS_SUBKEY = checktands.EMS_SUBKEY,
                EMS_DISTRICT = checktands.Examiner.EMS_WORK_ADD3,
                EMS_WORK_ADD1 = checktands.Examiner.EMS_WORK_ADD1,
                EMS_WORK_ADD2 = checktands.Examiner.EMS_WORK_ADD2,
                EMS_WORK_ADD3 = checktands.Examiner.EMS_WORK_ADD3,




            };
            bool tandstatus = false;
            if (checktands != null)
            {
                examinerData.EMS_PURPOSEOFJOURNEY = checktands.EMS_PURPOSEOFJOURNEY;
                //examinerData.EMS_TOTAL = checktands.EMS_TOTAL;
                examinerData.EMS_VENUE = checktands.EMS_VENUE;
                examinerData.TANDSCODE = checktands.TANDSCODE;
              
                examinerData.CENTRE_SUPERVISOR_DATE = checktands.CENTRE_SUPERVISOR_DATE;
                examinerData.CENTRE_SUPERVISOR_STATUS = checktands.CENTRE_SUPERVISOR_STATUS;
                examinerData.CENTRE_SUPERVISOR_STATUS_BY = checktands.CENTRE_SUPERVISOR_STATUS_BY;
                examinerData.ACCOUNTS_STATUS = checktands.ACCOUNTS_STATUS;
                examinerData.ACCOUNTS_STATUS_BY = checktands.ACCOUNTS_STATUS_BY;
                examinerData.ACCOUNTS_DATE = checktands.ACCOUNTS_DATE;
                examinerData.CENTRE_SUPERVISOR_COMMENT = checktands.CENTRE_SUPERVISOR_COMMENT;
                
                examinerData.ACCOUNTS_REVIEW = checktands.ACCOUNTS_REVIEW;
                examinerData.ACCOUNTS_REVIEW_DATE = checktands.ACCOUNTS_REVIEW_DATE;
                examinerData.ACCOUNTS_REVIEW_BY = checktands.ACCOUNTS_REVIEW_BY;
                examinerData.ACCOUNTS_REVIEW_COMMENT = checktands.ACCOUNTS_REVIEW_COMMENT;
                examinerData.ReturnBackStatus = checktands.ReturnBackStatus;
                examinerData.ReturnBackBy = checktands.ReturnBackBy;
                examinerData.ReturnComment = checktands.ReturnComment;
                examinerData.ReturnDate = checktands.ReturnDate;
                tandstatus = true;
                examinerData.EMS_TOTAL = checktands.EMS_TOTAL.ToString();
                examinerData.ADJ_TOTAL = checktands.ADJ_TOTAL.ToString();
                examinerData.Date = checktands.DATE;
                var trans = checktands.Examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == checktands.EMS_SUBKEY);

                if (trans != null)
                {
                    if (trans != null)
                    {
                        if (trans.RegisterStatus == "Present")
                        {
                            examinerData.SUBJECT_MANAGER_STATUS = "Recommended";
                            examinerData.SUBJECT_MANAGER_DATE = trans.RegisterStatusDate;
                            examinerData.SUBJECT_MANAGER_STATUS_BY = trans.RegisterStatusBy;
                            examinerData.SUBJECT_MANAGER_COMMENT = "Recommended";
                        }
                        else
                        {
                            examinerData.SUBJECT_MANAGER_STATUS = "Pending";
                            examinerData.SUBJECT_MANAGER_DATE = trans.RegisterStatusDate;
                            examinerData.SUBJECT_MANAGER_STATUS_BY = trans.RegisterStatusBy;
                        }
                    }
                }

            }





            ViewBag.TANDSTATUS = tandstatus;
            ViewBag.TandSDetails = checktands?.TandSDetails;
            ViewBag.TandSFile = checktands?.TandSFiles;
            ViewBag.AccountsStatus = checktands?.ACCOUNTS_STATUS;
            ViewBag.TandSAdvance = checktands?.TandSAdvance;
            ViewBag.FEE_TEA = advanceFees?.FEE_TEA ?? 0.00m;  // Decimal default value
            ViewBag.FEE_BREAKFAST = advanceFees?.FEE_BREAKFAST ?? 0.00m;
            ViewBag.FEE_TRANSPORT = advanceFees?.FEE_TRANSPORT ?? 0.00m;
            ViewBag.FEE_ACCOMMODATION_RES = advanceFees?.FEE_ACCOMMODATION_RES ?? 0.00m;
            ViewBag.FEE_ACCOMMODATION_NONRES = advanceFees?.FEE_ACCOMMODATION_NONRES ?? 0.00m;
            ViewBag.FEE_LUNCH = advanceFees?.FEE_LUNCH ?? 0.00m;
            ViewBag.FEE_DINNER = advanceFees?.FEE_DINNER ?? 0.00m;  // String default
            ViewBag.FEE_OVERNIGHTALLOWANCE = advanceFees?.FEE_OVERNIGHTALLOWANCE ?? 0.00m;
            ViewBag.TotalAdv = totalAdv;

            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Claim Adjusted  Successfully.";
                //return RedirectToAction("TandSAccountsReview", new { claimId = claimId, nationalId = nationalId, examinerCode = examinerCode, subKey = subKey, isSuccess = false });

                return Redirect($"/Transcribers/TandSAccountsReview?claimId={claimId}&nationalId={nationalId}&examinerCode={examinerCode}&subKey={subKey}&isSuccess=false");

            }


            return View(examinerData);

        }

        [Authorize]
        public async Task<IActionResult> GetUsers(string examCode = "", string activity = "")
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            IEnumerable<ApplicationUserViewModel> model = new List<ApplicationUserViewModel>();
            List<ApplicationUserViewModel> modelList = new List<ApplicationUserViewModel>();
  

            List<ApplicationUser> users = new List<ApplicationUser>();

            var trans = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID.StartsWith(examCode) && a.EMS_ACTIVITY == activity && (a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT"
            || a.EMS_ECT_EXAMINER_CAT_CODE == "S" || a.EMS_ECT_EXAMINER_CAT_CODE == "I" || a.EMS_ECT_EXAMINER_CAT_CODE == "A")).ToListAsync();

            foreach (var item in trans)
            {
                var user = await _context.Users.Include(a => a.Examiner).FirstOrDefaultAsync(a => a.IDNumber == item.EMS_NATIONAL_ID && a.EMS_SUBKEY == item.EMS_SUBKEY);

                users.Add(user);
            }

            ////var usersList = await _userRepository.UsersGetAll();



            var filteredUsers = new List<ApplicationUser>();


            var allowedRoles = new List<string> { "PBT", "BT", "S", "I","A"};

            foreach (var user in users)
            {
                var userRoles1 = await _userManager.GetRolesAsync(user);

                // Check if the user does not have any of the excluded roles
                if (userRoles1.Any(role => allowedRoles.Contains(role)))
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
                

                });



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


     


        [Authorize(Roles = "SubjectManager,CentreSupervisor,Admin,OfficerSpecialNeeds,SuperAdmin")]
        public async Task<IActionResult> TranscribersRegister(string examCode = "", string activity = "")
        {

            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,

                    Activity = activity,
                };


                HttpContext.Session.SetObjectAsJson("Session", userSession);
            }
            else
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
            }

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "Session Expired",
                    text = " You have been logged out for security reasons login again.",
                    showConfirmButton = true,
                    confirmButtonColor = "#ffc107", // Warning color
                                                    //timer = 5000, // Auto-close after 5 seconds
                    timerProgressBar = true
                });



                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
            }

            ViewBag.ExamCode = userSession.ExamCode;

            ViewBag.Activity = userSession.Activity;


            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdatePresence([FromBody] UpdatePresenceRequest request)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

         

            var userRoles = await _userManager.GetRolesAsync(currentUser);


            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            var examiner = await _registerRepository.GetExaminer(request.SubKey);
            if (examiner == null)
            {
                return NotFound("Examiner not found.");
            }

            examiner.RegisterStatus = request.IsPresent ? "Present" : "Absent";

            examiner.RegisterStatusDate = DateTime.Now.ToString();

            if (examiner.RegisterStatus == "Present")
            {
                examiner.RecommendedStatus = "Recommended";
                examiner.RecommendedBy = currentUser.UserName;
                examiner.RecommendedDate = DateTime.Now.ToString();
                examiner.IsPresent = true;

            }
            else
            {
                examiner.RecommendedStatus = "Pending";
                examiner.RecommendedDate = DateTime.Now.ToString();
                examiner.RecommendedBy = currentUser.UserName;
                examiner.IsPresent= false;
            }



            await _registerRepository.MarkPresent(examiner, currentUser.Id);

            return Ok();
        }



    
    public async Task<int> InitializeNextExaminerNumber(string subsubId, string papercode)
        {
            var componentList = await _examinerRepository.GetAllExaminers();
            componentList = componentList.Where(a => a.EMS_SUB_SUB_ID == subsubId && a.EMS_PAPER_CODE == papercode);



            // Get the maximum examinerNumber, increment by 1, and convert back to string
            int maxExaminerNumber = componentList
                .Where(a => int.TryParse(a.EMS_EXAMINER_NUMBER, out _)) // Ensure valid integer strings only
                .Select(a => int.Parse(a.EMS_EXAMINER_NUMBER))
                .DefaultIfEmpty(0) // Default to 0 if list is empty or no valid numbers
                .Max() + 1;

            ;



            return maxExaminerNumber;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ResetPasswordToDefault(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var examiner = await _examinerRepository.GetExaminerRecord(user.IDNumber);
            if (examiner == null)
            {
                return NotFound("Examiner not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Check if the user does not have 'accounts', 'admin', or 'subjectmanager' roles
            var allowedRoles = new[] { "PBT", "BT", "S", "I","A"};
            bool isAllowed= roles.Any(role => allowedRoles.Contains(role));
            if (isAllowed)
            {

        
                string defaultPassword = GenerateDefaultPassword(user);
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
      

            return Ok(new { message = "Password Reset Succefully" });
        }
        private string GenerateDefaultPassword(ApplicationUser user)
        {
            string password = $"{user.UserName.ToLower()}900101.*";
            return password;
        }

        public async Task<int> InitializeNextTrainingCode()
        {
            // Fetch the maximum training code as a string
            var userTrainingMaxStr = await _maxExaminerCodeRepository.GetMaxExaminerCodeFromDatabase();

            // Convert the string to an integer
            if (userTrainingMaxStr != null && int.TryParse(userTrainingMaxStr, out int userTrainingMax))
            {
                return userTrainingMax + 1;
            }

            return 1900001;

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

        private async Task<BankData> GetBankDataAsync(string bankCode, string branchCode)
        {
            return await _banksRepository.GetBankDataByParameter(bankCode, branchCode);
        }


        [Authorize]
        public async Task<IActionResult> ScriptsTranscribed(string examCode)
        {


            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);


            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,
           
                };

                HttpContext.Session.SetObjectAsJson("Session", userSession);
            }
            else
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
            }


            if (userSession == null)
                    {
                        // Store SweetAlert configuration in TempData
                        TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                        {
                            icon = "warning",  // Changed to warning icon
                            title = "ACCESS DENIED!",
                            text = "You are not authorized for this activity. Please check your account credentials. You will be logged out for security reasons.",
                            showConfirmButton = true,
                            confirmButtonColor = "#ffc107", // Warning color
                                                            //timer = 5000, // Auto-close after 5 seconds
                            timerProgressBar = true,
                            customClass = new
                            {
                                container = "swal2-flicker", // Applies to the entire modal
                                title = "swal2-title-danger" // Applies only to the title
                            }
                        });

                        await _signInManager.SignOutAsync();
                        return Redirect("/Identity/Account/Login");
                    }

                    ViewBag.ExamCode = userSession.ExamCode;

            return View();
        }







        [Authorize]
        public async Task<IActionResult> GetTransactionData(string examCode = "")
        {
            IEnumerable<ScriptMarkedViewModel> model = new List<ScriptMarkedViewModel>();
            List<ScriptMarkedViewModel> modelList = new List<ScriptMarkedViewModel>();

            var entriesData = new EntriesData();
            IEnumerable<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();

            examiners = await _context.EXAMINER_TRANSACTIONS.Where(em => em.EMS_SUBKEY.StartsWith(examCode) && (em.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || em.EMS_ECT_EXAMINER_CAT_CODE == "BT" || em.EMS_ECT_EXAMINER_CAT_CODE == "S" || em.EMS_ECT_EXAMINER_CAT_CODE == "I"))
                    .Include(a => a.Examiner)
                .ToListAsync();

            modelList = examiners.Select(ex => new ScriptMarkedViewModel
            {
                ExaminerCode = ex.EMS_EXAMINER_CODE,
                FirstName = ex.Examiner.EMS_EXAMINER_NAME,
                LastName = ex.Examiner.EMS_LAST_NAME,
                IdNumber = ex.EMS_NATIONAL_ID,
                Subject = ex.EMS_SUB_SUB_ID + "/" + ex.EMS_PAPER_CODE,
                SubKey = ex.EMS_SUBKEY,

                Category = ex.EMS_ECT_EXAMINER_CAT_CODE,
                ScriptMarked = ex.SCRIPTS_MARKED ?? 0, // Default to 0 if null
                PaperCode = ex.EMS_PAPER_CODE,
        
                Perfomance = ex.Examiner.EMS_PERFORMANCE_INDEX,
                IsPresent = ex.IsPresent,
                ScriptApportioned = entriesData?.AppointedScripts ?? "0"
            }).ToList();


            model = modelList
    .GroupBy(a => a.IdNumber)
    .Select(g => g.First())
    .OrderBy(a => a.ExaminerNumber)
    .AsQueryable();


            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[7][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[7][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Apply search filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
    (p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.LastName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.IdNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.BMS?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.ExaminerNumber?.ToLower().Contains(searchValue.ToLower()) ?? false)
);

            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumnDir == "asc")
                {
                    model = model.OrderBy(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
                else
                {
                    model = model.OrderByDescending(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
            }


            // Get total records count after filtering
            var totalRecords = model.Count();

            // Apply pagination
            var data = model.Skip(skip).Take(pageSize).ToList();

            // Prepare the response
            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                data,

            };

            return Ok(jsonData);



        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateTransPresence([FromBody] UpdateTranscactionPresenceRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.idNumber))
            {
                return BadRequest(new { success = false, message = "Invalid request data." });
            }

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized(new { success = false, message = "User is not authenticated." });
            }

            try
            {
                var transaction = await _context.EXAMINER_TRANSACTIONS
              .FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == request.idNumber && e.EMS_SUBKEY == request.SubKey);


                var message = "";
                transaction.IsPresent = request.IsPresent ? true : false;
                transaction.IsPresentBy = currentUser.Id;
                transaction.IsPresentDate = DateTime.Now.ToString();
                _context.EXAMINER_TRANSACTIONS.Update(transaction);


                await _context.SaveChangesAsync(currentUser.Id);

                if (transaction.IsPresent)
                {
                    message = "Presence updated successfully.";
                }
                else
                {
                    message = "Absent updated successfully.";
                }
                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }




        [Authorize]
        public async Task<IActionResult> GetTranscribedData(string examCode = "")
        {
            IEnumerable<ScriptMarkedViewModel> model = new List<ScriptMarkedViewModel>();
            List<ScriptMarkedViewModel> modelList = new List<ScriptMarkedViewModel>();


            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var entriesData = new EntriesData();
            IEnumerable<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();


            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var filteredExaminers = new List<ExaminerScriptsMarked>();

       

                if (!string.IsNullOrEmpty(examCode))
                {
                    examiners = await _context.EXAMINER_TRANSACTIONS.Where(em => em.EMS_SUBKEY.StartsWith(examCode) && (em.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || em.EMS_ECT_EXAMINER_CAT_CODE == "BT" || em.EMS_ECT_EXAMINER_CAT_CODE == "S" || em.EMS_ECT_EXAMINER_CAT_CODE == "I"))
                    .Include(a => a.Examiner)
                    .ToListAsync();
             

                }
              



            modelList = examiners.Select(ex => new ScriptMarkedViewModel
            {
                ExaminerCode = ex.EMS_EXAMINER_CODE,
                FirstName = ex.Examiner.EMS_EXAMINER_NAME,
                LastName = ex.Examiner.EMS_LAST_NAME,
                IdNumber = ex.EMS_NATIONAL_ID,
                Subject = ex.EMS_SUB_SUB_ID + "/" + ex.EMS_PAPER_CODE,
                SubKey = ex.EMS_SUBKEY,
 
                Category = ex.EMS_ECT_EXAMINER_CAT_CODE,
                ScriptMarked = ex.SCRIPTS_MARKED ?? 0, // Default to 0 if null
                PaperCode = ex.EMS_PAPER_CODE,

                Perfomance = ex.EMS_PERFORMANCE_INDEX,
                IsPresent = ex.IsPresent,
                ScriptApportioned = entriesData?.AppointedScripts ?? "0",
                Status = ex.EMS_APPROVED_STATUS
            }).ToList();


            // Check if ALL examiners are either Approved OR not Present
        


            model = modelList
    .GroupBy(a => a.IdNumber)
    .Select(g => g.First())
    .OrderBy(a => a.ExaminerNumber)
    .AsQueryable();


            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[7][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[7][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Apply search filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
    (p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.LastName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.IdNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.BMS?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.ExaminerNumber?.ToLower().Contains(searchValue.ToLower()) ?? false)
);

            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumnDir == "asc")
                {
                    model = model.OrderBy(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
                else
                {
                    model = model.OrderByDescending(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
            }


            // Get total records count after filtering
            var totalRecords = model.Count();

            // Apply pagination
            var data = model.Skip(skip).Take(pageSize).ToList();

            // Prepare the response
            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                data,
             
            };

            return Ok(jsonData);



        }


        [Authorize(Roles = "PMS,DPMS,RPMS,SubjectManager,CentreSupervisor")]
        public async Task<IActionResult> ScriptsTranscibedApproval(string examCode)
        {


            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var userSession = new SessionModel();
   

            if (userRoles != null && userRoles.Contains("SubjectManager") || userRoles.Contains("CentreSupervisor"))
            {
                if (!string.IsNullOrEmpty(examCode))
                {
                    userSession = new SessionModel()
                    {
                        ExamCode = examCode,
                       
                    };

                
                    HttpContext.Session.SetObjectAsJson("Session", userSession);
                }
                else
                {
                    userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");

                }

            }


            if (userSession != null)
            {
                ViewBag.ExamCode = userSession.ExamCode;
       
            }
            else
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "ACCESS DENIED!",
                    text = "You are not authorized for this activity. Please check your account credentials. You will be logged out for security reasons.",
                    showConfirmButton = true,
                    confirmButtonColor = "#ffc107", // Warning color
                                                    //timer = 5000, // Auto-close after 5 seconds
                    timerProgressBar = true,
                    customClass = new
                    {
                        container = "swal2-flicker", // Applies to the entire modal
                        title = "swal2-title-danger" // Applies only to the title
                    }
                });



                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
            }



            return View();
        }


        [Authorize]
        public async Task<IActionResult> GetApprovalData(string examCode = "")
        {
            IEnumerable<ScriptMarkedViewModel> model = new List<ScriptMarkedViewModel>();
            List<ScriptMarkedViewModel> modelList = new List<ScriptMarkedViewModel>();
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var filteredExaminers = new List<ExaminerScriptsMarked>();


            var pending = 0;
            var approved = 0;
            var totalCount = 0;
            var apportioned = 0;
            var pirates = 0;
            var absent = 0;
            int totalscripts = 0;




       
         if (userRoles != null && userRoles.Contains("SubjectManager"))
            {

              var  examiners = await _context.EXAMINER_TRANSACTIONS
                      .Where(em => em.EMS_SUBKEY.StartsWith(examCode)
                                   && em.SCRIPTS_MARKED > 0
                                && em.RegisterStatus == "Present" && em.IsPresent  && (em.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || em.EMS_ECT_EXAMINER_CAT_CODE == "BT" || em.EMS_ECT_EXAMINER_CAT_CODE == "S" || em.EMS_ECT_EXAMINER_CAT_CODE == "I"))
                
                      .GroupJoin(
                          _context.EXM_EXAMINER_MASTER,
                          em => em.EMS_NATIONAL_ID,
                          examiner => examiner.EMS_NATIONAL_ID,
                          (em, examinerGroup) => new { em, examinerGroup })
                      .SelectMany(
                          x => x.examinerGroup.DefaultIfEmpty(),
                          (x, examiner) => new ExaminerScriptsMarked
                          {
                          
                              EMS_PAPER_CODE = x.em.EMS_PAPER_CODE,
                              EMS_SUBKEY = x.em.EMS_SUBKEY,
                              EMS_SUB_SUB_ID = x.em.EMS_SUB_SUB_ID,
                              EMS_EXAMINER_CODE = x.em.EMS_EXAMINER_CODE,
                              EMS_NATIONAL_ID = x.em.EMS_NATIONAL_ID,
                              SCRIPTS_MARKED = x.em.SCRIPTS_MARKED,
                         
                              EMS_ECT_EXAMINER_CAT_CODE = x.em.EMS_ECT_EXAMINER_CAT_CODE,
                              Examiner = examiner,
                              EMS_COMPILED_STATUS = x.em.EMS_COMPILED_STATUS,
                              EMS_APPROVED_STATUS = x.em.EMS_APPROVED_STATUS,
                              EMS_CENTRE_SUPERVISOR_STATUS = x.em.EMS_CENTRE_SUPERVISOR_STATUS,
                              EMS_CERTIFIED_STATUS = x.em.EMS_CERTIFIED_STATUS,
                              IsPresent = x.em.IsPresent,
                              EMS_ACTIVITY = x.em.EMS_ACTIVITY,
                              EMS_PERFORMANCE_INDEX = x.em.EMS_PERFORMANCE_INDEX
                          })
                      .ToListAsync();

                //var examiners = await _transactionRepository.CheckExaminerInTransactions(examCode, subjectCode, paperCode, regionCode);
                var presentExaminers = await _context.EXAMINER_TRANSACTIONS
                        .Where(em =>  em.RegisterStatus == "Present" && em.EMS_SUBKEY.StartsWith(examCode) && em.IsPresent && (em.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || em.EMS_ECT_EXAMINER_CAT_CODE == "BT" || em.EMS_ECT_EXAMINER_CAT_CODE == "S" || em.EMS_ECT_EXAMINER_CAT_CODE == "I"))
                        .ToListAsync(); ;

                pending = examiners.Where(a => a.EMS_APPROVED_STATUS == "Approved" && a.EMS_COMPILED_STATUS == "Compiled" && a.EMS_CERTIFIED_STATUS == "Pending").Count();
                approved = examiners.Where(a => a.EMS_APPROVED_STATUS == "Approved" && a.EMS_COMPILED_STATUS == "Compiled" && a.EMS_CERTIFIED_STATUS == "Certified").Count();
                totalCount = presentExaminers.Count();
                var newTransactionData = new List<ExaminerScriptsMarked>();
                foreach (var item in examiners)
                {
                    totalscripts += item.SCRIPTS_MARKED.GetValueOrDefault();
                    if (item.EMS_COMPILED_STATUS == "Compiled" && item.EMS_APPROVED_STATUS == "Approved" && item.EMS_CERTIFIED_STATUS == "Pending")
                    {
                        var newTranscation = new ExaminerScriptsMarked()
                        {
                            EMS_EXAMINER_CODE = item.EMS_EXAMINER_CODE,
                            EMS_NATIONAL_ID = item.EMS_NATIONAL_ID,
                            EMS_SUBKEY = item.EMS_SUBKEY,
                            SCRIPTS_MARKED = item.SCRIPTS_MARKED,
                            Examiner = item.Examiner,
                            EMS_COMPILED_STATUS = item.EMS_COMPILED_STATUS,
                            EMS_COMPILED_BY = item.EMS_COMPILED_BY,
                            EMS_COMPILED_DATE = item.EMS_COMPILED_DATE,
                            EMS_APPROVED_STATUS = item.EMS_APPROVED_STATUS,
                            EMS_APPROVED_DATE = item.EMS_APPROVED_DATE,
                            EMS_APPROVED_BY = item.EMS_APPROVED_BY,
                            EMS_CERTIFIED_BY = item.EMS_CERTIFIED_BY,
                            EMS_CERTIFIED_DATE = item.EMS_CERTIFIED_DATE,
                            EMS_CERTIFIED_STATUS = item.EMS_CERTIFIED_STATUS,
                            EMS_CENTRE_SUPERVISOR_DATE = item.EMS_CENTRE_SUPERVISOR_DATE,
                            EMS_CENTRE_SUPERVISOR_NAME = item.EMS_CENTRE_SUPERVISOR_NAME,
                            EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS,
                            EMS_SUB_SUB_ID = item.EMS_SUB_SUB_ID,
                            EMS_ECT_EXAMINER_CAT_CODE = item.EMS_ECT_EXAMINER_CAT_CODE,
                            EMS_PAPER_CODE = item.EMS_PAPER_CODE,
                           
                            EMS_ACTIVITY = item.EMS_ACTIVITY,
                            IsPresent = item.IsPresent,
                            EMS_PERFORMANCE_INDEX = item.EMS_PERFORMANCE_INDEX
                        };
                        var activityName = await _activityRepository.GetActivityByValueAsync(item.EMS_ACTIVITY);
                        if (activityName != null)
                        {
                            newTranscation.EMS_ACTIVITY = activityName.Text;
                        }
                        filteredExaminers.Add(newTranscation);
                    }

                }


                foreach (var ex in filteredExaminers)
                {
                    if (ex.Examiner != null)
                    {

                        modelList.Add(new ScriptMarkedViewModel
                        {
                            ExaminerCode = ex.EMS_EXAMINER_CODE,
                            FirstName = ex.Examiner.EMS_EXAMINER_NAME,
                            LastName = ex.Examiner.EMS_LAST_NAME,
                            IdNumber = ex.EMS_NATIONAL_ID,
                            Subject = ex.EMS_SUB_SUB_ID.Substring(3) + "/" + ex.EMS_PAPER_CODE,
                            SubKey = ex.EMS_SUBKEY,
                       
                            Category = ex.EMS_ECT_EXAMINER_CAT_CODE,
                            ScriptMarked = ex.SCRIPTS_MARKED,
                            PaperCode = ex.EMS_PAPER_CODE,
                            Activity = ex.EMS_ACTIVITY,
                            Status = ex.EMS_CERTIFIED_STATUS,
                            IsPresent = ex.IsPresent,
                            Perfomance = ex.EMS_PERFORMANCE_INDEX


                        });
                    }
                }

            }
            else if (userRoles != null && userRoles.Contains("CentreSupervisor"))
            {
                var examiners = await _context.EXAMINER_TRANSACTIONS
                     .Where(em => em.EMS_SUBKEY.StartsWith(examCode)
                                  && em.SCRIPTS_MARKED > 0
                                  && em.RegisterStatus == "Present" && em.IsPresent && (em.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || em.EMS_ECT_EXAMINER_CAT_CODE == "BT" || em.EMS_ECT_EXAMINER_CAT_CODE == "S" || em.EMS_ECT_EXAMINER_CAT_CODE == "I"))

                     .GroupJoin(
                         _context.EXM_EXAMINER_MASTER,
                         em => em.EMS_NATIONAL_ID,
                         examiner => examiner.EMS_NATIONAL_ID,
                         (em, examinerGroup) => new { em, examinerGroup })
                     .SelectMany(
                         x => x.examinerGroup.DefaultIfEmpty(),
                         (x, examiner) => new ExaminerScriptsMarked
                         {

                             EMS_PAPER_CODE = x.em.EMS_PAPER_CODE,
                             EMS_SUBKEY = x.em.EMS_SUBKEY,
                             EMS_SUB_SUB_ID = x.em.EMS_SUB_SUB_ID,
                             EMS_EXAMINER_CODE = x.em.EMS_EXAMINER_CODE,
                             EMS_NATIONAL_ID = x.em.EMS_NATIONAL_ID,
                             SCRIPTS_MARKED = x.em.SCRIPTS_MARKED,

                             EMS_ECT_EXAMINER_CAT_CODE = x.em.EMS_ECT_EXAMINER_CAT_CODE,
                             Examiner = examiner,
                             EMS_COMPILED_STATUS = x.em.EMS_COMPILED_STATUS,
                             EMS_APPROVED_STATUS = x.em.EMS_APPROVED_STATUS,
                             EMS_CENTRE_SUPERVISOR_STATUS = x.em.EMS_CENTRE_SUPERVISOR_STATUS,
                             EMS_CERTIFIED_STATUS = x.em.EMS_CERTIFIED_STATUS,
                             IsPresent = x.em.IsPresent,
                             EMS_ACTIVITY = x.em.EMS_ACTIVITY,
                             EMS_PERFORMANCE_INDEX = x.em.EMS_PERFORMANCE_INDEX
                         })
                     .ToListAsync();

                //var examiners = await _transactionRepository.CheckExaminerInTransactions(examCode, subjectCode, paperCode, regionCode);
                var presentExaminers = await _context.EXAMINER_TRANSACTIONS
                        .Where(em => em.EMS_SUBKEY.StartsWith(examCode) && em.RegisterStatus == "Present"  && em.IsPresent && (em.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || em.EMS_ECT_EXAMINER_CAT_CODE == "BT" || em.EMS_ECT_EXAMINER_CAT_CODE == "S" || em.EMS_ECT_EXAMINER_CAT_CODE == "I"))
                        .ToListAsync(); ;



                pending = examiners.Where(a => a.EMS_APPROVED_STATUS == "Approved" && a.EMS_COMPILED_STATUS == "Compiled" && a.EMS_CERTIFIED_STATUS == "Certified" && a.EMS_CENTRE_SUPERVISOR_STATUS == "Pending").Count();
                approved = examiners.Where(a => a.EMS_APPROVED_STATUS == "Approved" && a.EMS_COMPILED_STATUS == "Compiled" && a.EMS_CERTIFIED_STATUS == "Certified" && a.EMS_CENTRE_SUPERVISOR_STATUS == "Approved").Count();
                totalCount = presentExaminers.Count();
                var newTransactionData = new List<ExaminerScriptsMarked>();
                foreach (var item in examiners)
                {
                    totalscripts += item.SCRIPTS_MARKED.GetValueOrDefault();
                    if (item.EMS_COMPILED_STATUS == "Compiled" && item.EMS_APPROVED_STATUS == "Approved" && item.EMS_CERTIFIED_STATUS == "Certified" && item.EMS_CENTRE_SUPERVISOR_STATUS == "Pending")
                    {
                        var newTranscation = new ExaminerScriptsMarked()
                        {
                            EMS_EXAMINER_CODE = item.EMS_EXAMINER_CODE,
                            EMS_NATIONAL_ID = item.EMS_NATIONAL_ID,
                            EMS_SUBKEY = item.EMS_SUBKEY,
                            SCRIPTS_MARKED = item.SCRIPTS_MARKED,
                            Examiner = item.Examiner,
                            EMS_COMPILED_STATUS = item.EMS_COMPILED_STATUS,
                            EMS_COMPILED_BY = item.EMS_COMPILED_BY,
                            EMS_COMPILED_DATE = item.EMS_COMPILED_DATE,
                            EMS_APPROVED_STATUS = item.EMS_APPROVED_STATUS,
                            EMS_APPROVED_DATE = item.EMS_APPROVED_DATE,
                            EMS_APPROVED_BY = item.EMS_APPROVED_BY,
                            EMS_CERTIFIED_BY = item.EMS_CERTIFIED_BY,
                            EMS_CERTIFIED_DATE = item.EMS_CERTIFIED_DATE,
                            EMS_CERTIFIED_STATUS = item.EMS_CERTIFIED_STATUS,
                            EMS_CENTRE_SUPERVISOR_DATE = item.EMS_CENTRE_SUPERVISOR_DATE,
                            EMS_CENTRE_SUPERVISOR_NAME = item.EMS_CENTRE_SUPERVISOR_NAME,
                            EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS,
                            EMS_SUB_SUB_ID = item.EMS_SUB_SUB_ID,
                            EMS_ECT_EXAMINER_CAT_CODE = item.EMS_ECT_EXAMINER_CAT_CODE,
                            EMS_PAPER_CODE = item.EMS_PAPER_CODE,
                   
                            EMS_ACTIVITY = item.EMS_ACTIVITY,
                            IsPresent = item.IsPresent,
                            EMS_PERFORMANCE_INDEX = item.EMS_PERFORMANCE_INDEX

                        };
                        var activityName = await _activityRepository.GetActivityByValueAsync(item.EMS_ACTIVITY);
                        if (activityName != null)
                        {
                            newTranscation.EMS_ACTIVITY = activityName.Text;
                        }
                        filteredExaminers.Add(newTranscation);

                    }

                }

                foreach (var ex in filteredExaminers)
                {

                    if (ex.Examiner != null)
                    {
                        modelList.Add(new ScriptMarkedViewModel
                        {
                            ExaminerCode = ex.EMS_EXAMINER_CODE,
                            FirstName = ex.Examiner.EMS_EXAMINER_NAME,
                            LastName = ex.Examiner.EMS_LAST_NAME,
                            IdNumber = ex.EMS_NATIONAL_ID,
                            Subject = ex.EMS_SUB_SUB_ID.Substring(3) + "/" + ex.EMS_PAPER_CODE,
                            SubKey = ex.EMS_SUBKEY,
                            BMS = ex.EMS_EXM_SUPERORD,
                            ExaminerNumber = ex.EMS_EXAMINER_NUMBER,
                            Category = ex.EMS_ECT_EXAMINER_CAT_CODE,
                            ScriptMarked = ex.SCRIPTS_MARKED,
                            PaperCode = ex.EMS_PAPER_CODE,
                            Activity = ex.EMS_ACTIVITY,
                            Status = ex.EMS_CENTRE_SUPERVISOR_STATUS,
                            IsPresent = ex.IsPresent,
                            Perfomance = ex.EMS_PERFORMANCE_INDEX


                        });
                    }

                }

            }

            model = modelList.OrderBy(a => a.ExaminerNumber).AsQueryable();

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Apply search filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
     (p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
     (p.LastName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
     (p.IdNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
     (p.BMS?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
     (p.ExaminerNumber?.ToLower().Contains(searchValue.ToLower()) ?? false)
 );

            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumnDir == "asc")
                {
                    model = model.OrderBy(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
                else
                {
                    model = model.OrderByDescending(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
            }

            double percentageApproved = 0;

            if (totalCount > 0)
            {
                percentageApproved = (double)approved / totalCount * 100;
            }

            string pendingPercentageString = percentageApproved.ToString("0.00") + "%";
            // Get total records count after filtering
            var totalRecords = model.Count();

            // Apply pagination
            var data = model.Skip(skip).Take(pageSize).ToList();

            // Prepare the response
            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                pendingPercentageString,
                totalCount,
                approved,
                pending,
                apportioned,
                absent,
                pirates,
                totalscripts,
                data
            };

            return Ok(jsonData);



        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveData([FromBody] List<ScriptMarkedViewModel> data)
        {


            var checkScriptsValidation = data.Where(a => a.IsPresent && a.ScriptMarked == 0);
            if (checkScriptsValidation.Any())
            {
                return Json(new { success = false, message = "Some present examiners have 0 transcribed scripts." });
            }

            var checkPerfomance = data.Where(a => a.IsPresent && a.Perfomance == "");
            if (checkPerfomance.Any())
            {
                return Json(new { success = false, message = "Some present examiners have no perfomance." });
            }

            List<ExaminerScriptsMarked> examersScriptsMarked = new List<ExaminerScriptsMarked>();

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);


            if (data != null)
            {

                foreach (var item in data)
                {

                    var newTransction = new ExaminerScriptsMarked()
                    {
                        EMS_NATIONAL_ID = item.IdNumber,
                        EMS_SUBKEY = item.SubKey,
                   
                        SCRIPTS_MARKED = item.ScriptMarked,
                      
                        EMS_COMPILED_BY = currentUser.UserName,
                        EMS_COMPILED_DATE = DateTime.Now.ToString(),
                        EMS_COMPILED_STATUS = "Compiled",
                        EMS_APPROVED_DATE = DateTime.Now.ToString(),
                        EMS_APPROVED_BY = currentUser.UserName,
                        EMS_APPROVED_STATUS = "Approved",
                        EMS_CERTIFIED_BY = currentUser.UserName,
                        EMS_CERTIFIED_DATE = DateTime.Now.ToString(),
                        EMS_CERTIFIED_STATUS = "Pending",
                        EMS_CENTRE_SUPERVISOR_DATE = DateTime.Now.ToString(),
                        EMS_CENTRE_SUPERVISOR_NAME = currentUser.UserName,
                        EMS_CENTRE_SUPERVISOR_STATUS = "Pending",
                       
                        EMS_PERFORMANCE_INDEX = item.Perfomance,


                    };

                    examersScriptsMarked.Add(newTransction);
                }

                foreach (var item in examersScriptsMarked)
                {
                    var existingEntity = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(e =>
                        e.EMS_SUBKEY == item.EMS_SUBKEY &&
                        e.EMS_NATIONAL_ID == item.EMS_NATIONAL_ID
                    );


                    if (existingEntity != null)
                    {
                        existingEntity.SCRIPTS_MARKED = item.SCRIPTS_MARKED;
                        existingEntity.EMS_APPROVED_BY = item.EMS_APPROVED_BY;
                        existingEntity.EMS_APPROVED_DATE = item.EMS_APPROVED_DATE;
                        existingEntity.EMS_APPROVED_STATUS = item.EMS_APPROVED_STATUS;
                        existingEntity.EMS_COMPILED_BY = item.EMS_COMPILED_BY;
                        existingEntity.EMS_COMPILED_STATUS = item.EMS_COMPILED_STATUS;
                        existingEntity.EMS_COMPILED_DATE = item.EMS_COMPILED_DATE;
                        existingEntity.EMS_CERTIFIED_BY = item.EMS_CERTIFIED_BY;
                        existingEntity.EMS_CERTIFIED_STATUS = item.EMS_CERTIFIED_STATUS;
                        existingEntity.EMS_CERTIFIED_DATE = item.EMS_CERTIFIED_DATE;
                        existingEntity.EMS_CENTRE_SUPERVISOR_DATE = item.EMS_CENTRE_SUPERVISOR_DATE;
                        existingEntity.EMS_CENTRE_SUPERVISOR_NAME = item.EMS_CENTRE_SUPERVISOR_NAME;
                        existingEntity.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                      
                        existingEntity.EMS_PERFORMANCE_INDEX = item.EMS_PERFORMANCE_INDEX;

                        _context.EXAMINER_TRANSACTIONS.Update(existingEntity);
                    }
                }

                await _context.SaveChangesAsync(currentUser.Id);
            }
          

            return Json(new { success = true });
        }


        [HttpPost]
        public async Task<IActionResult> ApproveExaminers1([FromBody] ApprovalRequestModel request)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);
                //if (request == null || request.Examiners == null)
                //{
                //    return Json(new { success = false, message = "Invalid request data." });
                //}

                // Check for examiners who are present but have no scripts marked
                //var presentExaminersWithoutScripts = await _transactionRepository.CheckExaminersPresentButNoScriptMarked(
                //    request.ExamCode, request.SubjectCode, request.PaperCode, request.RegionCode);

                //if (presentExaminersWithoutScripts.Any())
                //{
                //    // Get distinct EMS_EXM_SUPERORD values
                //    var distinctSuperords = presentExaminersWithoutScripts
                //        .Select(a => a.EMS_EXM_SUPERORD)
                //        .Where(s => !string.IsNullOrWhiteSpace(s))
                //        .Distinct()
                //        .ToList();

                //    var allExaminers = await _transactionRepository.CheckExaminerInTransactions(
                //        request.ExamCode, request.SubjectCode, request.PaperCode, request.RegionCode);

                //    // Prepare list of "CODE - NAME"
                //    var supervisorList = new List<string>();

                //    foreach (var code in distinctSuperords)
                //    {
                //        var supervisor = allExaminers.FirstOrDefault(a => a.EMS_EXAMINER_NUMBER == code);
                //        if (supervisor != null)
                //        {
                //            var name = $"{supervisor.Examiner.EMS_EXAMINER_NAME} {supervisor.Examiner.EMS_LAST_NAME}";
                //            supervisorList.Add($"{code} - {name}");
                //        }
                //        else
                //        {
                //            supervisorList.Add($"{code} - Unknown");
                //        }
                //    }

                //    // Join into a string
                //    string superordsList = string.Join(", ", supervisorList);

                //    return Json(new
                //    {
                //        success = false,
                //        message = "Some examiners are present but have no scripts marked. Please check before approving.",
                //        superords = superordsList
                //    });
                //}


                //var examiners = await _transactionRepository.CheckExaminerInTransactions(request.ExamCode, request.SubjectCode, request.PaperCode, request.RegionCode);

                var examiners = await _context.EXAMINER_TRANSACTIONS
                             .Where(em => em.EMS_SUBKEY.StartsWith(request.ExamCode)
                                          && em.SCRIPTS_MARKED > 0
                                          && em.RegisterStatus == "Present" && em.IsPresent && (em.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || em.EMS_ECT_EXAMINER_CAT_CODE == "BT" || em.EMS_ECT_EXAMINER_CAT_CODE == "S" || em.EMS_ECT_EXAMINER_CAT_CODE == "I"))
                             .OrderBy(em => em.EMS_EXAMINER_NUMBER)
                             .GroupJoin(
                                 _context.EXM_EXAMINER_MASTER,
                                 em => em.EMS_NATIONAL_ID,
                                 examiner => examiner.EMS_NATIONAL_ID,
                                 (em, examinerGroup) => new { em, examinerGroup })
                             .SelectMany(
                                 x => x.examinerGroup.DefaultIfEmpty(),
                                 (x, examiner) => new ExaminerScriptsMarked
                                 {

                                     EMS_SUBKEY = x.em.EMS_SUBKEY,
                                     EMS_EXAMINER_CODE = x.em.EMS_EXAMINER_CODE,
                                     EMS_NATIONAL_ID = x.em.EMS_NATIONAL_ID,
                                     SCRIPTS_MARKED = x.em.SCRIPTS_MARKED,

                                     EMS_ECT_EXAMINER_CAT_CODE = x.em.EMS_ECT_EXAMINER_CAT_CODE,
                                     Examiner = examiner,
                                     EMS_COMPILED_STATUS = x.em.EMS_COMPILED_STATUS,
                                     EMS_APPROVED_STATUS = x.em.EMS_APPROVED_STATUS,
                                     EMS_CENTRE_SUPERVISOR_STATUS = x.em.EMS_CENTRE_SUPERVISOR_STATUS,
                                     EMS_CERTIFIED_STATUS = x.em.EMS_CERTIFIED_STATUS,
                                     IsPresent = x.em.IsPresent,
                                     EMS_ACTIVITY = x.em.EMS_ACTIVITY,
                                     EMS_PERFORMANCE_INDEX = x.em.EMS_PERFORMANCE_INDEX
                                 })
                             .ToListAsync();

                if (examiners.Any())
                {
                    request.Examiners = examiners.Select(e => new ExaminerScriptModel
                    {

                        Category = e.EMS_ECT_EXAMINER_CAT_CODE,
                        IsPresent = e.IsPresent,
                        ScriptMarked = e.SCRIPTS_MARKED.GetValueOrDefault(),

                        IdNumber = e.EMS_NATIONAL_ID,
                        SubKey = e.EMS_SUBKEY,
                        Status = e.EMS_APPROVED_STATUS,

                    }).ToList();

                }

                // If all conditions are met, proceed with approval logic (update database, etc.)
                //// Example: Call service method to approve examiners
                //var approvalSuccess = await _transactionRepository.ApproveExaminers(request, userRoles, currentUser);

                if (userRoles.Contains("SubjectManager") || userRoles.Contains("OfficerSpecialNeeds"))
                {
                    foreach (var item in request.Examiners)
                    {
                        var examiner = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(a =>
                            a.EMS_NATIONAL_ID == item.IdNumber && a.EMS_SUBKEY == item.SubKey && a.IsPresent);

                        if (examiner != null)
                        {
                            examiner.EMS_CERTIFIED_STATUS = "Certified";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_CERTIFIED_BY = currentUser.UserName;
                            examiner.EMS_CERTIFIED_DATE = DateTime.Now.ToString();
                            _context.EXAMINER_TRANSACTIONS.Update(examiner);
                        }
                    }
                }
                else if (userRoles.Contains("CentreSupervisor"))
                {
                    foreach (var item in request.Examiners)
                    {
                        var examiner = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(a =>
                            a.EMS_NATIONAL_ID == item.IdNumber && a.EMS_SUBKEY == item.SubKey && a.IsPresent);

                        if (examiner != null)
                        {
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Approved";
                            examiner.EMS_CENTRE_SUPERVISOR_NAME = currentUser.UserName;
                            examiner.EMS_CENTRE_SUPERVISOR_DATE = DateTime.Now.ToString();
                            _context.EXAMINER_TRANSACTIONS.Update(examiner);
                        }
                    }
                }

                // Save the changes to the database
                await _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "Examiners approved successfully!" });
            }
            catch (Exception)
            {

                return Json(new { success = false, message = "Failed!" });
            }

          
     
         
        }




    }
}
