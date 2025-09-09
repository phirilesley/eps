using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys
{
    public class ExaminerRecruitmentVenueDetailsRepository : IExaminerRecruitmentVenueDetailsRepository
    {
        private readonly ApplicationDbContext _context;

        public ExaminerRecruitmentVenueDetailsRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<ExaminerRecruitmentVenueDetails> GetByIdAsync(int id)
        {
            return await _context.ExaminerRecruitmentVenueDetails.FindAsync(id);
        }

        public async Task UpdateAsync(ExaminerRecruitmentVenueDetails model)
        {
            _context.ExaminerRecruitmentVenueDetails.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ExaminerRecruitmentVenueDetails>> GetAllAsync()
        {
            return await _context.ExaminerRecruitmentVenueDetails.ToListAsync();
        }

        public async Task<(IEnumerable<ExaminerRecruitmentVenueDetails>, int, int)> GetAllDataTableAsync(
    int start, int length, string searchValue, string sortColumn, string sortDirection)
        {
            var query = _context.ExaminerRecruitmentVenueDetails
                .OrderByDescending(v => v.DateUpdated) // Default sorting by DateUpdated DESC
                .AsQueryable();

            // Filtering (Search)
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(e => e.VenueName.Contains(searchValue) ||
                                         e.TrainingStartDate.ToString().Contains(searchValue) ||
                                         e.TrainingEndDate.ToString().Contains(searchValue) ||
                                         e.CheckInDate.ToString().Contains(searchValue) ||
                                         e.CheckOutDate.ToString().Contains(searchValue));
            }

            // Sorting based on selected column
            query = sortColumn switch
            {
                "VenueName" => sortDirection == "asc" ? query.OrderBy(e => e.VenueName) : query.OrderByDescending(e => e.VenueName),
                "TrainingStartDate" => sortDirection == "asc" ? query.OrderBy(e => e.TrainingStartDate) : query.OrderByDescending(e => e.TrainingStartDate),
                "TrainingEndDate" => sortDirection == "asc" ? query.OrderBy(e => e.TrainingEndDate) : query.OrderByDescending(e => e.TrainingEndDate),
                "CheckInDate" => sortDirection == "asc" ? query.OrderBy(e => e.CheckInDate) : query.OrderByDescending(e => e.CheckInDate),
                "CheckOutDate" => sortDirection == "asc" ? query.OrderBy(e => e.CheckOutDate) : query.OrderByDescending(e => e.CheckOutDate),
                "TrainingTime" => sortDirection == "asc" ? query.OrderBy(e => e.TrainingTime) : query.OrderByDescending(e => e.TrainingTime),
                _ => query.OrderByDescending(e => e.DateUpdated) // Default sorting by DateUpdated DESC
            };

            // Total records before filtering
            var totalRecords = await _context.ExaminerRecruitmentVenueDetails.CountAsync();

            // Pagination
            var data = await query.OrderByDescending(e=> e.DateUpdated).Skip(start).Take(length).ToListAsync();

            // Filtered record count
            var filteredRecords = query.Count();

            return (data, totalRecords, filteredRecords);
        }

        public async Task AddAsync(ExaminerRecruitmentVenueDetails venueDetails)
        {
            await _context.ExaminerRecruitmentVenueDetails.AddAsync(venueDetails);
            await _context.SaveChangesAsync();
        }

   

        public async Task DeleteAsync(int id)
        {
            var venueDetails = await _context.ExaminerRecruitmentVenueDetails.FindAsync(id);
            if (venueDetails != null)
            {
                _context.ExaminerRecruitmentVenueDetails.Remove(venueDetails);
                await _context.SaveChangesAsync();
            }
        }
    }
}
