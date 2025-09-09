using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class Centre
    {
        [Key]
        public string CentreNumber { get; set; }
        public string CentreName { get; set; }
        public string RegionCode { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }

        public string ClusterCode { get; set; }
        public string ClusterName { get; set; }
        public int DistrictId { get; set; }  
        public string SchoolType { get; set; }

        public string IsResident { get; set; }
        public string IsCluster { get; set; }
        

        public District District { get; set; }
    }
}
