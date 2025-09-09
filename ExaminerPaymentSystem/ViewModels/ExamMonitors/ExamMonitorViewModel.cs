using CsvHelper.Configuration.Attributes;
using ExaminerPaymentSystem.Models.ExamMonitors;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{
    public class ExamMonitorViewModel
    {

        public int Id { get; set; }

        [StringLength(60)]
        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [StringLength(60)]
        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [StringLength(60)]
        [Required]
        [DisplayName("National Id")]
        public string NationalId { get; set; }

        [StringLength(1)]
        [Required]
        [DisplayName("Gender")]
        public string? Sex { get; set; }

        [StringLength(60)]
        [DisplayName("Status")]
        public string? Status { get; set; }

        [StringLength(30)]
        [DisplayName("Professional Qualification")]
        public string? Qualification { get; set; }

        [StringLength(60)]
        [DisplayName("Teaching Experience")]
        public string? Experience { get; set; }

        [StringLength(2)]
        [Required]
        [DisplayName("Region")]
        public string Region { get; set; }

        [StringLength(10)]
        [DisplayName("Cell Number")]
        public string? Phone { get; set; }

        [StringLength(3)]
        [Required]
        [DisplayName("Age")]
        public string? Age { get; set; }

        [StringLength(6)]
        [Required]
        [DisplayName("Centre Number")]
        public string Centre { get; set; }

        [StringLength(60)]
        [DisplayName("Station")]
        public string? Station { get; set; }

        [StringLength(60)]
        [DisplayName("District")]
        public string? District { get; set; }



        public string? BankCodeZwg { get; set; }
        public string? BankNameZwg { get; set; }
        public string? BankBranchCodeZwg { get; set; }
        public string? BranchZwg { get; set; }

        public string? AccountNumberZwg { get; set; }

        public string? BankCodeUsd { get; set; }
        public string? BankNameUsd { get; set; }
        public string? BankBranchCodeUsd { get; set; }
        public string? BranchUsd { get; set; }
        public string? AccountNumberUsd { get; set; }
        public string ? Email { get; set; }
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

        [Display(Name = "Academic Qualifications")]
        [DataType(DataType.Upload)]
        [MaxFileSize(5 * 1024 * 1024)]
        [AllowedExtensions(new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" })]
        public IFormFile AcademicQualificationsFile { get; set; }

        [Display(Name = "National ID Document")]
        [DataType(DataType.Upload)]
        [MaxFileSize(5 * 1024 * 1024)]
        [AllowedExtensions(new[] { ".pdf", ".jpg", ".jpeg", ".png" })]
        public IFormFile NationalIdDocsFile { get; set; }


        public string? AcademicQualifications { get; set; }

        public string? NationalIdDocs { get; set; }

        public List<ExamMonitorProfessionalQualifications> ProfessionalQualifications { get; set; }

        public IEnumerable<SelectListItem> GenderOptions { get; set; }
        public IEnumerable<SelectListItem> RegionOptions { get; set; }
        public IEnumerable<SelectListItem> StatusOptions { get; set; }

        public ExamMonitorViewModel()
        {
            GenderOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "M", Text = "Male" },
            new SelectListItem { Value = "F", Text = "Female" }
        };
        }
    }

    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult($"Maximum allowed file size is {_maxFileSize / 1024 / 1024}MB.");
                }
            }
            return ValidationResult.Success;
        }
    }

    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult($"Allowed file extensions: {string.Join(", ", _extensions)}");
                }
            }
            return ValidationResult.Success;
        }
    }
}
