using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys
{
    public class ExaminerRecruitmentAttachmentsRepository : IExaminerRecruitmentAttachmentsRepository
    {
        private readonly ApplicationDbContext _context;
        public ExaminerRecruitmentAttachmentsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ExaminerRecruitmentAttachements>> GetAllByRecruitmentId(int recruitmentId)
        {
            return await _context.ExaminerRecruitmentAttachements
                .Where(a => a.ExaminerRecruitmentId == recruitmentId)
                .ToListAsync();
        }

        public async Task<ExaminerRecruitmentAttachements> GetByIdAsync(int id)
        {
            return await _context.ExaminerRecruitmentAttachements.FindAsync(id);
        }

        public async Task AddAsync(ExaminerRecruitmentAttachements attachments)
        {
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            _context.ExaminerRecruitmentAttachements.Add(attachments);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExaminerRecruitmentAttachements attachment)
        {
            _context.ExaminerRecruitmentAttachements.Update(attachment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var attachment = await _context.ExaminerRecruitmentAttachements.FindAsync(id);
            if (attachment != null)
            {
                _context.ExaminerRecruitmentAttachements.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
