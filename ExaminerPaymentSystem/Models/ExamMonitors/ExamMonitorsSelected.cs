using ExaminerPaymentSystem.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class ExamMonitorsRecruitment
    {
        [Key]
        [StringLength(450)]
        [Required]
        [DisplayName("National Id")]
        public string NationalId { get; set; }

        public Guid MonitorId { get; set; }

        [StringLength(60)]
        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [StringLength(60)]
        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }



        [StringLength(1)]
        [Required]
        [DisplayName("Gender")]
        public string? Sex { get; set; }

        [StringLength(60)]
        [DisplayName("Status")]
        public string? Status { get; set; }

        [StringLength(30)]
        [DisplayName("Professional Qualification")]
        public string? Qualification { get; set; }

        [StringLength(60)]
        [DisplayName("Teaching Experience")]
        public string? Experience { get; set; }

        [StringLength(2)]
        [Required]
        [DisplayName("Region")]
        public string Region { get; set; }

        [StringLength(10)]
        [DisplayName("Cell Number")]
        public string? Phone { get; set; }

        [StringLength(3)]
        [Required]
        [DisplayName("Age")]
        public string? Age { get; set; }

        [StringLength(6)]
        [Required]
        [DisplayName("Centre Number")]
        public string Centre { get; set; }

        [StringLength(60)]
        [DisplayName("Station")]
        public string? Station { get; set; }

        [StringLength(60)]
        [DisplayName("District")]
        public string? District { get; set; }

        public string? AcceptStatus { get; set; }
        public string? AcceptBy { get; set; }
        public DateTime AcceptDate { get; set; }
        public string? Comment { get; set; }
        public string? BankCodeZwg { get; set; }
        public string? BankNameZwg { get; set; }
        public string? BankBranchCodeZwg { get; set; }
        public string? BranchZwg { get; set; }

        public string? AccountNumberZwg { get; set; }

        public string? BankCodeUsd { get; set; }
        public string? BankNameUsd { get; set; }
        public string? BankBranchCodeUsd { get; set; }
        public string? BranchUsd { get; set; }

        public string? AccountNumberUsd { get; set; }
        public string? Email { get; set; }
        public string? Score { get; set; }

        public ICollection<ApplicationUser> ApplicationUsers { get; set; }

        public ICollection<ExamMonitorRegister> ExamMonitorRegisters { get; set; }

        public ICollection<ExamMonitorTransaction> ExamMonitorTransactions { get; set; }
        public ICollection<ExamMonitorTandS> ExamMonitorTandSs { get; set; }

        public ICollection<ExamMonitorProfessionalQualifications> ProfessionalQualifications { get; set; }
        public ICollection<ExamMonitorAttachements> Attachments { get; set; }
        public ICollection<ExamMonitorEmailInvitation> EmailInvitations { get; set; }
    }
}
