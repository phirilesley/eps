using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Repositories;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExaminerPaymentSystem.Controllers.ExaminerRecruitments
{
    public class ExaminerRecruitmentController : Controller
    {
        private readonly IExaminerRecruitmentRepository _repository;
        private readonly   ApplicationDbContext _context;
        private readonly ISubjectsRepository _subjects;
        private readonly ITeachingExperienceRepository _teachingExperienceRepository;
        private readonly IExaminerRecruitmentAttachmentsRepository _attachmentRepository;
        private readonly IExaminerRecruitmentProfessionalQualifications _professionalQualifications;

        public ExaminerRecruitmentController(IExaminerRecruitmentRepository repository, ISubjectsRepository subjects,
            ITeachingExperienceRepository teachingExperienceRepository, IExaminerRecruitmentAttachmentsRepository attachmentRepository,
            ApplicationDbContext context, IExaminerRecruitmentProfessionalQualifications professionalQualifications)
        {
            _repository = repository;
            _subjects = subjects;
            _teachingExperienceRepository = teachingExperienceRepository;
            _attachmentRepository = attachmentRepository;
            _context = context;
            _professionalQualifications = professionalQualifications;
        }

        [HttpGet]
        public IActionResult ExaminerIndex()
        {
            return View();
        }


        [HttpPost]

        public async Task<IActionResult> LoadExaminerRecruitments()
        {
            // Retrieve DataTables parameters
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault(); // asc or desc
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            // Retrieve custom filter parameters
            var sessionLevelFilter = Request.Form["sessionLevel"].FirstOrDefault();
            var subjectFilter = Request.Form["subject"].FirstOrDefault();
            var paperCodeFilter = Request.Form["paperCode"].FirstOrDefault();
            var regionCodeFilter = Request.Form["regionCode"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Map DataTables column index to database column names
            string[] columnNames = { "ExaminerName", "LastName", "PaperCode", "Subject", "CemId", "ExaminerCode", "PhoneHome", "Id", "EmailAddress", "Gender" };
            string sortColumn = columnNames[Convert.ToInt32(sortColumnIndex)];

            // Fetch paginated, sorted, and filtered data
            var result = await _repository.GetPaginatedAsync(skip, pageSize, searchValue, sortColumn, sortColumnDirection, sessionLevelFilter, subjectFilter, paperCodeFilter, regionCodeFilter);

            // Prepare JSON response
            var jsonData = new
            {
                draw = draw,
                recordsFiltered = result.TotalCount,
                recordsTotal = result.TotalCount,
                data = result.Data
            };

            return Json(jsonData);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var recruitment = await _repository.GetByIdAsync(id);

            if (recruitment == null)
            {
                return NotFound();  // Return 404 if no examiner with the provided ID exists
            }

            return View(recruitment);  // Return the examiner recruitment to the view
        }


        public IActionResult DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound("File name is not provided.");
            }

            // Remove any leading slashes
            fileName = fileName.TrimStart('/');

            // Construct the file path
         
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName.TrimStart('/'));


            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = "application/octet-stream";
            var downloadFileName = Path.GetFileName(filePath);

            return File(fileBytes, contentType, downloadFileName);
        }

        public IActionResult PreviewFile(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound("File name is not provided.");
            }

            // Remove any leading slashes
            fileName = fileName.TrimStart('/');

            // Construct the file path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            // Get the file extension
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            // Set appropriate content type based on file extension
            string contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".txt" => "text/plain",
                _ => "application/octet-stream" // Fallback for unknown types
            };

            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // For PDF, explicitly set content disposition to inline
            if (extension == ".pdf")
            {
                var fileStreamResult = new FileContentResult(fileBytes, contentType)
                {
                    FileDownloadName = null // Ensure no download is forced
                };
                Response.Headers["Content-Disposition"] = "inline";
                return fileStreamResult;
            }

            return File(fileBytes, contentType);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Generate a unique value for the examiner code
            string uniqueExaminerCode = GenerateUniqueExaminerCode(); // Custom method to generate a unique code

            // Optionally, create other default values for your fields
            string Experience = "5"; // Example default value
            string AcademiicQualification = "Diploma";
            string Addess2 = "Adree2";

            // Pass the values to the view
            ViewBag.UniqueExaminerCode = uniqueExaminerCode;
            ViewBag.AcademicQualification = AcademiicQualification;
            ViewBag.Addess2 = Addess2;

            return View();
        }

        private string GenerateUniqueExaminerCode()
        {
            // Create a random number generator
            Random random = new Random();

            // Generate a random 5-digit number
            int code = random.Next(10000, 99999);  // Generates a number between 10000 and 99999

            return code.ToString();  // Return the 5-digit code as a string
        }





        [HttpPost]
        public async Task<IActionResult> Create(ExaminerRecruitmentViewModel model)
        {
            //ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

            var emailExists = _context.ExaminerRecruitment.Any(u => u.EmailAddress == model.EmailAddress);
            var phoneExists = _context.ExaminerRecruitment.Any(u => u.PhoneHome == model.PhoneHome);
            var nationalIdExists = _context.ExaminerRecruitment.Any(u => u.CemId == model.CemId);

            if (emailExists)
            {
                ModelState.AddModelError("EmailAddress", "The email address is already taken.");
            }

            if (phoneExists)
            {
                ModelState.AddModelError("PhoneHome", "The phone number is already taken.");
            }

            if (nationalIdExists)
            {
                ModelState.AddModelError("CemId", "The National ID is already taken.");
            }

            if (ModelState.ErrorCount > 0)
            {
                ViewBag.UniqueExaminerCode = GenerateUniqueExaminerCode();
                return View(model);
            }

            if (model.DateOfBirth > DateTime.Today.AddYears(-20) || model.DateOfBirth > DateTime.Today)
            {
                ModelState.AddModelError("Date Of Birth", "You must be at least 20 years old.");
            }



            if (ModelState.IsValid)
            {
                try
                {
                    // Save PersonalDetails
                    var examinerRecruitmentDetails = new ExaminerRecruitment
                    {
                        ExaminerCode = model.ExaminerCode,
                        ExaminerName = model.ExaminerName?.ToUpper(),
                        LastName = model.LastName?.ToUpper(),
                        CemId = model.CemId?.ToUpper(),
                        Sex = model.Sex?.ToUpper(),
                        DateOfBirth = model.DateOfBirth, // Date remains unchanged
                        PhoneHome = model.PhoneHome,
                        Address = model.Address?.ToUpper(),
                        Address2 = model.Address2?.ToUpper(),
                        Qualification = model.Qualification?.ToUpper(),
                        EmailAddress = model.EmailAddress, // Email remains unchanged
                        AcademicQualification = model.AcademicQualification?.ToUpper(),
                        Experience = model.Experience?.ToUpper(),
                        PhoneBusiness = model.PhoneBusiness,
                        RegionCode = model.RegionCode,
                        DistrictCode = model.DistrictCode?.ToUpper(),
                        Subject = model.Subject?.ToUpper(),
                        PaperCode = model.PaperCode,
                        WorkAddress1 = model.WorkAddress1?.ToUpper(),
                        WorkAddress2 = model.WorkAddress2?.ToUpper(),
                        WorkAddress3 = model.WorkAddress3?.ToUpper(),
                        TrainingCentre = model.TrainingCentre?.ToUpper(),
                        CaptureDate = DateTime.UtcNow
                    };


                    await _repository.AddAsync(examinerRecruitmentDetails);


                    // Save TeachingExperiences
                    if (model.TeachingExperiences != null && model.TeachingExperiences.Any())
                    {
                        foreach (var experience in model.TeachingExperiences)
                        {
                            var teachingExperience = new TeachingExperience
                            {
                                ExaminerRecruitmentId = examinerRecruitmentDetails.Id,  // Make sure the Id is populated before saving
                                LevelTaught = experience.LevelTaught?.ToUpper(),
                                Subject = experience.Subject,
                                ExperienceYears = experience.ExperienceYears,
                                InstitutionName = experience.InstitutionName?.ToUpper()
                            };
                            await _teachingExperienceRepository.AddAsync(teachingExperience);
                        }
                    }


                    // Save professional Qualifications
                    if (model.ProfessionalQualifications != null && model.ProfessionalQualifications.Any())
                    {
                        foreach (var professionalQualification in model.ProfessionalQualifications)
                        {
                            var professionals = new ProfessionalQualifications
                            {
                                ExaminerRecruitmentId = examinerRecruitmentDetails.Id,  // Make sure the Id is populated before saving
                                ProgrammeName = professionalQualification.ProgrammeName?.ToUpper(),
                                InstitutionName = professionalQualification.InstitutionName.ToUpper(),
                                StartYear = professionalQualification.StartYear,
                                EndYear = professionalQualification.EndYear
                            };
                            await _professionalQualifications.AddAsync(professionals);
                        }
                    }

                    // Handle file uploads
                    string? institutionHeadDocUrl = null;
                    string? academicQualificationsUrl = null;
                    string? nationalIdDocsUrl = null;

                    if (model.AttachHeadComment != null)
                    {
                        institutionHeadDocUrl = await SaveFileAsync(model.AttachHeadComment, model.ExaminerName, model.LastName);
                    }

                    if (model.AcademicQualifications != null)
                    {
                        academicQualificationsUrl = await SaveFileAsync(model.AcademicQualifications, model.ExaminerName,model.LastName);
                    }

                    if (model.NationalIdDocs != null)
                    {
                        nationalIdDocsUrl = await SaveFileAsync(model.NationalIdDocs, model.ExaminerName, model.LastName);
                    }
                    var attachments = new ExaminerRecruitmentAttachements
                    {
                        ExaminerRecruitmentId = examinerRecruitmentDetails.Id,
                        InstitutionHeadDoc = institutionHeadDocUrl,
                        AcademicQualifications = academicQualificationsUrl,
                        NationalIdDocs = nationalIdDocsUrl,
                        Date = DateTime.UtcNow
                    };

                    await _attachmentRepository.AddAsync(attachments);


                    return RedirectToAction("SuccssFullSubmit");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while saving the data: {ex.Message}");
                }
            }

            string uniqueExaminerCode = GenerateUniqueExaminerCode(); // Custom method to generate a unique code

            // Pass the values to the view
            ViewBag.UniqueExaminerCode = uniqueExaminerCode;

            return View(model);
        }


        public IActionResult SuccssFullSubmit()
        {
            return View();
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var recruitment = await _repository.GetByIdAsync(id);
            if (recruitment == null)
                return NotFound();

            return View(recruitment);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ExaminerCode")] ExaminerRecruitment recruitment)
        {
            if (id != recruitment.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(recruitment);
                return RedirectToAction(nameof(Index));
            }
            return View(recruitment);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var recruitment = await _repository.GetByIdAsync(id);
            if (recruitment == null)
                return NotFound();

            return View(recruitment);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjects(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return BadRequest("Prefix is required.");
            }

            IEnumerable<Subjects> subjects;
            if (prefix == "6")
            {
                // Include subjects starting with both '6' and '5'
                subjects = await _subjects.GetSubjectsByPrefixes(new[] { "6", "5" });
            }
            else
            {
                // Filter subjects based on the single prefix
                subjects = await _subjects.GetSubjectsByPrefix(prefix);
            }

            return Ok(subjects); // Return the filtered list of subjects as JSON
        }




        [HttpGet]
        public async Task<IActionResult> GetSubjectByLevelProfessional(string subjectPrefix)
        {
            if (string.IsNullOrEmpty(subjectPrefix))
            {
                return BadRequest("Subject prefix is required.");
            }

            try
            {
                IEnumerable<(string SubSubjectCode, string SubSubjectDesc)> subjects;

                if (subjectPrefix == "6")
                {
                    // Fetch distinct subject details for prefixes "6" and "5"
                    subjects = await _subjects.GetSubjectByLevels(new[] { "6", "5" });
                }
                else
                {
                    // Fetch distinct subject details for the given prefix
                    subjects = await _subjects.GetSubjectByLevel(subjectPrefix);
                }

                // Convert tuples to JSON-compatible format
                var result = subjects.Select(s => new
                {
                    SubSubjectCode = s.SubSubjectCode,
                    SubSubjectDesc = s.SubSubjectDesc
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception
                //_logger.LogError(ex, "An error occurred while fetching subjects.");
                return StatusCode(500, "An error occurred while fetching subjects.");
            }
        }




        [HttpGet]
        public async Task<IActionResult> GetPaperCodesBySubjectCode(string subjectCode)
        {
            if (string.IsNullOrWhiteSpace(subjectCode))
            {
                return BadRequest("Subject code is required.");
            }

            try
            {
                var paperCodes = await _subjects.GetPaperCodesBySubjectCode(subjectCode);

                if (!paperCodes.Any())
                {
                    return NotFound($"No paper codes found for subject code '{subjectCode}'.");
                }

                return Ok(paperCodes);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "An error occurred while fetching paper codes for subject code {SubjectCode}", subjectCode);
                return StatusCode(500, ex.Message);
            }
        }




        private async Task<string> SaveFileAsync(IFormFile file, string firstName, string lastName)
        {
            if (file == null || file.Length == 0)
                return null;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Get the file name without extension
            string originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
            // Get the file extension
            string fileExtension = Path.GetExtension(file.FileName);
            // Append current datetime to the original file name
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Format the filename: FirstName_LastName_OriginalFileName_Timestamp.Extension
            string sanitizedFirstName = firstName?.Trim().Replace(" ", "_") ?? "Unknown";
            string sanitizedLastName = lastName?.Trim().Replace(" ", "_") ?? "Unknown";

            string uniqueFileName = $"{sanitizedFirstName}_{sanitizedLastName}_{originalFileName}_{timestamp}{fileExtension}";

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{uniqueFileName}"; // URL relative to the application root
        }


    }
}
