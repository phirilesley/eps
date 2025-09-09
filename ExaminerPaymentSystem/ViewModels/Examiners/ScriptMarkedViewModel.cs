namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class ScriptMarkedViewModel
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Subject { get; set; }
        public string? ExaminerNumber { get; set; }

        public string? BMS { get; set; }

        public string? PaperCode { get; set; }

        public string? Category { get; set; }

        public string? ExaminerCode { get; set; }
        public string? IdNumber { get; set; }
        public string? SubKey { get; set; }
        public string? ScriptApportioned { get; set; }
        public int? ScriptMarked { get; set; }

        public string? Role { get; set; }
        public string? Perfomance { get; set; }
        public bool IsPresent { get; set; }
        public string? Activity {  get; set; }
        public string? Status { get; set; }
    }
}
