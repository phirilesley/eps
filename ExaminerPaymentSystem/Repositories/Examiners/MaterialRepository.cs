using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly ApplicationDbContext _context;

        public MaterialRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Material>> GetAllAsync() => await _context.MaterialMaster.ToListAsync();
        public async Task<Material?> GetByIdAsync(int id) => await _context.MaterialMaster.FindAsync(id);
        public async Task AddAsync(Material material) { _context.MaterialMaster.Add(material); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Material material) { _context.MaterialMaster.Update(material); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        {
            var material = await _context.MaterialMaster.FindAsync(id);
            if (material != null)
            {
                _context.MaterialMaster.Remove(material);
                await _context.SaveChangesAsync();
            }
        }
    }
}
