using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;
using ExaminerPaymentSystem.Extensions;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol.Core.Types;
using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.AspNetCore.Authorization;
using System.Numerics;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Common;

using ExaminerPaymentSystem.Models.Other;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using ElmahCore;
using ExaminerPaymentSystem.ViewModels.Examiners;

namespace ExaminerPaymentSystem.Controllers.Examiners
{

    public class TandSController : Controller
    {

        private readonly IExaminerRepository _examinerRepository;
        private readonly ITandSRepository _tandSRepository;
        private readonly ITandSDetailsRepository _detailRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IAdvanceFeesRepository _advanceFeesRepository;
        private readonly IPaperMarkingRateRepository _paperMarkingRate;
        private readonly ICategoryRateRepository _categoryMarkingRate;
        private readonly IExamCodesRepository _examCodesRepository;
        private readonly ITandSFilesRepository _tandSFilesRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVenueRepository _venueRepository;
        private readonly IRegisterRepository _registerRepository;
        private readonly IActivityRepository _activityRepository;

        public TandSController(IExaminerRepository examinerRepository, SignInManager<ApplicationUser> signInManager, ITandSRepository tandSRepository, ITandSDetailsRepository tandSDetailsRepository, ApplicationDbContext applicationDbContext, IAdvanceFeesRepository advanceFeesRepository, IPaperMarkingRateRepository paperMarkingRate, ICategoryRateRepository categoryMarkingRate, IExamCodesRepository examCodesRepository, ITandSFilesRepository tandSFilesRepository, UserManager<ApplicationUser> userManager, IVenueRepository venueRepository, IRegisterRepository registerRepository, IActivityRepository activityRepository)
        {
            _examinerRepository = examinerRepository;
            _tandSRepository = tandSRepository;
            _detailRepository = tandSDetailsRepository;
            _signInManager = signInManager;
            _context = applicationDbContext;
            _advanceFeesRepository = advanceFeesRepository;
            _paperMarkingRate = paperMarkingRate;
            _categoryMarkingRate = categoryMarkingRate;
            _examCodesRepository = examCodesRepository;
            _tandSFilesRepository = tandSFilesRepository;
            _userManager = userManager;
            _venueRepository = venueRepository;
            _registerRepository = registerRepository;
            _activityRepository = activityRepository;
        }


        // GET: Activity/Select
        public IActionResult SelectActivity(string msg = "")
        {
            var model = new ActivityModel();



            if (!string.IsNullOrEmpty(msg) && msg == "NotInTransaction")
            {
                TempData["NotInTransaction"] = "We could not find your invitation for this Activity. Please check the details or select another Activity and try again .";


            }

            return View(model);

        }


        public async Task<IActionResult> Index(bool isSuccess = false)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var examinerData = new TandSViewModel();
                var examinerRecord = await _examinerRepository.GetExaminerRecord(currentUser.IDNumber);

                if (examinerRecord == null)
                {

                    // Store SweetAlert configuration in TempData
                    TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                    {
                        icon = "warning",  // Changed to warning icon
                        title = "ACCESS DENIED!",
                        text = "No Examiner Record is linked to this account . Please check your account credentials. You will be logged out for security reasons.",
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

                var percentageProfile = CheckDataValidation(examinerRecord);
                if (percentageProfile < 100)
                {
                    // Store SweetAlert configuration in TempData
                    TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                    {
                        icon = "warning",  // Changed to warning icon
                        title = "ACCESS DENIED!",
                        text = "Profile not complete  " + percentageProfile + "%  done. Please complete your profile",
                        showConfirmButton = true,
                        confirmButtonColor = "#ffc107", // Warning color

                        timerProgressBar = true,

                        customClass = new
                        {
                            container = "swal2-flicker", // Applies to the entire modal
                            title = "swal2-title-danger" // Applies only to the title
                        }
                    });

                    return Redirect("/Examiner/Index");
                }

                if (examinerRecord != null)
                {

                    var transaction = examinerRecord.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == currentUser.EMS_SUBKEY && a.EMS_NATIONAL_ID == currentUser.IDNumber);

                    if (transaction == null)
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
                    else
                    {
                        if (transaction.EMS_ACTIVITY != "BEM" && transaction.RegisterStatus != "Present")
                        {
                            // Store SweetAlert configuration in TempData
                            TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                            {
                                icon = "warning",  // Changed to warning icon
                                title = "ACCESS DENIED!",
                                text = "You are not authorized to apply for T n S you account is Absent in the register contact Subject Manager.",
                                showConfirmButton = true,
                                confirmButtonColor = "#ffc107", // Warning color

                                timerProgressBar = true,

                                customClass = new
                                {
                                    container = "swal2-flicker", // Applies to the entire modal
                                    title = "swal2-title-danger" // Applies only to the title
                                }
                            });

                            return Redirect("/Home/Index");
                        }
                        var checktands = await _tandSRepository.GetUserTandS(transaction.EMS_NATIONAL_ID, transaction.EMS_SUBKEY);

                        if (checktands == null)
                        {
                            var currentUserExamCode = transaction.EMS_SUBKEY.Substring(0, 3);
                            var currentActivity = transaction.EMS_SUBKEY.Substring(9,3);

                            var currentSession = await _context.CAN_EXAM
                                .FirstOrDefaultAsync(a => a.EXM_EXAM_CODE == currentUserExamCode);

                            var sessionExamCodes = await _context.CAN_EXAM
                                .Where(a => a.EXM_EXAM_SESSION == currentSession.EXM_EXAM_SESSION && a.ACTIVATED_SESSION == "Activated")
                                .Select(a => a.EXM_EXAM_CODE)
                                .ToListAsync();

                            // Corrected line - added missing parenthesis
                            var filteredScripts = examinerRecord.ExaminerScriptsMarkeds
                                .Where(script => sessionExamCodes.Any(examCode =>
                                    script.EMS_SUBKEY?.StartsWith(examCode) == true) && script.EMS_ACTIVITY == currentActivity)
                                .ToList();

                            var validateTnS = new List<TandS>();
                            foreach (var item in filteredScripts)
                            {
                                var a = await _tandSRepository.GetUserTandS(item.EMS_NATIONAL_ID, item.EMS_SUBKEY);
                                if (a != null)
                                {
                                    validateTnS.Add(a);
                                }
                            }

                            if (validateTnS.Any())
                            {

                                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                                {
                                    icon = "warning",
                                    title = "ACCESS DENIED!",
                                    text = "You already have a T and S for this session. Contact Subject manager.",
                                    showConfirmButton = true,
                                    //confirmButtonColor = "#dc3545",
                                    confirmButtonColor = "#ffc107", // Warning color
                                    //timer = 5000,
                                    timerProgressBar = true,
                                    customClass = new
                                    {
                                        container = "swal2-flicker", // Applies to the entire modal
                                        title = "swal2-title-danger" // Applies only to the title
                                    }
                                });




                                return Redirect("/Home/Index");
                            }

                        }



                        List<TandSDetail> tandSDetails = new List<TandSDetail>();
                        TandSAdvance advance = null;
                        IEnumerable<TandSFile> tandSFileCollection = new List<TandSFile>();


                        if (checktands != null)
                        {
                            tandSDetails = checktands.TandSDetails.ToList();

                            advance = checktands.TandSAdvance;

                            tandSFileCollection = checktands.TandSFiles.ToList();

                            decimal totalBusfare = 0;
                            decimal totalAccommodation = 0;
                            decimal totalLunch = 0;
                            decimal totalDinner = 0;
                            decimal totalTotal = 0;

                            foreach (var detail in tandSDetails)
                            {
                                // Convert the string values to decimal and add them to respective totals
                                totalBusfare += Convert.ToDecimal(detail.EMS_BUSFARE);
                                totalAccommodation += Convert.ToDecimal(detail.EMS_ACCOMMODATION);
                                totalLunch += Convert.ToDecimal(detail.EMS_LUNCH);
                                totalDinner += Convert.ToDecimal(detail.EMS_DINNER);
                                totalTotal += Convert.ToDecimal(detail.EMS_TOTAL);
                            }

                            // Assign totals to ViewBag
                            ViewBag.TotalBusfare = totalBusfare;
                            ViewBag.TotalAccommodation = totalAccommodation;
                            ViewBag.TotalLunch = totalLunch;
                            ViewBag.TotalDinner = totalDinner;
                            ViewBag.TotalTotal = totalTotal;
                        }

