using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class CategoryCheckInCheckOutRepository : ICategoryCheckInCheckOutRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryCheckInCheckOutRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryCheckInCheckOut>> GetAllAsync() => await _context.CATEGORYCHECKINCHECKOUT.ToListAsync();
        public async Task<CategoryCheckInCheckOut?> GetByIdAsync(int id) => await _context.CATEGORYCHECKINCHECKOUT.FindAsync(id);
        public async Task AddAsync(CategoryCheckInCheckOut categoryCheckInCheckOut) { _context.CATEGORYCHECKINCHECKOUT.Add(categoryCheckInCheckOut); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(CategoryCheckInCheckOut categoryCheckInCheckOut) { _context.CATEGORYCHECKINCHECKOUT.Update(categoryCheckInCheckOut); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        {
            var categoryCheckInCheckOut = await _context.CATEGORYCHECKINCHECKOUT.FindAsync(id);
            if (categoryCheckInCheckOut != null)
            {
                _context.CATEGORYCHECKINCHECKOUT.Remove(categoryCheckInCheckOut);
                await _context.SaveChangesAsync();
            }
        }
    }

}
