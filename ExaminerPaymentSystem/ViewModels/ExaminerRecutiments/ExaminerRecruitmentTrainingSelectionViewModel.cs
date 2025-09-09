using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ExaminerPaymentSystem.ViewModel.ExaminerRecutiments
{
    public class ExaminerRecruitmentTrainingSelectionViewModel
    {
        public int? ExaminerRecruitmentId { get; set; }
        public string? Status { get; set; }
        public string? Subject { get; set; }
        public string? Experience { get; set; }
        public string? PaperCode { get; set; }
        public string? RegionCode { get; set; }
        public string? ExaminerName { get; set; }
        public string? LastName { get; set; }
        public string? CemId { get; set; }
        public string? PhoneHome { get; set; }
        public string? EmailAddress { get; set; }
        public string? Gender { get; set; }
        public DateTime Date { get; set; }

        public List<ExaminerRecruimentDataTableViewModel> ExaminerRecruitment { get; set; }
        public List<TeachingExperienceViewModel> Experiences { get; set; }

    }
}
