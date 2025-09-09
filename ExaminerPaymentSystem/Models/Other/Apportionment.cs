using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class Apportionment

    {
        [Key]
        public int Id { get; set; }

        public string MKS_SUBJECT_CODE { get; set; }

        public string SUB_SUBJECT_DESC { get; set; }

        public string MKS_PAPER_CODE { get; set; }

        public string PPR_PAPER_DESC { get; set; }

        public string NUMBER_OF_CANDIDATES { get; set; }

    }
}
