using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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
    public class SubjectsController : Controller
    {
        private readonly IExamCodesRepository _examCodesRepository;
        private readonly ISubjectsRepository _subjectRepository;
        private readonly IPaperMarkingRateRepository _papercodesRepo;

        public SubjectsController(IExamCodesRepository examCodesRepository, ISubjectsRepository subjectRepository, IPaperMarkingRateRepository papercodesRepo)
        {
            _examCodesRepository = examCodesRepository;
            _subjectRepository = subjectRepository;
            _papercodesRepo = papercodesRepo;
        }

        [Authorize]
        public async Task<IActionResult> Index(string subjectCode)
        {
            var examCodes = await _examCodesRepository.GetAllExamCodes();
            var subjectCodes = await _subjectRepository.GetAllPaperCodes();
            //var paperCodes = await _subjectCodesRepository.GetPaperCodeById(subjectCode);

            ViewBag.ExamCodes = examCodes.Where(c => c.ACTIVATED_SESSION == "Activated").Select(e => new
            {
                e.EXM_EXAM_CODE,
                ExamCaption = $"{e.EXM_EXAM_LEVEL} {e.EXM_EXAM_SESSION} {e.EXM_EXAM_YEAR}"  
            }).ToList();

            ViewBag.SubjectCodes = subjectCodes.Select(e => new
            {
                e.SUB_SUB_ID,
                SubjectCaption = $"{e.SUB_SUBJECT_DESC}"
            }).ToList();

            // Check if subjectCode is provided and get paper codes accordingly
            //if (!string.IsNullOrEmpty(subjectCode))
            //{
            //    ViewBag.PaperCodes = await _subjectRepository.GetPaperCodeById(subjectCode);
            //}

            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetPaperCodes(string subjectCode)
        {
            var examCode = subjectCode.Substring(0, 3);
            var subject = subjectCode.Substring(3);
            IEnumerable<PaperMarkingRate> data = await _papercodesRepo.GetPaperCodesBySubject(subject, examCode);
            return Json(data);
        }

       
    }
}
