namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class ExaminerScriptsMarkedViewModel
    {
        public string? EMS_EXAMINER_NUMBER { get; set; }
        public string? EMS_EXAMINER_NAME { get; set; }

        public string? EMS_LAST_NAME { get; set; }
        public int? SCRIPTS_MARKED { get; set; }

        public string? EMS_NATIONAL_ID { get; set; }

        public string? EMS_ECT_EXAMINER_CAT_CODE { get; set; }

        public string? EMS_EXAM_CODE { get; set; }

        public string SCRIPT_RATE { get; set; }


        public string RESPONSIBILITY_FEES { get; set; }

        public string COORDINATION_FEES { get; set; }

        public string CAPTURING_FEES { get; set; }

        public string GRAND_TOTAL { get; set; }
    }
}
