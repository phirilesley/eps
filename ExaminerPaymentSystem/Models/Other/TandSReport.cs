using ExaminerPaymentSystem.Models.Major;

namespace ExaminerPaymentSystem.Models.Other
{
    public class TandSReport
    {

        public string? EMS_EXAMINER_NAME { get; set; }
        public string? EMS_LAST_NAME { get; set; }
        public string? EMS_ADDRESS { get; set; }

        public string? EMS_NATIONAL_ID { get; set; }

        public string? EMS_EXAMINER_CODE { get; set; }

        public string? EMS_PHONE_HOME { get; set; }

        public string? EMS_ACCOUNT_NO_FCA { get; set; }
        public string? EMS_BANK_NAME_FCA { get; set; }

        public string? EMS_ACCOUNT_NO_ZWL { get; set; }
        public string? EMS_BANK_NAME_ZWL { get; set; }
        public string? EMS_SUB_SUB_ID { get; set; }
        public string? EMS_LEVEL_OF_EXAM_MARKED { get; set; }
        public string? EMS_PAPER_CODE { get; set; }
        public string? TANDSCODE { get; set; }

        public string? EMS_PURPOSEOFJOURNEY { get; set; }


        public string? EMS_TOTAL { get; set; }

        public string? ADJ_TOTAL { get; set; }

        public List<TandSDetail> TANDSDETAILS { get; set; }
    }


    public class TravelAdvanceReport
    {
        public string? Status { get; set; }
        public string? Name { get; set; }
        public string? BankZIG { get; set; }
        public string? BankBranchZIG { get; set; }
        public string? BankAccountZIG { get; set; }
        public string? Subject {  get; set; }

        public string? BankUSD { get; set; }
        public string? BankBranchUSD { get; set; }
        public string? BankAccountUSD { get; set; }
        public string? WorkAddress { get; set; }
        public string? Venue { get; set; }
        public string? Days { get; set; }
        public string? TransitBusFare { get; set; }
        public string? LocalBusFare { get; set; }
        public string? TransitLunch { get; set; }
        public string? CheckInnAccommodation { get; set; }
        public string? Accommodation { get; set; }
        public string? Breakfast { get; set; }
        public string? LunchAndDinner { get; set; }
        public string? CheckInDinner { get; set; }

        public string? Supp { get; set; }
        public string? Total { get; set; }
        public string? LessAdvance { get; set; }
        public string? UsdBalance { get; set; }
        public string? Rate { get; set; }
        public string? ZigBalance { get; set; }
        public string? ZigTicket { get; set; }
        public string? ZigPayment { get; set; }
    }

    public class TravelExaminerMarkingReport
    {
        public string? Date { get; set; }
        public string? BankName { get; set; }

        public string? ShortCode { get; set; }

        public string? BankAccount { get; set; }

        public string? Fullname { get; set; }
        public string? Venue { get; set; }
        public string? Status { get; set; }

        public string? Subject { get; set; }

        public string? ScriptRate { get; set; }
        public string? ScriptMarked { get; set; }

        public string? TotalAfterScriptRate { get; set; }

        public string? Resp { get; set; }

        public string? Coord { get; set; }

        public string? GrandTotal { get; set; }

        public string? Capturing { get; set; }

        public string? Total { get; set; }

        public string? Rate { get; set; }

        public string? ZIGAmount { get; set; }

        public string? WHT { get; set; }


        public string? AmountPayable { get; set; }
    }

}
