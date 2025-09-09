using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class ExamCodes
    {
        [Key]
        public int ID { get; set; }

        public string? EXM_EXAM_CODE { get; set; }

        public string? EXM_EXAM_SESSION { get; set; }

        public string? EXM_EXAM_YEAR { get; set; }

        public string? EXM_START_DATE { get; set; }

        public string? EXM_EXAM_LEVEL { get; set; }

        public string? EXM_STATUS { get; set; }

        public string? EXM_CLOSE_DATE { get; set; }

        public string? ACTIVATED_SESSION { get; set; }

    }
}
