using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExamMonitors;
using ExaminerPaymentSystem.Models.ExamMonitors;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.ExamMonitors
{
    public class ExamMonitorRepository : IExamMonitorRepository
    {
    private readonly ApplicationDbContext _context;
        public ExamMonitorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(ExamMonitor entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(ExamMonitor entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ExamMonitor>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ExamMonitor>> GetByCentreAsync(string centre)
        {
            throw new NotImplementedException();
        }

        public Task<ExamMonitor> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ExamMonitor>> GetByRegionAsync(string region)
        {
            throw new NotImplementedException();
        }

        public async Task<ExamMonitor> GetMonitorByRecordAsync(string nationalId)
        {
            return await _context.ExamMonitors
              .Include(a => a.ExamMonitorTandSs)
              .Include(a => a.ExamMonitorTransactions)
              .FirstOrDefaultAsync(s => s.NationalId == nationalId);
        }

        public Task<int> SaveAsync()
        {
            throw new NotImplementedException();
        }

        public void Update(ExamMonitor entity)
        {
            throw new NotImplementedException();
        }
    }
}
