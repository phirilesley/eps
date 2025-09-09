using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class ExamMonitorReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
