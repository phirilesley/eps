using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class ValidateTandSRepository : IValidateTandS
    {
        private readonly ApplicationDbContext _context;

        public ValidateTandSRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ValidateTandS> GetValidateTandSById(int Id)
        {
            try
            {
                return await _context.VALIDATETANDS.FirstOrDefaultAsync(x => x.Id == Id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ValidateTandS> SaveValidateTandS(ValidateTandS model)
        {
            _context.VALIDATETANDS.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public Task<bool> UpdateValidateTandS(ValidateTandS model)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ValidateTandS>> ValidateTandSsGetAll()
        {
            try
            {
                return await _context.VALIDATETANDS.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                // You might want to throw the exception here or return null, depending on your requirements
                throw;
            }
        }
    }
}
