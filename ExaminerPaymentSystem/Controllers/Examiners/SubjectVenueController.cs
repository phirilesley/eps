using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class SubjectVenueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVenueRepository _venueRepository;

        public SubjectVenueController(ApplicationDbContext context,IVenueRepository venueRepository)
        {
            _context = context;
            _venueRepository = venueRepository;
        }

       

        // GET: SubjectVenue (List all)
        public async Task<IActionResult> Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> SearchSubjectVenue(string examCode, string subjectCode, string paperCode,string regionCode)
        {
            // Your existing search logic

            ViewBag.ShowUpdateForm = true;
            ViewBag.SelectedExamCode = examCode;
            ViewBag.SelectedSubjectCode = subjectCode.Substring(3);
            ViewBag.SelectedPaperCode = paperCode;
            ViewBag.SelectedRegionCode = regionCode;

            var model = new SubjectVenue();

            if (!string.IsNullOrEmpty(regionCode))
            {
                model = await _context.SubjectVenue
                     .FirstOrDefaultAsync(sv => sv.Subject == subjectCode.Substring(3) && sv.PaperCode == paperCode && sv.ExamCode == examCode && sv.Region == regionCode);

            }
            else
            {
                model = await _context.SubjectVenue
                    .FirstOrDefaultAsync(sv => sv.Subject == subjectCode.Substring(3) && sv.PaperCode == paperCode && sv.ExamCode == examCode);

            }
            var venues = await _venueRepository.VenuesGetAll();
            ViewBag.Venues = venues.Where(a => a.Status == "Active")
                        .Select(a => new { Text = a.Name, Value = a.Name })
                        .ToList();

            return View("Index", model);
        }

        // GET: SubjectVenue/Create (Display form)
        public IActionResult Create()
        {
            return View();
        }

        // POST: SubjectVenue/Create (Save data)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Subject,PaperCode,Venue")] SubjectVenue subjectVenue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subjectVenue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subjectVenue);
        }

        // GET: SubjectVenue/Edit/5 (Edit form)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectVenue = await _context.SubjectVenue.FindAsync(id);
            if (subjectVenue == null)
            {
                return NotFound();
            }
            return View(subjectVenue);
        }

        // POST: SubjectVenue/Edit/5 (Update data)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Subject,PaperCode,Venue")] SubjectVenue subjectVenue)
        {
            if (id != subjectVenue.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subjectVenue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectVenueExists(subjectVenue.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(subjectVenue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSubjectVenue(SubjectVenue model)
        {
            try
            {
                var existing = await _context.SubjectVenue
    .FirstOrDefaultAsync(a => a.Subject == model.Subject &&
                             a.PaperCode == model.PaperCode &&
                             a.ExamCode == model.ExamCode &&
                             (string.IsNullOrEmpty(model.Region) || a.Region == model.Region));

                if (existing != null)
                {
                    // Update logic
                    existing.Venue = model.Venue;
                    _context.SubjectVenue.Update(existing);
                    TempData["Success"] = "Venue updated successfully.";
                }
                else
                {

                    var data = new SubjectVenue()
                    {
                        Venue = model.Venue,
                        ExamCode = model.ExamCode,
                        PaperCode = model.PaperCode,
                        Subject = model.Subject,
                  
                    };

                    if (!string.IsNullOrEmpty(model.Region))
                    {
                        data.Region = model.Region;
                    }

                        // Add new logic
                        _context.SubjectVenue.Add(data);
                    TempData["Success"] = "Venue added successfully.";
                }

                await _context.SaveChangesAsync();



                return View("Index", model);
            }
            catch (Exception)
            {

                throw;
            }
                
               
        }


        // GET: SubjectVenue/Delete/5 (Delete confirmation)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectVenue = await _context.SubjectVenue.FirstOrDefaultAsync(m => m.Id == id);
            if (subjectVenue == null)
            {
                return NotFound();
            }
            return View(subjectVenue);
        }

        // POST: SubjectVenue/Delete/5 (Delete record)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subjectVenue = await _context.SubjectVenue.FindAsync(id);
            if (subjectVenue != null)
            {
                _context.SubjectVenue.Remove(subjectVenue);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectVenueExists(int id)
        {
            return _context.SubjectVenue.Any(e => e.Id == id);
        }
    }
}
