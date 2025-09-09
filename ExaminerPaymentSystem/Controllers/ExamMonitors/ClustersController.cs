//using AutoMapper;
//using ClosedXML.Excel;
//using ExaminerPaymentSystem.Data;
//using ExaminerPaymentSystem.Models.ExamMonitors;
//using ExaminerPaymentSystem.Services.ExamMonitors;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Threading.Tasks;

//namespace ExaminerPaymentSystem.Controllers.ExamMonitors
//{
//    public class ClustersController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        private readonly IMapper _mapper;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public ClustersController(IMapper mapper, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
//        {

//            _mapper = mapper;
//            _context = context;
//            _signInManager = signInManager;
//            _userManager = userManager;
//        }

//        public async Task<IActionResult> BulkAddClusters()
//        {
//            return View();
//        }

//        public async Task<IActionResult> DownloadClustersTemplate()
//        {


//            using (var workbook = new XLWorkbook())
//            {
//                var worksheet = workbook.Worksheets.Add("Clusters");

//                // Header row
//                worksheet.Cell(1, 1).Value = "CLUSTER CODE";
//                worksheet.Cell(1, 2).Value = "CLISTER NAME";
//                worksheet.Cell(1, 3).Value = "DISTRICT CODE";
//                worksheet.Cell(1, 4).Value = "DISTRICT NAME";
               
             



//                using (var stream = new MemoryStream())
//                {
//                    workbook.SaveAs(stream);
//                    stream.Seek(0, SeekOrigin.Begin);
//                    return File(stream.ToArray(),
//                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                                "ClustersTemplate.xlsx");
//                }
//            }
//        }


//        [HttpPost]
//        public async Task<IActionResult> UploadClusters(IFormFile file, string regionCode = "")
//        {
//            if (file == null || file.Length == 0)
//            {
//                TempData["Error"] = "Please upload a valid Excel file.";
//                return RedirectToAction("BulkAddClusters");
//            }

//            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
//            // Create an execution strategy
//            var executionStrategy = _context.Database.CreateExecutionStrategy();

//            return await executionStrategy.ExecuteAsync(async () =>
//            {
//                await using var transaction = await _context.Database.BeginTransactionAsync();

//                try
//                {
//                    _context.ChangeTracker.Clear();

//                    var clusters = new List<Cluster>();
//                    var clustersInFile = new HashSet<string>();
//                    var duplicateClusters = new HashSet<string>();
//                    var processedCount = 0;



//                    using (var stream = new MemoryStream())
//                    {
//                        await file.CopyToAsync(stream);
//                        using (var workbook = new XLWorkbook(stream))
//                        {
//                            var worksheet = workbook.Worksheet(1);
//                            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

//                            foreach (var row in rows)
//                            {
//                                var clusterCode = row.Cell(1).GetValue<string>()?.Trim();

//                                if (string.IsNullOrWhiteSpace(clusterCode))
//                                {
//                                    continue;
//                                }

//                                if (clustersInFile.Contains(clusterCode))
//                                {
//                                    duplicateClusters.Add(clusterCode);
//                                    continue;
//                                }

//                                clustersInFile.Add(clusterCode);
//                                processedCount++;



//                                var cluster = new Cluster
//                                {

//                                   ClusterCode = clusterCode,
//                                    ClusterName = row.Cell(2).GetValue<string>()?.Trim(),
//                                    DistrictCode = row.Cell(3).GetValue<string>()?.Trim(),
//                                    DistrictName = row.Cell(4).GetValue<string>()?.Trim(),
                                   


//                                };
//                                cluster.RegionCode = regionCode;
//                                clusters.Add(cluster);
//                            }
//                        }
//                    }
//                    //var existingRecords = new List<ExamMonitor>();
//                    // Final duplicate check before insert
//                    // Assuming examinerMonitors is a list of input objects
//                    var clusterCodes = clusters
//                        .Where(x => x.ClusterCode != null)
//                        .Select(x => x.ClusterCode!)
//                        .Distinct()
//                        .ToList();

//                    var existingRecords = await _context.Clusters
//                        .AsNoTracking()
//                        .Where(e => e.ClusterCode != null && clusterCodes.Contains(e.ClusterCode))
//                        .ToListAsync();

//                    var clustersNotInDB = clusters
//                        .Where(e => !existingRecords.Any(x =>
//                            x.ClusterCode == e.ClusterCode))
//                        .ToList();

//                    if (clustersNotInDB.Any())
//                    {
//                        _context.ChangeTracker.Clear();
//                        await _context.Clusters.AddRangeAsync(clustersNotInDB);
//                        await _context.SaveChangesAsync(currentUser.Id);
//                    }

//                    await transaction.CommitAsync();

