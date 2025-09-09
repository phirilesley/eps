using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class VenueController : Controller
    {
        private IVenueRepository _venueRepository;
        private readonly ApplicationDbContext _context;
        public VenueController(IVenueRepository venueRepository, ApplicationDbContext context)
        {
            _venueRepository = venueRepository;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {



            return View();


          
        }



        [Authorize]
        public async Task<IActionResult> GetData()
        {
          

            var modelList = await _venueRepository.VenuesGetAll();
            IEnumerable<Venue> model = new List<Venue>();

            model = modelList;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 50;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            if (!string.IsNullOrEmpty(searchValue))
            {
                model = model.Where(p =>
    (p.Name?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.Region?.ToLower().Contains(searchValue.ToLower()) ?? false)
);

            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumnDir == "asc")
                {
                    model = model.OrderBy(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
                else
                {
                    model = model.OrderByDescending(p =>
                        p.GetType().GetProperty(sortColumn)?.GetValue(p, null) ?? string.Empty
                    );
                }
            }

            var totalRecords = model.Count();

            var data = model.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                data
            };

            return Ok(jsonData);

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleStatus(int id, string status)
        {
            var venue = await _context.VENUES.FirstOrDefaultAsync(a => a.Id == id);
            if (venue == null) return NotFound();

            venue.Status = status;
            _context.Update(venue);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddVenue(Venue model)
        {
            var checkvenue = await _venueRepository.GetVenueByNameID(model.Name);

            if (checkvenue != null)
            {
                ViewBag.HasError = "Venue Already Exist";
                return View();
            }
            model.Status = "Active";

          var venue =  await _venueRepository.SaveVenue(model);
            if (venue != null)
            {
                ViewBag.HasSuccess = "Venue Create Successfully";
            }
           
            return Redirect("/Venue/Index");
        }
        }
}
