using AutoMapper;
using DocumentFormat.OpenXml.Office2013.Excel;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Services.ExamMonitors;
using ExaminerPaymentSystem.ViewModels.ExamMonitors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.WebPages;

namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class ExamMonitorsTandSsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExamMonitorService _service;
        private readonly IMapper _mapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamMonitorsTandSsController(IExamMonitorService service, IMapper mapper, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _mapper = mapper;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> IndexAccounts()
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);


                ViewBag.Phases = _context.Phases
            .Select(p => new {
                Value = p.PhaseCode,
                Text = p.PhaseName
            })
            .OrderBy(p => p.Text)
            .ToList();

                ViewBag.Sessions = _context.ExamSessions
                                    .Select(s => new {
                                        Value = s.SessionCode,
                                        Text = s.SessionName
                                    })
                    .OrderBy(s => s.Text)
                    .ToList();


            
          

            return View();

        }


        public async Task<IActionResult> IndexApproved()
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);


            ViewBag.Phases = _context.Phases
        .Select(p => new {
            Value = p.PhaseCode,
            Text = p.PhaseName
        })
        .OrderBy(p => p.Text)
        .ToList();

            ViewBag.Sessions = _context.ExamSessions
                                .Select(s => new {
                                    Value = s.SessionCode,
                                    Text = s.SessionName
                                })
                .OrderBy(s => s.Text)
                .ToList();





            return View();

        }



        [HttpPost]
        public async Task<IActionResult> GetTandSs([FromBody] DtParameters dtParameters)
        {
            try
            {

                // Initialize with default values if null
                dtParameters ??= new DtParameters();

                var searchValue = dtParameters.Search?.Value ?? string.Empty;

                var regionFilter = dtParameters.RegionFilter ?? string.Empty;
           
                var phaseFilter = dtParameters.PhaseFilter ?? string.Empty;
                var sessionFilter = dtParameters.SessionFilter ?? string.Empty;

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);

                // Base query
                IQueryable<ExamMonitorTandS> query = _context.ExamMonitorsClaimTandSs.Include(a => a.ExamMonitor).AsQueryable();

                if (userRoles.Contains("Accounts"))
                {
                    query = query.Where(a => a.InitiatorStatus == "Pending");
                }else if (userRoles.Contains("PeerReviewer"))
                {
                    query = query.Where(a => a.ReviewStatus == "Pending");
                }

                // For Admins - apply region filter only if specified
                if (!string.IsNullOrEmpty(regionFilter))
                {
                    query = query.Where(x => x.Region == regionFilter);
                }

                if (!string.IsNullOrEmpty(phaseFilter))
                {
                    query = query.Where(m => m.Phase == phaseFilter);

                }

                if (!string.IsNullOrEmpty(sessionFilter))
                {
                    query = query.Where(m => m.Session == sessionFilter);

                }

                // Apply global search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                       (m.ExamMonitor.FirstName).Contains(searchValue)||
                      m.ExamMonitor.LastName.Contains(searchValue) ||
                        m.NationalId.Contains(searchValue));
                }

                // Get total count before paging
                var totalRecords = await query.CountAsync();

                // Apply sorting
                //if (dtParameters.Order != null && dtParameters.Order.Any())
                //{
                //    var order = dtParameters.Order.First();
                //    var columnIndex = order.Column;
                //    var columnName = dtParameters.Columns[columnIndex].Data;
                //    var isAscending = order.Dir == DtOrderDir.Asc;

                //    query = columnName switch
                //    {
                //        "firstName" => isAscending ? query.OrderBy(m => m.FirstName) : query.OrderByDescending(m => m.FirstName),
                //        "lastName" => isAscending ? query.OrderBy(m => m.LastName) : query.OrderByDescending(m => m.LastName),
                //        "nationalId" => isAscending ? query.OrderBy(m => m.NationalId) : query.OrderByDescending(m => m.NationalId),
                //        "status" => isAscending ? query.OrderBy(m => m.Status) : query.OrderByDescending(m => m.Status),
                //        "region" => isAscending ? query.OrderBy(m => m.Region) : query.OrderByDescending(m => m.Region),
                //        _ => query.OrderBy(m => m.NationalId)
                //    };
                //}
                //else
                //{
                //    query = query.OrderBy(m => m.NationalId);
                //}

                // Apply paging
                var filteredRecords = await query.CountAsync();
                var start = Math.Max(0, dtParameters.Start);
                var length = dtParameters.Length > 0 ? dtParameters.Length : 10;
                var examMonitorTandS = query
          
                        .Select(m => new
                        {

                            SubKey = m.SubKey,
                            NationalId = m.NationalId,
                            ClaimId = m.ClaimID,
                            FirstName = m.ExamMonitor.FirstName,
                            LastName = m.ExamMonitor.LastName,
                            Region = m.Region,
                            Date = m.Date,
                            Status = m.ExamMonitor.Status,
                            RegionalManager = m.RegionalManagerStatus,
                            ClusterStatus = m.ClusterManagerStatus,
                            Initiator = m.InitiatorStatus,
                            Reviewer = m.ReviewStatus,

                            Centre = m.CentreAttached,

                            Phase = m.Phase,
                            Session = m.Session,

                        })
                        .ToList();

                return Json(new
                {
                    Draw = dtParameters.Draw,
                    RecordsTotal = totalRecords,
                    RecordsFiltered = filteredRecords,
                    Data = examMonitorTandS
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> GetApprovedTandSs([FromBody] DtParameters dtParameters)
        {
            try
            {

                // Initialize with default values if null
                dtParameters ??= new DtParameters();

                var searchValue = dtParameters.Search?.Value ?? string.Empty;

                var regionFilter = dtParameters.RegionFilter ?? string.Empty;

                var phaseFilter = dtParameters.PhaseFilter ?? string.Empty;
                var sessionFilter = dtParameters.SessionFilter ?? string.Empty;

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);

                // Base query
                IQueryable<ExamMonitorTandS> query = _context.ExamMonitorsClaimTandSs.Include(a => a.ExamMonitor).AsQueryable();

                if (userRoles.Contains("Accounts"))
                {
                    query = query.Where(a => a.InitiatorStatus == "Approved");
                }
                else if (userRoles.Contains("PeerReviewer"))
                {
                    query = query.Where(a => a.ReviewStatus == "Approved");
                }

              
                // For Admins - apply region filter only if specified
                if (!string.IsNullOrEmpty(regionFilter))
                {
                    query = query.Where(x => x.Region == regionFilter);
                }

                if (!string.IsNullOrEmpty(phaseFilter))
                {
                    query = query.Where(m => m.Phase == phaseFilter);

                }

                if (!string.IsNullOrEmpty(sessionFilter))
                {
                    query = query.Where(m => m.Session == sessionFilter);

                }

                // Apply global search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                       (m.ExamMonitor.FirstName).Contains(searchValue) ||
                      m.ExamMonitor.LastName.Contains(searchValue) ||
                        m.NationalId.Contains(searchValue));
                }

                // Get total count before paging
                var totalRecords = await query.CountAsync();

                // Apply sorting
                //if (dtParameters.Order != null && dtParameters.Order.Any())
                //{
                //    var order = dtParameters.Order.First();
                //    var columnIndex = order.Column;
                //    var columnName = dtParameters.Columns[columnIndex].Data;
                //    var isAscending = order.Dir == DtOrderDir.Asc;

                //    query = columnName switch
                //    {
                //        "firstName" => isAscending ? query.OrderBy(m => m.FirstName) : query.OrderByDescending(m => m.FirstName),
                //        "lastName" => isAscending ? query.OrderBy(m => m.LastName) : query.OrderByDescending(m => m.LastName),
                //        "nationalId" => isAscending ? query.OrderBy(m => m.NationalId) : query.OrderByDescending(m => m.NationalId),
                //        "status" => isAscending ? query.OrderBy(m => m.Status) : query.OrderByDescending(m => m.Status),
                //        "region" => isAscending ? query.OrderBy(m => m.Region) : query.OrderByDescending(m => m.Region),
                //        _ => query.OrderBy(m => m.NationalId)
                //    };
                //}
                //else
                //{
                //    query = query.OrderBy(m => m.NationalId);
                //}

                // Apply paging
                var filteredRecords = await query.CountAsync();
                var start = Math.Max(0, dtParameters.Start);
                var length = dtParameters.Length > 0 ? dtParameters.Length : 10;
                var examMonitorTandS = query
    
                        .Select(m => new
                        {

                            SubKey = m.SubKey,
                            NationalId = m.NationalId,
                            ClaimId = m.ClaimID,
                            FirstName = m.ExamMonitor.FirstName,
                            LastName = m.ExamMonitor.LastName,
                            Region = m.Region,
                            Date = m.Date,
                            Status = m.ExamMonitor.Status,
                            RegionalManager = m.RegionalManagerStatus,
                            ClusterStatus = m.ClusterManagerStatus,
                            Initiator = m.InitiatorStatus,
                            Reviewer = m.ReviewStatus,

                            Centre = m.CentreAttached,

                            Phase = m.Phase,
                            Session = m.Session,

                        })
                        .ToList();

                return Json(new
                {
                    Draw = dtParameters.Draw,
                    RecordsTotal = totalRecords,
                    RecordsFiltered = filteredRecords,
                    Data = examMonitorTandS
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }


  

        public async  Task<IActionResult> Details(string claimId, string subkey)
        {

            // Validate inputs
            if (string.IsNullOrEmpty(claimId) || string.IsNullOrEmpty(subkey))
            {
                return BadRequest("Claim ID and SubKey are required");
            }

            // Parse claimId to Guid
            if (!Guid.TryParse(claimId, out Guid claimGuid))
            {
                return BadRequest("Invalid Claim ID format");
            }

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            if (currentUser == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "ACCESS DENIED!",
                    text = "Your session expired please login",
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

            var transactionRecord = await _context.ExamMonitorTransactions
                .FirstOrDefaultAsync(a => a.SubKey == subkey);

            if (transactionRecord == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "ACCESS DENIED!",
                    text = "Record is Missing in Transaction Please Login Or Contact Admin",
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

            var monitorRecord = await _context.ExamMonitors
                .FirstOrDefaultAsync(m => m.NationalId == transactionRecord.NationalId);

            if (monitorRecord == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "ACCESS DENIED!",
                    text = "Record is Missing in Master Please Login Again or Contact Admin",
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

            var availableTandS = await _context.ExamMonitorsClaimTandSs
                .FirstOrDefaultAsync(a => a.SubKey == subkey && a.ClaimID == claimGuid);


            if (availableTandS == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "ACCESS DENIED!",
                    text = "something went wrong  Contact Admin",
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

            // If T&S exists, show Details view
            var dailyAdvances = await _context.ExamMonitorsDailyAdvances
                    .Where(a => a.SubKey == availableTandS.SubKey && a.ClaimID == availableTandS.ClaimID)
                    .ToListAsync();
            var stipendAdvance = await _context.ExamMonitorsStipendAdvances
                .FirstOrDefaultAsync(a => a.SubKey == availableTandS.SubKey && a.ClaimID == availableTandS.ClaimID);
            var phaseCode = availableTandS.SubKey.Substring(2, 2);
            var session = "N25";
            var phase = await _context.Phases.FirstOrDefaultAsync(a => a.PhaseCode == phaseCode);

            if(phase == null)
            {

            }

            decimal rate = 0;

            if (transactionRecord.Status == "Cluster Manager")
            {
                rate = phase.ClusterManagerRate;
            }
            else if (transactionRecord.Status == "Assistant Cluster Manager")
            {
                rate = phase.AssistantClusterManagerRate;
            }
            else if (transactionRecord.Status == "Resident Monitor")
            {
                rate = phase.ResidentMonitorRate;
            }

            var model = new ExamMonitorTandSsViewModel
            {
                ClaimID = availableTandS.ClaimID,
                NationalId = monitorRecord.NationalId,
                DailyAdvances = dailyAdvances,
                Rate = rate, // Null check for phase
                FullName = $"{monitorRecord.FirstName} {monitorRecord.LastName}",
                Session = session,
                PhoneNumber = monitorRecord.Phone,
                Days = stipendAdvance.Days,

                CentreAttached = transactionRecord.CentreAttached,
                Station = monitorRecord.Station,
                District = transactionRecord.District,
                Region = transactionRecord.Region,
                RoleStatus = transactionRecord.Status,
                AccountNumberZwg = monitorRecord.AccountNumberZwg,
                AccountNumberUsd = monitorRecord.AccountNumberUsd,
                BankNameUsd = monitorRecord.BankNameUsd,
                BankNameZwg = monitorRecord.BankNameZwg,
                BranchUsd = monitorRecord.BranchUsd,
                BranchZwg = monitorRecord.BranchZwg,
                PhaseCode = phaseCode,
            };

            ViewBag.Phases = _context.Phases
                .Where(a => a.Status == "Active")
.Select(p => new
{
Value = p.PhaseCode,
Text = $"{p.PhaseName} - {p.StartTime:dd/MM/yyyy} to {p.EndTime:dd/MM/yyyy}"
}).ToList();

            return View(model); // This will show the Create view

            return View(); // Pass the model to the view
        }

        public async Task<IActionResult> Create(string claimId, string subkey)
        {

            // Validate inputs
            if (string.IsNullOrEmpty(claimId) || string.IsNullOrEmpty(subkey))
            {
                return BadRequest("Claim ID and SubKey are required");
            }

            // Parse claimId to Guid
            if (!Guid.TryParse(claimId, out Guid claimGuid))
            {
                return BadRequest("Invalid Claim ID format");
            }

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            if (currentUser == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "ACCESS DENIED!",
                    text = "Your session expired please login",
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

            var transactionRecord = await _context.ExamMonitorTransactions
                .FirstOrDefaultAsync(a => a.SubKey == subkey);

            if (transactionRecord == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "ACCESS DENIED!",
                    text = "Record is Missing in Transaction Please Login Or Contact Admin",
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

            var monitorRecord = await _context.ExamMonitors
                .FirstOrDefaultAsync(m => m.NationalId == transactionRecord.NationalId);

            if (monitorRecord == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "ACCESS DENIED!",
                    text = "Record is Missing in Master Please Login Again or Contact Admin",
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

            var availableTandS = await _context.ExamMonitorsClaimTandSs
                .FirstOrDefaultAsync(a => a.SubKey == subkey && a.ClaimID == claimGuid);

            


            if (availableTandS  == null)
            {
                // Store SweetAlert configuration in TempData
                TempData["SweetAlert"] = JsonConvert.SerializeObject(new
                {
                    icon = "warning",  // Changed to warning icon
                    title = "ACCESS DENIED!",
                    text = "something went wrong  Contact Admin",
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


            // If T&S exists, show Details view
            var dailyAdvances = await _context.ExamMonitorsDailyAdvances
                    .Where(a => a.SubKey == availableTandS.SubKey && a.ClaimID == availableTandS.ClaimID)
                    .ToListAsync();
                var stipendAdvance = await _context.ExamMonitorsStipendAdvances
                    .FirstOrDefaultAsync(a => a.SubKey == availableTandS.SubKey && a.ClaimID == availableTandS.ClaimID);
                var phaseCode = availableTandS.SubKey.Substring(2, 2);
                var session = availableTandS.SubKey.Substring(4,3);
                var phase = await _context.Phases.FirstOrDefaultAsync(a => a.PhaseCode == phaseCode);

            if(phase == null)
            {

            }

                decimal rate = 0;

                if (transactionRecord.Status == "Cluster Manager")
                {
                    rate = phase.ClusterManagerRate;
                }
                else if (transactionRecord.Status == "Assistant Cluster Manager")
                {
                    rate = phase.AssistantClusterManagerRate;
                }
                else if (transactionRecord.Status == "Resident Monitor")
                {
                    rate = phase.ResidentMonitorRate;
                }

                var model = new ExamMonitorTandSsViewModel
                {
                    ClaimID = availableTandS.ClaimID,
                    NationalId = monitorRecord.NationalId,
                    DailyAdvances = dailyAdvances,
                    Rate = rate, // Null check for phase
                    FullName = $"{monitorRecord.FirstName} {monitorRecord.LastName}",
                    Session = session,
                    PhoneNumber = monitorRecord.Phone,
                    Days = stipendAdvance.Days,

                    CentreAttached = transactionRecord.CentreAttached,
                    Station = monitorRecord.Station,
                    District = transactionRecord.District,
                    Region = transactionRecord.Region,
                    RoleStatus = transactionRecord.Status,
                    AccountNumberZwg = monitorRecord.AccountNumberZwg,
                    AccountNumberUsd = monitorRecord.AccountNumberUsd,
                    BankNameUsd = monitorRecord.BankNameUsd,
                    BankNameZwg = monitorRecord.BankNameZwg,
                    BranchUsd = monitorRecord.BranchUsd,
                    BranchZwg = monitorRecord.BranchZwg,
                    PhaseCode = phaseCode,
                };

                ViewBag.Phases =  _context.Phases
                    .Where(a => a.Status == "Active")
.Select(p => new
{
   Value = p.PhaseCode,
   Text = $"{p.PhaseName} - {p.StartTime:dd/MM/yyyy} to {p.EndTime:dd/MM/yyyy}"
}).ToList();

            return View(model);

        }

        public async Task<IActionResult> SendClaim(string claimId, string subkey)
        {

            // Validate inputs
            if (string.IsNullOrEmpty(claimId) || string.IsNullOrEmpty(subkey))
            {
                return BadRequest("Claim ID and SubKey are required");
            }

            // Parse claimId to Guid
            if (!Guid.TryParse(claimId, out Guid claimGuid))
            {
                return BadRequest("Invalid Claim ID format");
            }

            // Query database with Guid
            var availableTandS = await _context.ExamMonitorsClaimTandSs
                .FirstOrDefaultAsync(a => a.SubKey == subkey && a.ClaimID == claimGuid);

            if (availableTandS == null)
            {
                return NotFound("Claim not found");
            }
            var monitorRecord = await _context.ExamMonitors
          .FirstOrDefaultAsync(m => m.NationalId == availableTandS.NationalId);
            var transactionRecord = await _context.ExamMonitorTransactions
                .FirstOrDefaultAsync(a => a.SubKey == subkey & a.NationalId == availableTandS.NationalId);
       
                // If T&S exists, show Details view
                var dailyAdvances = await _context.ExamMonitorsDailyAdvances
                    .Where(a => a.SubKey == availableTandS.SubKey && a.ClaimID == availableTandS.ClaimID)
                    .ToListAsync();
                var stipendAdvance = await _context.ExamMonitorsStipendAdvances
                    .FirstOrDefaultAsync(a => a.SubKey == availableTandS.SubKey && a.ClaimID == availableTandS.ClaimID);
                var phaseCode = availableTandS.SubKey.Substring(2, 2);
                var session = availableTandS.SubKey.Substring(4, 3);
                var phase = await _context.Phases.FirstOrDefaultAsync(a => a.PhaseCode == phaseCode);
            decimal rate = 0;

            if (phase == null)
            {

            }
         
            if (transactionRecord.Status == "Cluster Manager")
            {
                rate = phase.ClusterManagerRate;
            }
            else if (transactionRecord.Status == "Assistant Cluster Manager")
            {
                rate = phase.AssistantClusterManagerRate;
            }
            else if (transactionRecord.Status == "Resident Monitor")
            {
                rate = phase.ResidentMonitorRate;
            }

            var model = new ExamMonitorTandSsViewModel
                {
                    Date = availableTandS.Date,
                    ClaimID = availableTandS.ClaimID,
                    NationalId = monitorRecord.NationalId,
                    DailyAdvances = dailyAdvances,
                    Rate = rate , // Null check for phase
                    FullName = $"{monitorRecord.FirstName} {monitorRecord.LastName}",
                    Session = session,
                    PhoneNumber = monitorRecord.Phone,
                    Days = stipendAdvance.Days,
                    SubKey = availableTandS.SubKey,
                    CentreAttached = transactionRecord.CentreAttached,
                    Station = monitorRecord.Station,
                    District = transactionRecord.District,
                    Region = transactionRecord.Region,
                    RoleStatus = transactionRecord.Status,
                    AccountNumberZwg = monitorRecord.AccountNumberZwg,
                    AccountNumberUsd = monitorRecord.AccountNumberUsd,
                    BankNameUsd = monitorRecord.BankNameUsd,
                    BankNameZwg = monitorRecord.BankNameZwg,
                    BranchUsd = monitorRecord.BranchUsd,
                    BranchZwg = monitorRecord.BranchZwg,
                    PhaseCode = phaseCode,
                };
           ViewBag.IsReadOnly = true;
            ViewBag.Phases = _context.Phases
                  .Where(a => a.Status == "Active")
.Select(p => new
{
  Value = p.PhaseCode,
  Text = $"{p.PhaseName} - {p.StartTime:dd/MM/yyyy} to {p.EndTime:dd/MM/yyyy}"
}).ToList();

            ViewBag.NationalId = model.NationalId;
            ViewBag.ClaimId = model.ClaimID;
            ViewBag.SubKey = model.SubKey;

            return View(model);
            }

        public async Task<IActionResult> SendClaimApproved(string claimId, string subkey)
        {

            // Validate inputs
            if (string.IsNullOrEmpty(claimId) || string.IsNullOrEmpty(subkey))
            {
                return BadRequest("Claim ID and SubKey are required");
            }

            // Parse claimId to Guid
            if (!Guid.TryParse(claimId, out Guid claimGuid))
            {
                return BadRequest("Invalid Claim ID format");
            }

            // Query database with Guid
            var availableTandS = await _context.ExamMonitorsClaimTandSs
                .FirstOrDefaultAsync(a => a.SubKey == subkey && a.ClaimID == claimGuid);

            if (availableTandS == null)
            {
                return NotFound("Claim not found");
            }
            var monitorRecord = await _context.ExamMonitors
          .FirstOrDefaultAsync(m => m.NationalId == availableTandS.NationalId);
            var transactionRecord = await _context.ExamMonitorTransactions
                .FirstOrDefaultAsync(a => a.SubKey == subkey & a.NationalId == availableTandS.NationalId);

            // If T&S exists, show Details view
            var dailyAdvances = await _context.ExamMonitorsDailyAdvances
                .Where(a => a.SubKey == availableTandS.SubKey && a.ClaimID == availableTandS.ClaimID)
                .ToListAsync();
            var stipendAdvance = await _context.ExamMonitorsStipendAdvances
                .FirstOrDefaultAsync(a => a.SubKey == availableTandS.SubKey && a.ClaimID == availableTandS.ClaimID);
            var phaseCode = availableTandS.SubKey.Substring(2, 2);
            var session = availableTandS.SubKey.Substring(4, 1);
            var phase = await _context.Phases.FirstOrDefaultAsync(a => a.PhaseCode == phaseCode);

            decimal rate = 0;

            if (phase == null)
            {

            }

            if (transactionRecord.Status == "Cluster Manager")
            {
                rate = phase.ClusterManagerRate;
            }
            else if (transactionRecord.Status == "Assistant Cluster Manager")
            {
                rate = phase.AssistantClusterManagerRate;
            }
            else if (transactionRecord.Status == "Resident Monitor")
            {
                rate = phase.ResidentMonitorRate;
            }

            var model = new ExamMonitorTandSsViewModel
            {
                Date = availableTandS.Date,
                ClaimID = availableTandS.ClaimID,
                NationalId = monitorRecord.NationalId,
                DailyAdvances = dailyAdvances,
                Rate = phase?.Rate ?? 0, // Null check for phase
                FullName = $"{monitorRecord.FirstName} {monitorRecord.LastName}",
                Session = session,
                PhoneNumber = monitorRecord.Phone,
                Days = stipendAdvance.Days,
                SubKey = availableTandS.SubKey,
                CentreAttached = transactionRecord.CentreAttached,
                Station = monitorRecord.Station,
                District = transactionRecord.District,
                Region = transactionRecord.Region,
                RoleStatus = transactionRecord.Status,
                AccountNumberZwg = monitorRecord.AccountNumberZwg,
                AccountNumberUsd = monitorRecord.AccountNumberUsd,
                BankNameUsd = monitorRecord.BankNameUsd,
                BankNameZwg = monitorRecord.BankNameZwg,
                BranchUsd = monitorRecord.BranchUsd,
                BranchZwg = monitorRecord.BranchZwg,
                PhaseCode = phaseCode,
            };
            ViewBag.IsReadOnly = true;
            ViewBag.Phases = _context.Phases
                  .Where(a => a.Status == "Active")
.Select(p => new
{
    Value = p.PhaseCode,
    Text = $"{p.PhaseName} - {p.StartTime:dd/MM/yyyy} to {p.EndTime:dd/MM/yyyy}"
}).ToList();

            ViewBag.NationalId = model.NationalId;
            ViewBag.ClaimId = model.ClaimID;
            ViewBag.SubKey = model.SubKey;

            return View(model);
        }

   

        [HttpPost]
        public async Task<IActionResult> RejectClaim([FromBody] ClaimApprovalRequest request)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var claimId = request.ClaimId ?? string.Empty;
                var subKey = request.SubKey ?? string.Empty;
                var nationalId = request.NationalId ?? string.Empty;
                // Validate inputs
                if ( string.IsNullOrEmpty(nationalId) || string.IsNullOrEmpty(subKey))
                {
                    return BadRequest("Invalid claim data provided");
                }


                if (!Guid.TryParse(claimId, out Guid claimGuid))
                {
                    return BadRequest("Invalid Claim ID format");
                }
                // Get the claim from database
                var claim = await _context.ExamMonitorsClaimTandSs
                    .FirstOrDefaultAsync(c => c.ClaimID == claimGuid &&
                                            c.NationalId == nationalId &&
                                            c.SubKey == subKey);

                if (claim == null)
                {
                    return NotFound("Claim not found");
                }


                // Update claim status
                claim.ReviewStatus = "Pending";
                claim.ReviewStatusDate = DateTime.Now;
                claim.ReviewStatusBy = currentUser.UserName;
                claim.InitiatorStatus = "Pending";
                claim.InitiatorStatusDate = DateTime.Now;
                claim.InitiatorStatusBy = currentUser.UserName;
                _context.ExamMonitorsClaimTandSs.Update(claim);
                // Save changes
                await _context.SaveChangesAsync();

                // Optional: Send notification to approvers


                return Json(new { success = true, message = "Claim approved successfully" });

            }
            catch (Exception ex)
            {
       
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveReviewClaim([FromBody] ClaimApprovalRequest request)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                // Validate inputs
                var claimId = request.ClaimId ?? string.Empty;
                var subKey = request.SubKey ?? string.Empty;
                var nationalId = request.NationalId ?? string.Empty;
                // Validate inputs
                if (string.IsNullOrEmpty(nationalId) || string.IsNullOrEmpty(subKey))
                {
                    return BadRequest("Invalid claim data provided");
                }


                if (!Guid.TryParse(claimId, out Guid claimGuid))
                {
                    return BadRequest("Invalid Claim ID format");
                }

                // Get the claim from database
                var claim = await _context.ExamMonitorsClaimTandSs
                    .FirstOrDefaultAsync(c => c.ClaimID == claimGuid &&
                                            c.NationalId == nationalId &&
                                            c.SubKey == subKey);

                if (claim == null)
                {
                    return NotFound("Claim not found");
                }

                // Check if claim is already approved/rejected
                if (claim.ReviewStatus == "Approved")
                {
                    return BadRequest("This claim has already been approved");
                }



                // Update claim status
                claim.ReviewStatus = "Approved";
                claim.ReviewStatusDate = DateTime.Now;
                claim.ReviewStatusBy = currentUser.UserName;
                _context.ExamMonitorsClaimTandSs.Update(claim);
                // Save changes
                await _context.SaveChangesAsync();

                // Optional: Send notification to approvers


                return Json(new { success = true, message = "Claim approved successfully" });
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "An error occurred while processing your request" });
            }
        }
        public async Task<IActionResult> Generate()
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);


            ViewBag.Phases = _context.Phases
        .Select(p => new {
            Value = p.PhaseCode,
            Text = p.PhaseName
        })
        .OrderBy(p => p.Text)
        .ToList();

            ViewBag.Sessions = _context.ExamSessions
                                .Select(s => new {
                                    Value = s.SessionCode,
                                    Text = s.SessionName
                                })
                .OrderBy(s => s.Text)
                .ToList();

            if (userRoles.Contains("RegionalManager"))
            {
                ViewBag.RegionCode = currentUser.Region;

            }


            return View();

        }

        [HttpPost]
        public async Task<IActionResult> GenerateClaims(string session = "", string phaseCode = "", string region = "")
        {

           // var lists = new List<ExamMonitorRegister>();
      
           //var data = new ExamMonitorRegister
           // {
           //     Id = 1,
           //     SubKey = "01P1N2540295607I75",
           //     NationalId = "40295607I75",
           //     Date = "2025-08-10",
           //     Comment = "General comment 1",
           //     CompiledStatus = "Approved",
           //     CompiledBy = "User1",
           //     CompiledDate = "2025-08-11",
           //     ClusterManagerStatus = "Pending",
           //     ClusterManagerBy = "ClusterMgr1",
           //     ClusterManagerDate = "2025-08-12",
           //     RegionalManagerStatus = "Approved",
           //     RegionalManagerBy = "RegionMgr1",
           //     RegionalManagerDate = "2025-08-14",
           //     IsPresent = true,
           //     IsPresentBy = "Admin1",
           //     IsPresentDate = "2025-08-15",
           //     ExamMonitorRegisterDates = new List<ExamMonitorRegisterDate>
           //     {
           //         new ExamMonitorRegisterDate
           //         {
           //             Id = 1,
           //             SubKey = "01P1N2540295607I75",
           //             Date = new DateTime(2025, 8, 10),
           //             Comment = "Date specific comment 1 for SUB001",
           //             CompiledStatus = "Pending",
           //             CompiledBy = "Compiler1",
           //             CompiledDate = new DateTime(2025, 8, 11),
           //             ClusterManagerStatus = "Approved",
           //             ClusterManagerBy = "ClusterMgr1",
           //             ClusterManagerDate = new DateTime(2025, 8, 12),
           //             RegionalManagerStatus = "Pending",
           //             RegionalManagerBy = "RegionMgr1",
           //             RegionalManagerDate = new DateTime(2025, 8, 13),
           //             IsPresent = true
           //         },
           //         new ExamMonitorRegisterDate
           //         {
           //             Id = 2,
           //             SubKey = "01P1N2540295607I75",
           //             Date = new DateTime(2025, 8, 11),
           //             Comment = "Date specific comment 2 for SUB001",
           //             CompiledStatus = "Approved",
           //             CompiledBy = "Compiler2",
           //             CompiledDate = new DateTime(2025, 8, 12),
           //             ClusterManagerStatus = "Rejected",
           //             ClusterManagerBy = "ClusterMgr2",
           //             ClusterManagerDate = new DateTime(2025, 8, 13),
           //             RegionalManagerStatus = "Approved",
           //             RegionalManagerBy = "RegionMgr2",
           //             RegionalManagerDate = new DateTime(2025, 8, 14),
           //             IsPresent = false
           //         }
           //     }
           // };

           // lists.Add(data);
                var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }
                var key = currentUser.Region + phaseCode + session;

            var registers = await _context.ExamMonitorsRegisters.Where(a => a.SubKey.StartsWith(key) && a.RegionalManagerStatus == "Approved").ToListAsync();

                var newRegistersList = new List<ExamMonitorRegister>();
                foreach (var item in registers)
                {
                var dailyRegisters = await _context.ExamMonitorRegisterDates.Where(a => a.SubKey == item.SubKey).ToListAsync();


                    var newReg = new ExamMonitorRegister()
                    {
                        SubKey = item.SubKey,
                        NationalId = item.NationalId,
                    };

                    if (dailyRegisters.Any())
                    {
                        newReg.RegisterDates = dailyRegisters;
                    }

                    newRegistersList.Add(newReg);
                }


                foreach (var item in newRegistersList)
                {

                    // Get phase information
                    var currentPhase = await _context.Phases
                        .AsNoTracking() // Important for read-only operations
                        .FirstOrDefaultAsync(a => a.PhaseCode == phaseCode);
                    var transaction = await _context.ExamMonitorTransactions.FirstOrDefaultAsync(a => a.SubKey == item.SubKey);
                    var dailyClaims = new List<DailyAdvances>();
                    decimal stipendAmount = 0;
                    var days = 0;
                    decimal rate = 0;
                    if (transaction.Status == "Cluster Manager")
                    {
                        foreach (var item2 in item.RegisterDates)
                        {
                            var newD = new DailyAdvances()
                            {
                                Date = item2.Date,
                                Lunch = currentPhase.LunchRate,


                            };
                            dailyClaims.Add(newD);
                        }
                        days = dailyClaims.Count();
                        stipendAmount = days * currentPhase.ClusterManagerRate;
                        rate = currentPhase.ClusterManagerRate;
                    }
                    else if (transaction.Status == "Assistant Cluster Manager")
                    {
                        foreach (var item2 in item.RegisterDates)
                        {
                            var newD = new DailyAdvances()
                            {
                                Date = item2.Date,
                                Lunch = currentPhase.LunchRate,

                            };
                            dailyClaims.Add(newD);
                        }
                        days = dailyClaims.Count();
                        stipendAmount = days * currentPhase.AssistantClusterManagerRate;
                        rate = currentPhase.AssistantClusterManagerRate;
                    }
                    else if (transaction.Status == "Resident Monitor")
                    {
                        foreach (var item2 in item.RegisterDates)
                        {
                            var newD = new DailyAdvances()
                            {
                                Date = item2.Date,
                                Lunch = currentPhase.LunchRate,
                                Accomodation = currentPhase.AccomodationRate,
                                Dinner = currentPhase.DinnerRate,
                                Breakfast = currentPhase.BreakFastRate,

                            };
                            dailyClaims.Add(newD);
                        }
                        days = dailyClaims.Count();
                        stipendAmount = days * currentPhase.ResidentMonitorRate;
                        rate = currentPhase.ResidentMonitorRate;
                    }


                    var model = new ExamMonitorTandSsViewModel()
                    {
                        SubKey = item.SubKey,
                        NationalId = item.NationalId,
                        PhaseCode = phaseCode,
                        Session = session,
                        ClaimID = Guid.NewGuid(),
                        Region = transaction.Region,
                        CentreAttached = transaction.CentreAttached,
                        Days = days,
                        Date = DateTime.Now.ToString(),
                        DinnerRate = currentPhase.DinnerRate,
                        BreakFastRate = currentPhase.BreakFastRate,
                        AccomodationRate = currentPhase.AccomodationRate,
                        LunchRate = currentPhase.LunchRate,
                        ClusterManagerRate = currentPhase.ClusterManagerRate,
                        AssistantClusterManagerRate = currentPhase.AssistantClusterManagerRate,
                        ResidentMonitorRate = currentPhase.ResidentMonitorRate,
                        DailyAdvances = dailyClaims,
                    };

                    decimal dailyTotal = model.DailyAdvances.Sum(item =>
                        item.Breakfast + item.Lunch + item.Dinner + item.Accomodation);

                    // Create and save the main claim
                    var claim = new ExamMonitorTandS
                    {
                        ClaimID = model.ClaimID,
                        Days = model.Days,
                        SubKey = model.SubKey,
                        Session = model.Session,
                        Phase = model.PhaseCode,
                        ClusterManagerStatus = "Approved",
                        ClusterManagerStatusBy = currentUser.UserName,
                        ClusterManagerStatusDate = DateTime.Now,
                        Region = model.Region,
                        RegionalManagerStatus = "Approved",
                        RegionalManagerStatusBy = currentUser.UserName,
                        RegionalManagerStatusDate = DateTime.Now,
                        Date = DateTime.Now.ToString(),
                        NationalId = model.NationalId,
                        CentreAttached = model.CentreAttached,
                        PaidStatusComment = "New Claim",
                        PaidAmount = 0,
                        PaidStatus = "Pending",
                        PaidStatusDate = DateTime.Now.ToString(),
                        PaidStatusBy = currentUser.UserName,
                        TotalAmount = stipendAmount + dailyTotal,
                        InitiatorStatus = "Pending",
                        InitiatorStatusBy = currentUser.UserName,
                        InitiatorStatusDate = DateTime.Now,
                        ReviewStatus = "Pending",
                        ReviewStatusBy = currentUser.UserName,
                        ReviewStatusDate = DateTime.Now,
                    };

                    await _context.ExamMonitorsClaimTandSs.AddAsync(claim);
                    await _context.SaveChangesAsync();
                    // Create and save stipend record
                    var stipendRecord = new StipendAdvance
                    {
                        Days = model.Days,
                        ClaimID = model.ClaimID,
                        SubKey = model.SubKey,
                        Rate = rate,
                        PhaseCode = model.PhaseCode,
                        TotalAmount = stipendAmount,
                    };
                    await _context.ExamMonitorsStipendAdvances.AddAsync(stipendRecord);
                    await _context.SaveChangesAsync();
                    // Create daily advances
                    var dailyAdvances = model.DailyAdvances.Select(item => new DailyAdvances
                    {
                        Date = item.Date,
                        Dinner = item.Dinner,
                        Lunch = item.Lunch,
                        Accomodation = item.Accomodation,
                        Breakfast = item.Breakfast,
                        ClaimID = model.ClaimID,
                        SubKey = model.SubKey,
                        TotalAmount = item.Breakfast + item.Lunch + item.Dinner + item.Accomodation,
                    }).ToList();


                    // Add all and save once
                    await _context.ExamMonitorsDailyAdvances.AddRangeAsync(dailyAdvances);
                    await _context.SaveChangesAsync();

                }

            return Json(new
            {
                success = true,
                message = $"Successfully generated claims for {newRegistersList.Count} monitors",
                count = newRegistersList.Count
            });


        }

        [HttpPost]
        public async Task<IActionResult> SaveDailyAdvances(ExamMonitorTandSsViewModel model)
        {
            try
            {

       
                // Get current user
                var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Get phase information
                var phase = await _context.Phases
                    .AsNoTracking() // Important for read-only operations
                    .FirstOrDefaultAsync(a => a.PhaseCode == model.PhaseCode);

                if (phase == null)
                {
                    ModelState.AddModelError("", "Invalid phase selected");
                    return View(model);
                }

                // Calculate totals
                var stipendAmount = model.Days * phase.Rate;
                decimal dailyTotal = model.DailyAdvances.Sum(item =>
                    item.Breakfast + item.Lunch + item.Dinner + item.Accomodation);

                var claim = await _context.ExamMonitorsClaimTandSs.FirstOrDefaultAsync(a => a.SubKey == model.SubKey && a.ClaimID == model.ClaimID);

                // Create and save the main claim
             
                    
                    claim.Days = model.Days;


                claim.TotalAmount = stipendAmount + dailyTotal;
                claim.InitiatorStatus = "Approved";
                claim.InitiatorStatusBy = currentUser.UserName;
                claim.InitiatorStatusDate = DateTime.Now;

                _context.ExamMonitorsClaimTandSs.Update(claim);
                await _context.SaveChangesAsync();

                var stipendRecord = await _context.ExamMonitorsStipendAdvances.FirstOrDefaultAsync();



                stipendRecord.Days = model.Days;
                stipendRecord.TotalAmount = stipendAmount;

                _context.ExamMonitorsStipendAdvances.Update(stipendRecord);
                await _context.SaveChangesAsync();

              

                foreach (var item in model.DailyAdvances)
                {
                    var day = await _context.ExamMonitorsDailyAdvances.FirstOrDefaultAsync(a => a.ClaimID == claim.ClaimID && a.SubKey == claim.SubKey);
                    day.Date = item.Date;
                    day.Dinner = item.Dinner;
                    day.Lunch = item.Lunch;
                    day.Accomodation = item.Accomodation;
                    day.Breakfast = item.Breakfast;

                    day.TotalAmount = item.Breakfast + item.Lunch + item.Dinner + item.Accomodation;

                _context.ExamMonitorsDailyAdvances.Update(day);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("IndexAccounts");
            }
            catch (Exception ex)
            {
                // Log the error


                // Add error to model state
                return RedirectToAction("Create");

                // Return to view with model to show errors
              
            }
        }

        [HttpGet]
        public IActionResult Edit(string claimId, string nationalId, string subKey)
        {
            //// Verify parameters
            //if (string.IsNullOrEmpty(claimId) || string.IsNullOrEmpty(nationalId) || string.IsNullOrEmpty(subKey))
            //{
            //    TempData["ErrorMessage"] = "Required parameters are missing";
            //    return RedirectToAction("Index");
            //}

            //// Get claim from database
            //var claim = _context.Claims
            //    .Include(c => c.DailyAdvances)
            //    .FirstOrDefault(c => c.ClaimID == claimId &&
            //                       c.NationalId == nationalId &&
            //                       c.SubKey == subKey);

            //if (claim == null)
            //{
            //    TempData["ErrorMessage"] = "Claim not found or access denied";
            //    return RedirectToAction("Index");
            //}

            //// Check if editable
            //if (claim.CurrentStatus != "Submitted" || !claim.IsEditEnabled)
            //{
            //    TempData["ErrorMessage"] = "This claim cannot be edited at this time";
            //    return RedirectToAction("Details", new { id = claimId });
            //}

            // Map to ViewModel
            //var model = new ExamMonitorTandSsViewModel
            //{
            //    ClaimID = claim.ClaimID,
            //    NationalId = claim.NationalId,
            //    SubKey = claim.SubKey,
            //    // Map other properties...
            //    DailyAdvances = claim.DailyAdvances,
            //    IsEditEnabled = true,
            //    CurrentStatus = claim.CurrentStatus
            //};

            // Rebuild select lists if needed
            //ViewBag.Phases = new SelectList(_context.Phases, "PhaseCode", "DisplayName");

            return View();
        }

  
    }

    public class ClaimApprovalRequest
    {
        public string ClaimId { get; set; }
        public string NationalId { get; set; }
        public string SubKey { get; set; }
    }
}
