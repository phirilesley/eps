using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class CategoryRateViewModel
    {
        [Key]
        public int ID { get; set; }
        public string? PPR_EXAM_TYPE { get; set; }

        public string? EMS_ECT_EXAMINER_CAT_CODE { get; set; }

        public string? NAT_REP_ALLOWANCE { get; set; }

        public string? COORD_FEES { get; set; }

        public string? MOD_FEES { get; set; }

        public string? SUPER_FEES { get; set; }


    }
}










