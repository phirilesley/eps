using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Core.Types;
using OfficeOpenXml.Table.PivotTable;
using System.Security.Claims;

namespace ExaminerPaymentSystem.Controllers.ExaminerRecruitments
{
    public class ExaminerRecruitmentAssessmentController : Controller
    {
        private readonly IExaminerRecruitmentRegisterRepository _register_repository;
        private readonly IExaminerRecruitmentAssessmentRepository _assessment_repository;

    

        public ExaminerRecruitmentAssessmentController(IExaminerRecruitmentRegisterRepository registerRepository, IExaminerRecruitmentAssessmentRepository assessment_repository)
        {
            _register_repository = registerRepository;
            _assessment_repository = assessment_repository; 
        }

        public IActionResult AssesmentIndex()
        {
            return View();
        }

        public IActionResult GradingIndex(SessionData model)
        {

            // Store the filter values in session

            HttpContext.Session.SetString("PaperCode", model.PaperCode ?? string.Empty);
            HttpContext.Session.SetString("Subject", model.Subject ?? string.Empty);
            HttpContext.Session.SetString("Experience", model.Experience ?? string.Empty);


            // Set the session values to ViewBag to use them in the JavaScript
            ViewBag.Subject = HttpContext.Session.GetString("Subject");
            ViewBag.PaperCode = HttpContext.Session.GetString("PaperCode");
            ViewBag.Experience = HttpContext.Session.GetString("Experience");

            return View();
        }



