using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Models.Major;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Data
{
    public class ApplicationUser : IdentityUser
    {
     
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        [Required]
        public string EMS_SUBKEY { get; set; }

  

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        [Required]
        public string ExaminerCode { get; set; }


        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        [Required]
        [ForeignKey("Examiner")]
        public string IDNumber { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
       
        public bool Activated { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(50)")]
        public string? Activity {  get; set; }

        [DisplayName("Role Name")]
        public string? RoleId { get; set; }
        [PersonalData]
        [Column(TypeName = "nvarchar(50)")]
        public string ? Region { get; set; }

       
        public IdentityRole Role { get; set; }

        public Examiner Examiner { get; set; }

        public ExamMonitor ExamMonitor { get; set; }

        //Examiner Recruitment Relationships 

        public ICollection<ExaminerRecruitmentEmailInvitation> Invitations { get; set; }
        // Navigation collections for 1:M
        public ICollection<ExaminerRecruitmentAssessment> CapturerAssessments { get; set; }
        public ICollection<ExaminerRecruitmentAssessment> VerifierAssessments { get; set; }


    }
}
