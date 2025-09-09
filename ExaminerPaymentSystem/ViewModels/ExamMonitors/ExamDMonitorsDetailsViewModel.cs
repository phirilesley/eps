namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{
    public class ExamDMonitorsDetailsViewModel
    {
        // ViewModels/ExamMonitorDetailsViewModel.cs
        public class ExamMonitorDetailsViewModel
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string NationalId { get; set; }
            public string Gender { get; set; }
            public string Status { get; set; }
            public string Region { get; set; }
            public string Centre { get; set; }
            public string District { get; set; }
            public string Phone { get; set; }

            // Formatted properties
            public string FormattedExperience { get; set; }
            public string FormattedQualification { get; set; }
        }
    }
}
