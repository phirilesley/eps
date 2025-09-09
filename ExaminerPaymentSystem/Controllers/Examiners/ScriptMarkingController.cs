using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class ScriptMarkingController : Controller
    {
        private readonly IExaminerRepository _examinerRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public ScriptMarkingController(IExaminerRepository examinerRepository, SignInManager<ApplicationUser> signInManager)
        {
            _examinerRepository = examinerRepository;
            _signInManager = signInManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
