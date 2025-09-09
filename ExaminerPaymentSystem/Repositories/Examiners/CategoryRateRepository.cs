using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExaminerPaymentSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;


namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class CategoryRateRepository : ICategoryRateRepository
    {
        private readonly ApplicationDbContext _categorycodecontext;

        public CategoryRateRepository(ApplicationDbContext categorycodecontext)
        {
            _categorycodecontext = categorycodecontext;
        }


        //toddyc get exam types
        public async Task<IEnumerable<CategoryRate>> GetAllExamTypes()
        {

            return await _categorycodecontext.EXM_CATEGORY_MARKING_RATE.ToListAsync();
        }

        public async Task<IEnumerable<CategoryRate>> GetCategoryRatesByExamType(string examType)
        {
            return await _categorycodecontext.EXM_CATEGORY_MARKING_RATE.Where(e => e.PPR_EXAM_TYPE == examType).ToListAsync();
        }

        //examinercodes

        public async Task<IEnumerable<CategoryRate>> GetAllExaminerCodes()
        {
            return await _categorycodecontext.EXM_CATEGORY_MARKING_RATE.ToListAsync();
        }

        public async Task<CategoryRate> GetExaminerCodesById(string examinercode)
        {
            var data = await _categorycodecontext.EXM_CATEGORY_MARKING_RATE.FirstOrDefaultAsync(e => e.EMS_ECT_EXAMINER_CAT_CODE == examinercode);
            return data;
        }

        public async Task<IEnumerable<CategoryRate>> GetExamTypesByExaminerCategoryCode(string examinercategorycode)
        {
            // Null check for examcode
            if (examinercategorycode != null && examinercategorycode.Length < 5)
            {
                // Extract the exam session substring
                //string examSessionSubstring = examcode.Substring(0, examcode.Length-1);
                //string examSessionSubString = PPR_SUB_SUB_ID.Substring(PPR_SUB_SUB_ID.Length - 4, PPR_SUB_SUB_ID.Length - 1);

                // Fetch subjects based on the extracted exam session substring
                var examtypes = await _categorycodecontext.EXM_CATEGORY_MARKING_RATE
                //    .Where(s => s.SUB_SUB_ID.StartsWith(examcode))
                //    .OrderBy(s => s.SUB_SUBJECT_CODE)
                //    .Distinct()
                //    .ToListAsync();

                //return subjects;
                .Where(s => s.EMS_ECT_EXAMINER_CAT_CODE == examinercategorycode)
                      .Select(s => new CategoryRate()
                      {
                          PPR_EXAM_TYPE = s.PPR_EXAM_TYPE,
                          MOD_FEES = s.MOD_FEES,
                          COORD_FEES = s.COORD_FEES,
                          EMS_ECT_EXAMINER_CAT_CODE = s.EMS_ECT_EXAMINER_CAT_CODE,
                          NAT_REP_ALLOWANCE = s.NAT_REP_ALLOWANCE,
                          SUPER_FEES = s.SUPER_FEES,
                          ID = s.ID,

                          // Add other properties you need
                      })
                      .Distinct()
                      .OrderBy(s => s.ID)
                      .ToListAsync();

                return examtypes;
            }
            else
            {
                // If examcode is null or its length is less than 4, return an empty list
                return new List<CategoryRate>();
            }
        }

        public async Task UpdateCategoryMarkingRates(List<CategoryRate> rates, string CAT_CODE, string PPR_EXAM_TYPE)
        {

            foreach (var item in rates)
            {

                var categoryToUpdate = await _categorycodecontext.EXM_CATEGORY_MARKING_RATE
                    .FirstOrDefaultAsync(e => e.EMS_ECT_EXAMINER_CAT_CODE == CAT_CODE && e.PPR_EXAM_TYPE == PPR_EXAM_TYPE);
                if (categoryToUpdate != null)
                {
                    categoryToUpdate.MOD_FEES = item.MOD_FEES;
                    categoryToUpdate.COORD_FEES = item.COORD_FEES;
                    categoryToUpdate.SUPER_FEES = item.SUPER_FEES;
                    categoryToUpdate.NAT_REP_ALLOWANCE = item.NAT_REP_ALLOWANCE;

                    _categorycodecontext.EXM_CATEGORY_MARKING_RATE.Update(categoryToUpdate);
                    await _categorycodecontext.SaveChangesAsync();
                }



            }
        }


        public async Task UpdateCategoryMarkingRate(CategoryRate updatedCategoryRates)
        {

            var categoryToUpdate = await _categorycodecontext.EXM_CATEGORY_MARKING_RATE
                .FirstOrDefaultAsync(e => e.ID == updatedCategoryRates.ID);

            if (categoryToUpdate == null)
            {

                throw new Exception("Category not found");
            }

            foreach (var propertyInfo in typeof(CategoryRate).GetProperties())
            {
                if (propertyInfo.CanWrite && propertyInfo.Name != "Id")
                {
                    var updatedValue = typeof(CategoryRate).GetProperty(propertyInfo.Name)?.GetValue(updatedCategoryRates);
                    if (updatedValue != null)
                    {
                        propertyInfo.SetValue(categoryToUpdate, updatedValue);
                    }
                }
            }



            // Set the EntityState to Modified to indicate that the entity has been modified
            _categorycodecontext.Entry(categoryToUpdate).State = EntityState.Modified;

            // Save changes to the database
            await _categorycodecontext.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryRate>> GetCategoryRatesByExamType(string examType, string examinercategorycode)
        {
            // Null check for examinercategorycode
            //if (subject != null && subject.Length < 5)
            //{
            // Extract the exam session substring
            //string examSessionSubstring = examinercategorycode.Substring(0, examinercategorycode.Length-1);
            //string examSessionSubString = PPR_SUB_SUB_ID.Substring(PPR_SUB_SUB_ID.Length - 4, PPR_SUB_SUB_ID.Length - 1);

            // Fetch subjects based on the extracted exam session substring
            var examtypes = await _categorycodecontext.EXM_CATEGORY_MARKING_RATE
                  .Where(s => s.PPR_EXAM_TYPE == examType && s.EMS_ECT_EXAMINER_CAT_CODE == examinercategorycode)
                  .Select(s => new CategoryRate()
                  {

                      ID = s.ID,
                      MOD_FEES = s.MOD_FEES,
                      SUPER_FEES = s.SUPER_FEES,
                      EMS_ECT_EXAMINER_CAT_CODE = s.EMS_ECT_EXAMINER_CAT_CODE,
                      PPR_EXAM_TYPE = s.PPR_EXAM_TYPE,
                      COORD_FEES = s.COORD_FEES,
                      NAT_REP_ALLOWANCE = s.NAT_REP_ALLOWANCE,
                      // Add other properties you need
                  })
                  .Distinct()
                  .OrderBy(s => s.ID)
                  .ToListAsync();

            return examtypes;
            // }
            //else
            ////{

            ////    return new List<PaperMarkingRate>();
            ////}
        }

        public async Task<CategoryRate> GetCategoryMarkingRate(string examType, string examinercategorycode)
        {
            var data = await _categorycodecontext.EXM_CATEGORY_MARKING_RATE.FirstOrDefaultAsync(e => e.PPR_EXAM_TYPE == examType && e.EMS_ECT_EXAMINER_CAT_CODE == examinercategorycode);
            return data;
        }
    }
}