        public async Task<IActionResult> SaveExaminerTraineeGrade([FromBody] ExaminerTraineeGradeViewModel model)
        {
            if (model == null || model.ExaminerRecruitmentId <= 0 ||  string.IsNullOrEmpty(model.Grade))
            {
                return Json(new { success = false, message = "Invalid data. All required fields must be provided." });
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                //  Fetch existing assessment record
                var existingAssessment = await _assessment_repository.GetAssessmentByExaminerIdAsync(model.ExaminerRecruitmentId);

                // Initialize new assessment if none exists
                var examinerGrade = existingAssessment ?? new ExaminerRecruitmentAssessment
                {
                    ExaminerRecruitmentId = model.ExaminerRecruitmentId,
                    ExaminerRecruitmentRegisterId = model.ExaminerRecruitmentRegisterId,
                    Date = DateTime.UtcNow
                };

                //examinerGrade.Status = model.Status;
                //examinerGrade.Comments = model.Comments;

                // Role-based grade logic
                if (User.IsInRole("HRCapturer"))
                {
                    if (!string.IsNullOrEmpty(examinerGrade.VerifierGrade) && examinerGrade.VerifierGrade != model.Grade)
                    {
                        return Json(new { success = false, message = "Grade does not match that of the verifier." });
                    }

                    examinerGrade.CapturerGrade = model.Grade;
                    examinerGrade.CapturerId = userId;
                }
                else if (User.IsInRole("HRVerifier"))
                {
                    if (!string.IsNullOrEmpty(examinerGrade.CapturerGrade) && examinerGrade.CapturerGrade != model.Grade)
                    {
                        return Json(new { success = false, message = "Grade does not match that of the capturer." });
                    }

                    examinerGrade.VerifierGrade = model.Grade;
                    examinerGrade.VerifierId = userId;
                }
                else
                {
                    return Json(new { success = false, message = "Unauthorized role for grading." });
                }

                var result = await _assessment_repository.SaveExaminerTraineeGradeAsync(examinerGrade);

                return Json(new { success = result, message = result ? "Grade saved successfully!" : "Failed to save data." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }




        [HttpPost]
        public async Task<IActionResult> GetExaminerData(string? status)
        {
            // Extract DataTable parameters from the form request
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["searchValue"].FirstOrDefault();

            int skip = string.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start);
            int take = string.IsNullOrEmpty(length) ? 10 : Convert.ToInt32(length);

            var sortColumn = !string.IsNullOrEmpty(sortColumnIndex)
                ? Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault()
                : "examinerName";


            var sortDirection = string.IsNullOrEmpty(sortColumnDirection) ? "asc" : sortColumnDirection;



            // Retrieve custom filter parameters
            var sessionLevelFilter = Request.Form["sessionLevel"].FirstOrDefault();
            var subjectFilter = Request.Form["subject"].FirstOrDefault();
            var paperCodeFilter = Request.Form["paperCode"].FirstOrDefault();
            var regionCodeFilter = Request.Form["regionCode"].FirstOrDefault();

            // Fetch filtered examiners
            var result = await _register_repository.GetAllPresentAsync(skip, take, searchValue, sortColumn, sortDirection,
                 sessionLevelFilter, subjectFilter, paperCodeFilter, regionCodeFilter);

            // Apply status filter if needed
       
            if (!string.IsNullOrEmpty(status))
            {
                result.Data = status switch
                {
                    "Total" => result.Data.ToList(),
                    "TotalA" => result.Data.Where(x => x.Grade == "A").ToList(),
                    "TotalB" => result.Data.Where(x => x.Grade == "B").ToList(),
                    "TotalC" => result.Data.Where(x => x.Grade == "C").ToList(),
                    "TotalD" => result.Data.Where(x => x.Grade == "D").ToList(),
                    "Pending" => result.Data.Where(x => x.Grade == "Pending").ToList(), // Filtering Pending
                    _ => result.Data.ToList()
                };


            }


            // Return JSON response with all required totals
            return Json(new
            {
                draw = draw,
                recordsTotal = result.TotalCount,
                recordsFiltered = result.TotalCount,
                data = result.Data,
                totalGradeA = result.TotalGradeA,
                totalGradeB = result.TotalGradeB,
                totalGradeC = result.TotalGradeC,
                totalGradeD = result.TotalGradeD,
                totalPending = result.totalPending
            });
        }


        [HttpPost]
        public async Task<IActionResult> GetExaminerDataForAssesment(string? status)
        {
            // Extract DataTable parameters from the form request
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["searchValue"].FirstOrDefault();

            int skip = string.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start);
            int take = string.IsNullOrEmpty(length) ? 10 : Convert.ToInt32(length);

            var sortColumn = !string.IsNullOrEmpty(sortColumnIndex)
                ? Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault()
                : "examinerName";

            var sortDirection = string.IsNullOrEmpty(sortColumnDirection) ? "asc" : sortColumnDirection;

            // Get filter values from ViewBag if form values are empty
            var subject = Request.Form["subject"].FirstOrDefault() ?? ViewBag.Subject as string;
            var paperCode = Request.Form["paperCode"].FirstOrDefault() ?? ViewBag.PaperCode as string;
            var experience = Request.Form["experience"].FirstOrDefault() ?? ViewBag.Experience as string;


            string gradeField = User.IsInRole("HRCapturer") ? "CapturerGrade"
                 : User.IsInRole("HRVerifier") ? "VerifierGrade"
                 : "CapturerGrade"; // Default

            // Fetch filtered examiners
            //var result = await _register_repository.GetAllPresentForAssesmentAsync(skip, take, searchValue, sortColumn, sortDirection,
            //     experience, subject, paperCode);

            var result = await _register_repository.GetAllPresentForAssesmentAsync(
                                    skip, take, searchValue, sortColumn, sortDirection,
                                    experience, subject, paperCode, gradeField);



            // Determine current user role
            var isCapturer = User.IsInRole("HRCapturer");
            var isVerifier = User.IsInRole("HRVerifier");

            // Set Grade based on role
            foreach (var d in result.Data)
            {
                d.Grade = d.CapturerGrade ?? d.VerifierGrade ?? "Pending";

                if (isCapturer)
                {
                    d.Grade = d.CapturerGrade ?? "Pending";
                }
                else if (isVerifier)
                {
                    d.Grade = d.VerifierGrade ?? "Pending";
                }
            }


            // Apply status filter if needed
            if (!string.IsNullOrEmpty(status))
            {
                result.Data = status switch
                {
                    "Total" => result.Data.ToList(),
                    "TotalA" => result.Data.Where(x => x.Grade == "A").ToList(),
                    "TotalB" => result.Data.Where(x => x.Grade == "B").ToList(),
                    "TotalC" => result.Data.Where(x => x.Grade == "C").ToList(),
                    "TotalD" => result.Data.Where(x => x.Grade == "D").ToList(),
                    "Pending" => result.Data.Where(x => x.Grade == "Pending").ToList(),
                    _ => result.Data.ToList()
                };
            }

            // Return JSON response with all required totals
            return Json(new
            {
                draw = draw,
                recordsTotal = result.TotalCount,
                recordsFiltered = result.TotalCount,
                data = result.Data,
                totalGradeA = result.TotalGradeA,
                totalGradeB = result.TotalGradeB,
                totalGradeC = result.TotalGradeC,
                totalGradeD = result.TotalGradeD,
                totalPending = result.totalPending
            });
        }


        // Get Grade Detail

        [HttpGet]
        public async Task<IActionResult> GetByExaminerID(int Id)
        {
            // Check if Id is null or 0
            if (Id == 0)
            {
                TempData["ErrorMessage"] = "Invalid examiner ID provided.";
                return RedirectToAction("GetExaminerData");
            }

            var examiner = await _assessment_repository.GetByExaminerIdAsync(Id, true);

            if (examiner == null)
            {
                TempData["ErrorMessage"] = "Examiner not found with the provided ID.";
                return RedirectToAction("GetExaminerData");
            }

            var viewModel = new ExaminerRecruitmentAssessmentViewModel
            {
                ExaminerName = examiner.ExaminerRecruitment?.ExaminerName ?? "N/A",
                LastName = examiner.ExaminerRecruitment?.LastName ?? "N/A",
                ExaminerCode = examiner.ExaminerRecruitment?.ExaminerCode ?? "N/A",
                Gender = examiner.ExaminerRecruitment?.Sex ?? "N/A",
                CemId = examiner.ExaminerRecruitment?.CemId ?? "N/A",
                PaperCode = examiner.ExaminerRecruitment?.PaperCode ?? "N/A",
                Subject = examiner.ExaminerRecruitment?.Subject ?? "N/A",
                EmailAddress = examiner.ExaminerRecruitment?.EmailAddress ?? "N/A",
                PhoneHome = examiner.ExaminerRecruitment?.PhoneHome ?? "N/A",
                PhoneBusiness = examiner.ExaminerRecruitment?.PhoneBusiness ?? "N/A",
                ProfessionalQualification = examiner.ExaminerRecruitment?.AcademicQualification ?? "N/A",
                AcademicQualification = examiner.ExaminerRecruitment?.Qualification ?? "N/A",
                Grade = examiner.CapturerGrade ?? "Not Assessed",
                Qualification = examiner.ExaminerRecruitment?.Experience ??"N/A",
                WorkAdrress = examiner.ExaminerRecruitment?.WorkAddress1 ?? "N/A",
                WorkAdress2 = examiner.ExaminerRecruitment?.WorkAddress2 ?? "N/A",
                WorkAddress3 = examiner.ExaminerRecruitment?.WorkAddress3 ?? "N/A",
                IsPending = examiner.CapturerGrade == null // If Grade is null, it's pending
            };

            return View(viewModel);
        }

        public IActionResult PartiallyCapturedGradesIndex() {
        
          return View();

        }

        public IActionResult AllCapuredIndex()
        {

            return View();

        }

        [HttpPost("GetPartiallyCapturedTraineeExaminerGrades")]
        public async Task<IActionResult> GetPartiallyCapturedTraineeExaminerGrades()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();

            // Retrieve custom filter parameters
            var sessionLevelFilter = Request.Form["sessionLevel"].FirstOrDefault();
            var subjectFilter = Request.Form["subject"].FirstOrDefault();
            var paperCodeFilter = Request.Form["paperCode"].FirstOrDefault();
            var regionCodeFilter = Request.Form["regionCode"].FirstOrDefault();
            var reportType = string.IsNullOrEmpty(Request.Form["allCaptured"]) ? "" : Request.Form["allCaptured"].FirstOrDefault();

            // Fetch data from repository with filters
            var assessments = await _assessment_repository.GetPartiallyCapturedGradesAsync(
                sessionLevelFilter, subjectFilter, paperCodeFilter, regionCodeFilter,  reportType);

            // Store the total filtered count before applying search
            var recordsFiltered = assessments.Count;

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchValue))
            {
                assessments = assessments.Where(a =>
                    (a.ExaminerRecruitment.ExaminerName?.ToLower().Contains(searchValue) ?? false) ||
                    (a.ExaminerRecruitment.LastName?.ToLower().Contains(searchValue) ?? false) ||
                    (a.CapturerGrade?.ToLower().Contains(searchValue) ?? false) ||
                    (a.VerifierGrade?.ToLower().Contains(searchValue) ?? false) ||
                    (a.ExaminerRecruitment.CemId?.ToLower().Contains(searchValue) ?? false)
                ).ToList();
            }

