using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExamMonitors;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Repositories;
using ExaminerPaymentSystem.ViewModels;
using ExaminerPaymentSystem.ViewModels.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ExaminerPaymentSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRegisterRepository _registerRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IExaminerRepository _examinerRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITandSRepository _tandSRepository;
        private readonly IOptions<AppInfo> _appInfo;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IRegisterRepository registerRepository, SignInManager<ApplicationUser> signInManager, IExaminerRepository examinerRepository, UserManager<ApplicationUser> userManager, ITandSRepository tandSRepository, IOptions<AppInfo> appInfo, ApplicationDbContext context)
        {
            _logger = logger;
            _registerRepository = registerRepository;
            _signInManager = signInManager;
            _examinerRepository = examinerRepository;
            _userManager = userManager;
            _tandSRepository = tandSRepository;
            _appInfo = appInfo;
            _context = context;
        }


        // Add this action for AJAX filtering
        [HttpPost]
        public async Task<IActionResult> GetFilteredData([FromBody] FilterModel filters)
        {
            try
            {
                // Initialize with default values if null
                filters ??= new FilterModel();
                var regionFilter = filters.Region ?? string.Empty;

                // Ensure User is not null
                if (User?.Identity == null || !User.Identity.IsAuthenticated)
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                // Get the current user
                var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }

                // Get user roles
                var userRoles = await _userManager.GetRolesAsync(currentUser);
                if (userRoles == null || !userRoles.Any())
                {
                    return Forbid();
                }

                // Check if the user has any of the specified roles for redirect
                var allowedRoles = new[] { "PMS", "BMS", "Examiner", "DPMS", "RPMS", "A", "BT", "PBT" };
                if (userRoles.Any(role => allowedRoles.Contains(role)))
                {
                    string idNumber = currentUser.IDNumber;
                    string subKey = currentUser.EMS_SUBKEY;
                    string examinerCode = currentUser.ExaminerCode;

                    // Get examiner details
                    var examiner = await _examinerRepository.GetExaminer(idNumber, subKey, examinerCode);
                    if (examiner == null)
                    {
                        return BadRequest(new { error = "Examiner data not found" });
                    }

                    // Check attendance status
                    var checkRegister = await _registerRepository.CheckExaminerRegister(idNumber);
                    if (checkRegister != null && checkRegister.AttendanceStatus == "Pending")
                    {
                        // Return a flag instead of redirecting for API call
                        return Ok(new { redirectRequired = true });
                    }
                }

                // **CRITICAL: Use IQueryable for database-level filtering**
                IQueryable<ExamMonitor> monitorsQuery = _context.ExamMonitors;
                IQueryable<ExamMonitorsRecruitment> recruitmentsQuery = _context.ExamMonitorsRecruitments;
                IQueryable<ExamMonitorTransaction> transactionsQuery = _context.ExamMonitorTransactions;
                IQueryable<Centre> centresQuery = _context.Centres;
                IQueryable<Models.ExamMonitors.ExamMonitorRegister> registersQuery = _context.ExamMonitorsRegisters;
                IQueryable<ExamMonitorTandS> claimsQuery = _context.ExamMonitorsClaimTandSs;

                // Get active phases
                var activePhases = await _context.Phases
                    .Where(p => p.Status == "Active")
                    .Select(p => p.PhaseCode)
                    .ToListAsync();

                // Apply region filtering at database level
                if (userRoles.Contains("RegionalManager"))
                {
                    string userRegion = currentUser.Region;

                    if (!string.IsNullOrEmpty(userRegion))
                    {
                        monitorsQuery = monitorsQuery.Where(m => m.Region == userRegion);
                        recruitmentsQuery = recruitmentsQuery.Where(m => m.Region == userRegion);
                        transactionsQuery = transactionsQuery.Where(m => m.Region == userRegion);
                        centresQuery = centresQuery.Where(c => c.RegionCode == userRegion);
                        registersQuery = registersQuery.Where(r => r.SubKey.StartsWith(userRegion));
                        claimsQuery = claimsQuery.Where(c => c.Region == userRegion);
                    }
                }
                else if (!string.IsNullOrEmpty(regionFilter) && regionFilter != "all")
                {
                    // Apply region filter from request
                    monitorsQuery = monitorsQuery.Where(m => m.Region == regionFilter);
                    recruitmentsQuery = recruitmentsQuery.Where(m => m.Region == regionFilter);
                    transactionsQuery = transactionsQuery.Where(m => m.Region == regionFilter);
                    centresQuery = centresQuery.Where(c => c.RegionCode == regionFilter);
                    registersQuery = registersQuery.Where(r => r.SubKey.StartsWith(regionFilter));
                    claimsQuery = claimsQuery.Where(c => c.Region == regionFilter);
                }

                // Execute filtered queries
                var monitors = await monitorsQuery.ToListAsync();
                var recruitments = await recruitmentsQuery.ToListAsync();

                var rejectedMonitors = await recruitmentsQuery
                    .Where(a => a.AcceptStatus.ToLower() == "rejected")
                    .ToListAsync();

                var pendingRecruitments = await recruitmentsQuery
                    .Where(a => a.AcceptStatus.ToLower() == "pending")
                    .ToListAsync();

                var acceptedMonitors = await monitorsQuery
                    .Where(a => a.AcceptStatus.ToLower() == "accepted")
                    .ToListAsync();

                var selectedMonitors = await transactionsQuery
                    .Where(t => activePhases.Contains(t.Phase))
                    .ToListAsync();

                var deployedMonitors = await transactionsQuery
                    .Where(t => t.CentreAttached != "000000" && activePhases.Contains(t.Phase))
                    .ToListAsync();

                var clusterManagers = await transactionsQuery
                    .Where(a => a.Status == "Cluster Manager")
                    .ToListAsync();

                var assistantClusterManagers = await transactionsQuery
                    .Where(a => a.Status == "Assistant Cluster Manager")
                    .ToListAsync();

                var residentMonitors = await transactionsQuery
                    .Where(a => a.Status == "Resident Monitor")
                    .ToListAsync();

                var centres = await centresQuery.ToListAsync();
                var clusters = await centresQuery
                    .Where(a => a.IsCluster == "IsCluster")
                    .ToListAsync();

                var isResidentList = await centresQuery
                    .Where(a => a.IsResident == "IsResident")
                    .ToListAsync();

                var registers = await registersQuery.ToListAsync();
                var claims = await claimsQuery.ToListAsync();

                // Apply role-based filtering to claims
                IQueryable<ExamMonitorTandS> filteredClaimsQuery = claimsQuery;

                if (userRoles.Contains("Accounts"))
                {
                    filteredClaimsQuery = filteredClaimsQuery.Where(a => a.InitiatorStatus == "Approved");
                }
                else if (userRoles.Contains("PeerReviewer"))
                {
                    filteredClaimsQuery = filteredClaimsQuery.Where(a => a.ReviewStatus == "Approved");
                }

                var approvedClaims = await filteredClaimsQuery.ToListAsync();

                var pendingClaimsList = userRoles.Contains("Accounts")
                    ? await claimsQuery.Where(a => a.InitiatorStatus == "Pending").ToListAsync()
                    : userRoles.Contains("PeerReviewer")
                        ? await claimsQuery.Where(a => a.ReviewStatus == "Pending").ToListAsync()
                        : new List<ExamMonitorTandS>();

                var totalAmount = await claimsQuery.SumAsync(a => a.TotalAmount);

                // Calculate user-specific data
                var centreAttached = "Not Available";
                var roleStatus = "Not Available";
                var totalDays = 0;
                var presentDays = 0;
                var absentDays = 0;
                var extraDays = 0;

                if (userRoles.Contains("ClusterManager") || userRoles.Contains("AssistantClusterManager") || userRoles.Contains("ResidentMonitor"))
                {
                    var transaction = await _context.ExamMonitorTransactions
                        .FirstOrDefaultAsync(a => a.SubKey == currentUser.EMS_SUBKEY && a.NationalId == currentUser.IDNumber);

                    var register = await _context.ExamMonitorsRegisters
                        .Include(a => a.RegisterDates)
                        .FirstOrDefaultAsync(a => a.SubKey == currentUser.EMS_SUBKEY && a.NationalId == currentUser.IDNumber);

                    if (transaction != null)
                    {
                        var centre = await _context.Centres
                            .FirstOrDefaultAsync(a => a.CentreNumber == transaction.CentreAttached);

                        if (centre != null)
                        {
                            centreAttached = transaction.CentreAttached + " " + centre.CentreName;
                        }

                        if (register != null)
                        {
                            presentDays = register.RegisterDates.Count(a => a.IsPresent);
                            absentDays = register.RegisterDates.Count(a => !a.IsPresent);
                        }

                        roleStatus = transaction.Status;

                        var phase = await _context.Phases
                            .FirstOrDefaultAsync(p => p.PhaseCode == transaction.Phase);

                        if (phase != null)
                        {
                            IQueryable<DateTime> examDatesQuery;

                            if (userRoles.Contains("ResidentMonitor"))
                            {
                                examDatesQuery = _context.Exm_Timetable
                                    .Where(t => t.CentreCode == transaction.CentreAttached
                                               && t.Exam_date >= phase.StartTime.Date
                                               && t.Exam_date <= phase.EndTime.Date)
                                    .Select(t => t.Exam_date);
                            }
                            else if (userRoles.Contains("ClusterManager") || userRoles.Contains("AssistantClusterManager"))
                            {
                                var clusterCode = await _context.Centres
                                    .Where(c => c.CentreNumber == transaction.CentreAttached)
                                    .Select(c => c.ClusterCode)
                                    .FirstOrDefaultAsync();

                                var centresInCluster = await _context.Centres
                                    .Where(c => c.ClusterCode == clusterCode)
                                    .Select(c => c.CentreNumber)
                                    .ToListAsync();

                                examDatesQuery = _context.Exm_Timetable
                                    .Where(t => centresInCluster.Contains(t.CentreCode)
                                               && t.Exam_date >= phase.StartTime.Date
                                               && t.Exam_date <= phase.EndTime.Date)
                                    .Select(t => t.Exam_date);
                            }
                            else
                            {
                                examDatesQuery = _context.Exm_Timetable
                                    .Where(t => t.CentreCode == transaction.CentreAttached
                                               && t.Exam_date >= phase.StartTime.Date
                                               && t.Exam_date <= phase.EndTime.Date)
                                    .Select(t => t.Exam_date);
                            }

                            var actualExamDates = await examDatesQuery
                                .Distinct()
                                .OrderBy(d => d)
                                .ToListAsync();

                            // Add travel days
                            var allDates = new List<DateTime>();
                            if (actualExamDates.Any())
                            {
                                var firstExamDate = actualExamDates.Min();
                                var lastExamDate = actualExamDates.Max();

                                allDates.Add(firstExamDate.AddDays(-2));
                                allDates.Add(firstExamDate.AddDays(-1));
                                allDates.AddRange(actualExamDates);
                                allDates.Add(lastExamDate.AddDays(1));

                                totalDays = allDates.Count();
                            }

                            var extraDates = await _context.ExamMonitorRegisterDates
                                .Where(a => a.IsSupervisorAdded && a.SubKey == currentUser.EMS_SUBKEY)
                                .ToListAsync();

                            extraDays = extraDates.Count;
                            totalDays += extraDays;
                        }
                    }
                }

                // Calculate weekly data
                var weeklyData = new List<int>();
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);
                    var dayCount = registers.Count(r => r.Date == date.Date.ToString("yyyy-MM-dd") && r.IsPresent);
                    weeklyData.Add(dayCount);
                }

                var totalMonitorsCount = monitors.Count + recruitments.Count;

                return Ok(new
                {
                    extraDays = extraDays,
                    totalMonitors = totalMonitorsCount,
                    rejectedMonitors = rejectedMonitors.Count,
                    acceptedMonitors = acceptedMonitors.Count,
                    pendingMonitors = pendingClaimsList.Count,
                    deployedMonitors = deployedMonitors.Count,
                    selectedMonitors = selectedMonitors.Count,
                    totalClaims = claims.Count,
                    totalAmount = totalAmount,
                    approvedClaims = approvedClaims.Count,
                    initiator = "Initiator A",
                    reviewer = "Reviewer A",
                    regions = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" },
                    clusterData = clusters.Count,
                    centreData = centres.Count,
                    isResident = isResidentList.Count,
                    clusterManagers = clusterManagers.Count,
                    residentMonitors = residentMonitors.Count,
                    assistantClusterManagers = assistantClusterManagers.Count,
                    presentToday = registers.Count(r => r.Date == DateTime.Today.ToString("yyyy-MM-dd") && r.IsPresent),
                    totalRegisters = registers.Count,
                    weeklyPresent = weeklyData,
                    totalDays = totalDays,
                    presentDays = presentDays,
                    absentDays = absentDays,
                    centreAttached = centreAttached,
                    roleStatus = roleStatus,
                });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error in GetFilteredData");
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }



        public async Task<IActionResult> Index()
        {
            // Ensure User is not null
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                ViewBag.SystemName = _appInfo.Value.SystemName;
                return View();
            }
            else
            {
                // Get the current user
                var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account"); // Redirect to Login if user retrieval fails
                }

                // Get user roles
                var userRoles = await _userManager.GetRolesAsync(currentUser);
                if (userRoles == null || !userRoles.Any())
                {
                    ViewBag.SystemName = _appInfo.Value.SystemName;
                    return View(); // Return the view if the user has no roles
                }

                // Check if the user has any of the specified roles
                var allowedRoles = new[] { "PMS", "BMS", "Examiner", "DPMS", "RPMS", "A", "BT", "PBT" };
                if (userRoles.Any(role => allowedRoles.Contains(role)))
                {
                    string idNumber = currentUser.IDNumber;
                    string subKey = currentUser.EMS_SUBKEY;
                    string examinerCode = currentUser.ExaminerCode;

                    // Get examiner details
                    var examiner = await _examinerRepository.GetExaminer(idNumber, subKey, examinerCode);
                    if (examiner == null)
                    {
                        return View("Error"); // Show error view if examiner data is not found
                    }

                    // Check attendance status
                    var checkRegister = await _registerRepository.CheckExaminerRegister(idNumber);
                    if (checkRegister != null && checkRegister.AttendanceStatus == "Pending")
                    {
                        // Prepare data for redirection
                        var redirectData = new
                        {
                            username = currentUser.UserName,
                            firstname = examiner.EMS_EXAMINER_NAME,
                            lastname = examiner.EMS_LAST_NAME,
                            subject = examiner.EMS_SUB_SUB_ID,
                            papercode = examiner.EMS_PAPER_CODE,
                            subkey = subKey,
                            examinerCode,
                            idNumber
                        };

                        // Redirect to ConfirmAttendance
                        return RedirectToAction("ConfirmAttendance", "ExaminerRegister", redirectData);
                    }
                }

            }

            ViewBag.SystemName = _appInfo.Value.SystemName;

            var currentUserr = await _signInManager.UserManager.GetUserAsync(User);
            if (currentUserr == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to Login if user retrieval fails
            }
            var userRoless = await _userManager.GetRolesAsync(currentUserr);
            var monitors = await _context.ExamMonitors.ToListAsync();
            var recruitments= await _context.ExamMonitorsRecruitments.ToListAsync();
            
            var rejectedMonitors = recruitments.Where(a => a.AcceptStatus.ToLower() == "rejected").ToList();
            var pendingMonitors = recruitments.Where(a => a.AcceptStatus.ToLower() == "pending").ToList();
            var acceptedMonitors = monitors.Where(a => a.AcceptStatus == "accepted").ToList();
            var deployedMonitors = await _context.ExamMonitorTransactions.ToListAsync();
            var activePhases = await _context.Phases
      .Where(p => p.Status == "Active")
      .Select(p => p.PhaseCode)
      .ToListAsync();

            // Get all transactions for active phases
            var selectedMonitors = await _context.ExamMonitorTransactions
                .Where(t => activePhases.Contains(t.Phase))
                .ToListAsync();

            

            var clusterManagers = await _context.ExamMonitorTransactions.Where(a => a.Status == "Cluster Manager").ToListAsync();
            var assistantClusterManagers = await _context.ExamMonitorTransactions.Where(a => a.Status == "Assistant Cluster Manager").ToListAsync();

            var residentMonitors = await _context.ExamMonitorTransactions.Where(a => a.Status == "Resident Monitor").ToListAsync();

            var centres = await _context.Centres.ToListAsync();
            var clusters = await _context.Centres.Where(a => a.IsCluster == "IsCluster").ToListAsync();
            var isResident = await _context.Centres.Where(a => a.IsResident == "IsResident").ToListAsync();

            var registers = await _context.ExamMonitorsRegisters.ToListAsync();
            var claims = await _context.ExamMonitorsClaimTandSs.ToListAsync();
            var approvedClaims = claims.ToList();
            var pendingClaims = claims.ToList();
            var totalAmount = claims.Sum(a => a.TotalAmount);


            if (userRoless.Contains("RegionalManager"))
            {
                monitors = monitors.Where(m => m.Region == currentUserr.Region).ToList();
                selectedMonitors = selectedMonitors.Where(m => m.Region == currentUserr.Region).ToList();
                clusterManagers = clusterManagers.Where(m => m.Region == currentUserr.Region).ToList();
                assistantClusterManagers = assistantClusterManagers.Where(m => m.Region == currentUserr.Region).ToList();
                residentMonitors = residentMonitors.Where(m => m.Region == currentUserr.Region).ToList();
                centres = centres.Where(c => c.RegionCode == currentUserr.Region).ToList();
                clusters = clusters.Where(c => c.RegionCode == currentUserr.Region).ToList();
                isResident = isResident.Where(c => c.RegionCode == currentUserr.Region).ToList();
                registers = registers.Where(r => r.SubKey.StartsWith(currentUserr.Region)).ToList();
                claims = claims.Where(c => c.Region == currentUserr.Region).ToList();
           
            }
            var centreAttached = "Not Available";
            var roleStatus = "Not Available";
        
            var totalDays = 0;
            var presentDays = 0;
            var absentDays = 0;
            var extraDays = 0;
            if (userRoless.Contains("ClusterManager") || userRoless.Contains("AssistantClusterManager") || userRoless.Contains("ResidentMonitor"))
            {
                var transaction = await _context.ExamMonitorTransactions.FirstOrDefaultAsync(a => a.SubKey == currentUserr.EMS_SUBKEY && a.NationalId
                == currentUserr.IDNumber);
                var register = await _context.ExamMonitorsRegisters.Include(a => a.RegisterDates)
                    .FirstOrDefaultAsync(a => a.SubKey == currentUserr.EMS_SUBKEY && a.NationalId == currentUserr.IDNumber);
                
                if (transaction != null )
                {
                
                    var centre = await _context.Centres.FirstOrDefaultAsync(a => a.CentreNumber == transaction.CentreAttached);
                    if (centre != null)
                    {
                        centreAttached = transaction.CentreAttached + " " + centre.CentreName;
                    }
                        if(register != null)
                    {

                        presentDays = register.RegisterDates.Count(a => a.IsPresent);
                        absentDays = register.RegisterDates.Count(a => !a.IsPresent);
                    }

                    roleStatus = transaction.Status;
                    var phase = await _context.Phases
           .FirstOrDefaultAsync(p => p.PhaseCode == transaction.Phase);
                    IQueryable<DateTime> examDatesQuery;

                    if (userRoless.Contains("ResidentMonitor"))
                    {
                        examDatesQuery = _context.Exm_Timetable
                            .Where(t => t.CentreCode == transaction.CentreAttached
                                       && t.Exam_date >= phase.StartTime.Date
                                       && t.Exam_date <= phase.EndTime.Date)
                            .Select(t => t.Exam_date);
                    }
                    else if (userRoless.Contains("ClusterManager") || userRoless.Contains("AssistantClusterManager"))
                    {
                        var clusterCode = await _context.Centres
                            .Where(c => c.CentreNumber == transaction.CentreAttached)
                            .Select(c => c.ClusterCode)
                            .FirstOrDefaultAsync();

                        var centresInCluster = await _context.Centres
                            .Where(c => c.ClusterCode == clusterCode)
                            .Select(c => c.CentreNumber)
                            .ToListAsync();

                        examDatesQuery = _context.Exm_Timetable
                            .Where(t => centresInCluster.Contains(t.CentreCode)
                                       && t.Exam_date >= phase.StartTime.Date
                                       && t.Exam_date <= phase.EndTime.Date)
                            .Select(t => t.Exam_date);
                    }
                    else
                    {
                        examDatesQuery = _context.Exm_Timetable
                            .Where(t => t.CentreCode == transaction.CentreAttached
                                       && t.Exam_date >= phase.StartTime.Date
                                       && t.Exam_date <= phase.EndTime.Date)
                            .Select(t => t.Exam_date);
                    }

                    var actualExamDates = await examDatesQuery
                        .Distinct()
                        .OrderBy(d => d)
                        .ToListAsync();

                    // Add travel days: 2 days before first exam and 1 day after last exam
                    var allDates = new List<DateTime>();

                    if (actualExamDates.Any())
                    {
                        var firstExamDate = actualExamDates.Min();
                        var lastExamDate = actualExamDates.Max();

                        // Add 2 days before first exam
                        allDates.Add(firstExamDate.AddDays(-2));
                        allDates.Add(firstExamDate.AddDays(-1));

                        // Add all actual exam dates
                        allDates.AddRange(actualExamDates);

                        // Add 1 day after last exam
                        allDates.Add(lastExamDate.AddDays(1));
                        totalDays = allDates.Count();
                    }

                    var extraDates = await _context.ExamMonitorRegisterDates.Where(a => a.IsSupervisorAdded && a.SubKey == currentUserr.EMS_SUBKEY).ToListAsync();
                    extraDays = extraDates.Count();
                    totalDays = totalDays + extraDays;
                }
            }

          


            if (userRoless.Contains("Accounts"))

            { 
                approvedClaims = approvedClaims.Where(a => a.InitiatorStatus == "Approved").ToList();
                pendingClaims = pendingClaims.Where(a => a.InitiatorStatus == "Pending").ToList();
                
            }

            if (userRoless.Contains("PeerReviewer"))
            {
                approvedClaims = approvedClaims.Where(a => a.ReviewStatus == "Approved").ToList();
                pendingClaims = pendingClaims.Where(a => a.ReviewStatus == "Pending").ToList();
            }


            var weeklyData = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                var dayCount = registers.Count(r => r.Date == date.Date.ToString() && r.IsPresent);
                weeklyData.Add(dayCount);
            }
      
            //ViewBag.WeeklyPresentJson = JsonConvert.SerializeObject(weeklyData);
            var total = monitors.Count + recruitments.Count;
            var serverData = new
            {
               
                extraDays = extraDays,
                totalMonitors =total,
                rejectedMonitors = rejectedMonitors.Count(),
                acceptedMonitors = acceptedMonitors.Count(),
                pendingMonitors = pendingClaims.Count(),
                deployedMonitors = deployedMonitors.Count(),
                selectedMonitors = selectedMonitors.Count(),
                totalClaims = claims.Count(),
                totalAmount = totalAmount,
                approvedClaims = approvedClaims.Count(),
                initiator = "Initiator A",
                reviewer = "Reviewer A",
                regions = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" },
                clusterData = clusters.Count(),
                centreData = centres.Count(),
                isResident = isResident.Count(),
                clusterManagers = clusterManagers.Count(),
                residentMonitors = residentMonitors.Count(),
                assistantClusterManagers = assistantClusterManagers.Count(),
                presentToday = registers.Count(r => r.Date == DateTime.Today.ToString("yyyy-MM-dd") && r.IsPresent),
                totalRegisters = registers.Count(),
                weeklyPresent = weeklyData,
                totalDays = totalDays,
                presentDays = presentDays,
                absentDays = absentDays,
                centreAttached = centreAttached,
                roleStatus = roleStatus,
            };

            ViewBag.ServerDataJson = JsonConvert.SerializeObject(serverData);

            return View();
        }

        public async Task<IActionResult> ERS()
        {
            return Redirect("http://ers.zimsec.co.zw/Identity/Account/Login");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private double CheckDataValidation(Examiner examiner)
        {
            int totalFields = 17; // Assuming there are 12 total fields in the examiner object
            int filledFields = 0;

            if (examiner != null)
            {
                if (!string.IsNullOrEmpty(examiner.EMS_EXAMINER_CODE) && examiner.EMS_EXAMINER_CODE != "NULL" && examiner.EMS_EXAMINER_CODE != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_EXAMINER_NAME) && examiner.EMS_EXAMINER_NAME != "NULL" && examiner.EMS_EXAMINER_NAME != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_EXAMINER_NUMBER) && examiner.EMS_EXAMINER_NUMBER != "NULL" && examiner.EMS_EXAMINER_NUMBER != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_LAST_NAME) && examiner.EMS_LAST_NAME != "NULL" && examiner.EMS_LAST_NAME != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_NATIONAL_ID) && examiner.EMS_NATIONAL_ID != "NULL" && examiner.EMS_NATIONAL_ID != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_SEX) && examiner.EMS_SEX != "NULL" && examiner.EMS_SEX != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_ECT_EXAMINER_CAT_CODE) && examiner.EMS_ECT_EXAMINER_CAT_CODE != "NULL" && examiner.EMS_ECT_EXAMINER_CAT_CODE != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_QUALIFICATION) && examiner.EMS_QUALIFICATION != "NULL" && examiner.EMS_QUALIFICATION != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_PAPER_CODE) && examiner.EMS_PAPER_CODE != "NULL" && examiner.EMS_PAPER_CODE != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_PHONE_HOME) && examiner.EMS_PHONE_HOME != "NULL" && examiner.EMS_PHONE_HOME != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_ACCOUNT_NO_FCA) && examiner.EMS_ACCOUNT_NO_FCA != "NULL" && examiner.EMS_ACCOUNT_NO_FCA != "default_value")
                    filledFields++;
                if (!string.IsNullOrEmpty(examiner.EMS_ACCOUNT_NO_ZWL) && examiner.EMS_ACCOUNT_NO_ZWL != "NULL" && examiner.EMS_ACCOUNT_NO_ZWL != "default_value")
                    filledFields++;

                if (!string.IsNullOrEmpty(examiner.EMS_BRANCH_NAME_FCA) && examiner.EMS_BRANCH_NAME_FCA != "NULL" && examiner.EMS_BRANCH_NAME_FCA != "default_value")
                    filledFields++;

                if (!string.IsNullOrEmpty(examiner.EMS_BRANCH_NAME_ZWL) && examiner.EMS_BRANCH_NAME_ZWL != "NULL" && examiner.EMS_BRANCH_NAME_ZWL != "default_value")
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

    }


           public class FilterModel
{
        public string Region { get; set; } 
        public string DateFrom { get; set; }
        public string DateTo { get; set; }

        // If you need Session and Phase, add them here
        // public string Session { get; set; }
        // public string Phase { get; set; }
    }
}
