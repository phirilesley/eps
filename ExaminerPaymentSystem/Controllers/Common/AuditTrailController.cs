using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models;
using ExaminerPaymentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Controllers.Common
{
    public class AuditTrailsController : Controller
    {
        private readonly IAuditTrailRepository _auditTrailRepository;
        private readonly ApplicationDbContext _context;
        public AuditTrailsController(IAuditTrailRepository auditTrailRepository, ApplicationDbContext context)
        {
            _auditTrailRepository = auditTrailRepository;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {

            return View();

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault());
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault());
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();

            var query = _context.AuditTrails
                .AsNoTracking() // No tracking improves performance
                .Include(a => a.User)
                .Select(a => new
                {
                    a.Id,
                    a.Action,
                    a.Module,
                    a.AffectedTable,
                    a.TimeStamp,
                    a.User.UserName
                })
                .AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(a =>
                    EF.Functions.Like(a.Action, $"%{searchValue}%") ||
                    EF.Functions.Like(a.Module, $"%{searchValue}%") ||
                    EF.Functions.Like(a.AffectedTable, $"%{searchValue}%") ||
                    EF.Functions.Like(a.UserName, $"%{searchValue}%"));
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(a => a.TimeStamp)
                .Skip(start)
                .Take(length)
                .ToListAsync();

            return Json(new
            {
                draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                data = data.Select(a => new
                {
                    a.Action,
                    a.Module,
                    a.AffectedTable,
                    TimeStamp = a.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    User = a.UserName,
                    a.Id
                })
            });
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditTrail = await _context.AuditTrails
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditTrail == null)
            {
                return NotFound();
            }

            return View(auditTrail);
        }
    }
}