//                    var message = $"{processedCount} cluster(s) processed, {clustersNotInDB.Count} new cluster (s) added.";
//                    if (duplicateClusters.Any())
//                    {
//                        message += $" {duplicateClusters.Count} duplicate(s) skipped in file.";
//                    }
//                    if (clusters.Count - clustersNotInDB.Count > 0)
//                    {
//                        message += $" {clusters.Count - clustersNotInDB.Count} existing cluster(s) skipped.";
//                    }

//                    TempData["Success"] = message;
//                    return RedirectToAction("BulkAddClusters");
//                }
//                catch (Exception ex)
//                {
//                    await transaction.RollbackAsync();
//                    TempData["Error"] = "An error occurred: " + ex.Message;
//                    return RedirectToAction("BulkAddCentres");
//                }
//            });
//        }


//        [HttpGet]
//        public async Task<JsonResult> GetCluster(string clusterCode)
//        {
//            try
//            {
//                var cluster = await _context.Clusters
//                    .FirstOrDefaultAsync(m => m.ClusterCode == clusterCode);

//                if (cluster == null)
//                {
//                    return Json(new { success = false, message = "Cluster not found" });
//                }

//                return Json(new
//                {
//                    success = true,
             
//                    DistrictCode = cluster.DistrictCode,
//                    DistrictName = cluster.DistrictName,
//                    ClusterCode = cluster.ClusterCode,
//                    ClusterName = cluster.ClusterName,
                    

//                });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, message = ex.Message });
//            }
//        }

//        // GET: ExamMonitors
//        public async Task<IActionResult>  Index()
//        {
//            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
//            var userRoles = await _userManager.GetRolesAsync(currentUser);

//            if (userRoles.Contains("RegionalManager"))
//            {
//                ViewBag.Centres = _context.Centres.Where(a => a.RegionCode == currentUser.Region)
//       .Select(m => new {
//           Value = m.CentreNumber,
//           Text = m.CentreName
//       })
//       .Distinct()
//       .OrderBy(c => c.Text)
//       .ToList();

//                ViewBag.Clusters = _context.Clusters
//                    .Where(a => a.RegionCode == currentUser.Region)
//           .Select(m => new {
//               Value = m.ClusterCode,
//               Text = m.ClusterName
//           })
//           .Distinct()
//           .OrderBy(c => c.Text)
//           .ToList();

//                ViewBag.Districts = _context.Districts
//                    .Where(m => m.DistrictCode != null && m.RegionCode == currentUser.Region)
//                    .Select(m => new {
//                        Value = m.DistrictCode,
//                        Text = m.DistrictName
//                    })
//                    .Distinct()
//                    .OrderBy(d => d.Text)
//                    .ToList();
//                ViewBag.RegionCode = currentUser.Region;
//            }
//            else if (userRoles.Contains("Admin"))
//            {
//                ViewBag.Centres = _context.Centres
//       .Select(m => new {
//           Value = m.CentreNumber,
//           Text = m.CentreName
//       })
//       .Distinct()
//       .OrderBy(c => c.Text)
//       .ToList();

//                ViewBag.Clusters = _context.Clusters
//           .Select(m => new {
//               Value = m.ClusterCode,
//               Text = m.ClusterName
//           })
//           .Distinct()
//           .OrderBy(c => c.Text)
//           .ToList();

//                ViewBag.Districts = _context.Districts
//                    .Where(m => m.DistrictCode != null)
//                    .Select(m => new {
//                        Value = m.DistrictCode,
//                        Text = m.DistrictName
//                    })
//                    .Distinct()
//                    .OrderBy(d => d.Text)
//                    .ToList();

//            }

//            ViewBag.RegionCode = currentUser.Region;
//            return View();
//        }


//        [HttpPost]
//        public async Task<IActionResult> GetClusters([FromBody] DtParameters dtParameters)
//        {
//            try
//            {

//                // Initialize with default values if null
//                dtParameters ??= new DtParameters();

//                var searchValue = dtParameters.Search?.Value ?? string.Empty;

//                var districtFilter = dtParameters.DistrictFilter ?? string.Empty;

//                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
//                var userRoles = await _userManager.GetRolesAsync(currentUser);

//                // Base query
//                IQueryable<Centre> query = _context.Centres.AsQueryable();

//                if (userRoles.Contains("RegionalManager"))
//                {
//                    // For Regional Managers - enforce region filter from their profile
//                    if (string.IsNullOrEmpty(currentUser.Region))
//                    {
//                        // Return empty result if region is missing for Regional Manager
//                        return Json(new { data = new List<Cluster>() });
//                    }
//                    query = query.Where(x => x.RegionCode == currentUser.Region);
//                }
//                else if (userRoles.Contains("Admin"))
//                {
//                    var regionFilter = dtParameters.RegionFilter ?? string.Empty;
//                    // For Admins - apply region filter only if specified
//                    if (!string.IsNullOrEmpty(regionFilter))
//                    {
//                        query = query.Where(x => x.RegionCode == regionFilter);
//                    }
//                    // If no region filter specified, Admin sees all records
//                }




            

