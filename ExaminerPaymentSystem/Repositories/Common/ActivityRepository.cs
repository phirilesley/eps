using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.EntityFrameworkCore;


namespace ExaminerPaymentSystem.Repositories.Common
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly ApplicationDbContext _context;

        public ActivityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Activity>> GetAllActivitiesAsync()
        {
            return await _context.Activities.Where(a => a.Status == "Active").ToListAsync();
        }

        public async Task<Activity> GetActivityByValueAsync(string value)
        {
            return await _context.Activities.FirstOrDefaultAsync(a => a.Value == value);
        }

        public async Task AddActivitiesAsync(List<Activity> activities)
        {
            var existingActivities = await _context.Activities.ToListAsync();
            var newActivities = activities
                .Where(a => !existingActivities.Any(e => e.Value == a.Value))
                .ToList();

            if (newActivities.Any())
            {
                _context.Activities.AddRange(newActivities);
                await _context.SaveChangesAsync();
            }
        }
    }
}
