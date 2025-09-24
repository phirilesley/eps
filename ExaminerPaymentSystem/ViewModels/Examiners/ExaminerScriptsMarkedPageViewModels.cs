namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class ExaminerScriptsMarkedIndexPageViewModel
    {
        public string? ExamCode { get; set; }
        public string? SubjectCode { get; set; }
        public string? PaperCode { get; set; }
        public string? BmsCode { get; set; }
        public string? SupervisorName { get; set; }
    }

    public class ExaminerScriptsMarkedApprovalPageViewModel
    {
        public string? ExamCode { get; set; }
        public string? SubjectCode { get; set; }
        public string? PaperCode { get; set; }
        public string? RegionCode { get; set; }
    }

}
