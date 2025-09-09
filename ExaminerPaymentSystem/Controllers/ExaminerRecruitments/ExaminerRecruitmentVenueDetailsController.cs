using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Controllers.ExaminerRecruitments
{
    public class ExaminerRecruitmentVenueDetailsController : Controller
    {
        private readonly IExaminerRecruitmentVenueDetailsRepository _repository;
        private readonly ApplicationDbContext _context;
        
        public ExaminerRecruitmentVenueDetailsController(IExaminerRecruitmentVenueDetailsRepository repository, ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        // GET: api/ExaminerRecruitmentVenueDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExaminerRecruitmentVenueDetails>>> GetAll()
        {
            var venues = await _repository.GetAllAsync();
            return Ok(venues);
        }

        [HttpGet]
        public IActionResult VenueIndex()
        {
         return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetVenueDetails()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault(); // asc or desc
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            // Get column name from DataTables column index
            var columnMapping = new Dictionary<int, string>
            {
                { 0, "VenueName" },
                { 1, "TrainingStartDate" },
                { 2, "TrainingEndDate" },
                { 3, "CheckInDate" },
                { 4, "CheckOutDate" },
                { 5, "TrainingTime" }
            };

            // Resolve column name
            var sortColumn = columnMapping.ContainsKey(Convert.ToInt32(sortColumnIndex))
                ? columnMapping[Convert.ToInt32(sortColumnIndex)]
                : "VenueName"; // Default column

            var (data, totalRecords, filteredRecords) = await _repository.GetAllDataTableAsync(start, length, searchValue, sortColumn, sortColumnDirection);

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data
            });
        }




        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExaminerRecruitmentVenueDetails venueDetails)
        {
            // Check for duplicate VenueName
            var existingVenue = await _context.ExaminerRecruitmentVenueDetails
                .FirstOrDefaultAsync(v => v.VenueName == venueDetails.VenueName);


            if (existingVenue != null)
            {
                ModelState.AddModelError("VenueName", "Venue Name already exists.");
            }

            // Validate other rules
            if (venueDetails.TrainingStartDate > venueDetails.TrainingEndDate)
            {
                ModelState.AddModelError("TrainingStartDate", "Training Start Date cannot be greater than Training End Date.");
            }

            if (venueDetails.CheckInDate > venueDetails.CheckOutDate)
            {
                ModelState.AddModelError("CheckInDate", "Check-In Date cannot be greater than Check-Out Date.");
            }

          

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(venueDetails);
                TempData["SuccessMessage"] = "Venue details added successfully!"; // Set success message
                return RedirectToAction(nameof(VenueIndex));
            }

            return View(venueDetails);
        }


        // Edit Action
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var venueDetails = await _repository.GetByIdAsync(id);
            if (venueDetails == null)
            {
                return NotFound();
            }
            return View(venueDetails);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ExaminerRecruitmentVenueDetails model)
        {
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(model);
                TempData["SuccessMessage"] = "Venue details Updated successfully!"; // Set success message
                return RedirectToAction("VenueIndex"); // Redirect to the index or another relevant page
            }
            return View(model);
        }


        // DELETE: api/ExaminerRecruitmentVenueDetails/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
