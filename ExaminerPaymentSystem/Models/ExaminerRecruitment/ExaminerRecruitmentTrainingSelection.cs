using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExaminerRecruitment
{
    public class ExaminerRecruitmentTrainingSelection
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ExaminerRecruitment")]
        public int ExaminerRecruitmentId { get; set; }

        [ValidateNever]
        public ExaminerRecruitment ExaminerRecruitment { get; set; }

        [Required]
        public bool Status { get; set; }
        public DateTime Date { get; set; }
    }
}
