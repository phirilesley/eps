using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExaminerRecruitment
{
    public class ProfessionalQualifications
        
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ExaminerRecruitment")]
        public int ExaminerRecruitmentId { get; set; }
        public ExaminerRecruitment ExaminerRecruitment { get; set; }


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
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