                        var checkvenue = await _venueRepository.GetVenueSUbject(transaction.EMS_SUB_SUB_ID, transaction.EMS_PAPER_CODE);

                        examinerData = new TandSViewModel()
                        {
                            EMS_EXAMINER_CODE = examinerRecord.EMS_EXAMINER_CODE,
                            EMS_EXAMINER_NAME = examinerRecord.EMS_EXAMINER_NAME,
                            EMS_LAST_NAME = examinerRecord.EMS_LAST_NAME,
                            EMS_NATIONAL_ID = examinerRecord.EMS_NATIONAL_ID,
                            EMS_SUBKEY = transaction.EMS_SUBKEY,
                            EMS_ADDRESS = examinerRecord.EMS_WORK_ADD1 + " " + examinerRecord.EMS_WORK_ADD2,
                            EMS_ACCOUNT_NO_FCA = examinerRecord.EMS_ACCOUNT_NO_FCA,
                            EMS_ACCOUNT_NO_ZWL = examinerRecord.EMS_ACCOUNT_NO_ZWL,
                            EMS_BANK_NAME_ZWL = examinerRecord.EMS_BANK_NAME_ZWL,
                            EMS_BANK_NAME_FCA = examinerRecord.EMS_BANK_NAME_FCA,
                            EMS_LEVEL_OF_EXAM_MARKED = examinerRecord.EMS_LEVEL_OF_EXAM_MARKED,
                            EMS_PAPER_CODE = transaction.EMS_PAPER_CODE,
                            EMS_PHONE_HOME = examinerRecord.EMS_PHONE_HOME,

                            EMS_ECT_EXAMINER_CAT_CODE = examinerRecord.EMS_ECT_EXAMINER_CAT_CODE,
                            EMS_WORK_ADD1 = examinerRecord.EMS_WORK_ADD1,
                            EMS_WORK_ADD2 = examinerRecord.EMS_WORK_ADD2,
                            EMS_WORK_ADD3 = examinerRecord.EMS_WORK_ADD3,
                            EMS_PURPOSEOFJOURNEY = transaction.EMS_ACTIVITY,

                        };

                        if (transaction.EMS_SUB_SUB_ID is not null)
                        {
                            examinerData.EMS_SUB_SUB_ID = transaction.EMS_SUB_SUB_ID.Substring(3);
                        }

                        if (checkvenue != null)
                        {
                            examinerData.EMS_VENUE = checkvenue.Venue;
                        }

                        ViewBag.activity = transaction.EMS_ACTIVITY;


                        if (advance != null)
                        {
                            examinerData.TANDSADVANCE = advance;
                            ViewBag.TandSAdvance = advance;
                        }
                        bool tandstatus = false;
                        if (checktands != null)
                        {
                            examinerData.EMS_PURPOSEOFJOURNEY = checktands.EMS_PURPOSEOFJOURNEY;
                            examinerData.EMS_TOTAL = checktands.EMS_TOTAL.ToString();
                            examinerData.EMS_VENUE = checktands.EMS_VENUE;
                            examinerData.ACCOUNTS_STATUS = checktands.ACCOUNTS_STATUS;

                            tandstatus = true;
                        }



