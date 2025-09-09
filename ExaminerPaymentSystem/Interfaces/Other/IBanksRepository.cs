using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IBanksRepository
    {
        Task<BankData> SaveBank(BankData model);
        Task<BankData> UpdateBank(BankData updatedBankData);
        Task<IEnumerable<BankData>> GetAllBanks();
        Task<BankData> GetBankById(int id);
        Task<IEnumerable<BankData>> GetAllBanksData();

        Task<BankData> GetBankDataByParameter(string bankcode, string branchcode);

        Task<bool> DeleteBank(int id);
    }
}
