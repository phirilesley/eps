using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExamMonitors;
using ExaminerPaymentSystem.Models.ExamMonitors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExaminerPaymentSystem.Repositories.ExamMonitors
{
    public class ExamMonitorApprovalRepository : IExamMonitorApprovalRepository
    {
        private readonly ApplicationDbContext _context;
        //new
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamMonitorApprovalRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ExamMonitorApprovalIndexViewModel>> GetUsersWithRegistersAsync()
        {
            return await _context.ExamMonitorRegisterDates
                .Include(x => x.Register)
                .Include(z => z.Transaction)
                .Include(x => x.Transaction.ExamMonitor)
                .Where(x => x.Register != null)
                .Select(x => new ExamMonitorApprovalIndexViewModel
                {
                    SubKey = x.SubKey,
                    ClusterManagerBy = x.Register.ClusterManagerBy,
                    CentreAttached = x.Transaction.CentreAttached,
                    MonitorStatus = x.Transaction.Status,
                    ClusterManagerStatus = x.Register.ClusterManagerStatus,
                    RegionalManagerStatus = x.Register.RegionalManagerStatus,
                    CompiledStatus = x.CompiledStatus,
                    FullName = x.Transaction.ExamMonitor.FirstName + " "+ x.Transaction.ExamMonitor.LastName,
                    PhaseCode = x.Transaction.Phase,
                    SessionCode = x.Transaction.Session,
                    PhaseName = x.Transaction.Phase,
                    SessionName = x.Transaction.Session,
                    Role = x.Transaction.Status

                })
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<ExamMonitorRegisterDate>> GetRegisterDatesAsync(string subKey)
        {
            return await _context.ExamMonitorRegisterDates
                .Include(x => x.Register)
                .Where(x => x.SubKey == subKey)
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task UpdateApprovalStatusAsync(ExamMonitorRegisterDate registerDate)
        {
            _context.Entry(registerDate).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }


    }
}