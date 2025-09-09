using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class ClusterTransaction
    {
        [Key]
        public string SubKey { get; set; }
        public string ClusterCode { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string CentreNumber { get; set; }
        public string IsOALevel { get; set; }
        public string IsGrade7 { get; set; }
        public string IsResidentMonitor { get; set; }
        public string IsClusterManager { get; set; }
        public string IsClusterAssistantManager { get; set; }
    }
}
