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
    public class CategoryRateController : Controller 
    {
        //private readonly IExaminerCodesRepository _examinerCodesRepository;
        private readonly ICategoryRateRepository _categoryCodesRepository;

        public CategoryRateController(ICategoryRateRepository categoryCodesRepository)
        {
            //_examinerCodesRepository = examinerCodesRepository;
            _categoryCodesRepository = categoryCodesRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index(string examType)
        {
            var examinerCodes = await _categoryCodesRepository.GetAllExaminerCodes();
            var examTypes = await _categoryCodesRepository.GetAllExamTypes();
            var paperCodes = await _categoryCodesRepository.GetCategoryRatesByExamType(examType);

            //ViewBag.ExaminerCodes = examinerCodes.Select(e => new
            //{
            //    //toddycedit here
            //    EMS_ECT_EXAMINER_CAT_CODE = e.EMS_ECT_EXAMINER_CAT_CODE,
            //    ExaminerCaption = $"{e.EMS_ECT_EXAMINER_CAT_CODE} "
            //}).ToList();
            ViewBag.ExaminerCodes = examinerCodes
                .GroupBy(e => e.EMS_ECT_EXAMINER_CAT_CODE) // Group by EMS_ECT_EXAMINER_CAT_CODE
                .Select(g => g.First()) // Select the first item from each group to avoid duplicates
                .Select(e => new
                {
                    //toddycedit here
                    e.EMS_ECT_EXAMINER_CAT_CODE,
                    ExaminerCaption = $"{e.EMS_ECT_EXAMINER_CAT_CODE} "
                }).ToList();

            //ViewBag.ExamTypes = examTypes.Select(e => new
            //{
            //    PPR_EXAM_TYPE = e.PPR_EXAM_TYPE,
            //    ExamTypeCaption = $"{e.PPR_EXAM_TYPE}"
            //}).ToList();

            ViewBag.ExamTypes = examTypes.Select(e => e.PPR_EXAM_TYPE).Distinct().ToList();

            // Check if examType is provided
            if (!string.IsNullOrEmpty(examType))
            {
                ViewBag.SelectedExamType = examType; // Pass the selected exam type to the view
            }

            // Check if subjectCode is provided and get paper codes accordingly
            if (!string.IsNullOrEmpty(examType))
            {
                ViewBag.PaperCodes = await _categoryCodesRepository.GetCategoryRatesByExamType(examType);
            }

            return View();
        }

        //[HttpGet]
        //public async Task<IActionResult> GetExamTypesByExaminerCategoryCode(string examinercategorycode)
        //{
        //    // Implement logic to fetch subjects based on the selected exam session
        //    var examtypes = await _categoryCodesRepository.GetExamTypesByExaminerCategoryCode(examinercategorycode);
        //    return Json(examtypes);
        //}
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetExamTypesByExaminerCategoryCode(string examinerCategoryCode)
        {
            var examTypes = await _categoryCodesRepository.GetExamTypesByExaminerCategoryCode(examinerCategoryCode);

            return Json(examTypes);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCategoryRatesByExamType(string examType, string examinercategorycode)
        {
            // Implement logic to fetch subjects based on the selected exam session
            var papercodes = await _categoryCodesRepository.GetCategoryRatesByExamType(examType, examinercategorycode);
            //ViewBag.papercodes = papercodes;
            return Json(papercodes);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UpdateCategoryMarkingRate([FromBody] JsonObject jsonData)
        {
            try
            {
                // Check if jsonData is not null and log the JSON data
                if (jsonData != null)
                {
                    var PPR_EXAM_TYPE = jsonData["PPR_EXAM_TYPE"].ToString();
                    var CAT_CODE = jsonData["EMS_ECT_EXAMINER_CAT_CODE"].ToString();

                    List<CategoryRate> rates = System.Text.Json.JsonSerializer.Deserialize<List<CategoryRate>>(jsonData["tableData"].ToString());
                    string jsonString = jsonData.ToString();

                    await _categoryCodesRepository.UpdateCategoryMarkingRates(rates,CAT_CODE,PPR_EXAM_TYPE);

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
