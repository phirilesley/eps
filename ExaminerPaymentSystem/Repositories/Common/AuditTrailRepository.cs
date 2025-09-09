using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExaminerPaymentSystem.Repositories.Common
{
    public class AuditTrailRepository : IAuditTrailRepository
    {

        private readonly ApplicationDbContext _context;

        public AuditTrailRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AuditTrail>> AuditTrailGetAll()
        {
            return await _context.AuditTrails.ToListAsync();
        }

        public async Task<AuditTrail> GetAuditTrailById(int Id)
        {
            try
            {
                return await _context.AuditTrails.FirstOrDefaultAsync(x => x.Id == Id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<AuditTrail> GetAuditTrailByNameID(string name)
        {
            try
            {
                return await _context.AuditTrails.FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<AuditTrail> SaveAuditTrail(AuditTrail model)
        {
            _context.AuditTrails.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task UpdateAuditTrail(AuditTrail model)
        {
            var data = await _context.AuditTrails
                 .FirstOrDefaultAsync();
            if (data != null)
            {
             

                _context.AuditTrails.Update(data);
                await _context.SaveChangesAsync();
            }
        }
    }
}