            // Apply pagination
            var data = assessments.Skip(start).Take(length).ToList();

            return Json(new
            {
                draw = draw,
                recordsTotal = assessments.Count,
                recordsFiltered = recordsFiltered,
                data = data.Select(a => new
                {
                    a.Id,
                    FirstName = a.ExaminerRecruitment.ExaminerName,
                    LastName = a.ExaminerRecruitment.LastName,
                    CemId = a.ExaminerRecruitment.CemId,
                    Subject = a.ExaminerRecruitment.Subject,
                    PaperCode = a.ExaminerRecruitment.PaperCode,
                    Capturer = a.Capturer?.UserName,
                    Verifier = a.Verifier?.UserName,
                    //a.CapturerGrade,
                    //a.VerifierGrade,
                    Date = a.Date.ToString("yyyy-MM-dd")
                })
            });
        }

        public IActionResult GoodExaminerRecruitsEntiesReport()
        {

            return View();

        }

        public async Task<IActionResult> GetExaminerAssessmentReport()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
            var sessionLevelFilter = Request.Form["sessionLevel"].FirstOrDefault();
            var subjectFilter = Request.Form["subject"].FirstOrDefault();
            var paperCodeFilter = Request.Form["paperCode"].FirstOrDefault();
            var regionCodeFilter = Request.Form["regionCode"].FirstOrDefault();

            // Fetch data from repository
            var reports = await _assessment_repository.GetExaminerAssessmentReportAsync(sessionLevelFilter, subjectFilter, paperCodeFilter, regionCodeFilter);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                reports = reports.Where(r =>
                    r.Subject.ToLower().Contains(searchValue) ||
                    r.PaperCode.ToLower().Contains(searchValue)
                ).ToList();
            }

            // Store total filtered count before pagination
            var recordsFiltered = reports.Count;

            // Apply pagination
            var data = reports.Skip(start).Take(length).ToList();

            return Json(new
            {
                draw = draw,
                recordsTotal = reports.Count,
                recordsFiltered = recordsFiltered,
                data = data.Select(r => new
                {
                    r.Subject,
                    r.PaperCode,
                    r.GoodEntries,  
                    r.BadEntries,
                    r.Partially,
                    r.TotalPresentExaminers,
                    Percentage = $"{r.Percentage:F2}%"
                })
            });

        }



    }
}
