using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.ViewModel.ExaminerRecutiments
{
    public class ExaminerRecruitmentAttachmentsViewModel
    {
        [FileValidation(50, new[] { ".pdf", ".png", ".jpeg", ".jpg" })]
        [Display(Name = "Attach Institution's Head's Comments")]
        public IFormFile AttachHeadComment { get; set; }

        [FileValidation(50, new[] { ".pdf", ".png", ".jpeg", ".jpg" })]
        [Display(Name = "Academic Qualifications")]
        public IFormFile AcademicQualifications { get; set; }

        [FileValidation(50, new[] { ".pdf", ".png", ".jpeg", ".jpg" })]
        [Display(Name = "National Id")]
        public IFormFile NationalIdDocs { get; set; }

    }
}
