namespace ExaminerPaymentSystem.Models.Other
{
    public class TransactionReport
    {
        public string Subject {  get; set; }

        public string Paper { get; set; }
        public string Category { get; set; }
        public string Total { get; set; }
 
    }


    public class TransactionReport2
    {
        public string Category { get; set; }
        public string Count { get; set; }

    }

    public class TransactionReport3
    {
        public string ExamCode { get; set; }
        public string Subject { get; set; }

        public string Paper { get; set; }

        public string Region{ get; set; }
        public string Material { get; set; }
        public string Card { get; set; }

        public string Entries { get; set; }
        public string Examiners { get; set; }


    }

    public class TransactionReportNow
    {
         public string ExaminerNumber { get; set; }
        public string Name { get; set; }

        public string IdNumber { get; set; }
        public string Subject { get; set; }

        public string Category { get; set; }
        public int ScriptsMarked { get; set; }
        public string Responsibility { get; set; }

        public string Coodination { get; set; }

        public string Capturing { get; set; }

        public string GrandTotal { get; set; }

        public string FinalTotal { get; set; }

    }

    public class CheckListReport
    {
        
        public string Subject { get; set; }
        public string TandSPercentage { get; set; }
        public string ScriptsApprovalStatus {  get; set; }
        public string ScriptsMarkedPercentage { get; set; }

        public string TandSApprovalStatus { get; set; }
       public string overallStatus { get; set; }

    }

    public class CheckTransactionListReport
    {
        public string ExaminerNumber { get; set; }
        public string Name { get; set; }

        public string IdNumber { get; set; }
        public string Subject { get; set; }

        public string Category { get; set; }
        public int ScriptsMarked { get; set; }
        public string SubjectManager { get; set; }

        public string CentreSupervisor { get; set; }

        public string Paid { get; set; }

    }

    public class CheckTandSListReport
    {
    
        public string Name { get; set; }

        public string IdNumber { get; set; }
        public string Subject { get; set; }

        public string SubjectManager { get; set; }
        public string CentreSupervisor { get; set; }
        public string Initiator { get; set; }

        public string Reviewer { get; set; }
        public string Amount { get; set; }
        public string Paid { get; set; }

    }



}
