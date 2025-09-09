using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ExaminerPaymentSystem.Models;
using ExaminerPaymentSystem.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System;
using Microsoft.AspNetCore.Authorization;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Data;
using Microsoft.AspNetCore.Identity;
using ExaminerPaymentSystem.Models.Major;
using System.Diagnostics;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class MarksCapturedController : Controller
    {

        private readonly IExamCodesRepository _examCodesRepository;
        private readonly IExaminerRepository _examinerRepository;
        private readonly IPaperMarkingRateRepository _subjectCodesRepository;
        private readonly IMarksCapturedRepository _marksCapturedRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public MarksCapturedController(IExamCodesRepository examCodesRepository, IPaperMarkingRateRepository subjectCodesRepository, IExaminerRepository examinerRepository, IMarksCapturedRepository marksCapturedRepository,ITransactionRepository transactionRepository, SignInManager<ApplicationUser> signInManager)
        {
            _examCodesRepository = examCodesRepository;
            _subjectCodesRepository = subjectCodesRepository;
            _examinerRepository = examinerRepository;
            _marksCapturedRepository = marksCapturedRepository;
            _transactionRepository = transactionRepository;
            _signInManager = signInManager;
        }

        [Authorize]
        public async Task<IActionResult> Index(string ExaminerCode, string subjectCode, string paperCode, string regionCode)
        {
        

            return View();

        }

        [Authorize]
        public async Task<IActionResult> ScriptsData(string ExaminerCode, string subjectCode, string paperCode, string regionCode)
        {
            var examCodes = await _examCodesRepository.GetAllExamCodes();
            var subjectCodes = await _subjectCodesRepository.GetAllPaperCodes();
            var paperCodes = await _subjectCodesRepository.GetPaperCodeById(subjectCode);
            var marks = await _marksCapturedRepository.GetMarksCaptured(ExaminerCode, subjectCode, paperCode);
            //var scriptsmarked = await _

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

            ViewBag.Marks = marks.Select(e => new
            {
                e.RegionCode,
                e.TotalScriptsCaptured,
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

        [HttpPost]
        public async Task<IActionResult> Search(string examCode, string subjectCode, string paperCode, string regionCode)
        {
            try
            {
                var apportionedScripts = 0;// Example data
            
                var totalScriptsCaptured = 0;
                var absentScripts = 0;
                var pirateCandidates = 0;
                var exceptions = 0;
                var accountsTotalScriptCaptured = 0;

                var marksCapturedData = await _marksCapturedRepository.GetComponentMarkCaptured(examCode,subjectCode,paperCode,regionCode);
                var scriptMarkedData = await _transactionRepository.CheckExaminerInTransactions(examCode, subjectCode, paperCode, regionCode);
                var scriptTotal = 0;
                if (scriptMarkedData.Any())
                {
                    foreach (var script in scriptMarkedData)
                    {
                        scriptTotal += script.SCRIPTS_MARKED ?? 0;
                    }
                }

                if (marksCapturedData != null)
                {
                    apportionedScripts = marksCapturedData.ApportionedScripts; // Example data
                        // Example data
                    totalScriptsCaptured = marksCapturedData.TotalScriptsCaptured;
                    absentScripts = marksCapturedData.AbsentScripts;
                    pirateCandidates = marksCapturedData.PirateCandidates;
                    exceptions = marksCapturedData.Exceptions;
                   accountsTotalScriptCaptured = marksCapturedData.AccountsTotalScriptCaptured;
                }

                // Perform your search logic here
                var data = new
                {
                    apportionedScripts, // Example data
                    scriptMarked = scriptTotal,        // Example data
                    totalScriptsCaptured,
                    absentScripts,
                    pirateCandidates,
                    exceptions,
                    accountsTotalScriptCaptured,
                    subjectCode,
                    examCode,
                    paperCode,
                    regionCode

                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Save(string examCode, string subjectCode, string paperCode, string regionCode, int apportionedScripts, int scriptMarked, int totalScriptsCaptured, int absentScripts, int pirateCandidates, int exceptions,int accountsTotalScriptCaptured)
        {
            try
            {

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var newMarks = new MarksCaptured()
                {
                    ExamCode = examCode,
                    SubjectCode = subjectCode.Substring(3),
                    PaperCode = paperCode,
                    RegionCode = regionCode,
                    AbsentScripts = absentScripts,
                    PirateCandidates = pirateCandidates,
                    ApportionedScripts = apportionedScripts,
                    AccountsTotalScriptCaptured = accountsTotalScriptCaptured,
                    ScriptMarked = scriptMarked,
                    TotalScriptsCaptured = totalScriptsCaptured,
                    Exceptions = exceptions,

                };

                var result = await _marksCapturedRepository.InsertExaminerTransactions(newMarks,currentUser);

                if (!result.Success)
                {

                    return Json(new { success = false, message = result.Message });
                }
                // Perform your save logic here
                return Json(new { success = true, message = "Data saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> AccountsMarksCaptured(string ExaminerCode, string subjectCode, string paperCode, string regionCode)
        {
            var examCodes = await _examCodesRepository.GetAllExamCodes();
            var subjectCodes = await _subjectCodesRepository.GetAllPaperCodes();
            var paperCodes = await _subjectCodesRepository.GetPaperCodeById(subjectCode);
            var marks = await _marksCapturedRepository.GetMarksCaptured(ExaminerCode, subjectCode, paperCode);


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

            ViewBag.Marks = marks.Select(e => new
            {
                e.RegionCode,
                e.TotalScriptsCaptured,
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



        //[Authorize]
        //[HttpPost]
        //public async Task<ActionResult> InsertExaminerTransactions([FromBody] RootObjectMarksCaptured jsonData)
        //{
        //    try
        //    {
        //        if (jsonData != null)
        //        {
        //            await _marksCapturedRepository.InsertExaminerTransactions(jsonData);

        //            return Json(new { success = true, message = "Data inserted successfully" });
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "JSON data is null" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "An error occurred while inserting data into EXM_SCRIPT_CAPTURED table." + ex.Message });
        //    }
        //}


        //update marked scripts
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMarksCaptured(string ExaminerCode, string subjectCode, string paperCode, string regionCode)
        {
            // Implement logic to fetch subjects based on the selected exam session
            var marks = await _marksCapturedRepository.GetMarksCaptured(ExaminerCode, subjectCode, paperCode);
            //ViewBag.papercodes = papercodes;
            return Json(marks);
        }



    }

    public class RootObjectMarksCaptured
    {
        public string SUBJECT_CODE { get; set; }
        public string PAPER_CODE { get; set; }
        public string Exam_Code { get; set; }
        public string Region_Code { get; set; }

        public int TotalScriptsCaptured { get; set; }

        public int ScriptMarked { get; set; }

        public int AccountsTotalScriptCaptured { get; set; }

        public int AbsentScripts { get; set; }

        public int ApportionedScripts { get; set; }

        public int PirateCandidates {  get; set; }

        public int Exceptions {  get; set; }

        public int Id { get; set; }



      

    }
}
