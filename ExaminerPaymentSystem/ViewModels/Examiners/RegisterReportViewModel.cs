namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class RegisterReportViewModel
    {
        public string? ExaminerCode { get; set; }
        public string? IDNumber { get; set; }
        public string? FirstName { get; set; }
        public string? Surname { get; set; }

        public string? SubKey { get; set; }
        public string? ExamCode { get; set; }
        public string? SubjectCode { get; set; }

        public string? PaperCode { get; set; }

        public string Status { get; set; }

        public string? RecommendedBy { get; set; }

        public string? RecommendedDate { get; set; }

        public string? RecommendedStatus { get; set; }

        public string? AttendanceStatus { get; set; }

        public string? AttendanceStatusBy { get; set; }

        public string? AttendanceUpdateDate { get; set; }

        public string? StatusDate { get; set; }

    }
}
