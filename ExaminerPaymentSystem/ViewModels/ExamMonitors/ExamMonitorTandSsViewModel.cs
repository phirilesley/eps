using ExaminerPaymentSystem.Models.ExamMonitors;

namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{
    public class ExamMonitorTandSsViewModel
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Region { get; set; }
        public string Station {  get; set; }
        public string District { get; set; }
        public string RoleStatus { get; set; }
        public Guid ClaimID { get; set; }
        public string SubKey { get; set; }
        public string NationalId { get; set; }

        public string BankNameZwg { get; set; }

        public string BranchZwg { get; set; }

        public string AccountNumberZwg { get; set; }


        public string BankNameUsd { get; set; }

        public string BranchUsd { get; set; }

        public string AccountNumberUsd { get; set; }

        public string Session { get; set; }

        public string PhaseCode { get; set; }

        public string CentreAttached { get; set; }

        public string Date { get; set; }
        public string ClusterManagerStatus { get; set; }
        public string ClusterManagerStatusBy { get; set; }
        public DateTime ClusterManagerStatusDate { get; set; }

        public string RegionalManagerStatus { get; set; }
        public string RegionalManagerStatusBy { get; set; }
        public DateTime RegionalManagerStatusDate { get; set; }

        public string? PaidStatus { get; set; }
        public string? PaidStatusBy { get; set; }
        public string? PaidStatusDate { get; set; }
        public string? PaidStatusComment { get; set; }

        public decimal? PaidAmount { get; set; }
        public decimal Rate { get; set; }
        public decimal DinnerRate { get; set; }
        public decimal AccomodationRate { get; set; }
        public decimal LunchRate { get; set; }
        public decimal BreakFastRate { get; set; }
        public bool IsEditEnabled { get; set; } // Set by supervisor
        public string CurrentStatus { get; set; } // e.g., "Submitted", "Approved", "Rejected"

        public int Days { get; set; }
        public int AdjustedDays { get; set; }

        public decimal ClusterManagerRate { get; set; }
        public decimal AssistantClusterManagerRate { get; set; }

        public decimal ResidentMonitorRate { get; set; }
        public List<DailyAdvances> DailyAdvances { get; set; }
    }
}
