using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys 
{
    public class ExaminerRecruitmentRepository : IExaminerRecruitmentRepository
    {
        private readonly ApplicationDbContext _context;

        public ExaminerRecruitmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ExaminerRecruitment>> GetAllAsync()
        {
            return await _context.ExaminerRecruitment
                                 .Include(er => er.ExaminerRecruitmentAttachements)
                                 .Include(er => er.TeachingExperiences)
                                 .ToListAsync();
        }

        public async Task<ExaminerRecruitment> GetByIdAsync(int id)
        {
            // Eagerly load related data: ExaminerRecruitmentAttachements and TeachingExperiences
            return await _context.ExaminerRecruitment
                .Include(er => er.ExaminerRecruitmentAttachements)  // Include related attachments
                .Include(er => er.TeachingExperiences)              // Include related teaching experiences
                .Include(er => er.ProfessionalQualifications)
                .FirstOrDefaultAsync(er => er.Id == id);            // Return a single record based on the ID
        }



        public async Task AddAsync(ExaminerRecruitment examinerRecruitment)
        {
            try
            {
                await _context.ExaminerRecruitment.AddAsync(examinerRecruitment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Add context or log the exception
                var message = $"An error occurred while adding an ExaminerRecruitment entity: {ex.Message}";
              

                throw new Exception(message, ex); // Rethrow with additional context
            }
        }


        public async Task UpdateAsync(ExaminerRecruitment examinerRecruitment)
        {
            _context.ExaminerRecruitment.Update(examinerRecruitment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ExaminerRecruitment.FindAsync(id);
            if (entity != null)
            {
                _context.ExaminerRecruitment.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        /// Retrieves paginated data with optional search and filtering.
        /// </summary>
        public async Task<(List<ExaminerRecruimentDataTableViewModel> Data, int TotalCount)> GetPaginatedAsync(
             int skip,
             int take,
             string searchValue,
             string sortColumn,
             string sortDirection,
             string sessionLevelFilter,
             string subjectFilter,
             string paperCodeFilter,
             string regionCodeFilter)
        {
            // Start with the base query including related entities
            var query = _context.ExaminerRecruitment
                .Include(er => er.ExaminerRecruitmentAttachements)
                .Include(er => er.TeachingExperiences)
                .Include(er => er.ExaminerRecruitmentTrainingSelection)
                .Where(er => er.ExaminerRecruitmentTrainingSelection == null)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower(); // Convert to lower case for case-insensitive search
                query = query.Where(e =>
                    e.ExaminerName.ToLower().Contains(searchValue) ||
                    e.LastName.ToLower().Contains(searchValue) ||
                    e.CemId.ToLower().Contains(searchValue) ||
                    e.EmailAddress.ToLower().Contains(searchValue) ||
                    e.PhoneHome.ToLower().Contains(searchValue) ||
                    e.Subject.ToLower().Contains(searchValue) ||
                    e.PaperCode.ToLower().Contains(searchValue) ||
                    e.ExaminerCode.ToLower().Contains(searchValue) ||
                    e.Sex.ToLower().Contains(searchValue)
                );
            }

            // Apply custom filters

            if (!string.IsNullOrWhiteSpace(sessionLevelFilter))
            {
                query = query.Where(e => e.Experience.ToLower().Contains(sessionLevelFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(subjectFilter))
            {
                query = query.Where(e => e.Subject.ToLower().Contains(subjectFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(paperCodeFilter))
            {
                query = query.Where(e => e.PaperCode.ToLower().Contains(paperCodeFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(regionCodeFilter))
            {
                query = query.Where(e => e.RegionCode.ToLower().Contains(regionCodeFilter.ToLower())); // Assuming CemId is the region code
            }

            // Calculate total count after applying filters
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortDirection))
            {
                var isAscending = sortDirection.ToLower() == "asc";
                query = isAscending
                    ? query.OrderBy(e => EF.Property<object>(e, sortColumn))
                    : query.OrderByDescending(e => EF.Property<object>(e, sortColumn));
            }
            else
            {
                query = query.OrderBy(e => e.Id); // Default sorting
            }

            // Apply pagination
            var data = await query
                .OrderByDescending(e => e.CaptureDate)
                .Skip(skip)
                .Take(take)
                .Select(e => new ExaminerRecruimentDataTableViewModel
                {
                    Id = e.Id,
                    ExaminerName = e.ExaminerName,
                    LastName = e.LastName,
                    PaperCode = e.PaperCode,
                    Subject = e.Subject,
                    CemId = e.CemId,
                    ExaminerCode = e.ExaminerCode,
                    PhoneHome = e.PhoneHome,
                    EmailAddress = e.EmailAddress,
                    Gender = e.Sex,
                    AttachHeadComment = e.ExaminerRecruitmentAttachements.InstitutionHeadDoc,
                    AcademicQualifications = e.ExaminerRecruitmentAttachements.AcademicQualifications,
                    NationalIdDocs = e.ExaminerRecruitmentAttachements.NationalIdDocs,
                    Experiences = e.TeachingExperiences.Select(te => new TeachingExperienceViewModel
                    {
                        LevelTaught = te.LevelTaught,
                        Subject = te.Subject,
                        ExperienceYears = (int)te.ExperienceYears,
                        InstitutionName = te.InstitutionName
                    }).ToList()
                })
                .ToListAsync();

            return (data, totalCount);
        }


        /// <summary>
        /// Gets the total count of records without any filters.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.ExaminerRecruitment.CountAsync();
        }

        /// <summary>
        /// Gets the count of records after applying the search filter.
        /// </summary>
        public async Task<int> GetFilteredCountAsync(string searchValue)
        {
            var query = _context.ExaminerRecruitment.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(e =>
                    e.ExaminerName.Contains(searchValue) ||
                    e.LastName.Contains(searchValue) ||
                    e.CemId.Contains(searchValue) ||
                    e.ExaminerCode.Contains(searchValue) 
                );
            }

            return await query.CountAsync();
        }
    }
}
    

