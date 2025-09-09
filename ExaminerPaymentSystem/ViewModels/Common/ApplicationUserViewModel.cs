namespace ExaminerPaymentSystem.ViewModels.Common
{
    public class ApplicationUserViewModel
    {
        public string Id { get; set; }
        public string? ExaminerCode { get; set; }
        public string? RegionCode { get; set; }
        public string? SubKey { get; set; }
        public string? FirstName { get; set; }
        public string? Surname { get; set; }
        public string? IDNumber { get; set; }
        public string? UserName { get; set; }
        public string? Subject { get; set; }
        public string? PaperCode { get; set; }

        public string? Mobile { get; set; }

        public string? ExamCode { get; set; }
        public string? Email { get; set; }
        public bool Activated { get; set; }
        public IList<string> Roles { get; set; }
    }

}
