using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class BrailleTranscriptionRateController : Controller
    {

        private readonly IBrailleTranscriptionRateRepository _repository;

        public BrailleTranscriptionRateController(IBrailleTranscriptionRateRepository repository)
        {
            _repository = repository;
        }

        [Authorize]
        public async Task<IActionResult> Index(bool isSuccess = false)
        {
            var braille = await _repository.GetAllAsync();
            var data = braille.FirstOrDefault();
            ViewBag.IsSuccess = isSuccess;

            return View(data);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateBrailleTranscriptionRate(BrailleTranscriptionRate braille)
        {
            await _repository.UpdateAsync(braille);
            return RedirectToAction("Index", new { isSuccess = true });
        }
    }
}
