using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class ExaminerApportionmentViewModel
    {
        [Key]
        public int Id { get; set; }
        public string? category { get; set; }
        public string? sub_sub_id { get; set; }
        public string? PaperCode { get; set; }

        //public string? examCode { get; set; }
        //public string? subjectCode { get; set; }
        //public string? paperCode { get; set; }
        //public string? regionCode { get; set; }
        //public string? activity { get; set; }

        //public string? RegionCode { get; set; }

        public int TotalEntries { get; set; }

        public int AllowedExaminers { get; set; }

        public int ScriptsToExaminers { get; set; }
     
        public int TotalPMS { get; set; }
        public int TotalRPMS { get; set; }
        public int TotalDPMS { get; set; }
        public int TotalBMS { get; set; }
        public int TotalE { get; set; }


        public int ShareE { get; set; }

        public int SharePMS { get; set; }

        public int ShareDPMS { get; set; }

        public int ShareRPMS { get; set; }

        public int ShareBMS { get; set; }

        public int FinalSharePMS { get; set; }
        public int FinalShareRPMS { get; set; }
        public int FinalShareDPMS { get; set; }
        public int FinalShareBMS { get; set; }
        public int FinalShareE { get; set; }


        public int TotalScriptsPMS { get; set; }
        public int TotalScriptsRPMS { get; set; }
        public int TotalScriptsDPMS { get; set; }
        public int TotalScriptsBMS { get; set; }
        public int TotalScriptsE { get; set; }

        public int TotalShare {  get; set; }
        public int FinalTotalShare {  get; set; }

        public int TotalScripts { get; set; }
    }
}
