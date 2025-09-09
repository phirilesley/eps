using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using ExaminerPaymentSystem.Models;
using ExaminerPaymentSystem.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System;
using ExaminerPaymentSystem.Data;
using Microsoft.AspNetCore.Identity;

using ExaminerPaymentSystem.Interfaces.Other;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class ApportionmentController : Controller
    {

        private readonly IExamCodesRepository _examCodesRepository;
  
        private readonly IPaperMarkingRateRepository _subjectCodesRepository;
        private readonly IApportionmentRepository _apportionmentRepository;
        //private readonly IExaminerScriptsMarkedRepository _examinerRepository;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApportionmentController(IApportionmentRepository apportionmentRepository, IExamCodesRepository examCodesRepository, IPaperMarkingRateRepository subjectCodesRepository, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
          
            _examCodesRepository = examCodesRepository;
            _subjectCodesRepository = subjectCodesRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _apportionmentRepository = apportionmentRepository;
        }


        public async Task<IActionResult> Index(string subjectCode, string paperCode)
        {
            var examCodes = await _examCodesRepository.GetAllExamCodes();
            var subjectCodes = await _subjectCodesRepository.GetAllPaperCodes();
            var paperCodes = await _subjectCodesRepository.GetPaperCodeById(subjectCode);

            var apportionments = await _apportionmentRepository.GetNumberOfCandidatesBySubjectComponent(subjectCode, paperCode);

            var scriptsmarked = await _apportionmentRepository.GetNumberOfScriptsMarked(subjectCode, paperCode   );
            //return View(apportionments);


            ViewBag.ExamCodes = examCodes
             .Where(e => int.TryParse(e.EXM_EXAM_CODE, out int code) && code >= 100 && e.ACTIVATED_SESSION == "Activated") // Filter condition
             .Select(e => new
             {
                 e.EXM_EXAM_CODE,
                 ExamCaption = $"{e.EXM_EXAM_LEVEL} {e.EXM_EXAM_SESSION} {e.EXM_EXAM_YEAR}"
             })
             .ToList();

            ViewBag.SubjectCodes = subjectCodes.Select(e => new
            {
                PPR_SUB_SUB_ID = e.SUB_SUB_ID,
                SubjectCaption = $"{e.SUB_SUBJECT_DESC}"
            }).ToList();

            ViewBag.PpaperCodes = paperCodes.Select(e => new
            {
                e.PPR_PAPER_CODE,
                PaperCaption = $"{e.PPR_PAPER_CODE}"
            }).ToList();

            ViewBag.Apportionments = apportionments.Select(e => new
            {
                e.NUMBER_OF_CANDIDATES,
                NumberOfSubjectsCaption = $"{e.NUMBER_OF_CANDIDATES}"
            }).ToList();

            ViewBag.Scriptsmarked = scriptsmarked.Select(e => new
            {
                SCRIPT_MARKED = e.SCRIPTS_MARKED,
                NumberOfScriptsMarkedCaption = $"{e.SCRIPTS_MARKED}"
            });

            // Check if subjectCode is provided and get paper codes accordingly
            if (!string.IsNullOrEmpty(subjectCode))
            {
                ViewBag.PaperCodes = await _subjectCodesRepository.GetPaperCodeById(subjectCode);
            }

            return View();
        }

        // GET: Apportionment/GetNumberOfCandidatesBySubjectComponent
        [HttpGet]
        public async Task<IActionResult> GetNumberOfCandidatesBySubjectComponent(string subjectCode, string paperCode)
        {
            if (string.IsNullOrEmpty(subjectCode) || string.IsNullOrEmpty(paperCode))
            {
                return BadRequest("SubjectCode and PaperCode are required parameters.");
            }

            try
            {
                var apportionments = await _apportionmentRepository.GetNumberOfCandidatesBySubjectComponent(subjectCode, paperCode);

                // Check if apportionments were found
                if (apportionments == null || !apportionments.Any())
                {
                    return NotFound("No apportionments found for the specified SubjectCode and PaperCode.");
                }
                else
                {
                    return Json(apportionments);

                }
               
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: Apportionment/GetNumberOfScriptsMarked
        [HttpGet]
        public async Task<IActionResult> GetNumberOfScriptsMarked(string subjectCode, string paperCode)
        {
            if (string.IsNullOrEmpty(subjectCode) || string.IsNullOrEmpty(paperCode))
            {
                return BadRequest("SubjectCode and PaperCode are required parameters.");
            }

            try
            {
                var scriptsMarked = await _apportionmentRepository.GetNumberOfScriptsMarked(subjectCode, paperCode);

                // Calculate the sum of SCRIPTS_MARKED
                var sumScriptsMarked = scriptsMarked.Sum(e => e.SCRIPTS_MARKED);

                if (sumScriptsMarked == 0)
                {
                    return NotFound("No scripts found for the specified SubjectCode and PaperCode.");
                }
                else
                {
                    // Return the sum as JSON
                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            numberOfScriptsMarked = sumScriptsMarked
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    }
}
