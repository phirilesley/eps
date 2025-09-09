namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class PaperMarkingRateViewModel
    {
        public int Id { get; set; }
        public string SubSubId { get; set; }
        public string PaperCode { get; set; }
        public string PaperDescription { get; set; }
        public string MarkingRate { get; set; }
        public string ExamType { get; set; }

        public string ExamCode { get; set; }

        public string ExamSession { get; set; }

        public string ExamYear { get; set; }

        public string ExamLevel { get; set; }
    }
}
