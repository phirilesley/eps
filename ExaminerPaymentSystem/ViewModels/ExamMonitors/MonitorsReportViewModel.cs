using ExaminerPaymentSystem.Controllers.ExamMonitors;
using ExaminerPaymentSystem.Models.ExamMonitors;

namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{
    public class MonitorsReportViewModel
    {
        public List<MonitorDto> NewApplications { get; set; } = new();
        public List<MonitorDto> AcceptedMonitors { get; set; } = new();
        public List<MonitorDto> RejectedMonitors { get; set; } = new();
        public List<MonitorDto> SelectedMonitors { get; set; } = new();
        public List<MonitorDto> DeployedMonitors { get; set; } = new();

        // Totals
        public int TotalNewApplications { get; set; }
        public int TotalAccepted { get; set; }
        public int TotalRejected { get; set; }
        public int TotalSelected { get; set; }
        public int TotalDeployed { get; set; }
        public int TotalMonitors { get; set; }

        // Filters
        public string RegionFilter { get; set; }
        public string DistrictFilter { get; set; }
        public string StatusFilter { get; set; }
    }

    public class MonitorSummaryDto
    {
        public string NationalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string Status { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Centre { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime? ApplicationDate { get; set; }
        public string AcceptStatus { get; set; }
        public DateTime? AcceptDate { get; set; }
        public string AcceptBy { get; set; }
        public string Comment { get; set; }
        public List<AssignmentInfo> Assignments { get; set; } = new List<AssignmentInfo>();
    }

    public class AssignmentInfo
    {
        public string Phase { get; set; }
        public string Session { get; set; }
        public string CentreAttached { get; set; }
        public string Status { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}
