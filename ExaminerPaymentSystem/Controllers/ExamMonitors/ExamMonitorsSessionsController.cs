using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class ExamMonitorsSessionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
