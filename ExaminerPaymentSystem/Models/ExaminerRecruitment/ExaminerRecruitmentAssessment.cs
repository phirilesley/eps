using ExaminerPaymentSystem.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExaminerRecruitment
{
    public class ExaminerRecruitmentAssessment
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key in ExaminerRecruitmentAssessment pointing to ExaminerRecruitment
        public int ExaminerRecruitmentId { get; set; }
        public int? ExaminerRecruitmentRegisterId { get; set; } // New foreign key

        // Navigation property to the related ExaminerRecruitment
        [ForeignKey("ExaminerRecruitmentId")]
        public ExaminerRecruitment? ExaminerRecruitment { get; set; }
        public ExaminerRecruitmentRegister? ExaminerRecruitmentRegister { get; set; } // New navigation property

        [StringLength(2)]
        public string? CapturerGrade { get; set; }

        
        [StringLength(2)]
        public string? VerifierGrade { get; set; }

        public string? CapturerId { get; set; }
        public string? VerifierId { get; set; }

        [ForeignKey("CapturerId")]
        public ApplicationUser? Capturer { get; set; }

        [ForeignKey("VerifierId")]
        public ApplicationUser? Verifier { get; set; }

        public DateTime Date { get; set; }
    }
}
