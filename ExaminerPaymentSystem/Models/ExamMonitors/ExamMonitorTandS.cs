using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Major;
using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class ExamMonitorTandS
    {
        [Key]
        public Guid ClaimID { get; set; }
        public string SubKey { get; set; }
        public string NationalId { get; set; }
        
        public string Region { get; set; }

        public string Session { get; set; }

        public string Phase { get; set; }

        public string CentreAttached { get; set; }
        public string Date { get; set; }
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        public int Days { get; set; }
        public int AjustedDays { get; set; }
        public string ClusterManagerStatus { get; set; }
        public string ClusterManagerStatusBy { get; set; }
        public DateTime ClusterManagerStatusDate { get; set; }

        public string RegionalManagerStatus { get; set; }
        public string RegionalManagerStatusBy { get; set; }
         public DateTime RegionalManagerStatusDate { get; set; }

        public string InitiatorStatus { get; set; }
        public string InitiatorStatusBy { get; set; }
        public DateTime InitiatorStatusDate { get; set; }
        public string ReviewStatus { get; set; }
        public string ReviewStatusBy { get; set; }
        public DateTime ReviewStatusDate { get; set; }
        public string? PaidStatus { get; set; }
        public string? PaidStatusBy { get; set; }
        public string? PaidStatusDate { get; set; }
        public string? PaidStatusComment { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAdjustedAmount { get; set; }
        public decimal? PaidAmount { get; set; }

        public ExamMonitorTransaction ExamMonitorTransaction { get; set; }
        public ExamMonitor ExamMonitor { get; set; }
        public StipendAdvance StipendAdvance { get; set; }

        public ICollection<DailyAdvances> DailyAdvances { get; set; } = new List<DailyAdvances>();

    }

    public class DailyAdvances 
    {
        [Key]
        public int Id { get; set; }
        public string SubKey { get; set; }
        public Guid ClaimID { get; set; }
        public DateTime Date { get; set; }

        public decimal Lunch { get; set; }
        public decimal AdjustedLunch { get; set; }
        public decimal Breakfast { get; set; }
        public decimal AdjustedBreakfast { get; set; }
        public decimal Accomodation { get; set; }
        public decimal AdjustedAccomodation { get; set; }
        public decimal Dinner { get; set; }
        public decimal AdjustedDinner { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAdjustedAmount { get; set; }

        public ExamMonitorTandS Claim { get; set; }
    }

    public class StipendAdvance
    {
        [Key]
        public int Id { get; set; }
        public string SubKey { get; set; }
        public Guid ClaimID { get; set; }
        public decimal Rate { get; set; }

        public string PhaseCode { get; set; }

        public int Days { get; set; }
        public int AdjustedDays { get; set; }

        public decimal? TotalAmount { get; set; }
        public decimal? TotalAdjustedAmount { get; set; }
        public ExamMonitorTandS Claim { get; set; }

    }
}
