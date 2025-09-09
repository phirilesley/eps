using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.EntityFrameworkCore;
using System.Web.Mvc;

namespace ExaminerPaymentSystem.Repositories.Common
{
    public class BanksRepository : IBanksRepository
    {
        private readonly ApplicationDbContext _context;
        public BanksRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<BankData>> GetAllBanks()
        {
            var banks = await _context.BANKING_DATA
                .GroupBy(b => new { b.B_BANK_CODE, b.B_BANK_NAME })
                .Select(g => new BankData // Assuming BankData is your entity class
                {
                    B_BANK_CODE = g.Key.B_BANK_CODE,
                    B_BANK_NAME = g.Key.B_BANK_NAME
                }).ToListAsync();

            return banks;
        }

        public async Task<IEnumerable<BankData>> GetAllBanksData()
        {
            return await _context.BANKING_DATA.ToListAsync();
        }


        public async Task<BankData> GetBankById(int id)
        {
            return await _context.BANKING_DATA.FindAsync(id);
        }

        public async Task<bool> DeleteBank(int id)
        {
            var bank = await _context.BANKING_DATA.FindAsync(id);
            if (bank == null)
            {
                return false;
            }

            _context.BANKING_DATA.Remove(bank);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<BankData> GetBankDataByParameter(string bankcode, string branchcode)
        {
            var data = await _context.BANKING_DATA.FirstOrDefaultAsync(e => e.B_BANK_CODE == bankcode && e.BB_BRANCH_CODE == branchcode);
            return data;
        }

        public IEnumerable<BankData> GetBranches(string bankCode)
        {
            var branches = _context.BANKING_DATA
                .Where(b => b.B_BANK_CODE == bankCode)
                .Select(b => new BankData
                {
                    BB_BRANCH_NAME = b.BB_BRANCH_NAME,
                    BB_BRANCH_CODE = b.BB_BRANCH_CODE
                })
                .ToList();

            return branches;
        }

        public async Task<BankData> UpdateBank(BankData updatedBankData)
        {
            var data = await _context.BANKING_DATA
                .FirstOrDefaultAsync(b => b.Id == updatedBankData.Id); // Assuming 'Id' is the primary key

            if (data != null)
            {
                data.BB_BRANCH_CODE = updatedBankData.BB_BRANCH_CODE;
                data.BB_BRANCH_NAME = updatedBankData.BB_BRANCH_NAME;
                data.B_BANK_CODE = updatedBankData.B_BANK_CODE;
                data.B_BANK_NAME = updatedBankData.B_BANK_NAME;
                // ... update other necessary properties

                _context.BANKING_DATA.Update(data);
                await _context.SaveChangesAsync();
            }

            return data;
        }


        public async Task<BankData> SaveBank(BankData model)
        {
            _context.BANKING_DATA.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
