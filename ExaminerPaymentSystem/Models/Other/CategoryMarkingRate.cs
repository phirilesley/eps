using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class CategoryMarkingRate
    {
        [Key]
        public int Id { get; set; }
        public string? ECT_EXAMINER_CAT_CODE { get; set; }
        public string? ECT_EXAMINER_DESC { get; set; }
        public string? ECT_H_LEVEL { get; set; }
    }
}
