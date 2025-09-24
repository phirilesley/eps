using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Extensions;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Interfaces.Transcribers;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.ViewModels.Examiners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;



namespace ExaminerPaymentSystem.Controllers.Examiners
{

   
    public class ExaminerScriptsMarkedController : Controller
    {

        private readonly IExamCodesRepository _examCodesRepository;
        private readonly IExaminerRepository _examinerRepository;
        private readonly IPaperMarkingRateRepository _subjectCodesRepository;
        private readonly IApportionmentRepository _apportionmentRepository; 
        //private readonly IExaminerScriptsMarkedRepository _examinerRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMarksCapturedRepository _marksCapturedRepository;
        private readonly ITranscribersRepository _transcribersRepository;
        private readonly IActivityRepository _activityRepository;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExaminerScriptsMarkedController(IExamCodesRepository examCodesRepository, IPaperMarkingRateRepository subjectCodesRepository, IExaminerRepository examinerRepository, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IApportionmentRepository apportionmentRepository,
            ITransactionRepository transactionRepository,IMarksCapturedRepository marksCapturedRepository,ITranscribersRepository transcribersRepository,IActivityRepository activityRepository)
        {
            _examCodesRepository = examCodesRepository;
            _subjectCodesRepository = subjectCodesRepository;
            _examinerRepository = examinerRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _apportionmentRepository = apportionmentRepository;
            _transactionRepository = transactionRepository;
            _marksCapturedRepository = marksCapturedRepository;
            _transcribersRepository = transcribersRepository;
            _activityRepository = activityRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index(string examCode, string subjectCode, string paperCode, string searchBmsCode)
        {
            var viewModel = new ExaminerScriptsMarkedIndexPageViewModel();
            ViewData["Title"] = "Examiner Scripts Marked";

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            if (userRoles != null && (userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS") || userRoles.Contains("BMS")))
            {
                var examiner = await _examinerRepository.GetExaminerRecord(currentUser.IDNumber);
                if (examiner != null)
                {
                    var transaction = examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == currentUser.EMS_SUBKEY);

                    if (transaction == null)
                    {
                        TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                        {
                            icon = "warning",
                            title = "ACCESS DENIED!",
                            text = "You are not authorized for this activity. Please check your account credentials. You will be logged out for security reasons.",
                            showConfirmButton = true,
                            confirmButtonColor = "#ffc107",
                            timerProgressBar = true,
                            customClass = new
                            {
                                container = "swal2-flicker",
                                title = "swal2-title-danger"
                            }
                        });

                        await _signInManager.SignOutAsync();
                        return Redirect("/Identity/Account/Login");
                    }

                    viewModel.ExamCode = transaction.EMS_SUBKEY.Substring(0, 3);
                    viewModel.SubjectCode = transaction.EMS_SUB_SUB_ID.Substring(3);
                    viewModel.PaperCode = transaction.EMS_PAPER_CODE;
                    viewModel.BmsCode = transaction.EMS_EXAMINER_NUMBER;
                    viewModel.SupervisorName = examiner.EMS_EXAMINER_NAME + " " + examiner.EMS_LAST_NAME;
                }
            }

            return View(viewModel);
        }

     


     


        [Authorize]
        public async Task<IActionResult> GetTransactionData(string examCode = "", string subjectCode = "", string paperCode = "", string bmsCode = "")
        {
            IEnumerable<ScriptMarkedViewModel> model = new List<ScriptMarkedViewModel>();
            List<ScriptMarkedViewModel> modelList = new List<ScriptMarkedViewModel>();

            var entriesData = new EntriesData();
            IEnumerable<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();

            examiners = await _transactionRepository.GetExaminersFromTransaction(examCode,subjectCode,paperCode);

            modelList = examiners.Select(ex => new ScriptMarkedViewModel
            {
                ExaminerCode = ex.EMS_EXAMINER_CODE,
                FirstName = ex.Examiner.EMS_EXAMINER_NAME,
                LastName = ex.Examiner.EMS_LAST_NAME,
                IdNumber = ex.EMS_NATIONAL_ID,
                Subject = ex.EMS_SUB_SUB_ID + "/" + ex.EMS_PAPER_CODE,
                SubKey = ex.EMS_SUBKEY,
                BMS = ex.EMS_EXM_SUPERORD,
                ExaminerNumber = ex.EMS_EXAMINER_NUMBER,
                Category = ex.EMS_ECT_EXAMINER_CAT_CODE,
                ScriptMarked = ex.SCRIPTS_MARKED ?? 0, // Default to 0 if null
                PaperCode = ex.EMS_PAPER_CODE,
                Role = ex.EMS_CAPTURINGROLE,
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
        public async Task<IActionResult> UpdatePresence([FromBody] UpdateTranscactionPresenceRequest request)
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
                var message = "";
                await _transactionRepository.UpdatePresent(request, currentUser.Id);

                if (request.IsPresent)
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
        public async Task<IActionResult> GetData(string examCode = "", string subjectCode = "", string paperCode = "", string bmsCode = "")
        {
            IEnumerable<ScriptMarkedViewModel> model = new List<ScriptMarkedViewModel>();
            List<ScriptMarkedViewModel> modelList = new List<ScriptMarkedViewModel>();
        

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var entriesData = new EntriesData();
            IEnumerable<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();
            string name = "";
            string bmscodeFor = "";

            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var filteredExaminers = new List<ExaminerScriptsMarked>();

            if (userRoles != null && userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS"))
            {

                if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) && !string.IsNullOrEmpty(paperCode) && !string.IsNullOrEmpty(bmsCode))
                {
                    examiners = await _transactionRepository.GetTeamByBms(examCode,subjectCode,paperCode, bmsCode);
                    entriesData = await _transactionRepository.GetEntries(examCode,subjectCode,paperCode, bmsCode);
                    bmscodeFor = bmsCode;
                    var superVisor = examiners.FirstOrDefault(a => a.EMS_EXAMINER_NUMBER == bmsCode);
                    if (superVisor != null)
                    {
                        name = superVisor.Examiner.EMS_EXAMINER_NAME + " " + superVisor.Examiner.EMS_LAST_NAME;
                    }
                    
                }
                else
                {
                    var examinerRecord = await _examinerRepository.GetExaminerRecord(currentUser.IDNumber);
                    var examiner = examinerRecord.ExaminerScriptsMarkeds.FirstOrDefault(a =>  a.EMS_SUBKEY == currentUser.EMS_SUBKEY);

                    var bmsexamcode = examiner.EMS_SUBKEY.Substring(0, 3);
                    var bmssubjectcode = examiner.EMS_SUB_SUB_ID.Substring(3,4);
                    var bmspapercode = examiner.EMS_PAPER_CODE;
                    var bms = examiner.EMS_EXAMINER_NUMBER;
                    examiners = await _transactionRepository.GetTeamByBms(bmsexamcode, bmssubjectcode, bmspapercode, bms);
                    bmscodeFor = bms;
                    var superVisor = examiners.FirstOrDefault(a => a.EMS_EXAMINER_NUMBER == bms);
                    if (superVisor != null)
                    {
                        name = superVisor.Examiner.EMS_EXAMINER_NAME + " " + superVisor.Examiner.EMS_LAST_NAME;
                    }
                    entriesData = await _transactionRepository.GetEntries(bmsexamcode, bmssubjectcode, bmspapercode, bms);
                }


            }
            else if (userRoles != null && userRoles.Contains("BMS"))
            {
                var examinerRecord = await _examinerRepository.GetExaminerRecord(currentUser.IDNumber);
                var examiner = examinerRecord.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == currentUser.EMS_SUBKEY);
                if (examiner != null)
                {
                    var bmsexamcode = examiner.EMS_SUBKEY.Substring(0, 3);
                    var bmssubjectcode = examiner.EMS_SUB_SUB_ID.Substring(3, 4);
                    var bmspapercode = examiner.EMS_PAPER_CODE;
                    var bms = examiner.EMS_EXAMINER_NUMBER;
                    bmscodeFor = bms;
                    name = examinerRecord.EMS_EXAMINER_NAME + " " + examinerRecord.EMS_LAST_NAME;


                    examiners = await _transactionRepository.GetTeamByBms(bmsexamcode, bmssubjectcode, bmspapercode, bms);
                    entriesData = await _transactionRepository.GetEntries(bmsexamcode, bmssubjectcode, bmspapercode, bms);
                }
              

            }

                


            modelList = examiners.Select(ex => new ScriptMarkedViewModel
            {
                ExaminerCode = ex.EMS_EXAMINER_CODE,
                FirstName = ex.Examiner.EMS_EXAMINER_NAME,
                LastName = ex.Examiner.EMS_LAST_NAME,
                IdNumber = ex.EMS_NATIONAL_ID,
                Subject = ex.EMS_SUB_SUB_ID + "/" + ex.EMS_PAPER_CODE,
                SubKey = ex.EMS_SUBKEY,
                BMS = ex.EMS_EXM_SUPERORD,
                ExaminerNumber = ex.EMS_EXAMINER_NUMBER,
                Category = ex.EMS_ECT_EXAMINER_CAT_CODE,
                ScriptMarked = ex.SCRIPTS_MARKED ?? 0, // Default to 0 if null
                PaperCode = ex.EMS_PAPER_CODE,
                Role = ex.EMS_CAPTURINGROLE,
                Perfomance = ex.EMS_PERFORMANCE_INDEX,
                IsPresent = ex.IsPresent,
                ScriptApportioned = entriesData?.AppointedScripts ?? "0",
                Status = ex.EMS_APPROVED_STATUS
            }).ToList();


            // Check if ALL examiners are either Approved OR not Present
            bool disableSaveButton = modelList.All(x =>
                x.Status == "Approved" || x.IsPresent == false
            );


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
                disableSave = disableSaveButton,
                bms = bmscodeFor,
                name
            };

            return Ok(jsonData);



        }

 
        [Authorize(Roles = "PMS,DPMS,RPMS,SubjectManager,CentreSupervisor")]
        public async Task<IActionResult> Approval(string examCode, string subjectCode, string paperCode, string regionCode)
        {
            var viewModel = new ExaminerScriptsMarkedApprovalPageViewModel();

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var userSession = new SessionModel();
            if (userRoles != null && (userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS")))
            {
                var examiner = await _examinerRepository.GetExaminerRecord(currentUser.IDNumber);
                if (examiner != null)
                {
                    var transaction = examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == currentUser.EMS_SUBKEY && a.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID);

                    if (transaction != null)
                    {
                        userSession = new SessionModel()
                        {
                            ExamCode = transaction.EMS_SUBKEY.Substring(0, 3),
                            SubjectCode = transaction.EMS_SUB_SUB_ID,
                            PaperCode = transaction.EMS_PAPER_CODE,
                        };

                        if (transaction.EMS_SUB_SUB_ID.StartsWith("7"))
                        {
                            userSession.RegionCode = transaction.EMS_MARKING_REG_CODE;
                        }

                        HttpContext.Session.SetObjectAsJson("Session", userSession);
                    }
                }
            }

            if (userRoles != null && (userRoles.Contains("SubjectManager") || userRoles.Contains("CentreSupervisor")))
            {
                if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) && !string.IsNullOrEmpty(paperCode))
                {
                    userSession = new SessionModel()
                    {
                        ExamCode = examCode,
                        SubjectCode = subjectCode,
                        PaperCode = paperCode,
                    };

                    if (!string.IsNullOrEmpty(regionCode))
                    {
                        userSession.RegionCode = regionCode;
                    }
                    HttpContext.Session.SetObjectAsJson("Session", userSession);
                }
                else
                {
                    userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
                }
            }

            if (userSession != null)
            {
                viewModel.ExamCode = userSession.ExamCode;
                viewModel.SubjectCode = userSession.SubjectCode;
                viewModel.PaperCode = userSession.PaperCode;
                viewModel.RegionCode = string.IsNullOrEmpty(regionCode) ? string.Empty : userSession.RegionCode;
            }
            else
            {
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",
                    title = "ACCESS DENIED!",
                    text = "You are not authorized for this activity. Please check your account credentials. You will be logged out for security reasons.",
                    showConfirmButton = true,
                    confirmButtonColor = "#ffc107",
                    timerProgressBar = true,
                    customClass = new
                    {
                        container = "swal2-flicker",
                        title = "swal2-title-danger"
                    }
                });

                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
            }

