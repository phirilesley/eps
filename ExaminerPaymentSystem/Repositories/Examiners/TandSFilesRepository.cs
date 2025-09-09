using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Models.Major;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class TandSFilesRepository : ITandSFilesRepository
    {
        private readonly ApplicationDbContext _context;

        public TandSFilesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TandSFile> AddTandSFile(TandSFile tandSFile,string userid)
        {
            try
            {
                _context.TRAVELLING_AND_SUBSISTENCE_FILES.Add(tandSFile);
                await _context.SaveChangesAsync(userid);
                return tandSFile;
            }
            catch (Exception ex)
            {
                // Log exception details
                throw new Exception("Error saving file to database", ex);
            }
        }


        public async Task<IEnumerable<TandSFile>> GetTandSFiles(string nationaId, string tandscode, string subKey, string examinerCode)
        {
            return await _context.TRAVELLING_AND_SUBSISTENCE_FILES.Where(e => e.EMS_NATIONAL_ID == nationaId && e.TANDSCODE == tandscode && e.EMS_EXAMINER_CODE == examinerCode && e.EMS_SUBKEY == subKey).ToListAsync();
        }

       
    }
}
