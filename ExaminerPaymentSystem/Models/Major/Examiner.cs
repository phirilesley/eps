using DocumentFormat.OpenXml.Drawing.Charts;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Other;
using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Major
{
    public class Examiner
    {

        [Key]
        [Required(ErrorMessage = "Examiner National ID is required.")]
        public string EMS_NATIONAL_ID { get; set; }


        [Required(ErrorMessage = "Examiner Code is required.")]
           public string EMS_EXAMINER_CODE { get; set; }

        [Required]
        public string EMS_SUBKEY { get; set; }

        [Required(ErrorMessage = "Examiner Name is required.")]
        public string EMS_EXAMINER_NAME { get; set; }

        [Required(ErrorMessage = "Examiner Last is required.")]
        public string EMS_LAST_NAME { get; set; }
        [Required(ErrorMessage = "Examiner Gender is required.")]
        public string EMS_SEX { get; set; }

        public string? EMS_ADDRESS { get; set; }
        public string? EMS_EXPERIENCE { get; set; }
        public string? EMS_MARKING_EXPERIENCE { get; set; }

        public string? EMS_LEVEL_OF_EXAM_MARKED { get; set; }
        public string? EMS_STATUS { get; set; }
        public string? EMS_COMMENTS { get; set; }
        public string? EMS_PERFORMANCE_INDEX { get; set; }
        public string? EMS_SELECTED_FLAG { get; set; }
        public string? EMS_ECT_EXAMINER_CAT_CODE { get; set; }
        public string? EMS_SUB_SUB_ID { get; set; }
        public string? EMS_WORK_ADD1 { get; set; }
        public string? EMS_WORK_ADD2 { get; set; }
        public string? EMS_WORK_ADD3 { get; set; }

        public string? EMS_ADDRESS2 { get; set; }
        public string? EMS_ADDRESS3 { get; set; }
        public string? EMS_WORK { get; set; }
        public string? EMS_DISTRICT_CODE { get; set; }
        public string? EMS_REGION_CODE { get; set; }
        public string? EMS_PHONE_HOME { get; set; }

        public string? EMS_PHONE_BUS { get; set; }
        public string? EMS_QUALIFICATION { get; set; }
        public string? EMS_PAPER_CODE { get; set; }
        //public string? EMS_SUBJECT_CODE { get; set; }
        public string? EMS_MARKING_REG_CODE { get; set; }
        public string? EMS_EXAMINER_NUMBER { get; set; }
        public string? EMS_ACCOUNT_NO_FCA { get; set; }
        public string? EMS_BANK_NAME_FCA { get; set; }
        public string? EMS_BRANCH_NAME_FCA { get; set; }
        public string? EMS_BANK_CODE_FCA { get; set; }
        public string? EMS_BRANCH_CODE_FCA { get; set; }
        public string? EMS_ACCOUNT_NO_ZWL { get; set; }
        public string? EMS_BRANCH_NAME_ZWL { get; set; }
        public string? EMS_BANK_NAME_ZWL { get; set; }
        public string? EMS_BANK_CODE_ZWL { get; set; }
        public string? EMS_BRANCH_CODE_ZWL { get; set; }
        public string? EMS_TAX_ID_NUMBER { get; set; }
        public string? EMS_EXM_SUPERORD { get; set; }
        public string? EMS_SEL_GRADING { get; set; }
        public string? EMS_SEL_GRADE_REVIEW { get; set; }
        public string? EMS_SEL_COORDINATION { get; set; }
        public string? EMS_YEAR_TRAINED { get; set; }
        public string? EMS_DATE_OF_JOINING { get; set; }
        public string? EMS_SRC_SUPERORD { get; set; }
        public string? EMS_AID { get; set; }
        public string? CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

    

        // Collection navigation property
        public ICollection<ApplicationUser> ApplicationUsers { get; set; }

        public RegisterDto RegisterDto { get; set; }

        public ICollection<ExaminerScriptsMarked> ExaminerScriptsMarkeds { get; set; }
        public ICollection<TandS> TandSs { get; set; }

        public ICollection<ExaminerSubject> ExaminerSubjects { get; set; }

    }
}
