using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class District
    {
        [Key]
        public int Id { get; set; }
        public string DistrictName { get; set; }
        public string DistrictCode { get; set; }

        public string RegionCode { get; set; }

    }
}
