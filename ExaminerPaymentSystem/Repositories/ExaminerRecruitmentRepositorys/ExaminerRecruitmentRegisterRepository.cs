using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys
{
    public class ExaminerRecruitmentRegisterRepository : IExaminerRecruitmentRegisterRepository
    {
        private readonly ApplicationDbContext _context;
      
       

        public ExaminerRecruitmentRegisterRepository(ApplicationDbContext context)
        {
            _context =context;
         
        }

        public async Task<ExaminerRecruitmentRegister> GetByIdAsync(int id)
        {
            return await _context.ExaminerRecruitmentRegisters
                        .Where(e => e.ExaminerRecruitmentId == id)
                        .FirstOrDefaultAsync();
        }
            
        public async Task<IEnumerable<ExaminerRecruitmentRegister>> GetAllAsync()
        {
            return await _context.ExaminerRecruitmentRegisters.Include(e => e.ExaminerRecruitment).ToListAsync();
        }

        public async Task<(List<ExaminerRecruimentDataTableViewModel> Data, int TotalCount, int TotalGradeA, int TotalGradeB, int TotalGradeC, int TotalGradeD, int totalPending)> GetAllPresentAsync(
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
            var query = _context.ExaminerRecruitmentRegisters
                .Where(e => e.Status == true)
                .Include(e => e.ExaminerRecruitment)
                .ThenInclude(e => e.ExaminerRecruitmentAssessment)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(e =>
                    e.ExaminerRecruitment.ExaminerName.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.LastName.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.CemId.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.EmailAddress.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.PhoneHome.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.Subject.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.PaperCode.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.ExaminerCode.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.Sex.ToLower().Contains(searchValue)
                );
            }

   

            // Apply custom filters

            if (!string.IsNullOrWhiteSpace(sessionLevelFilter))
            {
                query = query.Where(e => e.ExaminerRecruitment.Experience.ToLower().Contains(sessionLevelFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(subjectFilter))
            {
                query = query.Where(e => e.ExaminerRecruitment.Subject.ToLower().Contains(subjectFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(paperCodeFilter))
            {
                query = query.Where(e => e.ExaminerRecruitment.PaperCode.ToLower().Contains(paperCodeFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(regionCodeFilter))
            {
                query = query.Where(e => e.ExaminerRecruitment.RegionCode.ToLower().Contains(regionCodeFilter.ToLower())); // Assuming CemId is the region code
            }

            // Calculate total count after applying filters
            var totalCount = await query.CountAsync();

            // Get filtered examiner IDs
            var filteredExaminerIds = await query.Select(e => e.ExaminerRecruitment.Id).ToListAsync();

            // Calculate total assessments based on filtered examiners
            var totalInAssessment = await _context.ExaminerRecruitmentAssessments
                .Where(er => filteredExaminerIds.Contains(er.ExaminerRecruitmentId)) // Filter only related assessments
                .CountAsync();

            // Calculate total pending
            var totalPending = totalCount - totalInAssessment;


            var gradeCounts = await _context.ExaminerRecruitmentAssessments
                .Where(er => filteredExaminerIds.Contains(er.ExaminerRecruitmentId)) // Filter only related assessments
                .GroupBy(er => er.CapturerGrade)
                .Select(g => new
                {
                    Grade = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var totalGradeA = gradeCounts.FirstOrDefault(g => g.Grade == "A")?.Count ?? 0;
            var totalGradeB = gradeCounts.FirstOrDefault(g => g.Grade == "B")?.Count ?? 0;
            var totalGradeC = gradeCounts.FirstOrDefault(g => g.Grade == "C")?.Count ?? 0;
            var totalGradeD = gradeCounts.FirstOrDefault(g => g.Grade == "D")?.Count ?? 0;




            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortDirection))
            {
                var isAscending = sortDirection.ToLower() == "asc";

                query = sortColumn switch
                {
                    "examinerName" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.ExaminerName) : query.OrderByDescending(e => e.ExaminerRecruitment.ExaminerName),
                    "lastName" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.LastName) : query.OrderByDescending(e => e.ExaminerRecruitment.LastName),
                    "paperCode" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.PaperCode) : query.OrderByDescending(e => e.ExaminerRecruitment.PaperCode),
                    "subject" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.Subject) : query.OrderByDescending(e => e.ExaminerRecruitment.Subject),
                    "cemId" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.CemId) : query.OrderByDescending(e => e.ExaminerRecruitment.CemId),
                    "examinerCode" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.ExaminerCode) : query.OrderByDescending(e => e.ExaminerRecruitment.ExaminerCode),
                    "phoneHome" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.PhoneHome) : query.OrderByDescending(e => e.ExaminerRecruitment.PhoneHome),
                    "emailAddress" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.EmailAddress) : query.OrderByDescending(e => e.ExaminerRecruitment.EmailAddress),
                    "gender" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.Sex) : query.OrderByDescending(e => e.ExaminerRecruitment.Sex),
                    _ => query.OrderBy(e => e.Id) // Default sorting if column is not found
                };
            }
            else
            {
                query = query.OrderBy(e => e.Id); // Default sorting
            }

            // Apply pagination and select data
            var data = await query
                .OrderByDescending(e => e.ExaminerRecruitment.ExaminerRecruitmentAssessment.Date)
                .Skip(skip)
                .Take(take)
                .Select(e => new ExaminerRecruimentDataTableViewModel
                {
                    Id = e.ExaminerRecruitment.Id,
                    ExaminerName = e.ExaminerRecruitment.ExaminerName,
                    LastName = e.ExaminerRecruitment.LastName,
                    PaperCode = e.ExaminerRecruitment.PaperCode,
                    Subject = e.ExaminerRecruitment.Subject,
                    CemId = e.ExaminerRecruitment.CemId,
                    ExaminerCode = e.ExaminerRecruitment.ExaminerCode,
                    PhoneHome = e.ExaminerRecruitment.PhoneHome,
                    EmailAddress = e.ExaminerRecruitment.EmailAddress,
                    Gender = e.ExaminerRecruitment.Sex,
                    Grade = e.ExaminerRecruitment.ExaminerRecruitmentAssessment != null
                    ? e.ExaminerRecruitment.ExaminerRecruitmentAssessment.CapturerGrade
                    : "Pending", // Mark as Pending if no assessment exists
                    //Statuss = e.ExaminerRecruitment.ExaminerRecruitmentAssessment.Status
                })
                .ToListAsync();

            return (data, totalCount, totalGradeA, totalGradeB, totalGradeC, totalGradeD, totalPending);
        }



        public async Task<(List<ExaminerRecruimentDataTableViewModel> Data, int TotalCount, int TotalGradeA, int TotalGradeB, int TotalGradeC, int TotalGradeD, int totalPending)>GetAllPresentForAssesmentAsync
            (int skip, int take, string searchValue, string sortColumn, string sortDirection,
             string experience, string subject, string paperCode, string gradeField)

        {
            var query = _context.ExaminerRecruitmentRegisters
                .Where(e => e.Status == true)
                .Include(e => e.ExaminerRecruitment)
                .ThenInclude(e => e.ExaminerRecruitmentAssessment)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(e =>
                    e.ExaminerRecruitment.ExaminerName.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.LastName.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.CemId.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.EmailAddress.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.PhoneHome.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.Subject.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.PaperCode.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.ExaminerCode.ToLower().Contains(searchValue) ||
                    e.ExaminerRecruitment.Sex.ToLower().Contains(searchValue)
                );
            }



            // Apply custom filters

            if (!string.IsNullOrWhiteSpace(experience))
            {
                query = query.Where(e => e.ExaminerRecruitment.Experience.ToLower().Contains(experience.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(subject))
            {
                query = query.Where(e => e.ExaminerRecruitment.Subject.ToLower().Contains(subject.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(paperCode))
            {
                query = query.Where(e => e.ExaminerRecruitment.PaperCode.ToLower().Contains(paperCode.ToLower()));
            }

            // Calculate total count after applying filters
            var totalCount = await query.CountAsync();

            // Get filtered examiner IDs
            var filteredExaminerIds = await query.Select(e => e.ExaminerRecruitment.Id).ToListAsync();

            // Calculate total assessments based on filtered examiners
            var totalInAssessment = await _context.ExaminerRecruitmentAssessments
                .Where(er => filteredExaminerIds.Contains(er.ExaminerRecruitmentId)) // Filter only related assessments
                .CountAsync();

            // Calculate total pending
           var totalPending = totalCount - totalInAssessment;


            var gradeCounts = await _context.ExaminerRecruitmentAssessments
               .Where(er => filteredExaminerIds.Contains(er.ExaminerRecruitmentId))
               .GroupBy(er => EF.Property<string>(er, gradeField))
               .Select(g => new
               {
                   Grade = g.Key,
                   Count = g.Count()
               })
               .ToListAsync();


            var totalGradeA = gradeCounts.FirstOrDefault(g => g.Grade == "A")?.Count ?? 0;
            var totalGradeB = gradeCounts.FirstOrDefault(g => g.Grade == "B")?.Count ?? 0;
            var totalGradeC = gradeCounts.FirstOrDefault(g => g.Grade == "C")?.Count ?? 0;
            var totalGradeD = gradeCounts.FirstOrDefault(g => g.Grade == "D")?.Count ?? 0;
       




            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortDirection))
            {
                var isAscending = sortDirection.ToLower() == "asc";

                query = sortColumn switch
                {
                    "examinerName" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.ExaminerName) : query.OrderByDescending(e => e.ExaminerRecruitment.ExaminerName),
                    "lastName" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.LastName) : query.OrderByDescending(e => e.ExaminerRecruitment.LastName),
                    "paperCode" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.PaperCode) : query.OrderByDescending(e => e.ExaminerRecruitment.PaperCode),
                    "subject" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.Subject) : query.OrderByDescending(e => e.ExaminerRecruitment.Subject),
                    "cemId" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.CemId) : query.OrderByDescending(e => e.ExaminerRecruitment.CemId),
                    "examinerCode" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.ExaminerCode) : query.OrderByDescending(e => e.ExaminerRecruitment.ExaminerCode),
                    "phoneHome" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.PhoneHome) : query.OrderByDescending(e => e.ExaminerRecruitment.PhoneHome),
                    "emailAddress" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.EmailAddress) : query.OrderByDescending(e => e.ExaminerRecruitment.EmailAddress),
                    "gender" => isAscending ? query.OrderBy(e => e.ExaminerRecruitment.Sex) : query.OrderByDescending(e => e.ExaminerRecruitment.Sex),
                    _ => query.OrderBy(e => e.Id) // Default sorting if column is not found
                };
            }
            else
            {
                query = query.OrderBy(e => e.Id); // Default sorting
            }

            // Apply pagination and select data
            var data = await query
                .OrderByDescending(e => e.ExaminerRecruitment.ExaminerRecruitmentAssessment.Date)
                .Skip(skip)
                .Take(take)
                .Select(e => new ExaminerRecruimentDataTableViewModel
                {
                    Id = e.ExaminerRecruitment.Id,
                    ExaminerName = e.ExaminerRecruitment.ExaminerName,
                    LastName = e.ExaminerRecruitment.LastName,
                    PaperCode = e.ExaminerRecruitment.PaperCode,
                    Subject = e.ExaminerRecruitment.Subject,
                    CemId = e.ExaminerRecruitment.CemId,
                    ExaminerCode = e.ExaminerRecruitment.ExaminerCode,
                    PhoneHome = e.ExaminerRecruitment.PhoneHome,
                    EmailAddress = e.ExaminerRecruitment.EmailAddress,
                    Gender = e.ExaminerRecruitment.Sex,
                    ExaminerRecruitmentRegisterId = e.Id,
                    CapturerGrade = e.ExaminerRecruitment.ExaminerRecruitmentAssessment != null
                    ? e.ExaminerRecruitment.ExaminerRecruitmentAssessment.CapturerGrade
                    : null,
                    VerifierGrade = e.ExaminerRecruitment.ExaminerRecruitmentAssessment != null
                    ? e.ExaminerRecruitment.ExaminerRecruitmentAssessment.VerifierGrade
                    : null,
                    //Statuss = e.ExaminerRecruitment.ExaminerRecruitmentAssessment.Status
                })

                .ToListAsync();

            return (data, totalCount, totalGradeA, totalGradeB, totalGradeC, totalGradeD, totalPending);
        }


        public async Task AddAsync(ExaminerRecruitmentRegister entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _context.ExaminerRecruitmentRegisters.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExaminerRecruitmentRegister entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.ExaminerRecruitmentRegisters.Update(entity);
            await SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) throw new ArgumentException($"Entity with ID {id} not found.");
            _context.ExaminerRecruitmentRegisters.Remove(entity);
        }

        public async Task<ExaminerRecruitmentRegister?> FindByExaminerIdAsync(int ExaminerRecruitmentId)
        {
            var examinerRecruit = await _context.ExaminerRecruitmentRegisters
                .SingleOrDefaultAsync(e => e.ExaminerRecruitmentId == ExaminerRecruitmentId);

            return examinerRecruit; // Returns null if no record is found, otherwise returns the record
        }


        public async Task<List<ExaminerRecruitmentAndSelectionViewModel>> LoadByParametersRegister(
         string subject,
         string paperCode,
         string experience)
        {
            var query = _context.ExaminerRecruitmentRegisters
                .Where(e => e.Status == true)
                .Include(e => e.ExaminerRecruitment)
                .AsQueryable();

            // Apply filters safely
            if (!string.IsNullOrWhiteSpace(subject))
            {
                query = query.Where(e =>
                    e.ExaminerRecruitment != null &&
                    EF.Functions.Like(e.ExaminerRecruitment.Subject, $"%{subject}%"));
            }

            if (!string.IsNullOrWhiteSpace(paperCode))
            {
                query = query.Where(e =>
                    e.ExaminerRecruitment != null &&
                    EF.Functions.Like(e.ExaminerRecruitment.PaperCode, $"%{paperCode}%"));
            }

            if (!string.IsNullOrWhiteSpace(experience))
            {
                query = query.Where(e =>
                    e.ExaminerRecruitment != null &&
                    EF.Functions.Like(e.ExaminerRecruitment.Experience, $"%{experience}%"));
            }

            return await query
                .Where(e => e.ExaminerRecruitment != null) // Additional safety
                .Select(er => new ExaminerRecruitmentAndSelectionViewModel
                {
                    ExaminerRecruitmentId = er.Id,
                    ExaminerName = er.ExaminerRecruitment.ExaminerName,
                    LastName = er.ExaminerRecruitment.LastName,
                    PaperCode = er.ExaminerRecruitment.PaperCode,
                    Subject = er.ExaminerRecruitment.Subject,
                    CemId = er.ExaminerRecruitment.CemId,
                    Experience = er.ExaminerRecruitment.Experience,
                    PhoneHome = er.ExaminerRecruitment.PhoneHome,
                    EmailAddress = er.ExaminerRecruitment.EmailAddress,
                    Gender = er.ExaminerRecruitment.Sex,
                })
                .ToListAsync();
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


        public async Task<List<ExaminerRecruitmentAssesmentPresentWithoutGrade>> GetPresentExaminersWithoutGradesAsync(
           string sessionLevelFilter,
           string subjectFilter,
           string paperCodeFilter,
           string regionCodeFilter)
        {
            var query = _context.ExaminerRecruitmentRegisters
               .Include(r => r.ExaminerRecruitment)
               .Include(r => r.ExaminerRecruitmentAssessment)
                   .ThenInclude(a => a.Capturer)
               .Include(r => r.ExaminerRecruitmentAssessment)
                   .ThenInclude(a => a.Verifier)
               .Where(r => r.Status == true && r.ExaminerRecruitmentAssessment == null)
               .Select(r => new ExaminerRecruitmentAssesmentPresentWithoutGrade
               {
                   ExaminerName = r.ExaminerRecruitment.ExaminerName,
                   LastName = r.ExaminerRecruitment.LastName,
                   CemId = r.ExaminerRecruitment.CemId,
                   Subject = r.ExaminerRecruitment.Subject,
                   PaperCode = r.ExaminerRecruitment.PaperCode,
                   RegionCode = r.ExaminerRecruitment.RegionCode,
                   Experience = r.ExaminerRecruitment.Experience,
                   CapturerUserName = r.ExaminerRecruitmentAssessment != null && r.ExaminerRecruitmentAssessment.Capturer != null
                    ? r.ExaminerRecruitmentAssessment.Capturer.UserName
                    : null,
                     VerifierUserName = r.ExaminerRecruitmentAssessment != null && r.ExaminerRecruitmentAssessment.Verifier != null
                    ? r.ExaminerRecruitmentAssessment.Verifier.UserName
                    : null
               });

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
                query = query.Where(e => e.RegionCode.ToLower().Contains(regionCodeFilter.ToLower()));
            }

            return await query.ToListAsync();
        }
    }
}
