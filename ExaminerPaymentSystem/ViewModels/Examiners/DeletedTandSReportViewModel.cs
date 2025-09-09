namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class DeletedTandSReportViewModel
    {
        public string EMS_EXAMINER_CODE { get; set; }
        public string EMS_NATIONAL_ID { get; set; }
        public string EMS_EXAMCODE { get; set; }

        public string EMS_SUBJECT_CODE { get; set; }

        public string EMS_PAPER_CODE { get; set; }
        public string EMS_FullName { get; set; }
        public string STATUS { get; set; }
        public string DeletedBy { get; set; }

        public decimal? Amount { get; set; }
    }
}
