using AutoMapper;
using ClosedXML.Excel;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Services.ExamMonitors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class DistrictsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DistrictsController(IMapper mapper, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {

            _mapper = mapper;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> BulkAddDistricts()
        {
            return View();
        }

        public async Task<IActionResult> DownloadDistrictsTemplate()
        {


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Centres");

              
                worksheet.Cell(1, 3).Value = "DISTRICT CODE";
                worksheet.Cell(1, 4).Value = "DISTRICT NAME";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "DistrictsTemplate.xlsx");
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> UploadDistricts(IFormFile file, string regionCode = "")
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return RedirectToAction("BulkAddDistricts");
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

                    var districts = new List<District>();
                    var districtsInFile = new HashSet<string>();
                    var duplicateDistricts = new HashSet<string>();
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
                                var districtCode = row.Cell(1).GetValue<string>()?.Trim();

                                if (string.IsNullOrWhiteSpace(districtCode))
                                {
                                    continue;
                                }

                                if (districtsInFile.Contains(districtCode))
                                {
                                    duplicateDistricts.Add(districtCode);
                                    continue;
                                }

                                districtsInFile.Add(districtCode);
                                processedCount++;



                                var district = new District
                                {

                                    DistrictCode = districtCode,
                                   
                                    DistrictName = row.Cell(2).GetValue<string>()?.Trim(),
                          


                                };
                                district.RegionCode = regionCode;
                                districts.Add(district);
                            }
                        }
                    }
                    //var existingRecords = new List<ExamMonitor>();
                    // Final duplicate check before insert
                    // Assuming examinerMonitors is a list of input objects
                    var districsCodes = districts
                        .Where(x => x.DistrictCode != null)
                        .Select(x => x.DistrictCode!)
                        .Distinct()
                        .ToList();

                    var existingRecords = await _context.Districts
                        .AsNoTracking()
                        .Where(e => e.DistrictCode != null && districsCodes.Contains(e.DistrictCode))
                        .ToListAsync();

                    var districtsNotInDB = districts
                        .Where(e => !existingRecords.Any(x =>
                            x.DistrictCode == e.DistrictCode))
                        .ToList();

                    if (districtsNotInDB.Any())
                    {
                        _context.ChangeTracker.Clear();
                        await _context.Districts.AddRangeAsync(districtsNotInDB);
                        await _context.SaveChangesAsync(currentUser.Id);
                    }

                    await transaction.CommitAsync();

                    var message = $"{processedCount} district(s) processed, {districtsNotInDB.Count} new district(s) added.";
                    if (duplicateDistricts.Any())
                    {
                        message += $" {duplicateDistricts.Count} duplicate(s) skipped in file.";
                    }
                    if (districts.Count - districtsNotInDB.Count > 0)
                    {
                        message += $" {districts.Count - districtsNotInDB.Count} existing district(s) skipped.";
                    }

                    TempData["Success"] = message;
                    return RedirectToAction("BulkAddDistricts");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "An error occurred: " + ex.Message;
                    return RedirectToAction("BulkAddDistricts");
                }
            });
        }


        [HttpGet]
        public async Task<JsonResult> GetDistrict(string districtCode)
        {
            try
            {
                var district = await _context.Districts.FirstOrDefaultAsync
                    (m => m.DistrictCode == districtCode);

                if (district == null)
                {
                    return Json(new { success = false, message = "District not found" });
                }

                return Json(new
                {
                    success = true,
                  
                    DistrictCode = district.DistrictCode,
                    DistrictName = district.DistrictName,
                    RegionCode = district.RegionCode
                   

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

                ViewBag.RegionCode = currentUser.Region;
            }
            return View();

        }


        [HttpPost]
        public async Task<IActionResult> GetDistricts([FromBody] DtParameters dtParameters)
        {
            try
            {

                // Initialize with default values if null
                dtParameters ??= new DtParameters();

                var searchValue = dtParameters.Search?.Value ?? string.Empty;


            

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);

                // Base query
                IQueryable<District> query = _context.Districts.AsQueryable();

                if (userRoles.Contains("RegionalManager"))
                {
                    // For Regional Managers - enforce region filter from their profile
                    if (string.IsNullOrEmpty(currentUser.Region))
                    {
                        // Return empty result if region is missing for Regional Manager
                        return Json(new { data = new List<District>() });
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


                // Apply global search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.DistrictCode.Contains(searchValue) ||
                        m.DistrictName.Contains(searchValue));
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
                        "districtName" => isAscending ? query.OrderBy(m => m.DistrictName) : query.OrderByDescending(m => m.DistrictName),
                        "districtCode" => isAscending ? query.OrderBy(m => m.DistrictCode) : query.OrderByDescending(m => m.DistrictCode),
                        _ => query.OrderBy(m => m.DistrictCode)
                    };
                }
                else
                {
                    query = query.OrderBy(m => m.DistrictCode);
                }

                // Apply paging
                var filteredRecords = await query.CountAsync();
                var start = Math.Max(0, dtParameters.Start);
                var length = dtParameters.Length > 0 ? dtParameters.Length : 10;
                var districts = query
                        .Select(m => new
                        {
                         
                            DistrictCode = m.DistrictCode,
                            DistrictName = m.DistrictName,
                            RegionCode = m.RegionCode,

                        })
                        .ToList();

                return Json(new
                {
                    Draw = dtParameters.Draw,
                    RecordsTotal = totalRecords,
                    RecordsFiltered = filteredRecords,
                    Data = districts
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] District district)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                // Add validation as needed
                if (_context.Districts.Any(m => m.DistrictCode == district.DistrictCode))
                {
                    return Json(new { success = false, message = "District with this district Code already exists" });
                }

                _context.Districts.Add(district);
                await _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "District created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateDistrict([FromForm] DistrictUpdateDTO dto)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var district = await _context.Districts.FirstOrDefaultAsync(m => m.DistrictCode == dto.DistrictCode);

                if (district == null)
                {
                    return Json(new { success = false, message = "District not found" });
                }

                // Update properties from DTO
                
           
                district.DistrictName = dto.DistrictName;
                district.RegionCode = dto.RegionCode;
   
                await _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "District updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating district: " + ex.Message });
            }
        }




        [HttpPost]

        public async Task<IActionResult> DeleteDistrict(string districtCode)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var district = await _context.Districts.FirstOrDefaultAsync(m => m.DistrictCode == districtCode);
                if (district == null)
                {
                    return Json(new { success = false, message = "District not found" });
                }

                _context.Districts.Remove(district);
             await   _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Details(string districtCode)
        {
            var district = await _context.Districts
                .FirstOrDefaultAsync(m => m.DistrictCode == districtCode);

            if (district == null)
            {
                return NotFound();
            }

            return View(district);
        }
    }

    public class DistrictUpdateDTO
    {

        public string RegionCode { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }




    }

}
