using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.XPath;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class TransactionController : Controller
    {
        private readonly IExaminerRepository _examinerRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaperMarkingRateRepository _paperMarkingRate;
        private readonly ICategoryRateRepository _categoryMarkingRate;
        private readonly IExamCodesRepository _examCodesRepository;
        private readonly IRateAndTaxRepository _taxRepository;
        private readonly ApplicationDbContext _context;


        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionController(IExaminerRepository examinerRepository, ITransactionRepository transactionRepository, IPaperMarkingRateRepository paperMarkingRate, ICategoryRateRepository categoryMarkingRate, IExamCodesRepository examCodesRepository,IRateAndTaxRepository taxRepository, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _examinerRepository = examinerRepository;
            _transactionRepository = transactionRepository;
            _paperMarkingRate = paperMarkingRate;
            _categoryMarkingRate = categoryMarkingRate;
            _examCodesRepository = examCodesRepository;
            _taxRepository = taxRepository;
            _signInManager = signInManager;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index(string examCode, string subjectCode, string paperCode, string regionCode)
        {
           

            return View();
        }

        [Authorize]
        public async Task<IActionResult> RateAndTax(bool isSuccess = false)
        {
            var rateandtax = await _taxRepository.GetFirstTaxAndRate();
            ViewBag.IsSuccess = isSuccess;

            return View(rateandtax);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateRateAndTax(RateAndTax model)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            await  _taxRepository.UpdateRate(model,currentUser.Id);
            return RedirectToAction("RateAndTax", new { isSuccess = true });
        }

        [Authorize]
        public async Task<IActionResult> GetTransactions(string examCode, string subjectCode, string paperCode, string capturingRate, string regionCode)
        {

            if (capturingRate == null)
            {
                return Json(new { success = false, message = "Enter Capturing Rate", subject = subjectCode, paperCode });
            }

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var sub = subjectCode.Substring(3);

            var papermarkingrate = await _paperMarkingRate.GetPaperMarkingRate(sub, paperCode, examCode);

            if (papermarkingrate == null || papermarkingrate.PPR_EXAM_TYPE == null)
            {
                return Json(new { success = false, message = "Paper rate is missing", subjectCode = sub, paperCode });
            }


       var result = await _transactionRepository.PerformOperationsOnExceptMultipleChoicePapers(examCode, sub, paperCode, capturingRate, regionCode,currentUser);

                if (!result.Success)
                {
                    return Json(new { success = false,message = result.Message, subjectCode = sub, paperCode });
                }

            var mergedExaminerList = new List<TransactionReportNow>();

            if (result.Success)
            {
                var examiners = await _transactionRepository.GetComponentExaminer(examCode, sub, paperCode, regionCode);

                mergedExaminerList = examiners
    .Where(item => item.EMS_COMPILED_STATUS == "Compiled"
        && item.EMS_APPROVED_STATUS == "Approved"
        && item.EMS_CERTIFIED_STATUS == "Certified"
        && item.EMS_CENTRE_SUPERVISOR_STATUS == "Approved")
    .Select(item => new TransactionReportNow
    {
        ExaminerNumber = item.EMS_EXAMINER_NUMBER,
        Name = item.Examiner.EMS_EXAMINER_NAME + " " + item.Examiner.EMS_LAST_NAME,
        IdNumber = item.EMS_NATIONAL_ID,
        Subject = item.EMS_SUB_SUB_ID.Substring(3, 4) + "/" + item.EMS_PAPER_CODE,
        ScriptsMarked = item.SCRIPTS_MARKED.GetValueOrDefault(),
        Category = item.EMS_ECT_EXAMINER_CAT_CODE,
        Responsibility = item.RESPONSIBILITY_FEES.GetValueOrDefault().ToString("F2"),
        Coodination = item.COORDINATION_FEES.GetValueOrDefault().ToString("F2"),
        Capturing = item.CAPTURING_FEES.GetValueOrDefault().ToString("F2"),
        GrandTotal = item.GRAND_TOTAL.GetValueOrDefault().ToString("F2"),
        FinalTotal = (item.CAPTURING_FEES.GetValueOrDefault() + item.GRAND_TOTAL.GetValueOrDefault()).ToString("F2")

    })
    .ToList();


            }




            // Return a JSON response indicating success
            return Json(new
            {
                success = true,
                subjectCode = sub,
                paperCode,
                data = mergedExaminerList // Replace with actual list
            });

        }

        [Authorize(Roles = "Admin,SuperAdmin,ExamsAdmin")]
        [HttpGet]
        public async Task<IActionResult> ImportExaminerTransactionFromOracle()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,ExamsAdmin")]
        [HttpPost]
        public async Task<IActionResult> ImportExaminerTransactionFromOracle(string examCode, string subjectCode, string paperCode, string? regionCode)
        {
            var result = await _transactionRepository.ImportExaminerTransactionFromOracle(examCode, subjectCode, paperCode, regionCode);
            if (result)
            {
                return Ok();
            }
            return NotFound();
        }

        [Authorize(Roles = "Admin,SuperAdmin,ExamsAdmin")]
        [HttpGet]
        public async Task<IActionResult> ExportExaminerTransactionToOracle()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,ExamsAdmin")]
        [HttpPost]
        public async Task<IActionResult> ExportExaminerTransactionToOracle(string examCode, string subjectCode, string paperCode, string? regionCode)
        {
            var result = await _transactionRepository.ExportExaminerTransactionToOracle(examCode, subjectCode, paperCode, regionCode);
            if (result)
            {
                return Ok();
            }
            return NotFound();
        }

        [Authorize(Roles = "Admin,SuperAdmin,ExamsAdmin")]
        [HttpGet]
        public async Task<IActionResult> ExportExaminerRefCatToOracle()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,ExamsAdmin")]
        [HttpPost]
        public async Task<IActionResult> ExportExaminerRefCatToOracle(string examCode, string subjectCode, string paperCode, string? regionCode)
        {
            var result = await _transactionRepository.ExportRefCatToOracle(examCode, subjectCode, paperCode, regionCode);
            if (result)
            {
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string examCode, string subjectCode, string paperCode, string regionCode)
        {
            try
            {
                List<Apportioned> apportionedResults = new List<Apportioned>();

                if (!string.IsNullOrEmpty(regionCode))
                {
                    apportionedResults = await _context.REF_CAT_PAPER
             .Where(x => x.CTP_PPR_SUB_PAPER_CODE ==  subjectCode + paperCode && x.CTP_REGION_CODE == regionCode

             )
             .ToListAsync();
                }
                else
                {
                    apportionedResults = await _context.REF_CAT_PAPER
             .Where(x => x.CTP_PPR_SUB_PAPER_CODE ==  subjectCode + paperCode

             )
             .ToListAsync();
                }

          

                return Json(new { success = true, data = apportionedResults });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        //[Authorize(Roles = "Admin,SuperAdmin")]
        //[HttpPost]
        //public async Task<IActionResult> ExportExaminersToOracle(string examCode, string subjectCode, string paperCode, string? regionCode)
        //{
        //    var result = await _examinerRepository.ExportExaminersToOracle(examLevel);
        //    if (result)
        //    {
        //        return RedirectToAction(nameof(ExaminerList));
        //    }
        //    return View(false);
        //}

        private readonly List<string> defaultActivities = new List<string>
    {
         "BOOK SELECTION", "CAPTURING", "COORDINATION", "GRADING",
        "ITEM WRITING", "MODERATION", "PROOF READING", "REMARKING", "TRAINING",
        "TRANSCRIBE", "VETTING"
    };

        [HttpGet]
        public async Task<IActionResult> ActivityRate()
        {
            var existingRates = await _context.ActivityRates.ToListAsync();

            var model = defaultActivities
                .Select(a => existingRates.FirstOrDefault(x => x.Activity == a) ?? new ActivityRate { Activity = a })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult SettingFees()
        {
      

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveActivityRates(List<ActivityRate> model)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            foreach (var rate in model)
            {
                var existing = await _context.ActivityRates.FirstOrDefaultAsync(x => x.Activity == rate.Activity);
                if (existing != null)
                {
                    existing.PercentageOfSetting = rate.PercentageOfSetting;
                    _context.ActivityRates.Update(existing);
                }
                else
                {
                    _context.ActivityRates.Add(rate);
                }
            }

          await  _context.SaveChangesAsync(currentUser.Id);

            TempData["Success"] = "Activity rates saved successfully!";
            return RedirectToAction("CreateActivityRates");
        }


        [HttpPost]
        public async Task<IActionResult> Search1(string examCode, string subjectCode, string paperCode)
        {
            try
            {
                decimal? settingFee = null;

                var settingFeeData = await _taxRepository.GetSettingRateBySubjectAsync(examCode, subjectCode, paperCode);

                if (settingFeeData != null)
                {
                    settingFee = settingFeeData.SettingFee;
                }

                var data = new
                {
                    settingFee,
                    subjectCode,
                    examCode,
                    paperCode
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> Save(string examCode, string subjectCode, string paperCode, int settingFee)
        {
            try
            {

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var newMarks = new SettingRate()
                {
                    SubSubId = examCode + subjectCode.Substring(3),
                   
                    PaperCode = paperCode,
                    SettingFee = settingFee,
                 

                };

           await _taxRepository.UpdateSettingRateAsync(newMarks, currentUser.Id);

           
                // Perform your save logic here
                return Json(new { success = true, message = "Data saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}
