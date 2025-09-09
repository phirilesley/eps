using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class BankData
    {
        [Key]
        public int Id { get; set; }

        public string? B_BANK_CODE { get; set; }

        public string? B_BANK_NAME { get; set; }

        public string? BB_BRANCH_NAME { get; set; }

        public string? BB_BRANCH_CODE { get; set; }
    }

    public class Bank
    {
        public int Id { get; set; }
        public string B_BANK_CODE { get; set; }
        public string B_BANK_NAME { get; set; }
    }

    public class Branch
    {
        public int Id { get; set; }
        public string B_BANK_CODE { get; set; }
        public string BB_BRANCH_NAME { get; set; }
        public string BB_BRANCH_CODE { get; set; }
    }

    public class BankDtoData
    {
        public string? EMS_ACCOUNT_NO_FCA { get; set; }
        public string? EMS_BANK_NAME_FCA { get; set; }
        public string? EMS_BRANCH_NAME_FCA { get; set; }
        public string? EMS_BANK_CODE_FCA { get; set; }
        public string? EMS_BRANCH_CODE_FCA { get; set; }
        public string? EMS_ACCOUNT_NO_ZWL { get; set; }
        public string? EMS_BRANCH_NAME_ZWL { get; set; }
        public string? EMS_BANK_NAME_ZWL { get; set; }
        public string? EMS_BANK_CODE_ZWL { get; set; }
        public string? EMS_BRANCH_CODE_ZWL { get; set; }
    }
}
