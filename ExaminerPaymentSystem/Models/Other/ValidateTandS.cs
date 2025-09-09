using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class ValidateTandS
    {
        [Key]
        public int Id { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        public string? ExaminerCategory { get; set; }

        public string? NumberOfDays { get; set; }

        public string? Venue { get; set; }

        public string? ExamCode { get; set; }

        public string? SubjectCode { get; set; }

        public string? PaperName { get; set; }

        public string? CompiledBy { get; set; }

        public string? CompiledDate { get; set; }

    }
}
