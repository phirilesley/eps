using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExaminerRecruitment
{
    public class ExaminerRecruitmentAttachements
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ExaminerRecruitment")]
        public int ExaminerRecruitmentId { get; set; }
        public ExaminerRecruitment ExaminerRecruitment { get; set; }

        [Required]
        [Display(Name = "Attach Institution's Head's Comments")]
        public string InstitutionHeadDoc { get; set; }

        [Required]
        public string AcademicQualifications { get; set; }

        [Required]
        public string NationalIdDocs{ get; set; }
        public DateTime Date { get; set; }
        }
    }

