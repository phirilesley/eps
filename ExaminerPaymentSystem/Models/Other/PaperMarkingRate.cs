using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class PaperMarkingRate
    {
        [Key]
        public string? SUB_SUB_ID { get; set; }
        public string? PPR_PAPER_CODE { get; set; }
        public string? PPR_PAPER_DESC { get; set; }
        public string? PPR_MARKING_RATE { get; set; }
        public string? PPR_EXAM_TYPE { get; set; }

        public string? SUB_SUBJECT_CODE { get; set; }

        public string? SUB_SUBJECT_DESC { get; set; }


    }
}










