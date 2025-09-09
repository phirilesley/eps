using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.Common
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult PageNotFound()
        {
            return View();
        }

        [Route("Error/ServerError")]
        public IActionResult ServerError()
        {
            return View();
        }

        [Route("Error/{statusCode}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            if (statusCode == 404)
            {
                return RedirectToAction("PageNotFound");
            }
            return RedirectToAction("ServerError");
        }
    }
}