//                if (!string.IsNullOrEmpty(districtFilter))
//                {
//                    query = query.Where(m => m.DistrictCode == districtFilter);
//                }

//                // Apply global search
//                if (!string.IsNullOrEmpty(searchValue))
//                {
//                    query = query.Where(m =>
//                        m.ClusterCode.Contains(searchValue) ||
                
//                        m.DistrictName.Contains(searchValue) ||
//                        m.ClusterName.Contains(searchValue));
//                }

//                // Get total count before paging
//                var totalRecords = await query.CountAsync();

//                // Apply sorting
//                if (dtParameters.Order != null && dtParameters.Order.Any())
//                {
//                    var order = dtParameters.Order.First();
//                    var columnIndex = order.Column;
//                    var columnName = dtParameters.Columns[columnIndex].Data;
//                    var isAscending = order.Dir == DtOrderDir.Asc;

//                    query = columnName switch
//                    {
//                        "clusterCode" => isAscending ? query.OrderBy(m => m.ClusterCode) : query.OrderByDescending(m => m.ClusterCode),
//                        "clusterName" => isAscending ? query.OrderBy(m => m.ClusterName) : query.OrderByDescending(m => m.ClusterName),
//                        _ => query.OrderBy(m => m.ClusterCode)
//                    };
//                }
//                else
//                {
//                    query = query.OrderBy(m => m.ClusterName);
//                }

//                // Apply paging
//                var filteredRecords = await query.CountAsync();
//                var start = Math.Max(0, dtParameters.Start);
//                var length = dtParameters.Length > 0 ? dtParameters.Length : 10;
//                var clusters = query
//                        .Skip(start)
//                    .Take(length)
//                        .Select(m => new
//                        {
                      
//                            ClusterName = m.ClusterName,
//                            ClusterCode = m.ClusterCode,
//                            DistrictCode = m.DistrictCode,
//                            DistrictName = m.DistrictName,
//                            RegionCode = m.RegionCode,

//                        })
//                        .ToList();

//                return Json(new
//                {
//                    Draw = dtParameters.Draw,
//                    RecordsTotal = totalRecords,
//                    RecordsFiltered = filteredRecords,
//                    Data = clusters
//                });

//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { error = "An error occurred while processing your request." });
//            }
//        }



//        [HttpPost]
//        public async Task<IActionResult> Create([FromBody] Cluster cluster)
//        {
//            try
//            {
//                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

//                // Add validation as needed
//                if (_context.Clusters.Any(m => m.ClusterCode == cluster.ClusterCode))
//                {
//                    return Json(new { success = false, message = "Cluster with this Cluster Code already exists" });
//                }

//                _context.Clusters.Add(cluster);
//                await _context.SaveChangesAsync(currentUser.Id);

//                return Json(new { success = true, message = "Cluster created successfully" });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, message = ex.Message });
//            }
//        }

//        [HttpPost]
//        public async Task<ActionResult> UpdateCluster([FromForm] ClusterUpdateDTO dto)
//        {
//            try
//            {
//                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
//                var cluster = await _context.Clusters.FirstOrDefaultAsync(m => m.ClusterCode == dto.ClusterCode);

//                if (cluster == null)
//                {
//                    return Json(new { success = false, message = "Cluster not found" });
//                }

//                // Update properties from DTO
         
//                cluster.DistrictCode = dto.DistrictCode;
//                cluster.DistrictName = dto.DistrictName;
//                cluster.ClusterCode = dto.ClusterCode;
//                cluster.ClusterName = dto.ClusterName;
    
//                await _context.SaveChangesAsync(currentUser.Id);

//                return Json(new { success = true, message = "Cluster updated successfully" });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, message = "Error updating cluster: " + ex.Message });
//            }
//        }




//        [HttpPost]

//        public async Task<IActionResult> DeleteCluster(string clusterCode)
//        {
//            try
//            {
//                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
//                var cluster = await _context.Clusters.FirstOrDefaultAsync(m => m.ClusterCode == clusterCode);
//                if (cluster == null)
//                {
//                    return Json(new { success = false, message = "Cluster not found" });
//                }

//                _context.Clusters.Remove(cluster);
//                _context.SaveChangesAsync(currentUser.Id);

//                return Json(new { success = true, message = "Deleted successfully" });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, message = ex.Message });
//            }
//        }

//        public async Task<IActionResult> Details(string clusterCode)
//        {
//            var cluster = await _context.Clusters
//                .FirstOrDefaultAsync(m => m.ClusterCode == clusterCode);

//            if (cluster == null)
//            {
//                return NotFound();
//            }

//            return View(cluster);
//        }
//    }

//    public class ClusterUpdateDTO
//    {
       
//        public string RegionCode { get; set; }
//        public string DistrictCode { get; set; }
//        public string DistrictName { get; set; }

//        public string ClusterCode { get; set; }
//        public string ClusterName { get; set; }


//    }
//}