namespace ExaminerPaymentSystem.Models.Other
{
    public class SubjectVenue
    {
        public int Id { get; set; }
        public string? ExamCode { get; set; }
        public string Subject { get; set; }
        public string PaperCode { get; set; }
        public string Venue { get; set; }
        public string? Region { get; set; }     
    }
}
