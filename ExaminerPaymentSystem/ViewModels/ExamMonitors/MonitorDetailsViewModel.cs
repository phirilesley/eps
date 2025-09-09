using ExaminerPaymentSystem.Models.ExamMonitors;

namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{
    public class MonitorDetailsViewModel
    {
        // Common properties
        public string NationalId { get; set; }
        public Guid MonitorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Sex { get; set; }
        public string? Status { get; set; }
        public string? Qualification { get; set; }
        public string? Experience { get; set; }
        public string Region { get; set; }
        public string? Phone { get; set; }
        public string? Age { get; set; }
        public string Centre { get; set; }
        public string? Station { get; set; }
        public string? District { get; set; }
        public string? Email { get; set; }
        public string? AcceptStatus { get; set; }
        public string? AcceptBy { get; set; }
        public DateTime AcceptDate { get; set; }
        public string? Comment { get; set; }

        // Collections
        public ICollection<ExamMonitorProfessionalQualifications> ProfessionalQualifications { get; set; }
        public ICollection<ExamMonitorAttachements> Attachments { get; set; }
        public ICollection<ExamMonitorEmailInvitation> EmailInvitations { get; set; }

        // Source tracking
        public string SourceTable { get; set; } // "Recruitment" or "Monitor"
    }
}
