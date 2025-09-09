using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace ExaminerPaymentSystem.Controllers.Common
{
    public class BankController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBanksRepository _banksRepository;

        public BankController(ApplicationDbContext context,IBanksRepository banksRepository)
        {
            _context = context;
            _banksRepository = banksRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
    
            return View();

        }


        [Authorize]
        public async Task<IActionResult> GetData()
        {
            IEnumerable<BankData> model = new List<BankData>();
            var modelList = await _banksRepository.GetAllBanksData();

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
    (p.B_BANK_CODE?.ToLower().Contains(searchValue.ToLower()) ?? false) ||
    (p.B_BANK_NAME?.ToLower().Contains(searchValue.ToLower()) ?? false)
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


            [Authorize]
        public IActionResult GetBanks()
        {
            var banks = _context.BANKING_DATA
                .GroupBy(b => new { b.B_BANK_CODE, b.B_BANK_NAME })
                .Select(g => new
                {
                    g.Key.B_BANK_CODE,
                    g.Key.B_BANK_NAME
                }).ToList();

            // Wrap the data in a JSON object with a "data" property
            return Json(new { data = banks });
        }

        [Authorize]
        // This method returns branches for a specific bank
        public IActionResult GetBranches(string bankCode)
        {
            var branches = _context.BANKING_DATA
                .Where(b => b.B_BANK_CODE == bankCode)
                .Select(b => new BankData
                {
                    BB_BRANCH_NAME = b.BB_BRANCH_NAME,
                    BB_BRANCH_CODE = b.BB_BRANCH_CODE
                }).ToList();

            // Wrap the data in a JSON object with a "data" property
            return Json(new { data = branches });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(BankData model)
        {
            if (ModelState.IsValid)
            {
                var existingBank = await _banksRepository.GetBankDataByParameter(model.B_BANK_CODE, model.BB_BRANCH_CODE);

                if (existingBank != null && existingBank.Id != model.Id)
                {
                    ViewBag.HasError = "Bank already exists";
                    return View(model);
                }

                if (model.Id == 0)
                {
                    // Add new bank
                    var newBank = await _banksRepository.SaveBank(model);
                    if (newBank != null)
                    {
                        ViewBag.HasSuccess = "Bank created successfully";
                    }
                }
                else
                {
                    // Update existing bank
                    var updatedBank = await _banksRepository.UpdateBank(model);
                    if (updatedBank != null)
                    {
                        ViewBag.HasSuccess = "Bank updated successfully";
                    }
                }

                return Redirect("/Bank/Index");
            }

            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var bank = await _banksRepository.GetBankById(id);
            if (bank == null)
            {
                return NotFound();
            }
            return View(bank);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _banksRepository.DeleteBank(id);
            return Redirect("/Bank/Index");
        }

    }
}
