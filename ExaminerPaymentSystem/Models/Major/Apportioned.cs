using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Major
{
    public class Apportioned
    {
        [Key]
        public int Id { get; set; }
        public string? CTP_PPR_SUB_PAPER_CODE { get; set; } 
        public string? CTP_ECT_CAT_CODE { get; set; } 
        public int? CTP_MAX_SCRIPTS { get; set; }
        public string? CTP_REGION_CODE { get; set; } 
    }
}
