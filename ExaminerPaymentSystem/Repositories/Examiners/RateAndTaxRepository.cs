using DocumentFormat.OpenXml.Spreadsheet;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class RateAndTaxRepository : IRateAndTaxRepository
    {

        private readonly ApplicationDbContext _context;

        public RateAndTaxRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAdvanceFees(RateAndTax rate)
        {
            _context.RATE_AND_TAX_INFO.Add(rate);
            await _context.SaveChangesAsync();

        }

        public async Task<RateAndTax> GetFirstAdvanceFee()
        {
            return await _context.RATE_AND_TAX_INFO.FirstOrDefaultAsync();
        }

        public async Task<RateAndTax> GetFirstTaxAndRate()
        {
            return await _context.RATE_AND_TAX_INFO.FirstOrDefaultAsync();
        }

        public async Task UpdateRate(RateAndTax rate,string userId)
        {
            try
            {
                var data = await _context.RATE_AND_TAX_INFO
               .FirstOrDefaultAsync();
                if (data != null)
                {
                    data.WHT = rate.WHT;
                    data.CurrentRate = rate.CurrentRate;

                    _context.RATE_AND_TAX_INFO.Update(data);
                    await _context.SaveChangesAsync(userId);
                }
            }
            catch (Exception)
            {

                throw;
            }
        
        }


        public async Task<List<SettingRate>> GetAllSettingRatesAsync()
        {
            return await _context.SettingRates.ToListAsync();
        }

        public async Task<SettingRate> GetSettingRateBySubjectAsync(string examCode, string subjectCode, string paperCode)
        {
            return await _context.SettingRates
                .FirstOrDefaultAsync(x => x.SubSubId == examCode + subjectCode && x.PaperCode == paperCode);
        }

        public async Task AddSettingRateAsync(SettingRate settingRate, string userId)
        {
            _context.SettingRates.Add(settingRate);
            await _context.SaveChangesAsync(userId);
        }

        public async Task UpdateSettingRateAsync(SettingRate settingRate, string userId)
        {
            var existing = await _context.SettingRates
                .FirstOrDefaultAsync(s =>
                    s.SubSubId == settingRate.SubSubId &&
                    
                    s.PaperCode == settingRate.PaperCode);

            if (existing != null)
            {
                // Update fields
                existing.SettingFee = settingRate.SettingFee;
         

                _context.SettingRates.Update(existing);
            }
            else
            {
        

                await _context.SettingRates.AddAsync(settingRate);
            }

            await _context.SaveChangesAsync(userId);
        }


        public async Task DeleteSettingRateAsync(string examCode, string subjectCode, string paperCode, string userId)
        {
            var existing = await _context.SettingRates
                .FirstOrDefaultAsync(x => x.SubSubId == examCode+ subjectCode && x.PaperCode == paperCode);

            if (existing != null)
            {
                _context.SettingRates.Remove(existing);
                await _context.SaveChangesAsync(userId);
            }
        }

        public async Task<List<ActivityRate>> GetAllActivityRatesAsync()
        {
            return await _context.ActivityRates.ToListAsync();
        }

        public async Task<ActivityRate> GetActivityRateBySubjectAsync(string activity)
        {
            return await _context.ActivityRates
              .FirstOrDefaultAsync(x => x.Activity == activity);
        }

        public async Task AddActivityRateAsync(ActivityRate rate, string userId)
        {
            _context.ActivityRates.Add(rate);
            await _context.SaveChangesAsync(userId);
        }

        public async Task UpdateActivityRateAsync(ActivityRate rate, string userId)
        {
            _context.ActivityRates.Update(rate);
            await _context.SaveChangesAsync(userId);
        }

        public async Task DeleteActivityRateAsync(string activity,string userId)
        {
            var existing = await _context.SettingRates
                .FirstOrDefaultAsync(x => x.SubSubId == activity);

            if (existing != null)
            {
                _context.SettingRates.Remove(existing);
                await _context.SaveChangesAsync(userId);
            }
        }
    }
}
