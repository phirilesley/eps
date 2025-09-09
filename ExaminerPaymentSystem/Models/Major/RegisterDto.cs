using ExaminerPaymentSystem.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.Major
{
    public class RegisterDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ExaminerCode { get; set; }

        [ForeignKey("Examiner")]
        public string? IDNumber { get; set; }
        [Required]
        public string EMS_SUBKEY { get; set; }

        public string? Status { get; set; }

        public string? RecommendedBy { get; set; }

        public string? RecommendedDate { get; set; }

        public string? RecommendedStatus { get; set; }

        public string? AttendanceStatus { get; set; }

        public string? AttendanceStatusBy { get; set; }

        public string? AttendanceUpdateDate { get; set; }

        public string? StatusDate { get; set; }

        public Examiner Examiner { get; set; }



    }

    public class RegisterViewModel
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Subject { get; set; }
        public string? ExaminerNumber { get; set; }

        public string? PaperCode { get; set; }

        public string? Category { get; set; }

        public string? ExaminerCode { get; set; }
        public string? IdNumber { get; set; }
        public string? SubKey { get; set; }

        public string? Status { get; set; }

        public string? RecommendedBy { get; set; }

        public string? RecommendedDate { get; set; }

        public string? RecommendedStatus { get; set; }

        public string? AttendanceStatus { get; set; }

        public string? AttendanceStatusBy { get; set; }

        public string? AttendanceUpdateDate { get; set; }

        public string? StatusDate { get; set; }

    }
}
