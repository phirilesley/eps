using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class BrailleTranscriptionRate
    {
        [Key]
        public int Id { get; set; }
        public decimal? Grade7 { get; set; } // Nullable decimal
        public decimal? OLevel { get; set; } // Nullable decimal
        public decimal? ALevel { get; set; } // Nullable decimal
    }

}
