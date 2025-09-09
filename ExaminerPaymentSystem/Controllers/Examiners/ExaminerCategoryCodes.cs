using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System;
using ExaminerPaymentSystem.Data;
using Microsoft.AspNetCore.Identity;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Repositories.Examiners;


namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class ExaminerCategoryCodesController : Controller
    {

        private readonly IExamCodesRepository _examCodesRepository;
        private readonly IExaminerRepository _examinerRepository;
        private readonly IPaperMarkingRateRepository _subjectCodesRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITransactionRepository _transactionRepository;
        //private readonly IExaminerScriptsMarkedRepository _examinerRepository;

        public ExaminerCategoryCodesController(IExamCodesRepository examCodesRepository, IPaperMarkingRateRepository subjectCodesRepository, IExaminerRepository examinerRepository, SignInManager<ApplicationUser> signInManager,TransactionRepository transactionRepository)
        {
            _examCodesRepository = examCodesRepository;
            _subjectCodesRepository = subjectCodesRepository;
            _examinerRepository = examinerRepository;
            _signInManager = signInManager;
            _transactionRepository = transactionRepository;
        }

        public async Task<IActionResult> Index(string ExaminerCode, string subjectCode, string paperCode, string searchBmsCode)
        {
            var examCodes = await _examCodesRepository.GetAllExamCodes();
            var subjectCodes = await _subjectCodesRepository.GetAllPaperCodes();
            var paperCodes = await _subjectCodesRepository.GetPaperCodeById(subjectCode);
            var examiners = await _transactionRepository.GetTeamByBms(ExaminerCode, subjectCode, paperCode, searchBmsCode);

            //ViewBag.ExamCodes = examCodes.Select(e => new
            //{
            //    EXM_EXAM_CODE = e.EXM_EXAM_CODE,
            //    ExamCaption = $"{e.EXM_EXAM_LEVEL} {e.EXM_EXAM_SESSION} {e.EXM_EXAM_YEAR}"
            //}).ToList();

            ViewBag.ExamCodes = examCodes
             .Where(e => int.TryParse(e.EXM_EXAM_CODE, out int code) && code >= 100 && e.ACTIVATED_SESSION == "Activated") // Filter condition
             .Select(e => new
            {
                e.EXM_EXAM_CODE,
                ExamCaption = $"{e.EXM_EXAM_LEVEL} {e.EXM_EXAM_SESSION} {e.EXM_EXAM_YEAR}"
             })
             .ToList();

            ViewBag.SubjectCodes = subjectCodes.Select(e => new
            {
                PPR_SUB_SUB_ID = e.SUB_SUB_ID,
                SubjectCaption = $"{e.SUB_SUBJECT_DESC}"
            }).ToList();

            ViewBag.PpaperCodes = paperCodes.Select(e => new
            {
                e.PPR_PAPER_CODE,
                PaperCaption = $"{e.PPR_PAPER_CODE}"
            }).ToList();

            // Check if subjectCode is provided and get paper codes accordingly
            if (!string.IsNullOrEmpty(subjectCode))
            {
                ViewBag.PaperCodes = await _subjectCodesRepository.GetPaperCodeById(subjectCode);
            }

            return View();
        }

        //[HttpGet]
        //public async Task<IActionResult> GetTeamByBms(String ExaminerCode, String SubjectCode, String paperCode, String searchBmsCode)
        //{
        //    // Implement logic to fetch subjects based on the selected exam session
        //    var examiners = await _examinerRepository.GetTeamByBms(ExaminerCode, SubjectCode, paperCode, searchBmsCode);
        //    //ViewBag.papercodes = papercodes;
        //    return Json(examiners);
        //}

        [HttpGet]
        public async Task<IActionResult> GetTeamByBms(string ExaminerCode, string SubjectCode, string paperCode, string searchBmsCode)
        {
            try
            {
            // Check for records in EXAMINER_TRANSACTIONS table
            var examinerTransactions = await _transactionRepository.CheckExaminerTransactions(ExaminerCode, SubjectCode, paperCode, searchBmsCode);

            if (examinerTransactions.Any())

                   
            {
                // Display results from EXAMINER_TRANSACTIONS table
                return Json(examinerTransactions);
            }
            else
            {
                // If no records in EXAMINER_TRANSACTIONS table, search in EXM_EXAMINER_MASTER table
                var examiners = await _transactionRepository.GetTeamByBms(ExaminerCode, SubjectCode, paperCode, searchBmsCode);

                // Display results from EXM_EXAMINER_MASTER table
                return Json(examiners);
            }
        }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }



        [HttpPost]
        public async Task<ActionResult> UpdateExaminerScriptsMarked([FromBody] JsonObject jsonData)
        {
            try
            {
                // Check if jsonData is not null and log the JSON data
                if (jsonData != null)
                {
                    var EMS_EXAMINER_NUMBER = jsonData["EMS_EXAMINER_NUMBER"];
                    RootObject rootObject = System.Text.Json.JsonSerializer.Deserialize<RootObject>(jsonData.ToString());
                    string jsonString = jsonData.ToString();


                    // Process and manipulate the JSON data as needed

                    // Save data to the database or perform other operations

                    return Json(new { success = true, message = "Data saved successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "JSON data is null" });
                }
            }
            catch (Exception ex)
            {
                // Handle exception and return error response
                return Json(new { success = false, message = "An error occurred while updating paper marking rate." + ex.Message });
            }
        }

        //new method to insert

        //[HttpPost]
        //public async Task<ActionResult> InsertExaminerTransactions([FromBody] JsonObject jsonData)
        //{
        //    try
        //    {
        //        if (jsonData != null)
        //        {
        //            RootObject data = JsonConvert.DeserializeObject<RootObject>(jsonData.ToString());


        //            List<ExaminerScriptsMarked> examiners = System.Text.Json.JsonSerializer.Deserialize<List<ExaminerScriptsMarked>>(jsonData["tableData"].ToString());
        //            //var EMS_EXAMINER_CODE = examiners.FirstOrDefault().EMS_EXAMINER_NUMBER;

        //            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

        //            if (currentUser == null)
        //            {

        //                // User is not authenticated, handle accordingly
        //                return RedirectToAction("Login", "Account"); // Redirect to login page

        //            }

        //            await _transactionRepository.InsertExaminerTransactions(data,currentUser);

        //            // Process and manipulate the JSON data as needed

        //            return Json(new { success = true, message = "Data inserted successfully" });
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "JSON data is null" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "An error occurred while inserting data into EXAMINER_TRANSACTIONS table." + ex.Message });
        //    }
        //}



    }

    public class RootObjects
    {
        public string SUBJECT_CODE { get; set; }
        public string PAPER_CODE { get; set; }
        public string Exam_Code { get; set; }
        public string searchBmsCode { get; set; }
        public List<ExaminerScriptsMarked> tableData { get; set; } // This completes the definition
    }
}