                        ViewBag.TANDSTATUS = tandstatus;
                        ViewBag.TandSDetails = tandSDetails;
                        ViewBag.TandSFile = tandSFileCollection;

                    }

                }


                var advanceFees = await _advanceFeesRepository.GetAdvanceFees();
                var venues = await _venueRepository.VenuesGetAll();
                var activities = await _activityRepository.GetAllActivitiesAsync();

                ViewBag.Venues = venues.Where(a => a.Status == "Active")
                         .Select(a => new { Text = a.Name, Value = a.Name })
                         .ToList();

                ViewBag.Activities = activities.Where(a => a.Status == "Active")
                    .Select(a => new { a.Text, a.Value })
                             .ToList();
                ViewBag.FEE_TEA = advanceFees?.FEE_TEA ?? 5.00m;  // Decimal default value
                ViewBag.FEE_BREAKFAST = advanceFees?.FEE_BREAKFAST ?? 5.00m;
                ViewBag.FEE_TRANSPORT = advanceFees?.FEE_TRANSPORT ?? 3.00m;
                ViewBag.FEE_ACCOMMODATION_RES = advanceFees?.FEE_ACCOMMODATION_RES ?? 0.00m;
                ViewBag.FEE_ACCOMMODATION_NONRES = advanceFees?.FEE_ACCOMMODATION_NONRES ?? 10.00m;
                ViewBag.FEE_LUNCH = advanceFees?.FEE_LUNCH ?? 0.00m;
                ViewBag.FEE_DINNER = advanceFees?.FEE_DINNER ?? 10.00m;  // String default
                ViewBag.FEE_OVERNIGHTALLOWANCE = advanceFees?.FEE_OVERNIGHTALLOWANCE ?? 5.00m;

                if (isSuccess)
                {
                    TempData["SuccessMessage"] = "Claim Submitted Successfully.";
                }
                return View(examinerData);
            }
            catch (Exception ex)
            {

                //ElmahExtensions.RaiseError(ex);
                TempData["Error"] = "Something Went Wrong: " + ex.Message;
                return Redirect("Error/ServerError");
            }

        }




        //public async Task<ActionResult> EditTandS()
        //{
        //    ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
        //    var checktands = await _tandSRepository.GetUserTandS(currentUser.IDNumber);
        //    Examiner examiner = new Examiner();


        //    List<TandSDetail> tandSDetails = new List<TandSDetail>();
        //    TandSAdvance advance = null;
        //    IEnumerable<TandSFile> tandSFileCollection = new List<TandSFile>();
        //    examiner = await _examinerRepository.GetExaminerRecord(currentUser.IDNumber);



        //    if (checktands != null)
        //    {
        //        tandSDetails = await _detailRepository.GetTandSDetails(checktands.EMS_NATIONAL_ID, checktands.TANDSCODE, checktands.EMS_SUBKEY, checktands.EMS_EXAMINER_CODE);

        //        advance = await _detailRepository.GetTandSAdvance(checktands.EMS_NATIONAL_ID, checktands.TANDSCODE, checktands.EMS_SUBKEY, checktands.EMS_EXAMINER_CODE);

        //        tandSFileCollection = await _tandSFilesRepository.GetTandSFiles(checktands.EMS_NATIONAL_ID, checktands.TANDSCODE, checktands.EMS_SUBKEY, checktands.EMS_EXAMINER_CODE);

        //        decimal totalBusfare = 0;
        //        decimal totalAccommodation = 0;
        //        decimal totalLunch = 0;
        //        decimal totalDinner = 0;
        //        decimal totalTotal = 0;

        //        foreach (var detail in tandSDetails)
        //        {
        //            // Convert the string values to decimal and add them to respective totals
        //            totalBusfare += Convert.ToDecimal(detail.EMS_BUSFARE);
        //            totalAccommodation += Convert.ToDecimal(detail.EMS_ACCOMMODATION);
        //            totalLunch += Convert.ToDecimal(detail.EMS_LUNCH);
        //            totalDinner += Convert.ToDecimal(detail.EMS_DINNER);
        //            totalTotal += Convert.ToDecimal(detail.EMS_TOTAL);
        //        }

        //        // Assign totals to ViewBag
        //        ViewBag.TotalBusfare = totalBusfare;
        //        ViewBag.TotalAccommodation = totalAccommodation;
        //        ViewBag.TotalLunch = totalLunch;
        //        ViewBag.TotalDinner = totalDinner;
        //        ViewBag.TotalTotal = totalTotal;
        //    }

        //    var sub = examiner.EMS_SUB_SUB_ID;
        //    var paper = examiner.EMS_PAPER_CODE;
        //    var checkvenue = await _venueRepository.GetVenueSUbject(sub, paper);

        //    var examinerData = new TandSViewModel()
        //    {
        //        EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
        //        EMS_EXAMINER_NAME = examiner.EMS_EXAMINER_NAME,
        //        EMS_LAST_NAME = examiner.EMS_LAST_NAME,
        //        EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
        //        EMS_SUBKEY = examiner.EMS_SUBKEY,
        //        EMS_ADDRESS = examiner.EMS_WORK_ADD1 + " " + examiner.EMS_WORK_ADD2,
        //        EMS_ACCOUNT_NO_FCA = examiner.EMS_ACCOUNT_NO_FCA,
        //        EMS_ACCOUNT_NO_ZWL = examiner.EMS_ACCOUNT_NO_ZWL,
        //        EMS_BANK_NAME_ZWL = examiner.EMS_BANK_NAME_ZWL,
        //        EMS_BANK_NAME_FCA = examiner.EMS_BANK_NAME_FCA,
        //        EMS_LEVEL_OF_EXAM_MARKED = examiner.EMS_LEVEL_OF_EXAM_MARKED,
        //        EMS_PAPER_CODE = examiner.EMS_PAPER_CODE,
        //        EMS_PHONE_HOME = examiner.EMS_PHONE_HOME,
        //        EMS_SUB_SUB_ID = examiner.EMS_SUB_SUB_ID,
        //        EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE,
        //        EMS_WORK_ADD1 = examiner.EMS_WORK_ADD1,
        //        EMS_WORK_ADD2 = examiner.EMS_WORK_ADD2,
        //        EMS_WORK_ADD3 = examiner.EMS_WORK_ADD3,


        //    };

        //    if (checkvenue != null)
        //    {
        //        examinerData.EMS_VENUE = checkvenue.Venue;
        //    }


        //    if (advance != null)
        //    {
        //        examinerData.TANDSADVANCE = advance;
        //        ViewBag.TandSAdvance = advance;
        //    }
        //    bool tandstatus = false;
        //    if (checktands != null)
        //    {
        //        examinerData.EMS_PURPOSEOFJOURNEY = checktands.EMS_PURPOSEOFJOURNEY;
        //        examinerData.EMS_TOTAL = checktands.EMS_TOTAL.ToString();
        //        examinerData.EMS_VENUE = checktands.EMS_VENUE;
        //        examinerData.ACCOUNTS_STATUS = checktands.ACCOUNTS_STATUS;
        //        examinerData.TANDSDETAILS = checktands.TandSDetails.ToList();
        //        examinerData.TANDSADVANCE = checktands.TandSAdvance;
        //        tandstatus = true;
        //    }

        //    var advanceFees = await _advanceFeesRepository.GetAdvanceFees();
        //    var venues = await _venueRepository.VenuesGetAll();
        //    ViewBag.TANDSTATUS = tandstatus;
        //    ViewBag.TandSDetails = tandSDetails;
        //    ViewBag.TandSFile = tandSFileCollection;
        //    ViewBag.Venues = venues.Where(a => a.Status == "Active")
        //             .Select(a => new { Text = a.Name, Value = a.Name })
        //             .ToList();


        //    ViewBag.FEE_TEA = advanceFees?.FEE_TEA ?? 5.00m;  // Decimal default value
        //    ViewBag.FEE_BREAKFAST = advanceFees?.FEE_BREAKFAST ?? 5.00m;
        //    ViewBag.FEE_TRANSPORT = advanceFees?.FEE_TRANSPORT ?? 3.00m;
        //    ViewBag.FEE_ACCOMMODATION_RES = advanceFees?.FEE_ACCOMMODATION_RES ?? 0.00m;
        //    ViewBag.FEE_ACCOMMODATION_NONRES = advanceFees?.FEE_ACCOMMODATION_NONRES ?? 10.00m;
        //    ViewBag.FEE_LUNCH = advanceFees?.FEE_LUNCH ?? 0.00m;
        //    ViewBag.FEE_DINNER = advanceFees?.FEE_DINNER ?? 10.00m;  // String default
        //    ViewBag.FEE_OVERNIGHTALLOWANCE = advanceFees?.FEE_OVERNIGHTALLOWANCE ?? 5.00m;
        //    return View(examinerData);
        //}



        private double CheckDataValidation(Examiner examiner)
        {
            int totalFields = 11; // Assuming there are 12 total fields in the examiner object
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

                // Add more conditions for other fields as needed
            }

            double percentageFilled = (double)filledFields / totalFields * 100;

            return percentageFilled;
        }


        [Authorize]
        [HttpPost]
        public async Task<ActionResult> SaveTandS(TandSViewModel viewModel, List<FileDocument> fileDocuments)
        {

            try
            {

                if (viewModel == null)
                {
                
                        // Store SweetAlert configuration in TempData
                        TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                        {
                            icon = "warning",  // Changed to warning icon
                            title = "Error!",
                            text = "Something went wrong contact Admin",
                            showConfirmButton = true,
                            confirmButtonColor = "#ffc107", // Warning color

                            timerProgressBar = true,

                            customClass = new
                            {
                                container = "swal2-flicker", // Applies to the entire modal
                                title = "swal2-title-danger" // Applies only to the title
                            }
                        });

                        return Redirect("/TandS/Index");
                    
                }

                if (viewModel.EMS_RESORNON == "RESIDENT" && (viewModel.TANDSADVANCE.ADV_DINNER == null || viewModel.TANDSADVANCE.ADV_OVERNIGHTALLOWANCE == null))
                {

                    // Store SweetAlert configuration in TempData
                    TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                    {
                        icon = "warning",  // Changed to warning icon
                        title = "Error!",
                        text = "Please check some fields can not be 0 since you selected Resident",
                        showConfirmButton = true,
                        confirmButtonColor = "#ffc107", // Warning color

                        timerProgressBar = true,

                        customClass = new
                        {
                            container = "swal2-flicker", // Applies to the entire modal
                            title = "swal2-title-danger" // Applies only to the title
                        }
                    });

                    return Redirect("/TandS/Index");

                }

                if (viewModel.EMS_RESORNON == "NON RESIDENT" && (viewModel.TANDSADVANCE.ADV_DINNER == null || viewModel.TANDSADVANCE.ADV_BREAKFAST == null || viewModel.TANDSADVANCE.ADV_TRANSPORT == null || viewModel.TANDSADVANCE.ADV_ACCOMMODATION_NONRES == null))
                {

                    // Store SweetAlert configuration in TempData
                    TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                    {
                        icon = "warning",  // Changed to warning icon
                        title = "Error!",
                        text = "Please check some fields can not be 0 since you selected Non Resident",
                        showConfirmButton = true,
                        confirmButtonColor = "#ffc107", // Warning color

                        timerProgressBar = true,

                        customClass = new
                        {
                            container = "swal2-flicker", // Applies to the entire modal
                            title = "swal2-title-danger" // Applies only to the title
                        }
                    });

                    return Redirect("/TandS/Index");

                }
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var examinerRecord = await _examinerRepository.GetExaminerRecord(currentUser.IDNumber);
                if (examinerRecord == null)
                {

                    // Store SweetAlert configuration in TempData
                    TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                    {
                        icon = "warning",  // Changed to warning icon
                        title = "ACCESS DENIED!",
                        text = "No Examiner Record is linked to this account . Please check your account credentials. You will be logged out for security reasons.",
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

              

                //var sub_id = viewModel.EMS_SUBKEY.Substring(0, 7);
                //var paperCode = viewModel.EMS_SUBKEY.Substring(7, 2);
                var transaction = examinerRecord.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == viewModel.EMS_SUBKEY && a.EMS_ACTIVITY == viewModel.EMS_PURPOSEOFJOURNEY && a.EMS_NATIONAL_ID == examinerRecord.EMS_NATIONAL_ID);

                if (transaction == null)
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
                var checktands = await _tandSRepository.GetUserTandS(viewModel.EMS_NATIONAL_ID, transaction.EMS_SUBKEY);
                if (checktands == null)
                {

                    DateTime currentDate = DateTime.Now.Date;
                    string formattedDate = currentDate.ToString("yyyy-MM-dd");
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

                    totalAdv += viewModel.TANDSADVANCE.ADV_ACCOMMODATION_NONRES.GetValueOrDefault() * advanceFees.FEE_ACCOMMODATION_NONRES.GetValueOrDefault();

                    totalAdv += viewModel.TANDSADVANCE.ADV_ACCOMMODATION_RES.GetValueOrDefault() * advanceFees.FEE_ACCOMMODATION_RES.GetValueOrDefault();

                    totalAdv += viewModel.TANDSADVANCE.ADV_BREAKFAST.GetValueOrDefault() * advanceFees.FEE_BREAKFAST.GetValueOrDefault();

                    totalAdv += viewModel.TANDSADVANCE.ADV_DINNER.GetValueOrDefault() * advanceFees.FEE_DINNER.GetValueOrDefault();

                    totalAdv += viewModel.TANDSADVANCE.ADV_LUNCH.GetValueOrDefault() * advanceFees.FEE_LUNCH.GetValueOrDefault();

                    totalAdv += viewModel.TANDSADVANCE.ADV_OVERNIGHTALLOWANCE.GetValueOrDefault() * advanceFees.FEE_OVERNIGHTALLOWANCE.GetValueOrDefault();

                    totalAdv += viewModel.TANDSADVANCE.ADV_TEAS.GetValueOrDefault() * advanceFees.FEE_TEA.GetValueOrDefault();

                    totalAdv += viewModel.TANDSADVANCE.ADV_TRANSPORT.GetValueOrDefault() * advanceFees.FEE_TRANSPORT.GetValueOrDefault();

                    var totalTandS = travellingtotal + totalAdv;

                    var newTandS = new TandS()
                    {
                        TANDSCODE = Guid.NewGuid().ToString(),
                        EMS_NATIONAL_ID = viewModel.EMS_NATIONAL_ID,
                        EMS_SUBKEY = viewModel.EMS_SUBKEY,
                        EMS_EXAMINER_CODE = viewModel.EMS_EXAMINER_CODE,
                        DATE = formattedDate,
                        STATUS = viewModel.EMS_RESORNON,
                        EMS_PURPOSEOFJOURNEY = viewModel.EMS_PURPOSEOFJOURNEY,
                        EMS_VENUE = viewModel.EMS_VENUE,
                        SUBJECT_MANAGER_STATUS = "Pending",
                        SUBJECT_MANAGER_DATE = formattedDate,
                        CENTRE_SUPERVISOR_DATE = formattedDate,
                        CENTRE_SUPERVISOR_STATUS = "Pending",
                        ACCOUNTS_STATUS = "Pending",
                        ACCOUNTS_DATE = formattedDate,
                        ACCOUNTS_REVIEW = "Pending",
                        ACCOUNTS_REVIEW_DATE = formattedDate,
                        EMS_TOTAL = totalTandS,

                    };

                    var savedTandS = await _tandSRepository.AddTandS(newTandS, currentUser.Id);


                    if (savedTandS != null)
                    {
                        if (viewModel.TANDSADVANCE != null)
                        {
                            var newAdvance = new TandSAdvance()
                            {
                                TANDSCODE = savedTandS.TANDSCODE,
                                EMS_NATIONAL_ID = savedTandS.EMS_NATIONAL_ID,
                                EMS_EXAMINER_CODE = savedTandS.EMS_EXAMINER_CODE,
                                EMS_SUBKEY = savedTandS.EMS_SUBKEY,
                                EMS_DATE = savedTandS.DATE,
                                ADV_ACCOMMODATION_NONRES = viewModel.TANDSADVANCE.ADV_ACCOMMODATION_NONRES,
                                ADV_ACCOMMODATION_RES = viewModel.TANDSADVANCE.ADV_ACCOMMODATION_RES,
                                ADV_BREAKFAST = viewModel.TANDSADVANCE.ADV_BREAKFAST,
                                ADV_DINNER = viewModel.TANDSADVANCE.ADV_DINNER,
                                ADV_LUNCH = viewModel.TANDSADVANCE.ADV_LUNCH,
                                ADV_OVERNIGHTALLOWANCE = viewModel.TANDSADVANCE.ADV_OVERNIGHTALLOWANCE,
                                ADV_TEAS = viewModel.TANDSADVANCE.ADV_TEAS,
                                ADV_TRANSPORT = viewModel.TANDSADVANCE.ADV_TRANSPORT,
                                ADV_STATUS = viewModel.EMS_RESORNON,
                                ADV_TOTAL = totalAdv

                            };
                            await _detailRepository.AddTandSAdvance(newAdvance, currentUser.Id);
                        }



                        foreach (var detail in viewModel.TANDSDETAILS)
                        {

                            decimal travellingtotal2 = 0;
                            travellingtotal2 += detail.EMS_BUSFARE.GetValueOrDefault();
                            travellingtotal2 += detail.EMS_ACCOMMODATION.GetValueOrDefault();
                            travellingtotal2 += detail.EMS_LUNCH.GetValueOrDefault();
                            travellingtotal2 += detail.EMS_DINNER.GetValueOrDefault();
                            var newTandSDetails = new TandSDetail()
                            {
                                EMS_NATIONAL_ID = savedTandS.EMS_NATIONAL_ID,
                                EMS_SUBKEY = savedTandS.EMS_SUBKEY,
                                TANDSCODE = savedTandS.TANDSCODE,
                                EMS_EXAMINER_CODE = savedTandS.EMS_EXAMINER_CODE,
                                EMS_DATE = detail.EMS_DATE,
                                EMS_DEPARTURE = detail.EMS_DEPARTURE,
                                EMS_ARRIVAL = detail.EMS_ARRIVAL,
                                EMS_PLACE = detail.EMS_PLACE,
                                EMS_BUSFARE = detail.EMS_BUSFARE,
                                EMS_LUNCH = detail.EMS_LUNCH,
                                EMS_ACCOMMODATION = detail.EMS_ACCOMMODATION,
                                EMS_DINNER = detail.EMS_DINNER,
                                EMS_TOTAL = travellingtotal2,

                            };

                            await _detailRepository.AddTandSDetail(newTandSDetails, currentUser.Id);

                        }

                        foreach (var filedoc in fileDocuments)
                        {
                            if (filedoc.Files != null)
                            {
                                foreach (var file in filedoc.Files)
                                {
                                    if (IsFileExtensionAllowed(file.FileName))
                                    {
                                        // Process the file
                                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                                        // Use a unique file name: NationalID, Subkey, and a unique identifier
                                        var uniqueFileName = $"{savedTandS.EMS_NATIONAL_ID}_{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                                        var filePath = Path.Combine(uploadPath, uniqueFileName);

                                        Directory.CreateDirectory(uploadPath); // Ensure the directory exists
                                        using (var stream = new FileStream(filePath, FileMode.Create))
                                        {
                                            file.CopyTo(stream);
                                        }

                                        // Save file info to the database
                                        var filetosave = new TandSFile()
                                        {
                                            EMS_NATIONAL_ID = savedTandS.EMS_NATIONAL_ID,
                                            EMS_SUBKEY = savedTandS.EMS_SUBKEY,
                                            TANDSCODE = savedTandS.TANDSCODE,
                                            EMS_EXAMINER_CODE = savedTandS.EMS_EXAMINER_CODE,
                                            FileName = uniqueFileName,
                                            Currency = filedoc.FileCurrency,
                                        };

                                        await _tandSFilesRepository.AddTandSFile(filetosave, currentUser.Id);
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("files", "Invalid file type.");
                                    }
                                }
                            }
                        }

                        transaction.EMS_VENUE = viewModel.EMS_VENUE;
                        _context.Update(transaction);
                        await _context.SaveChangesAsync();



                    }
                }

                return Redirect("/TandS/Index?isSuccess=true");

            }
            catch (Exception ex)
            {

                //ElmahExtensions.RaiseError(ex);
                TempData["Error"] = "Something Went Wrong: " + ex.Message;
                return Redirect("Error/ServerError");
            }


        }


        private bool IsFileExtensionAllowed(string fileName)
        {
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(fileExtension);
        }

        [Authorize]
        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            // Ensure fileName is not a path traversal attempt
            var safeFileName = Path.GetFileName(fileName);

            // Retrieve the file path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", safeFileName);

            // Check if the file exists
            if (System.IO.File.Exists(filePath))
            {
                // Provide the file for download
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, safeFileName);
            }
            else
            {
                // Handle file not found
                return NotFound();
            }
        }

        [Authorize(Roles = "Accounts,PeerReviewer,Admin,SuperAdmin")]
        public async Task<IActionResult> TandSList(string examCode = "", string subjectCode = "", string paperCode = "", string venue = "", string activity = "", string regionCode = "")
        {
            // First try to get existing session
            var userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session") ?? new SessionModel();

            // Update session only with provided parameters (won't overwrite with empty values)
            if (!string.IsNullOrEmpty(activity))
            {
                userSession.Activity = activity;
            }

            if (!string.IsNullOrEmpty(venue))
            {
                userSession.Venue = venue;
            }

            if (!string.IsNullOrEmpty(examCode))
            {
                userSession.ExamCode = examCode;
            }

            if (!string.IsNullOrEmpty(subjectCode))
            {
                userSession.SubjectCode = subjectCode.Length > 3 ? subjectCode.Substring(3) : subjectCode;
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                userSession.PaperCode = paperCode;
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                userSession.RegionCode = regionCode;
            }

            // Always save the updated session
            HttpContext.Session.SetObjectAsJson("Session", userSession);

        

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


            ViewBag.ExamCode = userSession.ExamCode ?? "";
            ViewBag.SubjectCode = userSession.SubjectCode ?? "";
            ViewBag.PaperCode = userSession.PaperCode ?? "";
            ViewBag.Venue = userSession.Venue ?? "";
            ViewBag.Activity = userSession.Activity ?? "";
            ViewBag.RegionCode = userSession.RegionCode ?? "";

            return View();
        }


        [Authorize(Roles = "Accounts,PeerReviewer,Admin,SuperAdmin")]
        public async Task<IActionResult> TandSFullList(string examCode = "", string subjectCode = "", string paperCode = "", string venue = "", string activity = "", string regionCode = "")
       {


            if (!string.IsNullOrEmpty(activity))
            {

                ViewBag.Activity = activity;

            }
            if (!string.IsNullOrEmpty(venue))
            {
                ViewBag.Venue = venue;
            }

            if (!string.IsNullOrEmpty(examCode))
            {
                ViewBag.ExamCode = examCode ?? "";
            }


            if (!string.IsNullOrEmpty(subjectCode))
            {
                ViewBag.SubjectCode = subjectCode.Substring(3) ?? "";
            

            }


            if (!string.IsNullOrEmpty(paperCode))
            {

                ViewBag.PaperCode = paperCode ?? "";
            }


            if (!string.IsNullOrEmpty(regionCode))
            {
                ViewBag.RegionCode = regionCode;
            }


          

            if (string.IsNullOrEmpty(activity) && string.IsNullOrEmpty(venue) && string.IsNullOrEmpty(examCode) && string.IsNullOrEmpty(subjectCode) && string.IsNullOrEmpty(paperCode) && string.IsNullOrEmpty(regionCode))
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

 
            return View();
        }




        [Authorize]
        public async Task<IActionResult> GetData(string examCode = "", string subjectCode = "", string paperCode = "", string venue = "", string statuss = "", string activity = "", string regionCode = "")
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var totalCount = 0;
            var approvedtands = 0;
            var pendingtands = 0;


            IEnumerable<TandS> tands = new List<TandS>();
            IEnumerable<TandSListViewModel> model = new List<TandSListViewModel>();
            List<TandSListViewModel> modelList = new List<TandSListViewModel>();

            if (!string.IsNullOrEmpty(activity) || !string.IsNullOrEmpty(venue))
            {
                var tandsList = await _tandSRepository.GetAllTandSByComponent2(examCode, subjectCode, paperCode, venue, regionCode, activity, userRoles.ToList());


                var checkPresent = await _registerRepository.GetComponentRegister2(examCode, subjectCode, paperCode, activity, regionCode, venue);
                totalCount = checkPresent.Where(a => a.RegisterStatus == "Present").Count();



                if (userRoles != null && userRoles.Contains("Accounts"))
                {

                    tands = tandsList.Where(a => a.CENTRE_SUPERVISOR_STATUS == "Approved" && a.ACCOUNTS_STATUS == "Pending");
                    approvedtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved").Count();
                    pendingtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Pending").Count();

                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tands = statuss switch
                        {
                            "TotalInvited" => tands, // No filter for total
                            "ApprovedCount" => tands.Where(e => e.ACCOUNTS_STATUS == "Approved"),
                            "PendingCount" => tands.Where(e => e.ACCOUNTS_STATUS == "Pending"),

                            _ => tands
                        };
                    }

                }
                else if (userRoles != null && userRoles.Contains("PeerReviewer"))
                {

                    tands = tandsList.Where(a => a.ACCOUNTS_STATUS == "Approved" && a.ACCOUNTS_REVIEW == "Pending");
                    approvedtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Approved").Count();
                    pendingtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Pending").Count();


                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tands = statuss switch
                        {
                            "TotalInvited" => tands, // No filter for total
                            "ApprovedCount" => tands.Where(e => e.ACCOUNTS_REVIEW == "Approved"),
                            "PendingCount" => tands.Where(e => e.ACCOUNTS_REVIEW == "Pending"),

                            _ => tands
                        };
                    }
                }
                else if (userRoles != null && userRoles.Contains("AssistantAccountant"))
                {
                    tands = tandsList.Where(a => a.ACCOUNTS_STATUS == "Approved" && a.ACCOUNTS_REVIEW == "Approved");
                    approvedtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved").Count();
                    pendingtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending").Count();

                }
                else if (userRoles != null && userRoles.Contains("Admin"))
                {
                    tands = tandsList.ToList();

                    approvedtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved").Count();
                    pendingtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending").Count();

                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tands = statuss switch
                        {
                            "TotalInvited" => tands, // No filter for total
                            "ApprovedCount" => tands.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved"),
                            "PendingCount" => tands.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending"),

                            _ => tands
                        };
                    }


                }
            }
            else
            {
                if (userRoles != null && userRoles.Contains("SuperAdmin"))
                {
                    tands = await _tandSRepository.GetAllTandS();

                    approvedtands = tands.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved").Count();
                    pendingtands = tands.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending").Count();

                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tands = statuss switch
                        {
                            "TotalInvited" => tands, // No filter for total
                            "ApprovedCount" => tands.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved"),
                            "PendingCount" => tands.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending"),

                            _ => tands
                        };
                    }

                }
            }

            modelList = tands.Select(ex => new TandSListViewModel
            {
                ExaminerCode = ex.EMS_EXAMINER_CODE,
                FirstName = ex.Examiner.EMS_EXAMINER_NAME,
                LastName = ex.Examiner.EMS_LAST_NAME,
                IDNumber = ex.EMS_NATIONAL_ID,
                Subject = ex.Examiner.EMS_SUB_SUB_ID + "/" + ex.Examiner.EMS_PAPER_CODE,
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
            }).ToList();


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
                model = model.Where(p =>
    (p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.LastName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.IDNumber?.ToLower().Contains(searchValue.ToLower()) ?? false)
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
                totalCount,
                approvedtands,
                pendingtands,
                returned,
                data
            };

            return Ok(jsonData);

        }

        [Authorize]
        public async Task<IActionResult> GetDataFullList(string examCode = "", string subjectCode = "", string paperCode = "", string venue = "", string request = "", string statuss = "", string activity = "", string regionCode = "")
        {

            IEnumerable<TandSListViewModel> model = new List<TandSListViewModel>();
            List<TandSListViewModel> modelList = new List<TandSListViewModel>();

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            IEnumerable<TandS> tands = new List<TandS>();

            var totalCount = 0;
            var approvedtands = 0;
            var pendingtands = 0;
            var InvitedExaminers = 0;
            var createdTandS = 0;
            var centreSupervisor = 0;


            if (!string.IsNullOrEmpty(activity) || !string.IsNullOrEmpty(venue))
            {

                var tandsList = await _tandSRepository.GetAllTandSByComponent2(examCode, subjectCode, paperCode, venue, regionCode, activity, userRoles.ToList());



                var checkPresent = await _registerRepository.GetComponentRegister2(examCode, subjectCode, paperCode, activity, regionCode, venue);

                totalCount = checkPresent.Where(a => a.RegisterStatus == "Present").Count();

                var totalTandSCreated = await _tandSRepository.GetAllAppliedTandS(examCode, subjectCode, paperCode, venue, regionCode, activity, userRoles.ToList());

                 InvitedExaminers = checkPresent.Count();
                createdTandS = totalTandSCreated.Count();
                centreSupervisor = tandsList.Where(a => a.CENTRE_SUPERVISOR_STATUS == "Approved").Count();
              


                if (userRoles != null && userRoles.Contains("Accounts"))
                {

                    tands = tandsList.Where(a => a.CENTRE_SUPERVISOR_STATUS == "Approved");
                    approvedtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved").Count();
                    pendingtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Pending").Count();

                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tands = statuss switch
                        {
                            "TotalInvited" => tands, // No filter for total
                            "ApprovedCount" => tands.Where(e => e.ACCOUNTS_STATUS == "Approved"),
                            "PendingCount" => tands.Where(e => e.ACCOUNTS_STATUS == "Pending"),

                            _ => tands
                        };
                    }

                }
                else if (userRoles != null && userRoles.Contains("PeerReviewer"))
                {

                    tands = tandsList.Where(a => a.ACCOUNTS_STATUS == "Approved");
                    approvedtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Approved").Count();
                    pendingtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Pending").Count();


                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tands = statuss switch
                        {
                            "TotalInvited" => tands, // No filter for total
                            "ApprovedCount" => tands.Where(e => e.ACCOUNTS_REVIEW == "Approved"),
                            "PendingCount" => tands.Where(e => e.ACCOUNTS_REVIEW == "Pending"),

                            _ => tands
                        };
                    }
                }
                else if (userRoles != null && userRoles.Contains("AssistantAccountant"))
                {
                    tands = tandsList.Where(a => a.ACCOUNTS_STATUS == "Approved");
                    approvedtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved").Count();
                    pendingtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending").Count();

                }
                else if (userRoles != null && userRoles.Contains("Admin"))
                {
                    tands = tandsList.Where(a => a.EMS_VENUE == venue);
                    approvedtands = tandsList.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved").Count();
                    pendingtands = tandsList.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending").Count();

                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tands = statuss switch
                        {
                            "TotalInvited" => tands, // No filter for total
                            "ApprovedCount" => tands.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved"),
                            "PendingCount" => tands.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending"),

                            _ => tands
                        };
                    }


                }
            }
            else
            {

                if (userRoles != null && userRoles.Contains("SuperAdmin"))
                {
                    tands = await _tandSRepository.GetAllTandS();

                    approvedtands = tands.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved").Count();
                    pendingtands = tands.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending").Count();

                    if (!string.IsNullOrEmpty(statuss))
                    {
                        tands = statuss switch
                        {
                            "TotalInvited" => tands, // No filter for total
                            "ApprovedCount" => tands.Where(e => e.ACCOUNTS_STATUS == "Approved" && e.ACCOUNTS_REVIEW == "Approved"),
                            "PendingCount" => tands.Where(e => e.ACCOUNTS_REVIEW == "Pending" || e.ACCOUNTS_STATUS == "Pending"),

                            _ => tands
                        };
                    }

                }
            }

            modelList = tands.Select(ex => new TandSListViewModel
            {
                ExaminerCode = ex.EMS_EXAMINER_CODE,
                FirstName = ex.Examiner.EMS_EXAMINER_NAME,
                LastName = ex.Examiner.EMS_LAST_NAME,
                IDNumber = ex.EMS_NATIONAL_ID,
                Subject = ex.Examiner.EMS_SUB_SUB_ID + "/" + ex.Examiner.EMS_PAPER_CODE,
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
            }).ToList();


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
                model = model.Where(p =>
    (p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.LastName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.IDNumber?.ToLower().Contains(searchValue.ToLower()) ?? false)
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
                totalCount,
                approvedtands,
                pendingtands,
                returned,
                invitedExaminers = InvitedExaminers,
                createdTandS,
                centreSupervisor,
                data
            };

            return Ok(jsonData);

        }

        [Authorize(Roles = "Accounts,PeerReviewer,Admin,SuperAdmin")]
        public async Task<IActionResult> TandSReview(string claimId, string nationalId, string examinerCode, string subKey, bool isSuccess = false)
        {
            try
            {
                List<TandSDetail> tandSDetails = new List<TandSDetail>();
                IEnumerable<TandSFile> tandSFileCollection = new List<TandSFile>();

                var checktands = await _tandSRepository.GetOneTandS(nationalId, claimId, subKey);

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


                tandSFileCollection = await _tandSFilesRepository.GetTandSFiles(checktands.EMS_NATIONAL_ID, checktands.TANDSCODE, checktands.EMS_SUBKEY, checktands.EMS_EXAMINER_CODE);


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
                    //return RedirectToAction("TandSReview", new { claimId = claimId, nationalId = nationalId, examinerCode = examinerCode, subKey = subKey, isSuccess = false });
                    return Redirect($"/TandS/TandSReview?claimId={claimId}&nationalId={nationalId}&examinerCode={examinerCode}&subKey={subKey}&isSuccess=false");

                }


                return View(examinerData);

            }
            catch (Exception ex)
            {

                TempData["Error"] = "Something Went Wrong: " + ex.Message;
                return Redirect("Error/ServerError");
            }



        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UpdateTandS(TandSViewModel viewModel)
        {
            try
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


                await _tandSRepository.UpdateTandS(tanstobeupdated, currentUser.Id);

                //return RedirectToAction("TandSReview", new { claimId = viewModel.TANDSCODE, nationalId = viewModel.EMS_NATIONAL_ID, examinerCode = viewModel.EMS_EXAMINER_CODE, subKey = viewModel.EMS_SUBKEY, isSuccess = true });

                return Redirect($"/TandS/TandSReview?claimId={viewModel.TANDSCODE}&nationalId={viewModel.EMS_NATIONAL_ID}&examinerCode={viewModel.EMS_EXAMINER_CODE}&subKey={viewModel.EMS_SUBKEY}&isSuccess=true");

            }
            catch (Exception ex)
            {

                TempData["Error"] = "Something Went Wrong: " + ex.Message;
                return Redirect("Error/ServerError");
            }



        }

        [Authorize]
        public async Task<IActionResult> AdvanceFees(bool isSuccess = false)
        {
            var fees = await _advanceFeesRepository.GetFirstAdvanceFee();

            ViewBag.IsSuccess = isSuccess;

            return View(fees);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateFees(TandSAdvanceFees model)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            await _advanceFeesRepository.UpdateAdvanceFees(model, currentUser.Id);

            return RedirectToAction("AdvanceFees", new { isSuccess = true });
        }

        [Authorize(Roles = "SubjectManager,CentreSupervisor")]
        public async Task<IActionResult> ApprovalList(string examCode = "", string subjectCode = "", string paperCode = "", string venue = "", string activity = "", string regionCode = "")
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var userSession = new SessionModel();

            if (userRoles != null && userRoles.Contains("SubjectManager"))
            {


                if (!string.IsNullOrEmpty(venue) && !string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) &&
                !string.IsNullOrEmpty(paperCode) && !string.IsNullOrEmpty(activity))
                {
                    userSession = new SessionModel()
                    {
                        ExamCode = examCode,
                        SubjectCode = subjectCode.Substring(3),
                        PaperCode = paperCode,
                        Venue = venue,
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
                ViewBag.SubjectCode = userSession.SubjectCode;
                ViewBag.PaperCode = userSession.PaperCode;
                ViewBag.Venue = userSession.Venue;
                ViewBag.Activity = userSession.Activity;

                ViewBag.RegionCode = string.IsNullOrEmpty(regionCode) ? "" : userSession.RegionCode;

            }

            if (userRoles != null && userRoles.Contains("CentreSupervisor"))
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session") ?? new SessionModel();

                // Update session only with provided parameters (won't overwrite with empty values)
                if (!string.IsNullOrEmpty(activity))
                {
                    userSession.Activity = activity;
                }

                if (!string.IsNullOrEmpty(venue))
                {
                    userSession.Venue = venue;
                }

                if (!string.IsNullOrEmpty(examCode))
                {
                    userSession.ExamCode = examCode;
                }

                if (!string.IsNullOrEmpty(subjectCode))
                {
                    userSession.SubjectCode = subjectCode.Length > 3 ? subjectCode.Substring(3) : subjectCode;
                }

                if (!string.IsNullOrEmpty(paperCode))
                {
                    userSession.PaperCode = paperCode;
                }

                if (!string.IsNullOrEmpty(regionCode))
                {
                    userSession.RegionCode = regionCode;
                }

                // Always save the updated session
                HttpContext.Session.SetObjectAsJson("Session", userSession);



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


                ViewBag.ExamCode = userSession.ExamCode ?? "";
                ViewBag.SubjectCode = userSession.SubjectCode ?? "";
                ViewBag.PaperCode = userSession.PaperCode ?? "";
                ViewBag.Venue = userSession.Venue ?? "";
                ViewBag.Activity = userSession.Activity ?? "";
                ViewBag.RegionCode = userSession.RegionCode ?? "";
            }

                return View();

        }



        [Authorize]
        public async Task<IActionResult> GetDataa(string examCode = "", string subjectCode = "", string paperCode = "", string venue = "", string statuss = "", string activity = "", string regionCode = "")
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            IEnumerable<TandSListViewModel> model = new List<TandSListViewModel>();
            List<TandSListViewModel> modelList = new List<TandSListViewModel>();
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var totalCount = 0;
            var approvedtands = 0;
            var pendingtands = 0;


            var tandsList = await _tandSRepository.GetAllTandSByComponent(examCode, subjectCode, paperCode
                , venue, regionCode, activity, userRoles.ToList());

            var checkPresent = new List<ExaminerScriptsMarked>();

       

                checkPresent = (await _registerRepository
   .GetComponentRegister(examCode, subjectCode, paperCode, activity, regionCode))
   .ToList();

            

            
                totalCount = checkPresent.Where(a => a.RegisterStatus == "Present").Count();



            IEnumerable<TandS> tands = new List<TandS>();



            if (userRoles != null && userRoles.Contains("SubjectManager"))
            {

                tands = tandsList.Where(a => a.SUBJECT_MANAGER_STATUS == "Pending" && a.EMS_VENUE == venue);
                approvedtands = tandsList.Where(e => e.SUBJECT_MANAGER_STATUS == "Recommended").Count();
                pendingtands = tandsList.Where(e => e.SUBJECT_MANAGER_STATUS == "Pending").Count();

                if (!string.IsNullOrEmpty(statuss))
                {
                    tands = statuss switch
                    {
                        "TotalInvited" => tands, // No filter for total
                        "ApprovedCount" => tands.Where(e => e.SUBJECT_MANAGER_STATUS == "Recommended"),
                        "PendingCount" => tands.Where(e => e.SUBJECT_MANAGER_STATUS == "Pending"),

                        _ => tands
                    };
                }

            }
            else if (userRoles != null && userRoles.Contains("CentreSupervisor"))
            {
                tands = tandsList.Where(a => a.SUBJECT_MANAGER_STATUS == "Pending" || a.CENTRE_SUPERVISOR_STATUS == "Pending" && a.EMS_VENUE == venue);
                approvedtands = tandsList.Where(e => e.CENTRE_SUPERVISOR_STATUS == "Approved").Count();
                pendingtands = tandsList.Where(e => e.CENTRE_SUPERVISOR_STATUS == "Pending").Count();

                if (!string.IsNullOrEmpty(statuss))
                {
                    tands = statuss switch
                    {
                        "TotalInvited" => tands, // No filter for total
                        "ApprovedCount" => tands.Where(e => e.CENTRE_SUPERVISOR_STATUS == "Approved"),
                        "PendingCount" => tands.Where(e => e.CENTRE_SUPERVISOR_STATUS == "Pending"),

                        _ => tands
                    };
                }


            }


            modelList = tands.Select(ex =>
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
                model = model.Where(p =>
(p.FirstName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
(p.LastName?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
(p.IDNumber?.ToLower().Contains(searchValue.ToLower()) ?? false)
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




        [Authorize(Roles = "SubjectManager,CentreSupervisor")]
        public async Task<IActionResult> Approve(string claimId, string nationalId, string examinerCode, string subKey)
        {

            try
            {
                // Fetch TandS record and advance fees
                var checkTandS = await _tandSRepository.GetOneTandS(nationalId, claimId, subKey);
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
            catch (Exception ex)
            {

                TempData["Error"] = "Something Went Wrong: " + ex.Message;
                return Redirect("Error/ServerError");
            }

        }


        [Authorize(Roles = "CentreSupervisor,OfficerSpecialNeeds,PeerReviewer")]
        [HttpPost]
        public async Task<IActionResult> Approve(string idNumber, string tandscode, string subKey, string examinerCode, string comment)
        {
            try
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

                    await _tandSRepository.ApproveTandS(tands, "SubjectManager", currentUser.Id);
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

                    await _tandSRepository.ApproveTandS(tands, "CentreSupervisor", currentUser.Id);
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

                    await _tandSRepository.ApproveTandS(tands, "PeerReviewer", currentUser.Id);
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
                    await _tandSRepository.ApproveTandS(tands, "AssistantAccountant", currentUser.Id);
                    return Json(new { success = true, message = "Approved successful" });
                }
                else
                {
                    return Json(new { success = false, message = "User does not have the required role" });
                }

            }
            catch (Exception ex)
            {

                TempData["Error"] = "Something Went Wrong: " + ex.Message;
                return Redirect("Error/ServerError");
            }


        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reject(string idNumber, string tandscode, string subKey, string examinercode, string comment)
        {
            try
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



                await _tandSRepository.RejectTandS(tands, "SubjectManager", currentUser, comment);



                return Json(new { success = true, message = "Rejection successful" });
            }
            catch (Exception ex)
            {

                TempData["Error"] = "Something Went Wrong: " + ex.Message;
                return Redirect("Error/ServerError");
            }

        }


        [Authorize]
        public async Task<IActionResult> ChangeTandS(string tandsCode, string idnumber, string subkey, string examinercode)
        {
            await _tandSRepository.ChangeTandS(tandsCode, idnumber, subkey, examinercode);
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReturnTandS(string idNumber, string subjectcode, string papercode, string tandscode, string subKey, string examinercode, string comment)
        {

            try
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

                    await _tandSRepository.ApproveTandS(tands, "PeerReviewer", currentUser.Id);
                    return Json(new { success = true, message = "Returning successful" });

                }

                else
                {
                    return Json(new { success = false, message = "User does not have the required role" });
                }

            }
            catch (Exception ex)
            {

                TempData["Error"] = "Something Went Wrong: " + ex.Message;
                return Redirect("Error/ServerError");
            }


        }


        [Authorize]
        public async Task<IActionResult> GetStatistics(string examCode, string subjectCode, string paperCode)
        {
            var query = _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.AsQueryable();
            var subKey = examCode + subjectCode + paperCode;
            // Filter by examCode, subjectCode, paperCode if provided
            if (!string.IsNullOrEmpty(subKey))
            {
                query = query.Where(t => t.EMS_SUBKEY.StartsWith(subKey));
            }



            var totalCount = await query.CountAsync();
            var approvedCount = await query.CountAsync(t => t.ACCOUNTS_STATUS == "Approved");
            var notApprovedCount = await query.CountAsync(t => t.ACCOUNTS_STATUS != "Approved");

            return Json(new
            {
                TotalCount = totalCount,
                ApprovedCount = approvedCount,
                NotApprovedCount = notApprovedCount
            });
        }


        [Authorize(Roles = "Admin,SuperAdmin,SubjectManager,CentreSupervisor")]
        [HttpGet]
        public async Task<JsonResult> Delete(string idNumber, string claimId, string examinerCode, string subKey)
        {
            try
            {
                var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var item = await _tandSRepository.GetOneTandS(idNumber, claimId, subKey);
                if (item != null)
                {
                    await _tandSRepository.DeleteTandS(item.EMS_NATIONAL_ID, subKey, currentUser);
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Item not found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }





        private async Task<TandS> MakeChangesToTandS(TandS tands, ApplicationUser user)
        {
            List<TandSDetail> tandSDetails = new List<TandSDetail>();
            TandSAdvance advance = new TandSAdvance();
            if (tands.EMS_NATIONAL_ID != null)
            {
                tandSDetails = await _detailRepository.GetTandSDetails(tands.EMS_NATIONAL_ID, tands.TANDSCODE, tands.EMS_SUBKEY, tands.EMS_EXAMINER_CODE);
                advance = await _detailRepository.GetTandSAdvance(tands.EMS_NATIONAL_ID, tands.TANDSCODE, tands.EMS_SUBKEY, tands.EMS_EXAMINER_CODE);

            }
            var advanceFees = await _advanceFeesRepository.GetAdvanceFees();


            List<TandSDetail> newadjustedDetails = new List<TandSDetail>();
            foreach (var item in tandSDetails)
            {
                var newDetail = new TandSDetail()
                {
                    Id = item.Id,
                    ADJ_BUSFARE = item.EMS_BUSFARE,
                    ADJ_ACCOMMODATION = item.EMS_ACCOMMODATION,
                    ADJ_DINNER = item.EMS_DINNER,
                    ADJ_LUNCH = item.EMS_LUNCH,
                    ADJ_TOTAL = item.EMS_TOTAL,
                    ADJ_BY = user.Email,

                };

                newadjustedDetails.Add(newDetail);

            }

            await _detailRepository.UpdateTandSDetail(newadjustedDetails, "");



            decimal totalAdv = 0;


            // Parse and add each value to the total if parsing is successful
            //if (decimal.TryParse(advance.ADV_ACCOMMODATION_NONRES, out decimal nonRes))
            //{
            //    decimal accommodationFeeNonRes = decimal.Parse(advanceFees.FirstOrDefault()?.FEE_ACCOMMODATION_NONRES ?? "0");
            //    totalAdv += nonRes * accommodationFeeNonRes;
            //}

            //if (decimal.TryParse(advance.ADV_ACCOMMODATION_RES, out decimal res))
            //{
            //    decimal accommodationFeeRes = decimal.Parse(advanceFees.FirstOrDefault()?.FEE_ACCOMMODATION_RES ?? "0");
            //    totalAdv += res * accommodationFeeRes;
            //}

            //if (decimal.TryParse(advance.ADV_BREAKFAST, out decimal breakfast))
            //{
            //    decimal breakfastFee = decimal.Parse(advanceFees.FirstOrDefault()?.FEE_BREAKFAST ?? "0");
            //    totalAdv += breakfast * breakfastFee;
            //}

            //if (decimal.TryParse(advance.ADV_DINNER, out decimal dinnerr))
            //{
            //    decimal dinnerFee = decimal.Parse(advanceFees.FirstOrDefault()?.FEE_DINNER ?? "0");
            //    totalAdv += dinnerr * dinnerFee;
            //}

            //if (decimal.TryParse(advance.ADV_LUNCH, out decimal lunchh))
            //{
            //    decimal lunchFee = decimal.Parse(advanceFees.FirstOrDefault()?.FEE_LUNCH ?? "0");
            //    totalAdv += lunchh * lunchFee;
            //}

            //if (decimal.TryParse(advance.ADV_OVERNIGHTALLOWANCE, out decimal overnightAllowance))
            //{
            //    decimal overnightAllowanceFee = decimal.Parse(advanceFees.FirstOrDefault()?.FEE_OVERNIGHTALLOWANCE ?? "0");
            //    totalAdv += overnightAllowance * overnightAllowanceFee;
            //}

            //if (decimal.TryParse(advance.ADV_TEAS, out decimal teas))
            //{
            //    decimal teasFee = decimal.Parse(advanceFees.FirstOrDefault()?.FEE_TEA ?? "0");
            //    totalAdv += teas * teasFee;
            //}

            //if (decimal.TryParse(advance.ADV_TRANSPORT, out decimal transport))
            //{
            //    decimal transportFee = decimal.Parse(advanceFees.FirstOrDefault()?.FEE_TRANSPORT ?? "0");
            //    totalAdv += transport * transportFee;
            //}



            //advance.ADV_TOTAL = totalAdv.ToString();

            advance.ADJ_ADV_ACCOMMODATION_NONRES = advance.ADV_ACCOMMODATION_NONRES;
            advance.ADJ_ADV_TEAS = advance.ADV_TEAS;
            advance.ADJ_ADV_TOTAL = advance.ADV_TOTAL;
            advance.ADJ_ADV_LUNCH = advance.ADV_LUNCH;
            advance.ADJ_ADV_ACCOMMODATION_RES = advance.ADV_ACCOMMODATION_RES;
            advance.ADJ_ADV_OVERNIGHTALLOWANCE = advance.ADV_OVERNIGHTALLOWANCE;
            advance.ADJ_ADV_TRANSPORT = advance.ADV_TRANSPORT;
            advance.ADJ_ADV_DINNER = advance.ADV_DINNER;
            advance.ADJ_ADV_BREAKFAST = advance.ADV_BREAKFAST;



            await _detailRepository.UpdateTandSAdvance(advance, "");




            var idnum = tands.EMS_NATIONAL_ID != null ? tands.EMS_NATIONAL_ID : tandSDetails[0].EMS_NATIONAL_ID;

            DateTime currentDate = DateTime.Now.Date;
            string formattedDate = currentDate.ToString("yyyy-MM-dd");




            var tanstobeupdated = new TandS
            {

                DATE_ADJ = formattedDate,
                EMS_NATIONAL_ID = tands.EMS_NATIONAL_ID,
                EMS_EXAMINER_CODE = tands.EMS_EXAMINER_CODE,
                TANDSCODE = tands.TANDSCODE,
                ACCOUNTS_DATE = formattedDate,
                STATUS = "Approved",
                ACCOUNTS_STATUS = "Approved",
                ACCOUNTS_STATUS_BY = user.UserName,
                ADJ_BY = user.Email,

            };

            decimal total = 0;

            foreach (var detail in tandSDetails)
            {
                // Convert string properties to decimal and add them to the total
                //if (decimal.TryParse(detail.EMS_BUSFARE, out decimal busfare))
                //{
                //    total += busfare;
                //}

                //if (decimal.TryParse(detail.EMS_ACCOMMODATION, out decimal accommodation))
                //{
                //    total += accommodation;
                //}

                //if (decimal.TryParse(detail.EMS_LUNCH, out decimal lunch))
                //{
                //    total += lunch;
                //}

                //if (decimal.TryParse(detail.EMS_DINNER, out decimal dinner))
                //{
                //    total += dinner;
                //}
            }

            string totalPassedToString = total.ToString();
            //tanstobeupdated.ADJ_TOTAL = totalPassedToString;



            await _tandSRepository.UpdateTandS(tanstobeupdated, "");
            return tanstobeupdated;
        }


        private bool checkTandSApprove(TandS tandS)
        {

            return false;
        }

    }
}

