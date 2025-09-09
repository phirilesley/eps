using System.ComponentModel.DataAnnotations;
using ExaminerPaymentSystem.Models.Major;

namespace ExaminerPaymentSystem.Models.Other
{
    public class ExaminerSubject
    {
        [Key]
        public string? EMS_NATIONAL_ID { get; set; }
        public string EMS_EXAMINER_CODE { get; set; }
        public string? EMS_SUBKEY { get; set; }
        public string? EMS_SUB_SUB_ID { get; set; }

        public string? EMS_PAPER_CODE { get; set; }

        public Examiner Examiner { get; set; }

    }
}
