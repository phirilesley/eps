using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Wordprocessing;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Extensions;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Transcribers;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Repositories;
using ExaminerPaymentSystem.ViewModels;
using ExaminerPaymentSystem.ViewModels.Examiners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class ExaminerRegisterController : Controller
    {
        private readonly IExaminerRepository _examinerRepository;
        private readonly IRegisterRepository _registerRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITranscribersRepository _transcribersRepository;

        public ExaminerRegisterController(IExaminerRepository examinerRepository, SignInManager<ApplicationUser> signInManager, IRegisterRepository registerRepository, UserManager<ApplicationUser> userManager, ITransactionRepository transactionRepository, ITranscribersRepository transcribersRepository)
        {
            _examinerRepository = examinerRepository;
            _registerRepository = registerRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _transactionRepository = transactionRepository;
            _transcribersRepository = transcribersRepository;

        }

        [Authorize(Roles = "SubjectManager,SuperAdmin")]
        public async Task<IActionResult> Index(string examCode = "", string subjectCode = "", string paperCode = "", string regionCode = "",string activity = "")
        {

            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) && !string.IsNullOrEmpty(paperCode) )
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,
                    SubjectCode = subjectCode.Substring(3),
                    PaperCode = paperCode,
                    Activity = activity,
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

            ViewBag.ExamCode = userSession.ExamCode;
            ViewBag.SubjectCode = userSession.SubjectCode;
            ViewBag.PaperCode = userSession.PaperCode;
            ViewBag.Activity = userSession.Activity;

            ViewBag.RegionCode = string.IsNullOrEmpty(regionCode) ? "" : userSession.RegionCode;
            


            return View();
        }



        [Authorize]
        public async Task<IActionResult> GetData(string examCode = "", string subjectCode = "", string paperCode = "",string activity = "", string regionCode = "", string status = "")
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);


            IEnumerable<RegisterViewModel> model = new List<RegisterViewModel>();
            List<RegisterViewModel> modelList = new List<RegisterViewModel>();



            var availableInRegister = await _registerRepository.GetComponentRegister(examCode, subjectCode, paperCode,activity,regionCode);

            if (!string.IsNullOrEmpty(status))
            {
                availableInRegister = status switch
                {
                    "TotalInvited" => availableInRegister, // No filter for total
                    "ConfirmedAttending" => availableInRegister.Where(e => e.AttendanceStatus == "Yes"),
                    "Pending" => availableInRegister.Where(e => e.AttendanceStatus == "Pending"),
                    "NotAttending" => availableInRegister.Where(e => e.AttendanceStatus == "No"),
                    "Recommended" => availableInRegister.Where(e => e.RecommendedStatus == "Recommended"),
                    "Absent" => availableInRegister.Where(e => e.RegisterStatus == "Absent"),
                    _ => availableInRegister
                };
            }
            modelList = availableInRegister.Select(item => new RegisterViewModel
            {
                FirstName = item.Examiner.EMS_EXAMINER_NAME,
                LastName = item.Examiner.EMS_LAST_NAME,
                PaperCode = item.EMS_PAPER_CODE,
                Subject = item.EMS_SUB_SUB_ID + "/" + item.EMS_PAPER_CODE,
                IdNumber = item.EMS_NATIONAL_ID,
                Category = item.EMS_ECT_EXAMINER_CAT_CODE,
                SubKey = item.EMS_SUBKEY,
                ExaminerNumber = item.EMS_EXAMINER_NUMBER,
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

            int pageSize = length != null ? Convert.ToInt32(length) : 50;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
    (p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.LastName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.IdNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.ExaminerNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.Category?.ToLower().Contains(searchValue.ToLower()) ?? false)
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
        public async Task<IActionResult> CountAbsentAndPresent(string examCode = "", string subjectCode = "", string paperCode = "", string activity="",string regionCode = "")
        {
            var availableInRegister = await _registerRepository.GetComponentRegister(examCode, subjectCode, paperCode,activity,regionCode);

            var presentExaminers = availableInRegister.Count(p => p.RegisterStatus == "Present");
            var absentExaminers = availableInRegister.Count(a => a.RegisterStatus == "Absent");
            var comingExaminers = availableInRegister.Count(a => a.AttendanceStatus == "Yes");
            var notComingExaminers = availableInRegister.Count(a => a.AttendanceStatus == "No");
            var pendingExaminers = availableInRegister.Count(a => a.AttendanceStatus == "Pending");

            var counts = new { Total = availableInRegister.Count(), PresentCount = presentExaminers, AbsentCount = absentExaminers, ComingCount = comingExaminers, NotcomingCount = notComingExaminers, PendingCount = pendingExaminers };

            return Json(counts);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ConfirmAttendance(string username, string firstname, string lastname, string subject, string papercode, string subKey, string examinerCode, string idNumber)
        {
            // Prepare the model with the received data
            var model = new ConfirmAttendanceViewModel
            {
                Username = username,
                FirstName = firstname,
                LastName = lastname,
                Subject = subject,
                PaperCode = papercode,
                SubKey = subKey,
                ExaminerCode = examinerCode,
                IdNumber = idNumber
            };

            // Pass the model to the view
            return View(model);

        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ConfirmAttendancee(string status, string examinerCode, string subKey, string idNumber)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

            string username = currentUser.UserName;

            var register = new ExaminerScriptsMarked()
            {
                AttendanceStatusBy = username,
                EMS_EXAMINER_CODE = examinerCode,
                EMS_SUBKEY = subKey,
                EMS_NATIONAL_ID = idNumber,
                AttendanceStatusDate= DateTime.Now.ToString(),
                AttendanceStatus = status
            };

            await _registerRepository.ConfirmRegister(register,currentUser.Id);

            return RedirectToAction("Index", "Home");

        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RecommendationPending(string username, string firstname, string lastname, string subject, string papercode, string subKey, string examinerCode, string idNumber)
        {
            // Prepare the model with the received data
            var model = new ConfirmAttendanceViewModel
            {
                Username = username,
                FirstName = firstname,
                LastName = lastname,
                Subject = subject,
                PaperCode = papercode,
                SubKey = subKey,
                ExaminerCode = examinerCode,
                IdNumber = idNumber
            };

            // Pass the model to the view
            return View(model);

        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdatePresence([FromBody] UpdatePresenceRequest request)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "Invalid request data." });
                }

                var examiner = await _registerRepository.GetExaminer(request.SubKey);
                if (examiner == null)
                {
                    return NotFound(new { success = false, message = "Examiner not found." });
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

                await _registerRepository.MarkPresent(examiner, currentUser.Id);

                // Return a success response with data
                return Ok(new
                {
                    success = true,
                    message = "Status updated successfully.",
                    newStatus = examiner.RegisterStatus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }




    }

        public class UpdatePresenceRequest
        {
            public string SubKey { get; set; }
            public bool IsPresent { get; set; }
        }
    
}