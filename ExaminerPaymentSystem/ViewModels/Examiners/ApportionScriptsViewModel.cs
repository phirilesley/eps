namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class ApportionScriptsViewModel
    {
        public int TotalEntries { get; set; }

        public int SelectedPMS { get; set; }
        public int MaxScriptsPMS { get; set; }

        public int SelectedRPMS { get; set; }
        public int MaxScriptsRPMS { get; set; }

        public int SelectedDPMS { get; set; }
        public int MaxScriptsDPMS { get; set; }

        public int SelectedBMS { get; set; }
        public int MaxScriptsBMS { get; set; }

        public int SelectedE { get; set; }
        public int MaxScriptsE { get; set; }

        // Totals for Selected and Max Scripts
        public int AllowedExaminers { get; set; }
        public int TotalSelected => SelectedPMS + SelectedRPMS + SelectedDPMS + SelectedBMS + SelectedE;
        public int TotalMaxScripts => MaxScriptsPMS + MaxScriptsRPMS + MaxScriptsDPMS + MaxScriptsBMS + MaxScriptsE;
    }
}
