using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class MaterialTransactionRepository : IMaterialTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public MaterialTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MaterialTransaction>> GetAllAsync() => await _context.MaterialTransaction.ToListAsync();
        public async Task<MaterialTransaction?> GetByIdAsync(int id) => await _context.MaterialTransaction.FindAsync(id);
        public async Task AddAsync(MaterialTransaction materialTransaction) { _context.MaterialTransaction.Add(materialTransaction); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(MaterialTransaction materialTransaction) { _context.MaterialTransaction.Update(materialTransaction); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        {
            var materialTransaction = await _context.MaterialTransaction.FindAsync(id);
            if (materialTransaction != null)
            {
                _context.MaterialTransaction.Remove(materialTransaction);
                await _context.SaveChangesAsync();
            }
        }
    }

}
