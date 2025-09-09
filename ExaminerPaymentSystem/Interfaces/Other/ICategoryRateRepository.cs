using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface ICategoryRateRepository
    {
        Task<IEnumerable<CategoryRate>> GetAllExamTypes();
        Task<IEnumerable<CategoryRate>> GetCategoryRatesByExamType(string examType);


        Task<IEnumerable<CategoryRate>> GetCategoryRatesByExamType(string examType, string examinercategorycode);
        Task<IEnumerable<CategoryRate>> GetExamTypesByExaminerCategoryCode(string examinercategorycode);


        Task UpdateCategoryMarkingRate(CategoryRate categoryrate);
        Task UpdateCategoryMarkingRates(List<CategoryRate> rates, string CAT_CODE, string PPR_EXAM_TYPE);

        Task<CategoryRate> GetCategoryMarkingRate(string examType, string examinercategorycode);

        Task<IEnumerable<CategoryRate>> GetAllExaminerCodes();
        Task<CategoryRate> GetExaminerCodesById(string examinercode);


    }
}
