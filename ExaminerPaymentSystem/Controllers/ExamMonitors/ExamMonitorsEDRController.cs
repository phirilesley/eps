using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.ViewModels.ExamMonitors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class ExamMonitorsEDRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<ExamMonitorsEDRController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamMonitorsEDRController(ApplicationDbContext context,
                                   IAuthorizationService authorizationService,
                                   ILogger<ExamMonitorsEDRController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _authorizationService = authorizationService;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: Supervisor dashboard to add extra days
        [Authorize(Policy = "SupervisorPolicy")]
        public async Task<IActionResult> SupervisorAddDays()
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            if (userRoles.Contains("RegionalManager"))
            {

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
            var model = new SupervisorAddDaysViewModel();
            return View(model);
        }

        // POST: Process phase/session selection and show people
        [HttpPost]
        [Authorize(Policy = "SupervisorPolicy")]
        public async Task<IActionResult> SupervisorSelectPhaseSession(SupervisorAddDaysViewModel model)
        {
       
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

                var userRoles = await _userManager.GetRolesAsync(currentUser);

                if (userRoles.Contains("RegionalManager"))
                {
                    model.Region = currentUser.Region;
                }

                    // Get people from the selected phase and session
                    var availablePeople = await _context.ExamMonitorTransactions
                    .Include(a => a.ExamMonitor)
                    .Where(r => r.Phase == model.Phase && r.Session == model.Session && r.Region == model.Region)
                    .Select(r => new PersonListItem
                    {
                      Id = r.NationalId,
                        Name = r.ExamMonitor.FirstName +" "+ r.ExamMonitor.LastName,
                        SubKey = r.SubKey
                    })
                    .ToListAsync();

                model.AvailablePeople = availablePeople;
                return View("SupervisorAddDays", model);
            

        }

        // GET: Show date entry form for specific person
        [Authorize(Policy = "SupervisorPolicy")]
        public async Task<IActionResult> SupervisorSelectPerson(string personId,string subKey ,string phase, string session)
        {
            var person = await _context.ExamMonitorTransactions
                .Include(a => a.ExamMonitor)
                .FirstOrDefaultAsync(r => r.NationalId == personId && r.SubKey == subKey);

            if (person == null)
            {
                return NotFound();
            }

            var model = new SupervisorAddDaysViewModel
            {
                Phase = phase,
                Session = session,
                SelectedPersonId = personId,
                SelectedPersonName = person.ExamMonitor.FirstName+ " "+ person.ExamMonitor.LastName,
                SelectedPersonSubKey = person.SubKey,
                Dates = new List<SupervisorDateModel> { new SupervisorDateModel() } // Start with one empty date
            };

            return View("SupervisorDateEntry", model);
        }

        // POST: Add supervisor extra days
        [HttpPost]
        [Authorize(Policy = "SupervisorPolicy")]
        public async Task<IActionResult> AddSupervisorDays(SupervisorAddDaysViewModel model)
        {
            if (model.Dates == null || !model.Dates.Any(d => d.Date != default))
            {
                TempData["ErrorMessage"] = "Please provide at least one valid date";
                return RedirectToAction("SupervisorSelectPerson", new
                {
                    personId = model.SelectedPersonId,
                    phase = model.Phase,
                    session = model.Session
                });
            }

            // Get the current user (supervisor)
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

            // Get or create the register
            var register = await _context.ExamMonitorsRegisters
                .FirstOrDefaultAsync(a => a.SubKey == model.SelectedPersonSubKey);

            if (register == null)
            {
                // Create a new register if it doesn't exist
                register = new ExamMonitorRegister
                {
                    SubKey = model.SelectedPersonSubKey,
                    NationalId = model.SelectedPersonId,
                    Date = DateTime.Now.ToString("yyyy-MM-dd"),
                    Comment = "Created automatically by supervisor",
                    CompiledStatus = "Pending",
                    ClusterManagerStatus = "Pending",
                    RegionalManagerStatus = "Pending",
                    IsPresent = true,
                    IsPresentBy = currentUser.UserName,
                    IsPresentDate = DateTime.Now.ToString("yyyy-MM-dd")
                };

                _context.ExamMonitorsRegisters.Add(register);
                await _context.SaveChangesAsync(); // Save to get the ID
            }

            foreach (var dateModel in model.Dates)
            {
                if (dateModel.Date == default) continue; // Skip empty dates

                // Check if date already exists for this person
                var existingDate = await _context.ExamMonitorRegisterDates
                    .FirstOrDefaultAsync(d => d.SubKey == model.SelectedPersonSubKey && d.Date == dateModel.Date);

                if (existingDate != null)
                {
                    // Update existing date if it was from timetable
                    if (existingDate.IsFromTimetable)
                    {
                        existingDate.SupervisorStatus = dateModel.Status;
                        existingDate.SupervisorComment = dateModel.Comment;
                        existingDate.SupervisorBy = currentUser.UserName;
                        existingDate.SupervisorDate = DateTime.Now;
                        existingDate.IsSupervisorAdded = true;

                        _context.ExamMonitorRegisterDates.Update(existingDate);
                    }
                    // If it was already a supervisor-added day, update it
                    else
                    {
                        existingDate.SupervisorStatus = dateModel.Status;
                        existingDate.SupervisorComment = dateModel.Comment;
                        existingDate.SupervisorBy = currentUser.UserName;
                        existingDate.SupervisorDate = DateTime.Now;

                        _context.ExamMonitorRegisterDates.Update(existingDate);
                    }
                }
                else
                {
                    // Create new supervisor-added date
                    var newDate = new ExamMonitorRegisterDate
                    {
                        SubKey = model.SelectedPersonSubKey,
                        Register = register, // Set the foreign key
                        Date = dateModel.Date,
                        Comment = dateModel.Comment,
                        IsPresent = true,
                        IsFromTimetable = false, // This is a supervisor-added day
                        IsSupervisorAdded = true,
                        SupervisorStatus = dateModel.Status,
                        SupervisorComment = dateModel.Comment,
                        SupervisorBy = currentUser.UserName,
                        SupervisorDate = DateTime.Now,
                        // Set other approval properties to defaults
                        CompiledStatus = "Pending",
                        ClusterManagerStatus = "Pending",
                        RegionalManagerStatus = "Pending"
                    };

                    _context.ExamMonitorRegisterDates.Add(newDate);
                }
            }

            try
            {
                await _context.SaveChangesAsync();

                _logger.LogInformation("Supervisor {Supervisor} added days for person {PersonId}",
                    currentUser.UserName, model.SelectedPersonId);

                TempData["SuccessMessage"] = $"Successfully added/updated {model.Dates.Count(d => d.Date != default)} days";
                return RedirectToAction("SupervisorViewRegister", new { subKey = model.SelectedPersonSubKey });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving supervisor days");
                TempData["ErrorMessage"] = "Error saving days. Please try again.";
                return RedirectToAction("SupervisorSelectPerson", new
                {
                    personId = model.SelectedPersonId,
                    phase = model.Phase,
                    session = model.Session,
                    subKey = model.SelectedPersonSubKey
                });
            }
        }

        // View person's register with distinction between timetable and supervisor days
        [Authorize(Policy = "SupervisorPolicy")]
        public async Task<IActionResult> SupervisorViewRegister(string subKey)
        {
            var register = await _context.ExamMonitorsRegisters
                .Include(a => a.ExamMonitor)
                .FirstOrDefaultAsync(r => r.SubKey == subKey);

            if (register == null)
            {
                return NotFound();
            }

            var allDates = await _context.ExamMonitorRegisterDates
                .Where(d => d.SubKey == subKey)
                .OrderBy(d => d.Date)
                .ToListAsync();

            var model = new SupervisorViewRegisterViewModel
            {
                MonitorName = register.ExamMonitor.FirstName+ " "+register.ExamMonitor.LastName,
                SubKey = subKey,
                TimetableDays = allDates.Where(d => d.IsFromTimetable).ToList(),
                SupervisorAddedDays = allDates.Where(d => !d.IsFromTimetable).ToList()
            };

            return View(model);
        }

        // Remove a supervisor-added day
        [Authorize(Policy = "SupervisorPolicy")]
        public async Task<IActionResult> RemoveSupervisorDay(int dateId)
        {
            var dateRecord = await _context.ExamMonitorRegisterDates.FindAsync(dateId);
            if (dateRecord == null)
            {
                return NotFound();
            }

            // Only allow removal of supervisor-added days (not timetable days)
            if (!dateRecord.IsFromTimetable)
            {
                _context.ExamMonitorRegisterDates.Remove(dateRecord);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Supervisor-added day removed successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Cannot remove timetable days. You can only update supervisor status for these days.";
            }

            return RedirectToAction("SupervisorViewRegister", new { subKey = dateRecord.SubKey });
        }
    }
}
