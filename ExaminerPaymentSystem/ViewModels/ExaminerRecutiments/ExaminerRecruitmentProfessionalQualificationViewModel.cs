using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.ViewModel.ExaminerRecutiments
{
    public class ExaminerRecruitmentProfessionalQualificationViewModel
    {
        [Required]
        [Display(Name = "Programme Name")]
        public string ProgrammeName { get; set; }
        [Required]
        [Display(Name = "Institution Name")]
        public string InstitutionName { get; set; }
        [Required]
        [Display(Name = "Start Year")]
        public string StartYear { get; set; }
        [Required]
        [Display(Name = "End Year")]
        public string EndYear { get; set; }
    }
}
