using AspNetCoreGeneratedDocument;
using DinkToPdf;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExamMonitors;
using ExaminerPaymentSystem.Models.ExamMonitors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExaminerPaymentSystem.Controllers
{
    [Authorize]
    public class ExamMonitorApprovalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExamMonitorApprovalRepository _approvalRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExamMonitorApprovalController> _logger;

        public ExamMonitorApprovalController(
            ApplicationDbContext context,
            IExamMonitorApprovalRepository approvalRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<ExamMonitorApprovalController> logger)
        {
            _context = context;
            _approvalRepository = approvalRepository;
            _userManager = userManager;
            _logger = logger;
        }

       
        public async Task<IActionResult> RegisterIndex()
        {
            var usersWithRegisters = await _approvalRepository.GetUsersWithRegistersAsync();
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            foreach (var item in usersWithRegisters)
            {
                var cluster = await _context.Centres.FirstOrDefaultAsync(a => a.CentreNumber == item.CentreAttached);
                if(cluster != null)
                {
                    item.ClusterCode = cluster.ClusterCode;
                    item.ClusterName = cluster.ClusterName;
                    item.CentreName = cluster.CentreName;

                }

                var phase = await _context.Phases.FirstOrDefaultAsync(a => a.PhaseCode == item.PhaseCode);
                if(phase != null)
                {
                    item.PhaseName = phase.PhaseName;
                }
                var session = await _context.ExamSessions.FirstOrDefaultAsync(a => a.SessionCode == item.SessionCode);
                if(session != null)
                {
                    item.SessionName = session.SessionName;
                }
                
            }


            if (userRoles.Contains("ClusterManager"))
            {
                // Get current user's cluster
                var currentUserCluster = await _context.ExamMonitorTransactions
                    .FirstOrDefaultAsync(a => a.SubKey == currentUser.EMS_SUBKEY);

                if (currentUserCluster != null)
                {
                    var cluster = await _context.Centres
                        .FirstOrDefaultAsync(a => a.CentreNumber == currentUserCluster.CentreAttached);

                    if (cluster != null)
                    {
                        usersWithRegisters = usersWithRegisters
                            .Where(a => a.ClusterCode == cluster.ClusterCode &&
                                       a.MonitorStatus != "Cluster Manager" && a.CompiledStatus == "Approved" && a.ClusterManagerStatus == "Pending");
                    }
                }
            }
            else if (userRoles.Contains("RegionalManager"))
            {
                
                usersWithRegisters = usersWithRegisters
                    .Where(a => a.ClusterManagerStatus == "Approved" && a.CompiledStatus == "Approved" && a.RegionalManagerStatus == "Pending" ); 

                
                var currentUserRegion = await _context.ExamMonitorTransactions
                    .Where(a => a.SubKey == currentUser.EMS_SUBKEY)
                    .Join(_context.Centres,
                        t => t.CentreAttached,
                        c => c.CentreNumber,
                        (t, c) => c.RegionCode)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(currentUserRegion))
                {
                    usersWithRegisters = usersWithRegisters
                        .Where(a => a.RegionCode == currentUserRegion);
                }

                ViewBag.RegionCode = currentUserRegion;
            }

            

            return View(usersWithRegisters);
        }


        //REGIONALMANAGER

        public async Task<IActionResult> RemRegisterIndex()
        {
            var usersWithRegisters = await _approvalRepository.GetUsersWithRegistersAsync();
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            foreach (var item in usersWithRegisters)
            {
                var cluster = await _context.Centres.FirstOrDefaultAsync(a => a.CentreNumber == item.CentreAttached);
                if (cluster != null)
                {
                    item.ClusterCode = cluster.ClusterCode;
                }

            }


            if (userRoles.Contains("ClusterManager"))
            {
                // Get current user's cluster
                var currentUserCluster = await _context.ExamMonitorTransactions
                    .FirstOrDefaultAsync(a => a.SubKey == currentUser.EMS_SUBKEY);

                if (currentUserCluster != null)
                {
                    var cluster = await _context.Centres
                        .FirstOrDefaultAsync(a => a.CentreNumber == currentUserCluster.CentreAttached);

                    if (cluster != null)
                    {
                        usersWithRegisters = usersWithRegisters
                            .Where(a => a.ClusterCode == cluster.ClusterCode &&
                                       a.MonitorStatus != "Cluster Manager" && a.CompiledStatus == "Approved" && a.ClusterManagerStatus == "Pending");
                    }
                }
            }
            else if (userRoles.Contains("RegionalManager"))
            {

                usersWithRegisters = usersWithRegisters
                    .Where(a => a.ClusterManagerStatus == "Approved" && a.CompiledStatus == "Approved" && a.RegionalManagerStatus == "Pending");


                var currentUserRegion = await _context.ExamMonitorTransactions
                    .Where(a => a.SubKey == currentUser.EMS_SUBKEY)
                    .Join(_context.Centres,
                        t => t.CentreAttached,
                        c => c.CentreNumber,
                        (t, c) => c.RegionCode)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(currentUserRegion))
                {
                    usersWithRegisters = usersWithRegisters
                        .Where(a => a.RegionCode == currentUserRegion);
                }
            }


            return View(usersWithRegisters);
        }




        public async Task<IActionResult> Approve(string subKey)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var registerDates = await _approvalRepository.GetRegisterDatesAsync(subKey);
            var transaction = await _context.ExamMonitorTransactions.Include(a => a.ExamMonitor)
                .FirstOrDefaultAsync(a => a.SubKey == subKey);

            if (transaction == null)
            {

            }
            var districtName = transaction.District;
            var district = await _context.Districts.FirstOrDefaultAsync(a => a.DistrictCode == transaction.District);
            if (district != null)
            {
                districtName = district.DistrictName;
            }

            var centreName = transaction.CentreAttached;
            var clusterName = transaction.CentreAttached;
            var centre = await _context.Centres.FirstOrDefaultAsync(a => a.CentreNumber == transaction.CentreAttached);
            if (centre != null)
            {
                centreName = centre.CentreName;
                clusterName = centre.ClusterName;
            }

            var viewModel = new ApproveRegisterViewModel
            {
                SubKey = subKey,
                UserName = registerDates.FirstOrDefault()?.Register?.ClusterManagerBy,
                RegisterDates = registerDates,
                FullName = transaction.ExamMonitor.FirstName + " "+ transaction.ExamMonitor.LastName,
                NationalId = transaction.ExamMonitor.NationalId,
                BankNameUsd = transaction.ExamMonitor.BankNameUsd,
                BankNameZwg = transaction.ExamMonitor.BankNameZwg,
                BranchUsd = transaction.ExamMonitor.BranchUsd,
                BranchZwg = transaction.ExamMonitor.BranchZwg,
                AccountNumberUsd = transaction.ExamMonitor.AccountNumberUsd,
                AccountNumberZwg = transaction.ExamMonitor.AccountNumberZwg,
                Phone = transaction.ExamMonitor.Phone,
                CentreName = centreName,
                ClusterName =clusterName,
                District = districtName,
                Region = transaction.ExamMonitor.Region,
                Role = transaction.Status,
                CurrentUserRole = userRoles.FirstOrDefault(r =>
                    r == "RegionalManager" || r == "ClusterManager")
            };

            bool isRegionalManager = await _userManager.IsInRoleAsync(currentUser, "RegionalManager");
            bool isClusterManager = await _userManager.IsInRoleAsync(currentUser, "ClusterManager");

            // Pass the role information to the view
            ViewBag.IsRegionalManager = isRegionalManager;
            ViewBag.IsClusterManager = isClusterManager;

            return View(viewModel);
        }

        //REGIONALMANAGER

        //public async Task<IActionResult> RemApprove(string subKey)
        //{
        //    var currentUser = await _userManager.GetUserAsync(User);
        //    var userRoles = await _userManager.GetRolesAsync(currentUser);

        //    var registerDates = await _approvalRepository.GetRegisterDatesAsync(subKey);

        //    var viewModel = new ApproveRegisterViewModel
        //    {
        //        SubKey = subKey,
        //        UserName = registerDates.FirstOrDefault()?.Register?.ClusterManagerBy,
        //        RegisterDates = registerDates,
        //        CurrentUserRole = userRoles.FirstOrDefault(r =>
        //            r == "RegionalManager" || r == "ClusterManager")
        //    };

        //    return View(viewModel);
        //}

        public async Task<IActionResult> RemApprove(string subKey)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var registerDates = await _approvalRepository.GetRegisterDatesAsync(subKey);

            // Set all IsPresent values to true
            foreach (var registerDate in registerDates)
            {
                registerDate.IsPresent = true;
            }

            var viewModel = new ApproveRegisterViewModel
            {
                SubKey = subKey,
                UserName = registerDates.FirstOrDefault()?.Register?.ClusterManagerBy,
                RegisterDates = registerDates,
                CurrentUserRole = userRoles.FirstOrDefault(r =>
                    r == "RegionalManager" || r == "ClusterManager")
            };

            return View(viewModel);
        }



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Approve(ApproveRegisterViewModel model)
        //{
        //    if (string.IsNullOrEmpty(model.SubKey) || model.RegisterDates == null)
        //    {
        //        TempData["ErrorMessage"] = "Invalid request data";
        //        return View(model);
        //    }

        //    var currentUser = await _userManager.GetUserAsync(User);
        //    var currentTime = DateTime.Now;

        //    try
        //    {

        //        var allRecords = await _approvalRepository.GetRegisterDatesAsync(model.SubKey);


        //        foreach (var date in model.RegisterDates)
        //        {
        //            var existingRecord = allRecords.FirstOrDefault(x => x.Id == date.Id);

        //            if (existingRecord != null)
        //            {
        //                if (User.IsInRole("RegionalManager"))
        //                {
        //                    existingRecord.RegionalManagerStatus = "Approved";
        //                    existingRecord.RegionalManagerBy = currentUser.UserName;
        //                    existingRecord.RegionalManagerDate = currentTime;
        //                }
        //                else if (User.IsInRole("ClusterManager"))
        //                {
        //                    existingRecord.ClusterManagerStatus = "Approved";
        //                    existingRecord.ClusterManagerBy = currentUser.UserName;
        //                    existingRecord.ClusterManagerDate = currentTime;
        //                }


        //                existingRecord.IsPresent = date.IsPresent;
        //                await _approvalRepository.UpdateApprovalStatusAsync(existingRecord);

        //            }
        //        }

        //        TempData["SuccessMessage"] = "Approvals saved successfully";
        //        return RedirectToAction("RegisterIndex", "ExamMonitorApproval");

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error saving approvals");
        //        TempData["ErrorMessage"] = $"Error saving approvals: {ex.Message}";
        //        return View(model);
        //    }
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ApproveRegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.SubKey) || model.RegisterDates == null)
            {
                TempData["ErrorMessage"] = "Invalid request data";
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentTime = DateTime.Now;

            try
            {
                var allRecords = await _approvalRepository.GetRegisterDatesAsync(model.SubKey);
                var mainRegister = await _context.ExamMonitorsRegisters
                    .FirstOrDefaultAsync(r => r.SubKey == model.SubKey);

                if (mainRegister == null)
                {
                    TempData["ErrorMessage"] = "Main register not found";
                    return View(model);
                }

                // Check if all checkboxes are ticked (all dates are marked as present)
                bool allCheckboxesTicked = model.RegisterDates.All(x => x.IsPresent);

                foreach (var date in model.RegisterDates)
                {
                    var existingRecord = allRecords.FirstOrDefault(x => x.Id == date.Id);

                    if (existingRecord != null)
                    {
                        if (User.IsInRole("RegionalManager"))
                        {
                            existingRecord.RegionalManagerStatus = "Approved";
                            existingRecord.RegionalManagerBy = currentUser.UserName;
                            existingRecord.RegionalManagerDate = currentTime;
                            existingRecord.RegionalManagerComment = date?.RegionalManagerComment;
                        }
                        else if (User.IsInRole("ClusterManager"))
                        {
                            existingRecord.ClusterManagerStatus = "Approved";
                            existingRecord.ClusterManagerBy = currentUser.UserName;
                            existingRecord.ClusterManagerDate = currentTime;
                            existingRecord.ClusterManagerComment = date?.ClusterManagerComment; // Get from model
                        }

                        existingRecord.IsPresent = date.IsPresent;
                        await _approvalRepository.UpdateApprovalStatusAsync(existingRecord);
                    }
                }

                //manual force allCheckboxesTicked to be true
                allCheckboxesTicked = true;
                // Update main register only if all checkboxes are ticked
                if (allCheckboxesTicked)
                {
                    if (User.IsInRole("RegionalManager"))
                    {
                        mainRegister.RegionalManagerStatus = "Approved";
                        mainRegister.RegionalManagerBy = currentUser.UserName;
                        mainRegister.RegionalManagerDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else if (User.IsInRole("ClusterManager"))
                    {
                        mainRegister.ClusterManagerStatus = "Approved";
                        mainRegister.ClusterManagerBy = currentUser.UserName;
                        mainRegister.ClusterManagerDate = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    _context.ExamMonitorsRegisters.Update(mainRegister);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Approvals saved successfully";
                return RedirectToAction("RegisterIndex", "ExamMonitorApproval");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving approvals");
                TempData["ErrorMessage"] = $"Error saving approvals: {ex.Message}";
                return View(model);
            }
        }




    }
}