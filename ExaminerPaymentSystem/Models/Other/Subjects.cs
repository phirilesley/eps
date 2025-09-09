using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class Subjects
    {
        [Key]
        //public int Id { get; set; } 
        public string SUB_SUB_ID { get; set; }

        public string SUB_SUBJECT_CODE { get; set; }

        public string SUB_SUBJECT_DESC { get; set; }

        //public virtual PaperMarkingRate PPR_SUB_SUB_ID { get; set; }

    }
}
