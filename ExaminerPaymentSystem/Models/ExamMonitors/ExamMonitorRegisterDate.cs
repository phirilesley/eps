using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class ExamMonitorRegisterDate
    {
        [Key]
        public int Id { get; set; }
        public string SubKey { get; set; } // Foreign key to ExamMonitorRegister
        public DateTime Date { get; set; }
        public string Comment { get; set; }

        // Navigation property
        public ExamMonitorRegister Register { get; set; }

        [ForeignKey(nameof(SubKey))]
        public ExamMonitorTransaction Transaction { get; set; }




        // Approval properties
        public string CompiledStatus { get; set; } = "Pending";
        public string? CompiledBy { get; set; }
        public DateTime? CompiledDate { get; set; }

        public string ClusterManagerStatus { get; set; } = "Pending";
        public string? ClusterManagerBy { get; set; }
        public DateTime? ClusterManagerDate { get; set; }

        public string? ClusterManagerComment { get; set; }

        public string RegionalManagerStatus { get; set; } = "Pending";
        public string? RegionalManagerBy { get; set; }
        public DateTime? RegionalManagerDate { get; set; }

        public string? RegionalManagerComment { get; set; }

        public bool IsPresent { get; set; }


        // 🔹 NEW for supervisor extra days
        public bool IsSupervisorAdded { get; set; } = false;
        public string? SupervisorStatus { get; set; }
        public string? SupervisorComment { get; set; }
        public string? SupervisorBy { get; set; }
        public DateTime? SupervisorDate { get; set; }

        // NEW: Distinguish between auto and manual entries
        public bool IsFromTimetable { get; set; } = true;


    }
}
