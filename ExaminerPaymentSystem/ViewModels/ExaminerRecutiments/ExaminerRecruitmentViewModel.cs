using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Xml;

namespace ExaminerPaymentSystem.ViewModel.ExaminerRecutiments
{
    public class ExaminerRecruitmentViewModel
    {
        public string ExaminerCode { get; set; }

        [StringLength(60)]
        [Required]
        [DisplayName("Examiner Name")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "The field must contain letters only")]
        public string ExaminerName { get; set; }

        [StringLength(20)]
        [Required]
        [DisplayName("Surname")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "The field must contain letters only")]
        public string LastName { get; set; }

       
        [StringLength(30)]
        [Required]
        [DisplayName("National Id")]
        [RegularExpression(@"^\d{8,9}[A-Z]\d{2}$", ErrorMessage = "Enter a valid National ID")]


        public string CemId { get; set; }

        [StringLength(1)]
        [Required]
        [DisplayName("Gender")]
        public string Sex { get; set; }

        [DataType(DataType.Date)]
        [Required]
        [DisplayName("Date Of Birth")]
        public DateTime DateOfBirth { get; set; }

        
        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "The phone number is invalid")]
        [DebuggerDisplay("Cell Phone")]
        public string PhoneHome { get; set; }

        [DisplayName("Home Address place")]
        [StringLength(240)]
        [Required]
   
        public string Address { get; set; }

       
        [StringLength(60)]
        [DisplayName("Town/City")]

        public string? Address3 { get; set; }

        [DisplayName("Email Address")]
        [EmailAddress]
        [Required]
        [StringLength(30)]
        //[Unique("EmailAddress", typeof(ExaminerRecruitment))]
        public string EmailAddress { get; set; }

        [DisplayName("Professional Qualification")]
        [StringLength(30)]
    
        public string? Qualification { get; set; }

        [DisplayName("Business Phone")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "The phone number is invalid")]
        public string PhoneBusiness { get; set; }

        [StringLength(5)]
        [Required]
        [DisplayName("Region")]
        public string RegionCode { get; set; }

        [Column("CEM_DISTRICT_CODE")]
        [StringLength(30)]
        [Required]
        [DisplayName("District")]
        public string DistrictCode { get; set; }

        [StringLength(60)]
        [Required]
        [DisplayName("Subject")]
        public string Subject { get; set; }

        [StringLength(3)]
        [DisplayName("Paper Code")]
        public string PaperCode { get; set; }


        [StringLength(20)]
        [DisplayName("Training Place")]
        public string TrainingCentre { get; set; }


        [StringLength(60)]
        [DisplayName("Teaching Experience")]
        public string Experience { get; set; }

        [DisplayName("Academic Qualification")]
        [StringLength(30)]
        [Required]
        public string AcademicQualification { get; set; }


        [Required]
        [StringLength(60)]
        [DisplayName("Box/House Number")]
        public string Address2 { get; set; }
       
        [StringLength(60)]
        [Required]

        [DisplayName("Work Institution")]
        public string? WorkAddress1 { get; set; }

        [DisplayName("PO Box/Number")]
        [StringLength(60)]
        public string WorkAddress2 { get; set; }


        [StringLength(60)]
        [DisplayName("Work Town/City")]
        [Required]
        public string? WorkAddress3 { get; set; }


        [FileValidation(50, new[] { ".pdf", ".png", ".jpeg", ".jpg" })]
        [Display(Name = "Attach Institution's Head's Comments")]
        public IFormFile AttachHeadComment { get; set; }

        [FileValidation(50, new[] { ".pdf", ".png", ".jpeg", ".jpg" })]
        [Display(Name = "Academic Qualifications")]
        public IFormFile AcademicQualifications { get; set; }

        [FileValidation(50, new[] { ".pdf", ".png", ".jpeg", ".jpg" })]
        [Display(Name = "National Id Attachment")]
        public IFormFile NationalIdDocs { get; set; }
   

        public List<TeachingExperienceViewModel>? TeachingExperiences { get; set; }
        public List<ExaminerRecruitmentProfessionalQualificationViewModel>? ProfessionalQualifications { get; set; }
        public List<ExaminerRecruitmentAttachmentsViewModel>? ExaminerRecruitmentAttachements { get; set; }

    }
}
