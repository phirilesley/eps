namespace ExaminerPaymentSystem.ViewModel.ExaminerRecutiments
{
    public class ExaminerRecruitmentAssesmentPresentWithoutGrade
    {
        public int ExaminerRecruitmentId { get; set; }
        public string? ExaminerName { get; set; }
        public string? LastName { get; set; }
        public string? CemId { get; set; }
        public string? Subject { get; set; }
        public string? PaperCode { get; set; }
        public string? RegionCode { get; set; }
        public string? Experience { get; set; }
        public string? CapturerGrade { get; set; }
        public string? VerifierGrade { get; set; }
        public string? CapturerUserName { get; set; }
        public string? VerifierUserName { get; set; }
        public string? StatusType { get; set; } // "Missing"
        public DateTime? Date { get; set; }
        public int? ExaminerRecruitmentRegisterId { get; set; }
    }
}
