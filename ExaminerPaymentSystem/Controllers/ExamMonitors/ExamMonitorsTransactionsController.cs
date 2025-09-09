using AutoMapper;
using ClosedXML.Excel;
using ExaminerPaymentSystem.Controllers.Examiners;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Services;
using ExaminerPaymentSystem.Services.ExamMonitors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class ExamMonitorsTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExamMonitorService _service;
        private readonly IMapper _mapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
     
        public ExamMonitorsTransactionsController(IExamMonitorService service, IMapper mapper, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,IUserRepository userRepository)
        {
            _service = service;
            _mapper = mapper;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepository;
        }

     

        // In your ExamMonitorsController.cs
        [HttpGet]
        public async Task<JsonResult> GetExamMonitorTransaction(string subKey)
        {
            try
            {
                var monitorTransaction = await _context.ExamMonitorTransactions
                    .Include(a => a.ExamMonitor)
                    .FirstOrDefaultAsync(m => m.SubKey == subKey);

                if (monitorTransaction == null)
                {
                    return Json(new { success = false, message = "Monitor Transaction not found" });
                }

                return Json(new
                {
                    success = true,
                    NationalId = monitorTransaction.NationalId,
                    Status = monitorTransaction.Status,
                    Region = monitorTransaction.Region,
                  SubKey = monitorTransaction.SubKey,
                    Centre = monitorTransaction.CentreAttached,
                    Phase = monitorTransaction.Phase,
                    Session = monitorTransaction.Session,
                   
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ExamMonitors
        public async Task<IActionResult> Index()
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            if (userRoles.Contains("RegionalManager"))
            {
                ViewBag.Centres = _context.Centres.Where(a => a.RegionCode == currentUser.Region)
       .Select(m => new {
           Value = m.CentreNumber,
           Text = m.CentreName
       })
       .Distinct()
       .OrderBy(c => c.Text)
       .ToList();

                ViewBag.Clusters = _context.Centres
                    .Where(a => a.RegionCode == currentUser.Region && a.IsCluster == "IsCluster")
           .Select(m => new {
               Value = m.ClusterCode,
               Text = m.ClusterName
           })
           .Distinct()
           .OrderBy(c => c.Text)
           .ToList();

                ViewBag.Districts = _context.Districts
                    .Where(m => m.DistrictCode != null && m.RegionCode == currentUser.Region)
                    .Select(m => new {
                        Value = m.DistrictCode,
                        Text = m.DistrictName
                    })
                    .Distinct()
                    .OrderBy(d => d.Text)
                    .ToList();

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


            }
            else if (userRoles.Contains("Admin") || userRoles.Contains("HR"))
            {
                ViewBag.Centres = _context.Centres
       .Select(m => new {
           Value = m.CentreNumber,
           Text = m.CentreName
       })
       .Distinct()
       .OrderBy(c => c.Text)
       .ToList();

        

                ViewBag.Districts = _context.Districts
                    .Where(m => m.DistrictCode != null)
                    .Select(m => new {
                        Value = m.DistrictCode,
                        Text = m.DistrictName
                    })
                    .Distinct()
                    .OrderBy(d => d.Text)
                    .ToList();

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


            }
            ViewBag.RegionCode = currentUser.Region;

            return View();

        }

        // POST: ExamMonitors/GetExamMonitorsGetExamMonitorsTransactions
        [HttpPost]
        public async Task<IActionResult> GetExamMonitorsTransactions([FromBody] DtParameters dtParameters)
        {
            try
            {

                // Initialize with default values if null
                dtParameters ??= new DtParameters();

                var searchValue = dtParameters.Search?.Value ?? string.Empty;

                var statusFilter = dtParameters.StatusFilter ?? string.Empty;
                var genderFilter = dtParameters.GenderFilter ?? string.Empty;
                var centreFilter = dtParameters.CentreFilter ?? string.Empty;
                var districtFilter = dtParameters.DistrictFilter ?? string.Empty;
                var clusterFilter = dtParameters.ClusterFilter ?? string.Empty;
                var phaseFilter = dtParameters.PhaseFilter ?? string.Empty;
                var sessionFilter = dtParameters.SessionFilter ?? string.Empty;

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);

                // Base query
                IQueryable<ExamMonitorTransaction> query = _context.ExamMonitorTransactions.AsQueryable();

                if (userRoles.Contains("RegionalManager"))
                {
                    // For Regional Managers - enforce region filter from their profile
                    if (string.IsNullOrEmpty(currentUser.Region))
                    {
                        // Return empty result if region is missing for Regional Manager
                        return Json(new { data = new List<ExamMonitor>() });
                    }
                    query = query.Where(x => x.Region == currentUser.Region);
                }
                else if (userRoles.Contains("Admin") || userRoles.Contains("HR"))
                {
                    var regionFilter = dtParameters.RegionFilter ?? string.Empty;
                    // For Admins - apply region filter only if specified
                    if (!string.IsNullOrEmpty(regionFilter))
                    {
                        query = query.Where(x => x.Region == regionFilter);
                    }
                    // If no region filter specified, Admin sees all records
                }



                if (!string.IsNullOrEmpty(centreFilter))
                {
                    query = query.Where(m => m.CentreAttached == centreFilter);
                }

                if (!string.IsNullOrEmpty(districtFilter))
                {
                    query = query.Where(m => m.District == districtFilter);
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
                        m.ExamMonitor.FirstName.Contains(searchValue) ||
                        m.ExamMonitor.LastName.Contains(searchValue) ||
                        m.ExamMonitor.NationalId.Contains(searchValue));
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
                var examMonitorTransactions = query
                        .Select(m => new
                        {
                            SubKey = m.SubKey,
                            NationalId = m.NationalId,

                            FirstName = m.ExamMonitor.FirstName,
                            LastName = m.ExamMonitor.LastName,
                    
                            Status = m.Status,

                            Region = m.Region,
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
                    Data = examMonitorTransactions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] ExamMonitor monitor)
        {
            try
            {
                // Generate new GUID for MonitorId
                monitor.MonitorId = Guid.NewGuid();

                // Add validation as needed
                if (_context.ExamMonitors.Any(m => m.NationalId == monitor.NationalId))
                {
                    return Json(new { success = false, message = "Monitor with this National ID already exists" });
                }

                _context.ExamMonitors.Add(monitor);
                _context.SaveChanges();

                return Json(new { success = true, message = "Monitor created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GetClusterMonitors([FromBody] DtParameters dtParameters)
        {
            try
            {
                var searchValue = dtParameters.Search?.Value;
                var orderColumnIndex = dtParameters.Order[0]?.Column;
                var orderDirection = dtParameters.Order[0]?.Dir;

                IQueryable<ExamMonitor> query = _context.ExamMonitors;

                // Search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.FirstName.Contains(searchValue) ||
                        (m.NationalId != null && m.LastName.Contains(searchValue))
                    );
                }

                // Ordering


                // Paging
                var totalRecords = query.Count();
                var materials = query
                    .Skip(dtParameters.Start)
                    .Take(dtParameters.Length)
                        .Select(m => new
                        {
                            NationalId = m.NationalId,
                            MonitorId = m.MonitorId,
                            FirstName = m.FirstName,
                            LastName = m.LastName,
                            Sex = m.Sex,
                            Status = m.Status,
                            Qualification = m.Qualification,
                            Experience = m.Experience,
                            Region = m.Region,
                            Phone = m.Phone,
                            Age = m.Age,
                            Centre = m.Centre,
                            Station = m.Station,
                            District = m.District,
                            BankNameZwg = m.BankNameZwg ?? "",
                            BranchZwg = m.BranchZwg ?? "",
                            AccountNumberZwg = m.AccountNumberZwg ?? "",
                            BankNameUsd = m.BankNameUsd,
                            BranchUsd = m.BranchUsd,
                            AccountNumberUsd = m.AccountNumberUsd
                        })
                        .ToList();

                return Json(new
                {
                    draw = dtParameters.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = materials
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: ExamMonitors/GetMonitor/{nationalId}



        [HttpPost]
        public async Task<ActionResult> UpdateMonitorTransaction([FromForm] MonitorTransactionUpdateDto dto)
        {
            try
            {
                var examMonitor = await _context.ExamMonitorTransactions.FirstOrDefaultAsync(m => m.SubKey == dto.SubKey);

                if (examMonitor == null)
                {
                    return Json(new { success = false, message = "Monitor  not selected contact Admin found" });
                }

                // Update properties from DTO
                examMonitor.Session = dto.Session;
                examMonitor.Phase = dto.Phase;
                examMonitor.CentreAttached = dto.CentreAttached;
                examMonitor.Status = dto.Status;
                examMonitor.Region = dto.Region;

                _context.ExamMonitorTransactions.Update(examMonitor);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Monitor updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating monitor: " + ex.Message });
            }
        }



        // DELETE: ExamMonitors/DeleteMonitor/{nationalId}
        [HttpPost]

        public async Task<IActionResult> DeleteMonitorTransaction(string subkey)
        {
            try
            {
                var monitorTransaction = _context.ExamMonitorTransactions.FirstOrDefault(m => m.SubKey == subkey);
                if (monitorTransaction == null)
                {
                    return Json(new { success = false, message = "Monitor Transaction not found" });
                }

                _context.ExamMonitorTransactions.Remove(monitorTransaction);
              await  _context.SaveChangesAsync();

                return Json(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // GET: ExamMonitors/Details/{nationalId}
        public async Task<ActionResult> Details(string id)
        {
            var monitor = await _context.ExamMonitors
                .FirstOrDefaultAsync(m => m.NationalId == id);

            if (monitor == null)
            {
                return NotFound();
            }

            return View(monitor);
        }




        public async Task<IActionResult> AssignMonitors()
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);
                // Get all clusters (both IsCluster and IsResident)
                var clusterList = await _context.Centres
                    .Where(a => a.IsCluster == "IsCluster" && a.RegionCode == currentUser.Region)
                    .Select(c => new { Id = c.CentreNumber, Name = c.ClusterName, Type = "IsCluster" })
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var isResidentList = await _context.Centres
                    .Where(a => a.IsResident == "IsResident" && a.RegionCode == currentUser.Region)
                    .Select(c => new { Id = c.CentreNumber, Name = c.CentreName, Type = "IsResident" })
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Clusters = clusterList.Concat(isResidentList).ToList();
                ViewBag.Phases = await _context.Phases
                    .Select(p => new { Value = p.PhaseCode, Text = p.PhaseName })
                    .OrderBy(p => p.Text)
                    .ToListAsync();

                ViewBag.Sessions = await _context.ExamSessions
                    .Select(s => new { Value = s.SessionCode, Text = s.SessionName })
                    .OrderBy(s => s.Text)
                    .ToListAsync();
                ViewBag.RegionCode = currentUser.Region;
                return View();
            }
            catch (Exception ex)
            {
               
                return View("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAssignmentData(string clusterId, string phase, string session, string clusterType)
        {
            try
            {
                if (string.IsNullOrEmpty(clusterId))
                    return BadRequest("Cluster ID is required");

                var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRegion = currentUser?.Region;

                // 1. Get current assignments FOR THIS CLUSTER (to display current assignments)
                var currentClusterAssignments = await _context.ExamMonitorTransactions
                    .Include(t => t.ExamMonitor)
                    .Where(t => t.CentreAttached == clusterId &&
                               t.Phase == phase &&
                               t.Session == session &&
                               t.Region == userRegion)
                    .ToListAsync();

                // 2. Get ALL assignments IN THIS PHASE/SESSION (to filter available monitors)
                var allAssignmentsInPhase = await _context.ExamMonitorTransactions
                    .Include(t => t.ExamMonitor)
                    .Where(t => t.Phase == phase &&
                               t.Session == session &&
                               t.Region == userRegion && t.CentreAttached != "XXXXXX")
                    .ToListAsync();

                // 3. Get IDs of all assigned monitors (for any cluster)
                var assignedMonitorIds = allAssignmentsInPhase
                    .Select(a => a.ExamMonitor?.NationalId)
                    .Where(id => id != null)
                    .Distinct()
                    .ToList();

                // 4. Get ALL monitors in the region
                var allMonitors = await _context.ExamMonitorTransactions
                    .Include(a => a.ExamMonitor)
                    .Where(m => m.Region == userRegion && m.Phase == phase && m.Session == session)
                    .Select(m => new {
                        id = m.NationalId,
                        name = $"{m.ExamMonitor.FirstName} {m.ExamMonitor.LastName} ({m.NationalId})"
                    })
                    .ToListAsync();

                // 5. Filter available monitors (not assigned to ANY cluster)
                var availableMonitors = allMonitors
                    .Where(m => !assignedMonitorIds.Contains(m.id))
                    .ToList();

                return Json(new
                {
                    success = true,
                    clusterId,
                    allMonitors, // For displaying current assignments
                    availableMonitors, // For dropdown options
                    isCluster = clusterType == "IsCluster",
                    currentResident = currentClusterAssignments
                        .FirstOrDefault(a => a.Status == "Resident Monitor")?.ExamMonitor?.NationalId,
                    currentManager = currentClusterAssignments
                        .FirstOrDefault(a => a.Status == "Cluster Manager")?.ExamMonitor?.NationalId,
                    currentAssistant = currentClusterAssignments
                        .FirstOrDefault(a => a.Status == "Assistant Cluster Manager")?.ExamMonitor?.NationalId
                });
            }
            catch (Exception ex)
            {
              
                return Json(new { success = false, message = "Error loading assignment data" });
            }
        }

  

        // Result tracking class
   

        //[HttpPost]
        //public async Task<IActionResult> SaveAssignments([FromBody] List<AssignmentDto> assignments)
        //{
        //    try
        //    {
        //        if (assignments == null || !assignments.Any())
        //            return BadRequest("No assignments provided");
        //        var currentUser = await _signInManager.UserManager.GetUserAsync(User);


        //        var operations = new List<Task>();
        //        var usersToCreate = new List<ApplicationUser>();
        //        var transactionsToAdd = new List<ExamMonitorTransaction>();

        //        foreach (var assignment in assignments)
        //        {
        //            // Remove existing assignments



        //              if(assignment.ClusterManagerId  != null)
        //                {
        //                var exist = await _context.ExamMonitorTransactions
        //                .FirstOrDefaultAsync(t => t.CentreAttached == assignment.CenterId &&
        //                           t.Phase == assignment.Phase &&
        //                           t.Session == assignment.Session && t.Region == currentUser.Region && t.Status == "Cluster Manager");
        //                if (exist != null)
        //                {
        //                    exist.CentreAttached = "000000";
        //                    _context.ExamMonitorTransactions.Update(exist);
        //                    await _context.SaveChangesAsync();

        //                    await HandleUserRemoval(exist);
        //                }


        //                }
        //            else if(assignment.AssistantResidentId != null)
        //            {
        //                var exist = await _context.ExamMonitorTransactions
        //              .FirstOrDefaultAsync(t => t.CentreAttached == assignment.CenterId &&
        //                         t.Phase == assignment.Phase &&
        //                         t.Session == assignment.Session && t.Region == currentUser.Region && t.Status == "Assistant Cluster Manager");

        //                if (exist != null)
        //                {
        //                    exist.CentreAttached = "000000";
        //                    _context.ExamMonitorTransactions.Update(exist);
        //                    await _context.SaveChangesAsync();

        //                    await HandleUserRemoval(exist);
        //                }
        //            }
        //            else if (assignment.ResidentMonitorId != null)
        //            {
        //                var exist = await _context.ExamMonitorTransactions
        //              .FirstOrDefaultAsync(t => t.CentreAttached == assignment.CenterId &&
        //                         t.Phase == assignment.Phase &&
        //                         t.Session == assignment.Session && t.Region == currentUser.Region && t.Status == "Resident Monitor");

        //                if (exist != null)
        //                {
        //                    exist.CentreAttached = "000000";
        //                    _context.ExamMonitorTransactions.Update(exist);
        //                    await _context.SaveChangesAsync();

        //                    await HandleUserRemoval(exist);
        //                }
        //            }

        //                var district = string.Empty;
        //            var centreDetails = await _context.Centres.FirstOrDefaultAsync(a => a.CentreNumber == assignment.CenterId);
        //            if(centreDetails != null)
        //            {
        //                district = centreDetails.DistrictCode;
        //            }

        //                // Process Resident Monitor
        //                if (!string.IsNullOrEmpty(assignment.ResidentMonitorId))
        //            {
        //                var subKey = currentUser.Region + assignment.Phase + assignment.Session + assignment.ResidentMonitorId;
        //                var monitorTransaction = await _context.ExamMonitorTransactions.Include(a => a.ExamMonitor).FirstOrDefaultAsync(a => a.NationalId == assignment.ResidentMonitorId && a.SubKey == subKey);

        //                if(monitorTransaction  != null)
        //                {
        //                    if (monitorTransaction.CentreAttached != "000000")
        //                    {
        //                        //they are already assigned somewhere because 000000 id default
        //                    }

        //                    if (monitorTransaction.ExamMonitor.Centre == assignment.CenterId)
        //                    {
        //                        //resident monitors can not be assigned to their station or centre they are from
        //                    }


        //                    monitorTransaction.Status = "Resident Monitor";
        //                    monitorTransaction.District = district;
        //                    monitorTransaction.CentreAttached = assignment.CenterId;
        //                    monitorTransaction.Phase = assignment.Phase;
        //                    monitorTransaction.Session = assignment.Session;
        //                    monitorTransaction.DeployStatus = "Deployed";
        //                    monitorTransaction.DeployStatusBy = currentUser.UserName;
        //                    monitorTransaction.DeployStatusDate = DateTime.Now;
        //                     _context.ExamMonitorTransactions.Update(monitorTransaction);

        //                    transactionsToAdd.Add(new ExamMonitorTransaction
        //                    {
        //                        SubKey = subKey,
        //                        NationalId = assignment.AssistantResidentId,
        //                        MonitorId = monitorTransaction.ExamMonitor.MonitorId,
        //                        Region = currentUser.Region ?? monitorTransaction.ExamMonitor.Region,
        //                        District = district,
        //                        CentreAttached = assignment.CenterId,
        //                        Phase = assignment.Phase,
        //                        Session = assignment.Session,
        //                        Status = "Resident Monitor",
        //                        AssignedDate = DateTime.UtcNow,
        //                        AssignedBy = currentUser.UserName
        //                    });

        //                }
        //                else
        //                {

        //                }


        //            }

        //            // Process Cluster assignments if needed
        //            if (assignment.Status == "IsCluster")
        //            {
        //                if (!string.IsNullOrEmpty(assignment.ClusterManagerId))
        //                {
        //                    var subKey = currentUser.Region + assignment.Phase + assignment.Session + assignment.ClusterManagerId;
        //                    var monitorTransaction = await _context.ExamMonitorTransactions.Include(a => a.ExamMonitor)
        //                        .FirstOrDefaultAsync(a => a.NationalId == assignment.ClusterManagerId && a.SubKey == subKey);

        //                    if (monitorTransaction != null)
        //                    {


        //                        if (monitorTransaction.CentreAttached != "000000")
        //                        {
        //                            //they are already assigned somewhere because 000000 id default
        //                        }

        //                        monitorTransaction.Status = "Cluster Manager";
        //                        monitorTransaction.District = district;
        //                        monitorTransaction.CentreAttached = assignment.CenterId;
        //                        monitorTransaction.Phase = assignment.Phase;
        //                        monitorTransaction.Session = assignment.Session;
        //                        monitorTransaction.DeployStatus = "Deployed";
        //                        monitorTransaction.DeployStatusBy = currentUser.UserName;
        //                        monitorTransaction.DeployStatusDate = DateTime.Now;
        //                        _context.ExamMonitorTransactions.Update(monitorTransaction);

        //                        transactionsToAdd.Add(new ExamMonitorTransaction
        //                        {
        //                            SubKey = subKey,
        //                            NationalId = assignment.ClusterManagerId,
        //                            MonitorId = monitorTransaction.ExamMonitor.MonitorId,
        //                            Region = currentUser.Region ?? monitorTransaction.ExamMonitor.Region,
        //                            District = district,
        //                            CentreAttached = assignment.CenterId,
        //                            Phase = assignment.Phase,
        //                            Session = assignment.Session,
        //                            Status = "Cluster Manager",
        //                            AssignedDate = DateTime.UtcNow,
        //                            AssignedBy = currentUser.UserName
        //                        });

        //                    }
        //                    else
        //                    {

        //                    }

        //                    }


        //                if (!string.IsNullOrEmpty(assignment.AssistantResidentId))
        //                {
        //                    var subKey = currentUser.Region + assignment.Phase + assignment.Session + assignment.AssistantResidentId;
        //                    var monitorTransaction = await _context.ExamMonitorTransactions.Include(a => a.ExamMonitor)
        //                        .FirstOrDefaultAsync(a => a.NationalId == assignment.AssistantResidentId && a.SubKey == subKey);

        //                    if (monitorTransaction != null)
        //                    {

        //                        if (monitorTransaction.CentreAttached != "000000")
        //                        {
        //                            //they are already assigned somewhere because 000000 id default
        //                        }

        //                        monitorTransaction.Status = "Assistant Cluster Manager";
        //                        monitorTransaction.District = district;
        //                        monitorTransaction.CentreAttached = assignment.CenterId;
        //                        monitorTransaction.Phase = assignment.Phase;
        //                        monitorTransaction.Session = assignment.Session;
        //                        monitorTransaction.DeployStatus = "Deployed";
        //                        monitorTransaction.DeployStatusBy = currentUser.UserName;
        //                        monitorTransaction.DeployStatusDate = DateTime.Now;
        //                        _context.ExamMonitorTransactions.Update(monitorTransaction);

        //                        transactionsToAdd.Add(new ExamMonitorTransaction
        //                        {
        //                            SubKey = subKey,
        //                            NationalId = assignment.AssistantResidentId,
        //                            MonitorId = monitorTransaction.ExamMonitor.MonitorId,
        //                            Region = currentUser.Region ?? monitorTransaction.ExamMonitor.Region,
        //                            District = district,
        //                            CentreAttached = assignment.CenterId,
        //                            Phase = assignment.Phase,
        //                            Session = assignment.Session,
        //                            Status = "Assistant Cluster Manager",
        //                            AssignedDate = DateTime.UtcNow,
        //                            AssignedBy = currentUser.UserName
        //                        });

        //                    }
        //                    else
        //                    {

        //                    }


        //                }

        //            }

        //            await _context.SaveChangesAsync();

        //            foreach (var item in transactionsToAdd)
        //            {
        //                await HandleMonitorUserAccountCreationSelection(item);
        //            }


        //        }

        //        return Json(new { success = true, message = "Assignments saved successfully" });
        //    }
        //    catch (Exception ex)
        //    {

        //        return StatusCode(500, new { success = false, message = "Error saving assignments" });
        //    }
        //}

        private async Task HandleMonitorUserAccountCreationSelection(ExamMonitorTransaction model)
        {
            //var subkey = $"{model.Region}{model.Phase}{model.Session}{model.NationalId}";
      
            var monitor = await _context.ExamMonitors.FirstOrDefaultAsync(a => a.NationalId == model.NationalId);

            var user = await _userRepository.GetUser(model.NationalId, model.SubKey);
            model.Status ??= "Resident Monitor"; // Default category

            if (user == null)
            {
                await CreateNewUser(model,monitor, model.SubKey);
            }
            else
            {
                await UpdateExistingUser(user, model, model.SubKey);
            }
        }

 

            public async Task CreateNewUser(ExamMonitorTransaction model,ExamMonitor monitor, string subkey)
            {
                var username = GenerateUsername(monitor);


                var newUser = new ApplicationUser
                {
                    UserName = username,
                    Email = $"{username}@ems.com",
                    EMS_SUBKEY = subkey,
                    Region = model.Region,
                    PhoneNumber = monitor.Phone ?? "0000000000",
                    IDNumber = model.NationalId,
                    ExaminerCode = "M100",
                    Activated = true,
                    LockoutEnabled = true,
                    EmailConfirmed = true,
                    Activity = "ExamMonitoring"
                };
                var password = GenerateDefaultPassword(newUser, model.CentreAttached);
                var result = await _userManager.CreateAsync(newUser, password);

                if (result.Succeeded)
                {
                    await AssignRole(newUser, model.Status);
                }
                else
                {
                    throw new ApplicationException($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            public async Task UpdateExistingUser(ApplicationUser user, ExamMonitorTransaction model, string subkey)
            {
                user.EMS_SUBKEY = subkey;
                user.Activity = "ExamMonitoring";

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await ReassignRole(user, model.Status);
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
                    "Resident Monitor" => "ResidentMonitor",
                    "Cluster Manager" => "ClusterManager",
                    "Assistant Cluster Manager" => "AssistantClusterManager",
                 
                    _ => "ResidentMonitor" // Default role
                };
            }

            private string GenerateUsername(ExamMonitor examiner)
            {
                string cleanFirstName = RemoveMiddleName(examiner.FirstName);
                string cleanSurname = RemoveMiddleName(examiner.LastName);

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


            private string GenerateDefaultPassword(ApplicationUser user, string centreNumber)
            {
                // Combine the parts of the password
                string password = $"{user.UserName.ToLower()}{centreNumber}.*";
                return password;
            }
            private string RemoveMiddleName(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return string.Empty;

                var nameParts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return nameParts.Length > 0 ? nameParts[0] : string.Empty;
            }

        private async Task HandleUserRemoval(ExamMonitorTransaction model)
        {
            //var key = $"{model.ExamCode}{model.SubjectCode}{model.PaperCode}{model.Activity}{model.IdNumber}";
            await _userRepository.DeleteUserWithKey(model.NationalId, model.SubKey);
        }

        //private async Task HandleTandSRemoval(ExaminerUpdateModel model, ApplicationUser applicationUser)
        //{
        //    var key = $"{model.ExamCode}{model.SubjectCode}{model.PaperCode}{model.Activity}{model.IdNumber}";
        //    await _andSRepository.DeleteTandS(model.IdNumber, key, applicationUser);
        //}


        // Result tracking class (keep this internal)
        [HttpPost]
        public async Task<IActionResult> SaveAssignments([FromBody] List<AssignmentDto> assignments)
        {
            try
            {
                if (assignments == null || !assignments.Any())
                    return BadRequest("No assignments provided");

                var currentUser = await _signInManager.UserManager.GetUserAsync(User);

                // Track deployment results using DTO
                var deploymentResults = new List<DeploymentResultDto>();
                var transactionsToAdd = new List<ExamMonitorTransaction>();

                foreach (var assignment in assignments)
                {
                    // Remove existing assignments
                    if (assignment.ClusterManagerId != null)
                    {
                        var exist = await _context.ExamMonitorTransactions
                            .FirstOrDefaultAsync(t => t.CentreAttached == assignment.CenterId &&
                                       t.Phase == assignment.Phase &&
                                       t.Session == assignment.Session && t.Region == currentUser.Region && t.Status == "Cluster Manager");
                        if (exist != null)
                        {
                            exist.CentreAttached = "XX"+ exist.CentreAttached;
                            _context.ExamMonitorTransactions.Update(exist);
                            await HandleUserRemoval(exist);
                        }
                    }
                    else if (assignment.AssistantResidentId != null)
                    {
                        var exist = await _context.ExamMonitorTransactions
                            .FirstOrDefaultAsync(t => t.CentreAttached == assignment.CenterId &&
                                       t.Phase == assignment.Phase &&
                                       t.Session == assignment.Session && t.Region == currentUser.Region && t.Status == "Assistant Cluster Manager");
                        if (exist != null)
                        {
                            exist.CentreAttached = "XX" + exist.CentreAttached;
                            _context.ExamMonitorTransactions.Update(exist);
                            await HandleUserRemoval(exist);
                        }
                    }
                    else if (assignment.ResidentMonitorId != null)
                    {
                        var exist = await _context.ExamMonitorTransactions
                            .FirstOrDefaultAsync(t => t.CentreAttached == assignment.CenterId &&
                                       t.Phase == assignment.Phase &&
                                       t.Session == assignment.Session && t.Region == currentUser.Region && t.Status == "Resident Monitor");
                        if (exist != null)
                        {
                            exist.CentreAttached = "XX" + exist.CentreAttached;
                            _context.ExamMonitorTransactions.Update(exist);
                            await HandleUserRemoval(exist);
                        }
                    }

                    var district = string.Empty;
                    var centreDetails = await _context.Centres.FirstOrDefaultAsync(a => a.CentreNumber == assignment.CenterId);
                    if (centreDetails != null)
                    {
                        district = centreDetails.DistrictCode;
                    }

                    // Process Resident Monitor
                    if (!string.IsNullOrEmpty(assignment.ResidentMonitorId))
                    {
                        var result = await ProcessResidentMonitorAssignment(assignment, currentUser, district);

                        // Convert to DTO
                        deploymentResults.Add(new DeploymentResultDto
                        {
                            MonitorId = result.MonitorId,
                            Role = result.Role,
                            CenterId = result.CenterId,
                            IsSuccess = result.IsSuccess,
                            ErrorReason = result.ErrorReason,
                            MonitorName = await GetMonitorName(result.MonitorId)
                        });

                        if (result.IsSuccess && result.Transaction != null)
                        {
                            transactionsToAdd.Add(result.Transaction);
                        }
                    }

                    // Process Cluster assignments if needed
                    if (assignment.Status == "IsCluster")
                    {
                        if (!string.IsNullOrEmpty(assignment.ClusterManagerId))
                        {
                            var result = await ProcessClusterManagerAssignment(assignment, currentUser, district);

                            deploymentResults.Add(new DeploymentResultDto
                            {
                                MonitorId = result.MonitorId,
                                Role = result.Role,
                                CenterId = result.CenterId,
                                IsSuccess = result.IsSuccess,
                                ErrorReason = result.ErrorReason,
                                MonitorName = await GetMonitorName(result.MonitorId)
                            });

                            if (result.IsSuccess && result.Transaction != null)
                            {
                                transactionsToAdd.Add(result.Transaction);
                            }
                        }

                        if (!string.IsNullOrEmpty(assignment.AssistantResidentId))
                        {
                            var result = await ProcessAssistantResidentAssignment(assignment, currentUser, district);

                            deploymentResults.Add(new DeploymentResultDto
                            {
                                MonitorId = result.MonitorId,
                                Role = result.Role,
                                CenterId = result.CenterId,
                                IsSuccess = result.IsSuccess,
                                ErrorReason = result.ErrorReason,
                                MonitorName = await GetMonitorName(result.MonitorId)
                            });

                            if (result.IsSuccess && result.Transaction != null)
                            {
                                transactionsToAdd.Add(result.Transaction);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Process user account creation for successful deployments
                foreach (var transaction in transactionsToAdd)
                {
                    await HandleMonitorUserAccountCreationSelection(transaction);
                }

                // Generate summary message
                var successCount = deploymentResults.Count(r => r.IsSuccess);
                var failedCount = deploymentResults.Count(r => !r.IsSuccess);

                var message = $"{successCount} monitor(s) deployed successfully.";

                if (failedCount > 0)
                {
                    message += $" {failedCount} monitor(s) failed deployment:";

                    var errors = deploymentResults
                        .Where(r => !r.IsSuccess)
                        .GroupBy(r => r.ErrorReason)
                        .Select(g => $"{g.Count()} {g.Key}")
                        .ToList();

                    message += " " + string.Join(", ", errors);
                }

                // Return using DTO to avoid circular references
                return Json(new AssignmentResponseDto
                {
                    Success = true,
                    Message = message,
                    DeployedCount = successCount,
                    FailedCount = failedCount,
                    DetailedResults = deploymentResults
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AssignmentResponseDto
                {
                    Success = false,
                    Message = "Error saving assignments: " + ex.Message
                });
            }
        }

        // Helper method to get monitor name
        private async Task<string> GetMonitorName(string nationalId)
        {
            if (string.IsNullOrEmpty(nationalId))
                return "Unknown Monitor";

            var monitor = await _context.ExamMonitors
                .Where(m => m.NationalId == nationalId)
                .Select(m => new { m.FirstName, m.LastName })
                .FirstOrDefaultAsync();

            return monitor != null ? $"{monitor.FirstName} {monitor.LastName}" : "Unknown Monitor";
        }

        // Helper methods for processing different assignment types
        private async Task<DeploymentResult> ProcessResidentMonitorAssignment(AssignmentDto assignment, ApplicationUser currentUser, string district)
        {
            var result = new DeploymentResult
            {
                MonitorId = assignment.ResidentMonitorId,
                Role = "Resident Monitor",
                CenterId = assignment.CenterId
            };

            try
            {
                var subKey = currentUser.Region + assignment.Phase + assignment.Session + assignment.ResidentMonitorId;
                var monitorTransaction = await _context.ExamMonitorTransactions
                    .Include(a => a.ExamMonitor)
                    .FirstOrDefaultAsync(a => a.NationalId == assignment.ResidentMonitorId && a.SubKey == subKey);

                if (monitorTransaction == null)
                {
                    result.IsSuccess = false;
                    result.ErrorReason = "not found in transactions";
                    return result;
                }

                // Check if already assigned elsewhere
                if (monitorTransaction.CentreAttached != "XXXXXX")
                {
                    result.IsSuccess = false;
                    result.ErrorReason = "already assigned to another center";
                    return result;
                }

                // Check if assigned to their own center
                if (monitorTransaction.ExamMonitor?.Centre == assignment.CenterId)
                {
                    result.IsSuccess = false;
                    result.ErrorReason = "cannot be assigned to their origin center";
                    return result;
                }

                // Update transaction
                monitorTransaction.Status = "Resident Monitor";
                monitorTransaction.District = district;
                monitorTransaction.CentreAttached = assignment.CenterId;
                monitorTransaction.Phase = assignment.Phase;
                monitorTransaction.Session = assignment.Session;
                monitorTransaction.DeployStatus = "Deployed";
                monitorTransaction.DeployStatusBy = currentUser.UserName;
                monitorTransaction.DeployStatusDate = DateTime.Now;

                _context.ExamMonitorTransactions.Update(monitorTransaction);

                result.IsSuccess = true;
                result.Transaction = monitorTransaction;
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorReason = $"error: {ex.Message}";
                return result;
            }
        }

        private async Task<DeploymentResult> ProcessClusterManagerAssignment(AssignmentDto assignment, ApplicationUser currentUser, string district)
        {
            var result = new DeploymentResult
            {
                MonitorId = assignment.ClusterManagerId,
                Role = "Cluster Manager",
                CenterId = assignment.CenterId
            };

            try
            {
                var subKey = currentUser.Region + assignment.Phase + assignment.Session + assignment.ClusterManagerId;
                var monitorTransaction = await _context.ExamMonitorTransactions
                    .Include(a => a.ExamMonitor)
                    .FirstOrDefaultAsync(a => a.NationalId == assignment.ClusterManagerId && a.SubKey == subKey);

                if (monitorTransaction == null)
                {
                    result.IsSuccess = false;
                    result.ErrorReason = "not found in transactions";
                    return result;
                }

                if (monitorTransaction.CentreAttached != "XXXXXX")
                {
                    result.IsSuccess = false;
                    result.ErrorReason = "already assigned to another center";
                    return result;
                }

                // Update transaction
                monitorTransaction.Status = "Cluster Manager";
                monitorTransaction.District = district;
                monitorTransaction.CentreAttached = assignment.CenterId;
                monitorTransaction.Phase = assignment.Phase;
                monitorTransaction.Session = assignment.Session;
                monitorTransaction.DeployStatus = "Deployed";
                monitorTransaction.DeployStatusBy = currentUser.UserName;
                monitorTransaction.DeployStatusDate = DateTime.Now;

                _context.ExamMonitorTransactions.Update(monitorTransaction);

                result.IsSuccess = true;
                result.Transaction = monitorTransaction;
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorReason = $"error: {ex.Message}";
                return result;
            }
        }

        private async Task<DeploymentResult> ProcessAssistantResidentAssignment(AssignmentDto assignment, ApplicationUser currentUser, string district)
        {
            var result = new DeploymentResult
            {
                MonitorId = assignment.AssistantResidentId,
                Role = "Assistant Cluster Manager",
                CenterId = assignment.CenterId
            };

            try
            {
                var subKey = currentUser.Region + assignment.Phase + assignment.Session + assignment.AssistantResidentId;
                var monitorTransaction = await _context.ExamMonitorTransactions
                    .Include(a => a.ExamMonitor)
                    .FirstOrDefaultAsync(a => a.NationalId == assignment.AssistantResidentId && a.SubKey == subKey);

                if (monitorTransaction == null)
                {
                    result.IsSuccess = false;
                    result.ErrorReason = "not found in transactions";
                    return result;
                }

                if (monitorTransaction.CentreAttached != "XXXXXX")
                {
                    result.IsSuccess = false;
                    result.ErrorReason = "already assigned to another center";
                    return result;
                }

                // Update transaction
                monitorTransaction.Status = "Assistant Cluster Manager";
                monitorTransaction.District = district;
                monitorTransaction.CentreAttached = assignment.CenterId;
                monitorTransaction.Phase = assignment.Phase;
                monitorTransaction.Session = assignment.Session;
                monitorTransaction.DeployStatus = "Deployed";
                monitorTransaction.DeployStatusBy = currentUser.UserName;
                monitorTransaction.DeployStatusDate = DateTime.Now;

                _context.ExamMonitorTransactions.Update(monitorTransaction);

                result.IsSuccess = true;
                result.Transaction = monitorTransaction;
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorReason = $"error: {ex.Message}";
                return result;
            }
        }

    }

    public class MonitorTransactionUpdateDto
    {
        public string SubKey { get; set; }
        public string NationalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
     
        public string Status { get; set; }
        public string Region { get; set; }
        public string Phase { get; set; }
        public string CentreAttached { get; set; }
        public string Session { get; set; }

    }



public class AssignmentDto
{
    public string CenterId { get; set; }
    public string ClusterManagerId { get; set; }
    public string ResidentMonitorId { get; set; }
    public string AssistantResidentId { get; set; }
    public string Phase { get; set; }
    public string Session { get; set; }
    public string Status { get; set; }
}

    public class DeploymentResultDto
    {
        public string MonitorId { get; set; }
        public string Role { get; set; }
        public string CenterId { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorReason { get; set; }
        public string MonitorName { get; set; }
    }

    public class AssignmentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int DeployedCount { get; set; }
        public int FailedCount { get; set; }
        public List<DeploymentResultDto> DetailedResults { get; set; }
    }
    public class AssignmentIndividual
    {
        public string CenterId { get; set; }
        public string ClusterManagerId { get; set; }
        public string ResidentMonitorId { get; set; }
        public string AssistantResidentId { get; set; }
        public string Phase { get; set; }
        public string Session { get; set; }
        public string Status { get; set; }
    }

    public class DeploymentResult
    {
        public string MonitorId { get; set; }
        public string Role { get; set; }
        public string CenterId { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorReason { get; set; }
        public ExamMonitorTransaction Transaction { get; set; }
    }
}
