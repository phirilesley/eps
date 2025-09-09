using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models
{
    public class ExamMonitorRegisterViewModel
    {


        //public bool IsReadOnly { get; set; }

        [Display(Name = "Cluster Name")]
        public string ClusterName { get; set; }
        public string Region {  get; set; }
        public string CentreName { get; set; }
        public string PhaseName { get; set; }
        public string CentreAttached { get; set; }

        public string FullName { get; set; }

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



        public string Phone { get; set; }


        public string Role { get; set; }

        [Display(Name = "Cluster Manager's Name")]
        public string ClusterManagersName { get; set; }

        [Display(Name = "District")]
        public string District { get; set; }

        public List<ExamDateEntry> ExamDates { get; set; } = new List<ExamDateEntry>();

    

        // Properties for the main register
        public string SubKey { get; set; }
        public string NationalId { get; set; }

        public DateTime PhaseStartDate { get; set; }

        public DateTime PhaseEndDate { get; set; }
    }

    public class ExamDateEntry
    {
        public DateTime Date { get; set; }
        public string Comment { get; set; }

        public bool IsTravelDay { get; set; }
    }

   
}