using AutoMapper;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.ExamMonitors;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class PhasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public PhasesController(IMapper mapper, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {

            _mapper = mapper;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
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
        public async Task<IActionResult> GetPhases([FromBody] DtParameters dtParameters)
        {
            try
            {

                // Initialize with default values if null
                dtParameters ??= new DtParameters();

                var searchValue = dtParameters.Search?.Value ?? string.Empty;




                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);

                // Base query
                IQueryable<Phase> query = _context.Phases.AsQueryable();


                // Apply global search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.PhaseName.Contains(searchValue) ||
                        m.PhaseCode.Contains(searchValue) ||
                        m.PhaseYear.Contains(searchValue));
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
                        "PhaseName" => isAscending ? query.OrderBy(m => m.PhaseName) : query.OrderByDescending(m => m.PhaseName),
                        "PhaseCode" => isAscending ? query.OrderBy(m => m.PhaseCode) : query.OrderByDescending(m => m.PhaseCode),
                        "PhaseYear" => isAscending ? query.OrderBy(m => m.PhaseYear) : query.OrderByDescending(m => m.PhaseYear),
                        _ => query.OrderBy(m => m.PhaseCode)
                    };
                }
                else
                {
                    query = query.OrderBy(m => m.PhaseYear);
                }

                // Apply paging
                var filteredRecords = await query.CountAsync();
                var start = Math.Max(0, dtParameters.Start);
                var length = dtParameters.Length > 0 ? dtParameters.Length : 10;
                var centres = query
             
                        .Select(m => new
                        {
                            PhaseName = m.PhaseName,

                            PhaseCode = m.PhaseCode,
                            PhaseYear = m.PhaseYear,
                            StartTime = m.StartTime.ToShortDateString(),
                            EndTime = m.EndTime.ToShortDateString(),
                           ClusterManagerRate = m.ClusterManagerRate,
                           AssistantClusterManagerRate = m.AssistantClusterManagerRate,
                           ResidentMonitorRate = m.ResidentMonitorRate,
                          LunchRate = m.LunchRate,
                          BreakFastRate = m.BreakFastRate,
                          AccomodationRate = m.AccomodationRate,
                          DinnerRate = m.DinnerRate,
                          Status =m.Status,
                          SessionCode = m.SessionCode,
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
        public async Task<IActionResult> Create([FromBody] Phase phase)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                // Add validation as needed
                if (_context.Phases.Any(m => m.PhaseCode == phase.PhaseCode))
                {
                    return Json(new { success = false, message = "Phase with this Phase Code already exists" });
                }

                if (_context.Phases.Any(m => m.PhaseName == phase.PhaseName))
                {
                    return Json(new { success = false, message = "Phase with this Phase Name already exists" });
                }

                _context.Phases.Add(phase);
                await _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "Centre created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdatePhase([FromForm] PhaseUpdateDTO dto)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var phase = await _context.Phases.FirstOrDefaultAsync(m => m.PhaseCode == dto.PhaseCode);

                if (phase == null)
                {
                    return Json(new { success = false, message = "Phase not found" });
                }


                phase.StartTime = dto.StartTime;
                phase.EndTime = dto.EndTime;
                phase.PhaseCode = dto.PhaseCode;
                phase.PhaseName = dto.PhaseName;
                phase.SessionCode = dto.SessionCode;
                phase.PhaseYear = dto.PhaseYear;


                _context.Update(phase);
                await _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "Phase updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating phase : " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<ActionResult> UpdateStipendRate([FromForm] PhaseStipendUpdateDTO dto)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var phase = await _context.Phases.FirstOrDefaultAsync(m => m.PhaseCode == dto.PhaseCode);

                if (phase == null)
                {
                    return Json(new { success = false, message = "Phase not found" });
                }

               phase.AssistantClusterManagerRate = dto.AssistantClusterManagerRate;
                phase.ResidentMonitorRate = dto.ResidentMonitorRate;
                phase.ClusterManagerRate = dto.ClusterManagerRate;

                _context.Update(phase);
                await _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "Phase updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating phase : " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdatePhaseDailyRates([FromForm] PhaseDailyUpdateDTO dto)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var phase = await _context.Phases.FirstOrDefaultAsync(m => m.PhaseCode == dto.PhaseCode);

                if (phase == null)
                {
                    return Json(new { success = false, message = "Phase not found" });
                }

                phase.DinnerRate = dto.DinnerRate;
                phase.BreakFastRate = dto.BreakFastRate;
                phase.LunchRate = dto.LunchRate;
                phase.AccomodationRate = dto.AccomodationRate;

                _context.Update(phase);
                await _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "Phase updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating phase : " + ex.Message });
            }
        }

        

        [HttpGet]
        public JsonResult GetPhase(string phaseCode)
        {
            try
            {
                var centre = _context.Phases
                    .FirstOrDefault(m => m.PhaseCode == phaseCode);

                if (centre == null)
                {
                    return Json(new { success = false, message = "Phase not found" });
                }

                return Json(new
                {
                    success = true,
                    PhaseCode = centre.PhaseCode,
                    PhaseName = centre.PhaseName,
                    PhaseYear = centre.PhaseYear,
                    SessionCode = centre.SessionCode,
                    EndTime = centre.EndTime,
                    StartTime = centre.StartTime,
     

                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        public JsonResult GetPhaseStipend(string phaseCode)
        {
            try
            {
                var centre = _context.Phases
                    .FirstOrDefault(m => m.PhaseCode == phaseCode);

                if (centre == null)
                {
                    return Json(new { success = false, message = "Phase not found" });
                }

                return Json(new
                {
                    success = true,
                    PhaseCode = centre.PhaseCode,

                    AssistantClusterManagerRate = centre.AssistantClusterManagerRate,
                    ClusterManagerRate = centre.ClusterManagerRate,
                    ResidentMonitorRate = centre.ResidentMonitorRate,

                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetPhaseDailyRates(string phaseCode)
        {
            try
            {
                var centre = _context.Phases
                    .FirstOrDefault(m => m.PhaseCode == phaseCode);

                if (centre == null)
                {
                    return Json(new { success = false, message = "Phase not found" });
                }

                return Json(new
                {
                    success = true,
                    PhaseCode = centre.PhaseCode,
                
                    LunchRate = centre.LunchRate,
                    DinnerRate = centre.DinnerRate,
                    BreakFastRate = centre.BreakFastRate,
                    AccomodationRate = centre.AccomodationRate,
                  

                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
 
        public async Task<IActionResult> UpdatePhaseStatus([FromBody] PhaseStatusUpdateDTO model)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request data" });
            }

            if (string.IsNullOrEmpty(model.PhaseCode))
            {
                return Json(new { success = false, message = "Phase code is required" });
            }

            try
            {
                // Get the centre from database
                var phase = await _context.Phases
                    .FirstOrDefaultAsync(c => c.PhaseCode == model.PhaseCode);

                if (phase == null)
                {
                    return Json(new { success = false, message = "Phase not found" });
                }

                // Update the resident status
                phase.Status = model.Status == "Active" ? "Active" : "Inactive";
                _context.Phases.Update(phase);

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
                    status =phase.Status // Return the updated value
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

        public async Task<IActionResult> DeletePhase(string phaseCode)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var phase = await _context.Phases.FirstOrDefaultAsync(m => m.PhaseCode == phaseCode);
                if (phase == null)
                {
                    return Json(new { success = false, message = "Phase not found" });
                }

                _context.Phases.Remove(phase);
              await  _context.SaveChangesAsync(currentUser.Id);

                return Json(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }

    public class PhaseUpdateDTO
    {
        public string PhaseCode { get; set; }
        public string PhaseName { get; set; }
        public string PhaseYear { get; set; }
        public string SessionCode { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    

    }

    public class PhaseStipendUpdateDTO
    {
        public string PhaseCode { get; set; }
      
        public decimal ClusterManagerRate { get; set; }
        public decimal AssistantClusterManagerRate { get; set; }

        public decimal ResidentMonitorRate { get; set; }

    }

    public class PhaseDailyUpdateDTO
    {
        public string PhaseCode { get; set; }

        public decimal ResidentMonitorRate { get; set; }
        public decimal DinnerRate { get; set; }
        public decimal AccomodationRate { get; set; }
        public decimal LunchRate { get; set; }
        public decimal BreakFastRate { get; set; }

    }

    public class PhaseStatusUpdateDTO
    {
        public string PhaseCode { get; set; }

       public string Status { get; set; }

    }
}
