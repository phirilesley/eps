namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class ApproveRegisterViewModel
    {

        public string SubKey { get; set; }
        public string FullName {  get; set; }
        public string NationalId {  get; set; }
        public string? District { get; set; }
        public string Region {  get; set; }
        public string Phone { get; set; }
  
        public string? BankCodeZwg { get; set; }
        public string? BankNameZwg { get; set; }
        public string? BankBranchCodeZwg { get; set; }
        public string? BranchZwg { get; set; }

        public string? AccountNumberZwg { get; set; }

        public string? BankCodeUsd { get; set; }
        public string? BankNameUsd { get; set; }
        public string? BankBranchCodeUsd { get; set; }
        public string? BranchUsd { get; set; }

        public string? AccountNumberUsd { get; set; }
        public string Role {  get; set; }
        public string ClusterName {  get; set; }
        public string CentreName {  get; set; }
        public string UserName { get; set; }
        public List<ExamMonitorRegisterDate> RegisterDates { get; set; }
        public string CurrentUserRole { get; set; }

    }
}
