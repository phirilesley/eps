using System.Net.Sockets;

namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class SummaryScriptApportionmentViewModel
    {
        // Category: PMS
        public int MaxScriptsPMS { get; set; }
        public int ExaminersAvailablePMS { get; set; }
        public int ExaminersChosenPMS { get; set; }
        public int ScriptCoveredPMS => MaxScriptsPMS * ExaminersChosenPMS;

        // Category: RPMS
        public int MaxScriptsRPMS { get; set; }
        public int ExaminersAvailableRPMS { get; set; }
        public int ExaminersChosenRPMS { get; set; }
        public int ScriptCoveredRPMS => MaxScriptsRPMS * ExaminersChosenRPMS;

        // Category: DPMS
        public int MaxScriptsDPMS { get; set; }
        public int ExaminersAvailableDPMS { get; set; }
        public int ExaminersChosenDPMS { get; set; }
        public int ScriptCoveredDPMS => MaxScriptsDPMS * ExaminersChosenDPMS;

        // Category: BMS
        public int MaxScriptsBMS { get; set; }
        public int ExaminersAvailableBMS { get; set; }
        public int ExaminersChosenBMS { get; set; }
        public int ScriptCoveredBMS => MaxScriptsBMS * ExaminersChosenBMS;

        // Category: E
        public int MaxScriptsE { get; set; }
        public int ExaminersAvailableE { get; set; }
        public int ExaminersChosenE { get; set; }
        public int ScriptCoveredE => MaxScriptsE * ExaminersChosenE;    

        // Totals
        public int TotalMaxScripts { get; set; }
        public int TotalExaminersAvailable { get; set; }
        public int TotalExaminersChosen { get; set; }
        public int TotalScriptCovered { get; set; }

        // Total Entries (This would be fetched from your database or calculated dynamically)
        public int TotalEntries { get; set; }

        public int AllowedExaminers { get; set; }
    }
}
