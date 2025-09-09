using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class MaxExaminerCodeRepository: IMaxExaminerCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public MaxExaminerCodeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LastNumberDatabase>> GetAllMaxExaminerCodes()
        {
            return await _context.MaxExaminerCode.ToListAsync();
        }

        public async Task<LastNumberDatabase> GetMaxExaminerCodeById(int id)
        {
            return await _context.MaxExaminerCode.FindAsync(id);
        }

        public async Task<string> GetMaxExaminerCodeFromDatabase()
        {
            var userTrainingMaxStr = await _context.EXM_EXAMINER_MASTER.MaxAsync(u => u.EMS_EXAMINER_CODE);
            return userTrainingMaxStr;
        }

        public async Task<LastNumberDatabase> SaveMaxExaminerCode(LastNumberDatabase model)
        {
            _context.MaxExaminerCode.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<LastNumberDatabase> UpdateMaxExaminerCode(LastNumberDatabase updatedBankData)
        {
            var existingRecord = await _context.MaxExaminerCode.FindAsync(updatedBankData.Id);
            if (existingRecord == null)
            {
                return null;
            }

            existingRecord.MaxNumber = updatedBankData.MaxNumber;
            // Update other properties as needed

            _context.MaxExaminerCode.Update(existingRecord);
            await _context.SaveChangesAsync();
            return existingRecord;
        }
    }
}
