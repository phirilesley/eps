using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys
{
    public class ExaminerRecruitmentAssessmentRepository : IExaminerRecruitmentAssessmentRepository
    {

        private readonly ApplicationDbContext _context;

        public ExaminerRecruitmentAssessmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExaminerRecruitmentAssessment>> GetAllAsync()
        {
            return await _context.ExaminerRecruitmentAssessments.ToListAsync();
        }

        public async Task<ExaminerRecruitmentAssessment?> GetByIdAsync(int id)
        {
            return await _context.ExaminerRecruitmentAssessments.FindAsync(id);
        }

        public async Task<ExaminerRecruitmentAssessment?> GetByExaminerIdAsync(int id, bool includeRecruitment = false)
        {
            var query = _context.ExaminerRecruitmentAssessments
                .Where(e => e.ExaminerRecruitmentId == id);

            if (includeRecruitment)
            {
                query = query.Include(e => e.ExaminerRecruitment);
            }

            return await query.SingleOrDefaultAsync();
        }

        public async Task<ExaminerRecruitmentAssessment?> GetAssessmentByExaminerIdAsync(int examinerRecruitmentId)
        {
            return await _context.ExaminerRecruitmentAssessments
                .FirstOrDefaultAsync(a => a.ExaminerRecruitmentId == examinerRecruitmentId);
        }


        public async Task AddAsync(ExaminerRecruitmentAssessment assessment)
        {
            await _context.ExaminerRecruitmentAssessments.AddAsync(assessment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExaminerRecruitmentAssessment assessment)
        {
            _context.ExaminerRecruitmentAssessments.Update(assessment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SaveExaminerTraineeGradeAsync(ExaminerRecruitmentAssessment examinerGrade)
        {
            try
            {
                // Check if a record with the same ExaminerRecruitmentId exists
                var existingRecord = await _context.ExaminerRecruitmentAssessments
                    .FirstOrDefaultAsync(e => e.ExaminerRecruitmentId == examinerGrade.ExaminerRecruitmentId);

                if (existingRecord != null)
                {
                    // Record exists, update the existing record
                    existingRecord.CapturerGrade = examinerGrade.CapturerGrade;
                    existingRecord.VerifierGrade = examinerGrade.VerifierGrade;
                    //existingRecord.Status = examinerGrade.Status;
                    //existingRecord.Comments = examinerGrade.Comments;
                    existingRecord.CapturerId = examinerGrade.CapturerId;
                    existingRecord.VerifierId = examinerGrade.VerifierId;
                    // Update any other fields as needed

                    _context.ExaminerRecruitmentAssessments.Update(existingRecord);
                }
                else
                {
                    // Record does not exist, insert a new one
                    _context.ExaminerRecruitmentAssessments.Add(examinerGrade);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message); // Log the error for debugging
                return false;
            }
        }




        public async Task DeleteAsync(int id)
        {
            var assessment = await _context.ExaminerRecruitmentAssessments.FindAsync(id);
            if (assessment != null)
            {
                _context.ExaminerRecruitmentAssessments.Remove(assessment);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<List<ExaminerRecruitmentAssessment>> GetPartiallyCapturedGradesAsync(
            string sessionLevelFilter,
            string subjectFilter,
            string paperCodeFilter,
            string regionCodeFilter,
             string reportType)
        {
            IQueryable<ExaminerRecruitmentAssessment> query;

            if (reportType != "" && reportType == "allCaptured")
            {
                query = _context.ExaminerRecruitmentAssessments
                    .Include(x => x.ExaminerRecruitment)
                    .Include(x => x.Capturer)
                    .Include(x => x.Verifier)
                    .Where(x => !string.IsNullOrEmpty(x.VerifierGrade) && !string.IsNullOrEmpty(x.CapturerGrade));
            }
            else
            {
                query = _context.ExaminerRecruitmentAssessments
                    .Include(x => x.ExaminerRecruitment)
                    .Include(x => x.Capturer)
                    .Include(x => x.Verifier)
                    .Where(x => string.IsNullOrEmpty(x.VerifierGrade) || string.IsNullOrEmpty(x.CapturerGrade));
            }

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
                query = query.Where(e => e.ExaminerRecruitment.RegionCode.ToLower().Contains(regionCodeFilter.ToLower()));
            }

            return await query.ToListAsync();
        }



        public async Task<List<ExaminerRecruitmentAssessmentReportViewModel>> GetExaminerAssessmentReportAsync(
                string sessionLevelFilter,
                string subjectFilter ,
                string paperCodeFilter,
                string regionCodeFilter)
             {
              var query = _context.ExaminerRecruitmentRegisters
                .Include(r => r.ExaminerRecruitment)
                .Include(r => r.ExaminerRecruitmentAssessment)
                .Where(r => r.Status == true)
                .Select(r => new
                {
                    Subject = r.ExaminerRecruitment.Subject,
                    PaperCode = r.ExaminerRecruitment.PaperCode,
                    RegionCode = r.ExaminerRecruitment.RegionCode,
                    Experience = r.ExaminerRecruitment.Experience,
                    CapturerGrade = r.ExaminerRecruitmentAssessment != null ? r.ExaminerRecruitmentAssessment.CapturerGrade : null,
                    VerifierGrade = r.ExaminerRecruitmentAssessment != null ? r.ExaminerRecruitmentAssessment.VerifierGrade : null,
                    HasAssessment = r.ExaminerRecruitmentAssessment != null,
                    IsPresent = r.Status == true
                });


            // Apply filters
            if (!string.IsNullOrWhiteSpace(sessionLevelFilter))
            {
                query = query.Where(x => x.Experience.ToLower().Contains(sessionLevelFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(subjectFilter))
            {
                query = query.Where(x => x.Subject.ToLower().Contains(subjectFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(paperCodeFilter))
            {
                query = query.Where(x => x.RegionCode.ToLower().Contains(paperCodeFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(regionCodeFilter))
            {
                query = query.Where(x => x.RegionCode.ToLower().Contains(regionCodeFilter.ToLower()));
            }

            // Group and calculate metrics

            // Group and calculate metrics
            var result = await query
                .GroupBy(x => new { x.Subject, x.PaperCode })
                .Select(g => new ExaminerRecruitmentAssessmentReportViewModel
                {
                    Subject = g.Key.Subject,
                    PaperCode = g.Key.PaperCode,
                    GoodEntries = g.Count(x => x.HasAssessment &&
                                               x.CapturerGrade != null &&
                                               x.VerifierGrade != null &&
                                               x.CapturerGrade == x.VerifierGrade),
                    BadEntries = g.Count(x => x.HasAssessment &&
                                              x.CapturerGrade != null &&
                                              x.VerifierGrade != null &&
                                              x.CapturerGrade != x.VerifierGrade),
                    TotalPresentExaminers = g.Count(x => x.IsPresent),
                    Partially = g.Count(x => x.HasAssessment &&
                                            ((x.CapturerGrade != null && x.VerifierGrade == null) ||
                                             (x.CapturerGrade == null && x.VerifierGrade != null))),
                    Percentage = g.Count(x => x.IsPresent) == 0
                        ? 0
                        : (double)g.Count(x => x.HasAssessment &&
                                              x.CapturerGrade != null &&
                                              x.VerifierGrade != null &&
                                              x.CapturerGrade == x.VerifierGrade) /
                          g.Count(x => x.IsPresent) * 100
                })
                .OrderBy(x => x.Subject)
                .ThenBy(x => x.PaperCode)
                .ToListAsync();

            return result;
        }



    }
}
