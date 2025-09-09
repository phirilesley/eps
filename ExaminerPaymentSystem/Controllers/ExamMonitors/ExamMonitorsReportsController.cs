using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.ViewModels.ExamMonitors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class ExamMonitorsReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
      

        public ExamMonitorsReportsController(ApplicationDbContext context,SignInManager<ApplicationUser> signInManager,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult>    AssignmentReport(string searchTerm, string regionFilter,
            string districtFilter, string phaseFilter, string sessionFilter)
        {
         
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            if (userRoles.Contains("RegionalManager"))
            {


                ViewBag.Districts = _context.Districts
                    .Where(m => m.DistrictCode != null && m.RegionCode == currentUser.Region)
                    .Select(m => new {
                        Value = m.DistrictCode,
                        Text = m.DistrictName
                    })
                    .Distinct()
                    .OrderBy(d => d.Text)
                    .ToList();



            }
            else if (userRoles.Contains("Admin"))
            {


                ViewBag.Districts = _context.Districts
                    .Where(m => m.DistrictCode != null)
                    .Select(m => new {
                                    Value = m.DistrictCode,
                                    Text = m.DistrictName
                    })
                    .Distinct()
                    .OrderBy(d => d.Text)
                    .ToList();




            }


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
            ViewBag.RegionCode = currentUser.Region;


            var viewModel = new ClusterReportViewModel
            {
                SearchTerm = searchTerm,
                RegionFilter = regionFilter,
                DistrictFilter = districtFilter,
                PhaseFilter = phaseFilter,
                SessionFilter = sessionFilter,
                Assignments = new List<ClusterAssignment>(),
            };

            if(string.IsNullOrEmpty(viewModel.SearchTerm) && string.IsNullOrEmpty(viewModel.RegionFilter) && string.IsNullOrEmpty(viewModel.DistrictFilter) && string.IsNullOrEmpty(viewModel.PhaseFilter) && string.IsNullOrEmpty(viewModel.SessionFilter))
            {
               return View(viewModel);  
            }

            var allCentres = await _context.Centres.ToListAsync();

            var clusters = await _context.Centres.Where(a => a.IsCluster == "IsCluster").ToListAsync();
            if (userRoles.Contains("RegionalManager"))
            {
                clusters = clusters.Where(c => c.RegionCode == currentUser.Region).ToList();
                allCentres = allCentres.Where(c => c.RegionCode == currentUser.Region).ToList();
            }
            else
            {
                if (!string.IsNullOrEmpty(regionFilter) && regionFilter != "All")
                {
                    clusters = clusters.Where(c => c.RegionCode == regionFilter).ToList();
                    allCentres = allCentres.Where(c => c.RegionCode == regionFilter).ToList();
                }
            
            }

            if (!string.IsNullOrEmpty(districtFilter) && districtFilter != "All")
            {
                clusters = clusters.Where(c => c.DistrictCode == districtFilter).ToList();
            }

            var clusterAssignements = new List<ClusterAssignment>();

            foreach (var item in clusters)
            {
                var centreAssignments = await _context.ExamMonitorTransactions
                    .Include(a => a.ExamMonitor)
                    .Where(a => a.Phase == phaseFilter && a.Session == sessionFilter && a.CentreAttached == item.CentreNumber)
                    .ToListAsync();

                string clusterManagerName = "Not Assigned";
                string assistantClusterManagerName = "Not Assigned";
                string residentMonitorName = "Not Assigned";
                string status = "Not Assigned";

                if (centreAssignments.Any())
                {
                    var clusterManager = centreAssignments.FirstOrDefault(a => a.Status == "Cluster Manager");
                    var assistantClusterManager = centreAssignments.FirstOrDefault(a => a.Status == "Assistant Cluster Manager");
                    var residentMonitor = centreAssignments.FirstOrDefault(a => a.Status == "Resident Monitor");

                    // Get names if assigned
                    if (clusterManager != null && clusterManager.ExamMonitor != null)
                    {
                        clusterManagerName = $"{clusterManager.ExamMonitor.FirstName} {clusterManager.ExamMonitor.LastName}";
                    }

                    if (assistantClusterManager != null && assistantClusterManager.ExamMonitor != null)
                    {
                        assistantClusterManagerName = $"{assistantClusterManager.ExamMonitor.FirstName} {assistantClusterManager.ExamMonitor.LastName}";
                    }

                    if (residentMonitor != null && residentMonitor.ExamMonitor != null)
                    {
                        residentMonitorName = $"{residentMonitor.ExamMonitor.FirstName} {residentMonitor.ExamMonitor.LastName}";
                    }

                    // Determine status based on assignments
                    int assignedCount = 0;
                    if (clusterManagerName != "Not Assigned") assignedCount++;
                    if (assistantClusterManagerName != "Not Assigned") assignedCount++;
                    if (residentMonitorName != "Not Assigned") assignedCount++;

                    if (assignedCount == 3)
                    {
                        status = "Assigned";
                    }
                    else if (assignedCount > 0)
                    {
                        status = "Partially Assigned";
                    }
                    else
                    {
                        status = "Not Assigned";
                    }
                }

                var clusterData = new ClusterAssignment()
                {
                    ClusterCode = item.ClusterCode,
                    ClusterName = item.ClusterName,
                    CentreNumber = item.CentreNumber,
                    CentreName = item.CentreName,
                    ClusterManager = clusterManagerName,
                    ResidentMonitor = residentMonitorName,
                    AssistantClusterManager = assistantClusterManagerName,
                    Region = item.RegionCode,
                    District = item.DistrictName,
                    Phase = phaseFilter,
                    Session = sessionFilter,
                    Status = status,
                    CentreType = "Cluster" // Add this to distinguish between cluster and resident centres      
                };

                clusterAssignements.Add(clusterData);
            }

            // Now process the resident centres
            var isResidents = await _context.Centres.Where(a => a.IsResident == "IsResident").ToListAsync();
            if (userRoles.Contains("RegionalManager"))
            {
                isResidents = isResidents.Where(c => c.RegionCode == currentUser.Region).ToList();
            }
            else
            {
                if (!string.IsNullOrEmpty(regionFilter) && regionFilter != "All")
                {
                    isResidents = isResidents.Where(c => c.RegionCode == regionFilter).ToList();
                }
                
            }

            if (!string.IsNullOrEmpty(districtFilter) && districtFilter != "All")
            {
                isResidents = isResidents.Where(c => c.DistrictCode == districtFilter).ToList();
            }
            foreach (var item in isResidents)
            {
                var centreAssignments = await _context.ExamMonitorTransactions
                    .Include(a => a.ExamMonitor)
                    .Where(a => a.Phase == phaseFilter && a.Session == sessionFilter && a.CentreAttached == item.CentreNumber)
                    .ToListAsync();

                string residentMonitorName = "Not Assigned";
                string status = "Not Assigned";

                if (centreAssignments.Any())
                {
                    var residentMonitor = centreAssignments.FirstOrDefault(a => a.Status == "Resident Monitor");

                    // Get name if assigned
                    if (residentMonitor != null && residentMonitor.ExamMonitor != null)
                    {
                        residentMonitorName = $"{residentMonitor.ExamMonitor.FirstName} {residentMonitor.ExamMonitor.LastName}";
                        status = "Assigned";
                    }
                }

                var residentData = new ClusterAssignment()
                {
                    ClusterCode = item.ClusterCode,
                    ClusterName = item.ClusterName ?? "N/A", // Resident centres don't have cluster names
                    CentreNumber = item.CentreNumber,
                    CentreName = item.CentreName,
                    ClusterManager = "N/A", // Not applicable for resident centres
                    ResidentMonitor = residentMonitorName,
                    AssistantClusterManager = "N/A", // Not applicable for resident centres
                    Region = item.RegionCode,
                    District = item.DistrictName,
                    Phase = phaseFilter,
                    Session = sessionFilter,
                    Status = status,
                    CentreType = "Resident" // Add this to distinguish between cluster and resident centres
                };

                clusterAssignements.Add(residentData);
            }

       
            viewModel.Assignments = clusterAssignements;

            // Calculate statistics
            viewModel.TotalClusters = clusters.Count();
            viewModel.TotalIsResidents = isResidents.Count();
           viewModel.TotalCentres = allCentres.Count;
            viewModel.AssignedCentres = clusterAssignements.Count(a => a.Status == "Assigned");
            viewModel.UnassignedCentres = clusterAssignements.Count(a => a.Status == "Not Assigned" || a.Status == "Partially Assigned");


    
          

            return View(viewModel);
        }


        private async Task<List<FilterOption>> GetRegions()
        {
            // Get regions from your Regions table or use the hardcoded list if you don't have a table
            return new List<FilterOption>
    {
        new FilterOption { Value = "01", Text = "01 - Harare" },
        new FilterOption { Value = "02", Text = "02 - Manicaland" },
        new FilterOption { Value = "03", Text = "03 - Mashonaland East" },
        new FilterOption { Value = "04", Text = "04 - Matabeleland North" },
        new FilterOption { Value = "05", Text = "05 - Midlands" },
        new FilterOption { Value = "06", Text = "06 - Masvingo" },
        new FilterOption { Value = "07", Text = "07 - Mashonaland Central" },
        new FilterOption { Value = "08", Text = "08 - Mashonaland West" },
        new FilterOption { Value = "09", Text = "09 - Matabeleland South" },
        new FilterOption { Value = "10", Text = "10 - Bulawayo" }
    };

            // If you have a Regions table, use this instead:
            // return await _context.Regions
            //     .Where(r => !string.IsNullOrEmpty(r.RegionCode))
            //     .Select(r => new FilterOption { Value = r.RegionCode, Text = $"{r.RegionCode} - {r.RegionName}" })
            //     .OrderBy(r => r.Value)
            //     .ToListAsync();
        }

        private async Task<List<FilterOption>> GetDistricts(string region = null)
        {
            IQueryable<District> query = _context.Districts;

            if (!string.IsNullOrEmpty(region))
                query = query.Where(d => d.RegionCode == region);

            return await query
                .Where(d => !string.IsNullOrEmpty(d.DistrictCode))
                .Select(d => new FilterOption
                {
                    Value = d.DistrictName,
                    Text = d.DistrictName
                })
                .OrderBy(d => d.Text)
                .ToListAsync();
        }

        private async Task<List<FilterOption>> GetPhases()
        {
            return await _context.Phases
                .Where(p => !string.IsNullOrEmpty(p.PhaseCode))
                .Select(p => new FilterOption
                {
                    Value = p.PhaseCode,
                    Text = p.PhaseName
                })
                .OrderBy(p => p.Text)
                .ToListAsync();
        }

        private async Task<List<FilterOption>> GetSessions()
        {
            return await _context.ExamSessions
                .Where(s => !string.IsNullOrEmpty(s.SessionCode))
                .Select(s => new FilterOption
                {
                    Value = s.SessionCode,
                    Text = s.SessionName
                })
                .OrderBy(s => s.Text)
                .ToListAsync();
        }


        [HttpGet]
        public async Task<IActionResult> GetDistrictsByRegion(string regionCode)
        {
            if (string.IsNullOrEmpty(regionCode))
            {
                return Json(new List<object>());
            }

            var districts = await _context.Districts
                .Where(d => d.RegionCode == regionCode)
                .Select(d => new
                {
                    Value = d.DistrictName,
                    Text = d.DistrictName
                })
                .OrderBy(d => d.Text)
                .ToListAsync();

            return Json(districts);
        }

        [HttpGet]
        public async Task<IActionResult> GetDistrictsByRegion2(string regionCode)
        {
            if (string.IsNullOrEmpty(regionCode))
            {
                return Json(new List<object>());
            }

            var districts = await _context.Districts
                .Where(d => d.RegionCode == regionCode)
                .Select(d => new
                {
                    Value = d.DistrictCode,
                    Text = d.DistrictName
                })
                .OrderBy(d => d.Text)
                .ToListAsync();

            return Json(districts);
        }

        //[HttpPost]
        //public async Task<JsonResult> GetDistrictsByRegion(string region)
        //{
        //    var districts = await GetDistricts(region);
        //    return Json(districts);
        //}

        [HttpGet]
        public async Task<IActionResult> ExamMonitorsReport(string regionFilter = "", string districtFilter = "", string statusFilter = "")
        {
            var model = new MonitorsReportViewModel
            {
                RegionFilter = regionFilter,
                DistrictFilter = districtFilter,
                StatusFilter = statusFilter
            };

            try
            {
                var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);

                // Accepted monitors (main table)
                IQueryable<ExamMonitor> query = _context.ExamMonitors
                    .Include(m => m.ExamMonitorTransactions)
                    .Include(m => m.ProfessionalQualifications)
                    .AsQueryable();

                // Recruitment monitors (pending/rejected)
                IQueryable<ExamMonitorsRecruitment> query2 = _context.ExamMonitorsRecruitments.AsQueryable();

                // Role-based region restriction
                if (userRoles.Contains("RegionalManager") && !string.IsNullOrEmpty(currentUser.Region))
                {
                    query = query.Where(x => x.Region == currentUser.Region);
                    query2 = query2.Where(x => x.Region == currentUser.Region);
                }
                else if (!string.IsNullOrEmpty(regionFilter))
                {
                    query = query.Where(x => x.Region == regionFilter);
                    query2 = query2.Where(x => x.Region == regionFilter);
                }

                // District filter
                if (!string.IsNullOrEmpty(districtFilter))
                {
                    query = query.Where(x => x.District == districtFilter);
                    query2 = query2.Where(x => x.District == districtFilter);
                }

                // Status filter
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    query = query.Where(x => x.Status == statusFilter);
                    query2 = query2.Where(x => x.Status == statusFilter);
                }

                // Fetch data
                var acceptedMonitors = await query.OrderByDescending(m => m.LastName).ToListAsync();
                var recruitmentMonitors = await query2.OrderByDescending(m => m.LastName).ToListAsync();

                // Combine into a common DTO
                var allMonitors = new List<MonitorDto>();

                // Map accepted monitors
                allMonitors.AddRange(acceptedMonitors.Select(m => new MonitorDto
                {
                    NationalId = m.NationalId,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Sex = m.Sex,
                    Status = m.Status,
                    Region = m.Region,
                    District = m.District,
                    Centre = m.Centre,
                    Phone = m.Phone,
                    AcceptStatus = "accepted", // main monitors are already accepted
                    IsSelected = m.ExamMonitorTransactions?.Any() == true,
                    IsDeployed = m.ExamMonitorTransactions?.Any(t => t.CentreAttached != "XXXXXX") == true
                }));

                // Map recruitment monitors (pending/rejected)
                allMonitors.AddRange(recruitmentMonitors.Select(r => new MonitorDto
                {
                    NationalId = r.NationalId,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Sex = r.Sex,
                    Status = r.Status,
                    Region = r.Region,
                    District = r.District,
                    Centre = r.Centre,
                    Phone = r.Phone,
                    AcceptStatus = r.AcceptStatus?.ToLower(), // could be "rejected" or "pending"
                    IsSelected = false,
                    IsDeployed = false
                }));

                // Categorize
                model.NewApplications = allMonitors;
                model.AcceptedMonitors = allMonitors.Where(m => m.AcceptStatus == "accepted").ToList();
                model.RejectedMonitors = allMonitors.Where(m => m.AcceptStatus == "rejected").ToList();
                model.SelectedMonitors = allMonitors.Where(m => m.IsSelected).ToList();
                model.DeployedMonitors = allMonitors.Where(m => m.IsDeployed).ToList();

                // Totals
                model.TotalNewApplications = model.NewApplications.Count;
                model.TotalAccepted = model.AcceptedMonitors.Count;
                model.TotalRejected = model.RejectedMonitors.Count;
                model.TotalSelected = model.SelectedMonitors.Count;
                model.TotalDeployed = model.DeployedMonitors.Count;
                model.TotalMonitors = allMonitors.Count;

                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while loading the report.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ClusterReport(string regionFilter = "", string districtFilter = "", string centreTypeFilter = "")
        {
            var model = new CentresReportViewModel
            {
                RegionFilter = regionFilter,
                DistrictFilter = districtFilter,
                CentreTypeFilter = centreTypeFilter
            };

            try
            {
                var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);

                // Base query
                IQueryable<Centre> query = _context.Centres
                    .Include(c => c.District)
                    .AsQueryable();

                // Apply region filter based on user role
                if (userRoles.Contains("RegionalManager") && !string.IsNullOrEmpty(currentUser.Region))
                {
                    query = query.Where(x => x.RegionCode == currentUser.Region);
                }
                else if (!string.IsNullOrEmpty(regionFilter) && regionFilter != "All")
                {
                    query = query.Where(x => x.RegionCode == regionFilter);
                }

                // Apply district filter
                if (!string.IsNullOrEmpty(districtFilter) && districtFilter != "All")
                {
                    query = query.Where(x => x.DistrictCode == districtFilter);
                }

                // Apply centre type filter
                if (!string.IsNullOrEmpty(centreTypeFilter) && centreTypeFilter != "All")
                {
                    switch (centreTypeFilter)
                    {
                        case "Clusters":
                            query = query.Where(x => x.IsCluster == "IsCluster");
                            break;
                        case "Residents":
                            query = query.Where(x => x.IsResident == "IsResident");
                            break;
                  
                        case "DistrictCentres":
                            query = query.Where(x => x.IsCluster != "IsCluster" && x.IsResident != "IsResident");
                            break;
                    }
                }

                // Get all centres
                var allCentres = await query
                    .OrderBy(c => c.RegionCode)
                    .ThenBy(c => c.DistrictCode)
                    .ThenBy(c => c.CentreNumber)
                    .ToListAsync();

                // Categorize centres
                model.AllCentres = allCentres;
                model.Clusters = allCentres.Where(c => c.IsCluster == "IsCluster").ToList();
                model.Residents = allCentres.Where(c => c.IsResident == "IsResident").ToList();
     
                model.DistrictCentres = allCentres.Where(c => c.IsCluster != "IsCluster" && c.IsResident != "IsResident").ToList();

                // Calculate statistics
                model.TotalCentres = model.AllCentres.Count;
                model.TotalClusters = model.Clusters.Count;
                model.TotalResidents = model.Residents.Count;
                model.TotalClusterCentres = model.ClusterCentres.Count;
                model.TotalDistrictCentres = model.DistrictCentres.Count;

                return View(model);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error loading centres report");
                ModelState.AddModelError("", "An error occurred while loading the report.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string tabName, string regionFilter = "", string districtFilter = "", string statusFilter = "")
        {
            // Reuse your filtering logic
            var report = await ExamMonitorsReport(regionFilter, districtFilter, statusFilter) as ViewResult;
            if (report?.Model is not MonitorsReportViewModel model)
            {
                return BadRequest("Could not load data.");
            }

            // Pick correct list
            List<MonitorDto> data = tabName.ToLower() switch
            {
                "new" => model.NewApplications,
                "accepted" => model.AcceptedMonitors,
                "rejected" => model.RejectedMonitors,
                "selected" => model.SelectedMonitors,
                "deployed" => model.DeployedMonitors,
                _ => model.NewApplications
            };

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Monitors");

            // Headers
            ws.Cell(1, 1).Value = "National ID";
            ws.Cell(1, 2).Value = "First Name";
            ws.Cell(1, 3).Value = "Last Name";
            ws.Cell(1, 4).Value = "Sex";
            ws.Cell(1, 5).Value = "Status";
            ws.Cell(1, 6).Value = "Region";
            ws.Cell(1, 7).Value = "District";
            ws.Cell(1, 8).Value = "Centre";
            ws.Cell(1, 9).Value = "Phone";
            ws.Cell(1, 10).Value = "Accept Status";
            ws.Cell(1, 11).Value = "Is Selected";
            ws.Cell(1, 12).Value = "Is Deployed";

            // Rows
            int row = 2;
            foreach (var m in data)
            {
                ws.Cell(row, 1).Value = m.NationalId;
                ws.Cell(row, 2).Value = m.FirstName;
                ws.Cell(row, 3).Value = m.LastName;
                ws.Cell(row, 4).Value = m.Sex;
                ws.Cell(row, 5).Value = m.Status;
                ws.Cell(row, 6).Value = m.Region;
                ws.Cell(row, 7).Value = m.District;
                ws.Cell(row, 8).Value = m.Centre;
                ws.Cell(row, 9).Value = m.Phone;
                ws.Cell(row, 10).Value = m.AcceptStatus;
                ws.Cell(row, 11).Value = m.IsSelected;
                ws.Cell(row, 12).Value = m.IsDeployed;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"{tabName}_Monitors_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }





    }

    public class MonitorDto
    {
        public string NationalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string Status { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Centre { get; set; }
        public string Phone { get; set; }
        public string AcceptStatus { get; set; } // "accepted" | "rejected" | "pending"
        public bool IsSelected { get; set; }     // true if transactions exist
        public bool IsDeployed { get; set; }     // true if deployed to a centre
    }

  


}
