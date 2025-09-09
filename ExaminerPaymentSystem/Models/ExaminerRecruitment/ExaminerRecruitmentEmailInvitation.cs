using ExaminerPaymentSystem.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExaminerRecruitment
{
    public class ExaminerRecruitmentEmailInvitation
    {

        [Key]
        public int Id { get; set; }

        [ForeignKey("ExaminerRecruitment")]
        public int ExaminerRecruitmentId { get; set; }

        public ExaminerRecruitment ExaminerRecruitment { get; set; }


        [ForeignKey("InvitedByUser")]
        public string? InvitedBy { get; set; }  // Identity User ID (string in ASP.NET Identity)

        public ApplicationUser InvitedByUser { get; set; } // Navigation property

        public DateTime DateInvited { get; set; } = DateTime.Now;
        
    }
}
