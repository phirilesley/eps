using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys
{
    public class ExaminerRecruitmentEmailInvitationRepository : IExaminerRecruitmentEmailInvitationRepository
    {
        private readonly ApplicationDbContext _context;

        public ExaminerRecruitmentEmailInvitationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExaminerRecruitmentEmailInvitation>> GetAllAsync()
        {
            return await _context.ExaminerRecruitmentEmailInvitations
                .Include(i => i.ExaminerRecruitment)
                .Include(i => i.InvitedByUser)
                .ToListAsync();
        }

        public async Task<ExaminerRecruitmentEmailInvitation> GetByIdAsync(int id)
        {

            return await _context.ExaminerRecruitmentEmailInvitations
            .Include(i => i.ExaminerRecruitment)
            .Include(i => i.InvitedByUser)
            .FirstOrDefaultAsync(i => i.Id == id);
           
        }

        public async Task<ExaminerRecruitmentEmailInvitation> GetByExaminerRecruitmentIdAsync(int id)
        {

            var invitation = await _context.ExaminerRecruitmentEmailInvitations
            .Include(i => i.ExaminerRecruitment)
            .Include(i => i.InvitedByUser)
            .FirstOrDefaultAsync(i => i.ExaminerRecruitmentId == id);
            return invitation;
        }

        public async Task AddAsync(ExaminerRecruitmentEmailInvitation invitation)
        {
            _context.ExaminerRecruitmentEmailInvitations.Add(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var invitation = await _context.ExaminerRecruitmentEmailInvitations.FindAsync(id);
            if (invitation != null)
            {
                _context.ExaminerRecruitmentEmailInvitations.Remove(invitation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task BulkDeleteAsync(IEnumerable<int> ids)
        {
            var invitations = await _context.ExaminerRecruitmentEmailInvitations
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();

            if (invitations.Any())
            {
                _context.ExaminerRecruitmentEmailInvitations.RemoveRange(invitations);
                await _context.SaveChangesAsync();
            }
        }
    }

}

