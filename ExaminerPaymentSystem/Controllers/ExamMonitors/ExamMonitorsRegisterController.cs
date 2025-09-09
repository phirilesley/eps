using DinkToPdf;
using DocumentFormat.OpenXml.Spreadsheet;
using ExaminerPaymentSystem.Controllers.Examiners;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models;
using ExaminerPaymentSystem.Models.ExamMonitors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class ExamMonitorsRegisterController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ExamMonitorsRegisterController> _logger; 

    public ExamMonitorsRegisterController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ExamMonitorsRegisterController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    //public async Task<IActionResult> Create()
    //{
    //    if (!User.Identity.IsAuthenticated)
    //    {
    //        return Challenge();
    //    }

    //    var user = await _userManager.GetUserAsync(User);
    //    if (user == null)
    //    {
    //        return NotFound("User not found.");
    //    }

    //    var userRoles = await _userManager.GetRolesAsync(user);
    //    var transaction = await _context.ExamMonitorTransactions
    //        .FirstOrDefaultAsync(t => t.SubKey == user.EMS_SUBKEY);

    //    if (transaction == null)
    //    {
    //        return NotFound("No exam monitor transaction found for current user.");
    //    }

    //    var phase = await _context.Phases
    //        .FirstOrDefaultAsync(p => p.PhaseCode == transaction.Phase);

    //    if (phase == null)
    //    {
    //        return NotFound($"No phase information found for phase code: {transaction.Phase}");
    //    }

    //    IQueryable<DateTime> examDatesQuery;

    //    if (userRoles.Contains("ResidentMonitor"))
    //    {
    //        examDatesQuery = _context.Exm_Timetable
    //            .Where(t => t.CentreCode == transaction.CentreAttached
    //                       && t.Exam_date >= phase.StartTime.Date
    //                       && t.Exam_date <= phase.EndTime.Date)
    //            .Select(t => t.Exam_date);
    //    }
    //    else if (userRoles.Contains("ClusterManager") || userRoles.Contains("AssistantClusterManager"))
    //    {
    //        var clusterCode = await _context.Centres
    //            .Where(c => c.CentreNumber == transaction.CentreAttached)
    //            .Select(c => c.ClusterCode)
    //            .FirstOrDefaultAsync();

    //        if (clusterCode == null)
    //        {
    //            return NotFound($"Cluster code not found for centre: {transaction.CentreAttached}");
    //        }

    //        var centresInCluster = await _context.Centres
    //            .Where(c => c.ClusterCode == clusterCode)
    //            .Select(c => c.CentreNumber)
    //            .ToListAsync();

    //        examDatesQuery = _context.Exm_Timetable
    //            .Where(t => centresInCluster.Contains(t.CentreCode)
    //                       && t.Exam_date >= phase.StartTime.Date
    //                       && t.Exam_date <= phase.EndTime.Date)
    //            .Select(t => t.Exam_date);
    //    }
    //    else
    //    {
    //        examDatesQuery = _context.Exm_Timetable
    //            .Where(t => t.CentreCode == transaction.CentreAttached
    //                       && t.Exam_date >= phase.StartTime.Date
    //                       && t.Exam_date <= phase.EndTime.Date)
    //            .Select(t => t.Exam_date);
    //    }

    //    var examDates = await examDatesQuery
    //        .Distinct()
    //        .OrderBy(d => d)
    //        .ToListAsync();

    //    var existingDates = await _context.ExamMonitorRegisterDates
    //        .Where(rd => rd.SubKey == user.EMS_SUBKEY)
    //        .ToListAsync();

    //    var viewModel = new ExamMonitorRegisterViewModel
    //    {
    //        SubKey = transaction.SubKey,
    //        NationalId = transaction.NationalId,
    //        ClusterName = transaction.CentreAttached,
    //        ClusterManagersName = user.UserName,
    //        District = transaction.District,
    //        ExamDates = examDates.Select(d => new ExamDateEntry
    //        {
    //            Date = d,
    //            Comment = existingDates.FirstOrDefault(ed => ed.Date == d)?.Comment ?? ""
    //        }).ToList(),
    //        PhaseStartDate = phase.StartTime.Date,
    //        PhaseEndDate = phase.EndTime.Date
    //    };

    //    return View(viewModel);
    //}

    public async Task<IActionResult> Create()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Challenge();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var transaction = await _context.ExamMonitorTransactions
            .Include(a => a.ExamMonitor)
            .FirstOrDefaultAsync(t => t.SubKey == user.EMS_SUBKEY);

        if (transaction == null)
        {
            return NotFound("No exam monitor transaction found for current user.");
        }

        var phase = await _context.Phases
            .FirstOrDefaultAsync(p => p.PhaseCode == transaction.Phase);

        if (phase == null)
        {
            return NotFound($"No phase information found for phase code: {transaction.Phase}");
        }

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

            if (clusterCode == null)
            {
                return NotFound($"Cluster code not found for centre: {transaction.CentreAttached}");
            }

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
        }

        var existingDates = await _context.ExamMonitorRegisterDates
            .Where(rd => rd.SubKey == user.EMS_SUBKEY)
            .ToListAsync();

        // Merge allDates with existingDates
        var mergedDates = allDates
            .Concat(existingDates.Select(ed => ed.Date))  // add DB dates
            .Distinct()                                   // remove duplicates
            .OrderBy(d => d)                              // keep them sorted
            .ToList();

        var districtName = transaction.District;
        var district = await _context.Districts.FirstOrDefaultAsync(a => a.DistrictCode == transaction.District);
        if (district != null)
        {
            districtName = district.DistrictName;
        }

        var centreName = transaction.CentreAttached;
        var clusterName = transaction.CentreAttached;
        var centre = await _context.Centres.FirstOrDefaultAsync(a => a.CentreNumber == transaction.CentreAttached); 
        if(centre != null)
        {
            centreName = centre.CentreName;
            clusterName = centre.ClusterName;
        }


        var viewModel = new ExamMonitorRegisterViewModel
        {
            SubKey = transaction.SubKey,
            PhaseName = phase.PhaseName,
            NationalId = transaction.NationalId,
            CentreAttached = transaction.CentreAttached,
            FullName = transaction.ExamMonitor.FirstName + " " + transaction.ExamMonitor.LastName,
            ClusterName = clusterName,
            CentreName = centreName,
            Role = transaction.Status,
            BankNameZwg = transaction.ExamMonitor.BankNameZwg,
            BankNameUsd = transaction.ExamMonitor.BankNameUsd,
            BranchUsd = transaction.ExamMonitor.BranchUsd,
            BranchZwg = transaction.ExamMonitor.BranchZwg,
            AccountNumberUsd = transaction.ExamMonitor.AccountNumberUsd,
            AccountNumberZwg = transaction.ExamMonitor.AccountNumberZwg,
            ClusterManagersName = user.UserName,
            Phone = transaction.ExamMonitor.Phone,
            Region = transaction.Region,
            District = districtName,
            ExamDates = mergedDates.Select(d => new ExamDateEntry
            {
                Date = d,
                Comment = existingDates.FirstOrDefault(ed => ed.Date == d)?.Comment ?? "",
                IsTravelDay = !actualExamDates.Contains(d) // Mark travel days
            }).ToList(),
            PhaseStartDate = phase.StartTime.Date,
            PhaseEndDate = phase.EndTime.Date
        };

        return View(viewModel);
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExamMonitorRegisterViewModel model)
    {
        var today = DateTime.Today;

        if (!User.Identity.IsAuthenticated)
        {
            return Challenge();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var transaction = await _context.ExamMonitorTransactions
            .FirstOrDefaultAsync(t => t.SubKey == user.EMS_SUBKEY);

        var phase = await _context.Phases
           .FirstOrDefaultAsync(p => p.PhaseCode == transaction.Phase);

        if (phase == null)
        {
            return NotFound($"No phase information found for phase code: {transaction.Phase}");
        }

        // Remove validation for future dates
        for (int i = 0; i < model.ExamDates.Count; i++)
        {
            if (model.ExamDates[i].Date > today)
            {
                ModelState.Remove($"ExamDates[{i}].Comment");
            }
        }

        // Check if there are any validation errors for non-future dates
        var hasErrors = ModelState
            .Where(ms => ms.Key.Contains("ExamDates") &&
                  model.ExamDates[int.Parse(ms.Key.Split('[', ']')[1])].Date <= today)
            .Any(ms => ms.Value.Errors.Count > 0);

        if (hasErrors)
        {
            TempData["ErrorMessage"] = "Please provide comments for all required dates.";
            return View(model);
        }

        try
        {
            var currentTime = DateTime.Now;

            // Determine user role
            var isClusterManager = userRoles.Contains("ClusterManager");
            var isResidentMonitor = userRoles.Contains("ResidentMonitor");
            var isAssistantClusterManager = userRoles.Contains("AssistantClusterManager");

            IQueryable<DateTime> examDatesQuery = Enumerable.Empty<DateTime>().AsQueryable();

            if (userRoles.Contains("ClusterManager") || userRoles.Contains("AssistantClusterManager"))
            {
                var clusterCode = await _context.Centres
                    .Where(c => c.CentreNumber == transaction.CentreAttached)
                    .Select(c => c.ClusterCode)
                    .FirstOrDefaultAsync();

                if (clusterCode == null)
                {
                    return NotFound($"Cluster code not found for centre: {transaction.CentreAttached}");
                }

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

            // Separate dates
            var pastOrTodayDates = model.ExamDates.Where(d => d.Date <= today).ToList();

            // Safely get the last date - check if there are any results first
            DateTime lastDate = DateTime.MinValue;
            bool hasExamDates = false;

            if (examDatesQuery.Any())
            {
                lastDate = await examDatesQuery.OrderBy(d => d).LastAsync();
                hasExamDates = true;
            }

            // NEW LOGIC: Check if lastDate exists in ExamDates to determine if we have future dates
            bool hasFutureDates = true; // Assume we have future dates by default

            if (hasExamDates)
            {
                // Check if the lastDate exists in the submitted ExamDates
                bool lastDateExistsInSubmittedDates = model.ExamDates.Any(d => d.Date == lastDate.Date);

                // If the last exam date exists in the submitted dates, then we don't have future dates
                hasFutureDates = !lastDateExistsInSubmittedDates;
            }
            else
            {
                // If no exam dates found in query, check if we have any dates beyond today
                hasFutureDates = model.ExamDates.Any(d => d.Date > today);
            }

            // All past/today dates must have comments
            bool pastDatesHaveComments = pastOrTodayDates.All(d => !string.IsNullOrWhiteSpace(d.Comment));

            // Only approve overall register if all past/today dates have comments AND no future dates exist
            bool canApproveRegister = pastDatesHaveComments && !hasFutureDates;

            // Check if main register exists
            var register = await _context.ExamMonitorsRegisters
                .FirstOrDefaultAsync(r => r.SubKey == model.SubKey);

            if (register == null)
            {
                register = new ExamMonitorRegister
                {
                    SubKey = model.SubKey,
                    NationalId = model.NationalId,
                    Date = currentTime.ToString("yyyy-MM-dd"),
                    Comment = "Reported for duty",
                    CompiledStatus = "Pending",
                    CompiledBy = user.UserName,
                    CompiledDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ClusterManagerStatus = "Pending",
                    ClusterManagerBy = user.UserName,
                    ClusterManagerDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegionalManagerStatus = "Pending",
                    RegionalManagerBy = user.UserName,
                    RegionalManagerDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsPresent = false,
                    IsPresentBy = user.UserName,
                    IsPresentDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss")
                };

                if (canApproveRegister)
                {
                    if (isClusterManager)
                    {
                        register.CompiledStatus = "Approved";
                        register.CompiledDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                        register.CompiledBy = user.UserName;
                        register.ClusterManagerStatus = "Approved";
                        register.ClusterManagerDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                        register.ClusterManagerBy = user.UserName;
                    }
                    else if (isResidentMonitor || isAssistantClusterManager)
                    {
                        register.CompiledStatus = "Approved";
                        register.CompiledDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                        register.CompiledBy = user.UserName;
                    }
                }

                _context.ExamMonitorsRegisters.Add(register);
            }
            else
            {
                if (canApproveRegister)
                {
                    if (isClusterManager)
                    {
                        register.CompiledStatus = "Approved";
                        register.ClusterManagerStatus = "Approved";
                        register.CompiledBy = user.UserName;
                        register.CompiledDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                        register.ClusterManagerBy = user.UserName;
                        register.ClusterManagerDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else if (isResidentMonitor || isAssistantClusterManager)
                    {
                        register.CompiledStatus = "Approved";
                        register.CompiledBy = user.UserName;
                        register.CompiledDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
                else
                {
                    // If there are future dates → keep Pending
                    if (isClusterManager)
                    {
                        register.CompiledStatus = "Pending";
                        register.ClusterManagerStatus = "Pending";
                    }
                    else if (isResidentMonitor || isAssistantClusterManager)
                    {
                        register.CompiledStatus = "Pending";
                    }
                }

                _context.ExamMonitorsRegisters.Update(register);
            }

            // Save daily entries (only past/today dates with comments)
            foreach (var dateEntry in pastOrTodayDates.Where(d => !string.IsNullOrWhiteSpace(d.Comment)))
            {
                var existingEntry = await _context.ExamMonitorRegisterDates
                    .FirstOrDefaultAsync(d => d.SubKey == model.SubKey && d.Date == dateEntry.Date);

                if (existingEntry != null)
                {
                    if (existingEntry.Comment != dateEntry.Comment)
                    {
                        existingEntry.Comment = dateEntry.Comment;

                        // Daily entries → approve immediately per role
                        if (isClusterManager)
                        {
                            existingEntry.CompiledStatus = "Approved";
                            existingEntry.ClusterManagerStatus = "Approved";
                            existingEntry.CompiledBy = user.UserName;
                            existingEntry.CompiledDate = currentTime;
                            existingEntry.ClusterManagerBy = user.UserName;
                            existingEntry.ClusterManagerDate = currentTime;
                            existingEntry.ClusterManagerComment = dateEntry.Comment;
                        }
                        else if (isResidentMonitor || isAssistantClusterManager)
                        {
                            existingEntry.CompiledStatus = "Approved";
                            existingEntry.CompiledBy = user.UserName;
                            existingEntry.CompiledDate = currentTime;
                            existingEntry.Comment = dateEntry.Comment;
                        }

                        _context.ExamMonitorRegisterDates.Update(existingEntry);
                    }
                }
                else
                {
                    var newDateEntry = new ExamMonitorRegisterDate
                    {
                        SubKey = model.SubKey,
                        Register = register,
                        Date = dateEntry.Date,
                        Comment = dateEntry.Comment,
                        CompiledStatus = "Approved",
                        CompiledBy = user.UserName,
                        CompiledDate = currentTime,
                        ClusterManagerStatus = "Pending",
                        ClusterManagerBy = user.UserName,
                        ClusterManagerDate = currentTime,
                        RegionalManagerStatus = "Pending",
                        RegionalManagerBy = user.UserName,
                        RegionalManagerDate = currentTime,
                        IsFromTimetable = true,
                        IsSupervisorAdded = false,
                        IsPresent = false
                    };

                    // Approve daily entries per role
                    if (isClusterManager)
                    {
                        newDateEntry.CompiledStatus = "Approved";
                        newDateEntry.ClusterManagerStatus = "Approved";
                    }
                    else if (isResidentMonitor || isAssistantClusterManager)
                    {
                        newDateEntry.CompiledStatus = "Approved";
                    }

                    _context.ExamMonitorRegisterDates.Add(newDateEntry);
                }
            }

            await _context.SaveChangesAsync(user.Id);
            TempData["SuccessMessage"] = "Register successfully updated.";
            return RedirectToAction(nameof(Create));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving exam monitor register");
            TempData["ErrorMessage"] = $"Error saving register: {ex.Message}";
            return View(model);
        }
    }



    
}


