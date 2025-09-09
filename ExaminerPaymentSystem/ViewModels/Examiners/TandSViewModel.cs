using System.ComponentModel.DataAnnotations;
using ExaminerPaymentSystem.Models.Major;

namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class TandSViewModel
    {

        public string? EMS_SUBKEY { get; set; }

        public string? Date { get; set; }

        public string? EMS_EXAMINER_NAME { get; set; }
        public string? EMS_LAST_NAME { get; set; }
        public string? EMS_ADDRESS { get; set; }

        public string? EMS_DISTRICT { get; set; }

        public string? EMS_WORK_ADD1 { get; set; }
        public string? EMS_WORK_ADD2 { get; set; }
        public string? EMS_WORK_ADD3 { get; set; }

        public string? EMS_NATIONAL_ID { get; set; }

        public string? EMS_EXAMINER_CODE { get; set; }

        public string? EMS_ECT_EXAMINER_CAT_CODE { get; set; }

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

        public string? EMS_RESORNON { get; set; }
        public string? EMS_VENUE { get; set; }


        public string? EMS_TOTAL { get; set; }

        public string? ADJ_TOTAL { get; set; }


        public string? CENTRE_SUPERVISOR_STATUS { get; set; }

        public string? CENTRE_SUPERVISOR_STATUS_BY { get; set; }
        public string? CENTRE_SUPERVISOR_DATE { get; set; }

        public string? CENTRE_SUPERVISOR_COMMENT { get; set; }

        public string? SUBJECT_MANAGER_STATUS { get; set; }

        public string? SUBJECT_MANAGER_STATUS_BY { get; set; }

        public string? SUBJECT_MANAGER_DATE { get; set; }

        public string? SUBJECT_MANAGER_COMMENT { get; set; }

        public string? ACCOUNTS_STATUS { get; set; }
        public string? ACCOUNTS_STATUS_BY { get; set; }
        public string? ACCOUNTS_DATE { get; set; }

        public string? ACCOUNTS_REVIEW { get; set; }
        public string? ACCOUNTS_REVIEW_BY { get; set; }
        public string? ACCOUNTS_REVIEW_DATE { get; set; }
        public string? ACCOUNTS_REVIEW_COMMENT { get; set; }

        public string? STATUS { get; set; }

        public string? ADJ_BY { get; set; }

        public string? ReturnBackStatus { get; set; }

        public string? ReturnBackBy { get; set; }

        public string? ReturnDate { get; set; }

        public string? ReturnComment { get; set; }

        public List<TandSDetail> TANDSDETAILS { get; set; }


        public TandSAdvance TANDSADVANCE { get; set; }


    }

    public class TandSListViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IDNumber { get; set; }

        public string ExaminerCode { get; set; }
        public string SubKey { get; set; }

        public string Subject { get; set; }
        public string Status { get; set; }

        public string AccountsStatus { get; set; }

        public string PeerStatus { get; set; }

        public string ClaimId { get; set; }

        public string ReturnBackStatus { get; set; }

        public string RecommendedStatus { get; set; }
        public string RecommendedBy { get; set; }
        public string RecommendedDate { get; set; }

        public string ApprovedStatus { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedDate { get; set; }

        public string CreatedDate { get; set; }

    }
}
