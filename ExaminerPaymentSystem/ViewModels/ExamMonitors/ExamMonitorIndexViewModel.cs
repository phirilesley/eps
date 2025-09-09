using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Models.ExamMonitors.Dtos;
using System.Web.WebPages.Html;

namespace ExaminerPaymentSystem.ViewModels.ExamMonitors
{
    public class ExamMonitorIndexViewModel
    {
       public IEnumerable<ExamMonitorDTO> ExamMonitors { get; set; }

        // Additional properties for filtering/searching
        public string SearchString { get; set; }
        public string RegionFilter { get; set; }
        public IEnumerable<SelectListItem> Regions { get; set; }
    }
}
