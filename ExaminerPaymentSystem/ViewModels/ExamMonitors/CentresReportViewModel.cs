using ExaminerPaymentSystem.Models.ExamMonitors;

namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{
    public class CentresReportViewModel
    {
        public List<Centre> AllCentres { get; set; } = new List<Centre>();
        public List<Centre> Clusters { get; set; } = new List<Centre>();
        public List<Centre> Residents { get; set; } = new List<Centre>();
        public List<Centre> ClusterCentres { get; set; } = new List<Centre>();
        public List<Centre> DistrictCentres { get; set; } = new List<Centre>();

        // Statistics
        public int TotalCentres { get; set; }
        public int TotalClusters { get; set; }
        public int TotalResidents { get; set; }
        public int TotalClusterCentres { get; set; }
        public int TotalDistrictCentres { get; set; }

        // Filters
        public string RegionFilter { get; set; }
        public string DistrictFilter { get; set; }
        public string CentreTypeFilter { get; set; }
    }

    public class CentreSummaryDto
    {
        public string CentreNumber { get; set; }
        public string CentreName { get; set; }
        public string RegionCode { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public string ClusterCode { get; set; }
        public string ClusterName { get; set; }
        public string SchoolType { get; set; }
        public string IsResident { get; set; }
        public string IsCluster { get; set; }
        public string Status { get; set; }
    }
}
