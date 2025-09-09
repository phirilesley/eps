namespace ExaminerPaymentSystem.Models.Other
{
    public class VenueReport
    {
        public string SubjectCode { get; set; }
        public string PaperCode { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int TotalCount { get; set; }

        public double Percentage { get; set; }
    }
}
