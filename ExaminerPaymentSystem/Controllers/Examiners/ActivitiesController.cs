using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class ActivitiesController : Controller
    {
        private IVenueRepository _venueRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public ActivitiesController(IVenueRepository venueRepository, ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _venueRepository = venueRepository;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {



            return View();



        }



        [Authorize]
        public async Task<IActionResult> GetData()
        {


            var modelList = await _context.Activities.ToListAsync();
            IEnumerable<Activity> model = new List<Activity>();

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
    (p.Text?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.Value?.ToLower().Contains(searchValue.ToLower()) ?? false)
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
        public async Task<IActionResult> ToggleStatus(string value, string status)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var venue = await _context.Activities.FirstOrDefaultAsync(a => a.Value == value);
            if (venue == null) return NotFound();

            venue.Status = status;
            _context.Update(venue);
            await _context.SaveChangesAsync(currentUser.Id);

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
        public async Task<IActionResult> AddActivity(Activity model)
        {
            try
            {
                var check = await _context.Activities.FirstOrDefaultAsync(a => a.Text == model.Text);

                if (check != null)
                {
                    ViewBag.HasError = "Activity Already Exist";
                    return View();
                }

                var check2 = await _context.Activities.FirstOrDefaultAsync(a => a.Value == model.Value);

                if (check2 != null)
                {
                    ViewBag.HasError = "Activity code Already Exist";
                    return View();
                }
                model.Status = "Deactivated";

                var venue = _context.Activities.Add(model);
                await _context.SaveChangesAsync();

                ViewBag.HasSuccess = "Activity Create Successfully";


                return Redirect("/Activities/Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating material: " + ex.Message;
                return Redirect("/Activities/Index"); // Return to form with error
            }
        }
    }

}
