using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Materials
        public IActionResult Index()
        {
            return View();
        }

        // GET: Materials/GetMaterials (for DataTables)
        [HttpPost]
        public IActionResult GetMaterials([FromBody] DtParameters dtParameters)
        {
            try
            {
                var searchValue = dtParameters.Search?.Value;
                var orderColumnIndex = dtParameters.Order[0]?.Column;
                var orderDirection = dtParameters.Order[0]?.Dir;

                IQueryable<Material> query = _context.MaterialMaster;

                // Search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.Name.Contains(searchValue) ||
                        (m.Description != null && m.Description.Contains(searchValue))
                    );
                }

                // Ordering
                if (orderColumnIndex.HasValue)
                {
                    var propertyName = dtParameters.Columns[orderColumnIndex.Value].Data;
                    if (propertyName == "name")
                    {
                        query = orderDirection == "asc" ?
                            query.OrderBy(m => m.Name) :
                            query.OrderByDescending(m => m.Name);
                    }
                    else if (propertyName == "description")
                    {
                        query = orderDirection == "asc" ?
                            query.OrderBy(m => m.Description) :
                            query.OrderByDescending(m => m.Description);
                    }
                    else
                    {
                        query = orderDirection == "asc" ?
                            query.OrderBy(m => m.Id) :
                            query.OrderByDescending(m => m.Id);
                    }
                }

                // Paging
                var totalRecords = query.Count();
                var materials = query
                    .Skip(dtParameters.Start)
                    .Take(dtParameters.Length)
                    .Select(m => new
                    {
                        id = m.Id,
                        name = m.Name,
                        description = m.Description ?? string.Empty
                    })
                    .ToList();

                return Json(new
                {
                    draw = dtParameters.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = materials
                });
            }
            catch (Exception ex)
            {
      
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: Materials/Get/5
        public IActionResult Get(int id)
        {
            var material = _context.MaterialMaster.Find(id);
            if (material == null)
            {
                return NotFound();
            }
            return Json(material);
        }

        // POST: Materials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([FromForm] Material material)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(material);
                    _context.SaveChanges();
                    TempData["Success"] = "Material created successfully!";
                    return RedirectToAction(nameof(Index)); // Redirect to list view
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error creating material: " + ex.Message;
                    return View(material); // Return to form with error
                }
            }

            TempData["Error"] = "Invalid data submitted.";
            return View(material); // Return to form with validation errors
        }
        // POST: Materials/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([FromForm] Material material)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(material);
                    _context.SaveChanges();
                    TempData["Success"] = "Material updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error updating material: " + ex.Message;
                    return View(material);
                }
            }

            TempData["Error"] = "Invalid data submitted.";
            return View(material);
        }
        // POST: Materials/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var material = _context.MaterialMaster.Find(id);
                if (material == null)
                {
                    TempData["Error"] = "Material not found.";
                    return NotFound();
                }

                _context.MaterialMaster.Remove(material);
                _context.SaveChanges();
                TempData["Success"] = "Material deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting material: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }

    // DataTables parameters class
    public class DtParameters
    {
        public int Draw { get; set; }
        public DtColumn[] Columns { get; set; }
        public DtOrder[] Order { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public DtSearch Search { get; set; }
    }

    public class DtColumn
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public DtSearch Search { get; set; }
    }

    public class DtOrder
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }

    public class DtSearch
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class DtResult<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<T> Data { get; set; }
    }
}
