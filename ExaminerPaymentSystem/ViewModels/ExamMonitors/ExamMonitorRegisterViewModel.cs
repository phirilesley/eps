using ExaminerPaymentSystem.Models.ExamMonitors;

namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{
    public class ExamMonitorRegisterViewModel
    {
        public int Id { get; set; }
        public string SubKey { get; set; }
        public string NationalId { get; set; }
        public string Date { get; set; }
        public string Comment { get; set; }
        public string CompiledStatus { get; set; }
        public string CompiledBy { get; set; }
        public string CompiledDate { get; set; }

        public string ClusterManagerStatus { get; set; }
        public string ClusterManagerBy { get; set; }
        public string ClusterManagerDate { get; set; }

        public string RegionalManagerStatus { get; set; }
        public string RegionalManagerBy { get; set; }
        public string RegionalManagerDate { get; set; }
        public bool IsPresent { get; set; }
        public string IsPresentBy { get; set; }
        public string IsPresentDate { get; set; }

        public ExamMonitor ExamMonitor { get; set; }

        public ExamMonitorTransaction ExamMonitorTransaction { get; set; }

        public ICollection<ExamMonitorRegisterDate> RegisterDates { get; set; }
    }
}
