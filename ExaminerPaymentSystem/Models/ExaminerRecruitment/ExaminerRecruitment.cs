using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExaminerRecruitment
{
    public class ExaminerRecruitment    {

        [Key]
        public int Id { get; set; }

        [Column("CEM_CAN_EXAMINER_CODE")]
        [StringLength(5)]

        public string? ExaminerCode { get; set; }

        [Column("CEM_CAN_EXAMINER_NAME")]
        [StringLength(60)]
        [Required]
        [DisplayName("First Name")]
        public string ExaminerName { get; set; }

        [Column("CEM_ID")]
        [StringLength(30)]
        [DisplayName("National Id")]
        public string CemId { get; set; }

        [Column("CEM_ADDRESS")]
        [StringLength(240)]
        [Required]
        [DisplayName("Home Address")]
        public string Address { get; set; }

        [Column("CEM_SCHOOL")]
        [StringLength(60)]
        public string? School { get; set; }

        [Column("CEM_STAGE_OF_SELECTION")]
        [StringLength(30)]
        public string? StageOfSelection { get; set; }

        [Column("CEM_SUBJECT")]
        [StringLength(60)]
        [Required]
        [DisplayName("Subject")]
        public string Subject { get; set; }

        [Column("CEM_QUALIFICATION")]
        [StringLength(30)]
        [DisplayName("Professional Qualification")]
        public string? Qualification { get; set; }

        [Column("CEM_DATE_OF_JOINING")]
        [DisplayName("D.O.B")]
        public DateTime? DateOfBirth { get; set; }

        [Column("CEM_EXPERIENCE")]
        [StringLength(60)]
        [DisplayName("Teaching Experience")]
        public string? Experience { get; set; }

        [Column("CEM_SELECTED_FLAG")]
        [StringLength(1)]
        public string? SelectedFlag { get; set; }

        [Column("CEM_WORK_ADD1")]
        [StringLength(60)]
       
        [DisplayName("Name Of Institution")]
        public string? WorkAddress1 { get; set; }

        [DisplayName("Work Address")]
        [Column("CEM_WORK_ADD2")]
        [StringLength(60)]
 
        public string? WorkAddress2 { get; set; }

        [Column("CEM_WORK_ADD3")]
        [StringLength(60)]
        public string? WorkAddress3 { get; set; }

        [Column("CEM_LAST_NAME")]
        [StringLength(20)]
        [Required]
        [DisplayName("Surname")]
        public string LastName { get; set; }

        [Column("CEM_ADDRESS2")]
        [StringLength(60)]
      
        public string? Address2 { get; set; }

        [Column("CEM_ADDRESS3")]
        [StringLength(60)]
        public string? Address3 { get; set; }

        [Column("CEM_DISTRICT_CODE")]
        [StringLength(30)]
        
        [DisplayName("District")]
        public string? DistrictCode { get; set; }

        [Column("CEM_REGION_CODE")]
        [StringLength(5)]
        [Required]
        [DisplayName("Region")]
        public string RegionCode { get; set; }

        [Column("CEM_PHONE_HOME")]
        [StringLength(10)]
        [DisplayName("Cell Number")]
        public string? PhoneHome { get; set; }

        [Column("CEM_SEX")]
        [StringLength(1)]
        [Required]
        [DisplayName("Gender")]
        public string? Sex { get; set; }

        [Column("CEM_PHONE_BUS")]
        [StringLength(10)]
        [DisplayName("Business Phone")]
        public string? PhoneBusiness { get; set; }

        [Column("CEM_PAPER_CODE")]
        [StringLength(3)]
        [DisplayName("Paper Code")]
        public string? PaperCode { get; set; }

        [Column("CEM_MARKING_EXP")]
        [StringLength(60)]
        public string? MarkingExperience { get; set; }

        [Column("CEM_ACADEMIC_QUAL")]
        [StringLength(30)]
        [DisplayName("Level Taught")]
        public string? AcademicQualification { get; set; }

        [Column("CEM_YEAR_TRAINED")]
        [StringLength(4)]
        public string? YearTrained { get; set; }

        [Column("CEM_ACCOUNT_NO")]
        [StringLength(30)]
        public string? AccountNumber { get; set; }

        [Column("CEM_BANK_CODE")]
        [StringLength(5)]
        public string? BankCode { get; set; }

        [Column("CEM_BRANCH_CODE")]
        [StringLength(5)]
        public string? BranchCode { get; set; }

        [Column("CEM_TRAINING_CENTRE")]
        [StringLength(20)]
        [DisplayName("Training Place")]
        public string? TrainingCentre { get; set; }

        [Column("CEM_PERFORMANCE_INDEX")]
        [StringLength(3)]
        public string? PerformanceIndex { get; set; }

        [Column("CEM_TRAINING_SESSION")]
        [StringLength(10)]
        public string? TrainingSession { get; set; }

        [Column("CEM_EMAIL_ADDRESS")]
        [EmailAddress]
        [DisplayName("Email Address")]
        [StringLength(30)]
        public string? EmailAddress { get; set; }

        [Column("CEM_CAPTURE_DATE")]
        public DateTime? CaptureDate { get; set; }

        [Column("CEM_CAPTURED_BY")]
        [StringLength(30)]
        public string? CapturedBy { get; set; }

        [Column("CEM_ACTIONED_BY")]
        [StringLength(30)]
        public string? ActionedBy { get; set; }

        [Column("CEM_ACTION_DATE")]
        public DateTime? ActionDate { get; set; }


        // Navigation property for one-to-one relationship
        public ExaminerRecruitmentRegister ExaminerRecruitmentRegister { get; set; }
        public ExaminerRecruitmentInvitation ExaminerRecruitmentInvitation { get; set; }
        public ExaminerRecruitmentEmailInvitation ExaminerRecruitmentEmailInvitation { get; set; }
        // Navigation property for related ExaminerRecruitmentAssessments
        public ExaminerRecruitmentAssessment ExaminerRecruitmentAssessment { get; set; }

        public ExaminerRecruitmentTrainingSelection ExaminerRecruitmentTrainingSelection { get; set; }
        public ExaminerRecruitmentAttachements ExaminerRecruitmentAttachements { get; set; }

        // Navigation Property for Teaching Experiences
        public ICollection<TeachingExperience> TeachingExperiences { get; set; }
        public ICollection<ProfessionalQualifications> ProfessionalQualifications { get; set; }
    }
}
