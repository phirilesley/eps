using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ExaminerPaymentSystem.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System;
using Microsoft.AspNetCore.Authorization;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class PaperMarkingRateController : Controller
    {
        private readonly IExamCodesRepository _examCodesRepository;
        private readonly IPaperMarkingRateRepository _subjectCodesRepository;

        public PaperMarkingRateController(IExamCodesRepository examCodesRepository, IPaperMarkingRateRepository subjectCodesRepository)
        {
            _examCodesRepository = examCodesRepository;
            _subjectCodesRepository = subjectCodesRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index(string subjectCode)
        {
            var examCodes = await _examCodesRepository.GetAllExamCodes();
            var subjectCodes = await _subjectCodesRepository.GetAllPaperCodes();
            var paperCodes = await _subjectCodesRepository.GetPaperCodeById(subjectCode);

            ViewBag.ExamCodes = examCodes
                .Where(e => int.Parse(e.EXM_EXAM_CODE) >= 100 && e.ACTIVATED_SESSION == "Activated")
                .Select(e => new
            {
                e.EXM_EXAM_CODE,
                ExamCaption = $"{e.EXM_EXAM_LEVEL} {e.EXM_EXAM_SESSION} {e.EXM_EXAM_YEAR}"
            }).ToList();

            ViewBag.SubjectCodes = subjectCodes.Select(e => new
            {
                PPR_SUB_SUB_ID = e.SUB_SUB_ID,
                SubjectCaption = $"{e.SUB_SUBJECT_DESC}"
            }).ToList();

            // Check if subjectCode is provided and get paper codes accordingly
            if (!string.IsNullOrEmpty(subjectCode))
            {
                ViewBag.PaperCodes = await _subjectCodesRepository.GetPaperCodeById(subjectCode);
            }

            return View();
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetSubjectsByExamSession(string examcode)
        {
            // Implement logic to fetch subjects based on the selected exam session
            var subjects = await _subjectCodesRepository.GetSubjectsByExamSession(examcode);
            return Json(subjects);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetPaperCodesBySubject(string subject, string examcode)
        {
            // Implement logic to fetch subjects based on the selected exam session
            var papercodes = await _subjectCodesRepository.GetPaperCodesBySubject(subject, examcode);
            //ViewBag.papercodes = papercodes;
            return Json(papercodes); 
        }
        /*
         [HttpPost]
         public async Task<ActionResult> UpdatePaperMarkingRate(string jsonData)
         {

             // Save the new Examiner using the repository
             //await _subjectCodesRepository.UpdatePaperMarkingRate();

             // Redirect to a success page or back to the form page
             return RedirectToAction("Index");

         }
*/
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UpdatePaperMarkingRate([FromBody] JsonObject jsonData)
        {
            try
            {
                // Check if jsonData is not null and log the JSON data
                if (jsonData != null)
                {
                    var PPR_SUB_SUB_ID = jsonData["SUB_SUB_ID"];
                    List<PaperMarkingRate> rates = System.Text.Json.JsonSerializer.Deserialize<List<PaperMarkingRate>>(jsonData["tableData"].ToString());
                    string jsonString = jsonData.ToString();

                    await _subjectCodesRepository.UpdatePaperMarkingRates(rates, PPR_SUB_SUB_ID.GetValue<string>());

                    // Process and manipulate the JSON data as needed

                    // Save data to the database or perform other operations

                    return Json(new { success = true, message = "Data saved successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "JSON data is null" });
                }
            }
            catch (Exception ex)
            {
                // Handle exception and return error response
                return Json(new { success = false, message = "An error occurred while updating paper marking rate." + ex.Message });
            }
        }
    }
}
