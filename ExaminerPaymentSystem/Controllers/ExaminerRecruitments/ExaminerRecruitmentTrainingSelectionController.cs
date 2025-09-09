using DocumentFormat.OpenXml.Drawing.Charts;
using ExaminerPaymentSystem.Extensions;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;
using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.ExaminerRecruitments
{
    public class ExaminerRecruitmentTrainingSelectionController : Controller
    {
        private readonly IExaminerRecruitmentTrainingSelectionRepository _repository;
        private readonly IExaminerRecruitmentRepository _examiner_repository;
        private readonly IExaminerRecruitmentRegisterRepository _register_repository;

        public ExaminerRecruitmentTrainingSelectionController(IExaminerRecruitmentTrainingSelectionRepository repository
            ,IExaminerRecruitmentRepository examiner_repository,
             IExaminerRecruitmentRegisterRepository register_repository)
        {
            _repository = repository;
            _examiner_repository = examiner_repository;
            _register_repository = register_repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _repository.GetAllAsync();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        [HttpGet]
        public IActionResult SelectedExaminer()
        {
   
            return View();
        }

        [HttpGet]
        public IActionResult DeselectedExaminer()
        {
       
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> LoadExaminerRecruitments(string status = "")
        {
            // Retrieve DataTables parameters from the request
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault(); // asc or desc
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            // Default pagination values
            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Map DataTables column index to database column names
            string[] columnNames = { "ExaminerName", "LastName", "PaperCode", "Subject", "CemId", "ExaminerCode", "PhoneHome", "Id", "EmailAddress", "Gender" };
            string sortColumn = columnNames[Convert.ToInt32(sortColumnIndex)];

            // Retrieve custom filter parameters
            var sessionLevelFilter = Request.Form["sessionLevel"].FirstOrDefault();
            var subjectFilter = Request.Form["subject"].FirstOrDefault();
            var paperCodeFilter = Request.Form["paperCode"].FirstOrDefault();
            var regionCodeFilter = Request.Form["regionCode"].FirstOrDefault();

            // Call the repository method to get paginated, sorted, and filtered data
            var result = await _repository.GetPaginatedAsync(skip, pageSize, searchValue, sortColumn, sortColumnDirection, sessionLevelFilter, subjectFilter, paperCodeFilter, regionCodeFilter);

            if (!string.IsNullOrEmpty(status))
            {
                result.Data = status switch
                {
                    "Total" => result.Data.ToList(), // Convert to List for Total
                    "Present" => result.Data.Where(x => x.ExaminerRecruitment.Any(er => er.Status == true)).ToList(),
                    "Pending" => result.Data.Where(x => x.ExaminerRecruitment.Any(er => er.Status == null)).ToList(),
                    "Absent" => result.Data.Where(x => x.ExaminerRecruitment.Any(er => er.Status == false)).ToList(),
                    _ => result.Data.ToList() // Default case, convert to List
                };
            }

            // Prepare the JSON response
            var jsonData = new
            {
                draw = draw,
                recordsFiltered = result.TotalCount, // Filtered count
                recordsTotal = result.TotalCount,    // Total count
                data = result.Data,                   // Data to be displayed in the DataTable
                totalExaminersWithRegisterTrue = result.TotalExaminersWithRegisterTrue,
                totalExaminersWithRegisterFalse = result.TotalExaminersWithRegisterFalse,
                totalSelectedNotRegistered = result.TotalSelectedNotRegistered,
                totalSelectedInTrainerTab = result.TotalSelectedInTrainerTab
            };

            return Json(jsonData);
        }

        [HttpPost]
        public async Task<IActionResult> LoadDeselectedExaminerRecruitments()
        {
            // Retrieve DataTables parameters from the request
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault(); // asc or desc
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            // Default pagination values
            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;


            // Retrieve custom filter parameters
            var sessionLevelFilter = Request.Form["sessionLevel"].FirstOrDefault();
            var subjectFilter = Request.Form["subject"].FirstOrDefault();
            var paperCodeFilter = Request.Form["paperCode"].FirstOrDefault();
            var regionCodeFilter = Request.Form["regionCode"].FirstOrDefault();

            // Map DataTables column index to database column names
            string[] columnNames = { "ExaminerName", "LastName", "PaperCode", "Subject", "CemId", "ExaminerCode", "PhoneHome", "Id", "EmailAddress", "Gender" };
            string sortColumn = columnNames[Convert.ToInt32(sortColumnIndex)];

            // Call the repository method to get paginated, sorted, and filtered data
            var result = await _repository.GetDeselectedExaminersAsync(skip, pageSize, searchValue, sortColumn, sortColumnDirection, sessionLevelFilter, subjectFilter, paperCodeFilter, regionCodeFilter);

            // Prepare the JSON response
            var jsonData = new
            {
                draw = draw,
                recordsFiltered = result.TotalCount, // Filtered count
                recordsTotal = result.TotalCount,    // Assuming the total count matches the filtered count in this case
                data = result.Data                    // Data to be displayed in the DataTable
            };

            return Json(jsonData);
        }



        [HttpPost]
        public async Task<IActionResult> Create(
            ExaminerRecruitmentTrainingSelection entity,
            int ExaminerRecruitmentId,
            bool Status,
            DateTime Date
)
        {
            if (!ModelState.IsValid)
            {
                TempData["ExaminerSelectionError"] = "Error, Please try again";
                return RedirectToAction("Detail", "ExaminerRecruitment", new { id = ExaminerRecruitmentId });
            }

            // Populate the entity with the received parameters
            entity.ExaminerRecruitmentId = ExaminerRecruitmentId;
            entity.Status = Status;
            entity.Date = Date;

            try
            {
                await _repository.AddAsync(entity);

                // Set success message based on status
                TempData["SuccessMessage"] = Status
                    ? "Examiner selected successfully!"
                    : "Examiner rejected successfully!";
                

                return RedirectToAction("ExaminerIndex", "ExaminerRecruitment");
            }
            catch (Exception ex)
            {
                // Store the error message in TempData
                TempData["ExaminerSelectionError"] = "Error. The examiner is already selected for training: " + ex.Message;
                return RedirectToAction("Detail", "ExaminerRecruitment", new { id = ExaminerRecruitmentId });
            }
        }


        [HttpGet]
            public async Task<IActionResult> UnselectDetail(int id)
            {
                var recruitment = await _examiner_repository.GetByIdAsync(id);

                if (recruitment == null)
                {
                    return NotFound();  // Return 404 if no examiner with the provided ID exists
                }

                return View(recruitment);  // Return the examiner recruitment to the view
            }


        [HttpGet]
        public async Task<IActionResult> ReSelectDetail(int id)
        {
            var recruitment = await _examiner_repository.GetByIdAsync(id);

            if (recruitment == null)
            {
                return NotFound();  // Return 404 if no examiner with the provided ID exists
            }

            return View(recruitment);  // Return the examiner recruitment to the view
        }


        [HttpPost]
        public async Task<IActionResult> Update(
            int id,
            int ExaminerRecruitmentId,
            bool status,
            DateTime Date
)
        {
            var examinerRecruitment = await _repository.GetByExaminerRecruitmentIdAsync(ExaminerRecruitmentId);
        
            var examinerRecruitRegister = await _register_repository.FindByExaminerIdAsync(ExaminerRecruitmentId);

            if (examinerRecruitRegister != null && status == false)
            {
                TempData["ExaminerSelectionError"] = "You cannot unselect the trainee examiner who has already been recorded on the register.";
                return RedirectToAction("UnselectDetail", "ExaminerRecruitmentTrainingSelection", new { id = ExaminerRecruitmentId });
            }

            if (examinerRecruitment == null || examinerRecruitment.ExaminerRecruitmentId != ExaminerRecruitmentId)
            {
                TempData["ExaminerSelectionError"] = "Examiner not found";
                return RedirectToAction("UnselectDetail", "ExaminerRecruitmentTrainingSelection", new { id = ExaminerRecruitmentId });
            }

            if (!ModelState.IsValid)
            {
                TempData["ExaminerSelectionError"] = "Error, Please try again";
                return RedirectToAction("UnselectDetail", "ExaminerRecruitmentTrainingSelection", new { id = ExaminerRecruitmentId });
            }

            try
            {
                // Update the existing entity
                examinerRecruitment.Status = status;
                examinerRecruitment.Date = Date;

                // Use the existing entity's Id to ensure we're updating, not inserting
                await _repository.UpdateAsync(examinerRecruitment);

                // Set success message based on status
                TempData["SuccessMessage"] = status
                    ? "Examiner Reselected successfully!"
                    : "Examiner Deselected successfully!";

                // Redirect based on status
                return status
                    ? RedirectToAction("DeselectedExaminer")
                    : RedirectToAction("SelectedExaminer");
                

            }
            catch (Exception ex)
            {
                // Store the error message in TempData
                TempData["ExaminerSelectionError"] = "Error. " + ex.Message;
                return RedirectToAction("SelectedExaminer", "ExaminerRecruitmentTrainingSelection", new { id = ExaminerRecruitmentId });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingEntity = await _repository.GetByIdAsync(id);
            if (existingEntity == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(id);
            return NoContent();
        }


    }
}
