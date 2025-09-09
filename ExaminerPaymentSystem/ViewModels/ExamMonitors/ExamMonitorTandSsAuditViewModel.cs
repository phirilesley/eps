using ExaminerPaymentSystem.Models.ExamMonitors;

namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{

    public class ExamMonitorTandSsAuditViewModel
    {
        // Original claim information
        public ExamMonitorTandSsViewModel OriginalClaim { get; set; }

        // Adjusted claim information
        public ExamMonitorTandSsViewModel AdjustedClaim { get; set; }

        // Audit information
        public string RoleStatus { get; set; }
        public Guid ClaimID { get; set; }
        public string SubKey { get; set; }
        public string NationalId { get; set; }
        public string Region { get; set; }
        public decimal Rate { get; set; }
        public string AuditorName { get; set; }
        public DateTime AuditDate { get; set; }
        public string AuditComments { get; set; }

        public string Session { get; set; }

        public string PhaseCode { get; set; }

        public string CentreAttached { get; set; }

        // Status flags
        public bool IsEditMode { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
    }

}
