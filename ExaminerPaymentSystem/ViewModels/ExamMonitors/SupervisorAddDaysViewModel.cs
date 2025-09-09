using ExaminerPaymentSystem.Models.ExamMonitors;

namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{
    public class SupervisorAddDaysViewModel
    {
        public string Phase { get; set; }
        public string Session { get; set; }
        public string Region { get; set; }
        public string SelectedPersonId { get; set; }
        public string SelectedPersonName { get; set; }
        public string SelectedPersonSubKey { get; set; }
        public List<SupervisorDateModel> Dates { get; set; } = new List<SupervisorDateModel>();
        public List<PersonListItem> AvailablePeople { get; set; } = new List<PersonListItem>();
    }

    public class SupervisorDateModel
    {
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; } = "Approved"; // Default status
    }

    public class PersonListItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SubKey { get; set; }
    }

    public class SupervisorViewRegisterViewModel
    {
        public string MonitorName { get; set; }
        public string SubKey { get; set; }
        public List<ExamMonitorRegisterDate> TimetableDays { get; set; } = new List<ExamMonitorRegisterDate>();
        public List<ExamMonitorRegisterDate> SupervisorAddedDays { get; set; } = new List<ExamMonitorRegisterDate>();
    }
}
