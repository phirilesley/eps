using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Major;
using Microsoft.EntityFrameworkCore;


namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class AdvanceFeesRepository : IAdvanceFeesRepository
    {
        private readonly ApplicationDbContext _context;

        public AdvanceFeesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAdvanceFees(TandSAdvanceFees fees)
        {
            _context.TRAVELLING_AND_SUBSISTENCE_FEES.Add(fees);
            await _context.SaveChangesAsync();
        }

        public Task DeleteAdvanceFees(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<TandSAdvanceFees> GetAdvanceFees()
        {
            return await _context.TRAVELLING_AND_SUBSISTENCE_FEES.FirstOrDefaultAsync();
        }

        public async Task UpdateAdvanceFees(TandSAdvanceFees fees,string userId)
        {
            try
            {
                var data = await _context.TRAVELLING_AND_SUBSISTENCE_FEES
                                        .FirstOrDefaultAsync();

                if (data != null)
                {
                    data.FEE_LUNCH = fees.FEE_LUNCH;
                    data.FEE_ACCOMMODATION_RES = fees.FEE_ACCOMMODATION_RES;
                    data.FEE_TRANSPORT = fees.FEE_TRANSPORT;
                    data.FEE_DINNER = fees.FEE_DINNER;
                    data.FEE_ACCOMMODATION_NONRES = fees.FEE_ACCOMMODATION_NONRES;
                    data.FEE_BREAKFAST = fees.FEE_BREAKFAST;
                    data.FEE_TEA = fees.FEE_TEA;
                    data.FEE_OVERNIGHTALLOWANCE = fees.FEE_OVERNIGHTALLOWANCE;

                    // Update the entity and save changes
                    _context.TRAVELLING_AND_SUBSISTENCE_FEES.Update(data);
                    await _context.SaveChangesAsync(userId);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error updating fees: {ex.Message}");
                throw;
            }
        }


        public async Task<TandSAdvanceFees> GetFirstAdvanceFee()
        {
            return await _context.TRAVELLING_AND_SUBSISTENCE_FEES.FirstOrDefaultAsync();
        }
    }
}