            return View(viewModel);
        }


        [Authorize]
        public async Task<IActionResult> GetApprovalData(string examCode = "", string subjectCode = "", string paperCode = "",string regionCode = "")
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
           
     

               
            if (userRoles != null && userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS"))
            {


                var dataForScripts = await _marksCapturedRepository.GetComponentMarkCaptured(examCode,subjectCode,paperCode,regionCode);
                if (dataForScripts != null)
                {
                    apportioned = dataForScripts.ApportionedScripts;
                    absent = dataForScripts.AbsentScripts;
                    pirates = dataForScripts.PirateCandidates;

                }
              
                var examiners = await _transactionRepository.CheckExaminerInTransactions(examCode, subjectCode, paperCode,regionCode);
                var presentExaminers = await _transactionRepository.CheckPresentExaminersInTransactions(examCode, subjectCode, paperCode, regionCode);

                pending = examiners.Where(a => a.EMS_APPROVED_STATUS == "Pending" && a.EMS_COMPILED_STATUS == "Compiled").Count();
                approved = examiners.Where(a => a.EMS_APPROVED_STATUS == "Approved" && a.EMS_COMPILED_STATUS == "Compiled").Count();
                totalCount = presentExaminers.Count();
                var newTransactionData = new List<ExaminerScriptsMarked>();
                foreach (var item in examiners)
                {
                    totalscripts += item.SCRIPTS_MARKED.GetValueOrDefault();
                    if (item.EMS_COMPILED_STATUS == "Compiled" && item.EMS_APPROVED_STATUS == "Pending")
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
                            EMS_EXAMINER_NUMBER =item.EMS_EXAMINER_NUMBER,
                            EMS_EXM_SUPERORD = item.EMS_EXM_SUPERORD,
                            IsPresent = item.IsPresent,
                            EMS_ACTIVITY = item.EMS_ACTIVITY,
                            EMS_PERFORMANCE_INDEX  = item.EMS_PERFORMANCE_INDEX
                        };

                        var activityName =  await _activityRepository.GetActivityByValueAsync(item.EMS_ACTIVITY);
                        if(activityName != null)
                        {
                            newTranscation.EMS_ACTIVITY = activityName.Text;
                        }
                        

                        filteredExaminers.Add(newTranscation);

                    }

                }

                foreach (var ex in filteredExaminers)
                {
                    //var status = "Pending";
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
                            Status = ex.EMS_APPROVED_STATUS,
                            IsPresent = ex.IsPresent,
                            Activity = ex.EMS_ACTIVITY,
                            Perfomance = ex.EMS_PERFORMANCE_INDEX
                        });
                    }
                }

            }
            else if (userRoles != null && userRoles.Contains("SubjectManager"))
                {
                    var dataForScripts = await _marksCapturedRepository.GetComponentMarkCaptured(examCode, subjectCode, paperCode, regionCode);
                    if (dataForScripts != null)
                    {
                        apportioned = dataForScripts.ApportionedScripts;
                        absent = dataForScripts.AbsentScripts;
                        pirates = dataForScripts.PirateCandidates;

                    }
                    
                    var examiners = await _transactionRepository.CheckExaminerInTransactions(examCode, subjectCode, paperCode, regionCode);
                var presentExaminers = await _transactionRepository.CheckPresentExaminersInTransactions(examCode, subjectCode, paperCode, regionCode);

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
                            EMS_EXAMINER_NUMBER = item.EMS_EXAMINER_NUMBER,
                            EMS_EXM_SUPERORD = item.EMS_EXM_SUPERORD,
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
                            Status = ex.EMS_CERTIFIED_STATUS,
                            IsPresent = ex.IsPresent,
                            Perfomance = ex.EMS_PERFORMANCE_INDEX


                        });
                    }
                    }

                }
                    else if(userRoles != null && userRoles.Contains("CentreSupervisor"))
                    {
                    var dataForScripts = await _marksCapturedRepository.GetComponentMarkCaptured(examCode, subjectCode, paperCode, regionCode);
                    if (dataForScripts != null)
                    {
                        apportioned = dataForScripts.ApportionedScripts;
                        absent = dataForScripts.AbsentScripts;
                        pirates = dataForScripts.PirateCandidates;

                    }
                 
                    var examiners = await _transactionRepository.CheckExaminerInTransactions(examCode, subjectCode, paperCode, regionCode);
                var presentExaminers = await _transactionRepository.CheckPresentExaminersInTransactions(examCode, subjectCode, paperCode, regionCode);
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
                                EMS_EXAMINER_NUMBER = item.EMS_EXAMINER_NUMBER,
                                EMS_EXM_SUPERORD = item.EMS_EXM_SUPERORD,
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
                    
                       if(ex.Examiner != null)
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

        [HttpPost]
        public async Task<IActionResult> ApproveExaminers1([FromBody] ApprovalRequestModel request)
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            //if (request == null || request.Examiners == null)
            //{
            //    return Json(new { success = false, message = "Invalid request data." });
            //}

            // Check for examiners who are present but have no scripts marked
            var presentExaminersWithoutScripts = await _transactionRepository.CheckExaminersPresentButNoScriptMarked(
                request.ExamCode, request.SubjectCode, request.PaperCode, request.RegionCode);

            if (presentExaminersWithoutScripts.Any())
            {
                // Get distinct EMS_EXM_SUPERORD values
                var distinctSuperords = presentExaminersWithoutScripts
                    .Select(a => a.EMS_EXM_SUPERORD)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .ToList();

                var allExaminers = await _transactionRepository.CheckExaminerInTransactions(
                    request.ExamCode, request.SubjectCode, request.PaperCode, request.RegionCode);

                // Prepare list of "CODE - NAME"
                var supervisorList = new List<string>();

                foreach (var code in distinctSuperords)
                {
                    var supervisor = allExaminers.FirstOrDefault(a => a.EMS_EXAMINER_NUMBER == code);
                    if (supervisor != null)
                    {
                        var name = $"{supervisor.Examiner.EMS_EXAMINER_NAME} {supervisor.Examiner.EMS_LAST_NAME}";
                        supervisorList.Add($"{code} - {name}");
                    }
                    else
                    {
                        supervisorList.Add($"{code} - Unknown");
                    }
                }

                // Join into a string
                string superordsList = string.Join(", ", supervisorList);

                return Json(new
                {
                    success = false,
                    message = "Some examiners are present but have no scripts marked. Please check before approving.",
                    superords = superordsList
                });
            }

            if (userRoles.Contains("SubjectManager"))
            {
                // Check for examiners who are present but have no scripts marked
                var presentExaminersNotApproved = await _transactionRepository.CheckExaminersPresentButNotApproved(
                request.ExamCode, request.SubjectCode, request.PaperCode, request.RegionCode);
                var pms = "";
                if (presentExaminersNotApproved.Any())
                {


                    var allExaminers = await _transactionRepository.CheckExaminerInTransactions(
                        request.ExamCode, request.SubjectCode, request.PaperCode, request.RegionCode);

                    // Prepare list of "CODE - NAME"


                    var supervisor = allExaminers.FirstOrDefault(a => a.EMS_EXAMINER_NUMBER == "1001");
                    if (supervisor != null)
                    {
                        var name = $"{supervisor.Examiner.EMS_EXAMINER_NAME} {supervisor.Examiner.EMS_LAST_NAME}";
                        pms = $"{"1001"} - {name}";
                    }
                    else
                    {
                        pms = $"{"1001"} - Unknown";
                    }

                    // Join into a string
                    string superord = pms;

                    return Json(new
                    {
                        success = false,
                        message = "Some examiners are not yet approved by the PMS. Please contact PMS",
                        superords = superord
                    });
                }

            }
               
            

            var examiners = await _transactionRepository.CheckExaminerInTransactions(request.ExamCode, request.SubjectCode, request.PaperCode, request.RegionCode);

            if (examiners.Any())
            {
                request.Examiners = examiners.Select(e => new ExaminerScriptModel
                {
                    ExaminerNumber = e.EMS_EXAMINER_NUMBER,
                    Category = e.EMS_ECT_EXAMINER_CAT_CODE,
                   IsPresent = e.IsPresent,
                    ScriptMarked = e.SCRIPTS_MARKED.GetValueOrDefault(),
                    BMS = e.EMS_EXM_SUPERORD,
                    IdNumber = e.EMS_NATIONAL_ID,
                    SubKey = e.EMS_SUBKEY,
                    Status = e.EMS_APPROVED_STATUS,

                }).ToList();

            }

            // If all conditions are met, proceed with approval logic (update database, etc.)
            // Example: Call service method to approve examiners
            var approvalSuccess = await _transactionRepository.ApproveExaminers(request,userRoles,currentUser);

            if (approvalSuccess.Success)
            {
                return Json(new { success = true, message = "Examiners approved successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to approve examiners." });
            }
        }

    


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetTeamByBms(string ExaminerCode, string SubjectCode, string paperCode, string searchBmsCode)
        {
            try
    {
             
                var examinerTransactions = await _transactionRepository.CheckExaminerTransactions(ExaminerCode, SubjectCode, paperCode, searchBmsCode);

            
                var examiners = await _transactionRepository.GetTeamByBms(ExaminerCode, SubjectCode, paperCode, searchBmsCode);

            
                var mergedList = examinerTransactions.Concat(examiners)
                                                    .GroupBy(e => e.EMS_EXAMINER_CODE)
                                                    .Select(g => g.First())
                                                    .ToList();

         
                return Json(mergedList);
            }
    catch (Exception ex)
    {
                // Handle any exceptions
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }


      

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveData([FromBody] List<ScriptMarkedViewModel> data)
        {


            var checkScriptsValidation = data.Where(a => a.IsPresent && a.ScriptMarked == 0);
            if (checkScriptsValidation.Any())
            {
                return Json(new { success = false, message = "Some present examiners have 0 marked scripts." });
            }

            var checkPerfomance = data.Where(a => a.IsPresent && a.Perfomance == "");
            if (checkPerfomance.Any())
            {
                return Json(new { success = false, message = "Examiners perfomance indicator missing." });
            }

            List<ExaminerScriptsMarked> examersScriptsMarked = new List<ExaminerScriptsMarked>();

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

       
                if(data != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(currentUser);

                    if (userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS"))
                    {
                        foreach (var item in data)
                        {
                      
                            var newTransction = new ExaminerScriptsMarked()
                            {
                                EMS_NATIONAL_ID = item.IdNumber,
                                EMS_SUBKEY = item.SubKey,
                                EMS_EXAMINER_CODE = item.ExaminerCode,
                                SCRIPTS_MARKED = item.ScriptMarked,
                                EMS_CAPTURINGROLE = item.Role,
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
                                EMS_SUB_SUB_ID = item.SubKey.Substring(0, 7),
                                EMS_PAPER_CODE = item.SubKey.Substring(7, 2),
                                EMS_PERFORMANCE_INDEX = item.Perfomance,


                            };
                          
                                examersScriptsMarked.Add(newTransction);
           
                        }
                        await _transactionRepository.InsertScriptMarked(examersScriptsMarked, currentUser.Id);
                    }
                    else if (userRoles.Contains("BMS"))
                    {
                        foreach (var item in data)
                        {

                            var newTransction = new ExaminerScriptsMarked()
                            {
                                EMS_NATIONAL_ID = item.IdNumber,
                                EMS_SUBKEY = item.SubKey,
                                EMS_EXAMINER_CODE = item.ExaminerCode,
                                SCRIPTS_MARKED = item.ScriptMarked,
                                EMS_CAPTURINGROLE = item.Role,
                                EMS_COMPILED_BY = currentUser.UserName,
                                EMS_COMPILED_DATE = DateTime.Now.ToString(),
                                EMS_COMPILED_STATUS = "Compiled",
                                EMS_APPROVED_DATE = DateTime.Now.ToString(),
                                EMS_APPROVED_BY = currentUser.UserName,
                                EMS_APPROVED_STATUS = "Pending",
                                EMS_CERTIFIED_BY = currentUser.UserName,
                                EMS_CERTIFIED_DATE = DateTime.Now.ToString(),
                                EMS_CERTIFIED_STATUS = "Pending",
                                EMS_CENTRE_SUPERVISOR_DATE = DateTime.Now.ToString(),
                                EMS_CENTRE_SUPERVISOR_NAME = currentUser.UserName,
                                EMS_CENTRE_SUPERVISOR_STATUS = "Pending",
                                EMS_SUB_SUB_ID = item.SubKey.Substring(0, 7), 
                                EMS_PAPER_CODE = item.SubKey.Substring(7, 2),
                                EMS_PERFORMANCE_INDEX = item.Perfomance,
                            };
                       
                                examersScriptsMarked.Add(newTransction);
                            

                           
                        }
                        await _transactionRepository.InsertScriptMarked(examersScriptsMarked, currentUser.Id);
                    }
                   
                }
               
            
              
            return Json(new { success = true });
        }


     
      
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> InsertExaminerTransactions([FromBody] JsonObject jsonData)
        {
            try
            {
                if (jsonData != null)
                {
                    RootObject data = JsonConvert.DeserializeObject<RootObject>(jsonData.ToString());


                    List<ExaminerScriptsMarked> examiners = System.Text.Json.JsonSerializer.Deserialize<List<ExaminerScriptsMarked>>(jsonData["tableData"].ToString());
                    //var EMS_EXAMINER_CODE = examiners.FirstOrDefault().EMS_EXAMINER_NUMBER;


                    ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                    if (currentUser == null)
                    {

                        // User is not authenticated, handle accordingly
                        return RedirectToAction("Login", "Account"); // Redirect to login page

                    }
                    
                    //await _transactionRepository.InsertExaminerTransactions(data,currentUser);

                    // Process and manipulate the JSON data as needed

                    return Json(new { success = true, message = "Data inserted successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "JSON data is null" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while inserting data into EXAMINER_TRANSACTIONS table." + ex.Message });
            }
        }

     

    }

    public class UpdateTranscactionPresenceRequest
    {
        public string idNumber { get; set; }

        public string SubKey { get; set; }
        public bool IsPresent { get; set; }
    }

    public class RootObject
    {
        public string SUBJECT_CODE { get; set; }
        public string PAPER_CODE { get; set; }
        public string Exam_Code { get; set; }
        public string searchBmsCode { get; set; }
        public List<ExaminerScriptsMarked> tableData { get; set; } // This completes the definition
    }


    public class ExaminerScriptModel
    {
        public string ExaminerNumber { get; set; }
        public string IdNumber { get; set; }
        public string LastName { get; set; }

        public string SubKey { get; set; }
        public string FirstName { get; set; }
        public string Category { get; set; }
        public string BMS { get; set; }
        public int ScriptMarked { get; set; }
        public string Status { get; set; }

        public bool IsPresent { get; set; }
    }

    public class ApprovalRequestModel
    {
        public string ExamCode { get; set; }
        public string SubjectCode { get; set; }
        public string PaperCode { get; set; }
        public string RegionCode { get; set; }
        public List<ExaminerScriptModel> Examiners { get; set; }
    }
    // Add the missing class definition for ExaminerApprovalModel
    public class ExaminerApprovalModel
    {
        public string EMS_EXAMINER_CODE { get; set; }
        public string EMS_NATIONAL_ID { get; set; }
        public string EMS_SUBKEY { get; set; }
        public int SCRIPTS_MARKED { get; set; }
        public string EMS_CAPTURINGROLE { get; set; }
        public bool IsPresent { get; set; }
    }
}


