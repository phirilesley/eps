
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys
{
    public class TeachingExperienceRepository  :ITeachingExperienceRepository
    {
        private readonly ApplicationDbContext _context;

        public TeachingExperienceRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<List<TeachingExperience>> GetByExaminerRecruitmentIdAsync(int examinerRecruitmentId)
        {
            return await _context.TeachingExperiences
                .Where(te => te.ExaminerRecruitmentId == examinerRecruitmentId)
                .ToListAsync();
        }

        public async Task AddAsync(TeachingExperience teachingExperience)
        {
            await _context.TeachingExperiences.AddAsync(teachingExperience);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TeachingExperience teachingExperience)
        {
            _context.TeachingExperiences.Update(teachingExperience);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var teachingExperience = await _context.TeachingExperiences.FindAsync(id);
            if (teachingExperience != null)
            {
                _context.TeachingExperiences.Remove(teachingExperience);
                await _context.SaveChangesAsync();
            }
        }



    }
}

    

