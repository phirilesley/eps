using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class CategoryMarkingRateViewModel
    {
        public int Id { get; set; }
        public string ExaminerCategoryCode { get; set; }
        public string ExaminerDescription { get; set; }
        public string HighestLevel { get; set; }
    }
}
