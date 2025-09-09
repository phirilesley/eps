using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class MarksCaptured
    {
        [Key]
        public int Id { get; set; }
        public string? ExamCode { get; set; }
        public string? SubjectCode { get; set; }
        public string? PaperCode { get; set; }

        public string? RegionCode { get; set; }

        public int TotalScriptsCaptured { get; set; }

        public int ScriptMarked { get; set; }

        public int AccountsTotalScriptCaptured { get; set; }

        public int AbsentScripts { get; set; }

        public int ApportionedScripts { get; set; }

        public int PirateCandidates { get; set; }

        public int Exceptions { get; set; }
    }
}
