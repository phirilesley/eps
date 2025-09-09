namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{
    public class ClusterReportViewModel
    {
        public string SearchTerm { get; set; }
        public string RegionFilter { get; set; }
        public string DistrictFilter { get; set; }
        public string PhaseFilter { get; set; }
        public string SessionFilter { get; set; }

        public List<ClusterAssignment> Assignments { get; set; }
        public List<FilterOption> Regions { get; set; }
        public List<FilterOption> Districts { get; set; }
        public List<FilterOption> Phases { get; set; }
        public List<FilterOption> Sessions { get; set; }

        // Statistics
        public int TotalClusters { get; set; }
        public int TotalIsResidents { get; set; }

        public int TotalCentres { get; set; }
        public int AssignedCentres { get; set; }
        public int UnassignedCentres { get; set; }
    }

    public class ClusterAssignment
    {
        public string ClusterCode { get; set; }
        public string ClusterName { get; set; }
        public string CentreNumber { get; set; }
        public string CentreName { get; set; }
        public string CentreType { get; set; }
        public string ClusterManager { get; set; }
        public string AssistantClusterManager { get; set; }
        public string ResidentMonitor { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Phase { get; set; }
        public string Session { get; set; }
        public string Status { get; set; }
        public DateTime? AssignedDate { get; set; }
    }


    public class ClusterAssignmentStatus
    {
        public string ClusterName { get; set; }
        public string CentreNumber { get; set; }
        public string CentreName { get; set; }
        public string CentreType { get; set; }
    
        public string Region { get; set; }
        public string District { get; set; }
        public string Phase { get; set; }
        public string Session { get; set; }
        public string Status { get; set; }
        public DateTime? AssignedDate { get; set; }
    }


    public class FilterOption
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
}
