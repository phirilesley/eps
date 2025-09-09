using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.Finance
{
    public class ExaminerTransactionsController : Controller
    {
        public IActionResult TandSTransactions()
        {
            return View();
        }

        public IActionResult ScriptsMarkedTransactions()
        {
            return View();
        }
    }
}
