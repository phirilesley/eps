namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    // Models/ContractTemplate.cs
    public class ContractTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string TemplateType { get; set; } // "AssistantClusterManager" or "ResidentMonitor"
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsActive { get; set; }
    }

    // Models/ContractData.cs
    public class ContractData
    {
        public string FullName { get; set; }
        public string IdNumber { get; set; }
        public string Status { get; set; }
        public string Title { get; set; } // Mr./Mrs./Miss/Dr./Prof
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
        public string BankName { get; set; }
        public string Branch { get; set; }
        public string AccountNumber { get; set; }

        public string USDBankName { get; set; }
        public string USDBranch { get; set; }
        public string USDAccountNumber { get; set; }
        public string NextOfKin { get; set; }
        public string NextOfKinCell { get; set; }
        public string Witness { get; set; }
        public decimal StipendRate { get; set; }

        public decimal LunchRate {  get; set; }
        public decimal AccomodationRate {  get; set; }
        public decimal DinnerRate {  get; set; }
        public decimal BreakFastRate {  get; set; }
        public string Session { get; set; }

        public string Phase { get; set; }
    }

    // Models/ContractViewModel.cs
    public class ContractViewModel
    {
        public ContractTemplate Template { get; set; }
        public ContractData Data { get; set; }
        public string ContractType { get; set; }
    }
}
