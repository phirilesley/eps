namespace ExaminerPaymentSystem.ViewModel.ExaminerRecutiments
{
    public class ExaminerRecruitmentAssessmentReportViewModel
    {
        public string? Subject { get; set; }
        public string? PaperCode { get; set; }
        public int? GoodEntries { get; set; }
        public int? TotalPresentExaminers { get; set; }
        public double? Percentage { get; set; }
        public int? BadEntries { get; set; }
        public int? Partially { get; set; }
    }
}
