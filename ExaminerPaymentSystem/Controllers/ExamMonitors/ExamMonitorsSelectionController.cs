using AutoMapper;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Services.ExamMonitors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class ExamMonitorsSelectionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExamMonitorService _service;
        private readonly IMapper _mapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamMonitorsSelectionController(IExamMonitorService service, IMapper mapper, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _mapper = mapper;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }



        // In your ExamMonitorsController.cs
        [HttpGet]
        public JsonResult GetExaminerTransaction(string subKey)
        {
            try
            {
                var monitorTransaction = _context.ExamMonitorTransactions
                    .Include(a => a.ExamMonitor)
                    .FirstOrDefault(m => m.SubKey == subKey);

                if (monitorTransaction == null)
                {
                    return Json(new { success = false, message = "Monitor Transaction not found" });
                }

                return Json(new
                {
                    success = true,
                    NationalId = monitorTransaction.ExamMonitor.NationalId,
                    FirstName = monitorTransaction.ExamMonitor.FirstName,
                    LastName = monitorTransaction.ExamMonitor.LastName,
                    Sex = monitorTransaction.ExamMonitor.Sex,
                    Status = monitorTransaction.ExamMonitor.Status,
                    Region = monitorTransaction.ExamMonitor.Region,
                    SubKey = monitorTransaction.Session,
                    CentreAttached = monitorTransaction.CentreAttached,
                    ClusterName = monitorTransaction.CentreAttached
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ExamMonitors
        public IActionResult Index()
        {
            // Get distinct centres from your database
            // Get distinct centres from your database
            ViewBag.Clusters = _context.Centres
                .Where(a => a.IsCluster == "IsCluster")
    .Select(m => new {
        Value = m.ClusterCode,
        Text = m.ClusterName
    })
    .Distinct()
    .OrderBy(c => c.Text)
    .ToList();

            ViewBag.Districts = _context.ExamMonitors
                .Where(m => m.District != null)
                .Select(m => new {
                    Value = m.District,
                    Text = m.District
                })
                .Distinct()
                .OrderBy(d => d.Text)
                .ToList();

            return View();

        }
    }
}
