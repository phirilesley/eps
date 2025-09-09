using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class ExamMonitorTransaction
    {
        [Key]
        public string SubKey { get; set; }
        public string Status { get; set; }
        public Guid MonitorId { get; set; }
        public string NationalId { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Session { get; set; }
        public string Phase { get; set; }
        public string CentreAttached { get; set; }
        public DateTime AssignedDate { get; set; }
        public string? AssignedBy { get; set; }

        public string? AssignedStatus { get; set; }

        public string? DeployStatus { get; set; }
        public DateTime DeployStatusDate { get; set; }
        public string?   DeployStatusBy { get; set; }
        public ExamMonitor ExamMonitor { get; set; }
        public ICollection<ExamMonitorRegister> ExamMonitorRegisters { get; set; }

        

    }
}
