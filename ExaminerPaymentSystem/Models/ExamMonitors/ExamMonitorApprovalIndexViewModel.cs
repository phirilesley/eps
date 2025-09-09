namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class ExamMonitorApprovalIndexViewModel
    {
        public string SubKey { get; set; }
        public string Role {  get; set; }
        public string PhaseCode { get; set; }
        public string PhaseName { get; set; }
        public string SessionCode { get; set; }
        public string SessionName { get; set; }
        public string ClusterManagerBy { get; set; }

        public string ClusterManagerStatus { get; set; }

        public string ClusterManagerComment { get; set; }

        public string RegionalManagerStatus { get; set; }

        public string RegionalManagerComment { get; set; }


        public string CompiledStatus { get; set; }

        //
        public string CentreName { get; set; }
        public string ClusterName { get; set; }
        public string CentreAttached { get; set; }

        public string FullName { get; set; }
        public string RegionCode => CentreAttached?.Substring(0, Math.Min(2, CentreAttached?.Length ?? 0));
        public string ClusterCode { get; set; }

        public string currentUserRole { get; set; } 

        public string MonitorStatus { get; set; }
    }
}