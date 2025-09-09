using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return Content("Test controller is working!");
        }
    }
}
