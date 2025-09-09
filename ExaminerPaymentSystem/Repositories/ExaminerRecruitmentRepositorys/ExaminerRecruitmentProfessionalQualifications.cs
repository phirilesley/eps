using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys
{
    public class ExaminerRecruitmentProfessionalQualifications : IExaminerRecruitmentProfessionalQualifications
    {
        private readonly ApplicationDbContext _context;

        public ExaminerRecruitmentProfessionalQualifications(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<List<ProfessionalQualifications>> GetByExaminerRecruitmentIdAsync(int examinerRecruitmentId)
        {
            return await _context.ExaminerRecruitmentProfessionalQualifications
                .Where(te => te.ExaminerRecruitmentId == examinerRecruitmentId)
                .ToListAsync();
        }

        public async Task AddAsync(ProfessionalQualifications professionalQualification)
        {
            await _context.ExaminerRecruitmentProfessionalQualifications.AddAsync(professionalQualification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProfessionalQualifications professionalQualification)
        {
            _context.ExaminerRecruitmentProfessionalQualifications.Update(professionalQualification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var professionalQualification = await _context.ExaminerRecruitmentProfessionalQualifications.FindAsync(id);
            if (professionalQualification != null)
            {
                _context.ExaminerRecruitmentProfessionalQualifications.Remove(professionalQualification);
                await _context.SaveChangesAsync();
            }
        }



    }
}

