using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class CheckInCheckOutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckInCheckOutController(ApplicationDbContext context)
        {
                _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCategories([FromBody] DtRequest request)
        {
            try
            {
                // If request is null, return empty response
                if (request == null)
                {
                    return Json(new
                    {
                        draw = 0,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>() // Empty list
                    });
                }

                IQueryable<CategoryCheckInCheckOut> query = _context.CATEGORYCHECKINCHECKOUT;

                // Apply filters from search form
                if (!string.IsNullOrEmpty(request.ExamCode))
                {
                    query = query.Where(x => x.SubSubId.StartsWith(request.ExamCode));
                }

                if (!string.IsNullOrEmpty(request.SubjectCode))
                {
                    query = query.Where(x => x.SubSubId.Substring(3) == request.SubjectCode);
                }

                if (!string.IsNullOrEmpty(request.PaperCode))
                {
                    query = query.Where(x => x.PaperCode == request.PaperCode);
                }

                if (!string.IsNullOrEmpty(request.RegionCode))
                {
                    query = query.Where(x => x.REGION == request.RegionCode);
                }

                // Get total count before paging
                var totalRecords = await query.CountAsync();

                // Apply paging and execute query asynchronously
                var data = await query
                    .Skip(request.Start)
                    .Take(request.Length)
                    .Select(x => new
                    {
                        id = x.Id,
                        subSubId = x.SubSubId,
                        paperCode = x.PaperCode,
                        region = x.REGION,
                        category = x.Category,
                        checkIn = x.CheckIn,
                        checkOut = x.CheckOut
                    })
                    .ToListAsync();

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = data
                });
            }
            catch (Exception ex)
            {
                // Consider logging the exception here
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }



        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([FromForm] CategoryCheckInCheckOut model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var record = _context.CATEGORYCHECKINCHECKOUT.Find(model.Id);
                if (record == null)
                {
                    return NotFound();
                }

                // Update properties
                record.SubSubId = model.SubSubId;
                record.PaperCode = model.PaperCode;
                record.REGION = model.REGION;
                record.Category = model.Category;
                record.CheckIn = model.CheckIn;
                record.CheckOut = model.CheckOut;

                _context.Update(record);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
             
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class DtRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string ExamCode { get; set; }
        public string SubjectCode { get; set; }
        public string PaperCode { get; set; }
        public string RegionCode { get; set; }
    }

}
