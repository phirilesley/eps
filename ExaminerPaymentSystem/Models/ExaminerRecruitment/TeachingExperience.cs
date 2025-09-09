using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExaminerRecruitment
{
    public class TeachingExperience
    {
        public int Id { get; set; }

        // Foreign Key to PersonalDetails
        [ForeignKey("ExaminerRecruitment")]
        public int ExaminerRecruitmentId { get; set; }

        public string? LevelTaught { get; set; }
        public string? Subject { get; set; }
        public int? ExperienceYears { get; set; }
        public string? InstitutionName { get; set; }

        // Navigation Property
        public ExaminerRecruitment ExaminerRecruitment { get; set; }
    }
}
