using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class BrailleTranscriptionRateRepository : IBrailleTranscriptionRateRepository
    {
        private readonly ApplicationDbContext _context;

        public BrailleTranscriptionRateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BrailleTranscriptionRate>> GetAllAsync()
        {
            return await _context.Braille_Transcription_Rate.ToListAsync();
        }

        public async Task<BrailleTranscriptionRate> GetByIdAsync(int id)
        {
            return await _context.Braille_Transcription_Rate.FindAsync(id);
        }

        public async Task AddAsync(BrailleTranscriptionRate rate)
        {
            await _context.Braille_Transcription_Rate.AddAsync(rate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BrailleTranscriptionRate rate)
        {
            _context.Braille_Transcription_Rate.Update(rate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rate = await _context.Braille_Transcription_Rate.FindAsync(id);
            if (rate != null)
            {
                _context.Braille_Transcription_Rate.Remove(rate);
                await _context.SaveChangesAsync();
            }
        }
    }

}
