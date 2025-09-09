using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExaminerRecruitment
{
    public class ExaminerRecruitmentInvitation
    {
        [Key]
        public int Id { get; set; }

        // Foreign key to ExaminerRecruitment (unchanged)
        [ForeignKey("ExaminerRecruitment")]
        public int ExaminerRecruitmentId { get; set; }
        public ExaminerRecruitment ExaminerRecruitment { get; set; }

        // New Foreign key to ExaminerRecruitmentVenueDetails
        [ForeignKey("ExaminerRecruitmentVenueDetails")]
        public int ExaminerRecruitmentVenueDetailsId { get; set; }
        public ExaminerRecruitmentVenueDetails ExaminerRecruitmentVenueDetails { get; set; }

        [Required]
        public bool Attendance { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}
