using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class ExamSessionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamSessionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ExamCodes
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetExamCodes([FromBody] DtParameters dtParameters)
        {
            try
            {
                var searchValue = dtParameters.Search?.Value;
                var orderColumnIndex = dtParameters.Order[0]?.Column;
                var orderDirection = dtParameters.Order[0]?.Dir;

                IQueryable<ExamCodes> query = _context.CAN_EXAM;

                // Search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(e =>
                        e.EXM_EXAM_CODE.Contains(searchValue) ||
                        e.EXM_EXAM_SESSION.Contains(searchValue) ||
                        e.EXM_EXAM_YEAR.Contains(searchValue) ||
                        e.EXM_EXAM_LEVEL.Contains(searchValue)
                    );
                }

                // Ordering
                if (orderColumnIndex.HasValue)
                {
                    var propertyName = dtParameters.Columns[orderColumnIndex.Value].Data;
                    if (propertyName == "exm_EXAM_CODE")
                        query = orderDirection == "asc" ? query.OrderBy(e => e.EXM_EXAM_CODE) : query.OrderByDescending(e => e.EXM_EXAM_CODE);
                    else if (propertyName == "exm_EXAM_SESSION")
                        query = orderDirection == "asc" ? query.OrderBy(e => e.EXM_EXAM_SESSION) : query.OrderByDescending(e => e.EXM_EXAM_SESSION);
                    else if (propertyName == "exm_EXAM_YEAR")
                        query = orderDirection == "asc" ? query.OrderBy(e => e.EXM_EXAM_YEAR) : query.OrderByDescending(e => e.EXM_EXAM_YEAR);
                    else
                        query = orderDirection == "asc" ? query.OrderBy(e => e.ID) : query.OrderByDescending(e => e.ID);
                }

                // Paging
                var totalRecords = query.Count();
                var examCodes = query
                    .Skip(dtParameters.Start)
                    .Take(dtParameters.Length)
                    .Select(e => new
                    {
                        id = e.ID,
                        exm_EXAM_CODE = e.EXM_EXAM_CODE,
                        exm_EXAM_SESSION = e.EXM_EXAM_SESSION,
                        exm_EXAM_YEAR = e.EXM_EXAM_YEAR,
                        exm_EXAM_LEVEL = e.EXM_EXAM_LEVEL,
                        exm_STATUS = e.ACTIVATED_SESSION,
                        exm_START_DATE = e.EXM_START_DATE,
                        exm_CLOSE_DATE = e.EXM_CLOSE_DATE,
                        activated_SESSION = e.ACTIVATED_SESSION
                    })
                    .ToList();

                return Json(new
                {
                    draw = dtParameters.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = examCodes
                });
            }
            catch (Exception ex)
            {
        
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: ExamCodes/Get/5
        public IActionResult Get(int id)
        {
            var examCode = _context.CAN_EXAM.Find(id);
            if (examCode == null)
            {
                return NotFound();
            }
            return Json(examCode);
        }

        // POST: ExamCodes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([FromForm] ExamCodes examCode)
        {
            if (ModelState.IsValid)
            {
                _context.Add(examCode);
                _context.SaveChanges();
                return Ok();
            }
            return BadRequest(ModelState);
        }

        // POST: ExamCodes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([FromForm] ExamCodes examCode)
        {
            if (ModelState.IsValid)
            {
                _context.Update(examCode);
                _context.SaveChanges();
                return Ok();
            }
            return BadRequest(ModelState);
        }

        // POST: ExamCodes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var examCode = _context.CAN_EXAM.Find(id);
            if (examCode != null)
            {
                _context.CAN_EXAM.Remove(examCode);
                _context.SaveChanges();
                return Ok();
            }
            return NotFound();
        }
    }
}
