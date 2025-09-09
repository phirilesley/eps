using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.ViewModel.ExaminerRecutiments
{
    public class ExaminerRecruimentDataTableViewModel
    {

        public int Id { get; set; }
        public string ExaminerCode { get; set; }
        public string ExaminerName { get; set; }

        public string LastName { get; set; }
        public string PaperCode { get; set; }
        public string Subject { get; set; }

        public string CemId { get; set; }
        public string PhoneHome { get; set; }
        public string EmailAddress { get; set; }
        public string Gender { get; set; }
        [ValidateNever]
        public bool? Status { get; set; }
        [ValidateNever]
        public string? Statuss { get; set; }
        [ValidateNever]
        public string? Region { get; set; }

        [ValidateNever]
        public string? Grade { get; set; }

        public string CapturerGrade { get; set; }
        public string VerifierGrade { get; set; }
        public int? ExaminerRecruitmentRegisterId { get; set; }

        [ValidateNever]
        public string? Level { get; set; }

        [ValidateNever]
        public DateTime Date { get; set; }

        [ValidateNever]
        public string? AttachHeadComment { get; set; }

        [ValidateNever]
        public string? AcademicQualifications { get; set; }
        [ValidateNever]
        public string? NationalIdDocs { get; set; }

        [ValidateNever]
        public ExaminerRecruitmentAssessment? ExaminerRecruitmentAssessment { get; set; }

         public List<TeachingExperienceViewModel>? Experiences { get; set; }
        

    }
}
