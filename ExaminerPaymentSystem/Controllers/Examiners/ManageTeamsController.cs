using ClosedXML.Excel;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Extensions;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Repositories;
using ExaminerPaymentSystem.Services;
using ExaminerPaymentSystem.ViewModels;
using ExaminerPaymentSystem.ViewModels.Examiners;
using iText.Forms.Xfdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.RegularExpressions;



namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class ManageTeamsController : Controller
    {
        private readonly IManageTeamsRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserRepository _userRepository;
        private readonly IExaminerRepository _examinerRepository;
        private readonly IExamCodesRepository _examCodesRepository;
        private readonly ISubjectsRepository _subjectsRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMaterialRepository _materialRepository;
        private readonly IUserManagementService _userManagementService;
        private readonly ITandSRepository _andSRepository;


        public ManageTeamsController(IManageTeamsRepository repository, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IUserRepository userRepository,
            IExaminerRepository examinerRepository, IExamCodesRepository examCodesRepository,
            ISubjectsRepository subjectsRepository, ApplicationDbContext context,
            IMaterialRepository materialRepository, IUserManagementService userManagementService,
            ITandSRepository andSRepository)
        {
            _repository = repository;
            _userManager = userManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
            _examinerRepository = examinerRepository;
            _examCodesRepository = examCodesRepository;
            _subjectsRepository = subjectsRepository;
            _context = context;
            _materialRepository = materialRepository;
            _userManagementService = userManagementService;
            _andSRepository = andSRepository;
        }


        public async Task<IActionResult> Report(string examCode, string subject, string paperCode, string regionCode)
        {

            return View();
        }

        public async Task<IActionResult> Report2(string examCode, string subject, string paperCode, string regionCode)
        {

            return View();
        }

        public async Task<IActionResult> Report3(string examCode, string subject, string paperCode, string regionCode)
        {

            return View();
        }

        public async Task<IActionResult> Report4(string examCode, string subject, string paperCode, string regionCode)
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetDataDownload([FromBody] DataTablesRequest request)
        {
           
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            // Access filter values from the request object
            var filterStatus = request.FilterStatus;

            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;

            // Query your data based on the filters
        
            var entries = await _context.EXM_SCRIPT_CAPTURED.ToListAsync();
            var refcat = await _context.REF_CAT_PAPER.ToListAsync();
            var material = await _context.MaterialTransaction.ToListAsync();

            if (!string.IsNullOrEmpty(examCode))
            {
                entries = entries.Where(a => a.ExamCode == examCode).ToList();
                //refcat = refcat.Where(a => a.CTP_PPR_SUB_PAPER_CODE.StartsWith(examCode)).ToList();
                //material = material.Where(a => a.SUBSUBID.StartsWith(examCode)).ToList();
            }

            if (!string.IsNullOrEmpty(subject))
            {
                entries = entries.Where(a => a.SubjectCode == subject.Substring(3)).ToList();
        
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                entries = entries.Where(a => a.PaperCode == paperCode).ToList();

            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                entries = entries.Where(a => a.RegionCode == regionCode).ToList();
 
            }

            var ListData = new List<TransactionReport3>();

            foreach (var item in entries)
            {
                var refcatpapers = new List<Apportioned>();
                var materials = new List<MaterialTransaction>();
                var examiners = new List<ExaminerScriptsMarked>();
           
                var newreport3 = new TransactionReport3()
                {
                    ExamCode = item.ExamCode,
                    Subject = item.SubjectCode,
                    Paper = item.PaperCode,
                    Region = "",
                    Card = "No",
                    Material = "No",
                    Entries = item.ApportionedScripts.ToString(),
                    Examiners = "0",
                };

                if (!string.IsNullOrEmpty(item.RegionCode) && item.SubjectCode.StartsWith("7"))
                {
                    refcatpapers = await _context.REF_CAT_PAPER.Where(a => a.CTP_PPR_SUB_PAPER_CODE == (item.ExamCode + item.SubjectCode + item.PaperCode) && a.CTP_REGION_CODE == item.RegionCode).ToListAsync();
                    materials = material.Where(a => 
                        a.Region == regionCode && a.SUBSUBID == (item.ExamCode + item.SubjectCode) && a.PAPERCODE == item.PaperCode).ToList();
                    examiners = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID== (item.ExamCode + item.SubjectCode) && a.EMS_PAPER_CODE == item.PaperCode && a.EMS_ACTIVITY == "BEM" && a.EMS_MARKING_REG_CODE == item.RegionCode).ToListAsync();

                    newreport3.Region = item.RegionCode;
                    if (refcatpapers.Any())
                    {
                        newreport3.Card = "Yes";
                    }
                    if (material.Any())
                    {
                        newreport3.Material = "Yes";
                    }
                    if (examiners.Any())
                    {
                        newreport3.Examiners = examiners.Count().ToString();
                    }
                }
                else
                {
                    refcatpapers = await _context.REF_CAT_PAPER.Where(a => a.CTP_PPR_SUB_PAPER_CODE == (item.ExamCode + item.SubjectCode + item.PaperCode)).ToListAsync();
                    materials = material.Where(a =>
                       a.SUBSUBID == (item.ExamCode + item.SubjectCode) && a.PAPERCODE == item.PaperCode).ToList();
                    examiners = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID == (item.ExamCode + item.SubjectCode) && a.EMS_PAPER_CODE == item.PaperCode && a.EMS_ACTIVITY == "BEM").ToListAsync();

          
                    if (refcatpapers.Any())
                    {
                        newreport3.Card = "Yes";
                    }
                    if (material.Any())
                    {
                        newreport3.Material = "Yes";
                    }
                    if (examiners.Any())
                    {
                        newreport3.Examiners = examiners.Count().ToString();
                    }
                }

                ListData.Add(newreport3);
            }


            return Json(new
            {
                draw = request.Draw,
                recordsTotal = ListData.Count,
                recordsFiltered = ListData.Count,
                data = ListData
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetDataApportionmentStats2([FromBody] DataTablesRequest request)
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            // Access filter values from the request object
            var filterStatus = request.FilterStatus;

            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;

            // Query your data based on the filters


            var transaction = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_ACTIVITY == "BEM").ToListAsync();

            if (!string.IsNullOrEmpty(examCode))
            {

                transaction = transaction.Where(a => a.EMS_SUB_SUB_ID.StartsWith(examCode)).ToList();
            }

            if (!string.IsNullOrEmpty(subject))
            {

                transaction = transaction.Where(a => a.EMS_SUB_SUB_ID.Length >= 7 &&
                                                     a.EMS_SUB_SUB_ID.Substring(3, 4) == subject.Substring(3)).ToList();
            }

            if (!string.IsNullOrEmpty(paperCode))
            {

                transaction = transaction.Where(a => a.EMS_PAPER_CODE == paperCode).ToList();
            }

            if (!string.IsNullOrEmpty(regionCode))
            {

                transaction = transaction.Where(a => a.EMS_MARKING_REG_CODE == regionCode).ToList();
            }

            var allCount = transaction.Count();
            var pmsCount = transaction.Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PMS").Count();
            var rpmsCount = transaction.Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "RPMS").Count();
            var dpmsCount = transaction.Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "DPMS").Count();
            var bmsCount = transaction.Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "BMS").Count();
            var eCount = transaction.Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "E").Count();

            var reportList = new List<TransactionReport2>
            {

                new TransactionReport2 { Category = "PMS", Count = pmsCount.ToString() },
                new TransactionReport2 { Category = "RPMS", Count = rpmsCount.ToString() },
                new TransactionReport2 { Category = "DPMS", Count = dpmsCount.ToString() },
                new TransactionReport2 { Category = "BMS", Count = bmsCount.ToString() },
                new TransactionReport2 { Category = "E", Count = eCount.ToString() },
                new TransactionReport2 { Category = "Total", Count = allCount.ToString() },
            };

            return Json(new
            {
                draw = request.Draw,
                recordsTotal = reportList.Count,
                recordsFiltered = reportList.Count,
                data = reportList
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetDataApportionmentStats([FromBody] DataTablesRequest request)
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            // Access filter values from the request object
            var filterStatus = request.FilterStatus;

            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;

            // Query your data based on the filters


            var transaction = await _context.EXAMINER_TRANSACTIONS
                .Where(a => a.EMS_ACTIVITY == "BEM")
                .ToListAsync();

            if (!string.IsNullOrEmpty(examCode))
            {
                transaction = transaction.Where(a => a.EMS_SUB_SUB_ID.StartsWith(examCode)).ToList();
            }

            if (!string.IsNullOrEmpty(subject))
            {
                transaction = transaction.Where(a => a.EMS_SUB_SUB_ID.Length >= 7 &&
                                                     a.EMS_SUB_SUB_ID.Substring(3, 4) == subject.Substring(3, 4))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                transaction = transaction.Where(a => a.EMS_PAPER_CODE == paperCode).ToList();
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                transaction = transaction.Where(a => a.EMS_MARKING_REG_CODE == regionCode).ToList();
            }

            var reportList = transaction
                .GroupBy(t => new
                {
                    Subject = t.EMS_SUB_SUB_ID.Substring(3, 4), // Adjust as needed
                    Paper = t.EMS_PAPER_CODE,
                    Category = t.EMS_ECT_EXAMINER_CAT_CODE
                })
                .Select(g => new TransactionReport
                {
                    Subject = g.Key.Subject,
                    Paper = g.Key.Paper,
                    Category = g.Key.Category,
                    Total = g.Count().ToString()
                })
                .ToList();



            return Json(new
            {
                draw = request.Draw,
                recordsTotal = reportList.Count,
                recordsFiltered = reportList.Count,
                data = reportList
            });
        }


        [HttpPost]
        public async Task<IActionResult> GetReportData([FromBody] DataTablesRequest request)
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            // Access filter values from the request object
            var filterStatus = request.FilterStatus;

            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;

            // Query your data based on the filters

            var transaction = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_ACTIVITY == "BEM").ToListAsync();
            var refcat = await _context.REF_CAT_PAPER.ToListAsync();
            var apportiment = await _context.ExaminerApportionment.ToListAsync();
            var entries = await _context.EXM_SCRIPT_CAPTURED.ToListAsync();

            var missing = new List<MarksCaptured>();

            foreach (var item in entries)
            {
                var a = refcat.Where(a =>
                    a.CTP_PPR_SUB_PAPER_CODE == item.ExamCode + item.SubjectCode + item.PaperCode);

                if (!a.Any())
                {
                    missing.Add(item);
                }

            }

            var data = new List<TransReport>();
            foreach (var item in refcat)
            {
                var subsubid = item.CTP_PPR_SUB_PAPER_CODE.Substring(0, 7);
                var papercode = item.CTP_PPR_SUB_PAPER_CODE.Substring(7, 2);
                var Total = 0;
                var Org = 0;
                var App = 0;
                var Refa = 0;


                if (item.CTP_ECT_CAT_CODE == "PMS")
                {
                    Total = transaction.Where(a =>
                        a.EMS_SUB_SUB_ID == subsubid && a.EMS_PAPER_CODE == papercode &&
                        a.EMS_ECT_EXAMINER_CAT_CODE == "PMS" && a.EMS_ACTIVITY == "BEM").Count();
                    var pmsOrgg = apportiment.FirstOrDefault(a =>
                        a.sub_sub_id == subsubid && a.PaperCode == papercode && a.category == "PMS");



                    if (pmsOrgg != null)
                    {
                        App = pmsOrgg.TotalExaminers;

                    }

                    if (item.CTP_MAX_SCRIPTS != null)
                    {
                        Org = item.CTP_MAX_SCRIPTS.GetValueOrDefault();

                    }

                    var neww = new TransReport()
                    {
                        Subject = subsubid + "/" + papercode,
                        Cat = item.CTP_ECT_CAT_CODE,
                        Org = App.ToString(),
                        Trans = Total.ToString(),
                        Ref = Org.ToString(),
                        Status = "Pending"
                    };

                    if (neww.Org != "0" && neww.Trans != "0" && neww.Ref != null)
                    {
                        neww.Status = "Done";
                    }

                    data.Add(neww);
                }

                if (item.CTP_ECT_CAT_CODE == "RPMS")
                {
                    Total = transaction.Where(a =>
                        a.EMS_SUB_SUB_ID == subsubid && a.EMS_PAPER_CODE == papercode &&
                        a.EMS_ECT_EXAMINER_CAT_CODE == "RPMS" && a.EMS_ACTIVITY == "BEM").Count();
                    var pmsOrgg = apportiment.FirstOrDefault(a =>
                        a.sub_sub_id == subsubid && a.PaperCode == papercode && a.category == "RPMS");



                    if (pmsOrgg != null)
                    {
                        App = pmsOrgg.TotalExaminers;

                    }

                    if (item.CTP_MAX_SCRIPTS != null)
                    {
                        Org = item.CTP_MAX_SCRIPTS.GetValueOrDefault();

                    }

                    var neww = new TransReport()
                    {
                        Subject = subsubid + "/" + papercode,
                        Cat = item.CTP_ECT_CAT_CODE,
                        Org = App.ToString(),
                        Trans = Total.ToString(),
                        Ref = Org.ToString(),
                        Status = "Pending"
                    };

                    if (neww.Org != "0" && neww.Trans != "0" && neww.Ref != null)
                    {
                        neww.Status = "Done";
                    }

                    data.Add(neww);
                }

                if (item.CTP_ECT_CAT_CODE == "DPMS")
                {
                    Total = transaction.Where(a =>
                        a.EMS_SUB_SUB_ID == subsubid && a.EMS_PAPER_CODE == papercode &&
                        a.EMS_ECT_EXAMINER_CAT_CODE == "DPMS" && a.EMS_ACTIVITY == "BEM").Count();
                    var pmsOrgg = apportiment.FirstOrDefault(a =>
                        a.sub_sub_id == subsubid && a.PaperCode == papercode && a.category == "DPMS");



                    if (pmsOrgg != null)
                    {
                        App = pmsOrgg.TotalExaminers;

                    }

                    if (item.CTP_MAX_SCRIPTS != null)
                    {
                        Org = item.CTP_MAX_SCRIPTS.GetValueOrDefault();

                    }

                    var neww = new TransReport()
                    {
                        Subject = subsubid + "/" + papercode,
                        Cat = item.CTP_ECT_CAT_CODE,
                        Org = App.ToString(),
                        Trans = Total.ToString(),
                        Ref = Org.ToString(),
                        Status = "Pending"
                    };

                    if (neww.Org != "0" && neww.Trans != "0" && neww.Ref != null)
                    {
                        neww.Status = "Done";
                    }

                    data.Add(neww);
                }

                if (item.CTP_ECT_CAT_CODE == "BMS")
                {
                    Total = transaction.Where(a =>
                        a.EMS_SUB_SUB_ID == subsubid && a.EMS_PAPER_CODE == papercode &&
                        a.EMS_ECT_EXAMINER_CAT_CODE == "BMS" && a.EMS_ACTIVITY == "BEM").Count();
                    var pmsOrgg = apportiment.FirstOrDefault(a =>
                        a.sub_sub_id == subsubid && a.PaperCode == papercode && a.category == "BMS");



                    if (pmsOrgg != null)
                    {
                        App = pmsOrgg.TotalExaminers;

                    }

                    if (item.CTP_MAX_SCRIPTS != null)
                    {
                        Org = item.CTP_MAX_SCRIPTS.GetValueOrDefault();

                    }

                    var neww = new TransReport()
                    {
                        Subject = subsubid + "/" + papercode,
                        Cat = item.CTP_ECT_CAT_CODE,
                        Org = App.ToString(),
                        Trans = Total.ToString(),
                        Ref = Org.ToString(),
                        Status = "Pending"
                    };

                    if (neww.Org != "0" && neww.Trans != "0" && neww.Ref != null)
                    {
                        neww.Status = "Done";
                    }

                    data.Add(neww);
                }


            }


            foreach (var item in data)
            {

                var subjectc = item.Subject;

                string subsubid = subjectc.Length >= 7 ? subjectc.Substring(0, 7) : null;

                string papercod = null;
                var parts = subjectc.Split('/');
                if (parts.Length == 2)
                {
                    papercod = parts[1]; // "02"
                }

                var Total = 0;
                var App = 0;
                Total = transaction.Where(a =>
                    a.EMS_SUB_SUB_ID == subsubid && a.EMS_PAPER_CODE == papercod &&
                    a.EMS_ECT_EXAMINER_CAT_CODE == "E" && a.EMS_ACTIVITY == "BEM").Count();
                var pmsOrgg = apportiment.FirstOrDefault(a =>
                    a.sub_sub_id == subsubid && a.PaperCode == papercod && a.category == "E");



                if (pmsOrgg != null)
                {
                    App = pmsOrgg.TotalExaminers;

                }

                var neww = new TransReport()
                {
                    Subject = subsubid + "/" + papercod,
                    Cat = "E",
                    Org = App.ToString(),
                    Trans = Total.ToString(),
                    Ref = "N/A",
                    Status = "Pending"
                };

                if (neww.Org != "0" && neww.Trans != "0")
                {
                    neww.Status = "Done";
                }

            }

            foreach (var item in missing)
            {
                var newt = new TransReport()
                {
                    Subject = item.ExamCode + item.SubjectCode + "/" + item.PaperCode,
                    Cat = "No",
                    Org = "No",
                    Trans = "No",
                    Ref = "No",
                    Status = "Pending"
                };

                data.Add(newt);
            }


            if (!string.IsNullOrEmpty(examCode))
            {
                data = data.Where(a => a.Subject.StartsWith(examCode)).ToList();
            }

            if (!string.IsNullOrEmpty(subject))
            {
                data = data.Where(a =>
                    !string.IsNullOrEmpty(a.Subject) &&
                    a.Subject.Length >= 7 &&
                    a.Subject.Substring(0, 7) == subject
                ).ToList();
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                data = data.Where(a =>
                {
                    if (string.IsNullOrEmpty(a.Subject)) return false;
                    var parts = a.Subject.Split('/');
                    return parts.Length == 2 && parts[1] == paperCode;
                }).ToList();
            }


            if (!string.IsNullOrEmpty(filterStatus))
            {
                if (filterStatus == "Pending")
                {
                    data = data.Where(a => a.Status == "Pending").ToList();
                }

                if (filterStatus == "Done")
                {
                    data = data.Where(a => a.Status == "Done").ToList();
                }
            }

            return Json(new
            {
                draw = request.Draw,
                recordsTotal = data.Count(),
                recordsFiltered = data.Count(),
                data
            });
        }

        public async Task<IActionResult> Index(string examCode, string subject, string paperCode, string regionCode)
        {
            var examinerTeams = await _repository.GetComponentTeamsAsync(examCode, subject, paperCode, regionCode);
            var viewModel = new ExaminerTeamsViewModel { ExaminerTeams = examinerTeams };
            return View(viewModel);
        }

        public async Task<IActionResult> MarkingMaterial(string examCode, string subjectCode, string paperCode,
            string regionCode, string activity)
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) &&
                !string.IsNullOrEmpty(paperCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,
                    SubjectCode = subjectCode,
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

            var checkinoutDates =new List<CategoryCheckInCheckOut>();
            var existingMaterials = new List<MaterialTransaction>();
            var model = new CombinedMaterialCategoryViewModel();
            if (!string.IsNullOrEmpty(regionCode))
            {
                // Get existing data
                checkinoutDates = await _context.CATEGORYCHECKINCHECKOUT
                .Where(a => a.SubSubId == userSession.ExamCode + userSession.SubjectCode && a.PaperCode == userSession.PaperCode && a.REGION == userSession.RegionCode)
                .ToListAsync();

                existingMaterials = await _context.MaterialTransaction
                   .Where(a => a.SUBSUBID == userSession.ExamCode + userSession.SubjectCode && a.PAPERCODE == userSession.PaperCode && a.Region == userSession.RegionCode)
                   .ToListAsync();

                model = new CombinedMaterialCategoryViewModel
                {
                    ExamCode = userSession.ExamCode,
                    SubjectCode = userSession.SubjectCode,
                    PaperCode = userSession.PaperCode,
                    Region = userSession.RegionCode,

                    CategoryDates = checkinoutDates.Select(c => new CategoryDateViewModel
                    {
                        Category = c.Category,
                        CheckIn = c.CheckIn,
                        CheckOut = c.CheckOut
                    }).ToList(),
                    Materials = existingMaterials.Select(m => new MaterialTransactionViewModel
                    {
                        MaterialName = m.ITEM,
                        Quantity = m.QUANTITY
                    }).ToList()
                };
            }
            else
            {
                // Get existing data
                checkinoutDates = await _context.CATEGORYCHECKINCHECKOUT
           .Where(a => a.SubSubId == userSession.ExamCode + userSession.SubjectCode && a.PaperCode == userSession.PaperCode )
           .ToListAsync();

                existingMaterials = await _context.MaterialTransaction
                   .Where(a => a.SUBSUBID == userSession.ExamCode + userSession.SubjectCode && a.PAPERCODE == userSession.PaperCode)
                   .ToListAsync(); ;

                model = new CombinedMaterialCategoryViewModel
                {
                    ExamCode = userSession.ExamCode,
                    SubjectCode = userSession.SubjectCode,
                    PaperCode = userSession.PaperCode,

                    CategoryDates = checkinoutDates.Select(c => new CategoryDateViewModel
                    {
                        Category = c.Category,
                        CheckIn = c.CheckIn,
                        CheckOut = c.CheckOut
                    }).ToList(),
                    Materials = existingMaterials.Select(m => new MaterialTransactionViewModel
                    {
                        MaterialName = m.ITEM,
                        Quantity = m.QUANTITY
                    }).ToList()
                };
            }
             

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning", // Changed to warning icon
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

            // Retrieve materials for the dropdown
            var materials = await _materialRepository.GetAllAsync();
            ViewBag.Materials = new SelectList(materials, "Name", "Name");
            ViewBag.ExamCode = userSession.ExamCode;
            ViewBag.SubjectCode = userSession.SubjectCode;
            ViewBag.PaperCode = userSession.PaperCode;
            ViewBag.Activity = userSession.Activity;

            ViewBag.RegionCode = string.IsNullOrEmpty(regionCode) ? "" : userSession.RegionCode;


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddMaterialTransactions(List<MaterialTransactionViewModel> transactions)
        {
            if (ModelState.IsValid)
            {
                // Process transactions (e.g., save to database)
                // Example: _context.MaterialTransactions.AddRange(transactions);
                // await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View("Index");
        }

        public async Task<IActionResult> ExaminerApportionment(string examCode, string subjectCode, string paperCode,
            string regionCode, string activity)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) &&
                !string.IsNullOrEmpty(paperCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,
                    SubjectCode = subjectCode,
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

            var dataForScripts = await _repository.GetComponentMarkCaptured(userSession.ExamCode,
                userSession.SubjectCode, userSession.PaperCode, userSession.RegionCode);
            int apportionedEntries = 0;
            int allowedExaminers = 0;
            if (dataForScripts != null)
            {
                apportionedEntries = dataForScripts.ApportionedScripts;
                allowedExaminers = apportionedEntries / 400;
            }

            var Apportion = new ExaminerApportionmentViewModel();

            var existingApportionedScripts =
                await _repository.GetApportionedExaminersAsync(examCode, subjectCode, paperCode, regionCode, activity);

            if (existingApportionedScripts != null)
            {
                Apportion = new ExaminerApportionmentViewModel
                {
                    TotalEntries = apportionedEntries,
                    AllowedExaminers = allowedExaminers,

                };


                foreach (var script in existingApportionedScripts)
                {
                    switch (script.category)
                    {
                        case "PMS":
                            Apportion.TotalPMS = script.TotalExaminers;
                            Apportion.SharePMS = script.ScriptPerExaminer;
                            break;
                        case "RPMS":
                            Apportion.TotalRPMS = script.TotalExaminers;
                            Apportion.ShareRPMS = script.ScriptPerExaminer;
                            break;
                        case "DPMS":
                            Apportion.TotalDPMS = script.TotalExaminers;
                            Apportion.ShareDPMS = script.ScriptPerExaminer;
                            break;
                        case "BMS":
                            Apportion.TotalBMS = script.TotalExaminers;
                            Apportion.ShareBMS = script.ScriptPerExaminer;
                            break;
                        case "E":
                            Apportion.TotalE = script.TotalExaminers;
                            Apportion.ShareE = script.ScriptPerExaminer;
                            break;
                    }

                    Apportion.TotalShare += script.ScriptPerExaminer;

                    Apportion.ScriptsToExaminers = script.ScriptAToExaminerX;
                }

                var ApportionedScripts =
                    await _repository.GetApportionedScriptsAsync(examCode, subjectCode, paperCode, regionCode,
                        activity);

                if (ApportionedScripts != null)
                {
                    foreach (var script in ApportionedScripts)
                    {
                        switch (script.CTP_ECT_CAT_CODE)
                        {
                            case "PMS":
                                Apportion.FinalSharePMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                                break;
                            case "RPMS":
                                Apportion.FinalShareRPMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                                break;
                            case "DPMS":
                                Apportion.FinalShareDPMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                                break;
                            case "BMS":
                                Apportion.FinalShareBMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                                break;
                            case "E":
                                Apportion.FinalShareE = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                                break;
                        }

                        Apportion.FinalTotalShare += script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                    }

                }

                Apportion.TotalScriptsPMS = Apportion.FinalSharePMS * Apportion.TotalPMS;
                Apportion.TotalScriptsRPMS = Apportion.FinalShareRPMS * Apportion.TotalRPMS;
                Apportion.TotalScriptsDPMS = Apportion.FinalShareDPMS * Apportion.TotalDPMS;
                Apportion.TotalScriptsBMS = Apportion.FinalShareBMS * Apportion.TotalBMS;
                Apportion.TotalScripts = Apportion.TotalScriptsPMS + Apportion.TotalScriptsRPMS +
                                         Apportion.TotalScriptsDPMS + Apportion.TotalScriptsBMS +
                                         Apportion.TotalScriptsE;

            }

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning", // Changed to warning icon
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
            ViewBag.Activity = userSession.Activity;

            ViewBag.RegionCode = string.IsNullOrEmpty(regionCode) ? "" : userSession.RegionCode;

            return View(Apportion);
        }


        public async Task<IActionResult> ExaminerApportionment2(string examCode, string subjectCode, string paperCode,
            string regionCode, string activity)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) &&
                !string.IsNullOrEmpty(paperCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,
                    SubjectCode = subjectCode,
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

            var dataForScripts = await _repository.GetComponentMarkCaptured(userSession.ExamCode,
                userSession.SubjectCode, userSession.PaperCode, userSession.RegionCode);
            int apportionedEntries = 0;
            int allowedExaminers = 0;
            if (dataForScripts != null)
            {
                apportionedEntries = dataForScripts.ApportionedScripts;
                allowedExaminers = apportionedEntries / 400;
            }

            var Apportion = new ExaminerApportionmentViewModel();

            var existingApportionedScripts =
                await _repository.GetApportionedExaminersAsync(examCode, subjectCode, paperCode, regionCode, activity);

            if (existingApportionedScripts != null)
            {
                Apportion = new ExaminerApportionmentViewModel
                {
                    TotalEntries = apportionedEntries,
                    AllowedExaminers = allowedExaminers,

                };


                foreach (var script in existingApportionedScripts)
                {
                    switch (script.category)
                    {
                        case "PMS":
                            Apportion.TotalPMS = script.TotalExaminers;
                            Apportion.SharePMS = script.ScriptPerExaminer;
                            break;
                        case "RPMS":
                            Apportion.TotalRPMS = script.TotalExaminers;
                            Apportion.ShareRPMS = script.ScriptPerExaminer;
                            break;
                        case "DPMS":
                            Apportion.TotalDPMS = script.TotalExaminers;
                            Apportion.ShareDPMS = script.ScriptPerExaminer;
                            break;
                        case "BMS":
                            Apportion.TotalBMS = script.TotalExaminers;
                            Apportion.ShareBMS = script.ScriptPerExaminer;
                            break;
                        case "E":
                            Apportion.TotalE = script.TotalExaminers;
                            Apportion.ShareE = script.ScriptPerExaminer;
                            break;
                    }

                    Apportion.TotalShare += script.ScriptPerExaminer;

                    Apportion.ScriptsToExaminers = script.ScriptAToExaminerX;
                }

                var ApportionedScripts =
                    await _repository.GetApportionedScriptsAsync(examCode, subjectCode, paperCode, regionCode,
                        activity);

                if (ApportionedScripts != null)
                {
                    foreach (var script in ApportionedScripts)
                    {
                        switch (script.CTP_ECT_CAT_CODE)
                        {
                            case "PMS":
                                Apportion.FinalSharePMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                                break;
                            case "RPMS":
                                Apportion.FinalShareRPMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                                break;
                            case "DPMS":
                                Apportion.FinalShareDPMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                                break;
                            case "BMS":
                                Apportion.FinalShareBMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                                break;
                            case "E":
                                Apportion.FinalShareE = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                                break;
                        }

                        Apportion.FinalTotalShare += script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                    }

                }

                Apportion.TotalScriptsPMS = Apportion.FinalSharePMS * Apportion.TotalPMS;
                Apportion.TotalScriptsRPMS = Apportion.FinalShareRPMS * Apportion.TotalRPMS;
                Apportion.TotalScriptsDPMS = Apportion.FinalShareDPMS * Apportion.TotalDPMS;
                Apportion.TotalScriptsBMS = Apportion.FinalShareBMS * Apportion.TotalBMS;
                Apportion.TotalScripts = Apportion.TotalScriptsPMS + Apportion.TotalScriptsRPMS +
                                         Apportion.TotalScriptsDPMS + Apportion.TotalScriptsBMS +
                                         Apportion.TotalScriptsE;

            }

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning", // Changed to warning icon
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
            ViewBag.Activity = userSession.Activity;

            ViewBag.RegionCode = string.IsNullOrEmpty(regionCode) ? "" : userSession.RegionCode;

            return View(Apportion);
        }

        public async Task<IActionResult> ApportionScripts(string examCode = "", string subjectCode = "",
            string paperCode = "", string regionCode = "", string activity = "")
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) &&
                !string.IsNullOrEmpty(paperCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,
                    SubjectCode = subjectCode,
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

            var dataForScripts = await _repository.GetComponentMarkCaptured(userSession.ExamCode,
                userSession.SubjectCode, userSession.PaperCode, userSession.RegionCode);
            int apportionedEntries = 0;
            int allowedExaminers = 0;
            if (dataForScripts != null)
            {
                apportionedEntries = dataForScripts.ApportionedScripts;
                allowedExaminers = apportionedEntries / 400;
            }

            var selectedInExaminers =
                await _repository.GetSelectedTeamsFromTransactionAsync(examCode, subjectCode, paperCode, regionCode,
                    activity);
            var pmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PMS");
            var rpmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "RPMS");
            var bmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "BMS");
            var aeExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "E");
            var dpmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "DPMS");

            var existingApportionedScripts =
                await _repository.GetApportionedScriptsAsync(examCode, subjectCode, paperCode, regionCode, activity);


            var Apportion = new ApportionScriptsViewModel
            {
                SelectedPMS = pmsExaminers,
                SelectedDPMS = dpmsExaminers,
                SelectedBMS = bmsExaminers,
                SelectedE = aeExaminers,
                SelectedRPMS = rpmsExaminers,
                TotalEntries = apportionedEntries,
                AllowedExaminers = allowedExaminers,
            };


            foreach (var script in existingApportionedScripts)
            {
                switch (script.CTP_ECT_CAT_CODE)
                {
                    case "PMS":
                        Apportion.MaxScriptsPMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                        break;
                    case "RPMS":
                        Apportion.MaxScriptsRPMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                        break;
                    case "DPMS":
                        Apportion.MaxScriptsDPMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                        break;
                    case "BMS":
                        Apportion.MaxScriptsBMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                        break;
                    case "E":
                        Apportion.MaxScriptsE = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                        break;
                }
            }


            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning", // Changed to warning icon
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
            ViewBag.Activity = userSession.Activity;

            ViewBag.RegionCode = string.IsNullOrEmpty(regionCode) ? "" : userSession.RegionCode;

            return View(Apportion);

        }

        public async Task<IActionResult> SummaryApportionScripts(string examCode = "", string subjectCode = "",
            string paperCode = "", string regionCode = "", string activity = "")
        {

            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) &&
                !string.IsNullOrEmpty(paperCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,
                    SubjectCode = subjectCode,
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

            var dataForScripts = await _repository.GetComponentMarkCaptured(userSession.ExamCode,
                userSession.SubjectCode, userSession.PaperCode, userSession.RegionCode);
            int apportionedEntries = 0;
            int allowedExaminers = 0;
            if (dataForScripts != null)
            {
                apportionedEntries = dataForScripts.ApportionedScripts;
                allowedExaminers = apportionedEntries / 400;
            }

            var apportionedScripts = await _repository.GetApportionedScriptsAsync(userSession.ExamCode,
                userSession.SubjectCode, userSession.PaperCode, userSession.RegionCode);

            var existingApportionedScripts = await _repository.GetApportionedScriptsAsync(userSession.ExamCode,
                userSession.SubjectCode, userSession.PaperCode, userSession.RegionCode, activity);

            //            // Initialize all max script values to 0 by default
            //            var maxScripts = new Dictionary<string, int>
            //{
            //    { "BMS", 0 },
            //    { "DPMS", 0 },
            //    { "E", 0 },
            //    { "PMS", 0 },
            //    { "RPMS", 0 }
            //};

            //            // Check if the apportionedScripts collection is not null
            //            if (apportionedScripts != null)
            //            {
            //                foreach (var script in apportionedScripts)
            //                {
            //                    // If the category code exists in the dictionary, update its value
            //                    if (maxScripts.ContainsKey(script.CTP_ECT_CAT_CODE))
            //                    {
            //                        maxScripts[script.CTP_ECT_CAT_CODE] = script.CTP_MAX_SCRIPTS.GetValueOrDefault()    ;
            //                    }
            //                }
            //            }

            //            // Now you can access the max scripts by category code from the dictionary
            //            var maxScriptsBms = maxScripts["BMS"];
            //            var maxScriptsPms = maxScripts["PMS"];
            //            var maxScriptsRPMS = maxScripts["RPMS"];
            //            var maxScriptsDPMS = maxScripts["DPMS"];
            //            var maxScriptsE = maxScripts["E"];

            var selectedInExaminers =
                await _repository.GetFromMasterAsync(examCode, subjectCode, paperCode, regionCode, activity);
            var pmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PMS");
            var rpmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "RPMS");
            var bmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "BMS");
            var aeExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "E");
            var dpmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "DPMS");


            var selectedInTransaction =
                await _repository.GetSelectedTeamsFromTransactionAsync(examCode, subjectCode, paperCode, regionCode,
                    activity);
            var pmsTransaction = selectedInTransaction.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PMS");
            var rpmsTransaction = selectedInTransaction.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "RPMS");
            var bmsTransaction = selectedInTransaction.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "BMS");
            var aeTransaction = selectedInTransaction.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "E");
            var dpmsTransaction = selectedInTransaction.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "DPMS");




            var Apportion = new SummaryScriptApportionmentViewModel()
            {
                ExaminersAvailableBMS = bmsExaminers,
                ExaminersAvailableDPMS = dpmsExaminers,
                ExaminersAvailableE = aeExaminers,
                ExaminersAvailablePMS = pmsExaminers,
                ExaminersAvailableRPMS = rpmsExaminers,

                ExaminersChosenBMS = bmsTransaction,
                ExaminersChosenDPMS = dpmsTransaction,
                ExaminersChosenE = aeTransaction,
                ExaminersChosenPMS = pmsTransaction,
                ExaminersChosenRPMS = rpmsTransaction,

                TotalEntries = apportionedEntries,
                AllowedExaminers = allowedExaminers,




            };


            foreach (var script in existingApportionedScripts)
            {
                switch (script.CTP_ECT_CAT_CODE)
                {
                    case "PMS":
                        Apportion.MaxScriptsPMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                        break;
                    case "RPMS":
                        Apportion.MaxScriptsRPMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                        break;
                    case "DPMS":
                        Apportion.MaxScriptsDPMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                        break;
                    case "BMS":
                        Apportion.MaxScriptsBMS = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                        break;
                    case "E":
                        Apportion.MaxScriptsE = script.CTP_MAX_SCRIPTS.GetValueOrDefault();
                        break;
                }
            }

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning", // Changed to warning icon
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
            ViewBag.Activity = userSession.Activity;

            ViewBag.RegionCode = string.IsNullOrEmpty(regionCode) ? "" : userSession.RegionCode;

            return View(Apportion);
        }



        public async Task<IActionResult> SelectTeam(string examCode = "", string subjectCode = "",
            string paperCode = "", string regionCode = "", string activity = "")
        {
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) &&
                !string.IsNullOrEmpty(paperCode))
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

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning", // Changed to warning icon
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
            ViewBag.Activity = userSession.Activity;

            ViewBag.RegionCode =  userSession.RegionCode;

            var examiners = new List<Examiner>();
            var selectedexaminers = new List<ExaminerScriptsMarked>();
            var orgCard = new List<ExaminerApportionment>();

            if (!string.IsNullOrEmpty(userSession.RegionCode))
            {
                examiners = await _context.EXM_EXAMINER_MASTER
                    .Where(a => a.EMS_SUB_SUB_ID == userSession.SubjectCode
                                && a.EMS_PAPER_CODE == userSession.PaperCode &&
                                a.EMS_MARKING_REG_CODE == userSession.RegionCode)
                    .Include(a => a.ExaminerScriptsMarkeds)
                    .Where(a => a.ExaminerScriptsMarkeds == null ||
                                !a.ExaminerScriptsMarkeds.Any(e =>
                                    e.EMS_ACTIVITY == "BEM" &&
                                    e.EMS_SUB_SUB_ID.StartsWith(userSession.ExamCode)))
                    .ToListAsync();




                selectedexaminers = await _context.EXAMINER_TRANSACTIONS.Where(a =>
                    a.EMS_SUB_SUB_ID == userSession.ExamCode + userSession.SubjectCode &&
                    a.EMS_PAPER_CODE == userSession.PaperCode && a.EMS_ACTIVITY == "BEM" &&
                    a.EMS_MARKING_REG_CODE == userSession.RegionCode).Include(a => a.Examiner).ToListAsync();

                orgCard = await _context.ExaminerApportionment.Where(a =>
                    a.sub_sub_id == userSession.ExamCode + userSession.SubjectCode &&
                    a.PaperCode == userSession.PaperCode).ToListAsync();
            }
            else
            {
                examiners = await _context.EXM_EXAMINER_MASTER
                    .Where(a => a.EMS_SUB_SUB_ID == userSession.SubjectCode
                                && a.EMS_PAPER_CODE == userSession.PaperCode)
                    .Include(a => a.ExaminerScriptsMarkeds)
                    .Where(a => a.ExaminerScriptsMarkeds == null ||
                                !a.ExaminerScriptsMarkeds.Any(e =>
                                    e.EMS_ACTIVITY == "BEM" &&
                                    e.EMS_SUB_SUB_ID.StartsWith(userSession.ExamCode)))
                    .ToListAsync();




                selectedexaminers = await _context.EXAMINER_TRANSACTIONS
                    .Where(a => a.EMS_SUB_SUB_ID == userSession.ExamCode + userSession.SubjectCode &&
                                a.EMS_PAPER_CODE == userSession.PaperCode && a.EMS_ACTIVITY == "BEM")
                    .Include(a => a.Examiner).ToListAsync();

                orgCard = await _context.ExaminerApportionment.Where(a =>
                    a.sub_sub_id == userSession.ExamCode + userSession.SubjectCode &&
                    a.PaperCode == userSession.PaperCode).ToListAsync();
            }



            var teams = await _repository.GetSuperordsBySubSubIdAndPaperCodeAsync(
                userSession.ExamCode,
                userSession.SubjectCode,
                userSession.PaperCode,
                userSession.Activity,
                userSession.RegionCode);


            if (!string.IsNullOrEmpty(regionCode))
            {
                if (teams == null || !teams.Any())
                {
                    // Parse region code (01-10) and calculate default team number
                    int regionNumber = int.TryParse(regionCode, out int num) ? num : 1;
                    regionNumber = Math.Clamp(regionNumber, 1, 10); // Ensure between 1-10

                    // Calculate default team number (1001 for 01, 2001 for 02,...10001 for 10)
                    int defaultTeamNumber = regionNumber * 1000 + 1;

                    teams = new List<string> { defaultTeamNumber.ToString() };
                }
            }
            else
            {
                // Check if null or empty, and add "1001"
                if (teams == null || !teams.Any())
                {
                    teams = new List<string> { "1001" }; // Use string if the list is of strings
                }
            }
               





            ViewBag.Teamss = teams.Select(t => new SelectListItem
            {
                Value = t,
                Text = t
            }).ToList();

            ViewBag.OrgCard = orgCard.Select(t => new ExaminerApportionment
            {
                category = t.category,
                TotalExaminers = t.TotalExaminers
            });


            ViewBag.AllExaminers = examiners.Select(t => new ExaminerSelect
            {
                Name = t.EMS_EXAMINER_NAME + " " + t.EMS_LAST_NAME,
                IdNumber = t.EMS_NATIONAL_ID,


            }).ToList();

            ViewBag.SelectedExaminers = selectedexaminers.Select(t => new ExaminerSelect
            {
                Name = t.Examiner.EMS_EXAMINER_NAME + " " + t.Examiner.EMS_LAST_NAME,
                IdNumber = t.EMS_NATIONAL_ID,
                Team = t.EMS_EXM_SUPERORD,
                ExaminerNumber = t.EMS_EXAMINER_NUMBER,
                Capturing = t.EMS_CAPTURINGROLE,
                Region = t.EMS_MARKING_REG_CODE,
                Status = t.EMS_ECT_EXAMINER_CAT_CODE,


            }).ToList();

            ViewBag.Teams = teams;
            return View();
        }

        public async Task<IActionResult> SelectTeam2(string examCode = "", string subjectCode = "",
            string paperCode = "", string regionCode = "", string activity = "")
        {
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subjectCode) &&
                !string.IsNullOrEmpty(paperCode))
            {
                userSession = new SessionModel()
                {
                    ExamCode = examCode,
                    SubjectCode = subjectCode,
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

            if (userSession == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning", // Changed to warning icon
                    title = "Session Expired",
                    text = " You have been logged out for security reasons login again.",
                    showConfirmButton = true,
                    confirmButtonColor = "#ffc107", // Warning color

                    timerProgressBar = true
                });



                await _signInManager.SignOutAsync();
                return Redirect("/Identity/Account/Login");
            }

            ViewBag.ExamCode = userSession.ExamCode;
            ViewBag.SubjectCode = userSession.SubjectCode;
            ViewBag.PaperCode = userSession.PaperCode;
            ViewBag.Activity = userSession.Activity;

            ViewBag.RegionCode = string.IsNullOrEmpty(regionCode) ? "" : userSession.RegionCode;

            var teams = await _repository.GetSuperordsBySubSubIdAndPaperCodeAsync(userSession.ExamCode,
                userSession.SubjectCode, userSession.PaperCode, userSession.Activity, userSession.RegionCode);
            ViewBag.Teams = teams;
            return View();
        }

        [HttpPost]
        public IActionResult SaveMember([FromBody] Team teamMember)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool result = /* _teamRepository.SaveTeamMember(teamMember);*/ true;

                if (result)
                {
                    return Ok(new { success = true, message = "Team member saved successfully" });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Error saving team member" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CalculateFomulars([FromBody] ExaminerApportionmentViewModel values)
        {
            try
            {
                double x = 0;
                double A = 0.0;

                if (values.TotalPMS == 0)
                {
                    return BadRequest(new { success = false, message = "Please select at least one PMS option." });
                }

                if (values.TotalPMS > 0 && values.TotalRPMS > 0 && values.TotalDPMS > 0 && values.TotalBMS > 0 &&
                    values.TotalE > 0)
                {
                    A = values.TotalEntries / (0.1 + 0.125 * values.TotalRPMS + 0.15 * values.TotalDPMS +
                                               0.2 * values.TotalBMS + values.TotalE);
                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS > 0 && values.TotalBMS > 0 &&
                         values.TotalE > 0)
                {
                    A = values.TotalEntries / (0.1 + 0.15 * values.TotalDPMS + 0.2 * values.TotalBMS + values.TotalE);
                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 && values.TotalBMS > 0 &&
                         values.TotalE > 0)
                {
                    A = values.TotalEntries / (0.15 + 0.2 * values.TotalBMS + values.TotalE);
                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 &&
                         values.TotalBMS == 0 && values.TotalE > 0)
                {
                    A = values.TotalEntries / (0.2 + values.TotalE);
                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 &&
                         values.TotalBMS == 0 && values.TotalE == 0)
                {
                    A = values.TotalEntries;
                }

                x = A;
                var result = new ScriptCalculationResult();
                if (values.TotalPMS > 0 && values.TotalRPMS > 0 && values.TotalDPMS > 0 && values.TotalBMS > 0 &&
                    values.TotalE > 0)
                {
                    //if(values.SharePMS > 0 && values.ShareDPMS > 0 && values.TotalRPMS > 0 && values.TotalBMS > 0 && values.ShareE > 0)
                    //{
                    //    result = new ScriptCalculationResult
                    //    {
                    //        SharePMS = values.SharePMS,
                    //        ShareRPMS = values.ShareRPMS,
                    //        ShareDPMS = values.ShareDPMS,
                    //        ShareBMS = values.ShareBMS,
                    //        ShareE = values.ShareE,
                    //        ScriptsToExaminers = (int)x,

                    //        Success = true
                    //    };
                    //}
                    //else
                    //{
                    double exactSharePMS = 0.1 * x;
                    double exactShareRPMS = 0.125 * x;
                    double exactShareDPMS = 0.15 * x;
                    double exactShareBMS = 0.2 * x;
                    double exactShareE = 1.0 * x;

                    result = new ScriptCalculationResult
                    {
                        SharePMS = (int)exactSharePMS,
                        ShareRPMS = (int)exactShareRPMS,
                        ShareDPMS = (int)exactShareDPMS,
                        ShareBMS = (int)exactShareBMS,
                        ShareE = (int)exactShareE,
                        ScriptsToExaminers = (int)x,

                        Success = true
                    };
                    //}



                    //if (result.SharePMS < 50 && values.TotalEntries > 50)
                    //{
                    //    var diffToFifty = result.SharePMS - 50;
                    //    var totalShareToE = result.ShareE - diffToFifty;
                    //    result.ShareE = totalShareToE;
                    //    result.SharePMS = 50;
                    //}


                    //if (result.ShareRPMS < 60 && values.TotalEntries > 60)
                    //{
                    //    var diffToFifty = result.ShareRPMS - 60;
                    //    var totalShareToE = result.ShareE - diffToFifty;
                    //    result.ShareE = totalShareToE;
                    //    result.ShareRPMS = 60;
                    //}

                    //if (result.ShareDPMS < 60 && values.TotalEntries > 60)
                    //{
                    //    var diffToFifty = result.ShareDPMS - 60;
                    //    var totalShareToE = result.ShareE - diffToFifty;
                    //    result.ShareE = totalShareToE;
                    //    result.ShareDPMS = 60;
                    //}

                    //if (result.ShareBMS < 70 && values.TotalEntries > 70)
                    //{
                    //    var diffToFifty = result.ShareBMS - 70;
                    //    var totalShareToE = result.ShareE - diffToFifty;
                    //    result.ShareE = totalShareToE;
                    //    result.ShareBMS = 70;
                    //}

                    var totalToRPMS = values.TotalRPMS * result.SharePMS;
                    var shareToPMS = totalToRPMS + result.SharePMS;



                    var totalToDPMS = values.TotalDPMS * result.ShareDPMS;
                    var shareToRPMS = totalToDPMS + result.ShareRPMS;



                    var totalToBMS = values.TotalBMS * result.ShareBMS;
                    var shareToDPMS = totalToBMS / values.TotalDPMS;

                    var totalToE = values.TotalE * result.ShareE;
                    var shareToBMS = totalToE / values.TotalBMS;

                    result.FinalSharePMS = shareToPMS;
                    result.FinalShareRPMS = shareToRPMS;
                    result.FinalShareDPMS = shareToDPMS;
                    result.FinalShareBMS = shareToBMS;



                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS > 0 && values.TotalBMS > 0 &&
                         values.TotalE > 0)
                {

                    double exactSharePMS = 0.1 * x;
                    double exactShareRPMS = 0.125 * x;
                    double exactShareDPMS = 0.15 * x;
                    double exactShareBMS = 0.2 * x;
                    double exactShareE = 1.0 * x;

                    result = new ScriptCalculationResult
                    {
                        SharePMS = (int)exactSharePMS,
                        ShareRPMS = 0,
                        ShareDPMS = (int)exactShareDPMS,
                        ShareBMS = (int)exactShareBMS,
                        ShareE = (int)exactShareE,
                        ScriptsToExaminers = (int)x,

                        Success = true
                    };







                    //if (result.SharePMS < 50 && values.TotalEntries > 50)
                    //{
                    //    var diffToFifty = 50 - result.SharePMS;
                    //    var totalShareToE = result.ShareE - diffToFifty;
                    //    result.ShareE = totalShareToE;
                    //    result.SharePMS = 50;
                    //}

                    //if (result.ShareDPMS < 60 && values.TotalEntries > 60)
                    //{
                    //    var diffToFifty = 60 - result.ShareDPMS;
                    //    var totalShareToE = result.ShareE - diffToFifty;
                    //    result.ShareE = totalShareToE;
                    //    result.ShareDPMS = 60;
                    //}

                    //if (result.ShareBMS < 70 && values.TotalEntries > 70)
                    //{
                    //    var diffToFifty = 70 - result.ShareBMS ;
                    //    var totalShareToE = result.ShareE - diffToFifty;
                    //    result.ShareE = totalShareToE;
                    //    result.ShareBMS = 70;
                    //}



                    var totalToDPMS = values.TotalDPMS * result.ShareDPMS;
                    var shareToPMS = totalToDPMS + result.SharePMS;


                    var totalToBMS = values.TotalBMS * result.ShareBMS;
                    var shareToDPMS = totalToBMS / values.TotalDPMS;

                    var totalToE = values.TotalE * result.ShareE;
                    var shareToBMS = totalToE / values.TotalBMS;

                    result.FinalSharePMS = shareToPMS;
                    result.FinalShareRPMS = 0;
                    result.FinalShareDPMS = shareToDPMS;
                    result.FinalShareBMS = shareToBMS;



                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 && values.TotalBMS > 0 &&
                         values.TotalE > 0)
                {

                    double exactSharePMS = 0.15 * x;
                    double exactShareRPMS = 0;
                    double exactShareDPMS = 0;
                    double exactShareBMS = 0.2 * x;
                    double exactShareE = 1.0 * x;

                    result = new ScriptCalculationResult
                    {
                        SharePMS = (int)exactSharePMS,
                        ShareRPMS = 0,
                        ShareDPMS = 0,
                        ShareBMS = (int)exactShareBMS,
                        ShareE = (int)exactShareE,
                        ScriptsToExaminers = (int)x,

                        Success = true
                    };

                    //if (result.SharePMS < 50 && values.TotalEntries > 50)
                    //{
                    //    var diffToFifty = result.SharePMS - 50;
                    //    var totalShareToE = result.ShareE - diffToFifty;
                    //    result.ShareE = totalShareToE;
                    //    result.SharePMS = 50;
                    //}

                    //if (result.ShareBMS < 60 && values.TotalEntries > 60)
                    //{
                    //    var diffToFifty = result.ShareBMS - 60;
                    //    var totalShareToE = result.ShareE - diffToFifty;
                    //    result.ShareE = totalShareToE;
                    //    result.ShareBMS = 60;
                    //}

                    var totalToBMS = values.TotalBMS * result.ShareBMS;
                    var shareToPMS = totalToBMS + result.SharePMS;




                    var totalToE = values.TotalE * result.ShareE;
                    var shareToBMS = totalToE / values.TotalBMS;

                    result.FinalSharePMS = shareToPMS;
                    result.FinalShareRPMS = 0;
                    result.FinalShareDPMS = 0;
                    result.FinalShareBMS = shareToBMS;


                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 &&
                         values.TotalBMS == 0 && values.TotalE > 0)
                {

                    double exactSharePMS = 0.2 * x;
                    double exactShareRPMS = 0;
                    double exactShareDPMS = 0;
                    double exactShareBMS = 0;
                    double exactShareE = 1.0 * x;

                    result = new ScriptCalculationResult
                    {
                        SharePMS = (int)exactSharePMS,
                        ShareRPMS = 0,
                        ShareDPMS = 0,
                        ShareBMS = 0,
                        ShareE = (int)exactShareE,
                        ScriptsToExaminers = (int)x,

                        Success = true
                    };

                    //if (result.SharePMS < 50 && values.TotalEntries > 50)
                    //{
                    //    var diffToFifty = result.SharePMS - 50;
                    //    var totalShareToE = result.ShareE - diffToFifty;
                    //    result.ShareE = totalShareToE;
                    //    result.SharePMS = 50;
                    //}

                    var totalToE = values.TotalE * result.ShareE;
                    var shareToPMS = totalToE + result.SharePMS;




                    result.FinalSharePMS = shareToPMS;
                    result.FinalShareRPMS = 0;
                    result.FinalShareDPMS = 0;
                    result.FinalShareBMS = 0;


                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 &&
                         values.TotalBMS == 0 && values.TotalE == 0)
                {
                    result = new ScriptCalculationResult
                    {
                        SharePMS = (int)x,
                        ShareRPMS = 0,
                        ShareDPMS = 0,
                        ShareBMS = 0,
                        ShareE = 0,
                        ScriptsToExaminers = (int)x,

                        Success = true
                    };


                    var shareToPMS = values.TotalEntries;




                    result.FinalSharePMS = shareToPMS;
                    result.FinalShareRPMS = 0;
                    result.FinalShareDPMS = 0;
                    result.FinalShareBMS = 0;
                }

                result.TotalScriptsPMS = values.TotalPMS * result.FinalSharePMS;
                result.TotalScriptsRPMS = values.TotalRPMS * result.FinalShareRPMS;
                result.TotalScriptsDPMS = values.TotalDPMS * result.FinalShareDPMS;
                result.TotalScriptsBMS = values.TotalBMS * result.FinalShareBMS;

                var calculatedTotal = result.TotalScriptsPMS + result.TotalScriptsRPMS + result.TotalScriptsDPMS +
                                      result.TotalScriptsBMS;

                int discrepancy = values.TotalEntries - calculatedTotal;

                // Add discrepancy to the largest share (e.g., Examiners)
                if (discrepancy <= 0 && values.TotalPMS != 0)
                {

                    result.FinalSharePMS += discrepancy;
                }

                result.TotalScriptsPMS = values.TotalPMS * result.FinalSharePMS;


                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CalculateFomulars2([FromBody] ExaminerApportionmentViewModel values)
        {
            try
            {
                double x = 0;
                double A = 0.0;

                if (values.TotalPMS == 0)
                {
                    return BadRequest(new { success = false, message = "Please select at least one PMS option." });
                }

                if (values.TotalPMS > 0 && values.TotalRPMS > 0 && values.TotalDPMS > 0 && values.TotalBMS > 0 &&
                    values.TotalE > 0)
                {
                    A = values.TotalEntries / (0.1 + 0.125 * values.TotalRPMS + 0.15 * values.TotalDPMS +
                                               0.2 * values.TotalBMS + values.TotalE);
                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS > 0 && values.TotalBMS > 0 &&
                         values.TotalE > 0)
                {
                    A = values.TotalEntries / (0.1 + 0.15 * values.TotalDPMS + 0.2 * values.TotalBMS + values.TotalE);
                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 && values.TotalBMS > 0 &&
                         values.TotalE > 0)
                {
                    A = values.TotalEntries / (0.15 + 0.2 * values.TotalBMS + values.TotalE);
                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 &&
                         values.TotalBMS == 0 && values.TotalE > 0)
                {
                    A = values.TotalEntries / (0.2 + values.TotalE);
                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 &&
                         values.TotalBMS == 0 && values.TotalE == 0)
                {
                    A = values.TotalEntries;
                }

                x = A;
                var result = new ScriptCalculationResult();
                if (values.TotalPMS > 0 && values.TotalRPMS > 0 && values.TotalDPMS > 0 && values.TotalBMS > 0 &&
                    values.TotalE > 0)
                {



                    result = new ScriptCalculationResult
                    {
                        ShareBMS = values.ShareBMS,
                        ShareDPMS = values.ShareDPMS,
                        SharePMS = values.SharePMS,
                        ShareE = values.ShareE,
                        ShareRPMS = values.ShareRPMS,
                        ScriptsToExaminers = (int)x,

                        Success = true
                    };




                    result.FinalSharePMS = values.FinalSharePMS;
                    result.FinalShareRPMS = values.FinalShareRPMS;
                    result.FinalShareDPMS = values.FinalShareDPMS;
                    result.FinalShareBMS = values.FinalShareBMS;


                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS > 0 && values.TotalBMS > 0 &&
                         values.TotalE > 0)
                {


                    result = new ScriptCalculationResult
                    {
                        ShareBMS = values.ShareBMS,
                        ShareDPMS = values.ShareDPMS,
                        SharePMS = values.SharePMS,
                        ShareE = values.ShareE,
                        ShareRPMS = 0,
                        ScriptsToExaminers = (int)x,

                        Success = true
                    };



                    result.FinalSharePMS = values.FinalSharePMS;
                    result.FinalShareRPMS = 0;
                    result.FinalShareDPMS = values.FinalShareDPMS;
                    result.FinalShareBMS = values.FinalShareBMS;


                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 && values.TotalBMS > 0 &&
                         values.TotalE > 0)
                {



                    result = new ScriptCalculationResult
                    {
                        ShareBMS = values.ShareBMS,
                        ShareDPMS = 0,
                        SharePMS = values.SharePMS,
                        ShareE = values.ShareE,
                        ShareRPMS = 0,
                        ScriptsToExaminers = (int)x,

                        Success = true
                    };


                    result.FinalSharePMS = values.FinalSharePMS;
                    result.FinalShareRPMS = 0;
                    result.FinalShareDPMS = 0;
                    result.FinalShareBMS = values.FinalShareBMS;




                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 &&
                         values.TotalBMS == 0 && values.TotalE > 0)
                {


                    result = new ScriptCalculationResult
                    {
                        ShareBMS = values.ShareBMS,
                        ShareDPMS = 0,
                        SharePMS = 0,
                        ShareE = values.ShareE,
                        ShareRPMS = 0,
                        ScriptsToExaminers = (int)x,

                        Success = true
                    };

                    result.FinalSharePMS = values.FinalSharePMS;
                    result.FinalShareRPMS = 0;
                    result.FinalShareDPMS = 0;
                    result.FinalShareBMS = 0;



                }
                else if (values.TotalPMS > 0 && values.TotalRPMS == 0 && values.TotalDPMS == 0 &&
                         values.TotalBMS == 0 && values.TotalE == 0)
                {
                    result = new ScriptCalculationResult
                    {
                        SharePMS = (int)x,
                        ShareRPMS = 0,
                        ShareDPMS = 0,
                        ShareBMS = 0,
                        ShareE = 0,
                        ScriptsToExaminers = (int)x,

                        Success = true
                    };


                    var shareToPMS = values.TotalEntries;




                    result.FinalSharePMS = shareToPMS;
                    result.FinalShareRPMS = 0;
                    result.FinalShareDPMS = 0;
                    result.FinalShareBMS = 0;
                }

                result.TotalScriptsPMS = values.TotalPMS * result.FinalSharePMS;
                result.TotalScriptsRPMS = values.TotalRPMS * result.FinalShareRPMS;
                result.TotalScriptsDPMS = values.TotalDPMS * result.FinalShareDPMS;
                result.TotalScriptsBMS = values.TotalBMS * result.FinalShareBMS;

                //var calculatedTotal = result.TotalScriptsPMS + result.TotalScriptsRPMS + result.TotalScriptsDPMS + result.TotalScriptsBMS;

                //int discrepancy = values.TotalEntries - calculatedTotal;

                //// Add discrepancy to the largest share (e.g., Examiners)
                //if (discrepancy <= 0 && values.TotalPMS != 0)
                //{

                //    result.FinalSharePMS += discrepancy;
                //}

                //result.TotalScriptsPMS = values.TotalPMS * result.FinalSharePMS;


                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        //[HttpPost]
        //public IActionResult DownloadOrganizationCard(ExaminerApportionmentViewModel model, string examCode, string subjectCode, string paperCode, string regionCode, string activity)
        //{
        //    try
        //    {
        //        // Create memory stream for PDF
        //        var stream = new MemoryStream();

        //        // Initialize PDF writer
        //        using (var writer = new PdfWriter(stream))
        //        {
        //            using (var pdf = new PdfDocument(writer))
        //            {
        //                var document = new Document(pdf, PageSize.A4);
        //                document.SetMargins(40, 40, 40, 40);

        //                // Add title
        //                var title = new Paragraph("2025 DECEMBER O LEVEL MARKING: ORGANISATION CARD")
        //                    .SetTextAlignment(TextAlignment.CENTER)
        //                    .SetFontSize(16)
        //                    .SetBold();
        //                document.Add(title);

        //                // Add subject info
        //                document.Add(new Paragraph("\n"));
        //                document.Add(new Paragraph($"A. SUBJECT NAME ______ {subjectCode}"));
        //                document.Add(new Paragraph($"B. SUBJECT PAPER CODE ______ {subjectCode+"/"+paperCode}"));
        //                document.Add(new Paragraph($"C. PAPER EXAMINER ______ {model.TotalEntries}"));
        //                document.Add(new Paragraph($"D. MARKING CENTRE ______ CENTRALISED MARKING"));

        //                // Create main table
        //                var table = new Table(9, false);
        //                table.SetWidth(UnitValue.CreatePercentValue(100));

        //                // Add headers
        //                table.AddHeaderCell("PMS").SetBold();
        //                table.AddHeaderCell("No of PMS").SetBold();
        //                table.AddHeaderCell("Share per PMS").SetBold();
        //                table.AddHeaderCell("DPMS").SetBold();
        //                table.AddHeaderCell("No of DPMS").SetBold();
        //                table.AddHeaderCell("Share per DPMS").SetBold();
        //                table.AddHeaderCell("BMS").SetBold();
        //                table.AddHeaderCell("No of BMS").SetBold();
        //                table.AddHeaderCell("Share per BMS").SetBold();

        //                // Add data row
        //                table.AddCell(" ");
        //                table.AddCell(model.TotalPMS.ToString());
        //                table.AddCell(model.SharePMS.ToString("0.00"));
        //                table.AddCell(" ");
        //                table.AddCell(model.TotalDPMS.ToString());
        //                table.AddCell(model.ShareDPMS.ToString("0.00"));
        //                table.AddCell(" ");
        //                table.AddCell(model.TotalBMS.ToString());
        //                table.AddCell(model.ShareBMS.ToString("0.00"));

        //                // Add second row with Examiners and Grand Total
        //                var examinersRow = new Cell(1, 6).Add(new Paragraph("No of Examiners"));
        //                table.AddCell(examinersRow);
        //                table.AddCell(model.TotalE.ToString());
        //                table.AddCell(model.ShareE.ToString("0.00"));
        //                table.AddCell(new Cell(1, 8).Add(new Paragraph("GRAND TOTAL")).SetBold());
        //                table.AddCell(model.TotalEntries.ToString());

        //                document.Add(table);

        //                // Add check-in/check-out table
        //                document.Add(new Paragraph("\n\nSPECIFY CHECK IN CHECK OUT DETAILS"));

        //                var checkTable = new Table(3, false);
        //                checkTable.SetWidth(UnitValue.CreatePercentValue(100));

        //                checkTable.AddHeaderCell("EXAMINER CATEGORY").SetBold();
        //                checkTable.AddHeaderCell("CHECK IN").SetBold();
        //                checkTable.AddHeaderCell("CHECK OUT").SetBold();

        //                checkTable.AddCell("PMS");
        //                checkTable.AddCell(" ");
        //                checkTable.AddCell(" ");

        //                checkTable.AddCell("DPMS");
        //                checkTable.AddCell(" ");
        //                checkTable.AddCell(" ");

        //                checkTable.AddCell("BMS");
        //                checkTable.AddCell(" ");
        //                checkTable.AddCell(" ");

        //                checkTable.AddCell("EXAMINERS");
        //                checkTable.AddCell(" ");
        //                checkTable.AddCell(" ");

        //                document.Add(checkTable);

        //                // Add footer
        //                document.Add(new Paragraph("\n\nSUBJECT: SUBJECT MANAGEMENT ______ / JURO ______ DATE: ______ / WWW.DATUM.COM/PRIVATE")
        //                    .SetTextAlignment(TextAlignment.CENTER));

        //                document.Close();
        //            }
        //        }

        //        stream.Position = 0;
        //        return File(stream, "application/pdf", $"OrganizationCard_{subjectCode}_{paperCode}.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> DownloadOrganizationCard(
            string examCode,
            string subjectCode,
            string paperCode,
            string regionCode,
            string activity,
            int TotalPMS,
            int TotalRPMS,
            int TotalDPMS,
            int TotalBMS,
            int TotalE,
            decimal FinalSharePMS,
            decimal FinalShareRPMS,
            decimal FinalShareDPMS,
            decimal FinalShareBMS,
            decimal FinalShareE,
            int TotalEntries)
        {
            var stream = new MemoryStream();

            try
            {
                var sessionMon = DateTime.Now.Month.ToString();
                var currentYear = DateTime.Now.Year.ToString();
                var level = "";
                var subjectName = subjectCode;
                var session = await _examCodesRepository.GetExamCodesById(examCode);
                var subjectDTO = await _subjectsRepository.GetSubjectCode(subjectCode);
                if (sessionMon != null)
                {
                    sessionMon = session.EXM_EXAM_SESSION;
                    currentYear = session.EXM_EXAM_YEAR;
                    level = session.EXM_EXAM_LEVEL;
                }

                if (subjectName != null)
                {
                    subjectName = subjectDTO.SUB_SUBJECT_DESC;
                }

                var checkoutAndInDates = new List<CategoryCheckInCheckOut>();
                string pmsCheckIn = "";
                string pmsCheckout = "";
                string rpmsCheckIn = "";
                string rpmsCheckout = "";
                string dpmsCheckIn = "";
                string dpmsCheckout = "";
                string bmsCheckIn = "";
                string bmsCheckout = "";
                string eCheckIn = "";
                string eCheckout = "";
                string capCheckIn = "";
                string capCheckout = "";

                var query = _context.CATEGORYCHECKINCHECKOUT
                    .Where(a => a.SubSubId == examCode + subjectCode && a.PaperCode == paperCode);

                if (!string.IsNullOrEmpty(regionCode))
                {
                    query = query.Where(a => a.REGION == regionCode);
                }

                checkoutAndInDates = query.ToList();

                if (checkoutAndInDates.Any())
                {
                    pmsCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "PMS")?.CheckIn ?? "";
                    pmsCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "PMS")?.CheckOut ?? "";

                    rpmsCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "RPMS")?.CheckIn ?? "";
                    rpmsCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "RPMS")?.CheckOut ?? "";

                    dpmsCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "DPMS")?.CheckIn ?? "";
                    dpmsCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "DPMS")?.CheckOut ?? "";

                    bmsCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "BMS")?.CheckIn ?? "";
                    bmsCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "BMS")?.CheckOut ?? "";

                    eCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "E")?.CheckIn ?? "";
                    eCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "E")?.CheckOut ?? "";

                    capCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "CAPTURERS")?.CheckIn ?? "";
                    capCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "CAPTURERS")?.CheckOut ?? "";
                }


                var totalExaminers = TotalPMS + TotalRPMS + TotalDPMS + TotalBMS + TotalE;
                // Configure writer to NOT close the stream
                var writer = new PdfWriter(stream);
                writer.SetCloseStream(false);

                // Create landscape page size (A4 rotated)
                var pageSize = PageSize.A4.Rotate();

                using (var pdf = new PdfDocument(writer))
                using (var document = new Document(pdf, pageSize))
                {
                    document.SetMargins(40, 40, 40, 40);

                    // Add title
                    var title = new Paragraph($"{currentYear} {sessionMon} {level} LEVEL MARKING: ORGANISATION CARD")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetBold()
                        .SetBorder(new SolidBorder(1)) // 1 unit width border
                        .SetPadding(3) // Add some padding inside the border
                        .SetMarginBottom(2); // Add margin below the title

                    document.Add(title);

                    // Add subject info

                    document.Add(new Paragraph($"A. SUBJECT NAME :     {subjectName}"));
                    document.Add(new Paragraph($"B. SUBJECT PAPER CODE :    {subjectCode}/{paperCode}"));
                    document.Add(new Paragraph($"C. PAPER EXAMINER :    {TotalEntries}"));
                    document.Add(new Paragraph($"D. MARKING CENTRE  : CENTRALISED MARKING"));

                    // Create main table with 6 columns (PMS, RPMS, DPMS, BMS, EXAMINERS, GRAND TOTAL)


                    // Create main table with 12 columns (2 for each category: count and share)
                    var table = new Table(12); // PMS(2), RPMS(2), DPMS(2), BMS(2), EXAMINERS(2), GRAND TOTAL(2)
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    // First header row - main categories spanning 2 columns each
                    table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("PMS").SetBold())
                        .SetTextAlignment(TextAlignment.CENTER));
                    table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("RPMS").SetBold())
                        .SetTextAlignment(TextAlignment.CENTER));
                    table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("DPMS").SetBold())
                        .SetTextAlignment(TextAlignment.CENTER));
                    table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("BMS").SetBold())
                        .SetTextAlignment(TextAlignment.CENTER));
                    table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("EXAMINERS").SetBold())
                        .SetTextAlignment(TextAlignment.CENTER));
                    table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("GRAND TOTAL").SetBold())
                        .SetTextAlignment(TextAlignment.CENTER));

                    // Second header row - subheaders
                    table.AddHeaderCell("No. of PMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("Share per PMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("No. of RPMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("Share per RPMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("No. of DPMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("Share per DPMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("No. of BMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("Share per BMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("No. of Examiners").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("Share per Examiner").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("Total Examiners").SetBold().SetTextAlignment(TextAlignment.CENTER);
                    table.AddHeaderCell("Total Script").SetBold().SetTextAlignment(TextAlignment.CENTER);

                    // Data row
                    table.AddCell(TotalPMS.ToString()).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(FinalSharePMS.ToString("0")).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(TotalRPMS.ToString()).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(FinalShareRPMS.ToString("0")).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(TotalDPMS.ToString()).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(FinalShareDPMS.ToString("0")).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(TotalBMS.ToString()).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(FinalShareBMS.ToString("0")).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(TotalE.ToString()).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(FinalShareE.ToString("0")).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(totalExaminers.ToString()).SetTextAlignment(TextAlignment.CENTER);
                    table.AddCell(TotalEntries.ToString()).SetTextAlignment(TextAlignment.CENTER);


                    document.Add(table);


                    // Add check-in/check-out table
                    document.Add(new Paragraph("\nSPECIFY CHECK IN CHECK OUT DETAILS")).SetTopMargin(1);

                    var checkTable = new Table(3, false).SetMarginTop(0);
                    checkTable.SetWidth(UnitValue.CreatePercentValue(100));

                    checkTable.AddHeaderCell("EXAMINER CATEGORY").SetBold();
                    checkTable.AddHeaderCell("CHECK IN").SetBold();
                    checkTable.AddHeaderCell("CHECK OUT").SetBold();

                    checkTable.AddCell("PMS");
                    checkTable.AddCell(pmsCheckIn);
                    checkTable.AddCell(pmsCheckout);

                    checkTable.AddCell("RPMS");
                    checkTable.AddCell(rpmsCheckIn);
                    checkTable.AddCell(rpmsCheckout);

                    checkTable.AddCell("DPMS");
                    checkTable.AddCell(dpmsCheckIn);
                    checkTable.AddCell(dpmsCheckout);

                    checkTable.AddCell("BMS");
                    checkTable.AddCell(bmsCheckIn);
                    checkTable.AddCell(bmsCheckout);

                    checkTable.AddCell("EXAMINERS");
                    checkTable.AddCell(eCheckIn);
                    checkTable.AddCell(eCheckout);

                    checkTable.AddCell("MARK CAPTURERS");
                    checkTable.AddCell(capCheckIn);
                    checkTable.AddCell(capCheckout);



                    document.Add(checkTable);

                    // Add footer
                    document.Add(new Paragraph("\n\nSUBJECT: SUBJECT MANAGER_________________ DATE: ___/_______/_____ ")
                        .SetTextAlignment(TextAlignment.CENTER));

                    document.Close();
                }

                stream.Position = 0;
                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"OrganizationCard_{subjectCode}_{paperCode}.pdf"
                };
            }
            catch (Exception ex)
            {
                stream?.Dispose();
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        public async Task<IActionResult> Download(
     string examCode,
     string subjectCode,
     string paperCode,
     string regionCode,
     string activity)
        {
            var refcatpaper = new List<Apportioned>();

            var componentExaminers = new List<ExaminerScriptsMarked>();

            var total = new MarksCaptured(); 

            if (!string.IsNullOrEmpty(regionCode))
            {
                refcatpaper = await _context.REF_CAT_PAPER.Where(a => a.CTP_PPR_SUB_PAPER_CODE == (examCode + subjectCode + paperCode) && a.CTP_REGION_CODE == regionCode).ToListAsync();

                componentExaminers = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID == (examCode + subjectCode) && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == "BEM" && a.EMS_MARKING_REG_CODE == regionCode).ToListAsync();


                total = await _context.EXM_SCRIPT_CAPTURED
                .FirstOrDefaultAsync(a => a.SubjectCode == subjectCode && a.ExamCode == examCode && a.PaperCode == paperCode && a.RegionCode == regionCode);
            }
            else
            {
                 refcatpaper = await _context.REF_CAT_PAPER.Where(a => a.CTP_PPR_SUB_PAPER_CODE == (examCode + subjectCode + paperCode)).ToListAsync();

                 componentExaminers = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID == (examCode + subjectCode) && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == "BEM").ToListAsync();

                total = await _context.EXM_SCRIPT_CAPTURED
                .FirstOrDefaultAsync(a => a.SubjectCode == subjectCode && a.ExamCode == examCode && a.PaperCode == paperCode);

            }
            

            

            var totalPMS = componentExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PMS");
            var totalRPMS = componentExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "RPMS");
            var totalDPMS = componentExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "DPMS");
            var totalBMS = componentExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "BMS");
            var totalE = componentExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "E");

        
            var finalSharePMS = refcatpaper.FirstOrDefault(a => a.CTP_ECT_CAT_CODE == "PMS")?.CTP_MAX_SCRIPTS ?? 0;
            var finalShareRPMS = refcatpaper.FirstOrDefault(a => a.CTP_ECT_CAT_CODE == "RPMS")?.CTP_MAX_SCRIPTS ?? 0;
            var finalShareDPMS = refcatpaper.FirstOrDefault(a => a.CTP_ECT_CAT_CODE == "DPMS")?.CTP_MAX_SCRIPTS ?? 0;
            var finalShareBMS = refcatpaper.FirstOrDefault(a => a.CTP_ECT_CAT_CODE == "BMS")?.CTP_MAX_SCRIPTS ?? 0;
            var finalShareE = refcatpaper.FirstOrDefault(a => a.CTP_ECT_CAT_CODE == "E")?.CTP_MAX_SCRIPTS ?? 0;

            int totalEntries = total?.ApportionedScripts ?? 0;

            return await DownloadOrganizationCard2(examCode, subjectCode, paperCode, regionCode, activity,
                totalPMS, totalRPMS, totalDPMS, totalBMS, totalE,
                finalSharePMS, finalShareRPMS, finalShareDPMS, finalShareBMS, finalShareE, totalEntries);
        }

        public async Task<IActionResult> DownloadOrganizationCard2(
            string examCode,
            string subjectCode,
            string paperCode,
            string regionCode,
            string activity,
            int TotalPMS,
            int TotalRPMS,
            int TotalDPMS,
            int TotalBMS,
            int TotalE,
            decimal FinalSharePMS,
            decimal FinalShareRPMS,
            decimal FinalShareDPMS,
            decimal FinalShareBMS,
            decimal FinalShareE,
            int TotalEntries)
        {
            var stream = new MemoryStream();

            var sessionMon = DateTime.Now.Month.ToString();
            var currentYear = DateTime.Now.Year.ToString();
            var level = "";
            var subjectName = subjectCode;
            var session = await _examCodesRepository.GetExamCodesById(examCode);
            var subjectDTO = await _subjectsRepository.GetSubjectCode(subjectCode);

            if (session != null)
            {
                sessionMon = session.EXM_EXAM_SESSION;
                currentYear = session.EXM_EXAM_YEAR;
                level = session.EXM_EXAM_LEVEL;
            }

            if (subjectDTO != null)
            {
                subjectName = subjectDTO.SUB_SUBJECT_DESC;
            }

            var checkoutAndInDates = new List<CategoryCheckInCheckOut>();
            string pmsCheckIn = "";
            string pmsCheckout = "";
            string rpmsCheckIn = "";
            string rpmsCheckout = "";
            string dpmsCheckIn = "";
            string dpmsCheckout = "";
            string bmsCheckIn = "";
            string bmsCheckout = "";
            string eCheckIn = "";
            string eCheckout = "";
            string capCheckIn = "";
            string capCheckout = "";

            var query = _context.CATEGORYCHECKINCHECKOUT
                .Where(a => a.SubSubId == examCode + subjectCode && a.PaperCode == paperCode);

            if (!string.IsNullOrEmpty(regionCode))
            {
                query = query.Where(a => a.REGION == regionCode);
            }

            checkoutAndInDates = await query.ToListAsync();

            if (checkoutAndInDates.Any())
            {
                pmsCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "PMS")?.CheckIn ?? "";
                pmsCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "PMS")?.CheckOut ?? "";

                rpmsCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "RPMS")?.CheckIn ?? "";
                rpmsCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "RPMS")?.CheckOut ?? "";

                dpmsCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "DPMS")?.CheckIn ?? "";
                dpmsCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "DPMS")?.CheckOut ?? "";

                bmsCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "BMS")?.CheckIn ?? "";
                bmsCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "BMS")?.CheckOut ?? "";

                eCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "E")?.CheckIn ?? "";
                eCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "E")?.CheckOut ?? "";

                capCheckIn = checkoutAndInDates.FirstOrDefault(a => a.Category == "CAPTURERS")?.CheckIn ?? "";
                capCheckout = checkoutAndInDates.FirstOrDefault(a => a.Category == "CAPTURERS")?.CheckOut ?? "";
            }

            var totalExaminers = TotalPMS + TotalRPMS + TotalDPMS + TotalBMS + TotalE;

            // Configure writer to NOT close the stream
            var writer = new PdfWriter(stream);
            writer.SetCloseStream(false);

            // Create landscape page size (A4 rotated)
            var pageSize = PageSize.A4.Rotate();

            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf, pageSize))
            {
                document.SetMargins(40, 40, 40, 40);

                // Add title
                var title = new Paragraph($"{currentYear} {sessionMon} {level} LEVEL MARKING: ORGANISATION CARD")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetBold()
                    .SetBorder(new SolidBorder(1))
                    .SetPadding(3)
                    .SetMarginBottom(2);

                document.Add(title);

                // Add subject info
                document.Add(new Paragraph($"A. SUBJECT NAME :     {subjectName}"));
                document.Add(new Paragraph($"B. SUBJECT PAPER CODE :    {subjectCode}/{paperCode}"));
                document.Add(new Paragraph($"C. PAPER EXAMINER :    {TotalEntries}"));
                document.Add(new Paragraph($"D. MARKING CENTRE  : CENTRALISED MARKING"));

                // Create main table with 12 columns
                var table = new Table(12);
                table.SetWidth(UnitValue.CreatePercentValue(100));

                // First header row
                table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("PMS").SetBold()).SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("RPMS").SetBold()).SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("DPMS").SetBold()).SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("BMS").SetBold()).SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("EXAMINERS").SetBold()).SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("GRAND TOTAL").SetBold()).SetTextAlignment(TextAlignment.CENTER));

                // Second header row
                table.AddHeaderCell("No. of PMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("Share per PMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("No. of RPMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("Share per RPMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("No. of DPMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("Share per DPMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("No. of BMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("Share per BMS").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("No. of Examiners").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("Share per Examiner").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("Total Examiners").SetBold().SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell("Total Script").SetBold().SetTextAlignment(TextAlignment.CENTER);

                // Data row
                table.AddCell(TotalPMS.ToString()).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(FinalSharePMS.ToString("0")).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(TotalRPMS.ToString()).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(FinalShareRPMS.ToString("0")).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(TotalDPMS.ToString()).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(FinalShareDPMS.ToString("0")).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(TotalBMS.ToString()).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(FinalShareBMS.ToString("0")).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(TotalE.ToString()).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(FinalShareE.ToString("0")).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(totalExaminers.ToString()).SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(TotalEntries.ToString()).SetTextAlignment(TextAlignment.CENTER);

                document.Add(table);

                // Add check-in/check-out table
                document.Add(new Paragraph("\nSPECIFY CHECK IN CHECK OUT DETAILS")).SetTopMargin(1);

                var checkTable = new Table(3, false).SetMarginTop(0);
                checkTable.SetWidth(UnitValue.CreatePercentValue(100));

                checkTable.AddHeaderCell("EXAMINER CATEGORY").SetBold();
                checkTable.AddHeaderCell("CHECK IN").SetBold();
                checkTable.AddHeaderCell("CHECK OUT").SetBold();

                checkTable.AddCell("PMS");
                checkTable.AddCell(pmsCheckIn);
                checkTable.AddCell(pmsCheckout);

                checkTable.AddCell("RPMS");
                checkTable.AddCell(rpmsCheckIn);
                checkTable.AddCell(rpmsCheckout);

                checkTable.AddCell("DPMS");
                checkTable.AddCell(dpmsCheckIn);
                checkTable.AddCell(dpmsCheckout);

                checkTable.AddCell("BMS");
                checkTable.AddCell(bmsCheckIn);
                checkTable.AddCell(bmsCheckout);

                checkTable.AddCell("EXAMINERS");
                checkTable.AddCell(eCheckIn);
                checkTable.AddCell(eCheckout);

                checkTable.AddCell("MARK CAPTURERS");
                checkTable.AddCell(capCheckIn);
                checkTable.AddCell(capCheckout);

                document.Add(checkTable);

                // Add footer
                document.Add(new Paragraph("\n\nSUBJECT: SUBJECT MANAGER_________________ DATE: ___/_______/_____ ")
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Close();
            }

            stream.Position = 0;
            return new FileStreamResult(stream, "application/pdf")
            {
                FileDownloadName = $"OrganizationCard_{subjectCode}_{paperCode}.pdf"
            };
        }

        [HttpPost]
        public async Task<IActionResult> DownloadMaterialCard(
            string examCode,
            string subjectCode,
            string paperCode,
            string regionCode)
        {
            var stream = new MemoryStream();

            try
            {
                // Get session and subject info
                var sessionMon = DateTime.Now.Month.ToString();
                var currentYear = DateTime.Now.Year.ToString();
                var level = "";
                var subjectName = subjectCode;
                var session = await _examCodesRepository.GetExamCodesById(examCode);
                var subjectDTO = await _subjectsRepository.GetSubjectCode(subjectCode);

                if (session != null)
                {
                    sessionMon = session.EXM_EXAM_SESSION;
                    currentYear = session.EXM_EXAM_YEAR;
                    level = session.EXM_EXAM_LEVEL;
                }

                if (subjectDTO != null)
                {
                    subjectName = subjectDTO.SUB_SUBJECT_DESC;
                }

                // Get materials data from database
                var materials = await _context.MaterialTransaction
                    .Where(m => m.SUBSUBID == examCode + subjectCode && m.PAPERCODE == paperCode)
                    .OrderBy(m => m.ITEM)
                    .ToListAsync();



                // Configure writer
                var writer = new PdfWriter(stream);
                writer.SetCloseStream(false);
                var pageSize = PageSize.A4.Rotate();

                using (var pdf = new PdfDocument(writer))
                using (var document = new Document(pdf, pageSize))
                {
                    document.SetMargins(40, 40, 40, 40);

                    // Add title
                    var title = new Paragraph($"{currentYear} {sessionMon} {level} LEVEL MARKING: MATERIALS REQUIRED")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetBold()
                        .SetBorder(new SolidBorder(1))
                        .SetPadding(3)
                        .SetMarginBottom(20);
                    document.Add(title);

                    // Add subject info
                    document.Add(new Paragraph($"A. SUBJECT NAME :     {subjectName}"));
                    document.Add(new Paragraph($"B. SUBJECT PAPER CODE :    {subjectCode}/{paperCode}"));

                    document.Add(new Paragraph($"D. MARKING CENTRE  : CENTRALISED MARKING"));

                    // Add materials table header
                    document.Add(new Paragraph("\n\nMATERIALS REQUIRED:")
                        .SetBold()
                        .SetMarginBottom(10));

                    // Create materials table
                    var table = new Table(UnitValue.CreatePercentArray(new float[] { 70, 30 }))
                        .UseAllAvailableWidth()
                        .SetMarginBottom(20);

                    // Add table headers
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Name of Item").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Quantity").SetBold()));

                    // Add materials data
                    int counter = 1;
                    foreach (var material in materials)
                    {
                        table.AddCell(new Cell().Add(new Paragraph($"{counter}. {material.ITEM}")));
                        table.AddCell(new Cell().Add(new Paragraph(material.QUANTITY.ToString())));
                        counter++;
                    }

                    document.Add(table);

                    // Add footer
                    document.Add(new Paragraph("\n\nSUBJECT: SUBJECT MANAGER_________________ DATE: ___/_______/_____ ")
                        .SetTextAlignment(TextAlignment.CENTER));

                    document.Close();
                }

                stream.Position = 0;
                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"OrganizationCard_{subjectCode}_{paperCode}.pdf"
                };
            }
            catch (Exception ex)
            {
                stream?.Dispose();
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }



        [HttpPost]
        public async Task<IActionResult> SaveExaminerApportionment(ExaminerApportionmentViewModel modell,
            string examCode, string subjectCode,
            string paperCode, string regionCode,
            string activity)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                var model = new ApportionScriptsViewModel()
                {
                    SelectedPMS = modell.TotalPMS,
                    SelectedRPMS = modell.TotalRPMS,
                    SelectedDPMS = modell.TotalDPMS,
                    SelectedBMS = modell.TotalBMS,
                    SelectedE = modell.TotalE,
                    MaxScriptsPMS = modell.FinalSharePMS,
                    MaxScriptsDPMS = modell.FinalShareDPMS,
                    MaxScriptsRPMS = modell.FinalShareRPMS,
                    MaxScriptsBMS = modell.FinalShareBMS,

                };



                var result = await _repository.SaveApportionment(model, examCode, subjectCode, paperCode, regionCode,
                    activity, currentUser.Id);

                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }


                var examinersResult = await _repository.SaveExaminerApportionment(modell, examCode, subjectCode,
                    paperCode, regionCode, activity, currentUser.Id);

                if (!examinersResult.Success)
                {
                    return BadRequest(result.Message);
                }

                return Json(new { success = true, message = "Apportionment saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> GetDataSelectTeam(string examCode = "", string subjectCode = "",
            string paperCode = "", string activity = "", string regionCode = "", string status = "")
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);


            IEnumerable<SelectTeamViewModel> model = new List<SelectTeamViewModel>();




            var availableInRegister =
                await _repository.GetTeamsFromMasterAsync(examCode, subjectCode, paperCode, regionCode, activity);

            if (!string.IsNullOrEmpty(status))
            {
                availableInRegister = status switch
                {
                    "TotalInvited" => availableInRegister, // No filter for total
                    "Selected" => availableInRegister.Where(e => e.Selected == "Selected"),
                    "PMS" => availableInRegister.Where(e => e.Category == "PMS" && e.Selected == "Selected"),
                    "RPMS" => availableInRegister.Where(e => e.Category == "RPMS" && e.Selected == "Selected"),
                    "DPMS" => availableInRegister.Where(e => e.Category == "DPMS" && e.Selected == "Selected"),
                    "BMS" => availableInRegister.Where(e => e.Category == "BMS" && e.Selected == "Selected"),
                    "AE" => availableInRegister.Where(e => e.Category == "E" && e.Selected == "Selected"),
                    _ => availableInRegister
                };
            }


            model = availableInRegister.ToList();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"]
                .FirstOrDefault();
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
                    (p.Category?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
                    (p.Team?.ToLower().Contains(searchValue.ToLower()) ?? false)
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

        [Authorize]
        public async Task<IActionResult> GetData2(string examCode, string subjectCode, string paperCode,
            string regionCode, string status = "")
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);


            IEnumerable<SelectTeamViewModel> model = new List<SelectTeamViewModel>();




            var availableInRegister = await _repository.GetAllFromMasterAsync();

            if (!string.IsNullOrEmpty(subjectCode))
            {
                var sub = subjectCode.Substring(3);
                availableInRegister = availableInRegister.Where(a => a.SubjectCode == sub);
            }


            if (!string.IsNullOrEmpty(paperCode))
            {
                availableInRegister = availableInRegister.Where(a => a.PaperCode == paperCode);
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                availableInRegister = availableInRegister.Where(a => a.Region == regionCode);
            }

            if (!string.IsNullOrEmpty(status))
            {
                availableInRegister = status switch
                {
                    "TotalInvited" => availableInRegister, // No filter for total
                    "Selected" => availableInRegister.Where(e => e.Selected == "Selected"),
                    "PMS" => availableInRegister.Where(e => e.Category == "PMS" && e.Selected == "Selected"),
                    "RPMS" => availableInRegister.Where(e => e.Category == "RPMS" && e.Selected == "Selected"),
                    "DPMS" => availableInRegister.Where(e => e.Category == "DPMS" && e.Selected == "Selected"),
                    "BMS" => availableInRegister.Where(e => e.Category == "BMS" && e.Selected == "Selected"),
                    "AE" => availableInRegister.Where(e => e.Category == "E" && e.Selected == "Selected"),
                    _ => availableInRegister
                };
            }


            model = availableInRegister.ToList();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"]
                .FirstOrDefault();
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CountStats(string examCode = "", string subjectCode = "",
            string paperCode = "", string activity = "", string regionCode = "")
        {
            var availableInRegister =
                await _repository.GetTeamsFromMasterAsync(examCode, subjectCode, paperCode, regionCode, activity);

            var selectedInExaminers =
                await _repository.GetSelectedTeamsFromTransactionAsync(examCode, subjectCode, paperCode, regionCode,
                    activity);

            var selectedExaminers = selectedInExaminers.Count();

            var pmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PMS");
            var rpmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "RPMS");
            var bmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "BMS");
            var aeExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "E");
            var dpmsExaminers = selectedInExaminers.Count(a => a.EMS_ECT_EXAMINER_CAT_CODE == "DPMS");

            var counts = new
            {
                Total = availableInRegister.Count(), SelectedCount = selectedExaminers, PmsCount = pmsExaminers,
                RpmsCount = rpmsExaminers, BmsCount = bmsExaminers, aeCount = aeExaminers, dpmsCount = dpmsExaminers
            };

            return Json(counts);
        }


        // Controller Method
        //[HttpPost]
        //public async Task<IActionResult> UpdateSelect([FromBody] ExaminerUpdateModel model)
        //{
        //    if (model == null)
        //    {
        //        return BadRequest("Invalid data.");
        //    }

        //    try
        //    {
        //        ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
        //        var data = await _repository.UpdateTransactionRecord(model, currentUser);

        //        if (data.Success && data.Message == "Examiner Selected")
        //        {
        //            var subkey = model.ExamCode + model.SubjectCode + model.PaperCode + model.Activity + model.IdNumber;
        //            var examiner = await _examinerRepository.GetExaminerRecord(model.IdNumber);

        //            if(examiner != null)
        //            {
        //                var user = await _userRepository.GetUser(model.IdNumber, subkey);

        //                if (user == null)
        //                {

        //                    string cleanFirstName = RemoveMiddleName(examiner.EMS_EXAMINER_NAME);
        //                    string cleanSurname = RemoveMiddleName(examiner.EMS_LAST_NAME);
        //                    var subjectcode = model.SubjectCode;
        //                    var papercode = model.PaperCode;
        //                    if (model.Category == null)
        //                    {
        //                        model.Category = "E";
        //                    }

        //                    // Generate prefixes
        //                    string firstNamePrefix = cleanFirstName.Substring(0, Math.Min(3, cleanFirstName.Length)).ToLower();
        //                    string surnamePrefix = cleanSurname.ToLower();

        //                    // Construct the username
        //                    string username = $"{firstNamePrefix}{surnamePrefix}";
        //                    // Check if the examiner already exists as a user


        //                    var originalUsername = username;
        //                    var existingUser = await _userManager.FindByNameAsync(username);
        //                    int counter = 1;

        //                    // Check if the existing username already exists and matches the criteria
        //                    while (existingUser != null && (existingUser.UserName == username && existingUser.EMS_SUBKEY != subkey))
        //                    {
        //                        // Append the counter to the username
        //                        username = originalUsername + counter.ToString();

        //                        // Check if the new username exists
        //                        existingUser = await _userManager.FindByNameAsync(username);

        //                        // Increment the counter for the next iteration
        //                        counter++;
        //                    }


        //                    if (existingUser == null)
        //                    {

        //                        // Create a new user based on the examiner details
        //                        var newUser = new ApplicationUser
        //                        {

        //                            UserName = username,
        //                            Email = $"{username}@ems.com",
        //                            EMS_SUBKEY = subkey,
        //                            PhoneNumber = examiner.EMS_PHONE_HOME ?? "0000000000",
        //                            IDNumber = examiner.EMS_NATIONAL_ID,
        //                            Examiner = examiner,
        //                            ExaminerCode = examiner.EMS_EXAMINER_CODE,
        //                            Activated = true,
        //                            LockoutEnabled = true,
        //                            EmailConfirmed = true,
        //                            Activity = model.Activity,
        //                            RoleId = "",

        //                        };


        //                        string defaultPassword = GenerateDefaultPassword(newUser, subjectcode, papercode);

        //                        // Create the user with the generated password
        //                        var results = await _userManager.CreateAsync(newUser, defaultPassword);

        //                        user = newUser;


        //                        if (model.Category == "E")
        //                        {
        //                            if (results.Succeeded)
        //                            {
        //                                await _userManager.AddToRoleAsync(user, "Examiner");

        //                            }
        //                            else
        //                            {
        //                                // Handle errors if user creation fails
        //                                foreach (var error in results.Errors)
        //                                {
        //                                    ModelState.AddModelError(string.Empty, error.Description);
        //                                }
        //                            }

        //                        }
        //                        else if (model.Category == "A")
        //                        {
        //                            if (results.Succeeded)
        //                            {
        //                                await _userManager.AddToRoleAsync(user, "A");
        //                            }
        //                            else
        //                            {
        //                                // Handle errors if user creation fails
        //                                foreach (var error in results.Errors)
        //                                {
        //                                    ModelState.AddModelError(string.Empty, error.Description);
        //                                }
        //                            }
        //                        }
        //                        else if (model.Category == "BT")
        //                        {
        //                            if (results.Succeeded)
        //                            {
        //                                await _userManager.AddToRoleAsync(user, "BT");
        //                            }
        //                            else
        //                            {
        //                                // Handle errors if user creation fails
        //                                foreach (var error in results.Errors)
        //                                {
        //                                    ModelState.AddModelError(string.Empty, error.Description);
        //                                }
        //                            }
        //                        }
        //                        else if (model.Category == "PBT")
        //                        {
        //                            if (results.Succeeded)
        //                            {
        //                                await _userManager.AddToRoleAsync(user, "PBT");
        //                            }
        //                            else
        //                            {
        //                                // Handle errors if user creation fails
        //                                foreach (var error in results.Errors)
        //                                {
        //                                    ModelState.AddModelError(string.Empty, error.Description);
        //                                }
        //                            }
        //                        }
        //                        else if (model.Category == "PMS")
        //                        {
        //                            if (results.Succeeded)
        //                            {
        //                                await _userManager.AddToRoleAsync(user, "PMS");
        //                            }
        //                            else
        //                            {
        //                                // Handle errors if user creation fails
        //                                foreach (var error in results.Errors)
        //                                {
        //                                    ModelState.AddModelError(string.Empty, error.Description);
        //                                }
        //                            }
        //                        }
        //                        else if (model.Category == "BMS")
        //                        {
        //                            if (results.Succeeded)
        //                            {
        //                                await _userManager.AddToRoleAsync(user, "BMS");
        //                            }
        //                            else
        //                            {
        //                                // Handle errors if user creation fails
        //                                foreach (var error in results.Errors)
        //                                {
        //                                    ModelState.AddModelError(string.Empty, error.Description);
        //                                }
        //                            }

        //                        }
        //                        else if (model.Category == "DPMS")
        //                        {
        //                            if (results.Succeeded)
        //                            {
        //                                await _userManager.AddToRoleAsync(user, "DPMS");
        //                            }
        //                            else
        //                            {
        //                                // Handle errors if user creation fails
        //                                foreach (var error in results.Errors)
        //                                {
        //                                    ModelState.AddModelError(string.Empty, error.Description);
        //                                }
        //                            }
        //                        }
        //                        else if (model.Category == "RPMS")
        //                        {
        //                            if (results.Succeeded)
        //                            {
        //                                await _userManager.AddToRoleAsync(user, "RPMS");
        //                            }
        //                            else
        //                            {
        //                                // Handle errors if user creation fails
        //                                foreach (var error in results.Errors)
        //                                {
        //                                    ModelState.AddModelError(string.Empty, error.Description);
        //                                }
        //                            }
        //                        }
        //                    }


        //                }
        //                else
        //                {

        //                    user.EMS_SUBKEY = subkey;
        //                    user.Activity = model.Activity;
        //                    var results = await _userManager.UpdateAsync(user);

        //                    if (model.Category == "E")
        //                    {
        //                        if (results.Succeeded)
        //                        {
        //                            var currentRoles = await _userManager.GetRolesAsync(user);
        //                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        //                            await _userManager.AddToRoleAsync(user, "Examiner");
        //                        }
        //                        else
        //                        {
        //                            // Handle errors if user creation fails
        //                            foreach (var error in results.Errors)
        //                            {
        //                                ModelState.AddModelError(string.Empty, error.Description);
        //                            }
        //                        }

        //                    }
        //                    else if (model.Category == "A")
        //                    {
        //                        if (results.Succeeded)
        //                        {
        //                            var currentRoles = await _userManager.GetRolesAsync(user);
        //                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        //                            await _userManager.AddToRoleAsync(user, "A");
        //                        }
        //                        else
        //                        {
        //                            // Handle errors if user creation fails
        //                            foreach (var error in results.Errors)
        //                            {
        //                                ModelState.AddModelError(string.Empty, error.Description);
        //                            }
        //                        }
        //                    }
        //                    else if (model.Category == "BT")
        //                    {
        //                        if (results.Succeeded)
        //                        {
        //                            var currentRoles = await _userManager.GetRolesAsync(user);
        //                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        //                            await _userManager.AddToRoleAsync(user, "BT");
        //                        }
        //                        else
        //                        {
        //                            // Handle errors if user creation fails
        //                            foreach (var error in results.Errors)
        //                            {
        //                                ModelState.AddModelError(string.Empty, error.Description);
        //                            }
        //                        }
        //                    }
        //                    else if (model.Category == "PBT")
        //                    {
        //                        if (results.Succeeded)
        //                        {
        //                            var currentRoles = await _userManager.GetRolesAsync(user);
        //                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        //                            await _userManager.AddToRoleAsync(user, "PBT");
        //                        }
        //                        else
        //                        {
        //                            // Handle errors if user creation fails
        //                            foreach (var error in results.Errors)
        //                            {
        //                                ModelState.AddModelError(string.Empty, error.Description);
        //                            }
        //                        }
        //                    }
        //                    else if (model.Category == "PMS")
        //                    {
        //                        if (results.Succeeded)
        //                        {
        //                            var currentRoles = await _userManager.GetRolesAsync(user);
        //                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        //                            await _userManager.AddToRoleAsync(user, "PMS");
        //                        }
        //                        else
        //                        {
        //                            // Handle errors if user creation fails
        //                            foreach (var error in results.Errors)
        //                            {
        //                                ModelState.AddModelError(string.Empty, error.Description);
        //                            }
        //                        }
        //                    }
        //                    else if (model.Category == "BMS")
        //                    {
        //                        if (results.Succeeded)
        //                        {
        //                            var currentRoles = await _userManager.GetRolesAsync(user);
        //                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        //                            await _userManager.AddToRoleAsync(user, "BMS");
        //                        }
        //                        else
        //                        {
        //                            // Handle errors if user creation fails
        //                            foreach (var error in results.Errors)
        //                            {
        //                                ModelState.AddModelError(string.Empty, error.Description);
        //                            }
        //                        }

        //                    }
        //                    else if (model.Category == "DPMS")
        //                    {
        //                        if (results.Succeeded)
        //                        {
        //                            var currentRoles = await _userManager.GetRolesAsync(user);
        //                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        //                            await _userManager.AddToRoleAsync(user, "DPMS");
        //                        }
        //                        else
        //                        {
        //                            // Handle errors if user creation fails
        //                            foreach (var error in results.Errors)
        //                            {
        //                                ModelState.AddModelError(string.Empty, error.Description);
        //                            }
        //                        }
        //                    }
        //                    else if (model.Category == "RPMS")
        //                    {
        //                        if (results.Succeeded)
        //                        {
        //                            var currentRoles = await _userManager.GetRolesAsync(user);
        //                            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        //                            await _userManager.AddToRoleAsync(user, "RPMS");
        //                        }
        //                        else
        //                        {
        //                            // Handle errors if user creation fails
        //                            foreach (var error in results.Errors)
        //                            {
        //                                ModelState.AddModelError(string.Empty, error.Description);
        //                            }
        //                        }
        //                    }
        //                }
        //            }



        //        }
        //        else
        //        {
        //            var key = model.ExamCode + model.SubjectCode + model.PaperCode + model.Activity + model.IdNumber;
        //            var user = await _userRepository.GetUser(model.IdNumber, key);
        //            if (user != null)
        //            {
        //                var result = _userRepository.DeleteUser(user.IDNumber, user.EMS_SUBKEY);
        //            }


        //        }

        //        if (!data.Success)
        //        {
        //            return BadRequest(data.Message);
        //        }
        //        return Ok();

        //    }
        //    catch (Exception ex)
        //    {

        //        return StatusCode(500, "An error occurred while updating the presence status.");
        //    }
        //}

        public async Task<ValidationResult> ValidateBMSTeamRolesAsync(ExaminerUpdateModel model)
        {
           
            bool hasV = model.TeamMembers
                .Any(t => t.CapturingRole?.ToUpper() == "V");

            bool hasC = model.TeamMembers
                .Any(t => t.CapturingRole?.ToUpper() == "C");

            if (!hasV || !hasC)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Each BMS team must have both a Capturer (C) and Verifier (V)."
                };
            }
            

            return new ValidationResult { IsValid = true };
        }

        public async Task<ValidationResult> ValidatePMSTeamRolesAsync(ExaminerUpdateModel model)
        {

            bool allAreE = model.TeamMembers.All(t => t.Category == "E");


            if (allAreE) {
                bool hasV = model.TeamMembers
                   .Any(t => t.CapturingRole?.ToUpper() == "V");

                bool hasC = model.TeamMembers
                    .Any(t => t.CapturingRole?.ToUpper() == "C");

                if (!hasV || !hasC)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Team must have both a Capturer (C) and Verifier (V)."
                    };
                }


            }


            return new ValidationResult { IsValid = true };
        }



        public async Task<ActionResult<IEnumerable<ExaminerSelect>>> GetTeamMembers(string teamNumber, string idNumber)
        {
            var supervisor = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(a =>
                a.EMS_NATIONAL_ID == idNumber && a.EMS_ACTIVITY == "BEM" && a.EMS_EXAMINER_NUMBER == teamNumber);

            var members = await _context.EXAMINER_TRANSACTIONS
                .Where(e => e.EMS_EXM_SUPERORD == supervisor.EMS_EXAMINER_NUMBER &&
                            e.EMS_SUB_SUB_ID == supervisor.EMS_SUB_SUB_ID &&
                            e.EMS_PAPER_CODE == supervisor.EMS_PAPER_CODE && e.EMS_ACTIVITY == "BEM")
                .Include(a => a.Examiner)
                .Select(e => new ExaminerSelect
                {
                    IdNumber = e.EMS_NATIONAL_ID,
                    Name = e.Examiner.EMS_EXAMINER_NAME + " " + e.Examiner.EMS_LAST_NAME,
                    Capturing = e.EMS_CAPTURINGROLE,
                    Status = e.EMS_ECT_EXAMINER_CAT_CODE
                })
                .ToListAsync();

            if (!members.Any())
            {
                return NotFound($"No members found for team {teamNumber}");
            }

            return Ok(members);
        }


        public async Task<IActionResult> UpdateSelect([FromBody] ExaminerUpdateModel model)
        {
            if (model == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                if (model.TeamMembers == null)
                {
                    List<TeamMemberDto> TeamMembers = new List<TeamMemberDto>();
                    model.TeamMembers = TeamMembers;
                }

                if (model.Activity == "BEM")
                {


                    if (model.TeamMembers.Any() && model.Category == "BMS")
                    {
                       

                        var validateCandV = await ValidateBMSTeamRolesAsync(model);
                        if (!validateCandV.IsValid)
                        {
                            return BadRequest(validateCandV.ErrorMessage);
                        }

                    }

                    if (model.TeamMembers.Any() && model.Category == "PMS")
                    {


                        var validateCandV = await ValidatePMSTeamRolesAsync(model);
                        if (!validateCandV.IsValid)
                        {
                            return BadRequest(validateCandV.ErrorMessage);
                        }

                    }


                    if (model.Category == "PMS")
                    {
                        if (!string.IsNullOrEmpty(model.RegionCode) || model.SubjectCode.StartsWith("7"))
                        {
                            // Parse region code (01-10) and calculate base number
                            int regionNumber = int.TryParse(model.RegionCode, out int num) ? num : 1;
                            regionNumber = Math.Clamp(regionNumber, 1, 10); // Ensure 1-10

                            // PMS always gets the first number in the region
                            int baseNumber = regionNumber * 1000 + 1; // 1001, 2001,...10001

                            // Check if this PMS number already exists
                            bool pmsExists = await _context.EXAMINER_TRANSACTIONS
                                .AnyAsync(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                            && a.EMS_PAPER_CODE == model.PaperCode
                                            && a.EMS_ACTIVITY == "BEM"
                                            && a.EMS_EXAMINER_NUMBER == baseNumber.ToString()
                                            && a.EMS_MARKING_REG_CODE == model.RegionCode);

                            if (pmsExists)
                            {
                                // Find next available number in this region's range
                                var existingNumbers = await _context.EXAMINER_TRANSACTIONS
                                    .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                             && a.EMS_PAPER_CODE == model.PaperCode
                                             && a.EMS_ACTIVITY == "BEM"
                                             && a.EMS_MARKING_REG_CODE == model.RegionCode
                                             && a.EMS_EXAMINER_NUMBER.StartsWith(regionNumber.ToString()))
                                    .Select(a => int.Parse(a.EMS_EXAMINER_NUMBER))
                                    .ToListAsync();

                                baseNumber = existingNumbers.Any()
                                    ? existingNumbers.Max() + 1
                                    : baseNumber;
                            }

                            // Assign numbers
                            model.ExaminerNumber = baseNumber.ToString();
         
                        }
                        else
                        {
                            var maxPMS = await _context.EXAMINER_TRANSACTIONS
                           .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                       && a.EMS_PAPER_CODE == model.PaperCode
                                       && a.EMS_ACTIVITY == "BEM"
                                       && a.EMS_ECT_EXAMINER_CAT_CODE == "PMS")
                           .Select(a => a.EMS_EXAMINER_NUMBER)
                           .ToListAsync();

                            int nextExaminerNumber = maxPMS
                                .Select(code => int.TryParse(code, out int number) ? number : 0)
                                .DefaultIfEmpty(1000)
                                .Max() + 1;

                            model.ExaminerNumber = nextExaminerNumber.ToString();
                            model.Team = "1001";
                        }
                           

                    }
                    else if (model.Category == "RPMS")
                    {
                        if (!string.IsNullOrEmpty(model.RegionCode) || model.SubjectCode.StartsWith("7"))
                        {
                            // Parse region code (01-10) and calculate base number
                            int regionNumber = int.TryParse(model.RegionCode, out int num) ? num : 1;
                            regionNumber = Math.Clamp(regionNumber, 1, 10); // Ensure 1-10

                            // Get all existing numbers in this region
                            var existingNumbers = await _context.EXAMINER_TRANSACTIONS
                                .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                         && a.EMS_PAPER_CODE == model.PaperCode
                                         && a.EMS_ACTIVITY == "BEM"
                                         && a.EMS_MARKING_REG_CODE == model.RegionCode
                                         && a.EMS_EXAMINER_NUMBER.StartsWith(regionNumber.ToString()))
                                .Select(a => int.Parse(a.EMS_EXAMINER_NUMBER))
                                .ToListAsync();

                            int basePmsNumber = regionNumber * 1000 + 1; // 1001, 2001,...10001
                            int rpmsNumber;

                            // Check if PMS exists
                            bool pmsExists = existingNumbers.Contains(basePmsNumber);

                            if (pmsExists)
                            {
                                // RPMS gets the next available number after PMS
                                rpmsNumber = existingNumbers.Any()
                                    ? existingNumbers.Max() + 1
                                    : basePmsNumber + 1; // If PMS exists but no other numbers

                                // Ensure we don't skip numbers (e.g., if PMS is 1001 but 1002 is taken)
                                while (existingNumbers.Contains(rpmsNumber))
                                {
                                    rpmsNumber++;
                                }
                            }
                            else
                            {
                                // If no PMS, RPMS gets the first number
                                rpmsNumber = basePmsNumber;
                            }

                            // Assign numbers
                            model.ExaminerNumber = rpmsNumber.ToString();
                            model.Team = basePmsNumber.ToString(); // Team is always PMS base number
                        }
                        else
                        {
                          

                                var maxRPMS = await _context.EXAMINER_TRANSACTIONS
                             .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                         && a.EMS_PAPER_CODE == model.PaperCode
                                         && a.EMS_ACTIVITY == "BEM"
                                         && a.EMS_ECT_EXAMINER_CAT_CODE == "RPMS")
                             .Select(a => a.EMS_EXAMINER_NUMBER)
                             .ToListAsync();

                            int nextExaminerNumber = maxRPMS
                                .Select(code => int.TryParse(code, out int number) ? number : 0)
                                .DefaultIfEmpty(2000) // If list is empty, use 2000, so result becomes 2001
                                .Max() + 1;

                            model.ExaminerNumber = nextExaminerNumber.ToString();
                        }

                         
                    }
                    else if (model.Category == "DPMS")
                    {
                        if (!string.IsNullOrEmpty(model.RegionCode) || model.SubjectCode.StartsWith("7"))
                        {
                            // Parse region code (01-10)
                            int regionNumber = int.TryParse(model.RegionCode, out int num) ? num : 1;
                            regionNumber = Math.Clamp(regionNumber, 1, 10);

                            // Get all existing examiner numbers in this region that start with region code
                            var existingNumbers = await _context.EXAMINER_TRANSACTIONS
                                .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                         && a.EMS_PAPER_CODE == model.PaperCode
                                         && a.EMS_ACTIVITY == "BEM"
                                         && a.EMS_MARKING_REG_CODE == model.RegionCode
                                         && a.EMS_EXAMINER_NUMBER.StartsWith(regionNumber.ToString()))
                                .Select(a => int.Parse(a.EMS_EXAMINER_NUMBER))
                                .ToListAsync();

                            // Calculate next available number
                            int nextExaminerNumber = existingNumbers.Any()
                                ? existingNumbers.Max() + 1
                                : regionNumber * 1000 + 1; // Start with region base if no examiners exist

                            // Ensure number starts with region code
                            if (!nextExaminerNumber.ToString().StartsWith(regionNumber.ToString()))
                            {
                                nextExaminerNumber = regionNumber * 1000 + 1;
                                while (existingNumbers.Contains(nextExaminerNumber))
                                {
                                    nextExaminerNumber++;
                                }
                            }

                            // Assign numbers
                            model.ExaminerNumber = nextExaminerNumber.ToString();

                           
                        }
                        else
                        {
                            var maxDPMS = await _context.EXAMINER_TRANSACTIONS
                           .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                       && a.EMS_PAPER_CODE == model.PaperCode
                                       && a.EMS_ACTIVITY == "BEM"
                                       && a.EMS_ECT_EXAMINER_CAT_CODE == "DPMS" )
                           .Select(a => a.EMS_EXAMINER_NUMBER)
                           .ToListAsync();

                            int nextExaminerNumber = maxDPMS
                                .Select(code => int.TryParse(code, out int number) ? number : 0)
                                .DefaultIfEmpty(2000)
                                .Max() + 1;

                            model.ExaminerNumber = nextExaminerNumber.ToString();
                        }
                       
                    }

                    else if (model.Category == "BMS")
                    {
                        if (!string.IsNullOrEmpty(model.RegionCode) || model.SubjectCode.StartsWith("7"))
                        {
                            // Parse region code (01-10)
                            int regionNumber = int.TryParse(model.RegionCode, out int num) ? num : 1;
                            regionNumber = Math.Clamp(regionNumber, 1, 10);

                            // Get all existing examiner numbers in this region that start with region code
                            var existingNumbers = await _context.EXAMINER_TRANSACTIONS
                                .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                         && a.EMS_PAPER_CODE == model.PaperCode
                                         && a.EMS_ACTIVITY == "BEM"
                                         && a.EMS_MARKING_REG_CODE == model.RegionCode
                                         && a.EMS_EXAMINER_NUMBER.StartsWith(regionNumber.ToString()))
                                .Select(a => int.Parse(a.EMS_EXAMINER_NUMBER))
                                .ToListAsync();

                            // Calculate next available number
                            int nextExaminerNumber = existingNumbers.Any()
                                ? existingNumbers.Max() + 1
                                : regionNumber * 1000 + 1; // Start with region base if no examiners exist

                            // Ensure number starts with region code
                            if (!nextExaminerNumber.ToString().StartsWith(regionNumber.ToString()))
                            {
                                nextExaminerNumber = regionNumber * 1000 + 1;
                                while (existingNumbers.Contains(nextExaminerNumber))
                                {
                                    nextExaminerNumber++;
                                }
                            }

                            // Assign numbers
                            model.ExaminerNumber = nextExaminerNumber.ToString();

             
                        }
                        else
                        {
                            var maxBMS = await _context.EXAMINER_TRANSACTIONS
                                .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                            && a.EMS_PAPER_CODE == model.PaperCode
                                            && a.EMS_ACTIVITY == "BEM"
                                            && a.EMS_ECT_EXAMINER_CAT_CODE == "BMS")
                                .Select(a => a.EMS_EXAMINER_NUMBER)
                                .ToListAsync();

                            int nextExaminerNumber = maxBMS
                                .Select(code => int.TryParse(code, out int number) ? number : 0)
                                .DefaultIfEmpty(3000) // Default base, so first becomes 3001
                                .Max() + 1;

                            model.ExaminerNumber = nextExaminerNumber.ToString();
                        }

                            
                    }
                    else if (model.Category == "E")
                    {
                        if (!string.IsNullOrEmpty(model.RegionCode) || model.SubjectCode.StartsWith("7"))
                        {
                            // Parse region code (01-10)
                            int regionNumber = int.TryParse(model.RegionCode, out int num) ? num : 1;
                            regionNumber = Math.Clamp(regionNumber, 1, 10);

                            // Get all existing examiner numbers in this region that start with region code
                            var existingNumbers = await _context.EXAMINER_TRANSACTIONS
                                .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                         && a.EMS_PAPER_CODE == model.PaperCode
                                         && a.EMS_ACTIVITY == "BEM"
                                         && a.EMS_MARKING_REG_CODE == model.RegionCode
                                         && a.EMS_EXAMINER_NUMBER.StartsWith(regionNumber.ToString()))
                                .Select(a => int.Parse(a.EMS_EXAMINER_NUMBER))
                                .ToListAsync();

                            // Calculate next available number
                            int nextExaminerNumber = existingNumbers.Any()
                                ? existingNumbers.Max() + 1
                                : regionNumber * 1000 + 1; // Start with region base if no examiners exist

                            // Ensure number starts with region code
                            if (!nextExaminerNumber.ToString().StartsWith(regionNumber.ToString()))
                            {
                                nextExaminerNumber = regionNumber * 1000 + 1;
                                while (existingNumbers.Contains(nextExaminerNumber))
                                {
                                    nextExaminerNumber++;
                                }
                            }

                            // Assign numbers
                            model.ExaminerNumber = nextExaminerNumber.ToString();

                
                        }
                        else
                        {
                            var maxEorA = await _context.EXAMINER_TRANSACTIONS
                                .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                            && a.EMS_PAPER_CODE == model.PaperCode
                                            && a.EMS_ACTIVITY == "BEM"
                                            && (a.EMS_ECT_EXAMINER_CAT_CODE == "E" || a.EMS_ECT_EXAMINER_CAT_CODE == "A"))
                                .Select(a => a.EMS_EXAMINER_NUMBER)
                                .ToListAsync();

                            int nextExaminerNumber = maxEorA
                                .Select(code => int.TryParse(code, out int number) ? number : 0)
                                .DefaultIfEmpty(4000) // Ensures first will be 4001
                                .Max() + 1;

                            model.ExaminerNumber = nextExaminerNumber.ToString();
                        }

                            
                    }


                    var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                    var result = await _repository.UpdateTransactionRecord(model, currentUser);

                    if (!result.Success)
                    {
                        return BadRequest(result.Message);
                    }

                    if (result.Message == "Examiner Selected")
                    {
                        await HandleExaminerSelection(model);
                    }
                    else if (result.Message == "Examiner Removed")
                    {
                        await HandleExaminerRemoval(model);
                        await HandleTandSRemoval(model, currentUser);
                    }

                    if (model.TeamMembers.Any())
                    {
                        foreach (var member in model.TeamMembers)
                        {

                            if (member.Category == null || string.IsNullOrEmpty(member.Category))
                            {
                                member.Category = "E";

                            }

                            member.Team = model.Team;

                            if (member.Category == "RPMS")
                            {
                                if (!string.IsNullOrEmpty(model.RegionCode) || model.SubjectCode.StartsWith("7"))
                                {
                                    // Parse region code (01-10) and calculate base number
                                    int regionNumber = int.TryParse(model.RegionCode, out int num) ? num : 1;
                                    regionNumber = Math.Clamp(regionNumber, 1, 10); // Ensure 1-10

                                    // Get all existing numbers in this region
                                    var existingNumbers = await _context.EXAMINER_TRANSACTIONS
                                        .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                                    && a.EMS_PAPER_CODE == model.PaperCode
                                                    && a.EMS_ACTIVITY == "BEM"
                                                    && a.EMS_MARKING_REG_CODE == model.RegionCode
                                                    && a.EMS_EXAMINER_NUMBER.StartsWith(regionNumber.ToString()))
                                        .Select(a => int.Parse(a.EMS_EXAMINER_NUMBER))
                                        .ToListAsync();

                                    int basePmsNumber = regionNumber * 1000 + 1; // 1001, 2001,...10001
                                    int rpmsNumber;

                                    // Check if PMS exists
                                    bool pmsExists = existingNumbers.Contains(basePmsNumber);

                                    if (pmsExists)
                                    {
                                        // RPMS gets the next available number after PMS
                                        rpmsNumber = existingNumbers.Any()
                                            ? existingNumbers.Max() + 1
                                            : basePmsNumber + 1; // If PMS exists but no other numbers

                                        // Ensure we don't skip numbers (e.g., if PMS is 1001 but 1002 is taken)
                                        while (existingNumbers.Contains(rpmsNumber))
                                        {
                                            rpmsNumber++;
                                        }
                                    }
                                    else
                                    {
                                        // If no PMS, RPMS gets the first number
                                        rpmsNumber = basePmsNumber;
                                    }

                                    // Assign numbers
                                    member.ExaminerNumber = rpmsNumber.ToString();
                                
                                }

                                else
                                {
                                    var maxRPMS = await _context.EXAMINER_TRANSACTIONS
                                        .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                                    && a.EMS_PAPER_CODE == model.PaperCode
                                                    && a.EMS_ACTIVITY == "BEM"
                                                    && a.EMS_ECT_EXAMINER_CAT_CODE == "RPMS")
                                        .Select(a => a.EMS_EXAMINER_NUMBER)
                                        .ToListAsync();

                                    int nextExaminerNumber = maxRPMS
                                        .Select(code => int.TryParse(code, out int number) ? number : 0)
                                        .DefaultIfEmpty(2000) // If list is empty, use 2000, so result becomes 2001
                                        .Max() + 1;

                                    member.ExaminerNumber = nextExaminerNumber.ToString();
                                }

                            }
                            else if (member.Category == "DPMS")
                            {
                                if (!string.IsNullOrEmpty(model.RegionCode) || model.SubjectCode.StartsWith("7"))
                                {
                                    // Parse region code (01-10)
                                    int regionNumber = int.TryParse(model.RegionCode, out int num) ? num : 1;
                                    regionNumber = Math.Clamp(regionNumber, 1, 10);

                                    // Get all existing examiner numbers in this region that start with region code
                                    var existingNumbers = await _context.EXAMINER_TRANSACTIONS
                                        .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                                 && a.EMS_PAPER_CODE == model.PaperCode
                                                 && a.EMS_ACTIVITY == "BEM"
                                                 && a.EMS_MARKING_REG_CODE == model.RegionCode
                                                 && a.EMS_EXAMINER_NUMBER.StartsWith(regionNumber.ToString()))
                                        .Select(a => int.Parse(a.EMS_EXAMINER_NUMBER))
                                        .ToListAsync();

                                    // Calculate next available number
                                    int nextExaminerNumber = existingNumbers.Any()
                                        ? existingNumbers.Max() + 1
                                        : regionNumber * 1000 + 1; // Start with region base if no examiners exist

                                    // Ensure number starts with region code
                                    if (!nextExaminerNumber.ToString().StartsWith(regionNumber.ToString()))
                                    {
                                        nextExaminerNumber = regionNumber * 1000 + 1;
                                        while (existingNumbers.Contains(nextExaminerNumber))
                                        {
                                            nextExaminerNumber++;
                                        }
                                    }

                                    // Assign numbers
                                    member.ExaminerNumber = nextExaminerNumber.ToString();


                                }
                                else
                                {
                                    var maxDPMS = await _context.EXAMINER_TRANSACTIONS
                                        .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                                    && a.EMS_PAPER_CODE == model.PaperCode
                                                    && a.EMS_ACTIVITY == "BEM"
                                                    && a.EMS_ECT_EXAMINER_CAT_CODE == "DPMS")
                                        .Select(a => a.EMS_EXAMINER_NUMBER)
                                        .ToListAsync();

                                    int nextExaminerNumber = maxDPMS
                                        .Select(code => int.TryParse(code, out int number) ? number : 0)
                                        .DefaultIfEmpty(2000)
                                        .Max() + 1;

                                    member.ExaminerNumber = nextExaminerNumber.ToString();
                                }

                            }

                            else if (member.Category == "BMS")
                            {
                                if (!string.IsNullOrEmpty(model.RegionCode) || model.SubjectCode.StartsWith("7"))
                                {
                                    // Parse region code (01-10)
                                    int regionNumber = int.TryParse(model.RegionCode, out int num) ? num : 1;
                                    regionNumber = Math.Clamp(regionNumber, 1, 10);

                                    // Get all existing examiner numbers in this region that start with region code
                                    var existingNumbers = await _context.EXAMINER_TRANSACTIONS
                                        .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                                 && a.EMS_PAPER_CODE == model.PaperCode
                                                 && a.EMS_ACTIVITY == "BEM"
                                                 && a.EMS_MARKING_REG_CODE == model.RegionCode
                                                 && a.EMS_EXAMINER_NUMBER.StartsWith(regionNumber.ToString()))
                                        .Select(a => int.Parse(a.EMS_EXAMINER_NUMBER))
                                        .ToListAsync();

                                    // Calculate next available number
                                    int nextExaminerNumber = existingNumbers.Any()
                                        ? existingNumbers.Max() + 1
                                        : regionNumber * 1000 + 1; // Start with region base if no examiners exist

                                    // Ensure number starts with region code
                                    if (!nextExaminerNumber.ToString().StartsWith(regionNumber.ToString()))
                                    {
                                        nextExaminerNumber = regionNumber * 1000 + 1;
                                        while (existingNumbers.Contains(nextExaminerNumber))
                                        {
                                            nextExaminerNumber++;
                                        }
                                    }

                                    // Assign numbers
                                    member.ExaminerNumber = nextExaminerNumber.ToString();


                                }
                                else
                                {
                                    var maxBMS = await _context.EXAMINER_TRANSACTIONS
                                    .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                                && a.EMS_PAPER_CODE == model.PaperCode
                                                && a.EMS_ACTIVITY == "BEM"
                                                && a.EMS_ECT_EXAMINER_CAT_CODE == "BMS")
                                    .Select(a => a.EMS_EXAMINER_NUMBER)
                                    .ToListAsync();

                                    int nextExaminerNumber = maxBMS
                                        .Select(code => int.TryParse(code, out int number) ? number : 0)
                                        .DefaultIfEmpty(3000) // Default base, so first becomes 3001
                                        .Max() + 1;

                                    member.ExaminerNumber = nextExaminerNumber.ToString();
                                }
                            }
                            else if (member.Category == "E")
                            {
                                if (!string.IsNullOrEmpty(model.RegionCode) || model.SubjectCode.StartsWith("7"))
                                {
                                    // Parse region code (01-10)
                                    int regionNumber = int.TryParse(model.RegionCode, out int num) ? num : 1;
                                    regionNumber = Math.Clamp(regionNumber, 1, 10);

                                    // Get all existing examiner numbers in this region that start with region code
                                    var existingNumbers = await _context.EXAMINER_TRANSACTIONS
                                        .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                                 && a.EMS_PAPER_CODE == model.PaperCode
                                                 && a.EMS_ACTIVITY == "BEM"
                                                 && a.EMS_MARKING_REG_CODE == model.RegionCode
                                                 && a.EMS_EXAMINER_NUMBER.StartsWith(regionNumber.ToString()))
                                        .Select(a => int.Parse(a.EMS_EXAMINER_NUMBER))
                                        .ToListAsync();

                                    // Calculate next available number
                                    int nextExaminerNumber = existingNumbers.Any()
                                        ? existingNumbers.Max() + 1
                                        : regionNumber * 1000 + 1; // Start with region base if no examiners exist

                                    // Ensure number starts with region code
                                    if (!nextExaminerNumber.ToString().StartsWith(regionNumber.ToString()))
                                    {
                                        nextExaminerNumber = regionNumber * 1000 + 1;
                                        while (existingNumbers.Contains(nextExaminerNumber))
                                        {
                                            nextExaminerNumber++;
                                        }
                                    }

                                    // Assign numbers
                                    member.ExaminerNumber = nextExaminerNumber.ToString();


                                }
                                else
                                {
                                    var maxEorA = await _context.EXAMINER_TRANSACTIONS
                                    .Where(a => a.EMS_SUB_SUB_ID == model.ExamCode + model.SubjectCode
                                                && a.EMS_PAPER_CODE == model.PaperCode
                                                && a.EMS_ACTIVITY == "BEM"
                                                && (a.EMS_ECT_EXAMINER_CAT_CODE == "E" ||
                                                    a.EMS_ECT_EXAMINER_CAT_CODE == "A"))
                                    .Select(a => a.EMS_EXAMINER_NUMBER)
                                    .ToListAsync();

                                    int nextExaminerNumber = maxEorA
                                        .Select(code => int.TryParse(code, out int number) ? number : 0)
                                        .DefaultIfEmpty(4000) // Ensures first will be 4001
                                        .Max() + 1;

                                    member.ExaminerNumber = nextExaminerNumber.ToString();
                                }
                            }

                            var newModel = new ExaminerUpdateModel
                            {
                                IdNumber = member.IdNumber,
                                ExaminerNumber = member.ExaminerNumber,
                                Team = member.Team,
                                CapturingRole = member.CapturingRole,

                                Category = member.Category,
                                IsSelected = true,
                                ExamCode = model.ExamCode,
                                SubjectCode = model.SubjectCode,
                                PaperCode = model.PaperCode,
                                Activity = model.Activity,
                            };

                            var results = await _repository.UpdateTransactionRecord(newModel, currentUser);

                            if (!results.Success)
                            {
                                return BadRequest(result.Message);
                            }

                            if (results.Message == "Examiner Selected")
                            {
                                await HandleExaminerSelection(newModel);
                            }
                            else if (results.Message == "Examiner Removed")
                            {
                                await HandleExaminerRemoval(newModel);
                                await HandleTandSRemoval(newModel, currentUser);
                            }
                        }

                    }

                }
                else
                {

                    model.Team = "1001";
                    model.Category = "E";
                    model.ExaminerNumber = "1001";
                    model.CapturingRole = "N/A";


                    var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                    var result = await _repository.UpdateTransactionRecord(model, currentUser);

                    if (!result.Success)
                    {
                        return BadRequest(result.Message);
                    }

                    if (result.Message == "Examiner Selected")
                    {
                        await HandleExaminerSelection(model);
                    }
                    else if (result.Message == "Examiner Removed")
                    {
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

        private async Task HandleExaminerSelection(ExaminerUpdateModel model)
        {
            var subkey = $"{model.ExamCode}{model.SubjectCode}{model.PaperCode}{model.Activity}{model.IdNumber}";
            var examiner = await _examinerRepository.GetExaminerRecord(model.IdNumber);

            if (examiner == null) return;


            var user = await _userRepository.GetUser(model.IdNumber, subkey);
            model.Category ??= "E"; // Default category

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
        public async Task<IActionResult> UpdateCategory(string idNumber, string category, string examCode,
            string subjectCode, string paperCode, string regionCode, string activity)
        {
            if (string.IsNullOrWhiteSpace(idNumber) || string.IsNullOrWhiteSpace(category))
            {
                return BadRequest("Invalid data provided.");
            }

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var result = await _repository.UpdateCategory(idNumber, category, examCode, subjectCode, paperCode,
                regionCode, activity, currentUser.Id);

            if (result.Success)
            {
                var subKey = examCode + subjectCode + paperCode + activity + idNumber;
                var user = await _userRepository.GetUser(idNumber, subKey);

                if (user != null)
                {

                    var results = await _userManager.UpdateAsync(user);

                    if (category == "E")
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
                    else if (category == "A")
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
                    else if (category == "BT")
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
                    else if (category == "PBT")
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
                    else if (category == "PMS")
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
                    else if (category == "BMS")
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
                    else if (category == "DPMS")
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
                    else if (category == "RPMS")
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
                return BadRequest(new { message = result.Message });
            }

            return Ok();

        }

        [HttpPost]
        public async Task<IActionResult> UpdateCapturingRole(string idNumber, string capturingRole, string examCode,
            string subjectCode, string paperCode, string regionCode, string activity)
        {
            if (string.IsNullOrWhiteSpace(idNumber) || string.IsNullOrWhiteSpace(capturingRole))
            {
                return BadRequest("Invalid data provided.");
            }

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var result = await _repository.UpdateCategory(idNumber, capturingRole, examCode, subjectCode, paperCode,
                regionCode, activity, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok();


        }

        [HttpPost]
        public async Task<IActionResult> UpdateExaminerNumber(string idNumber, string examinerNumber,
            string examCode, string subjectCode,
            string paperCode, string regionCode,
            string activity)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var result = await _repository.UpdateExaminerNumber(idNumber, examinerNumber, examCode, subjectCode,
                paperCode, regionCode, activity, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok();




        }

        [HttpPost]
        public async Task<IActionResult> UpdateTeam(string idNumber, string team,
            string examCode, string subjectCode,
            string paperCode, string regionCode,
            string activity)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var result = await _repository.UpdateTeam(idNumber, team, examCode, subjectCode, paperCode, regionCode,
                activity, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok();



        }


        [HttpPost]
        public async Task<IActionResult> SaveApportionment(ApportionScriptsViewModel model, string examCode,
            string subjectCode,
            string paperCode, string regionCode,
            string activity)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

            var result = await _repository.SaveApportionment(model, examCode, subjectCode, paperCode, regionCode,
                activity, currentUser.Id);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }


            return RedirectToAction("ApportionedScripts");
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

        public async Task<IActionResult> ExportTeamStructure(string examCode, string subjectCode, string paperCode,
            string regionCode, string activity)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(examCode) || string.IsNullOrEmpty(subjectCode))
                {
                    return BadRequest("Exam code and subject code are required");
                }

                // Create separate contexts for parallel operations
                var teamData =
                    await _repository.GetFromMasterAsync(examCode, subjectCode, paperCode, regionCode, activity);
                if (teamData == null || !teamData.Any())
                {
                    return NotFound("No examiner data found for the specified criteria");
                }

                // Sequential execution instead of parallel to avoid context conflicts
                var session = await _examCodesRepository.GetExamCodesById(examCode);
                if (session == null)
                {
                    return NotFound("Session data not found");
                }

                var subjectDTO = await _subjectsRepository.GetSubjectCode(subjectCode);
                if (subjectDTO == null)
                {
                    return NotFound("Subject data not found");
                }

                // Transform data
                var data = teamData.Select(t => new SelectTeamViewModel
                {
                    ExaminerNumber = "",
                    Name = $"{t.EMS_EXAMINER_NAME?.Trim()} {t.EMS_LAST_NAME?.Trim()}".Trim(),
                    Team = t.EMS_EXM_SUPERORD,
                    IdNumber = t.EMS_NATIONAL_ID ?? "",
                    Sex = t.EMS_SEX ?? "",
                    Status = t.EMS_ECT_EXAMINER_CAT_CODE,
                    Station = t.EMS_WORK_ADD1 ?? "",
                    District = t.EMS_WORK_ADD2 ?? "",
                    Province = t.EMS_WORK_ADD3 ?? "",
                    Phone = t.EMS_PHONE_HOME ?? "",
                    Region = t.EMS_MARKING_REG_CODE ?? "",
                    CapturingRole = ""
                }).ToList();

                // Generate Excel
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Team Structure");

                // Set headers
                SetDocumentHeaders(worksheet, subjectCode, paperCode,
                    subjectDTO.SUB_SUBJECT_DESC ?? subjectCode,
                    session.EXM_EXAM_LEVEL ?? "",
                    session.EXM_EXAM_SESSION ?? DateTime.Now.Month.ToString());

                // Set column headers
                SetColumnHeaders(worksheet);

                // Add data rows
                for (int i = 0; i < data.Count; i++)
                {
                    AddDataRowSafe(worksheet, i + 6, data[i]);
                }

                // Apply validations if data exists
                if (data.Count > 0)
                {
                    if (!string.IsNullOrEmpty(regionCode))
                    {
                        ApplyValidationsSafeGrade7(worksheet, data.Count, regionCode);
                    }
                    else
                    {
                        ApplyValidationsSafe(worksheet, data.Count);
                    }


                }

                // Final formatting
                worksheet.Columns().AdjustToContents();
                worksheet.SheetView.FreezeRows(5);

                // Return file
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(),
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      string.IsNullOrEmpty(regionCode)
          ? $"{subjectCode}_{paperCode}_TeamStructure_New_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
          : $"{subjectCode}_{paperCode}_{regionCode}_TeamStructure_New_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An error occurred while generating the team structure file. Please try again.");
            }
        }


        private void SetDocumentHeaders(IXLWorksheet worksheet, string subjectCode, string paperCode,
            string subjectName, string level, string sessionMon)
        {
            worksheet.Cell("A1").Value = "ZIMBABWE SCHOOL EXAMINATION COUNCIL";
            worksheet.Cell("A3").Value = $"{subjectCode}/{paperCode} {subjectName} TEAM STRUCTURE";
            worksheet.Cell("B3").Value = $"{subjectName} {level} {sessionMon} {DateTime.Now.Year} TEAM STRUCTURE";

            worksheet.Range("A1:H1").Merge().Style.Font.SetBold().Font.FontSize = 14;
            worksheet.Range("A3:H3").Merge().Style.Font.SetBold().Font.FontSize = 12;
        }

        private void SetColumnHeaders(IXLWorksheet worksheet)
        {
            var headers = new[]
            {
                "EX.NO", "NAME", "NATIONALID", "SEX", "STATUS", "CAPTURING", "TEAM", "REGION", "STATION", "DISTRICT",
                "PROVINCE", "CELL NO"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(5, i + 1).Value = headers[i];
                worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                worksheet.Cell(5, i + 1).Style.Alignment.WrapText = true;
            }
        }

        private void AddDataRowSafe(IXLWorksheet worksheet, int rowNum, SelectTeamViewModel data)
        {
            try
            {
                // Simplified formula that's more reliable
                worksheet.Cell(rowNum, 1).Value = ""; // Will be set by validation

                // Basic data cells
                worksheet.Cell(rowNum, 2).Value = data.Name;
                worksheet.Cell(rowNum, 3).Value = data.IdNumber;
                worksheet.Cell(rowNum, 4).Value = data.Sex;
                worksheet.Cell(rowNum, 5).Value = data.Status;
                worksheet.Cell(rowNum, 6).Value = data.CapturingRole;
                worksheet.Cell(rowNum, 7).Value = data.Team;
                worksheet.Cell(rowNum, 8).Value = data.Region;
                worksheet.Cell(rowNum, 9).Value = data.Station;
                worksheet.Cell(rowNum, 10).Value = data.District;
                worksheet.Cell(rowNum, 11).Value = data.Province;
                worksheet.Cell(rowNum, 12).Value = data.Phone;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void ApplyValidationsSafeGrade7(IXLWorksheet worksheet, int rowCount, string regionCode)
        {
            try
            {
                var firstRow = 6;
                var lastRow = firstRow + rowCount - 1;

                // Create validation sheet
                var validationSheet = worksheet.Workbook.Worksheets.Add("ValidationData");
                validationSheet.Hide();

                // 1. Status validation (Column E)
                var statusValues = new[] { "PMS", "RPMS", "DPMS", "BMS", "E" };
                for (int i = 0; i < statusValues.Length; i++)
                {
                    validationSheet.Cell(i + 1, 1).Value = statusValues[i];
                }

                worksheet.Range(firstRow, 5, lastRow, 5)
                    .SetDataValidation()
                    .List(validationSheet.Range(1, 1, statusValues.Length, 1));

                // 2. Capturing role validation (Column F)
                var capturingValues = new[] { "V", "C" };
                for (int i = 0; i < capturingValues.Length; i++)
                {
                    validationSheet.Cell(i + 1, 2).Value = capturingValues[i];
                }

                worksheet.Range(firstRow, 6, lastRow, 6)
                    .SetDataValidation()
                    .List(validationSheet.Range(1, 2, capturingValues.Length, 2));

                // 3. Region validation (Column H)
                var regionValues = Enumerable.Range(1, 10).Select(i => i.ToString("00")).ToArray();
                for (int i = 0; i < regionValues.Length; i++)
                {
                    validationSheet.Cell(i + 1, 3).Value = regionValues[i];
                }

                worksheet.Range(firstRow, 8, lastRow, 8) // Changed from 4 to 8 for column H
                    .SetDataValidation()
                    .List(validationSheet.Range(1, 3, regionValues.Length, 3));

                // Initialize counters for all regions
                Dictionary<string, int> regionExaminerCounters = new Dictionary<string, int>();
                Dictionary<string, int> regionTeamCounters = new Dictionary<string, int>();
                for (int region = 1; region <= 10; region++)
                {
                    string regionKey = region.ToString("00");
                    regionExaminerCounters[regionKey] = 1;
                    regionTeamCounters[regionKey] = 1;
                }

                // Process each row
                for (int i = firstRow; i <= lastRow; i++)
                {
                    // Get region from column H (8) instead of D (4)
                    string region = worksheet.Cell(i, 8).Value.ToString();
                    string status = worksheet.Cell(i, 5).Value.ToString();

                    // Validate and format region
                    if (!int.TryParse(region, out int regionNumber) || regionNumber < 1 || regionNumber > 10)
                    {
                        regionNumber = int.Parse(regionCode);
                        region = regionNumber.ToString("00");
                        worksheet.Cell(i, 8).Value = region; // Update column H (8)
                    }
                    else
                    {
                        region = regionNumber.ToString("00");
                    }

                    // Calculate base numbers for this region
                    int regionBase = regionNumber * 1000;

                    // Assign examiner number (2001, 2002, etc. for region 02)
                    worksheet.Cell(i, 1).Value = regionBase + regionExaminerCounters[region]++;

                    // Assign team number based on status
                    int teamNumber;
                    if (status == "PMS" || status == "RPMS" || status == "DPMS")
                    {
                        teamNumber = regionBase + 1;
                    }
                    else if (status == "BMS")
                    {
                        teamNumber = regionBase + regionTeamCounters[region]++;
                    }
                    else // E
                    {
                        teamNumber = GetSupervisingBMSTeamNumber(worksheet, i, firstRow);
                    }

                    worksheet.Cell(i, 7).Value = teamNumber;

                    // Lock non-E team cells

                    worksheet.Cell(i, 7).Style.Protection.Locked = true;
                    worksheet.Cell(i, 7).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(i, 1).Style.Font.FontColor = XLColor.Blue;

                    worksheet.Cell(i, 5).Style.Font.FontColor = XLColor.Purple;
                    worksheet.Cell(i, 6).Style.Font.FontColor = XLColor.DarkBrown;

                }

                // Create region-specific team lists
                CreateRegionTeamLists(worksheet, validationSheet);

                // Apply team number validations
                for (int i = firstRow; i <= lastRow; i++)
                {
                    string region = worksheet.Cell(i, 8).Value.ToString(); // Column H (8)
                    var teamCell = worksheet.Cell(i, 7);

                    if (worksheet.Cell(i, 5).Value.ToString() != "E")
                    {
                        teamCell.SetDataValidation().List($"=Region{region}_Teams");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ApplyValidationsSafe: {ex.Message}");
                throw;
            }
        }

        private int GetSupervisingBMSTeamNumber(IXLWorksheet worksheet, int currentRow, int firstRow)
        {
            // Get region from column H (8)
            string currentRegion = worksheet.Cell(currentRow, 8).Value.ToString();
            if (!int.TryParse(currentRegion, out int regionNumber))
            {
                regionNumber = 2; // Default to region 02
            }

            // Find most recent BMS in same region above current row
            for (int i = currentRow - 1; i >= firstRow; i--)
            {
                if (worksheet.Cell(i, 5).Value.ToString() == "BMS" &&
                    worksheet.Cell(i, 8).Value.ToString() == currentRegion) // Column H (8)
                {
                    if (int.TryParse(worksheet.Cell(i, 7).Value.ToString(), out int teamNumber))
                    {
                        return teamNumber;
                    }
                }
            }

            return regionNumber * 1000 + 1;
        }

        private void CreateRegionTeamLists(IXLWorksheet worksheet, IXLWorksheet validationSheet)
        {
            int validationRow = 1;

            for (int region = 1; region <= 10; region++)
            {
                int regionBase = region * 1000;
                string regionKey = region.ToString("00");

                // Create team list for this region (1001-1999, 2001-2999, etc.)
                for (int i = 1; i <= 999; i++)
                {
                    validationSheet.Cell(validationRow, 4).Value = (regionBase + i).ToString();
                    validationRow++;
                }

                // Create named range for this region's teams
                var range = validationSheet.Range(
                    validationSheet.Cell(validationRow - 999, 4),
                    validationSheet.Cell(validationRow - 1, 4));

                worksheet.DefinedNames.Add($"Region{regionKey}_Teams", range);
            }
        }



        private void ApplyValidationsSafe(IXLWorksheet worksheet, int rowCount)
        {
            try
            {
                var firstRow = 6;
                var lastRow = firstRow + rowCount - 1;

                // Create validation sheet
                var validationSheet = worksheet.Workbook.Worksheets.Add("ValidationData");
                validationSheet.Hide();

                // 1. Status validation (Column E)
                var statusValues = new[] { "PMS", "RPMS", "DPMS", "BMS", "E" };
                for (int i = 0; i < statusValues.Length; i++)
                {
                    validationSheet.Cell(i + 1, 1).Value = statusValues[i];
                }

                worksheet.Range(firstRow, 5, lastRow, 5)
                    .SetDataValidation()
                    .List(validationSheet.Range(1, 1, statusValues.Length, 1));

                // 2. Capturing role validation (Column F)
                var capturingValues = new[] { "V", "C" };
                for (int i = 0; i < capturingValues.Length; i++)
                {
                    validationSheet.Cell(i + 1, 2).Value = capturingValues[i];
                }

                worksheet.Range(firstRow, 6, lastRow, 6)
                    .SetDataValidation()
                    .List(validationSheet.Range(1, 2, capturingValues.Length, 2));

                // 3. Auto-generate examiner numbers (Column A)
                for (int i = firstRow; i <= lastRow; i++)
                {
                    worksheet.Cell(i, 1).FormulaA1 =
                        $"IF(E{i}=\"PMS\",\"1001\"," +
                        $"IF(E{i}=\"RPMS\",2000+COUNTIF($E$6:$E${i},\"RPMS\")," +
                        $"IF(E{i}=\"DPMS\",2000+COUNTIF($E$6:$E${i},\"RPMS\")+COUNTIF($E$6:$E${i},\"DPMS\")," +
                        $"IF(E{i}=\"BMS\",3000+COUNTIF($E$6:$E${i},\"BMS\")," +
                        $"IF(E{i}=\"E\",4000+COUNTIF($E$6:$E${i},\"E\"),\"\")))))";
                }

                // 4. Team number validation (Column G)
                // First set default value (1001) for all rows
                for (int i = firstRow; i <= lastRow; i++)
                {
                    worksheet.Cell(i, 7).Value = "1001"; // Default team
                }

                // Create validation ranges on the validation sheet
                int validationRow = 1;

                // PMS Teams (only 1001)
                validationSheet.Cell(validationRow++, 3).Value = "1001";
                var pmsRange = validationSheet.Range(1, 3, 1, 3);
                worksheet.Workbook.DefinedNames.Add("PMS_Teams", pmsRange);

                // RPMS Teams (only 1001)
                validationSheet.Cell(validationRow++, 3).Value = "1001";
                var rpmsRange = validationSheet.Range(1, 3, 1, 3);
                worksheet.Workbook.DefinedNames.Add("RPMS_Teams", rpmsRange);

                // DPMS Teams (only 1001)
                validationSheet.Cell(validationRow++, 3).Value = "1001";
                var dpmsRange = validationSheet.Range(1, 3, 1, 3);
                worksheet.Workbook.DefinedNames.Add("DPMS_Teams", dpmsRange);

                // BMS Teams (1001 + 2001-2999)
                validationRow = 1; // Reset row counter
                validationSheet.Cell(validationRow++, 4).Value = "1001";
                for (int i = 2001; i <= 2999; i++)
                {
                    validationSheet.Cell(validationRow++, 4).Value = i.ToString();
                }

                var bmsRange = validationSheet.Range(1, 4, 1000, 4);
                worksheet.Workbook.DefinedNames.Add("BMS_Teams", bmsRange);

                // E Teams (1001 + 3001-3999)
                validationRow = 1; // Reset row counter
                validationSheet.Cell(validationRow++, 5).Value = "1001";
                for (int i = 3001; i <= 3999; i++)
                {
                    validationSheet.Cell(validationRow++, 5).Value = i.ToString();
                }

                var eRange = validationSheet.Range(1, 5, 1000, 5);
                worksheet.Workbook.DefinedNames.Add("E_Teams", eRange);

                // Apply data validation
                for (int i = firstRow; i <= lastRow; i++)
                {
                    var status = worksheet.Cell(i, 5).Value.ToString();
                    var teamCell = worksheet.Cell(i, 7);
                    var validation = teamCell.GetDataValidation();
                    validation.Clear();

                    // Set validation based on status
                    validation.List(
                        $"=INDIRECT({worksheet.Cell(i, 5).Address.ToStringFixed(XLReferenceStyle.A1)} & \"_Teams\")");

                    // Lock non-E team cells and set their values
                    worksheet.Cell(i, 7).Style.Protection.Locked = true;
                    worksheet.Cell(i, 7).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(i, 1).Style.Font.FontColor = XLColor.Blue;

                    worksheet.Cell(i, 5).Style.Font.FontColor = XLColor.Purple;
                    worksheet.Cell(i, 6).Style.Font.FontColor = XLColor.DarkBrown;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> ExportSelectedTeamStructure(string examCode, string subjectCode,
            string paperCode, string regionCode, string activity)
        {
            var teamData =
                await _repository.GetSelectedTeamsFromTransactionAsync(examCode, subjectCode, paperCode, regionCode,
                    activity);

            var data = teamData.Select(t => new SelectTeamViewModel
            {
                ExaminerNumber = t.EMS_EXAMINER_NUMBER,
                Name = t.Examiner.EMS_EXAMINER_NAME + " " + t.Examiner.EMS_LAST_NAME,
                Team = t.EMS_EXM_SUPERORD,
                IdNumber = t.EMS_NATIONAL_ID,
                Sex = t.Examiner.EMS_SEX,
                Status = t.EMS_ECT_EXAMINER_CAT_CODE,
                Station = t.Examiner.EMS_WORK_ADD1,
                District = t.Examiner.EMS_WORK_ADD2,
                Province = t.Examiner.EMS_WORK_ADD3,
                Phone = t.Examiner.EMS_PHONE_HOME,
                Region = t.EMS_MARKING_REG_CODE,
                CapturingRole = t.EMS_CAPTURINGROLE
            }).ToList();


            var sessionMon = DateTime.Now.Month.ToString();
            var currentYear = DateTime.Now.Year.ToString();
            var level = "";
            var subjectName = subjectCode;
            var session = await _examCodesRepository.GetExamCodesById(examCode);
            var subjectDTO = await _subjectsRepository.GetSubjectCode(subjectCode);
            if (sessionMon != null)
            {
                sessionMon = session.EXM_EXAM_SESSION;
                currentYear = session.EXM_EXAM_YEAR;
                level = session.EXM_EXAM_LEVEL;
            }

            if (subjectName != null)
            {
                subjectName = subjectDTO.SUB_SUBJECT_DESC;
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Team Structure");

            // Custom Headers (Rows 1 to 3)
            worksheet.Cell("A1").Value = "ZIMBABWE SCHOOL EXAMINATION COUNCIL";
            worksheet.Cell("A3").Value = $"{subjectCode}/{paperCode} {subjectName} TEAM STRUCTURE";
            worksheet.Cell("B3").Value = $"{subjectName} {level} {sessionMon} {DateTime.Now.Year} TEAM STRUCTURE";

            worksheet.Range("A1:H1").Merge().Style.Font.SetBold().Font.FontSize = 14;
            worksheet.Range("A3:H3").Merge().Style.Font.SetBold().Font.FontSize = 12;

            // Column Headers - Row 5
            worksheet.Cell("A5").Value = "EX.NO";
            worksheet.Cell("B5").Value = "NAME";
            worksheet.Cell("C5").Value = "NATIONALID";
            worksheet.Cell("D5").Value = "SEX";
            worksheet.Cell("E5").Value = "STATUS";
            worksheet.Cell("F5").Value = "CAPTURING";
            worksheet.Cell("G5").Value = "TEAM";
            worksheet.Cell("H5").Value = "REGION";
            worksheet.Cell("I5").Value = "STATION";
            worksheet.Cell("J5").Value = "DISTRICT";
            worksheet.Cell("K5").Value = "PROVINCE";
            worksheet.Cell("L5").Value = "CELL NO";




            int row = 6;

            // First list all PMS examiners
            var pmsExaminers = data.Where(x => x.Status == "PMS").OrderBy(x => x.ExaminerNumber).ToList();
            if (pmsExaminers.Any())
            {
                worksheet.Cell(row, 1).Value = "PMS EXAMINERS";
                worksheet.Range(row, 1, row, 12).Merge().Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightBlue;
                row++;

                foreach (var member in pmsExaminers)
                {
                    AddMemberRow(worksheet, row, member);
                    row++;
                }

                row += 2; // Add two empty rows after section
            }

            // Then list all DPMS examiners
            var dpmsExaminers = data.Where(x => x.Status == "DPMS").OrderBy(x => x.ExaminerNumber).ToList();
            if (dpmsExaminers.Any())
            {
                worksheet.Cell(row, 1).Value = "DPMS EXAMINERS";
                worksheet.Range(row, 1, row, 12).Merge().Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightBlue;
                row++;

                foreach (var member in dpmsExaminers)
                {
                    AddMemberRow(worksheet, row, member);
                    row++;
                }

                row += 2; // Add two empty rows after section
            }

            // Then list all BMS examiners in their own section
            var bmsExaminers = data.Where(x => x.Status == "BMS")
                .OrderBy(x => x.ExaminerNumber)
                .ToList();

            if (bmsExaminers.Any())
            {
                worksheet.Cell(row, 1).Value = "BMS EXAMINERS";
                worksheet.Range(row, 1, row, 12).Merge().Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightBlue;
                row++;

                foreach (var bms in bmsExaminers)
                {
                    AddMemberRow(worksheet, row, bms);
                    row++;
                }

                row += 2; // Add two empty rows after section
            }

            // TEAM STRUCTURE SECTION - DPMS with their BMS first
            worksheet.Cell(row, 1).Value = "DPMS WITH THEIR BMS";
            worksheet.Range(row, 1, row, 12).Merge().Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightGreen;
            row += 2;

            foreach (var dpms in dpmsExaminers)
            {
                // Add DPMS row
                AddMemberRow(worksheet, row, dpms);
                row++;

                // Find all BMS under this DPMS
                var bmsUnderDpms = bmsExaminers.Where(b => b.Team == dpms.ExaminerNumber)
                    .OrderBy(b => b.ExaminerNumber)
                    .ToList();

                // Add BMS under this DPMS
                foreach (var bms in bmsUnderDpms)
                {
                    AddMemberRow(worksheet, row, bms);
                    row++;
                }

                // Add empty row between DPMS teams
                if (dpms != dpmsExaminers.Last())
                {
                    row += 2;
                }
            }

            // TEAM STRUCTURE SECTION - BMS with their E next
            worksheet.Cell(row, 1).Value = "BMS WITH THEIR E";
            worksheet.Range(row, 1, row, 12).Merge().Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightGreen;
            row += 2;

            foreach (var bms in bmsExaminers)
            {
                // Add BMS row
                AddMemberRow(worksheet, row, bms);
                row++;

                // Find all E under this BMS
                var eUnderBms = data.Where(a => a.Team == bms.ExaminerNumber)
                    .OrderBy(a => a.ExaminerNumber)
                    .ToList();

                // Add E under this BMS
                foreach (var e in eUnderBms)
                {
                    AddMemberRow(worksheet, row, e);
                    row++;
                }

                // Add empty row between BMS teams
                if (bms != bmsExaminers.Last())
                {
                    row += 2;
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            var currentDateTime = DateTime.Now.ToString("yyyyMMdd_HHmm");

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"TeamStructure_{subjectCode}_{paperCode}_{currentDateTime}.xlsx");
        }

        private void AddMemberRow(IXLWorksheet worksheet, int row, SelectTeamViewModel member)
        {
            worksheet.Cell(row, 1).Value = member.ExaminerNumber;
            worksheet.Cell(row, 2).Value = member.Name;
            worksheet.Cell(row, 3).Value = member.IdNumber;
            worksheet.Cell(row, 4).Value = member.Sex;
            worksheet.Cell(row, 5).Value = member.Status;
            worksheet.Cell(row, 6).Value = member.CapturingRole;
            worksheet.Cell(row, 7).Value = member.Team;
            worksheet.Cell(row, 8).Value = member.Region;
            worksheet.Cell(row, 9).Value = member.Station;
            worksheet.Cell(row, 10).Value = member.District;
            worksheet.Cell(row, 11).Value = member.Province;
            worksheet.Cell(row, 12).Value = member.Phone;
        }
        
        
        public List<TeamMember> FlattenTeamHierarchy(TeamHierarchy hierarchy)
        {
            var members = new List<TeamMember>();
    
            // Add PMS if exists
            if (hierarchy.PMS != null)
            {
                members.Add(hierarchy.PMS);
            }
    
            // Add RPMS if exists
            if (hierarchy.RPMS != null)
            {
                members.Add(hierarchy.RPMS);
            }
    
            // Process all DPMS and their subordinates
            foreach (var dpmsNode in hierarchy.DPMS)
            {
                // Add the DPMS
                members.Add(dpmsNode.Member);
        
                // Process all subordinates of this DPMS
                foreach (var subordinate in dpmsNode.Subordinates)
                {
                    // Add BMS or E directly under DPMS
                    members.Add(subordinate.Member);
            
                    // If this is a BMS, add its Examiners
                    if (subordinate.Member.Status == "BMS")
                    {
                        foreach (var examinerNode in subordinate.Subordinates)
                        {
                            members.Add(examinerNode.Member);
                        }
                    }
                }
            }
    
            return members;
        }


        [HttpPost]
        public async Task<IActionResult> UploadTeamStructure(string examCode, string subjectCode, string paperCode,
            string regionCode, string activity, IFormFile file)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction("Index");
            }

            if (!System.IO.Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only Excel files (.xlsx) are allowed.";
                return RedirectToAction("Index");
            }



            try
            {
                var removeData = await _repository.ClearTeam(examCode, subjectCode, paperCode, regionCode, activity,
                    currentUser.Id);
                List<TeamMember> allTeamMembers = new List<TeamMember>();
                var teamHierarchy = new TeamHierarchy();
                var updatedData = new List<TeamMemberImportModel>();
                if (removeData.Success)
                {
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        stream.Position = 0; // Reset the stream position to the beginning
                        bool isNewFormat = file.FileName.Contains("_New_");

                        if (isNewFormat)
                        {
                            allTeamMembers = ProcessTeamStructure(stream);

                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(regionCode) || subjectCode.StartsWith("7"))
                            {
                                teamHierarchy = ProcessTeamStructureGrade7(stream, regionCode);
                                // Create a list to hold all team members


                                // Add PMS if exists
                                if (teamHierarchy.PMS != null)
                                {
                                    allTeamMembers.Add(teamHierarchy.PMS);
                                }

                                // Add RPMS if exists
                                if (teamHierarchy.RPMS != null)
                                {
                                    allTeamMembers.Add(teamHierarchy.RPMS);
                                }

                                // Process all DPMS and their hierarchies
                                foreach (var dpmsNode in teamHierarchy.DPMS)
                                {
                                    // Add the DPMS
                                    allTeamMembers.Add(dpmsNode.Member);

                                    // Process all subordinates of this DPMS
                                    foreach (var subordinate in dpmsNode.Subordinates)
                                    {
                                        // Add BMS or E directly under DPMS
                                        allTeamMembers.Add(subordinate.Member);

                                        // If this is a BMS, add its Examiners
                                        if (subordinate.Member.Status == "BMS")
                                        {
                                            foreach (var examiner in subordinate.Subordinates)
                                            {
                                                allTeamMembers.Add(examiner.Member);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                teamHierarchy = ProcessTeamStructureOAOLD(stream);
                                // Create a list to hold all team members


                                // Add PMS if exists
                                if (teamHierarchy.PMS != null)
                                {
                                    allTeamMembers.Add(teamHierarchy.PMS);
                                }

                                // Add RPMS if exists
                                if (teamHierarchy.RPMS != null)
                                {
                                    allTeamMembers.Add(teamHierarchy.RPMS);
                                }

                                // Process all DPMS and their hierarchies
                                foreach (var dpmsNode in teamHierarchy.DPMS)
                                {
                                    // Add the DPMS
                                    allTeamMembers.Add(dpmsNode.Member);

                                    // Process all subordinates of this DPMS
                                    foreach (var subordinate in dpmsNode.Subordinates)
                                    {
                                        // Add BMS or E directly under DPMS
                                        allTeamMembers.Add(subordinate.Member);

                                        // If this is a BMS, add its Examiners
                                        if (subordinate.Member.Status == "BMS")
                                        {
                                            foreach (var examiner in subordinate.Subordinates)
                                            {
                                                allTeamMembers.Add(examiner.Member);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                        foreach (var member in allTeamMembers)
                        {
                            var model = new TeamMemberImportModel
                            {
                                ExaminerNumber = member.ExaminerNumber,
                                Name = member.Name,
                                IdNumber = member.IdNumber,
                                Sex = member.Sex,
                                Status = member.Status.ToUpper(), // Ensure uppercase
                                Capturing = member.Capturing,
                                Team = member.Team,
                                Region = member.Region,
                                Station = member.Station,
                                District = member.District,
                                Province = member.Province,
                                Phone = member.Phone,
                            };

                            updatedData.Add(model);
                        }


                        updatedData = updatedData.DistinctBy(a => a.IdNumber).ToList();




                        var validationMasterResult = await ValidateMasterData(updatedData,subjectCode,paperCode);
                        if (!validationMasterResult.IsValid)
                        {
                            TempData["ErrorMessage"] = validationMasterResult.ErrorMessage;
                            return RedirectToAction("SelectTeam");
                        }

                        foreach (var item in updatedData)
                        {
                            if (string.IsNullOrEmpty(item.Status))
                            {
                                item.Status = "E"; // Default to Examiner if missing
                            }
                        }

                    if (!string.IsNullOrEmpty(regionCode) && subjectCode.StartsWith("7"))
                    {

                        if (!updatedData.Any(x => x.Status == "PMS"))
                        {
                            if (!updatedData.Any(x => x.Status == "RPMS"))
                            {
                                TempData["ErrorMessage"] = "No PMS or RPMS examiners found in upload file";
                                return RedirectToAction("SelectTeam");
                            }
                        }

                    }
                    else
                    {
                        if (!updatedData.Any(x => x.Status == "PMS"))
                        {
                            TempData["ErrorMessage"] = "No PMS examiners found in upload file";
                            return RedirectToAction("SelectTeam");
                        }
                    }
                        var validationTeamResult = ValidateTeamDuplicates(updatedData);
                        if (!validationTeamResult.IsValid)
                        {
                            TempData["ErrorMessage"] = validationTeamResult.ErrorMessage;
                            return RedirectToAction("SelectTeam");
                        }


                        if (!string.IsNullOrEmpty(regionCode) && subjectCode.StartsWith("7"))
                        {
                        foreach (var item in updatedData)
                        {
                            if(item.Region == null || string.IsNullOrEmpty(item.Region))
                            {
                                item.Region = regionCode;
                            }
                        }
                            var validationRegionResult = ValidateRegionTeamBeltsData(updatedData, regionCode);
                            if (!validationRegionResult.IsValid)
                            {
                                TempData["ErrorMessage"] = validationRegionResult.ErrorMessage;
                                return RedirectToAction("SelectTeam");
                            }
                        }
                        else
                        {
                            var validationRegionResult = ValidateTeamBeltsData(updatedData);
                            if (!validationRegionResult.IsValid)
                            {
                                TempData["ErrorMessage"] = validationRegionResult.ErrorMessage;
                                return RedirectToAction("SelectTeam");
                            }
                        }

                        var validationCapturingResult = ValidateCapturingTeamBeltsData(updatedData);
                        if (!validationCapturingResult.IsValid)
                        {
                            TempData["ErrorMessage"] = validationCapturingResult.ErrorMessage;
                            return RedirectToAction("SelectTeam");
                        }

                        var result = await _repository.ProcessTeamUpdates(updatedData, examCode, subjectCode,
                            paperCode, regionCode, activity, currentUser);

                        if (result.Success)
                        {
                            // 1. Preload all examiners in one query
                            var allIdNumbers = updatedData.Select(x => x.IdNumber).Distinct().ToList();
                            var allExaminers = await _examinerRepository.GetExaminersByIdNumbers(allIdNumbers);
                            var examinerDictionary = allExaminers.ToDictionary(x => x.EMS_NATIONAL_ID);

                            // 2. Preload existing users
                            var allSubkeys = updatedData
                                .Select(x => examCode + subjectCode + paperCode + activity + x.IdNumber).Distinct();
                            var existingUsers = await _examinerRepository.GetUsersBySubkeys(allSubkeys);
                            var existingUserSubkeys = new HashSet<string>(existingUsers.Select(u => u.EMS_SUBKEY));

                            // Get all existing usernames upfront
                            var allExistingUsernames = new HashSet<string>(
                                await _userManager.Users.Select(u => u.UserName).ToListAsync(),
                                StringComparer.OrdinalIgnoreCase
                            );

                            // 3. Prepare new users
                            var usersToCreate = new List<ApplicationUser>();
                            var roleAssignments = new Dictionary<string, string>(); // Key: subkey, Value: role

                            foreach (var item in updatedData)
                            {
                                var subkey = examCode + subjectCode + paperCode + activity + item.IdNumber;

                                if (!existingUserSubkeys.Contains(subkey) &&
                                    examinerDictionary.TryGetValue(item.IdNumber, out var examiner))
                                {
                                    string cleanFirstName = RemoveMiddleName(examiner.EMS_EXAMINER_NAME).Trim();
                                    string cleanSurname = RemoveMiddleName(examiner.EMS_LAST_NAME).Trim();

                                    // Generate valid username
                                    string username = GenerateValidUsername(cleanFirstName, cleanSurname,
                                        allExistingUsernames, usersToCreate);

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
                                        Activity = activity,
                                        RoleId = "",
                                    };

                                    usersToCreate.Add(newUser);
                                    allExistingUsernames.Add(username); // Track the new username
                                    roleAssignments[subkey] = item.Status ?? "E";
                                }
                            }

                            // 4. Batch create users
                            if (usersToCreate.Any())
                            {
                                var createResult =
                                    await _userManager.CreateBatchAsync(usersToCreate, subjectCode, paperCode);

                                if (!createResult.Succeeded)
                                {
                                    // Handle creation errors
                                    TempData["ErrorMessage"] = "Error creating users: " +
                                                               string.Join(", ",
                                                                   createResult.Errors.Select(e => e.Description));
                                    return RedirectToAction("SelectTeam");
                                }

                                // 5. Batch assign roles
                                // 5. Batch assign roles
                                var roleGroups = usersToCreate
                                    .GroupBy(u => roleAssignments[u.EMS_SUBKEY])
                                    .Where(g => !string.IsNullOrEmpty(g.Key));

                                foreach (var group in roleGroups)
                                {
                                    // Map role codes to actual role names
                                    string roleName = group.Key switch
                                    {
                                        "E" => "Examiner",
                                        "PMS" => "PMS",
                                        "BMS" => "BMS",
                                        "DPMS" => "DPMS",
                                        "RPMS" => "RPMS",
                                        "I" => "I",
                                        "S" => "S",
                                        "A" => "A",
                                        "BT" => "BT",
                                        "PBT" => "PBT",
                                        _ => group.Key // Default case
                                    };

                                    var resultss =
                                        await _userManager.AddToRolesBatchAsync(group.ToList(), new[] { roleName });
                                    if (!resultss.Succeeded)
                                    {
                                        // Handle role assignment errors
                                        TempData["ErrorMessage"] = "Error assigning roles: " +
                                                                   string.Join(", ",
                                                                       resultss.Errors.Select(e => e.Description));
                                        return RedirectToAction("SelectTeam");
                                    }

                                }
                            }

                        }
                        TempData["SuccessMessage"] = "Team structure updated successfully!";
                    }
                else
                {
                    TempData["Error"] = "Team structure updated failed contact Admin!";
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error uploading file: {ex.Message}";

            }

            return RedirectToAction("SelectTeam");
        }

        private bool IsHeaderOrEmptyRow(IXLRow row)
        {
            var cellValue = row.Cell(1).Value.ToString();
            return string.IsNullOrWhiteSpace(cellValue) ||
                   cellValue.Contains("EXAMINERS") ||
                   cellValue.Contains("WITH THEIR");
        }

        private async Task<ValidationResult> ValidateMasterData(List<TeamMemberImportModel> data,string subject,string paperCode)
        {
            var notInMaster = new List<string>();
            var notDataInmaster = new List<TeamMemberImportModel>();

            foreach (var item in data)
            {
                var examinerRecord = await _context.EXM_EXAMINER_MASTER
                    .FirstOrDefaultAsync(a => a.EMS_NATIONAL_ID == item.IdNumber);

                if (examinerRecord == null)
                {
              
                    notDataInmaster.Add(item);
                }
            }

            foreach (var item in notDataInmaster)
            {
                if(item.IdNumber == null || string.IsNullOrEmpty( item.IdNumber))
                {
                    continue;
                }

                if (item.IdNumber == null || string.IsNullOrEmpty(item.IdNumber))
                {
                    continue;
                }

                // Remove spaces and special characters, then uppercase
                item.IdNumber = Regex.Replace(item.IdNumber, "[^a-zA-Z0-9]", "").ToUpper();

                var validStatuses = new[] { "E", "BMS", "RPMS", "PMS", "DPMS", "V", "C", "DPMS DC" };

                if (!validStatuses.Contains(item.Status))
                {
                    item.Status = "E"; // fallback
                }
                // Starting point if no examiner exists
                int userTrainingMaxStr2 = 190000;

                // Get the maximum examiner code from database
                var userTrainingMaxStr = await _context.EXM_EXAMINER_MASTER
                    .MaxAsync(u => u.EMS_EXAMINER_CODE);

                // If value exists and can be parsed to int
                if (!string.IsNullOrEmpty(userTrainingMaxStr) && int.TryParse(userTrainingMaxStr, out int userTrainingMax))
                {
                    userTrainingMaxStr2 = userTrainingMax + 1; // increment
                }

                // Now convert to string for saving
                string newexaminercode = userTrainingMaxStr2.ToString();

                // Split by space
                var parts = item.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                string firstName = parts.Length > 0 ? parts[0] : "";
                string lastName = parts.Length > 1 ? parts[1] : "";

                var newexaminer =  new Examiner
                {
                    EMS_EXAMINER_CODE = newexaminercode,
                    EMS_EXAMINER_NUMBER = item.ExaminerNumber,
                    EMS_EXM_SUPERORD = item.Team,
                    EMS_ECT_EXAMINER_CAT_CODE = item.Status,
                    EMS_SUB_SUB_ID = subject,
                    EMS_EXAMINER_NAME = firstName,
                    EMS_NATIONAL_ID = item.IdNumber,
                    EMS_SEX = item.Sex,
                    EMS_LAST_NAME = lastName,
                    EMS_REGION_CODE = item.Region,
                    EMS_MARKING_REG_CODE = item.Region,
                    EMS_PAPER_CODE = paperCode,
                    EMS_LEVEL_OF_EXAM_MARKED = "7",
                    EMS_SUBKEY = subject + paperCode + item.IdNumber,
                    EMS_COMMENTS = "New Examiners Added via Team Upload"
                };

                var checkexist = await _context.EXM_EXAMINER_MASTER.FirstOrDefaultAsync(a => a.EMS_NATIONAL_ID == newexaminer.EMS_NATIONAL_ID);

                if (checkexist == null)
                {
                    _context.EXM_EXAMINER_MASTER.Add(newexaminer);
                    await _context.SaveChangesAsync();
                }

             
            }

            foreach (var item in data)
            {
                var examinerRecord = await _context.EXM_EXAMINER_MASTER
                    .FirstOrDefaultAsync(a => a.EMS_NATIONAL_ID == item.IdNumber);

                if (examinerRecord == null)
                {
                    notInMaster.Add(item.IdNumber);
                 
                }
            }

            if (notInMaster.Any()) 
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage =
                        $"Fix the following Error these  national ID  numbers were not found in examiner master data: {string.Join(", ", notInMaster)}"
                };
            }

            return new ValidationResult { IsValid = true };
        }


        private ValidationResult ValidateTeamDuplicates(List<TeamMemberImportModel> data)
        {
            // Check for duplicate examiner numbers
            var duplicates = data
                .GroupBy(x => x.ExaminerNumber)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Duplicate examiner numbers found: {string.Join(", ", duplicates)}"
                };
            }

           

            return new ValidationResult { IsValid = true };
        }



        private ValidationResult ValidateRegionTeamBeltsData(List<TeamMemberImportModel> data, string regionCode)
        {
            // Determine the effective region code to validate against
            var effectiveRegionCode = !string.IsNullOrEmpty(regionCode)
                ? regionCode
                : data.FirstOrDefault()?.Region;

            if (string.IsNullOrEmpty(effectiveRegionCode))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Region code not specified and could not be determined from member data."
                };
            }

            // Validate all members belong to the correct region
            var examinersWrongRegion = data
                .Where(member => member.Region != effectiveRegionCode)
                .ToList();

            if (examinersWrongRegion.Any())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"The following examiners don't belong to region {effectiveRegionCode}: " +
                                 string.Join(", ", examinersWrongRegion.Select(e => $"{e.ExaminerNumber} ({e.Name})"))
                };
            }

            // Validate examiner number prefixes match region
            // Validate examiner number prefixes match region
            // Figure out expected prefix based on region
            string regionPrefix;
            if (int.TryParse(effectiveRegionCode, out int regionNumber))
            {
                if (regionNumber < 10)
                    regionPrefix = regionNumber.ToString(); // "07" => "7"
                else
                    regionPrefix = effectiveRegionCode;     // "10" => "10"
            }
            else
            {
                // fallback in case region code is invalid
                regionPrefix = effectiveRegionCode;
            }

            var examinersWrongNumberPrefix = data
                .Where(member =>
                {
                    if (string.IsNullOrEmpty(member.ExaminerNumber))
                        return true; // Flag empty examiner numbers

                    return !member.ExaminerNumber.StartsWith(regionPrefix, StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            if (examinersWrongNumberPrefix.Any())
            {
                var errorDetails = examinersWrongNumberPrefix
                    .Select(e => $"{e.ExaminerNumber} ({e.Name}) - Region: {e.Region}")
                    .ToList();

                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Examiner number validation failed for region {effectiveRegionCode}:\n" +
                                 $"• Expected prefix: '{regionPrefix}'\n" +
                                 $"• Problematic examiners:\n   {string.Join("\n   ", errorDetails)}\n\n" +
                                 "Note: Examiner numbers should start with the correct region prefix."
                };
            }



            return new ValidationResult { IsValid = true };
        }

        private ValidationResult ValidateCapturingTeamBeltsData(List<TeamMemberImportModel> data)
        {
            // First check if there are any BMS examiners at all
            if (!data.Any(x => x.Status == "BMS"))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "No BMS examiners found in the upload file."
                };
            }

            var validationErrors = new List<string>();
            var bmsExaminers = data.Where(x => x.Status == "BMS").ToList();

            foreach (var bms in bmsExaminers)
            {
                // Get all members that belong to this BMS's team (where Team == BMS.ExaminerNumber)
                var teamMembers = data.Where(x => x.Team == bms.ExaminerNumber).ToList();

                bool hasVerifier = teamMembers.Any(m => m.Capturing == "V");
                bool hasCapturer = teamMembers.Any(m => m.Capturing == "C");
               

                var errorMessages = new List<string>();

                

                if (!hasVerifier) errorMessages.Add("Missing V (Verifier)");
                if (!hasCapturer) errorMessages.Add("Missing C (Capturer)");

                if (errorMessages.Any())
                {
                    validationErrors.Add($"Team {bms.ExaminerNumber} ({bms.Name}): {string.Join("; ", errorMessages)}");
                }
            }

            if (validationErrors.Any())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "BMS team validation failed:\n" +
                                 string.Join("\n", validationErrors) +
                                 "\n\nEach BMS team must have:\n" +
                                 "- At least one member with 'V' (Verifier) role\n" +
                                 "- At least one member with 'C' (Capturer) role\n" +
                                 "- No invalid role designations"
                };
            }

            return new ValidationResult { IsValid = true };
        }

        private ValidationResult ValidateTeamBeltsData(List<TeamMemberImportModel> data)
        {
            // Check for examiner numbers that don't match their status prefix
            var wrongExaminerNumbers = new List<TeamMemberImportModel>();

            foreach (var examiner in data)
            {
                bool isValid = examiner.Status switch
                {
                    "PMS" => examiner.ExaminerNumber.StartsWith("1"),
                    "RPMS" or "DPMS" => examiner.ExaminerNumber.StartsWith("2"),
                    "BMS" => examiner.ExaminerNumber.StartsWith("3"),
                    "E" => examiner.ExaminerNumber.Length > 0 &&
                           char.IsDigit(examiner.ExaminerNumber[0]) &&
                           int.Parse(examiner.ExaminerNumber[0].ToString()) >= 4,
                    _ => true // Default case if status is unknown
                };

                if (!isValid)
                {
                    wrongExaminerNumbers.Add(examiner);
                }
            }

            if (wrongExaminerNumbers.Any())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"The following examiner numbers don't match their status requirements:\n" +
                                   string.Join("\n", wrongExaminerNumbers.Select(e =>
                                       $"Examiner: {e.ExaminerNumber}, " +
                                       $"Status: {e.Status}, " +
                                       $"Expected Prefix: {(e.Status == "PMS" ? "1" :
                                           e.Status == "RPMS" || e.Status == "DPMS" ? "2" :
                                           e.Status == "BMS" ? "3" : "4 or higher")}"
                                   ))
                };
            }

            // Check for missing sequential examiner numbers
            var statusGroups = data
                .GroupBy(e => e.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Numbers = g.Select(e =>
                        {
                            if (int.TryParse(e.ExaminerNumber, out int num))
                                return num;
                            return -1; // Invalid number
                        })
                        .Where(n => n != -1)
                        .OrderBy(n => n)
                        .ToList()
                })
                .ToList();

            var missingNumbers = new List<string>();

            foreach (var group in statusGroups)
            {
                if (group.Numbers.Count < 2) continue; // Need at least 2 numbers to check sequence

                int expectedPrefix = group.Status switch
                {
                    "PMS" => 1,
                    "RPMS" or "DPMS" => 2,
                    "BMS" => 3,
                    "E" => 4,
                    _ => 0 // Unknown status
                };

                // Filter numbers that start with correct prefix
                var validNumbers = group.Numbers
                    .Where(n => n.ToString().StartsWith(expectedPrefix.ToString()))
                    .ToList();

                for (int i = 1; i < validNumbers.Count; i++)
                {
                    int current = validNumbers[i];
                    int previous = validNumbers[i - 1];

                    if (current != previous + 1)
                    {
                        for (int missing = previous + 1; missing < current; missing++)
                        {
                            // Ensure missing number would have correct prefix
                            if (missing.ToString().StartsWith(expectedPrefix.ToString()))
                            {
                                missingNumbers.Add($"{group.Status}: {missing}");
                            }
                        }
                    }
                }
            }

            if (missingNumbers.Any())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Missing sequential examiner numbers detected:\n" +
                                   string.Join("\n", missingNumbers)
                };
            }

            return new ValidationResult { IsValid = true };
        }


        [HttpPost]
        public async Task<IActionResult> ProcessMaterialAndCategory(CombinedMaterialCategoryViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    var materials = await _materialRepository.GetAllAsync();
            //    ViewBag.Materials = new SelectList(materials, "Name", "Name");
            //    return View("MarkingMaterial", model);
            //}



            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                if (!string.IsNullOrEmpty(model.Region))
                {
                    // 1. Process Category Dates (Update or Insert)
                    foreach (var categoryDate in model.CategoryDates)
                    {
                        var existingCategory = await _context.CATEGORYCHECKINCHECKOUT
                            .FirstOrDefaultAsync(c =>
                                c.SubSubId == model.ExamCode + model.SubjectCode &&
                                c.PaperCode == model.PaperCode && c.REGION == model.Region &&
                                c.Category == categoryDate.Category);

                        if (existingCategory != null)
                        {
                            // Update existing
                            existingCategory.CheckIn = categoryDate.CheckIn;
                            existingCategory.CheckOut = categoryDate.CheckOut;

                            _context.Update(existingCategory);
                        }
                        else
                        {
                            // Insert new
                            await _context.CATEGORYCHECKINCHECKOUT.AddAsync(new CategoryCheckInCheckOut
                            {
                                Category = categoryDate.Category,
                                CheckIn = categoryDate.CheckIn,
                                CheckOut = categoryDate.CheckOut,
                                SubSubId = model.ExamCode + model.SubjectCode,
                                PaperCode = model.PaperCode,
                                REGION = model.Region

                            });
                        }
                    }

                    // 2. Process Materials (Delete all existing and insert new)
                    // First delete all existing materials for this combination
                    var existingMaterials = await _context.MaterialTransaction
                        .Where(m =>
                            m.SUBSUBID == model.ExamCode + model.SubjectCode && m.Region == model.Region &&
                            m.PAPERCODE == model.PaperCode)
                        .ToListAsync();

                    if (existingMaterials.Any())
                    {
                        _context.MaterialTransaction.RemoveRange(existingMaterials);
                    }

                    // Then add all current materials from the form
                    foreach (var material in model.Materials)
                    {
                        if (!string.IsNullOrWhiteSpace(material.MaterialName) && material.Quantity > 0)
                        {
                            await _context.MaterialTransaction.AddAsync(new MaterialTransaction
                            {
                                ITEM = material.MaterialName,
                                QUANTITY = material.Quantity,
                                SUBSUBID = model.ExamCode + model.SubjectCode,
                                PAPERCODE = model.PaperCode,
                                Region = model.Region
                            });
                        }
                    }

                    await _context.SaveChangesAsync(currentUser.Id);

                }
                else
                {
                    // 1. Process Category Dates (Update or Insert)
                    foreach (var categoryDate in model.CategoryDates)
                    {
                        var existingCategory = await _context.CATEGORYCHECKINCHECKOUT
                            .FirstOrDefaultAsync(c =>
                                c.SubSubId == model.ExamCode + model.SubjectCode &&
                                c.PaperCode == model.PaperCode &&
                                c.Category == categoryDate.Category);

                        if (existingCategory != null)
                        {
                            // Update existing
                            existingCategory.CheckIn = categoryDate.CheckIn;
                            existingCategory.CheckOut = categoryDate.CheckOut;

                            _context.Update(existingCategory);
                        }
                        else
                        {
                            // Insert new
                            await _context.CATEGORYCHECKINCHECKOUT.AddAsync(new CategoryCheckInCheckOut
                            {
                                Category = categoryDate.Category,
                                CheckIn = categoryDate.CheckIn,
                                CheckOut = categoryDate.CheckOut,
                                SubSubId = model.ExamCode + model.SubjectCode,
                                PaperCode = model.PaperCode,


                            });
                        }
                    }

                    // 2. Process Materials (Delete all existing and insert new)
                    // First delete all existing materials for this combination
                    var existingMaterials = await _context.MaterialTransaction
                        .Where(m =>
                            m.SUBSUBID == model.ExamCode + model.SubjectCode &&
                            m.PAPERCODE == model.PaperCode)
                        .ToListAsync();

                    if (existingMaterials.Any())
                    {
                        _context.MaterialTransaction.RemoveRange(existingMaterials);
                    }

                    // Then add all current materials from the form
                    foreach (var material in model.Materials)
                    {
                        if (!string.IsNullOrWhiteSpace(material.MaterialName) && material.Quantity > 0)
                        {
                            await _context.MaterialTransaction.AddAsync(new MaterialTransaction
                            {
                                ITEM = material.MaterialName,
                                QUANTITY = material.Quantity,
                                SUBSUBID = model.ExamCode + model.SubjectCode,
                                PAPERCODE = model.PaperCode,

                            });
                        }
                    }

                    await _context.SaveChangesAsync(currentUser.Id);

                }



                TempData["SuccessMessage"] = "Data saved successfully!";
                return RedirectToAction("MarkingMaterial", new
                {
                    examCode = model.ExamCode,
                    subjectCode = model.SubjectCode,
                    paperCode = model.PaperCode,
                    regionCode = model.Region,

                });
            }
            catch (Exception ex)
            {



                // Repopulate dropdown and return to form
                var materials = await _materialRepository.GetAllAsync();
                ViewBag.Materials = new SelectList(materials, "Name", "Name");


                return View("MarkingMaterial", model);
            }
        }

        private void UpdateMemberDetails(Examiner existing, TeamMemberImportModel updated)
        {

            existing.EMS_NATIONAL_ID = updated.IdNumber;

            existing.EMS_ECT_EXAMINER_CAT_CODE = updated.Status;
            existing.EMS_EXM_SUPERORD = updated.Team;

        }

        private string GenerateValidUsername(string firstName, string lastName, HashSet<string> existingUsernames,
            List<ApplicationUser> newUsers)
        {
            // Remove all non-alphanumeric characters
            string cleanFirst = Regex.Replace(firstName, @"[^a-zA-Z0-9]", "").ToLower();
            string cleanLast = Regex.Replace(lastName, @"[^a-zA-Z0-9]", "").ToLower();

            // Generate base username (first 3 letters of first name + surname)
            string usernameBase = $"{cleanFirst.Substring(0, Math.Min(3, cleanFirst.Length))}{cleanLast}";

            // Ensure username starts with a letter
            if (usernameBase.Length > 0 && char.IsDigit(usernameBase[0]))
            {
                usernameBase = "u" + usernameBase;
            }

            // Make unique if needed
            string username = usernameBase;
            int suffix = 1;

            while (existingUsernames.Contains(username) ||
                   newUsers.Any(u => u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                username = $"{usernameBase}{suffix++}";
            }

            return username;
        }

        private string RemoveMiddleName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var nameParts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return nameParts.Length > 0 ? nameParts[0] : string.Empty;
        }

        public TeamHierarchy ProcessTeamStructureGrade7(Stream stream,string regionCode)
        {
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet("Team Structure");
                var rows = worksheet.RowsUsed();

                var teamMembers = new List<TeamMember>();

                // First pass - collect all valid team members
                foreach (var row in rows)
                {
                    if (row.IsEmpty()) continue;

                    var status = row.Cell(5).GetString().Trim().ToUpper();
                    //if (string.IsNullOrEmpty(status)) continue;

                    var member = new TeamMember
                    {
                        Name = row.Cell(2).GetString().Trim(),
                        IdNumber = row.Cell(3).GetString().Trim(),
                        Sex = row.Cell(4).GetString().Trim(),
                        Status = status,
                        Capturing = row.Cell(6).GetString().Trim(),
                        Region = row.Cell(8).GetString().Trim(),
                        Station = row.Cell(9).GetString().Trim(),
                        District = row.Cell(10).GetString().Trim(),
                        Province = row.Cell(11).GetString().Trim(),
                        Phone = row.Cell(12).GetString().Trim(),
                        RowNumber = row.RowNumber()
                    };

                    if (member.ExaminerNumber == "EX.NO")
                        continue;

                    if (member.IdNumber == "NATIONALID")
                        continue;
                    if (string.IsNullOrEmpty(member.IdNumber))
                        continue;

                    var validStatuses = new[] { "E", "BMS", "RPMS", "PMS", "DPMS", "V", "C", "DPMS DC" };

                    if (!validStatuses.Contains(member.Status))
                    {
                        member.Status = "E"; // fallback
                    }

                    if(!string.IsNullOrEmpty(member.Region))
                    {
                        member.Region = regionCode;
                    }

                    if (member.Region != regionCode)
                    {
                        member.Region = regionCode;
                    }

                    teamMembers.Add(member);
                }

                // Second pass - build proper hierarchy
                return BuildProperHierarchyGrade7(teamMembers,regionCode);
            }
        }
        
        public TeamHierarchy ProcessTeamStructureOAOLD(Stream stream)
        {
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet("Team Structure");
                var rows = worksheet.RowsUsed();

                var teamMembers = new List<TeamMember>();

                // First pass - collect all valid team members
                foreach (var row in rows)
                {
                    if (row.IsEmpty()) continue;

                    var status = row.Cell(5).GetString().Trim().ToUpper();
                    if (string.IsNullOrEmpty(status)) continue;

                    var member = new TeamMember
                    {
                        Name = row.Cell(2).GetString().Trim(),
                        IdNumber = row.Cell(3).GetString().Trim(),
                        Sex = row.Cell(4).GetString().Trim(),
                        Status = status,
                        Capturing = row.Cell(6).GetString().Trim(),
                        Region = row.Cell(8).GetString().Trim(),
                        Station = row.Cell(9).GetString().Trim(),
                        District = row.Cell(10).GetString().Trim(),
                        Province = row.Cell(11).GetString().Trim(),
                        Phone = row.Cell(12).GetString().Trim(),
                        RowNumber = row.RowNumber()
                    };

         
                    if (string.IsNullOrEmpty(member.IdNumber))
                        continue;

                    if (member.ExaminerNumber == "EX.NO")
                        continue;

                    if (member.IdNumber == "NATIONALID")
                        continue;

                  

                    var validStatuses = new[] { "E", "BMS", "RPMS", "PMS", "DPMS", "V", "C" , "DPMS DC" };

                    if (!validStatuses.Contains(member.Status))
                    {
                        member.Status = "E"; // fallback
                    }

                    teamMembers.Add(member);
                }

                // Second pass - build proper hierarchy
                return BuildProperHierarchyOAOLD(teamMembers);
            }
        }

        private TeamHierarchy BuildProperHierarchyOAOLD(List<TeamMember> members)
{
    var hierarchy = new TeamHierarchy();
    TeamNode currentDPMSNode = null;
    TeamNode currentBMSNode = null;

    // Counters for examiner numbers
    int pmsCounter = 1001;
    int dpmsCounter = 2001;
    int bmsCounter = 3001;
    int eCounter = 4001;

    foreach (var member in members)
    {
                if (member.IdNumber == "NATIONALID")
                    continue;

        switch (member.Status)
        {
            case "PMS":
                member.ExaminerNumber = pmsCounter.ToString();
                member.Team = pmsCounter.ToString(); // PMS team is their own number
                hierarchy.PMS = member;
                
                pmsCounter++; // Increment for next PMS (if any)
                currentDPMSNode = null; // Reset current DPMS
                currentBMSNode = null; // Reset current BMS
                break;

            case "DPMS":
            case "DPMS DC":
                // Create new DPMS node
                member.ExaminerNumber = dpmsCounter.ToString();
                member.Team = hierarchy.PMS.ExaminerNumber; // DPMS team is PMS number
                member.Status = "DPMS";
                
                var dpmsNode = new TeamNode { Member = member };
                hierarchy.DPMS.Add(dpmsNode);
                currentDPMSNode = dpmsNode;
                currentBMSNode = null; // Reset current BMS when new DPMS starts
                
                dpmsCounter++; // Increment for next DPMS
                break;

            case "BMS":
                if (currentDPMSNode != null)
                {
                    // Create new BMS node under current DPMS
                    member.ExaminerNumber = bmsCounter.ToString();
                    member.Team = currentDPMSNode.Member.ExaminerNumber; // BMS team is DPMS number
                    
                    var bmsNode = new TeamNode { Member = member };
                    currentDPMSNode.Subordinates.Add(bmsNode);
                    currentBMSNode = bmsNode;
                    
                    bmsCounter++; // Increment for next BMS
                }
                break;

            case "E":
                string supervisorNumber = null;
                
                if (currentBMSNode != null)
                {
                    // Add examiner to current BMS
                    supervisorNumber = currentBMSNode.Member.ExaminerNumber;
                }
                else if (currentDPMSNode != null)
                {
                    // Fallback: add examiner to DPMS if no BMS
                    supervisorNumber = "1001";
                        }
         

                if (supervisorNumber != null)
                        {
                            member.ExaminerNumber = eCounter.ToString();
                            member.Team = supervisorNumber; // E team is supervisor's number

                            var examinerNode = new TeamNode { Member = member };
                            if (currentBMSNode != null)
                            {
                                currentBMSNode.Subordinates.Add(examinerNode);
                            }
                            else
                            {
                                currentDPMSNode.Subordinates.Add(examinerNode);
                            }

                            eCounter++; // Increment for next E
                        }
                break;
                    case "V":
                        string supervisorNumber1 = null;

                        if (currentBMSNode != null)
                        {
                            // Add examiner to current BMS
                            supervisorNumber1 = currentBMSNode.Member.ExaminerNumber;
                        }
                        else if (currentDPMSNode != null)
                        {
                            // Fallback: add examiner to DPMS if no BMS
                            supervisorNumber1 = "1001";
                        }


                        if (supervisorNumber1 != null)
                        {
                            member.ExaminerNumber = eCounter.ToString();
                            member.Team = supervisorNumber1; // E team is supervisor's number
                            member.Status = "E";
                            member.Capturing = "V";

                            var examinerNode = new TeamNode { Member = member };
                            if (currentBMSNode != null)
                            {
                                currentBMSNode.Subordinates.Add(examinerNode);
                            }
                            else
                            {
                                currentDPMSNode.Subordinates.Add(examinerNode);
                            }

                            eCounter++; // Increment for next E
                        }
                        break;
                    case "C":
                        string supervisorNumber2 = null;

                        if (currentBMSNode != null)
                        {
                            // Add examiner to current BMS
                            supervisorNumber2 = currentBMSNode.Member.ExaminerNumber;
                        }
                        else if (currentDPMSNode != null)
                        {
                            // Fallback: add examiner to DPMS if no BMS
                            supervisorNumber2 = "1001";
                        }


                        if (supervisorNumber2 != null)
                        {
                            member.ExaminerNumber = eCounter.ToString();
                            member.Team = supervisorNumber2; // E team is supervisor's number
                            member.Status = "E";
                            member.Capturing = "C";

                            var examinerNode = new TeamNode { Member = member };
                            if (currentBMSNode != null)
                            {
                                currentBMSNode.Subordinates.Add(examinerNode);
                            }
                            else
                            {
                                currentDPMSNode.Subordinates.Add(examinerNode);
                            }

                            eCounter++; // Increment for next E
                        }
                        break;

                }
    }

    return hierarchy;
}
        
        
        private TeamHierarchy BuildProperHierarchyGrade7(List<TeamMember> members, string region)
{
    // Ensure region is 2 digits (e.g., "01" instead of "1")
    region = region.PadLeft(2, '0');
    
    // Filter members for the specified region
    var regionMembers = members.Where(m => m.Region.PadLeft(2, '0') == region).ToList();
    
    // Calculate base number for this region (e.g., region "01" -> 1000, "02" -> 2000)
    int regionBase = int.Parse(region) * 1000;
    
    var hierarchy = new TeamHierarchy();
    TeamNode currentDPMSNode = null;
    TeamNode currentBMSNode = null;

    // Single counter that increments sequentially
    int currentNumber = regionBase + 1;

    // First pass - assign PMS and RPMS
    foreach (var member in regionMembers)
            {
                if (member.IdNumber == "NATIONALID")
                    continue;

                if (member.Status == "PMS")
        {
            member.ExaminerNumber = currentNumber.ToString();
            member.Team = currentNumber.ToString();
            hierarchy.PMS = member;
            currentNumber++;
        }
        else if (member.Status == "RPMS")
        {
            member.ExaminerNumber = currentNumber.ToString();
            // If no PMS, RPMS becomes the top (team = own number)
            member.Team = hierarchy.PMS?.ExaminerNumber ?? currentNumber.ToString();
            hierarchy.RPMS = member;
            currentNumber++;
        }
    }

    // Second pass - assign DPMS, BMS, and E
    foreach (var member in regionMembers)
    {
        switch (member.Status)
        {
            case "DPMS":
            case "DPMS DC":
                member.ExaminerNumber = currentNumber.ToString();
                member.Team = hierarchy.PMS?.ExaminerNumber ?? hierarchy.RPMS?.ExaminerNumber;
                        member.Status = "DPMS";
                        var dpmsNode = new TeamNode { Member = member };
                hierarchy.DPMS.Add(dpmsNode);
                currentDPMSNode = dpmsNode;
                currentBMSNode = null;
                
                currentNumber++;
                break;

            case "BMS":
                if (currentDPMSNode != null)
                {
                    member.ExaminerNumber = currentNumber.ToString();
                    member.Team = currentDPMSNode.Member.ExaminerNumber;
                    
                    var bmsNode = new TeamNode { Member = member };
                    currentDPMSNode.Subordinates.Add(bmsNode);
                    currentBMSNode = bmsNode;
                    
                    currentNumber++;
                }
                break;

            case "E":
                   
                string supervisorNumber = null;
                
                if (currentBMSNode != null)
                {
                    supervisorNumber = currentBMSNode.Member.ExaminerNumber;
                }
                else if (currentDPMSNode != null)
                {
                    supervisorNumber = currentDPMSNode.Member.ExaminerNumber;
                        }
                        else
                        {
                            supervisorNumber = currentNumber.ToString();
                        }

                        if (supervisorNumber != null)
                {
                    member.ExaminerNumber = currentNumber.ToString();
                    member.Team = supervisorNumber;
                    
                    var examinerNode = new TeamNode { Member = member };
                    if (currentBMSNode != null)
                    {
                        currentBMSNode.Subordinates.Add(examinerNode);
                    }
                    else
                    {
                        currentDPMSNode.Subordinates.Add(examinerNode);
                    }
                    
                    currentNumber++;
                }
                break;
                    case "C":

                        string supervisorNumber1 = null;

                        if (currentBMSNode != null)
                        {
                            supervisorNumber1 = currentBMSNode.Member.ExaminerNumber;
                        }
                        else if (currentDPMSNode != null)
                        {
                            supervisorNumber1 = currentDPMSNode.Member.ExaminerNumber;
                        }
                        else
                        {
                            supervisorNumber1 = currentNumber.ToString();
                        }

                        if (supervisorNumber1 != null)
                        {
                            member.ExaminerNumber = currentNumber.ToString();
                            member.Team = supervisorNumber1;
                            member.Status = "E";
                            member.Capturing = "C"; // Set capturing role to C  

                            var examinerNode = new TeamNode { Member = member };
                            if (currentBMSNode != null)
                            {
                                currentBMSNode.Subordinates.Add(examinerNode);
                            }
                            else
                            {
                                currentDPMSNode.Subordinates.Add(examinerNode);
                            }

                            currentNumber++;
                        }
                        break;
                    case "V":

                        string supervisorNumber2 = null;

                        if (currentBMSNode != null)
                        {
                            supervisorNumber2 = currentBMSNode.Member.ExaminerNumber;
                        }
                        else if (currentDPMSNode != null)
                        {
                            supervisorNumber2 = currentDPMSNode.Member.ExaminerNumber;
                        }
                        else
                        {
                            supervisorNumber2 = currentNumber.ToString();
                        }

                        if (supervisorNumber2 != null)
                        {
                            member.ExaminerNumber = currentNumber.ToString();
                            member.Team = supervisorNumber2;
                            member.Status = "E";
                            member.Capturing = "V"; // Set capturing role to C  

                            var examinerNode = new TeamNode { Member = member };
                            if (currentBMSNode != null)
                            {
                                currentBMSNode.Subordinates.Add(examinerNode);
                            }
                            else
                            {
                                currentDPMSNode.Subordinates.Add(examinerNode);
                            }

                            currentNumber++;
                        }
                        break;
                }
    }

    return hierarchy;
}





        public List<TeamMember> ProcessTeamStructure(Stream fileStream)
        {
            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = workbook.Worksheet("Team Structure");

                // Find the actual data starting row by looking for "EX.NO" header
                int dataStartRow = 1;
                while (dataStartRow <= 100) // Limit search to first 100 rows
                {
                    if (worksheet.Cell(dataStartRow, 1).GetString() == "EX.NO")
                        break;
                    dataStartRow++;
                }

                if (dataStartRow > 100)
                    throw new Exception("Could not find data header row");

                // Get all rows from header down (skip 1 less since header is included)
                var rows = worksheet.Rows(dataStartRow, worksheet.LastRowUsed().RowNumber());

                var teamMembers = new List<TeamMember>();

                foreach (var row in rows)
                {
                    //if (row.IsEmpty()) continue;

                    if (row.Cells().All(c => string.IsNullOrWhiteSpace(c.GetString())))
                        continue;

                    var member = new TeamMember
                    {
                         ExaminerNumber = row.Cell(1).GetString().Trim(),
                        Name = row.Cell(2).GetString().Trim(),
                        IdNumber = row.Cell(3).GetString().Trim(),
                        Sex = row.Cell(4).GetString().Trim(),
                        Status = row.Cell(5).GetString().Trim().ToUpper(),
                        Capturing = row.Cell(6).GetString().Trim(),
                        Team = row.Cell(7).GetString().Trim(),
                        Region = row.Cell(8).GetString().Trim(),
                        Station = row.Cell(9).GetString().Trim(),
                        District = row.Cell(10).GetString().Trim(),
                        Province = row.Cell(11).GetString().Trim(),
                        Phone = row.Cell(12).GetString().Trim()
                    };

                    if (member.ExaminerNumber == "EX.NO")
                        continue;

                    if (member.IdNumber == "NATIONALID")
                        continue;

                    if (string.IsNullOrEmpty(member.IdNumber))
                        continue;

                    var validStatuses = new[] { "E", "BMS", "RPMS", "PMS", "DPMS", "V", "C" };

                    if (!validStatuses.Contains(member.Status))
                    {
                        member.Status = "E"; // fallback
                    }

                    teamMembers.Add(member);
                }

                return teamMembers.ToList();
            }
        }


        //private TeamHierarchy BuildTeamHierarchy(List<TeamMember> members)
        //{
        //    var hierarchy = new TeamHierarchy();

        //    // Find the PMS (Principal Marker Supervisor)
        //    var pms = members.FirstOrDefault(m => m.Status == "PMS");
        //    if (pms == null) return hierarchy; // No hierarchy without PMS

        //    hierarchy.PMS = pms;

        //    // Find all DPMS (Deputy Principal Marker Supervisors)
        //    var dpmsList = members.Where(m => m.Status == "DPMS").ToList();

        //    // Find all BMS (Branch Marker Supervisors)
        //    var bmsList = members.Where(m => m.Status == "BMS").ToList();

        //    // Find all Examiners (E)
        //    var examiners = members.Where(m => m.Status == "E").ToList();

        //    // Case 1: If there are DPMS, they report to PMS
        //    if (dpmsList.Any())
        //    {
        //        foreach (var dpms in dpmsList)
        //        {
        //            var dpmsNode = new TeamNode { Member = dpms };

        //            // Find BMS that report to this DPMS (based on team number or other logic)
        //            var dpmsBms = bmsList.Where(b => ShouldReportTo(b, dpms)).ToList();

        //            foreach (var bms in dpmsBms)
        //            {
        //                var bmsNode = new TeamNode { Member = bms };

        //                // Find examiners that report to this BMS
        //                var bmsExaminers = examiners.Where(e => ShouldReportTo(e, bms)).ToList();
        //                bmsNode.Subordinates = bmsExaminers.Select(e => new TeamNode { Member = e }).ToList();

        //                dpmsNode.Subordinates.Add(bmsNode);
        //                bmsList.Remove(bms); // Remove assigned BMS
        //            }

        //            hierarchy.DPMS.Add(dpmsNode);
        //        }

        //        // Any remaining BMS (if DPMS didn't cover all) report directly to PMS
        //        foreach (var remainingBms in bmsList)
        //        {
        //            var bmsNode = new TeamNode { Member = remainingBms };

        //            // Find examiners that report to this BMS
        //            var bmsExaminers = examiners.Where(e => ShouldReportTo(e, remainingBms)).ToList();
        //            bmsNode.Subordinates = bmsExaminers.Select(e => new TeamNode { Member = e }).ToList();

        //            hierarchy.DPMS.Add(bmsNode);
        //        }
        //    }
        //    // Case 2: If no DPMS, BMS report directly to PMS
        //    else if (bmsList.Any())
        //    {
        //        foreach (var bms in bmsList)
        //        {
        //            var bmsNode = new TeamNode { Member = bms };

        //            // Find examiners that report to this BMS
        //            var bmsExaminers = examiners.Where(e => ShouldReportTo(e, bms)).ToList();
        //            bmsNode.Subordinates = bmsExaminers.Select(e => new TeamNode { Member = e }).ToList();

        //            hierarchy.DPMS.Add(bmsNode);
        //        }
        //    }
        //    // Case 3: If no DPMS or BMS, examiners report directly to PMS
        //    else
        //    {
        //        hierarchy.DPMS = examiners.Select(e => new TeamNode { Member = e }).ToList();
        //    }

        //    return hierarchy;
        //}

        //private bool ShouldReportTo(TeamMember subordinate, TeamMember supervisor)
        //{
        //    // Implement logic to determine if subordinate reports to supervisor
        //    // This could be based on team number, region, or other criteria
        //    // Example: Check if team numbers match or follow a pattern

        //    // Simple example - if team numbers are related (e.g., 1001 supervises 2001)
        //    if (supervisor.Status == "DPMS" && subordinate.Status == "BMS")
        //    {
        //        return subordinate.Team.StartsWith(supervisor.Team.Substring(0, 2));
        //    }
        //    else if (supervisor.Status == "BMS" && subordinate.Status == "E")
        //    {
        //        return subordinate.Team.StartsWith(supervisor.Team.Substring(0, 3));
        //    }

        //    return false;
        //}

        //private string GenerateDefaultPassword(ApplicationUser user, string subjectcode, string papercode)
        //{
        //    // Ensure subject and paper codes are cleaned
        //    string cleanSubject = (subjectcode ?? "").Trim().ToUpper();
        //    string cleanPaper = (papercode ?? "").Trim().ToUpper();

        //    // Generate the base password components
        //    string usernamePart = user.UserName?.Trim().ToLower() ?? "user";
        //    string subjectPart = cleanSubject.Length > 0 ? cleanSubject : "SUB";
        //    string paperPart = cleanPaper.Length > 0 ? cleanPaper : "PAP";

        //    // Combine components with standard format
        //    string password = $"{usernamePart}{subjectPart}{paperPart}.*";

        //    // Ensure password meets complexity requirements
        //    //if (password.Length < 8)
        //    //{
        //    //    password = password.PadRight(8, '!');
        //    //}

        //    //// Add at least one uppercase character if missing
        //    //if (!password.Any(char.IsUpper))
        //    //{
        //    //    password = char.ToUpper(password[0]) + password.Substring(1);
        //    //}

        //    return password;
        //}



    }

    public class TeamHierarchy
    {
        public TeamMember PMS { get; set; }
        public TeamMember RPMS { get; set; }  // Added RPMS property
        public List<TeamNode> DPMS { get; set; } = new List<TeamNode>();

        public string PrintStructure()
        {
            var sb = new StringBuilder();
    
            // Print PMS with team number (which will be their own number)
            sb.AppendLine($"PMS: {PMS?.Name ?? "None"} ({PMS?.ExaminerNumber ?? "N/A"}) [Team: {PMS?.Team ?? (PMS?.ExaminerNumber ?? "N/A")}]");
    
            // Print RPMS
            sb.AppendLine($"RPMS: {RPMS?.Name ?? "None"} ({RPMS?.ExaminerNumber ?? "N/A"}) [Team: {RPMS?.Team ?? "N/A"}]");
    
            // Print DPMS and their subordinates
            foreach (var dpms in DPMS)
            {
                sb.AppendLine($"  DPMS: {dpms.Member.Name} ({dpms.Member.ExaminerNumber}) [Team: {dpms.Member.Team}]");

                // Print BMS under this DPMS
                foreach (var bms in dpms.Subordinates.Where(s => s.Member.Status == "BMS"))
                {
                    sb.AppendLine($"    BMS: {bms.Member.Name} ({bms.Member.ExaminerNumber}) [Team: {bms.Member.Team}]");

                    // Print Examiners under this BMS
                    foreach (var examiner in bms.Subordinates)
                    {
                        sb.AppendLine($"      E: {examiner.Member.Name} ({examiner.Member.ExaminerNumber}) [Team: {examiner.Member.Team}]");
                    }
                }

                // Print Examiners directly under DPMS (no BMS)
                foreach (var examiner in dpms.Subordinates.Where(s => s.Member.Status == "E"))
                {
                    sb.AppendLine($"    E: {examiner.Member.Name} ({examiner.Member.ExaminerNumber}) [Team: {examiner.Member.Team}]");
                }
            }

            return sb.ToString();
        }
    }

    public class TeamNode
    {
        public TeamMember Member { get; set; }
        public List<TeamNode> Subordinates { get; set; } = new List<TeamNode>();
    }

    public class TeamMember
    {
        public string ExaminerNumber { get; set; }

    public string IdNumber { get; set; }
    public string Sex { get; set; }

    public string Capturing { get; set; }
    public string Team { get; set; }

        public string Name { get; set; }
        public string Status { get; set; }
        public string Region { get; set; }
        public string Station { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public string Phone { get; set; }
        public int RowNumber { get; set; }
    }

    public class ExaminerUpdateModel
    {
        public string IdNumber { get; set; }
        public bool IsSelected { get; set; }
        public string ExamCode { get; set; }
        public string SubjectCode { get; set; }
        public string PaperCode { get; set; }
        public string RegionCode { get; set; }
        public string Activity { get; set; }
        public string Category { get; set; }
        public string CapturingRole { get; set; }
        public string ExaminerNumber { get; set; }
        public string Team { get; set; }

        public List<TeamMemberDto> TeamMembers { get; set; }
    }

    public class TeamMemberDto
    {
        public string IdNumber { get; set; }
        public string ExaminerNumber { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
        public string Category { get; set; }
        public string CapturingRole { get; set; }
    }

    // Import model
    public class TeamStructureImportModel
    {
        public string EX_NO { get; set; }
        public string NAME { get; set; }
        public string NATIONALID { get; set; }
        public string SEX { get; set; }
        public string STATUS { get; set; }
        public string STATION { get; set; }
        public string DISTRICT { get; set; }
        public string PROVINCE { get; set; }
        public string CELL_NO { get; set; }
        public string TEAM { get; set; }
    }


    public class TeamHierarchyTracker
    {
        public string CurrentDPMS { get; set; }
        public string CurrentBMS { get; set; }
    }

    public class TeamMemberImportModel
    {
        public string ExaminerNumber { get; set; }
        public string Name { get; set; }
        public string IdNumber { get; set; }
        public string Sex { get; set; }
        public string Status { get; set; }
        public string Team { get; set; }
        public string Capturing { get; set; }

        public string Region { get; set; }

        public string Station { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public string Phone { get; set; }
    }





    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ScriptCalculationResult
    {
        public int SharePMS { get; set; }
        public int ShareRPMS { get; set; }
        public int ShareDPMS { get; set; }
        public int ShareBMS { get; set; }
        public int ShareE { get; set; }

        public int FinalSharePMS { get; set; }
        public int FinalShareRPMS { get; set; }
        public int FinalShareDPMS { get; set; }
        public int FinalShareBMS { get; set; }
        public int FinalShareE { get; set; }


        public int TotalScriptsPMS { get; set; }
        public int TotalScriptsRPMS { get; set; }
        public int TotalScriptsDPMS { get; set; }
        public int TotalScriptsBMS { get; set; }
        public int TotalScriptsE { get; set; }

        public int ScriptsToExaminers { get; set; }


        public int TotalShare => SharePMS + ShareRPMS + ShareDPMS + ShareBMS + ShareE;
        public int FinalTotalShare => FinalSharePMS + FinalShareRPMS + FinalShareDPMS + FinalShareBMS + FinalShareE;

        public int TotalScripts =>
            TotalScriptsPMS + TotalScriptsRPMS + TotalScriptsDPMS + TotalScriptsBMS + TotalScriptsE;

        public bool Success { get; set; }
    }



    public class ScriptCalculationResult1
    {
        public double SharePMS { get; set; }
        public double ShareRPMS { get; set; }
        public double ShareDPMS { get; set; }
        public double ShareBMS { get; set; }
        public double ShareE { get; set; }

        public double FinalSharePMS { get; set; }
        public double FinalShareRPMS { get; set; }
        public double FinalShareDPMS { get; set; }
        public double FinalShareBMS { get; set; }
        public double FinalShareE { get; set; }


        public double TotalScriptsPMS { get; set; }
        public double TotalScriptsRPMS { get; set; }
        public double TotalScriptsDPMS { get; set; }
        public double TotalScriptsBMS { get; set; }
        public double TotalScriptsE { get; set; }

        public double ScriptsToExaminers { get; set; }


        public double TotalShare => SharePMS + ShareRPMS + ShareDPMS + ShareBMS + ShareE;
        public double FinalTotalShare => FinalSharePMS + FinalShareRPMS + FinalShareDPMS + FinalShareBMS + FinalShareE;

        public double TotalScripts =>
            TotalScriptsPMS + TotalScriptsRPMS + TotalScriptsDPMS + TotalScriptsBMS + TotalScriptsE;

        public bool Success { get; set; }
    }


    public class DataTableRequest2
    {
        public string ExamCode { get; set; }
        public string Subject { get; set; }
        public string PaperCode { get; set; }
        public string RegionCode { get; set; }
    }

    public static class UserManagerExtensions
    {
        public static async Task<IdentityResult> CreateBatchAsync<TUser>(
            this UserManager<TUser> userManager,
            IEnumerable<TUser> users,
            string subjectCode = "",
            string paperCode = "") where TUser : ApplicationUser
        {
            var result = IdentityResult.Success;
            var allErrors = new List<IdentityError>();

            foreach (var user in users)
            {
                // Generate password for each user
                string password = GenerateDefaultPassword(user, subjectCode, paperCode);

                var createResult = await userManager.CreateAsync(user, password);

                if (!createResult.Succeeded)
                {
                    // Collect errors but continue processing
                    allErrors.AddRange(createResult.Errors.Select(e => new IdentityError
                    {
                        Code = e.Code,
                        Description = $"User {user.UserName}: {e.Description}"
                    }));
                }
            }

            return allErrors.Count > 0
                ? IdentityResult.Failed(allErrors.ToArray())
                : IdentityResult.Success;
        }

        public static async Task<IdentityResult> AddToRolesBatchAsync<TUser>(
            this UserManager<TUser> userManager,
            IEnumerable<TUser> users,
            IEnumerable<string> roles) where TUser : IdentityUser
        {
            var result = IdentityResult.Success;

            foreach (var user in users)
            {
                var addResult = await userManager.AddToRolesAsync(user, roles);

                if (!addResult.Succeeded)
                {
                    // Combine errors from all failed role assignments
                    var errors = addResult.Errors.Select(e => new IdentityError
                    {
                        Code = e.Code,
                        Description = $"User {user.UserName}: {e.Description}"
                    });

                    return IdentityResult.Failed(errors.ToArray());
                }
            }

            return result;
        }

        private static string GenerateDefaultPassword(ApplicationUser user, string subjectcode, string papercode)
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





    }
}







//public class TeamHierarchy
//{
//    public TeamMember PMS { get; set; }
//    public List<TeamNode> DPMS { get; set; } = new List<TeamNode>();


//}

//public class TeamNode
//{
//    public TeamMember Member { get; set; }
//    public List<TeamNode> Subordinates { get; set; } = new List<TeamNode>();
//}

//public class TeamMember
//{
//    public string ExaminerNumber { get; set; }
//    public string Name { get; set; }
//    public string IdNumber { get; set; }
//    public string Sex { get; set; }
//    public string Status { get; set; }
//    public string Capturing { get; set; }
//    public string Team { get; set; }
//    public string Region { get; set; }
//    public string Station { get; set; }
//    public string District { get; set; }
//    public string Province { get; set; }
//    public string Phone { get; set; }
//}


