using AutoMapper;
using ClosedXML.Excel;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Services.ExamMonitors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.Threading;

namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class CentresController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public CentresController(IMapper mapper, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
        
            _mapper = mapper;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> BulkAddCentres()
        {
            return View();
        }

        public async Task<IActionResult> DownloadCentresTemplate()
        {
       

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Centres");

                // Header row
                worksheet.Cell(1, 1).Value = "CENTRE NO";
                worksheet.Cell(1, 2).Value = "CENTRE NAME";
                worksheet.Cell(1, 3).Value = "DISTRICT CODE";
                worksheet.Cell(1, 4).Value = "DISTRICT NAME";
                worksheet.Cell(1, 5).Value = "CLUSTER CODE";
                worksheet.Cell(1, 6).Value = "CLISTER NAME";
                worksheet.Cell(1, 7).Value = "SCHOOL TYPE";

                

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "CentresTemplate.xlsx");
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> UploadCentres(IFormFile file, string regionCode = "")
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return RedirectToAction("BulkAddCentres");
            }

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            // Create an execution strategy
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    _context.ChangeTracker.Clear();

                    var centres = new List<Centre>();
                    var centreNumbersInFile = new HashSet<string>();
                    var duplicateCentres = new HashSet<string>();
                    var processedCount = 0;



                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        using (var workbook = new XLWorkbook(stream))
                        {
                            var worksheet = workbook.Worksheet(1);
                            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                            foreach (var row in rows)
                            {
                                var centreNumber = row.Cell(1).GetValue<string>()?.Trim();

                                if (string.IsNullOrWhiteSpace(centreNumber))
                                {
                                    continue;
                                }

                                if (centreNumbersInFile.Contains(centreNumber))
                                {
                                    duplicateCentres.Add(centreNumber);
                                    continue;
                                }

                                centreNumbersInFile.Add(centreNumber);
                                processedCount++;



                                var centre = new Centre
                                {
                                   
                                    CentreNumber = centreNumber,
                                    CentreName = row.Cell(2).GetValue<string>()?.Trim(),
                                    DistrictCode = row.Cell(3).GetValue<string>()?.Trim(),
                                    DistrictName = row.Cell(4).GetValue<string>()?.Trim(),
                                    ClusterCode = row.Cell(5).GetValue<string>()?.Trim(),
                                    ClusterName = row.Cell(6).GetValue<string>()?.Trim(),
                                    SchoolType = row.Cell(7).GetValue<string>()?.Trim(),
                                    
                                 
                                };

                                centre.RegionCode = regionCode;
                                centre.IsResident = "NotResident";
                                centre.IsCluster = "NotResident";
                                centre.SchoolType = "G";
                                centre.DistrictId = 1; 
                                centres.Add(centre);
                            }
                        }
                    }
                    //var existingRecords = new List<ExamMonitor>();
                    // Final duplicate check before insert
                    // Assuming examinerMonitors is a list of input objects
                    var centerNumbers = centres
                        .Where(x => x.CentreNumber != null)
                        .Select(x => x.CentreNumber!)
                        .Distinct()
                        .ToList();

                    var existingRecords = await _context.Centres
                        .AsNoTracking()
                        .Where(e => e.CentreNumber != null && centerNumbers.Contains(e.CentreNumber))
                        .ToListAsync();

                    var centresNotInDB = centres
                        .Where(e => !existingRecords.Any(x =>
                            x.CentreNumber == e.CentreNumber))
                        .ToList();

                    if (centresNotInDB.Any())
                    {
                        _context.ChangeTracker.Clear();
                        await _context.Centres.AddRangeAsync(centresNotInDB);
                        await _context.SaveChangesAsync(currentUser.Id);
                    }

                    await transaction.CommitAsync();

                    var message = $"{processedCount} centre(s) processed, {centresNotInDB.Count} new centre(s) added.";
                    if (duplicateCentres.Any())
                    {
                        message += $" {duplicateCentres.Count} duplicate(s) skipped in file.";
                    }
                    if (centres.Count - centresNotInDB.Count > 0)
                    {
                        message += $" {centres.Count - centresNotInDB.Count} existing centre(s) skipped.";
                    }

                    TempData["Success"] = message;
                    return RedirectToAction("BulkAddCentres");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "An error occurred: " + ex.Message;
                    return RedirectToAction("BulkAddCentres");
                }
            });
        }


        [HttpGet]
        public JsonResult GetCentre(string centreNumber)
        {
            try
            {
                var centre = _context.Centres
                    .FirstOrDefault(m => m.CentreNumber == centreNumber);

                if (centre == null)
                {
                    return Json(new { success = false, message = "Centre not found" });
                }

                return Json(new
                {
                    success = true,
                    CentreNumber = centre.CentreNumber,
                    CentreName = centre.CentreName,
                    DistrictCode = centre.DistrictCode,
                    DistrictName = centre.DistrictName,
                    ClusterCode = centre.ClusterCode,
                    ClusterName = centre.ClusterName,
                    SchoolType = centre.SchoolType,
                    RegionCode = centre.RegionCode,
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
                ViewBag.RegionCode = currentUser.Region;
            }
            else if (userRoles.Contains("Admin"))
            {
              

                ViewBag.Clusters = _context.Centres
                    .Where(a => a.IsCluster == "IsCluster")
           .Select(m => new {
               Value = m.ClusterCode,
               Text = m.ClusterName
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

            }

            return View();
         

        }

     
        [HttpPost]
        public async Task<IActionResult> GetCentres([FromBody] DtParameters dtParameters)
        {
            try
            {

                // Initialize with default values if null
                dtParameters ??= new DtParameters();

                var searchValue = dtParameters.Search?.Value ?? string.Empty;

               
                var clusterFilter = dtParameters.ClusterFilter ?? string.Empty;
                var districtFilter = dtParameters.DistrictFilter ?? string.Empty;

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);

                // Base query
                IQueryable<Centre> query = _context.Centres.AsQueryable();

                if (userRoles.Contains("RegionalManager"))
                {
                    // For Regional Managers - enforce region filter from their profile
                    if (string.IsNullOrEmpty(currentUser.Region))
                    {
                        // Return empty result if region is missing for Regional Manager
                        return Json(new { data = new List<Centre>() });
                    }
                    query = query.Where(x => x.RegionCode == currentUser.Region);
                }
                else if (userRoles.Contains("Admin") || userRoles.Contains("HR"))
                {
                    var regionFilter = dtParameters.RegionFilter ?? string.Empty;
                    // For Admins - apply region filter only if specified
                    if (!string.IsNullOrEmpty(regionFilter))
                    {
                        query = query.Where(x => x.RegionCode == regionFilter);
                    }
                    // If no region filter specified, Admin sees all records
                }




                if (!string.IsNullOrEmpty(clusterFilter))
                {
                    query = query.Where(m => m.ClusterCode == clusterFilter);
                }

                if (!string.IsNullOrEmpty(districtFilter))
                {
                    query = query.Where(m => m.DistrictCode == districtFilter);
                }

                // Apply global search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.CentreNumber.Contains(searchValue) ||
                        m.CentreName.Contains(searchValue) ||
                        m.DistrictName.Contains(searchValue) ||
                        m.ClusterName.Contains(searchValue));
                }

                // Get total count before paging
                var totalRecords = await query.CountAsync();

                // Apply sorting
                if (dtParameters.Order != null && dtParameters.Order.Any())
                {
                    var order = dtParameters.Order.First();
                    var columnIndex = order.Column;
                    var columnName = dtParameters.Columns[columnIndex].Data;
                    var isAscending = order.Dir == DtOrderDir.Asc;

                    query = columnName switch
                    {
                        "centreName" => isAscending ? query.OrderBy(m => m.CentreName) : query.OrderByDescending(m => m.CentreName),
                        "centreNumber" => isAscending ? query.OrderBy(m => m.CentreNumber) : query.OrderByDescending(m => m.CentreNumber),
                        _ => query.OrderBy(m => m.CentreNumber)
                    };
                }
                else
                {
                    query = query.OrderBy(m => m.CentreNumber);
                }

                // Apply paging
                var filteredRecords = await query.CountAsync();
                    var start = Math.Max(0, dtParameters.Start);
                    var length = dtParameters.Length > 0 ? dtParameters.Length : 10;
                    var centres = query
                            .Select(m => new
                            {
                                CentreNumber = m.CentreNumber,

                                CentreName = m.CentreName,
                                ClusterName = m.ClusterName,
                                ClusterCode = m.ClusterCode,
                                DistrictCode = m.DistrictCode,
                                DistrictName = m.DistrictName,
                                RegionCode = m.RegionCode,
                                IsResident = m.IsResident,
                                IsCluster = m.IsCluster,
                            })
                            .ToList();

                    return Json(new
                    {
                        Draw = dtParameters.Draw,
                        RecordsTotal = totalRecords,
                        RecordsFiltered = filteredRecords,
                        Data = centres
                    });
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateResidentStatus([FromBody] ResidentUpdateModel model)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request data" });
            }

            if (string.IsNullOrEmpty(model.CentreNumber))
            {
                return Json(new { success = false, message = "Centre number is required" });
            }

            try
            {
                // Get the centre from database
                var centre = await _context.Centres
                    .FirstOrDefaultAsync(c => c.CentreNumber == model.CentreNumber);

                if (centre == null)
                {
                    return Json(new { success = false, message = "Centre not found" });
                }

                // Update the resident status
                centre.IsResident = model.IsResident == "IsResident" ? "IsResident" : "NotResident";
                _context.Centres.Update(centre);

                // Save changes
                int affectedRows = await _context.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No changes were made to the resident status"
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Resident status updated successfully",
                    isResident = centre.IsResident // Return the updated value
                });
            }
            catch (Exception ex)
            {
            
                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating resident status"
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClusterStatus([FromBody] ResidentUpdateModel model)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request data" });
            }

            if (string.IsNullOrEmpty(model.CentreNumber))
            {
                return Json(new { success = false, message = "Centre number is required" });
            }

            try
            {
                // Get the centre from database
                var centre = await _context.Centres
                    .FirstOrDefaultAsync(c => c.CentreNumber == model.CentreNumber);

                if (centre == null)
                {
                    return Json(new { success = false, message = "Centre not found" });
                }

                // Update the resident status
                centre.IsCluster = model.IsResident == "IsCluster" ? "IsCluster" : "NotResident";
                _context.Centres.Update(centre);

                // Save changes
                int affectedRows = await _context.SaveChangesAsync();

                if (affectedRows == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No changes were made to the resident status"
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Cluster status updated successfully",
                    isResident = centre.IsCluster // Return the updated value
                });
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating resident status"
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Centre centre)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                // Add validation as needed
                if (_context.Centres.Any(m => m.CentreNumber == centre.CentreNumber))
                {
                    return Json(new { success = false, message = "Centre with this Centre Number already exists" });
                }
           
                centre.IsResident = "NotResident";
                centre.IsCluster = "NotResident";
             
                centre.DistrictId = 1;
                _context.Centres.Add(centre);
              await  _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "Centre created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
       
        [HttpPost]
        public async Task<ActionResult> UpdateCentre([FromForm] CentreUpdateDTO dto)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var centre = await _context.Centres.FirstOrDefaultAsync(m => m.CentreNumber == dto.CentreNumber);

                if (centre == null)
                {
                    return Json(new { success = false, message = "Centre not found" });
                }

                var district = await _context.Districts.FirstOrDefaultAsync(a => a.DistrictCode == dto.DistrictCode);
                var clusterCentres = await _context.Centres
     .Where(a => a.ClusterCode == dto.ClusterCode)
     .ToListAsync();


                var clusterName = clusterCentres.FirstOrDefault()?.ClusterName;

                // Update properties from DTO
              
                centre.CentreName = dto.CentreName;
                centre.DistrictCode = dto.DistrictCode;
                centre.DistrictName = district.DistrictName ?? dto.DistrictCode;
                centre.ClusterCode  = dto.ClusterCode;
                centre.ClusterName = clusterName;
                centre.SchoolType = dto.SchoolType;
                centre.RegionCode = dto.RegionCode;



             await   _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "Centre updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating centre: " + ex.Message });
            }
        }




        [HttpPost]

        public async Task<IActionResult> DeleteCentre(string centreNumber)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var centre = await _context.Centres.FirstOrDefaultAsync(m => m.CentreNumber == centreNumber);
                if (centre == null)
                {
                    return Json(new { success = false, message = "Centre not found" });
                }

                _context.Centres.Remove(centre);
              await  _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
      
        public async Task<IActionResult> Details(string centreNumber)
        {
            var centre = await _context.Centres
                .FirstOrDefaultAsync(m => m.CentreNumber == centreNumber);

            if (centre == null)
            {
                return NotFound();
            }

            return View(centre);
        }
    }


    public class ResidentUpdateModel
    {
        public string CentreNumber { get; set; }
        public string IsResident { get; set; } // Keep as string

    }
    public class CentreUpdateDTO
    {
        public string CentreNumber { get; set; }
        public string CentreName { get; set; }
        public string RegionCode { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }

        public string ClusterCode { get; set; }
        public string ClusterName { get; set; }

        public string SchoolType { get; set; }

    }
    }
