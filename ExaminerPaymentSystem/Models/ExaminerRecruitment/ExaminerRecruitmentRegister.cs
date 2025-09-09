using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExaminerRecruitment
{
    public class ExaminerRecruitmentRegister
    {
        [Key]
        public int Id { get; set; }

        // Foreign key to ExaminerRecruitment
        [ForeignKey("ExaminerRecruitment")]
        public int ExaminerRecruitmentId { get; set; }

        [ValidateNever]
        public ExaminerRecruitment ExaminerRecruitment { get; set; }
        public ExaminerRecruitmentAssessment ExaminerRecruitmentAssessment { get; set; } // New navigation property

        public bool? Status { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }

}
