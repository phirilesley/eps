namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class SettingRateViewModel
    {
        public string? ExamCode { get; set; }
        public string? SubjectCode { get; set; }
        public string? PaperCode { get; set; }

        public decimal? SettingFee { get; set; } // nullable
    }
}
