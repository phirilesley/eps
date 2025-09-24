using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2013.Excel;
using ElmahCore;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Extensions;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.ViewModels.Examiners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            T[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new T[size];

                bucket[count++] = item;
                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
                yield return bucket.Take(count);
        }
    }
    public class ExaminerController : Controller
    {

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
        private readonly ILogger<ExaminerController> _logger;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ApplicationDbContext _context;

        // Constructor to inject the repository dependency
        public ExaminerController(IExaminerRepository examinerRepository, SignInManager<ApplicationUser> signInManager,
           IPaperMarkingRateRepository paperMarkingRate, ICategoryRateRepository categoryMarkingRate, IExamCodesRepository examCodesRepository, IBanksRepository banksRepository, UserManager<ApplicationUser> userManager, IUserRepository userRepository, ITandSRepository tandSRepository, IRegisterRepository registerRepository, IMaxExaminerCodeRepository examinerCodeRepository, ILogger<ExaminerController> logger, ITransactionRepository transactionRepository, ApplicationDbContext context)
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
            _transactionRepository = transactionRepository;
            _context = context;
        }


        public async Task<IActionResult> BulkAddExaminers()
        {
            return View();
        }

        public async Task<IActionResult> DownloadExaminerTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Examiners");

                // Header row
                worksheet.Cell(1, 1).Value = "EMS_NATIONAL_ID";
                worksheet.Cell(1, 2).Value = "EMS_EXAMINER_NAME";
                worksheet.Cell(1, 3).Value = "EMS_LAST_NAME";
                worksheet.Cell(1, 4).Value = "EMS_SEX";
                

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "ExaminerTemplate.xlsx");
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> UploadExaminers(IFormFile file, string subjectCode = "", string paperCode = "", string regionCode = "")
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return RedirectToAction("BulkAddExaminers");
            }

            // Create an execution strategy
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    _context.ChangeTracker.Clear();

                    var examiners = new List<Examiner>();
                    var nationalIdsInFile = new HashSet<string>();
                    var duplicateNationalIds = new HashSet<string>();
                    var processedCount = 0;

                    // Get existing examiner codes to prevent duplicates
                    var existingExaminerCodes = await _context.EXM_EXAMINER_MASTER
                        .AsNoTracking()
                        .Select(e => e.EMS_EXAMINER_CODE)
                        .ToListAsync();

                    // Get the highest numeric examiner code
                    int maxCode = 1000;
                    var numericCodes = existingExaminerCodes
                        .Where(c => int.TryParse(c, out _))
                        .Select(c => int.Parse(c))
                        .ToList();

                    if (numericCodes.Any())
                    {
                        maxCode = numericCodes.Max();
                    }

                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        using (var workbook = new XLWorkbook(stream))
                        {
                            var worksheet = workbook.Worksheet(1);
                            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                            foreach (var row in rows)
                            {
                                var nationalId = row.Cell(1).GetValue<string>()?.Trim();

                                if (string.IsNullOrWhiteSpace(nationalId))
                                {
                                    continue;
                                }

                                if (nationalIdsInFile.Contains(nationalId))
                                {
                                    duplicateNationalIds.Add(nationalId);
                                    continue;
                                }
                                nationalId = Regex.Replace(nationalId, "[^a-zA-Z0-9]", "").ToUpper();
                                nationalIdsInFile.Add(nationalId);
                                processedCount++;
                                maxCode++;

                                // Generate a unique examiner code
                                string examinerCode;
                                do
                                {
                                    examinerCode = maxCode.ToString("D8");
                                    maxCode++;
                                } while (existingExaminerCodes.Contains(examinerCode));

                                var examiner = new Examiner
                                {
                                    EMS_EXAMINER_CODE = examinerCode,
                                    EMS_NATIONAL_ID = nationalId,
                                    EMS_EXAMINER_NAME = row.Cell(2).GetValue<string>()?.Trim(),
                                    EMS_LAST_NAME = row.Cell(3).GetValue<string>()?.Trim(),
                                    EMS_SEX = (row.Cell(4).GetValue<string>()?.Trim() ?? "M").ToUpper(),
                                    EMS_SUB_SUB_ID = subjectCode?.Trim(),
                                    EMS_PAPER_CODE = paperCode?.Trim(),
                                    EMS_ECT_EXAMINER_CAT_CODE = "E",
                                    EMS_SUBKEY = $"{subjectCode}{paperCode}{nationalId}"
                                };

                                examiner.EMS_REGION_CODE = regionCode?.Trim();
                                if (subjectCode?.StartsWith("7") == true)
                                {
                                    examiner.EMS_MARKING_REG_CODE = regionCode?.Trim();
                                }

                                examiner.EMS_LEVEL_OF_EXAM_MARKED = subjectCode switch
                                {
                                    string s when s.StartsWith("7") => "7",
                                    string s when s.StartsWith("4") => "O",
                                    string s when s.StartsWith("6") || s.StartsWith("5") => "A",
                                    _ => null
                                };

                                examiners.Add(examiner);
                            }
                        }
                    }

                    // Final duplicate check before insert
                    var existingRecords = await _context.EXM_EXAMINER_MASTER
                        .AsNoTracking()
                        .Where(e => examiners.Select(x => x.EMS_NATIONAL_ID).Contains(e.EMS_NATIONAL_ID))
                        .ToListAsync();

                    var examinerNotInDB = examiners
                        .Where(e => !existingRecords.Any(x =>
                            x.EMS_NATIONAL_ID == e.EMS_NATIONAL_ID))
                        .GroupBy(e => e.EMS_EXAMINER_CODE)  // Group by code to ensure uniqueness
                        .Select(g => g.First())              // Take first from each group
                        .ToList();

                    if (examinerNotInDB.Any())
                    {
                        _context.ChangeTracker.Clear();
                        await _context.EXM_EXAMINER_MASTER.AddRangeAsync(examinerNotInDB);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    var message = $"{processedCount} examiner(s) processed, {examinerNotInDB.Count} new examiner(s) added.";
                    if (duplicateNationalIds.Any())
                    {
                        message += $" {duplicateNationalIds.Count} duplicate(s) skipped in file.";
                    }
                    if (examiners.Count - examinerNotInDB.Count > 0)
                    {
                        message += $" {examiners.Count - examinerNotInDB.Count} existing examiner(s) skipped.";
                    }

                    TempData["Success"] = message;
                    return RedirectToAction("BulkAddExaminers");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "An error occurred: " + ex.Message;
                    return RedirectToAction("BulkAddExaminers");
                }
            });
        }
        // Extension method for batching


        [Authorize(Roles = "PMS, BMS,Examiner,DPMS,RPMS,A,BT,PBT")]
        [HttpGet]
        public async Task<IActionResult> Index(bool isSuccess = false, string message = "", string msg = "")
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            string idnumber = currentUser.IDNumber;

           

            var examiner = await _examinerRepository.GetExaminerRecord(idnumber);
            var transaction = examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == currentUser.EMS_SUBKEY);
            int totalFields = 11;
            int filledFields = 0;

            if (examiner != null)
            {

                if (!string.IsNullOrEmpty(examiner.EMS_EXAMINER_NAME) && examiner.EMS_EXAMINER_NAME != "NULL" && examiner.EMS_EXAMINER_NAME != "default_value")
                    filledFields++;

                if (!string.IsNullOrEmpty(examiner.EMS_LAST_NAME) && examiner.EMS_LAST_NAME != "NULL" && examiner.EMS_LAST_NAME != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_NATIONAL_ID) && examiner.EMS_NATIONAL_ID != "NULL" && examiner.EMS_NATIONAL_ID != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_SEX) && examiner.EMS_SEX != "NULL" && examiner.EMS_SEX != "default_value")
                    filledFields++;

                if (!string.IsNullOrEmpty(examiner.EMS_QUALIFICATION) && examiner.EMS_QUALIFICATION != "NULL" && examiner.EMS_QUALIFICATION != "default_value")
                    filledFields++;

                if (!string.IsNullOrEmpty(examiner.EMS_PHONE_HOME) && examiner.EMS_PHONE_HOME != "NULL" && examiner.EMS_PHONE_HOME != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_ACCOUNT_NO_FCA) && examiner.EMS_ACCOUNT_NO_FCA != "NULL" && examiner.EMS_ACCOUNT_NO_FCA != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_ACCOUNT_NO_ZWL) && examiner.EMS_ACCOUNT_NO_ZWL != "NULL" && examiner.EMS_ACCOUNT_NO_ZWL != "default_value")
                    filledFields++;

                if (!string.IsNullOrEmpty(examiner.EMS_BANK_NAME_FCA) && examiner.EMS_BANK_NAME_FCA != "NULL" && examiner.EMS_BANK_NAME_FCA != "default_value")
                    filledFields++;

                if (!string.IsNullOrEmpty(examiner.EMS_BANK_NAME_ZWL) && examiner.EMS_BANK_NAME_ZWL != "NULL" && examiner.EMS_BANK_NAME_ZWL != "default_value")
                    filledFields++;

                if (!string.IsNullOrEmpty(examiner.EMS_MARKING_EXPERIENCE) && examiner.EMS_MARKING_EXPERIENCE != "NULL" && examiner.EMS_MARKING_EXPERIENCE != "default_value")
                    filledFields++;

            }

            double percentageFilled = (double)filledFields / totalFields * 100;

            ViewBag.PercentageFilled = percentageFilled;
            ViewBag.IsSuccess = isSuccess;
            ViewBag.Message = message;
            ViewBag.EMS_SUB_SUB_ID_Substring = examiner.EMS_SUB_SUB_ID;

            if (isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["SuccessMessage"] = "Record Updated Successfully.";
                return View(examiner);
            }
            else if (!isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = message;

            }

            if (!string.IsNullOrEmpty(msg) && msg == "ProfileIncomplete")
            {
                TempData["SMessage"] = "Update Record First.";


            }
            else
            {
                TempData["SMessage"] = null;
            }

            if(transaction != null)
            {
                examiner.EMS_SUB_SUB_ID = transaction.EMS_SUB_SUB_ID.Substring(3);
                examiner.EMS_PAPER_CODE = transaction.EMS_PAPER_CODE;
                examiner.EMS_ECT_EXAMINER_CAT_CODE = transaction.EMS_ECT_EXAMINER_CAT_CODE;
                examiner.EMS_EXAMINER_NUMBER = transaction.EMS_EXAMINER_NUMBER;
                examiner.EMS_SRC_SUPERORD = transaction.EMS_EXM_SUPERORD;
            }


            return View(examiner);
        }




        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAbsentExaminersByCategory(string categoryCode, string subSubId, string paperCode, string regionCode, string activity, string examCode)
        {
            var examiners = await _examinerRepository.GetAbsentExaminersByCategoryAsync(categoryCode, examCode, subSubId, paperCode, activity, regionCode);
            return Json(examiners);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetPresentBMS(string subSubId, string paperCode, string regionCode, string activity, string examCode)
        {
            var examiners = await _examinerRepository.GetSuperordsBySubSubIdAndPaperCodeAsync(examCode, subSubId, paperCode, activity, regionCode);
            return Json(examiners);
        }

        [Authorize]
        public async Task<IActionResult> ChangeTeam(string idNumber = "", string examCode = "", string activity = "", string subjectCode = "", string paperCode = "", bool isSuccess = false, string message = "")
        {
            var examiner = await _examinerRepository.GetExaminerRecord(idNumber);




            if (examiner == null)
            {
                return NotFound();
            }
            var examinerTransaction = examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID && a.EMS_ACTIVITY == activity && a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode);

            examiner.EMS_SUB_SUB_ID = subjectCode;
            examiner.EMS_PAPER_CODE = paperCode;
            if (examinerTransaction != null)
            {
                examiner.EMS_ECT_EXAMINER_CAT_CODE = examinerTransaction.EMS_ECT_EXAMINER_CAT_CODE;
                examiner.EMS_EXAMINER_NUMBER = examinerTransaction.EMS_EXAMINER_NUMBER;
                examiner.EMS_MARKING_REG_CODE = examinerTransaction.EMS_MARKING_REG_CODE;
                examiner.EMS_EXM_SUPERORD = examinerTransaction.EMS_EXM_SUPERORD;
            }

            ViewBag.examCode = examCode;
            ViewBag.activity = activity;

            if (!isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["Error"] = message;

            }

            return View(examiner);
        }


        [HttpGet]
        public async Task<IActionResult> ChangeRole(string idNumber = "", string examCode = "", string activity = "", string subjectCode = "", string paperCode = "", bool isSuccess = false, string message = "")
        {
            var examiner = await _examinerRepository.GetExaminerRecord(idNumber);

            if (examiner == null)
            {
                return NotFound();
            }

            var examinerTransaction = examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID && a.EMS_ACTIVITY == activity && a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode);
            examiner.EMS_SUB_SUB_ID = subjectCode;
            examiner.EMS_PAPER_CODE = paperCode;
            if (examinerTransaction != null)
            {
                examiner.EMS_ECT_EXAMINER_CAT_CODE = examinerTransaction.EMS_ECT_EXAMINER_CAT_CODE;
                examiner.EMS_EXAMINER_NUMBER = examinerTransaction.EMS_EXAMINER_NUMBER;
                examiner.EMS_MARKING_REG_CODE = examinerTransaction.EMS_MARKING_REG_CODE;
                examiner.EMS_EXM_SUPERORD = examinerTransaction.EMS_EXM_SUPERORD;
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
        [HttpPost]
        public async Task<IActionResult> ChangeRoleAndTeam(Examiner examiner, string examCode = "", string activity = "")
        {

            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                examiner.EMS_SUBKEY = examCode + examiner.EMS_SUB_SUB_ID + examiner.EMS_PAPER_CODE + activity + examiner.EMS_NATIONAL_ID;

                var newTransction = new ExaminerScriptsMarked()
                {

                    EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,

                    EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE,
                    EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER,
                    EMS_EXM_SUPERORD = examiner.EMS_EXM_SUPERORD,
                    EMS_SUB_SUB_ID = examCode + examiner.EMS_SUB_SUB_ID,
                    EMS_PAPER_CODE = examiner.EMS_PAPER_CODE,
                    EMS_ACTIVITY = activity,
                    EMS_SUBKEY = examiner.EMS_SUBKEY,
                    EMS_MARKING_REG_CODE = examiner.EMS_MARKING_REG_CODE,

                };


                var result = await _transactionRepository.UpdateExaminerToTransaction(newTransction, currentUser.Id);

                if (result.Success)
                {

                    var user = await _userRepository.GetUser(examiner.EMS_NATIONAL_ID, examiner.EMS_SUBKEY);

                    if (user != null)
                    {

                        var results = await _userManager.UpdateAsync(user);

                        if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "E")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "Examiner");
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
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
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
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "BT")
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
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "PMS")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "PMS");
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
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "BMS")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "BMS");
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
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "DPMS")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "DPMS");
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
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "RPMS")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "RPMS");
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

                    return RedirectToAction("ChangeRole", new
                    {


                        idNumber = examiner.EMS_NATIONAL_ID,
                        examCode,
                        subjectCode = examiner.EMS_SUB_SUB_ID,
                        paperCode = examiner.EMS_PAPER_CODE,
                        activity,
                        isSuccess = false,
                        message = result.Message,


                    });
                }

                return RedirectToAction(nameof(Assignments), new { isSuccess = true, message = "Updated Successfully" });
            }
            catch (Exception ex)
            {

                ElmahExtensions.RaiseError(ex);

                return RedirectToAction("ChangeRole", new
                {
                    examinercode = examiner.EMS_EXAMINER_CODE,
                    subKey = examiner.EMS_SUBKEY,
                    idNumber = examiner.EMS_NATIONAL_ID,
                    examCode,
                    subjectCode = examiner.EMS_SUB_SUB_ID,
                    paperCode = examiner.EMS_PAPER_CODE,
                    activity,
                    isSuccess = false,
                    message = ex.Message,


                });
            }


        }

        [Authorize(Roles = "Admin,SuperAdmin,ExamsAdmin")]
        [HttpGet]
        public async Task<IActionResult> ImportExaminersFromOracle()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,ExamsAdmin")]
        [HttpPost]
        public async Task<IActionResult> ImportExaminersFromOracle(string examLevel)
        {
            var result = await _examinerRepository.ImportExaminersFromOracle(examLevel);
            if (result)
            {
                return RedirectToAction(nameof(ExaminerList));
            }
            return View(false);
        }

        [Authorize(Roles = "Admin,SuperAdmin,ExamsAdmin")]
        [HttpGet]
        public async Task<IActionResult> ExportExaminersToOracle()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,ExamsAdmin")]
        [HttpPost]
        public async Task<IActionResult> ExportExaminersToOracle(string examLevel)
        {
            var result = await _examinerRepository.ExportExaminersToOracle(examLevel);
            if (result)
            {
                return RedirectToAction(nameof(ExaminerList));
            }
            return View(false);
        }



        [Authorize(Roles = "Admin,SuperAdmin,SubjectManager,CentreSupervisor")]
        [HttpGet]
        public async Task<IActionResult> AdminEdit(string idNumber = "", string examCode = "", string activity = "", bool isSuccess = false, string message = "")
        {
            var examiner = await _examinerRepository.GetExaminerRecord(idNumber);
            if (examiner == null)
            {
                return NotFound();
            }
            ViewBag.examCode = examCode;
            ViewBag.activity = activity;

            if (!isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["Error"] = message;

            }
            return View(examiner);
        }

        [Authorize(Roles = "Admin,SuperAdmin,SubjectManager,CentreSupervisor")]
        [HttpPost]
        public async Task<IActionResult> AdminEdit(Examiner examiner, string activity, string examCode)
        {

            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                examiner.EMS_SUBKEY = examCode + examiner.EMS_SUB_SUB_ID + examiner.EMS_PAPER_CODE + activity + examiner.EMS_NATIONAL_ID;

                // Save the changes
                var result = await _examinerRepository.AdminUpdateExaminerDetails(examiner, examCode, currentUser.Id, activity);

                if (!result.Success)
                {
                    return RedirectToAction("AdminEdit", new
                    {


                        idNumber = examiner.EMS_NATIONAL_ID,
                        examCode,
                        activity,
                        isSuccess = false,
                        message = result.Message,


                    });
                }

                return RedirectToAction("ExaminerList", new { isSuccess = true, message = "Record Updated Successfully" });
            }
            catch (Exception ex)
            {

                ElmahExtensions.RaiseError(ex);
                return RedirectToAction("ChangeRole", new
                {


                    idNumber = examiner.EMS_NATIONAL_ID,
                    examCode,
                    activity,
                    isSuccess = false,
                    message = ex.Message,


                });
            }

        }



        [Authorize(Roles = "Admin,SuperAdmin,SubjectManager,CentreSupervisor")]
        [HttpGet]
        public async Task<IActionResult> EditExaminer(string idNumber = "", string examCode = "", string subjectCode = "", string paperCode = "", string activity = "", bool isSuccess = false, string message = "")
        {
            var examiner = await _examinerRepository.GetExaminerRecord(idNumber);
            if (examiner == null)
            {
                TempData["Error"] = "Examiner not in transaction";
                return View();
            }
            var examinerTransaction = examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID && a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity);
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



        [Authorize(Roles = "Admin,SuperAdmin,SubjectManager,CentreSupervisor")]
        [HttpPost]
        public async Task<IActionResult> EditExaminer(Examiner examiner, string activity, string examCode, string attendance)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);



                // Save the changes
                var result = await _examinerRepository.EditExaminer(examiner, examCode, currentUser.Id, activity, attendance);

                if (result.Success)
                {

                    var user = await _userRepository.GetUser(examiner.EMS_NATIONAL_ID, examiner.EMS_SUBKEY);

                    if (user != null)
                    {
                        // Update user details
                        var results = await _userManager.UpdateAsync(user);

                        if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "E")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "Examiner");
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
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
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
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "BT")
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
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "PMS")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "PMS");
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
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "BMS")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "BMS");
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
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "DPMS")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "DPMS");
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
                        else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "RPMS")
                        {
                            if (results.Succeeded)
                            {
                                var currentRoles = await _userManager.GetRolesAsync(user);
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                await _userManager.AddToRoleAsync(user, "RPMS");
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
                    return RedirectToAction("EditExaminer", new
                    {


                        idNumber = examiner.EMS_NATIONAL_ID,
                        examCode,
                        activity,
                        isSuccess = false,
                        message = result.Message,


                    });
                }

                return RedirectToAction(nameof(Assignments), new { isSuccess = true, message = "Record Updated Successfully" });
            }
            catch (Exception ex)
            {

                ElmahExtensions.RaiseError(ex);
                return RedirectToAction("EditExaminer", new
                {


                    idNumber = examiner.EMS_NATIONAL_ID,
                    examCode,
                    activity,
                    isSuccess = false,
                    message = ex.Message,


                });
            }


        }



        [Authorize(Roles = "Admin,SuperAdmin,SubjectManager,CentreSupervisor")]
        [HttpGet]
        public async Task<IActionResult> ChangeSubject(string idNumber = "", string examCode = "", string activity = "", string subject = "", string paperCode = "")
        {
            var examiner = await _examinerRepository.GetExaminerRecord(idNumber);
            if (examiner == null)
            {
                return NotFound();
            }
            ViewBag.examCode = examCode;
            ViewBag.activity = activity;
            return View(examiner);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangeSubject(Examiner examiner)
        {

            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                examiner.EMS_SUBKEY = examiner.EMS_SUBKEY.Substring(0, 3) + examiner.EMS_SUB_SUB_ID + examiner.EMS_PAPER_CODE;
                // Save the changes
                var result = await _examinerRepository.ChangeSubject(examiner, currentUser.Id);


                var user = await _userRepository.GetUser(examiner.EMS_NATIONAL_ID, examiner.EMS_SUBKEY);
                if (user != null)
                {
                    // Update user details

                    user.EMS_SUBKEY = examiner.EMS_SUBKEY;

                    var results = await _userManager.UpdateAsync(user);

                    if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "E")
                    {
                        if (results.Succeeded)
                        {
                            var currentRoles = await _userManager.GetRolesAsync(user);
                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                            await _userManager.AddToRoleAsync(user, "Examiner");
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
                            var currentRoles = await _userManager.GetRolesAsync(user);
                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
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
                    else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "BT")
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
                    else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "PMS")
                    {
                        if (results.Succeeded)
                        {
                            var currentRoles = await _userManager.GetRolesAsync(user);
                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                            await _userManager.AddToRoleAsync(user, "PMS");
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
                    else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "BMS")
                    {
                        if (results.Succeeded)
                        {
                            var currentRoles = await _userManager.GetRolesAsync(user);
                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                            await _userManager.AddToRoleAsync(user, "BMS");
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
                    else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "DPMS")
                    {
                        if (results.Succeeded)
                        {
                            var currentRoles = await _userManager.GetRolesAsync(user);
                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                            await _userManager.AddToRoleAsync(user, "DPMS");
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
                    else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "RPMS")
                    {
                        if (results.Succeeded)
                        {
                            var currentRoles = await _userManager.GetRolesAsync(user);
                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                            await _userManager.AddToRoleAsync(user, "RPMS");
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





                // Save changes
                var checktands = await _andSRepository.GetUserTandS(examiner.EMS_NATIONAL_ID, examiner.EMS_SUBKEY);

                if (checktands != null)
                {
                    var tands = new TandS()
                    {

                        EMS_SUBKEY = examiner.EMS_SUBKEY,
                        EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID

                    };


                    await _andSRepository.AdjustTandS(tands, currentUser.Id);
                }



                if (!result.Success)
                {
                    return RedirectToAction("ExaminerList", new { isSuccess = false, message = result.Message });
                }

                return RedirectToAction("ExaminerList", new { isSuccess = true, message = "Updated Successfully" });
            }
            catch (Exception ex)
            {

                ElmahExtensions.RaiseError(ex);
                TempData["Error"] = "Examiner Details could not be Created" + ex.Message;
                return View();
            }


        }


        [Authorize(Roles = "SuperAdmin,Admin,SubjectManager,CentreSupervisor,PMS, BMS,DPMS,RPMS")]
        [HttpGet]
        public async Task<IActionResult> Assign(string idNumber = "", bool isSuccess = false, string message = "")
        {
            var examiner = await _examinerRepository.GetExaminerRecord(idNumber);
            if (examiner == null)
            {
                return NotFound();
            }

            if (!isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["Error"] = message;

            }

            var viewModel = new ExaminerTransactionViewModel
            {
                EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
                EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                EMS_EXAMINER_NAME = examiner.EMS_EXAMINER_NAME,
                EMS_LAST_NAME = examiner.EMS_LAST_NAME,
                EMS_SUB_SUB_ID = examiner.EMS_SUB_SUB_ID,
                EMS_PAPER_CODE = examiner.EMS_PAPER_CODE,
                EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER,
                EMS_EXM_SUPERORD = examiner.EMS_EXM_SUPERORD,
                EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE,
                EMS_MARKING_REG_CODE = examiner.EMS_MARKING_REG_CODE,
                Activity = string.Empty
            };

            return View("Transactions/Assign", viewModel);
        }

        [Authorize(Roles = "SuperAdmin,Admin,SubjectManager,CentreSupervisor")]
        [HttpPost]
        public async Task<IActionResult> Assign(ExaminerTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Transactions/Assign", model);
            }

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var subkey = model.EMS_SUB_SUB_ID + model.EMS_PAPER_CODE + model.Activity + model.EMS_NATIONAL_ID;

            var newTransction = new ExaminerScriptsMarked()
            {

                EMS_NATIONAL_ID = model.EMS_NATIONAL_ID,
                EMS_EXAMINER_CODE = model.EMS_EXAMINER_CODE,
                EMS_ECT_EXAMINER_CAT_CODE = model.EMS_ECT_EXAMINER_CAT_CODE,
                EMS_EXAMINER_NUMBER = model.EMS_EXAMINER_NUMBER,
                EMS_EXM_SUPERORD = model.EMS_EXM_SUPERORD,
                EMS_SUB_SUB_ID = model.EMS_SUB_SUB_ID,
                EMS_PAPER_CODE = model.EMS_PAPER_CODE,
                EMS_ACTIVITY = model.Activity,
                EMS_SUBKEY = subkey,
                EMS_MARKING_REG_CODE = model.EMS_MARKING_REG_CODE,
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


            var result = await _transactionRepository.AddNewExaminerToTransaction(newTransction, currentUser.Id);

            if (!result.Success)
            {
                return RedirectToAction(nameof(Assign), new { isSuccess = false, message = result.Message, idNumber = model.EMS_NATIONAL_ID });
            }

            return RedirectToAction("ExaminerList", new { isSuccess = true, message = result.Message });
        }






        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UpdateExaminer(Examiner examiner)
        {
            try
            {
                // Fetch current user
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                // Fetch bank details for ZWL and FCA
                var bankZWL = await GetBankDataAsync(examiner.EMS_BANK_CODE_ZWL, examiner.EMS_BRANCH_CODE_ZWL);
                var bankFCA = await GetBankDataAsync(examiner.EMS_BANK_CODE_FCA, examiner.EMS_BRANCH_CODE_FCA);

                // Update bank details if found
                if (bankFCA != null)
                {
                    examiner.EMS_BANK_CODE_FCA = bankFCA.B_BANK_CODE;
                    examiner.EMS_BANK_NAME_FCA = bankFCA.B_BANK_NAME;
                    examiner.EMS_BRANCH_CODE_FCA = bankFCA.BB_BRANCH_CODE;
                    examiner.EMS_BRANCH_NAME_FCA = bankFCA.BB_BRANCH_NAME;
                }

                if (bankZWL != null)
                {
                    examiner.EMS_BANK_NAME_ZWL = bankZWL.B_BANK_NAME;
                    examiner.EMS_BANK_CODE_ZWL = bankZWL.B_BANK_CODE;
                    examiner.EMS_BRANCH_CODE_ZWL = bankZWL.BB_BRANCH_CODE;
                    examiner.EMS_BRANCH_NAME_ZWL = bankZWL.BB_BRANCH_NAME;
                }

                // Clean the National ID by removing non-alphanumeric characters
                string cleanedNationalId = Regex.Replace(examiner.EMS_NATIONAL_ID, @"[^a-zA-Z0-9]", "");
                examiner.EMS_NATIONAL_ID = cleanedNationalId;

                // Update examiner record
                var updateResult = await _examinerRepository.UpdateExaminerRecord(examiner, currentUser.Id);
                if (!updateResult.Success)
                {
                    //return RedirectToAction("Index", new { isSuccess = false, message = updateResult.Message });

                    return Redirect($"/Examiner/Index?isSuccess=false&message={updateResult.Message}");
                }

                //return RedirectToAction("Index", new { isSuccess = true, message = updateResult.Message });
                //return Redirect($"/Examiner/Index?claimId={viewModel.TANDSCODE}&nationalId={viewModel.EMS_NATIONAL_ID}&examinerCode={viewModel.EMS_EXAMINER_CODE}&subKey={viewModel.EMS_SUBKEY}&isSuccess=true");

                return Redirect($"/Examiner/Index?isSuccess=true&message={updateResult.Message}");
            }
            catch (Exception ex)
            {
                ElmahExtensions.RaiseError(ex);
                TempData["Error"] = "Examiner Details could not be Created" + ex.Message;
                return View(examiner);
            }
        }





        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet]
        public async Task<JsonResult> DeleteExaminer(string idNumber)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                await _userRepository.DeleteUser(idNumber, currentUser.Id);


                await _andSRepository.DeleteAllExaminerTandS(idNumber, currentUser);

                await _transactionRepository.DeleteTranscation(idNumber,currentUser);
                
                // Remove from Examiner
                await _examinerRepository.RemoveExaminer(idNumber, currentUser.Id);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [Authorize]
        public async Task<IActionResult> ExaminerList(string examCode = "", string subjectCode = "", string paperCode = "", string activity = "", string regionCode = "", bool isSuccess = false, string message = "")
        {


            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var userSession = new SessionModel();
            if (userRoles != null && userRoles.Contains("BMS") || userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS"))
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
                            Activity = transaction.EMS_SUBKEY.Substring(9, 3),
                        };

                        if (transaction.EMS_SUB_SUB_ID.StartsWith("7"))
                        {
                            userSession.RegionCode = transaction.EMS_MARKING_REG_CODE;
                        }

                        HttpContext.Session.SetObjectAsJson("Session", userSession);
                    }
                }

            }

            if (userRoles != null && userRoles.Contains("SubjectManager") || userRoles.Contains("CentreSupervisor") || userRoles.Contains("Admin") || userRoles.Contains("SuperAdmin"))
            {



                if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) && !string.IsNullOrEmpty(paperCode))
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

            }
            if (userSession != null)
            {
                ViewBag.ExamCode = userSession.ExamCode;
                ViewBag.SubjectCode = userSession.SubjectCode;
                ViewBag.PaperCode = userSession.PaperCode;
                ViewBag.Activity = userSession.Activity;

                ViewBag.RegionCode = string.IsNullOrEmpty(regionCode) ? "" : userSession.RegionCode;
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


            if (isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["SuccessMessage"] = message;

            }

            if (!isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = message;

            }

            return View();
        }


        [Authorize]
        public async Task<IActionResult> Assignments(string examCode = "", string subjectCode = "", string paperCode = "", string activity = "", string regionCode = "", bool isSuccess = false, string message = "")
        {
            var viewModel = new ExaminerAssignmentsPageViewModel();

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var userSession = new SessionModel();

            if (userRoles != null && (userRoles.Contains("BMS") || userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS")))
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
                            SubjectCode = transaction.EMS_SUB_SUB_ID.Substring(3),
                            PaperCode = transaction.EMS_PAPER_CODE,
                            Activity = transaction.EMS_SUBKEY.Substring(9, 3),
                        };

                        if (transaction.EMS_SUB_SUB_ID.StartsWith("7"))
                        {
                            userSession.RegionCode = transaction.EMS_MARKING_REG_CODE;
                        }

                        HttpContext.Session.SetObjectAsJson("Session", userSession);
                    }
                }
            }

            if (userRoles != null && (userRoles.Contains("SubjectManager") || userRoles.Contains("CentreSupervisor") || userRoles.Contains("Admin") || userRoles.Contains("SuperAdmin")))
            {
                if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) && !string.IsNullOrEmpty(paperCode))
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
            }

            if (userSession != null)
            {
                viewModel.ExamCode = userSession.ExamCode;
                viewModel.SubjectCode = userSession.SubjectCode;
                viewModel.PaperCode = userSession.PaperCode;
                viewModel.ActivityCode = userSession.Activity;
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

            if (isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["SuccessMessage"] = message;
            }

            if (!isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["Error"] = message;
            }

            return View("Transactions/Index", viewModel);
        }

        [Authorize]
        public async Task<IActionResult> GetData(string examCode = "", string subjectCode = "", string paperCode = "", string activity = "", string regionCode = "")
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);


            IEnumerable<ExaminersListModel> model = new List<ExaminersListModel>();
            List<ExaminersListModel> modelList = new List<ExaminersListModel>();
            IEnumerable<Examiner> examinersList = new List<Examiner>();


            if (userRoles != null && userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS") || userRoles.Contains("BMS") || userRoles.Contains("SubjectManager") || userRoles.Contains("CentreSupervisor"))
            {


                examinersList = await _examinerRepository.GetComponentExaminers(examCode, subjectCode, paperCode, activity, regionCode);
            }


            if (userRoles != null && userRoles.Contains("Admin"))
            {
                examinersList = await _examinerRepository.GetComponentExaminers(examCode, subjectCode, paperCode, activity, regionCode);

            }


            if (userRoles != null && userRoles.Contains("SuperAdmin"))
            {

                examinersList = await _examinerRepository.GetAllExaminers();
            }

            modelList = examinersList
.Select(ex => new ExaminersListModel
{
    ExaminerCode = ex.EMS_EXAMINER_CODE,
    FirstName = ex.EMS_EXAMINER_NAME,
    LastName = ex.EMS_LAST_NAME,
    IDNumber = ex.EMS_NATIONAL_ID,
    Subject = ex.EMS_SUB_SUB_ID + "/" + ex.EMS_PAPER_CODE,
    SubKey = ex.EMS_SUBKEY,
    BMS = ex.EMS_EXM_SUPERORD,
    ExaminerNumber = ex.EMS_EXAMINER_NUMBER,
    Category = ex.EMS_ECT_EXAMINER_CAT_CODE,


})
.ToList();


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
    (p.IDNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.ExaminerNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.BMS?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
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
        [HttpPost]
        public async Task<IActionResult> GetAssignments(string examCode = "", string subjectCode = "", string paperCode = "", string activity = "", string regionCode = "")
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);


            IEnumerable<ExaminersListModel> model = new List<ExaminersListModel>();
            List<ExaminersListModel> modelList = new List<ExaminersListModel>();
            IEnumerable<Examiner> examinersList = new List<Examiner>();


            if (userRoles != null && userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS") || userRoles.Contains("BMS") || userRoles.Contains("SubjectManager") || userRoles.Contains("CentreSupervisor"))
            {


                examinersList = await _examinerRepository.GetPresentComponentExaminers(examCode, subjectCode, paperCode, activity, regionCode);
            }


            if (userRoles != null && userRoles.Contains("Admin"))
            {
                examinersList = await _examinerRepository.GetComponentExaminersTransaction(examCode, subjectCode, paperCode, activity, regionCode);

            }


            if (userRoles != null && userRoles.Contains("SuperAdmin"))
            {

                examinersList = await _examinerRepository.GetAllComponentExaminersTransaction(examCode, activity);
            }

            modelList = examinersList
.Select(ex => new ExaminersListModel
{
    ExaminerCode = ex.EMS_EXAMINER_CODE,
    FirstName = ex.EMS_EXAMINER_NAME,
    LastName = ex.EMS_LAST_NAME,
    IDNumber = ex.EMS_NATIONAL_ID,
    Subject = ex.EMS_SUB_SUB_ID + "/" + ex.EMS_PAPER_CODE,
    SubKey = ex.EMS_SUBKEY,
    BMS = ex.EMS_EXM_SUPERORD,
    ExaminerNumber = ex.EMS_EXAMINER_NUMBER,
    Category = ex.EMS_ECT_EXAMINER_CAT_CODE,

})
.ToList();


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
    (p.IDNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.ExaminerNumber?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.BMS?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
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


        [Authorize(Roles = "SuperAdmin,Admin,SubjectManager,CentreSupervisor,PMS, BMS,DPMS,RPMS")]
        [HttpGet]
        public async Task<IActionResult> AddOrReplaceExaminer(string examCode = "", string subjectCode = "", string paperCode = "", bool isSuccess = false, string message = "")
        {

            var newexaminercode = await InitializeNextTrainingCode();

            ViewBag.NewExaminerCode = newexaminercode.ToString();

            var viewModel = new ReplaceExaminerViewModel
            {
                EMS_EXAMINER_CODE = newexaminercode.ToString(),
                EMS_SUB_SUB_ID = string.Concat(examCode, subjectCode ?? string.Empty),
                EMS_PAPER_CODE = paperCode ?? string.Empty,
                Activity = string.Empty
            };



            if (!isSuccess && !string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = message;

            }

            return View(viewModel);
        }




        [Authorize(Roles = "SuperAdmin,Admin,SubjectManager,CentreSupervisor,OfficerSpecialNeeds,PMS, BMS,DPMS,RPMS")]
        [HttpPost]
        public async Task<IActionResult> AddOrReplaceExaminer(ReplaceExaminerViewModel model)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.NewExaminerCode = model.EMS_EXAMINER_CODE;
                    return View(model);
                }

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                var examiner = model.ToExaminer();
                examiner.CreatedBy = currentUser.UserName;
                examiner.CreatedDate = DateTime.Now.ToString();
                examiner.EMS_SUBKEY = model.EMS_SUB_SUB_ID + model.EMS_PAPER_CODE + model.Activity + model.EMS_NATIONAL_ID;

                var examCode = model.EMS_SUB_SUB_ID?.Length >= 3 ? model.EMS_SUB_SUB_ID.Substring(0, 3) : string.Empty;
                var subjectCode = model.EMS_SUB_SUB_ID?.Length > 3 ? model.EMS_SUB_SUB_ID.Substring(3) : string.Empty;

                examiner.EMS_SUB_SUB_ID = subjectCode;
                var result = await _examinerRepository.AddOrReplaceExaminer(examiner, currentUser.Id);

                if (result.Success)
                {
                    var subKey = examiner.EMS_SUBKEY;
                    string cleanFirstName = RemoveMiddleName(examiner.EMS_EXAMINER_NAME);
                    string cleanSurname = RemoveMiddleName(examiner.EMS_LAST_NAME);
                    var subjectcode = examiner.EMS_SUB_SUB_ID;
                    var papercode = examiner.EMS_PAPER_CODE;
                    if (examiner.EMS_ECT_EXAMINER_CAT_CODE == null)
                    {
                        examiner.EMS_ECT_EXAMINER_CAT_CODE = "E";
                    }

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
                        while (existingUser != null && existingUser.UserName == username && existingUser.EMS_SUBKEY != subKey && existingUser.IDNumber != examiner.EMS_NATIONAL_ID)
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
                                EmailConfirmed = true,
                                Activity = model.Activity
                            };


                            // Generate a default password
                            string defaultPassword = GenerateDefaultPassword(user, subjectcode, papercode);

                            // Create the user with the generated password
                            var results = await _userManager.CreateAsync(user, defaultPassword);

                            if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "E")
                            {
                                if (results.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(user, "Examiner");
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
                            else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "PBT")
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
                            else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "PMS")
                            {
                                if (results.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(user, "PMS");
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
                            else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "BMS")
                            {
                                if (results.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(user, "BMS");
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
                            else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "DPMS")
                            {
                                if (results.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(user, "DPMS");
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
                            else if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "RPMS")
                            {
                                if (results.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(user, "RPMS");
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


                    var newTransction = new ExaminerScriptsMarked()
                    {

                        EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                        EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
                        EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE,
                        EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER,
                        EMS_EXM_SUPERORD = examiner.EMS_EXM_SUPERORD,
                        EMS_SUB_SUB_ID = examiner.EMS_SUB_SUB_ID,
                        EMS_PAPER_CODE = examiner.EMS_PAPER_CODE,
                        EMS_ACTIVITY = model.Activity,
                        EMS_SUBKEY = subKey,
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


                    await _transactionRepository.AddNewExaminerToTransaction(newTransction, currentUser.Id);


                }

                if (!result.Success)
                {

                    return RedirectToAction("AddOrReplaceExaminer", new
                    {
                        examCode,
                        subjectCode,
                        paperCode = examiner.EMS_PAPER_CODE,
                        isSuccess = false,
                        message = result.Message,

                    });


                }

                return RedirectToAction("ExaminerList", new { isSuccess = true, message = result.Message });
            }
            catch (Exception ex)
            {

                ElmahExtensions.RaiseError(ex);
                TempData["Error"] = "Examiner Details could not be Created" + ex.Message;
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.NewExaminerCode = model.EMS_EXAMINER_CODE;
                return View(model);
            }
        }


        [Authorize(Roles = "SuperAdmin,Admin,SubjectManager,CentreSupervisor")]
        [HttpGet]
        public async Task<IActionResult> AddNewExaminer(string examCode = "", string subjectCode = "", string paperCode = "", string activity = "", bool isSuccess = false, string message = "")
        {
            try
            {
                var newexaminercode = await InitializeNextTrainingCode();
                ViewBag.NewExaminerCode = newexaminercode.ToString();
                var viewModel = new AddExaminerViewModel
                {
                    EMS_EXAMINER_CODE = newexaminercode.ToString(),
                    EMS_SUB_SUB_ID = subjectCode ?? string.Empty,
                    EMS_PAPER_CODE = paperCode ?? string.Empty,
                    ExamCode = examCode ?? string.Empty,
                    Activity = activity ?? string.Empty
                };

                if (activity != "BEM")
                {
                    viewModel.EMS_EXM_SUPERORD = "1001";
                    viewModel.EMS_EXAMINER_NUMBER = "1001";

                }

                if (!isSuccess && !string.IsNullOrEmpty(message))
                {
                    TempData["ErrorMessage"] = message;

                }

                return View(viewModel);
            }
            catch (Exception ex)
            {

                //ElmahExtensions.RaiseError(ex);
                TempData["Error"] = "Something Went Wrong: " + ex.Message;
                return Redirect("Error/ServerError");
            }


        }


        public bool IsValidZimbabweNationalId(string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                return false;

            string cleanedId = nationalId.Trim().ToUpper();

            // Must be exactly 10, 11, or 12 characters
            if (cleanedId.Length != 10 && cleanedId.Length != 11 && cleanedId.Length != 12)
                return false;

            // Regex patterns
            string pattern10 = @"^\d{7}[A-Z]\d{2}$";  // 10-char format
            string pattern11 = @"^\d{8}[A-Z]\d{2}$";  // 11-char format
            string pattern12 = @"^\d{9}[A-Z]\d{2}$";  // 12-char format

            return Regex.IsMatch(cleanedId, pattern10)
                || Regex.IsMatch(cleanedId, pattern11)
                || Regex.IsMatch(cleanedId, pattern12);
        }


        [Authorize(Roles = "SuperAdmin,Admin,SubjectManager,CentreSupervisor")]
        [HttpPost]
        public async Task<IActionResult> AddNewExaminer(AddExaminerViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    ViewBag.NewExaminerCode = model.EMS_EXAMINER_CODE;
                    return View(model);
                }



                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                if (!IsValidZimbabweNationalId(model.EMS_NATIONAL_ID))
                {
                    ModelState.AddModelError("EMS_NATIONAL_ID",
                        "Invalid Zimbabwe National ID format. Examples: 08123456D53");
                    ViewBag.NewExaminerCode = model.EMS_EXAMINER_CODE;
                    return View(model);
                }

                var examiner = model.ToExaminer();
                examiner.CreatedBy = currentUser.UserName;
                examiner.CreatedDate = DateTime.Now.ToString();

                var subkey = model.ExamCode + examiner.EMS_SUB_SUB_ID + examiner.EMS_PAPER_CODE + model.Activity + examiner.EMS_NATIONAL_ID;
                examiner.EMS_SUBKEY = subkey;




                var result = await _examinerRepository.AddNewExaminer(examiner, currentUser.Id);

                //if (result.Success)
                //{

                //    var user = await _userRepository.GetUser(examiner.EMS_NATIONAL_ID, subkey);
                //    examiner.EMS_ECT_EXAMINER_CAT_CODE ??= "E"; // Default category

                //    if (user == null)
                //    {
                //        await CreateNewUser(examiner, subkey, activity);
                //    }
                //    else
                //    {
                //        await UpdateExistingUser(user, activity, subkey, examiner);
                //    }


                //    var newTransction = new ExaminerScriptsMarked()
                //    {

                //        EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                //        EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
                //        EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE,
                //        EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER,
                //        EMS_EXM_SUPERORD = examiner.EMS_EXM_SUPERORD,
                //        EMS_SUB_SUB_ID = examCode + examiner.EMS_SUB_SUB_ID,
                //        EMS_PAPER_CODE = examiner.EMS_PAPER_CODE,
                //        EMS_ACTIVITY = activity,
                //        EMS_SUBKEY = subkey,
                //        EMS_MARKING_REG_CODE = examiner.EMS_MARKING_REG_CODE,
                //        IsPresent = false,
                //        RegisterStatus = "Absent",
                //        RegisterStatusBy = currentUser.UserName,
                //        RegisterStatusDate = DateTime.Now.ToString(),
                //        RecommendedStatus = "Pending",
                //        RecommendedBy = currentUser.UserName,
                //        RecommendedDate = DateTime.Now.ToString(),
                //        AttendanceStatus = "Pending",
                //        AttendanceStatusBy = currentUser.UserName,
                //        AttendanceStatusDate = DateTime.Now.ToString(),
                //        SCRIPTS_MARKED = 0
                //    };


                //    await _transactionRepository.AddNewExaminerToTransaction(newTransction, currentUser.Id);


                //}

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("AddNewExaminer", new
                    {
                        examCode = model.ExamCode,
                        subjectCode = examiner.EMS_SUB_SUB_ID,
                        paperCode = examiner.EMS_PAPER_CODE,
                        activity = model.Activity,
                        isSuccess = false,
                        message = result.Message,

                    });


                }

                return RedirectToAction("ExaminerList", new { isSuccess = true, message = result.Message });
            }
            catch (Exception ex)
            {



                TempData["Error"] = "Something Went Wrong: " + ex.Message;
                return Redirect("Error/ServerError");
            }




        }




        private string RemoveMiddleName(string name)
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


        private string GenerateDefaultPassword(ApplicationUser user, string subjectcode, string papercode)
        {
            //string cleanFirstName = RemoveMiddleName(firstname);
            //string cleanSurname = RemoveMiddleName(surname);

            //// Generate prefixes
            //string firstNamePrefix = cleanFirstName.Substring(0, Math.Min(3, cleanFirstName.Length)).ToLower();
            //string surnamePrefix = cleanSurname.ToLower();
            //int currentYear = DateTime.Now.Year;

            // Combine the parts of the password
            string password = $"{user.UserName.ToLower()}{subjectcode}{papercode}.*";

            // Capitalize the first letter
            //password = char.ToUpper(password[0]) + password.Substring(1);

            return password;
        }



        public async Task CreateNewUser(Examiner examiner, string subkey, string activity)
        {
            var username = GenerateUsername(examiner);


            var newUser = new ApplicationUser
            {
                UserName = username,
                Email = $"{username}@ems.com",
                EMS_SUBKEY = subkey,
                PhoneNumber = examiner.EMS_PHONE_HOME ?? "0000000000",
                IDNumber = examiner.EMS_NATIONAL_ID,
                ExaminerCode = examiner.EMS_EXAMINER_CODE,
                Activated = true,
                LockoutEnabled = true,
                EmailConfirmed = true,
                Activity = activity
            };
            var password = GenerateDefaultPassword(newUser, examiner.EMS_SUB_SUB_ID, examiner.EMS_PAPER_CODE);
            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                await AssignRole(newUser, examiner.EMS_ECT_EXAMINER_CAT_CODE);
            }
            else
            {
                throw new ApplicationException($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task UpdateExistingUser(ApplicationUser user, string activity, string subkey, Examiner model)
        {
            user.EMS_SUBKEY = subkey;
            user.Activity = activity;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await ReassignRole(user, model.EMS_ECT_EXAMINER_CAT_CODE);
            }
            else
            {
                throw new ApplicationException($"User update failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }


       



        private async Task AssignRole(ApplicationUser user, string category)
        {
            var roleName = GetRoleName(category);
            await _userManager.AddToRoleAsync(user, roleName);
        }

        private async Task ReassignRole(ApplicationUser user, string category)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var roleName = GetRoleName(category);
            await _userManager.AddToRoleAsync(user, roleName);
        }

        private string GetRoleName(string category)
        {
            return category switch
            {
                "E" => "Examiner",
                "A" => "A",
                "BT" => "BT",
                "PBT" => "PBT",
                "PMS" => "PMS",
                "BMS" => "BMS",
                "DPMS" => "DPMS",
                "RPMS" => "RPMS",
                "S" => "S",
                "I" => "I",
                _ => "Examiner" // Default role
            };
        }

        private string GenerateUsername(Examiner examiner)
        {
            string cleanFirstName = RemoveMiddleName(examiner.EMS_EXAMINER_NAME);
            string cleanSurname = RemoveMiddleName(examiner.EMS_LAST_NAME);

            string firstNamePrefix = cleanFirstName.Substring(0, Math.Min(3, cleanFirstName.Length)).ToLower();
            string surnamePrefix = cleanSurname.ToLower();

            string username = $"{firstNamePrefix}{surnamePrefix}";
            string originalUsername = username;

            int counter = 1;
            while (_userManager.FindByNameAsync(username).Result != null)
            {
                username = $"{originalUsername}{counter++}";
            }

            return username;
        }





    }


}
