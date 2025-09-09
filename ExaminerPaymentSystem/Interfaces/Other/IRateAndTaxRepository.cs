using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IRateAndTaxRepository
    {
        Task UpdateRate(RateAndTax rate, string userId);


        Task<RateAndTax> GetFirstTaxAndRate();

        Task<List<SettingRate>> GetAllSettingRatesAsync();
        Task<SettingRate> GetSettingRateBySubjectAsync(string examCode, string subjectCode, string paperCode);
        Task AddSettingRateAsync(SettingRate settingRate,string userId);
        Task UpdateSettingRateAsync(SettingRate settingRate,string userId);
        Task DeleteSettingRateAsync(string examCode, string subjectCode, string paperCode,string userId);


        Task<List<ActivityRate>> GetAllActivityRatesAsync();
        Task<ActivityRate> GetActivityRateBySubjectAsync(string activity);
        Task AddActivityRateAsync(ActivityRate rate, string userId);
        Task UpdateActivityRateAsync(ActivityRate rate, string userId);
        Task DeleteActivityRateAsync(string activity,string userId);

    }
}
