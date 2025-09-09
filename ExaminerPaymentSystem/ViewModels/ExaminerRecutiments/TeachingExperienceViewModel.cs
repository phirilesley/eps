using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.ViewModel.ExaminerRecutiments
{
    public class TeachingExperienceViewModel
    {
        [Required]
        public string LevelTaught { get; set; }
        [Required]

        public string Subject { get; set; }
        [Required]

        public int ExperienceYears { get; set; }
        [Required]

        public string InstitutionName { get; set; }
    }
}
