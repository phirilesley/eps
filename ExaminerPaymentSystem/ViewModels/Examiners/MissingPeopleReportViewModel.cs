namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class MissingPeopleReportViewModel
    {

        public string EMS_EXAMINER_CODE { get; set; }
        public string EMS_NATIONAL_ID { get; set; }
        public string EMS_EXAMCODE { get; set; }
        public string EMS_SUBKEY { get; set; }
        public string EMS_SUBJECT_CODE { get; set; }

        public string EMS_PAPER_CODE { get; set; }
        public string EMS_FullName { get; set; }
        public string Bankzig { get; set; }

        public string BankNamezig { get; set; }
        public string Branchzig { get; set; }

        public string Bankfca { get; set; }
        public string Branchfca { get; set; }
        public string BankNamefca { get; set; }
        public string Phone { get; set; }
        public string STATUS { get; set; }
    }

    public class MissingScriptReportViewModel
    {

        public string EMS_EXAMINER_CODE { get; set; }
        public string EMS_NATIONAL_ID { get; set; }
        public string EMS_EXAMCODE { get; set; }
        public string EMS_SUBKEY { get; set; }
        public string EMS_SUBJECT_CODE { get; set; }

        public string EMS_PAPER_CODE { get; set; }
        public string EMS_FullName { get; set; }
        public int? SCRIPTMARKED { get; set; }
        public string STATUS { get; set; }

        public string SUBSTATUS { get; set; }

        public string CENTSTATUS { get; set; }

        public string PMSSTATUS { get; set; }
    }
}
