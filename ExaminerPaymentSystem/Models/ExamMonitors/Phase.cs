using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class Phase
    {
        [Key]
        public int Id { get; set; }
        public string PhaseCode { get; set; }
        public string PhaseName { get; set; }
        public string PhaseYear { get; set; }
        public string SessionCode { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
        public decimal Rate { get; set; }
        public decimal ClusterManagerRate { get; set; }
        public decimal AssistantClusterManagerRate { get; set; }

        public decimal ResidentMonitorRate { get; set; }
        public decimal DinnerRate { get; set; }
        public decimal AccomodationRate { get; set; }
        public decimal LunchRate { get; set; }
        public decimal BreakFastRate { get; set; }
        public string LevelCode { get; set; }

        public Level Level { get; set; }
    }
}
