using AutoMapper;
using ClosedXML.Excel;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Models.ExamMonitors.Dtos;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Repositories.Common;
using ExaminerPaymentSystem.Services.ExamMonitors;
using ExaminerPaymentSystem.ViewModels.Examiners;
using ExaminerPaymentSystem.ViewModels.ExamMonitors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ExaminerPaymentSystem.Controllers.ExamMonitors
{
    public class ExamMonitorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExamMonitorService _service;
        private readonly IMapper _mapper;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBanksRepository _banksRepository;

        public ExamMonitorsController(IExamMonitorService service, IMapper mapper,ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IBanksRepository banksRepository)
        {
            _service = service;
            _mapper = mapper;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _banksRepository = banksRepository;
        }

        [Authorize]
        public async Task<IActionResult> BulkAddExamMonitors()
        {
            return View();
        }


        [Authorize]
        public async Task<IActionResult> DownloadExamMonitorsTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Exam Monitors");

                // Add instructions
                var instructionsSheet = workbook.Worksheets.Add("Instructions");
                instructionsSheet.Cell(1, 1).Value = "EXAM MONITORS TEMPLATE INSTRUCTIONS";
                instructionsSheet.Cell(2, 1).Value = "1. Required fields: NATIONAL_ID, FIRSTNAME, LASTNAME";
                instructionsSheet.Cell(3, 1).Value = "2. NATIONAL_ID must be unique";
                instructionsSheet.Cell(4, 1).Value = "3. GENDER should be 'M' or 'F'";
                instructionsSheet.Cell(5, 1).Value = "4. OUTCOME: 'accepted' goes to ExamMonitors, anything else goes to Recruitment";
                instructionsSheet.Cell(6, 1).Value = "5. If OUTCOME is empty, it defaults to 'rejected'";
                instructionsSheet.Cell(7, 1).Value = "6. CATEGORY field is for general category (Resident Monitors, Cluster Manager,Assistant Cluster Manager, etc.)";
                instructionsSheet.Cell(8, 1).Value = "7. Do not modify the column headers";
                instructionsSheet.Cell(9, 1).Value = "8. Remove this instructions sheet before uploading";

                // Header row
                worksheet.Cell(1, 1).Value = "NATIONAL_ID*";
                worksheet.Cell(1, 2).Value = "LASTNAME*";
                worksheet.Cell(1, 3).Value = "FIRSTNAME*";
                worksheet.Cell(1, 4).Value = "GENDER";
                worksheet.Cell(1, 5).Value = "CATEGORY";
                worksheet.Cell(1, 6).Value = "AGE";
                worksheet.Cell(1, 7).Value = "PHONENUMBER";
                worksheet.Cell(1, 8).Value = "REGION";
                worksheet.Cell(1, 9).Value = "STATION";
                worksheet.Cell(1, 10).Value = "DISTRICT";
                worksheet.Cell(1, 11).Value = "QUALIFICATION";
                worksheet.Cell(1, 12).Value = "EXPERIENCE";
                worksheet.Cell(1, 13).Value = "OUTCOME*";
                worksheet.Cell(1, 14).Value = "COMMENT";
                worksheet.Cell(1, 15).Value = "SCORE";

                worksheet.Cell(2, 5).Value = "Resident Monitor";

                // Add example data for ACCEPTED_STATUS
                worksheet.Cell(2, 13).Value = "accepted"; // Example: goes to ExamMonitors
                worksheet.Cell(3, 13).Value = "rejected"; // Example: goes to Recruitment
                worksheet.Cell(4, 13).Value = "pending";  // Example: goes to Recruitment

                // Style required fields
                for (int i = 1; i <= 3; i++)
                {
                    worksheet.Cell(1, i).Style.Font.Bold = true;
                    worksheet.Cell(1, i).Style.Font.FontColor = XLColor.Red;
                }
                // Style ACCEPTED_STATUS as required
                worksheet.Cell(1, 13).Style.Font.Bold = true;
                worksheet.Cell(1, 13).Style.Font.FontColor = XLColor.Red;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "ExamMonitorTemplate.xlsx");
                }
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadExamMonitors(IFormFile file, string regionCode = "")
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return RedirectToAction("BulkAddMonitors");
            }

            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var acceptedMonitors = new List<ExamMonitor>();
                    var rejectedMonitors = new List<ExamMonitorsRecruitment>();

                    var nationalIdsInFile = new HashSet<string>();
                    var nationalIdsAcceptedInFile = new HashSet<string>();

                    var errors = new List<UploadError>();
                    var duplicatesInFile = new List<UploadError>();
                    var existingInDb = new List<UploadError>();
                    int processedCount = 0;

                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        using (var workbook = new XLWorkbook(stream))
                        {
                            var worksheet = workbook.Worksheet(1);
                            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);
                            int rowNumber = 2;

                            foreach (var row in rows)
                            {
                                var nationalId = row.Cell(1).GetValue<string>()?.Trim();

                              
                                var lastName = row.Cell(2).GetValue<string>()?.Trim();
                                var firstName = row.Cell(3).GetValue<string>()?.Trim();

                                // Skip completely empty rows
                                if (string.IsNullOrWhiteSpace(nationalId) &&
                                    string.IsNullOrWhiteSpace(firstName) &&
                                    string.IsNullOrWhiteSpace(lastName))
                                {
                                    rowNumber++;
                                    continue;
                                }

                                // Validate required fields
                                if (string.IsNullOrWhiteSpace(nationalId))
                                {
                                    errors.Add(new UploadError { RowNumber = rowNumber, NationalId = nationalId, FirstName = firstName, LastName = lastName, ErrorMessage = "National ID is required" });
                                    rowNumber++;
                                    continue;
                                }

                                if (string.IsNullOrWhiteSpace(firstName))
                                {
                                    errors.Add(new UploadError { RowNumber = rowNumber, NationalId = nationalId, FirstName = firstName, LastName = lastName, ErrorMessage = "First Name is required" });
                                    rowNumber++;
                                    continue;
                                }

                                if (string.IsNullOrWhiteSpace(lastName))
                                {
                                    errors.Add(new UploadError { RowNumber = rowNumber, NationalId = nationalId, FirstName = firstName, LastName = lastName, ErrorMessage = "Last Name is required" });
                                    rowNumber++;
                                    continue;
                                }

                                // Check duplicates in file
                                if (nationalIdsInFile.Contains(nationalId))
                                {
                                    duplicatesInFile.Add(new UploadError { RowNumber = rowNumber, NationalId = nationalId, FirstName = firstName, LastName = lastName, ErrorMessage = "Duplicate National ID in upload file" });
                                    rowNumber++;
                                    continue;
                                }

                                nationalId = Regex.Replace(nationalId, "[^a-zA-Z0-9]", "").ToUpper();
                                nationalIdsInFile.Add(nationalId);
                                processedCount++;

                                // ACCEPTED_STATUS
                                var acceptedStatus = row.Cell(13).GetValue<string>()?.Trim();
                                var normalizedStatus = !string.IsNullOrEmpty(acceptedStatus) ? acceptedStatus.ToLower() : "rejected";

                                if (normalizedStatus == "accepted")
                                {
                                    if (nationalIdsAcceptedInFile.Contains(nationalId))
                                    {
                                        duplicatesInFile.Add(new UploadError { RowNumber = rowNumber, NationalId = nationalId, FirstName = firstName, LastName = lastName, ErrorMessage = "Duplicate accepted National ID in file" });
                                        rowNumber++;
                                        continue;
                                    }

                              

                                    nationalIdsAcceptedInFile.Add(nationalId);

                                    acceptedMonitors.Add(new ExamMonitor
                                    {
                                        NationalId = nationalId,
                                        MonitorId = Guid.NewGuid(),
                                        FirstName = firstName,
                                        LastName = lastName,
                                        Sex = row.Cell(4).GetValue<string>()?.Trim(),
                                        Status = row.Cell(5).GetValue<string>()?.Trim(),
                                        Age = row.Cell(6).GetValue<string>()?.Trim(),
                                        Phone = row.Cell(7).GetValue<string>()?.Trim(),
                                        Region = row.Cell(8).GetValue<string>()?.Trim(),
                                        Centre = "XXXXXX",
                                        Station = row.Cell(9).GetValue<string>()?.Trim(),
                                        District = row.Cell(10).GetValue<string>()?.Trim(),
                                        Qualification = row.Cell(11).GetValue<string>()?.Trim(),
                                        Experience = row.Cell(12).GetValue<string>()?.Trim(),
                                        AcceptStatus = normalizedStatus,
                                        Comment = "New Upload",
                                       Score = row.Cell(15).GetValue<string>()?.Trim()
                                    });
                                }
                                else
                                {
                                    rejectedMonitors.Add(new ExamMonitorsRecruitment
                                    {
                                        MonitorId = Guid.NewGuid(),
                                        NationalId = nationalId,
                                        FirstName = firstName,
                                        LastName = lastName,
                                        Sex = row.Cell(4).GetValue<string>()?.Trim(),
                                        Status = row.Cell(5).GetValue<string>()?.Trim(),
                                        Age = row.Cell(6).GetValue<string>()?.Trim(),
                                        Phone = row.Cell(7).GetValue<string>()?.Trim(),
                                        Region = row.Cell(8).GetValue<string>()?.Trim(),
                                        Centre = "XXXXXX",
                                        Station = row.Cell(9).GetValue<string>()?.Trim(),
                                        District = row.Cell(10).GetValue<string>()?.Trim(),
                                        Qualification = row.Cell(11).GetValue<string>()?.Trim(),
                                        Experience = row.Cell(12).GetValue<string>()?.Trim(),
                                        AcceptStatus = normalizedStatus,
                                        Comment = row.Cell(14).GetValue<string>()?.Trim(),
                                        Score = row.Cell(15).GetValue<string>()?.Trim()
                                    });
                                }

                                rowNumber++;
                            }
                        }
                    }

                    // Check against database
                    var allNationalIds = acceptedMonitors.Select(x => x.NationalId)
                        .Concat(rejectedMonitors.Select(x => x.NationalId))
                        .Where(x => x != null)
                        .Distinct()
                        .ToList();

                    var existingInRecruitment = await _context.ExamMonitorsRecruitments
                        .AsNoTracking()
                        .Where(e => e.NationalId != null && allNationalIds.Contains(e.NationalId))
                        .ToListAsync();

                    var existingInMain = await _context.ExamMonitors
                        .AsNoTracking()
                        .Where(e => e.NationalId != null && allNationalIds.Contains(e.NationalId))
                        .ToListAsync();

                    // Filter out duplicates in DB
                    var acceptedMonitorsToAdd = acceptedMonitors
                        .Where(e => !existingInRecruitment.Any(x => x.NationalId == e.NationalId) &&
                                    !existingInMain.Any(x => x.NationalId == e.NationalId))
                        .GroupBy(x => x.NationalId)
                        .Select(g => g.First())
                        .ToList();

                    var rejectedMonitorsToAdd = rejectedMonitors
                        .Where(e => !existingInRecruitment.Any(x => x.NationalId == e.NationalId) &&
                                    !existingInMain.Any(x => x.NationalId == e.NationalId))
                        .GroupBy(x => x.NationalId)
                        .Select(g => g.First())
                        .ToList();

                    // Track existing for error report
                    foreach (var monitor in acceptedMonitors)
                    {
                        if (existingInRecruitment.Any(x => x.NationalId == monitor.NationalId))
                        {
                            existingInDb.Add(new UploadError { NationalId = monitor.NationalId, FirstName = monitor.FirstName, LastName = monitor.LastName, ErrorMessage = "Already exists in Recruitment table" });
                        }
                        else if (existingInMain.Any(x => x.NationalId == monitor.NationalId))
                        {
                            existingInDb.Add(new UploadError { NationalId = monitor.NationalId, FirstName = monitor.FirstName, LastName = monitor.LastName, ErrorMessage = "Already exists in main Exam Monitors table" });
                        }
                    }

                    foreach (var monitor in rejectedMonitors)
                    {
                        if (existingInRecruitment.Any(x => x.NationalId == monitor.NationalId))
                        {
                            existingInDb.Add(new UploadError { NationalId = monitor.NationalId, FirstName = monitor.FirstName, LastName = monitor.LastName, ErrorMessage = "Already exists in Recruitment table" });
                        }
                        else if (existingInMain.Any(x => x.NationalId == monitor.NationalId))
                        {
                            existingInDb.Add(new UploadError { NationalId = monitor.NationalId, FirstName = monitor.FirstName, LastName = monitor.LastName, ErrorMessage = "Already exists in main Exam Monitors table" });
                        }
                    }

                    // Save to database
                    if (acceptedMonitorsToAdd.Any())
                        await _context.ExamMonitors.AddRangeAsync(acceptedMonitorsToAdd);

                    if (rejectedMonitorsToAdd.Any())
                        await _context.ExamMonitorsRecruitments.AddRangeAsync(rejectedMonitorsToAdd);

                    if (acceptedMonitorsToAdd.Any() || rejectedMonitorsToAdd.Any())
                        await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Generate error report if needed
                    if (errors.Any() || duplicatesInFile.Any() || existingInDb.Any())
                    {
                        var errorReport = GenerateErrorReport(errors, duplicatesInFile, existingInDb);
                        HttpContext.Session.SetString("ErrorReport", Convert.ToBase64String(errorReport));
                    }

                    var message = $"{processedCount} exam monitor(s) processed. ";
                    if (acceptedMonitorsToAdd.Any())
                        message += $"{acceptedMonitorsToAdd.Count} added to Exam Monitors (Accepted). ";
                    if (rejectedMonitorsToAdd.Any())
                        message += $"{rejectedMonitorsToAdd.Count} added to Recruitment (Rejected/Other). ";
                    if (duplicatesInFile.Any())
                        message += $"{duplicatesInFile.Count} duplicate(s) skipped in file. ";
                    if (existingInDb.Any())
                        message += $"{existingInDb.Count} existing exam monitor(s) skipped. ";
                    if (errors.Any())
                        message += $"{errors.Count} record(s) with errors.";

                    TempData["Success"] = message;
                    return RedirectToAction("BulkAddExamMonitors");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "An error occurred: " + ex.Message;
                    return RedirectToAction("BulkAddExamMonitors");
                }
            });
        }


       

        // Helper method to generate error report
        private byte[] GenerateErrorReport(List<UploadError> errors, List<UploadError> duplicates, List<UploadError> existing)
        {
            using (var workbook = new XLWorkbook())
            {
                // Errors worksheet
                if (errors.Any())
                {
                    var errorsWorksheet = workbook.Worksheets.Add("Validation Errors");
                    errorsWorksheet.Cell(1, 1).Value = "Row Number";
                    errorsWorksheet.Cell(1, 2).Value = "National ID";
                    errorsWorksheet.Cell(1, 3).Value = "First Name";
                    errorsWorksheet.Cell(1, 4).Value = "Last Name";
                    errorsWorksheet.Cell(1, 5).Value = "Error Message";

                    for (int i = 0; i < errors.Count; i++)
                    {
                        errorsWorksheet.Cell(i + 2, 1).Value = errors[i].RowNumber;
                        errorsWorksheet.Cell(i + 2, 2).Value = errors[i].NationalId;
                        errorsWorksheet.Cell(i + 2, 3).Value = errors[i].FirstName;
                        errorsWorksheet.Cell(i + 2, 4).Value = errors[i].LastName;
                        errorsWorksheet.Cell(i + 2, 5).Value = errors[i].ErrorMessage;
                    }
                }

                // Duplicates worksheet
                if (duplicates.Any())
                {
                    var duplicatesWorksheet = workbook.Worksheets.Add("File Duplicates");
                    duplicatesWorksheet.Cell(1, 1).Value = "Row Number";
                    duplicatesWorksheet.Cell(1, 2).Value = "National ID";
                    duplicatesWorksheet.Cell(1, 3).Value = "First Name";
                    duplicatesWorksheet.Cell(1, 4).Value = "Last Name";
                    duplicatesWorksheet.Cell(1, 5).Value = "Error Message";

                    for (int i = 0; i < duplicates.Count; i++)
                    {
                        duplicatesWorksheet.Cell(i + 2, 1).Value = duplicates[i].RowNumber;
                        duplicatesWorksheet.Cell(i + 2, 2).Value = duplicates[i].NationalId;
                        duplicatesWorksheet.Cell(i + 2, 3).Value = duplicates[i].FirstName;
                        duplicatesWorksheet.Cell(i + 2, 4).Value = duplicates[i].LastName;
                        duplicatesWorksheet.Cell(i + 2, 5).Value = duplicates[i].ErrorMessage;
                    }
                }

                // Existing records worksheet
                if (existing.Any())
                {
                    var existingWorksheet = workbook.Worksheets.Add("Existing Records");
                    existingWorksheet.Cell(1, 1).Value = "National ID";
                    existingWorksheet.Cell(1, 2).Value = "First Name";
                    existingWorksheet.Cell(1, 3).Value = "Last Name";
                    existingWorksheet.Cell(1, 4).Value = "Error Message";

                    for (int i = 0; i < existing.Count; i++)
                    {
                        existingWorksheet.Cell(i + 2, 1).Value = existing[i].NationalId;
                        existingWorksheet.Cell(i + 2, 2).Value = existing[i].FirstName;
                        existingWorksheet.Cell(i + 2, 3).Value = existing[i].LastName;
                        existingWorksheet.Cell(i + 2, 4).Value = existing[i].ErrorMessage;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        // Action to download error report
        public IActionResult DownloadErrorReport()
        {
            var errorReportBase64 = HttpContext.Session.GetString("ErrorReport");
            if (!string.IsNullOrEmpty(errorReportBase64))
            {
                var errorReport = Convert.FromBase64String(errorReportBase64);

                // Clear the session after downloading
                HttpContext.Session.Remove("ErrorReport");

                return File(errorReport,
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                           "UploadErrors.xlsx");
            }

            TempData["Error"] = "No error report available.";
            return RedirectToAction("BulkAddExamMonitors");
        }

        // In your ExamMonitorsController.cs
        [Authorize]
        public async Task<IActionResult> AcceptApplication(string nationalId)
        {
            try
            {

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                // Find the recruitment record
                var recruitmentRecord = await _context.ExamMonitorsRecruitments
                    .FirstOrDefaultAsync(r => r.NationalId == nationalId);

                var existingMonitor = await _context.ExamMonitors.FirstOrDefaultAsync(r => r.NationalId == nationalId);

                if (recruitmentRecord == null && existingMonitor == null)
                {
                    return Json(new { success = false, message = "Application not found." });
                }

              if(recruitmentRecord != null)
                {
                    // Create a new ExamMonitor from the recruitment record
                    var examMonitor = new ExamMonitor
                    {
                        NationalId = recruitmentRecord.NationalId,
                        MonitorId = recruitmentRecord.MonitorId,
                        FirstName = recruitmentRecord.FirstName,
                        LastName = recruitmentRecord.LastName,
                        Sex = recruitmentRecord.Sex,
                        Status = recruitmentRecord.Status,
                        AcceptStatus = "accepted",
                        AcceptBy = currentUser.UserName,
                        AcceptDate = DateTime.Now,
                        Qualification = recruitmentRecord.Qualification,
                        Experience = recruitmentRecord.Experience,
                        Region = recruitmentRecord.Region,
                        Phone = recruitmentRecord.Phone,
                        Age = recruitmentRecord.Age,
                        Centre = recruitmentRecord.Centre,
                        Station = recruitmentRecord.Station, 
                        Email = recruitmentRecord.Email,
                        ProfessionalQualifications = recruitmentRecord.ProfessionalQualifications,
                        Attachments = recruitmentRecord.Attachments,
                        EmailInvitations = recruitmentRecord.EmailInvitations
                    };

                    // Add to the main table
                    _context.ExamMonitors.Add(examMonitor);

                    // Remove from recruitment table
                    _context.ExamMonitorsRecruitments.Remove(recruitmentRecord);
                }
                else
                {
                    existingMonitor.AcceptStatus = "accepted";
                    existingMonitor.AcceptBy = currentUser.UserName;
                    existingMonitor.AcceptDate = DateTime.Now;
                }
                    await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Application accepted successfully." });


            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while rejecting the application." });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RejectApplication(string nationalId, string rejectionReason, bool notifyApplicant = false)
        {
            try
            {
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                // Find the recruitment record
                var recruitmentRecord = await _context.ExamMonitorsRecruitments
                    .FirstOrDefaultAsync(r => r.NationalId == nationalId);
                var existingMonitor = await _context.ExamMonitors.FirstOrDefaultAsync(r => r.NationalId == nationalId);

                if (recruitmentRecord == null && existingMonitor == null)
                {
                    return Json(new { success = false, message = "Application not found." });
                }

                if(recruitmentRecord != null)
                {
                    // Update status fields instead of deleting
                    recruitmentRecord.AcceptStatus = "rejected";
                    recruitmentRecord.AcceptBy = currentUser.UserName;
                    recruitmentRecord.AcceptDate = DateTime.Now;
                    recruitmentRecord.Comment = rejectionReason;

                    // Mark as modified and save
                    _context.ExamMonitorsRecruitments.Update(recruitmentRecord);
                }
             if(existingMonitor != null)
                {
                    var examMonitor = new ExamMonitorsRecruitment
                    {
                        NationalId = existingMonitor.NationalId,
                        MonitorId = existingMonitor.MonitorId,
                        FirstName = existingMonitor.FirstName,
                        LastName = existingMonitor.LastName,
                        Sex = existingMonitor.Sex,
                        Status = existingMonitor.Status,
                        AcceptStatus = "rejected",
                        AcceptBy = currentUser.UserName,
                        AcceptDate = DateTime.Now,
                        Qualification = existingMonitor.Qualification,
                        Experience = existingMonitor.Experience,
                        Region = existingMonitor.Region,
                        Phone = existingMonitor.Phone,
                        Age = existingMonitor.Age,
                        Centre = existingMonitor.Centre,
                        Station = existingMonitor.Station,
                        District = existingMonitor.District,
                        Email = existingMonitor.Email,
                        ProfessionalQualifications = existingMonitor.ProfessionalQualifications,
                        Attachments = existingMonitor.Attachments,
                        EmailInvitations = existingMonitor.EmailInvitations
                    };

                    // Add to the main table
                    _context.ExamMonitorsRecruitments.Add(examMonitor);

                    // Remove from recruitment table
                    _context.ExamMonitors.Remove(existingMonitor);

                }

                    await _context.SaveChangesAsync();

                //// If notification is requested, send email
                //if (notifyApplicant && !string.IsNullOrEmpty(recruitmentRecord.Email))
                //{
                //    await SendRejectionEmail(recruitmentRecord.Email, recruitmentRecord.FirstName, rejectionReason);
                //}

                return Json(new { success = true, message = "Application rejected successfully." });
            }
            catch (Exception ex)
            {
                // Log error
                //_logger.LogError(ex, "Error rejecting application for {NationalId}", nationalId);
                return Json(new { success = false, message = "An error occurred while rejecting the application." });
            }
        }

        //private async Task SendRejectionEmail(string email, string firstName, string rejectionReason)
        //{
        //    try
        //    {
        //        var subject = "Your Exam Monitor Application Status";
        //        var body = $@"
        //    <p>Dear {firstName},</p>
        //    <p>Thank you for applying to become an Exam Monitor.</p>
        //    <p>After careful consideration, we regret to inform you that your application has not been successful at this time.</p>
        //    <p><strong>Reason for rejection:</strong> {rejectionReason}</p>
        //    <p>We encourage you to gain more experience and apply again in the future.</p>
        //    <p>Best regards,<br>Exam Board Team</p>";

        //        // Use your email service here
        //        // await _emailService.SendEmailAsync(email, subject, body);

        //        _logger.LogInformation("Rejection email sent to {Email}", email);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error sending rejection email to {Email}", email);
        //    }
        //}

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

            var examMonitor = await _context.ExamMonitors
                 .Include(e => e.ProfessionalQualifications)
            
                    .FirstOrDefaultAsync(m => m.NationalId == currentUser.IDNumber);

            var attachments = await _context.ExamMonitorAttachements.Where(a => a.NationalId == currentUser.IDNumber).ToListAsync();

            var qualifications = await _context.ExamMonitorProfessionalQualifications.Where(a => a.NationalId == currentUser.IDNumber).ToListAsync();

            if (examMonitor == null)
                {
                    return BadRequest("Monitor not found" );
                }

            var model = new ExamMonitorViewModel
            {

                NationalId = examMonitor.NationalId,
                FirstName = examMonitor.FirstName,
                LastName = examMonitor.LastName,
                Sex = examMonitor.Sex,
                Status = examMonitor.Status,
                Qualification = examMonitor.Qualification,
                Experience = examMonitor.Experience,
                Region = examMonitor.Region,
                Phone = examMonitor.Phone,
                Age = examMonitor.Age,
                Centre = examMonitor.Centre,
                Station = examMonitor.Station,
                District = examMonitor.District,
                Email = examMonitor.Email,
                BankBranchCodeUsd = examMonitor.BankBranchCodeUsd,
                BankCodeUsd = examMonitor.BankCodeUsd,
                BankNameUsd = examMonitor.BankNameUsd,
                BranchUsd = examMonitor.BranchUsd,
                AccountNumberUsd = examMonitor.AccountNumberUsd,
                BankCodeZwg = examMonitor.BankCodeZwg,
                BankNameZwg = examMonitor.BankNameZwg,
                BranchZwg = examMonitor.BranchZwg,
                AccountNumberZwg = examMonitor.AccountNumberZwg,
                BankBranchCodeZwg = examMonitor.BankBranchCodeZwg,
                ProfessionalQualifications = qualifications?.ToList() ?? new List<ExamMonitorProfessionalQualifications>(),
                AcademicQualifications = attachments?.FirstOrDefault()?.AcademicQualifications,
                NationalIdDocs = attachments?.FirstOrDefault()?.NationalIdDocs,

            };

            ViewBag.Districts = _context.Districts
                 .Where(m => m.DistrictCode != null && m.RegionCode == examMonitor.Region)
                 .Select(m => new {
                     Value = m.DistrictCode,
                     Text = m.DistrictName
                 })
                 .Distinct()
                 .OrderBy(d => d.Text)
                 .ToList();
            return View(model);
            }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(ExamMonitorViewModel viewModel) 
        {
            try
            {
      
                var examMonitor = await _context.ExamMonitors
                 
                    .FirstOrDefaultAsync(e => e.NationalId == viewModel.NationalId);

               

                if (examMonitor == null)
                {
                    TempData["ErrorMessage"] = "Monitor not found.";
                    return RedirectToAction("Edit");
                }

                // Fetch bank details for ZWL and FCA
                var bankZWL = await GetBankDataAsync(viewModel.BankCodeZwg, viewModel.BankBranchCodeZwg);
                var bankFCA = await GetBankDataAsync(viewModel.BankCodeUsd, viewModel.BankBranchCodeUsd);

                // Update bank details if found
                if (bankZWL != null)
                {
                    examMonitor.BankCodeZwg = bankZWL.B_BANK_CODE;
                    examMonitor.BankNameZwg = bankZWL.B_BANK_NAME;
                    examMonitor.BankBranchCodeZwg = bankZWL.BB_BRANCH_CODE;
                    examMonitor.BranchZwg = bankZWL.BB_BRANCH_NAME;
                }

                if (bankFCA != null)
                {
                    examMonitor.BankNameUsd = bankFCA.B_BANK_NAME;
                    examMonitor.BankCodeUsd = bankFCA.B_BANK_CODE;
                    examMonitor.BankBranchCodeUsd = bankFCA.BB_BRANCH_CODE;
                    examMonitor.BranchUsd = bankFCA.BB_BRANCH_NAME;
                }


                // Update basic properties
                examMonitor.FirstName = viewModel.FirstName;
                examMonitor.LastName = viewModel.LastName;
                examMonitor.Sex = viewModel.Sex;
                examMonitor.Status = viewModel.Status;
                examMonitor.Email = viewModel.Email;
                examMonitor.Qualification = viewModel.Qualification;
                examMonitor.Experience = viewModel.Experience;
                examMonitor.Region = viewModel.Region;
                examMonitor.Phone = viewModel.Phone;
                examMonitor.Age = viewModel.Age;
                examMonitor.Centre = viewModel.Centre;
                examMonitor.Station = viewModel.Station;
                examMonitor.District = viewModel.District;
        
                examMonitor.AccountNumberZwg = viewModel.AccountNumberZwg;
          
                examMonitor.AccountNumberUsd = viewModel.AccountNumberUsd;

                // Handle file uploads - use the ViewModel properties
                await HandleFileUploads(examMonitor, viewModel.AcademicQualificationsFile, viewModel.NationalIdDocsFile);

                // Handle professional qualifications
                await HandleProfessionalQualifications(examMonitor, viewModel.ProfessionalQualifications);

                // Update in database
                _context.ExamMonitors.Update(examMonitor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Exam monitor details updated successfully!";
                return RedirectToAction("Edit");
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "An error occurred while updating the exam monitor.");

                return View(viewModel);
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditHR(string nationalId)
        {
            

            var examMonitor = await _context.ExamMonitors
                    .FirstOrDefaultAsync(m => m.NationalId == nationalId);

            var examRecruitment = await _context.ExamMonitorsRecruitments
                    .FirstOrDefaultAsync(m => m.NationalId == nationalId);

            var attachments = await _context.ExamMonitorAttachements.Where(a => a.NationalId == nationalId).ToListAsync();

            var qualifications = await _context.ExamMonitorProfessionalQualifications.Where(a => a.NationalId == nationalId).ToListAsync();

            if (examMonitor == null && examRecruitment == null)
            {
                TempData["ErrorMessage"] = "Monitor not found.";
                return RedirectToAction(nameof(DetailsRecruitment), new { id = nationalId });
            }

            ExamMonitorViewModel model = new ExamMonitorViewModel();

            if (examMonitor != null)
            {
                model = new ExamMonitorViewModel
                {
                    NationalId = examMonitor.NationalId,
                    FirstName = examMonitor.FirstName,
                    LastName = examMonitor.LastName,
                    Sex = examMonitor.Sex,
                    Status = examMonitor.Status,
                    Qualification = examMonitor.Qualification,
                    Experience = examMonitor.Experience,
                    Region = examMonitor.Region,
                    Phone = examMonitor.Phone,
                    Age = examMonitor.Age,
                    Centre = examMonitor.Centre,
                    Station = examMonitor.Station,
                    District = examMonitor.District,
                    Email = examMonitor.Email,
                    BankBranchCodeUsd = examMonitor.BankBranchCodeUsd,
                    BankCodeUsd = examMonitor.BankCodeUsd,
                    BankNameUsd = examMonitor.BankNameUsd,
                    BranchUsd = examMonitor.BranchUsd,
                    AccountNumberUsd = examMonitor.AccountNumberUsd,
                    BankCodeZwg = examMonitor.BankCodeZwg,
                    BankNameZwg = examMonitor.BankNameZwg,
                    BranchZwg = examMonitor.BranchZwg,
                    AccountNumberZwg = examMonitor.AccountNumberZwg,
                    BankBranchCodeZwg = examMonitor.BankBranchCodeZwg,
                    ProfessionalQualifications = qualifications?.ToList()
                                                 ?? new List<ExamMonitorProfessionalQualifications>(),
                    AcademicQualifications = attachments?.FirstOrDefault()?.AcademicQualifications,
                    NationalIdDocs = attachments?.FirstOrDefault()?.NationalIdDocs,
                };
            }
            else // use examRecruitment
            {
                model = new ExamMonitorViewModel
                {
                    NationalId = examRecruitment.NationalId,
                    FirstName = examRecruitment.FirstName,
                    LastName = examRecruitment.LastName,
                    Sex = examRecruitment.Sex,
                    Status = examRecruitment.Status,
                    Qualification = examRecruitment.Qualification,
                    Experience = examRecruitment.Experience,
                    Region = examRecruitment.Region,
                    Phone = examRecruitment.Phone,
                    Age = examRecruitment.Age,
                    Centre = examRecruitment.Centre,
                    Station = examRecruitment.Station,
                    District = examRecruitment.District,
                    Email = examRecruitment.Email,
                    BankBranchCodeUsd = examRecruitment.BankBranchCodeUsd,
                    BankCodeUsd = examRecruitment.BankCodeUsd,
                    BankNameUsd = examRecruitment.BankNameUsd,
                    BranchUsd = examRecruitment.BranchUsd,
                    AccountNumberUsd = examRecruitment.AccountNumberUsd,
                    BankCodeZwg = examRecruitment.BankCodeZwg,
                    BankNameZwg = examRecruitment.BankNameZwg,
                    BranchZwg = examRecruitment.BranchZwg,
                    AccountNumberZwg = examRecruitment.AccountNumberZwg,
                    BankBranchCodeZwg = examRecruitment.BankBranchCodeZwg,
                    ProfessionalQualifications = qualifications?.ToList()
                                                 ?? new List<ExamMonitorProfessionalQualifications>(),
                    AcademicQualifications = attachments?.FirstOrDefault()?.AcademicQualifications,
                    NationalIdDocs = attachments?.FirstOrDefault()?.NationalIdDocs,
                };
            }

            ViewBag.Districts = _context.Districts
                .Where(m => m.DistrictCode != null && m.RegionCode == model.Region)
                .Select(m => new { Value = m.DistrictCode, Text = m.DistrictName })
                .Distinct()
                .OrderBy(d => d.Text)
                .ToList();

            return View(model);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditHR(ExamMonitorViewModel viewModel)
        {
            try
            {
                // Fetch from both tables
                var examMonitor = await _context.ExamMonitors
                    .FirstOrDefaultAsync(e => e.NationalId == viewModel.NationalId);

                var examRecruitment = await _context.ExamMonitorsRecruitments
                    .FirstOrDefaultAsync(m => m.NationalId == viewModel.NationalId);

                if (examMonitor == null && examRecruitment == null)
                {
                    TempData["ErrorMessage"] = "Monitor not found.";
                    return RedirectToAction(nameof(DetailsRecruitment));
                }

                if (examMonitor != null)
                {
                    // Update bank info
                    await UpdateBankDataAsync(viewModel, examMonitor);

                    // Update other properties
                    MapViewModelToExamMonitor(viewModel, examMonitor);

                    // Handle files
                    if (viewModel.AcademicQualificationsFile != null && viewModel.AcademicQualificationsFile.Length > 0 && viewModel.NationalIdDocsFile != null && viewModel.NationalIdDocsFile.Length > 0)
                    {
                        await HandleFileUploads(examMonitor, viewModel.AcademicQualificationsFile, viewModel.NationalIdDocsFile);
                    }
                    // Handle professional qualifications
                    await HandleProfessionalQualifications(examMonitor, viewModel.ProfessionalQualifications);

                    _context.ExamMonitors.Update(examMonitor);
                }
                else // examRecruitment != null
                {
                    await UpdateBankDataAsync(viewModel, examRecruitment);

                    MapViewModelToExamRecruitment(viewModel, examRecruitment);

                    var dtoMonitor = new ExamMonitor()
                    {
                        NationalId = examRecruitment.NationalId
                    };

                    if (viewModel.AcademicQualificationsFile != null && viewModel.AcademicQualificationsFile.Length > 0 && viewModel.NationalIdDocsFile != null && viewModel.NationalIdDocsFile.Length > 0)
                    {
                        await HandleFileUploads(dtoMonitor, viewModel.AcademicQualificationsFile, viewModel.NationalIdDocsFile);
                    }

                    await HandleProfessionalQualifications(dtoMonitor, viewModel.ProfessionalQualifications);

                    _context.ExamMonitorsRecruitments.Update(examRecruitment);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Details updated successfully!";
                return RedirectToAction("DetailsRecruitment", new { id = viewModel.NationalId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the exam monitor: " + ex.Message);
                return View(viewModel);
            }
        }

        private void MapViewModelToExamMonitor(ExamMonitorViewModel vm, ExamMonitor monitor)
        {
            monitor.FirstName = vm.FirstName;
            monitor.LastName = vm.LastName;
            monitor.Sex = vm.Sex;
            monitor.Status = vm.Status;
            monitor.Email = vm.Email;
            monitor.Qualification = vm.Qualification;
            monitor.Experience = vm.Experience;
            monitor.Region = vm.Region;
            monitor.Phone = vm.Phone;
            monitor.Age = vm.Age;
            monitor.Centre = vm.Centre;
            monitor.Station = vm.Station;
            monitor.District = vm.District;
            monitor.AccountNumberZwg = vm.AccountNumberZwg;
            monitor.AccountNumberUsd = vm.AccountNumberUsd;
        }

        private void MapViewModelToExamRecruitment(ExamMonitorViewModel vm, ExamMonitorsRecruitment recruit)
        {
            recruit.FirstName = vm.FirstName;
            recruit.LastName = vm.LastName;
            recruit.Sex = vm.Sex;
            recruit.Status = vm.Status;
            recruit.Email = vm.Email;
            recruit.Qualification = vm.Qualification;
            recruit.Experience = vm.Experience;
            recruit.Region = vm.Region;
            recruit.Phone = vm.Phone;
            recruit.Age = vm.Age;
            recruit.Centre = vm.Centre;
            recruit.Station = vm.Station;
            recruit.District = vm.District;
            recruit.AccountNumberZwg = vm.AccountNumberZwg;
            recruit.AccountNumberUsd = vm.AccountNumberUsd;
        }

        private async Task UpdateBankDataAsync(ExamMonitorViewModel vm, ExamMonitor monitor)
        {
            var bankZWL = await GetBankDataAsync(vm.BankCodeZwg, vm.BankBranchCodeZwg);
            var bankFCA = await GetBankDataAsync(vm.BankCodeUsd, vm.BankBranchCodeUsd);

            if (bankZWL != null)
            {
                monitor.BankCodeZwg = bankZWL.B_BANK_CODE;
                monitor.BankNameZwg = bankZWL.B_BANK_NAME;
                monitor.BankBranchCodeZwg = bankZWL.BB_BRANCH_CODE;
                monitor.BranchZwg = bankZWL.BB_BRANCH_NAME;
            }

            if (bankFCA != null)
            {
                monitor.BankNameUsd = bankFCA.B_BANK_NAME;
                monitor.BankCodeUsd = bankFCA.B_BANK_CODE;
                monitor.BankBranchCodeUsd = bankFCA.BB_BRANCH_CODE;
                monitor.BranchUsd = bankFCA.BB_BRANCH_NAME;
            }
        }

        private async Task UpdateBankDataAsync(ExamMonitorViewModel vm, ExamMonitorsRecruitment recruit)
        {
            var bankZWL = await GetBankDataAsync(vm.BankCodeZwg, vm.BankBranchCodeZwg);
            var bankFCA = await GetBankDataAsync(vm.BankCodeUsd, vm.BankBranchCodeUsd);

            if (bankZWL != null)
            {
                recruit.BankCodeZwg = bankZWL.B_BANK_CODE;
                recruit.BankNameZwg = bankZWL.B_BANK_NAME;
                recruit.BankBranchCodeZwg = bankZWL.BB_BRANCH_CODE;
                recruit.BranchZwg = bankZWL.BB_BRANCH_NAME;
            }

            if (bankFCA != null)
            {
                recruit.BankNameUsd = bankFCA.B_BANK_NAME;
                recruit.BankCodeUsd = bankFCA.B_BANK_CODE;
                recruit.BankBranchCodeUsd = bankFCA.BB_BRANCH_CODE;
                recruit.BranchUsd = bankFCA.BB_BRANCH_NAME;
            }
        }


        private async Task<BankData> GetBankDataAsync(string bankCode, string branchCode)
        {
            return await _banksRepository.GetBankDataByParameter(bankCode, branchCode);
        }

        private async Task HandleFileUploads(ExamMonitor examMonitor, IFormFile academicQualificationsFile, IFormFile nationalIdDocsFile)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "exam-monitors", examMonitor.NationalId);

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Get or create a single attachment record for this monitor
            var attachment = await _context.ExamMonitorAttachements
                .FirstOrDefaultAsync(a => a.NationalId == examMonitor.NationalId);

            // Track if we need to save changes
            bool needsSave = false;
            bool isNewAttachment = false;

            if (attachment == null)
            {
                attachment = new ExamMonitorAttachements
                {
                    NationalId = examMonitor.NationalId,
                    Date = DateTime.Now
                };
                isNewAttachment = true;
            }
            else
            {
                attachment.Date = DateTime.Now; // Update the date
                needsSave = true; // Existing entity is being modified
            }

            // Handle academic qualifications file
            if (academicQualificationsFile != null && academicQualificationsFile.Length > 0)
            {
                var academicFileName = await SaveFile(academicQualificationsFile, uploadsPath, "academic_qualifications");
                if (attachment.AcademicQualifications != academicFileName)
                {
                    attachment.AcademicQualifications = academicFileName;
                    needsSave = true;
                }
            }

            // Handle national ID file
            if (nationalIdDocsFile != null && nationalIdDocsFile.Length > 0)
            {
                var nationalIdFileName = await SaveFile(nationalIdDocsFile, uploadsPath, "national_id");
                if (attachment.NationalIdDocs != nationalIdFileName)
                {
                    attachment.NationalIdDocs = nationalIdFileName;
                    needsSave = true;
                }
            }

            // Only add to context and save if we have changes
            if (isNewAttachment && (academicQualificationsFile != null || nationalIdDocsFile != null))
            {
                _context.ExamMonitorAttachements.Add(attachment);
                needsSave = true;
            }

            // Save changes immediately for attachments
            if (needsSave)
            {
                await _context.SaveChangesAsync();
            }
        }

        [Authorize]
        private async Task<string> SaveFile(IFormFile file, string uploadsPath, string fileType)
        {
            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{fileType}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new Exception($"File size cannot exceed 5MB. {fileType} file is too large.");
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(fileExtension.ToLower()))
            {
                throw new Exception($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
            }

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadFile(string nationalId, string fileType)
        {
            try
            {
                var attachment = await _context.ExamMonitorAttachements
                    .FirstOrDefaultAsync(a => a.NationalId == nationalId &&
                        (fileType == "academic" ? a.AcademicQualifications != null : a.NationalIdDocs != null));

                if (attachment == null)
                {
                    return NotFound();
                }

                var fileName = fileType == "academic" ? attachment.AcademicQualifications : attachment.NationalIdDocs;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "exam-monitors", nationalId, fileName);
              
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, GetContentType(filePath), fileName);
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, "An error occurred while downloading the file.");
            }
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
    {
        {".pdf", "application/pdf"},
        {".doc", "application/msword"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".jpg", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".png", "image/png"}
    };
        }

        private async Task HandleProfessionalQualifications(ExamMonitor examMonitor, List<ExamMonitorProfessionalQualifications> qualifications)
        {
            // Remove existing qualifications
            var existingQualifications = _context.ExamMonitorProfessionalQualifications
                .Where(q => q.NationalId == examMonitor.NationalId);
            _context.ExamMonitorProfessionalQualifications.RemoveRange(existingQualifications);

            // Add new qualifications
            if (qualifications != null && qualifications.Any())
            {
                foreach (var qualification in qualifications)
                {
                    if (!string.IsNullOrEmpty(qualification.ProgrammeName))
                    {
                        qualification.NationalId = examMonitor.NationalId;
                        qualification.Date = DateTime.Now;
                        _context.ExamMonitorProfessionalQualifications.Add(qualification);
                    }
                }
            }
        }



        // In your ExamMonitorsController.cs
        [HttpGet]
        [Authorize]
        public JsonResult GetMonitor(string nationalId)
        {
            try
            {
                var monitor = _context.ExamMonitors
                    .FirstOrDefault(m => m.NationalId == nationalId);

                if (monitor == null)
                {
                    return Json(new { success = false, message = "Monitor not found" });
                }

                return Json(new
                {
                    success = true,
                    NationalId = monitor.NationalId,
                    FirstName = monitor.FirstName,
                    LastName = monitor.LastName,
                    Sex = monitor.Sex,
                    Status = monitor.Status,
                    Region = monitor.Region,
                    Phone = monitor.Phone,
                    Centre = monitor.Centre,
                    Station = monitor.Station
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ExamMonitors
        [Authorize]
        public async Task<IActionResult> Index()
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            if (userRoles.Contains("RegionalManager"))
            {
                ViewBag.Centres = _context.Centres.Where(a => a.RegionCode == currentUser.Region)
       .Select(m => new {
           Value = m.CentreNumber,
           Text = m.CentreName
       })
       .Distinct()
       .OrderBy(c => c.Text)
       .ToList();

                ViewBag.Clusters = _context.Centres
                    .Where(a => a.RegionCode == currentUser.Region && a.IsCluster == "IsCluster")
           .Select(m => new {
               Value = m.ClusterCode,
               Text = m.ClusterName
           })
           .Distinct()
           .OrderBy(c => c.Text)
           .ToList();

                ViewBag.Districts = _context.Districts
                    .Where(m => m.DistrictCode != null && m.RegionCode == currentUser.Region)
                    .Select(m => new {
                        Value = m.DistrictName,
                        Text = m.DistrictName
                    })
                    .Distinct()
                    .OrderBy(d => d.Text)
                    .ToList();

            }
            else if (userRoles.Contains("Admin") || userRoles.Contains("HR"))
            {
                ViewBag.Centres = _context.Centres
       .Select(m => new {
           Value = m.CentreNumber,
           Text = m.CentreName
       })
       .Distinct()
       .OrderBy(c => c.Text)
       .ToList();

                ViewBag.Clusters = _context.Centres
                    .Where(a => a.IsCluster == "IsCluster")
           .Select(m => new {
               Value = m.ClusterCode,
               Text = m.ClusterName
           })
           .Distinct()
           .OrderBy(c => c.Text)
           .ToList();

                ViewBag.Districts = _context.Districts
                    .Where(m => m.DistrictCode != null)
                    .Select(m => new {
                        Value = m.DistrictName,
                        Text = m.DistrictName
                    })
                    .Distinct()
                    .OrderBy(d => d.Text)
                    .ToList();

            }

            ViewBag.Phases = _context.Phases
      .Select(p => new {
          Value = p.PhaseCode,
          Text = p.PhaseName
      })
      .OrderBy(p => p.Text)
      .ToList();

            ViewBag.Sessions = _context.ExamSessions
                                .Select(s => new {
                                    Value = s.SessionCode,
                                    Text = s.SessionName
                                })
                .OrderBy(s => s.Text)
                .ToList();

            ViewBag.RegionCode = currentUser.Region;


            return View();
         
        }

        [Authorize]
        public async Task<IActionResult> Recruitment()
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            if (userRoles.Contains("RegionalManager"))
            {
                ViewBag.Centres = _context.Centres.Where(a => a.RegionCode == currentUser.Region)
       .Select(m => new {
           Value = m.CentreNumber,
           Text = m.CentreName
       })
       .Distinct()
       .OrderBy(c => c.Text)
       .ToList();

                ViewBag.Clusters = _context.Centres
                    .Where(a => a.RegionCode == currentUser.Region && a.IsCluster == "IsCluster")
           .Select(m => new {
               Value = m.ClusterCode,
               Text = m.ClusterName
           })
           .Distinct()
           .OrderBy(c => c.Text)
           .ToList();

                ViewBag.Districts = _context.Districts
                    .Where(m => m.DistrictCode != null && m.RegionCode == currentUser.Region)
                    .Select(m => new {
                        Value = m.DistrictName,
                        Text = m.DistrictName
                    })
                    .Distinct()
                    .OrderBy(d => d.Text)
                    .ToList();

            }
            else if (userRoles.Contains("Admin") || userRoles.Contains("HR"))
            {
                ViewBag.Centres = _context.Centres
       .Select(m => new {
           Value = m.CentreNumber,
           Text = m.CentreName
       })
       .Distinct()
       .OrderBy(c => c.Text)
       .ToList();

                ViewBag.Clusters = _context.Centres
                    .Where(a => a.IsCluster == "IsCluster")
           .Select(m => new {
               Value = m.ClusterCode,
               Text = m.ClusterName
           })
           .Distinct()
           .OrderBy(c => c.Text)
           .ToList();

                ViewBag.Districts = _context.Districts
                    .Where(m => m.DistrictCode != null)
                    .Select(m => new {
                        Value = m.DistrictName,
                        Text = m.DistrictName
                    })
                    .Distinct()
                    .OrderBy(d => d.Text)
                    .ToList();

            }

            ViewBag.RegionCode = currentUser.Region;


            return View();

        }

        // POST: ExamMonitors/GetExamMonitors
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetExamMonitors([FromBody] DtParameters dtParameters)
            {
                try
                {

                // Initialize with default values if null
                dtParameters ??= new DtParameters();

                var searchValue = dtParameters.Search?.Value ?? string.Empty;
                
                var statusFilter = dtParameters.StatusFilter ?? string.Empty;
                var genderFilter = dtParameters.GenderFilter ?? string.Empty;
                var centreFilter = dtParameters.CentreFilter ?? string.Empty;
                var districtFilter = dtParameters.DistrictFilter ?? string.Empty;

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                
                var userRoles = await _userManager.GetRolesAsync(currentUser);

                // Base query
                IQueryable<ExamMonitor> query = _context.ExamMonitors.Include(a => a.ExamMonitorTransactions).AsQueryable();

                if (userRoles.Contains("RegionalManager"))
                {
                    // For Regional Managers - enforce region filter from their profile
                    if (string.IsNullOrEmpty(currentUser.Region))
                    {
                        // Return empty result if region is missing for Regional Manager
                        return Json(new { data = new List<ExamMonitor>() });
                    }
                    query = query.Where(x => x.Region == currentUser.Region);
                }
                else if (userRoles.Contains("Admin") || userRoles.Contains("HR"))
                {
                    var regionFilter = dtParameters.RegionFilter ?? string.Empty;
                    // For Admins - apply region filter only if specified
                    if (!string.IsNullOrEmpty(regionFilter))
                    {
                        query = query.Where(x => x.Region == regionFilter);
                    }
                 
                }

       

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        query = query.Where(m => m.Status == statusFilter);
                    }

                    if (!string.IsNullOrEmpty(genderFilter))
                    {
                        query = query.Where(m => m.Sex == genderFilter);
                    }

                    if (!string.IsNullOrEmpty(centreFilter))
                    {
                        query = query.Where(m => m.Centre == centreFilter);
                }

                    if (!string.IsNullOrEmpty(districtFilter))
                {
                    query = query.Where(m => m.District == districtFilter);
                }

                // Apply global search
                if (!string.IsNullOrEmpty(searchValue))
                    {
                        query = query.Where(m =>
                            m.FirstName.Contains(searchValue) ||
                            m.LastName.Contains(searchValue) ||
                            m.NationalId.Contains(searchValue) ||
                            m.Phone.Contains(searchValue) ||
                            m.Centre.Contains(searchValue));
                    }

                    // Get total count before paging
                    var totalRecords = await query.CountAsync();

           
                    if (dtParameters.Order != null && dtParameters.Order.Any())
                {
                    var order = dtParameters.Order.First();
                    var columnIndex = order.Column;
                    var columnName = dtParameters.Columns[columnIndex].Data;
                    var isAscending = order.Dir == DtOrderDir.Asc;

                    query = columnName switch
                    {
                        "firstName" => isAscending ? query.OrderBy(m => m.FirstName) : query.OrderByDescending(m => m.FirstName),
                        "lastName" => isAscending ? query.OrderBy(m => m.LastName) : query.OrderByDescending(m => m.LastName),
                        "nationalId" => isAscending ? query.OrderBy(m => m.NationalId) : query.OrderByDescending(m => m.NationalId),
                        "status" => isAscending ? query.OrderBy(m => m.Status) : query.OrderByDescending(m => m.Status),
                        "region" => isAscending ? query.OrderBy(m => m.Region) : query.OrderByDescending(m => m.Region),
                        _ => query.OrderBy(m => m.NationalId)
                    };
                }
                else
                {
                    query = query.OrderBy(m => m.NationalId);
                }

                // Apply paging
                var filteredRecords = await query.CountAsync();
                var start = Math.Max(0, dtParameters.Start);
                var length = dtParameters.Length > 0 ? dtParameters.Length : 10;
                var examMonitors =  query
                        .Select(m => new 
                        {
                            NationalId = m.NationalId,
                          
                            FirstName = m.FirstName,
                            LastName = m.LastName,
                            Sex = m.Sex,
                            Status = m.Status,
                          
                            Region = m.Region,
                            Phone = m.Phone,
                    
                            Centre = m.Centre,
                            Station = m.Station,
                            AcceptStatus = m.AcceptStatus,
                            Assignments = m.ExamMonitorTransactions
                    .Where(t => t.SubKey != null)
                    .Select(t => new
                    {
                        t.Phase,
                        t.Session,
                        t.Status
                    })
                    .ToList()

                        })
                        .ToList();

                    return Json(new 
                    {
                        Draw = dtParameters.Draw,
                        RecordsTotal = totalRecords,
                        RecordsFiltered = filteredRecords,
                        Data = examMonitors
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = "An error occurred while processing your request." });
                }
            }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AssignMonitorsToTransaction([FromBody] AssignmentRequest request)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var transactionsToAdd = new List<ExamMonitorTransaction>();
                var alreadyAssignedMonitors = new List<string>();
                var successfullyAssigned = new List<string>();

                foreach (var nationalId in request.MonitorIds)
                {
                    var monitorRecord = await _context.ExamMonitors
                        .FirstOrDefaultAsync(m => m.NationalId == nationalId);

                    if (monitorRecord == null)
                        continue;

                    // Generate a unique subkey for this assignment
                    var subKey = monitorRecord.Region + request.Phase + request.Session + nationalId;

                    // Check if this monitor is already assigned to the same session and phase
                    var existingTransaction = await _context.ExamMonitorTransactions
                        .FirstOrDefaultAsync(t => t.NationalId == nationalId &&
                                                 t.Phase == request.Phase &&
                                                 t.Session == request.Session);

                    if (existingTransaction != null)
                    {
                        alreadyAssignedMonitors.Add($"{monitorRecord.FirstName} {monitorRecord.LastName} ({nationalId})");
                        continue;
                    }

                    // Get district from monitor record or current user
                    var district = monitorRecord.District ?? "00";

                    transactionsToAdd.Add(new ExamMonitorTransaction
                    {
                        SubKey = subKey,
                        MonitorId = monitorRecord.MonitorId,
                        Region = monitorRecord.Region,
                        NationalId = monitorRecord.NationalId,
                        CentreAttached = "XXXXXX",
                        District = district,
                        Phase = request.Phase,
                        Session = request.Session,
                        Status = monitorRecord.Status,
                        AssignedStatus = request.HrStatus,
                        AssignedDate = DateTime.UtcNow,
                        AssignedBy = currentUser.UserName,
                        DeployStatus = "Pending",
                        DeployStatusBy = currentUser.UserName,
                        DeployStatusDate = DateTime.UtcNow
                    });

                    successfullyAssigned.Add($"{monitorRecord.FirstName} {monitorRecord.LastName} ({nationalId})");
                }

                if (transactionsToAdd.Any())
                {
                    await _context.ExamMonitorTransactions.AddRangeAsync(transactionsToAdd);
                    await _context.SaveChangesAsync();
                }

                // Prepare response message
                var message = $"{successfullyAssigned.Count} monitors assigned successfully.";

                if (alreadyAssignedMonitors.Any())
                {
                    message += $" {alreadyAssignedMonitors.Count} monitors were already assigned to this session/phase.";
                }

                return Json(new
                {
                    success = true,
                    message = message,
                    assignedCount = successfullyAssigned.Count,
                    duplicateCount = alreadyAssignedMonitors.Count,
                    duplicates = alreadyAssignedMonitors,
                    assignedMonitors = successfullyAssigned
                });
            }
            catch (Exception ex)
            {
                // Log error
                //_logger.LogError(ex, "Error assigning monitors to transaction");
                return Json(new
                {
                    success = false,
                    message = "An error occurred while assigning monitors.",
                    error = ex.Message
                });
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetExamMonitorsRecruitments([FromBody] DtParameters dtParameters)
        {
            try
            {
                // Initialize with default values if null
                dtParameters ??= new DtParameters();

                var searchValue = dtParameters.Search?.Value ?? string.Empty;
                var statusFilter = dtParameters.StatusFilter ?? string.Empty;
                var genderFilter = dtParameters.GenderFilter ?? string.Empty;
                var centreFilter = dtParameters.CentreFilter ?? string.Empty;
                var districtFilter = dtParameters.DistrictFilter ?? string.Empty;

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(currentUser);

                // Base queries
                IQueryable<ExamMonitorsRecruitment> query1 = _context.ExamMonitorsRecruitments.AsQueryable();
                IQueryable<ExamMonitor> query2 = _context.ExamMonitors.AsQueryable();

                // Apply role-based filtering to both queries
                if (userRoles.Contains("RegionalManager"))
                {
                    if (string.IsNullOrEmpty(currentUser.Region))
                    {
                        return Json(new { data = new List<object>() });
                    }
                    query1 = query1.Where(x => x.Region == currentUser.Region);
                    query2 = query2.Where(x => x.Region == currentUser.Region);
                }
                else if (userRoles.Contains("Admin") || userRoles.Contains("HR"))
                {
                    var regionFilter = dtParameters.RegionFilter ?? string.Empty;
                    if (!string.IsNullOrEmpty(regionFilter))
                    {
                        query1 = query1.Where(x => x.Region == regionFilter);
                        query2 = query2.Where(x => x.Region == regionFilter);
                    }
                }

                // Apply filters to both queries
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    query1 = query1.Where(m => m.Status == statusFilter);
                    query2 = query2.Where(m => m.Status == statusFilter);
                }

                if (!string.IsNullOrEmpty(genderFilter))
                {
                    query1 = query1.Where(m => m.Sex == genderFilter);
                    query2 = query2.Where(m => m.Sex == genderFilter);
                }

                if (!string.IsNullOrEmpty(centreFilter))
                {
                    query1 = query1.Where(m => m.Centre == centreFilter);
                    query2 = query2.Where(m => m.Centre == centreFilter);
                }

                if (!string.IsNullOrEmpty(districtFilter))
                {
                    query1 = query1.Where(m => m.District == districtFilter);
                    query2 = query2.Where(m => m.District == districtFilter);
                }

                // Apply global search to both queries
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query1 = query1.Where(m =>
                        m.FirstName.Contains(searchValue) ||
                        m.LastName.Contains(searchValue) ||
                        m.NationalId.Contains(searchValue) ||
                        m.Phone.Contains(searchValue) ||
                        m.Centre.Contains(searchValue));

                    query2 = query2.Where(m =>
                        m.FirstName.Contains(searchValue) ||
                        m.LastName.Contains(searchValue) ||
                        m.NationalId.Contains(searchValue) ||
                        m.Phone.Contains(searchValue) ||
                        m.Centre.Contains(searchValue));
                }

                // Get total count from both queries
                var totalRecords1 = await query1.CountAsync();
                var totalRecords2 = await query2.CountAsync();
                var totalRecords = totalRecords1 + totalRecords2;

                // Apply sorting to both queries separately
                if (dtParameters.Order != null && dtParameters.Order.Any())
                {
                    var order = dtParameters.Order.First();
                    var columnIndex = order.Column;
                    var columnName = dtParameters.Columns[columnIndex].Data;
                    var isAscending = order.Dir == DtOrderDir.Asc;

                    query1 = columnName switch
                    {
                        "firstName" => isAscending ? query1.OrderBy(m => m.FirstName) : query1.OrderByDescending(m => m.FirstName),
                        "lastName" => isAscending ? query1.OrderBy(m => m.LastName) : query1.OrderByDescending(m => m.LastName),
                        "nationalId" => isAscending ? query1.OrderBy(m => m.NationalId) : query1.OrderByDescending(m => m.NationalId),
                        "status" => isAscending ? query1.OrderBy(m => m.Status) : query1.OrderByDescending(m => m.Status),
                        "region" => isAscending ? query1.OrderBy(m => m.Region) : query1.OrderByDescending(m => m.Region),
                        _ => query1.OrderBy(m => m.NationalId)
                    };

                    query2 = columnName switch
                    {
                        "firstName" => isAscending ? query2.OrderBy(m => m.FirstName) : query2.OrderByDescending(m => m.FirstName),
                        "lastName" => isAscending ? query2.OrderBy(m => m.LastName) : query2.OrderByDescending(m => m.LastName),
                        "nationalId" => isAscending ? query2.OrderBy(m => m.NationalId) : query2.OrderByDescending(m => m.NationalId),
                        "status" => isAscending ? query2.OrderBy(m => m.Status) : query2.OrderByDescending(m => m.Status),
                        "region" => isAscending ? query2.OrderBy(m => m.Region) : query2.OrderByDescending(m => m.Region),
                        _ => query2.OrderBy(m => m.NationalId)
                    };
                }
                else
                {
                    query1 = query1.OrderBy(m => m.NationalId);
                    query2 = query2.OrderBy(m => m.NationalId);
                }

                // Apply paging to both queries
                var start = Math.Max(0, dtParameters.Start);
                var length = dtParameters.Length > 0 ? dtParameters.Length : 10;

                var examMonitors1 = await query1
                    .Select(m => new
                    {
                        NationalId = m.NationalId,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        Sex = m.Sex,
                        Status = m.Status,
                        Region = m.Region,
                        Phone = m.Phone,
                        Centre = m.Centre,
                        Station = m.Station,
                        AcceptStatus = m.AcceptStatus,
                    })
            
                    .ToListAsync();

                var examMonitors2 = await query2
                    .Select(m => new
                    {
                        NationalId = m.NationalId,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        Sex = m.Sex,
                        Status = m.Status,
                        Region = m.Region,
                        Phone = m.Phone,
                        Centre = m.Centre,
                        Station = m.Station ?? string.Empty,
                        AcceptStatus = m.AcceptStatus,
                    })
               
                    .ToListAsync();

                // Combine the results
                var examMonitors = examMonitors1.Concat(examMonitors2).ToList();

                // Get filtered count
                var filteredRecords1 = await query1.CountAsync();
                var filteredRecords2 = await query2.CountAsync();
                var filteredRecords = filteredRecords1 + filteredRecords2;

                return Json(new
                {
                    Draw = dtParameters.Draw,
                    RecordsTotal = totalRecords,
                    RecordsFiltered = filteredRecords,
                    Data = examMonitors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }





        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] ExamMonitor monitor)
        {
            try
            {
                // Generate new GUID for MonitorId
                monitor.MonitorId = Guid.NewGuid();

                // Add validation as needed
                if (_context.ExamMonitors.Any(m => m.NationalId == monitor.NationalId))
                {
                    return Json(new { success = false, message = "Monitor with this National ID already exists" });
                }


                if (_context.ExamMonitorsRecruitments.Any(m => m.NationalId == monitor.NationalId))
                {
                    return Json(new { success = false, message = "Monitor with this National ID already exists" });
                }

                _context.ExamMonitors.Add(monitor);
                _context.SaveChanges();

                return Json(new { success = true, message = "Monitor created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // PUT: ExamMonitors/UpdateMonitor
        [HttpPost]
        [Authorize]
        public ActionResult UpdateMonitor([FromForm] MonitorUpdateDto dto)
        {
            try
            {
                var examMonitor = _context.ExamMonitors.FirstOrDefault(m => m.NationalId == dto.NationalId);

                if (examMonitor == null)
                {
                    return Json(new { success = false, message = "Monitor not found" });
                }

                // Update properties from DTO
                examMonitor.FirstName = dto.FirstName;
                examMonitor.LastName = dto.LastName;
                examMonitor.Sex = dto.Sex;
                examMonitor.Status = dto.Status;
                examMonitor.Region = dto.Region;
                examMonitor.Phone = dto.Phone;
                examMonitor.Centre = dto.Centre;
                examMonitor.Station = dto.Station;
                

                _context.SaveChanges();

                return Json(new { success = true, message = "Monitor updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating monitor: " + ex.Message });
            }
        }



        // DELETE: ExamMonitors/DeleteMonitor/{nationalId}
        [HttpPost("ExamMonitors/DeleteMonitor/{nationalId}")]
        [Authorize]
        public async Task<IActionResult> DeleteMonitor(string nationalId)
        {
            try
            {
                var monitor = await _context.ExamMonitors.FirstOrDefaultAsync(m => m.NationalId == nationalId);
                var monitorRecruitment = await _context.ExamMonitorsRecruitments.FirstOrDefaultAsync(a => a.NationalId == nationalId);
                if (monitor == null && monitorRecruitment == null)
                {
                    return Json(new { success = false, message = "Monitor not found" });
                }


                var professionalQualification = await _context.ExamMonitorProfessionalQualifications.Where(a => a.NationalId == monitor.NationalId).ToListAsync();

                foreach (var item in professionalQualification)
                {
                    _context.ExamMonitorProfessionalQualifications.Remove(item);
                    _context.SaveChanges();
                }

                var files = await _context.ExamMonitorAttachements.Where(a => a.NationalId == monitor.NationalId).ToListAsync();

                foreach (var item in files)
                {
                    _context.ExamMonitorAttachements.Remove(item);
                    _context.SaveChanges();
                }

                if (monitor != null)
                {
                    _context.ExamMonitors.Remove(monitor);
                    _context.SaveChanges();
                }
                else
                {
                    _context.ExamMonitorsRecruitments.Remove(monitorRecruitment);
                    _context.SaveChanges();
                }


                return Json(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("ExamMonitors/DeleteRecruitment/{nationalId}")]
        [Authorize]
        public async Task<IActionResult> DeleteRecruitment(string nationalId)
        {
            try
            {
                var monitor = await _context.ExamMonitors.FirstOrDefaultAsync(m => m.NationalId == nationalId);
                var monitorRecruitment = await _context.ExamMonitorsRecruitments.FirstOrDefaultAsync(a => a.NationalId == nationalId);
                if (monitor == null && monitorRecruitment == null)
                {
                    return Json(new { success = false, message = "Monitor not found" });
                }


                var professionalQualification = await _context.ExamMonitorProfessionalQualifications.Where(a => a.NationalId == monitor.NationalId).ToListAsync();

                foreach (var item in professionalQualification)
                {
                    _context.ExamMonitorProfessionalQualifications.Remove(item);
                    _context.SaveChanges();
                }

                var files = await _context.ExamMonitorAttachements.Where(a => a.NationalId == monitor.NationalId).ToListAsync();

                foreach (var item in files)
                {
                    _context.ExamMonitorAttachements.Remove(item);
                    _context.SaveChanges();
                }

                if (monitor != null)
                {
                    _context.ExamMonitors.Remove(monitor);
                    _context.SaveChanges();
                }
                else
                {
                    _context.ExamMonitorsRecruitments.Remove(monitorRecruitment);
                    _context.SaveChanges();
                }

                  

                return Json(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // GET: ExamMonitors/Details/{nationalId}
        [Authorize]
        public async Task<ActionResult> Details(string id)
        {
            var monitor = await _context.ExamMonitors
                .FirstOrDefaultAsync(m => m.NationalId == id);


            if (monitor == null)
            {
                return NotFound();
            }
            var attachments = await _context.ExamMonitorAttachements.Where(a => a.NationalId == id).ToListAsync();

            var qualifications = await _context.ExamMonitorProfessionalQualifications.Where(a => a.NationalId == id).ToListAsync();

            if (attachments.Any())
            {
                monitor.Attachments = attachments;
            }

            if (qualifications.Any()) {
                monitor.ProfessionalQualifications = qualifications;
            }


            return View(monitor);
        }


        [Authorize]
        public async Task<ActionResult> DetailsRecruitment(string id)
        {
            // Check recruitment table first
            var recruitmentMonitor = await _context.ExamMonitorsRecruitments
                .Include(e => e.ProfessionalQualifications)
                .FirstOrDefaultAsync(m => m.NationalId == id);

            var attachments = await _context.ExamMonitorAttachements.Where(a => a.NationalId == id).ToListAsync();

            var qualifications = await _context.ExamMonitorProfessionalQualifications.Where(a => a.NationalId == id).ToListAsync();

            if (recruitmentMonitor != null)
            {
                var viewModel = new MonitorDetailsViewModel
                {
                    NationalId = recruitmentMonitor.NationalId,
                    MonitorId = recruitmentMonitor.MonitorId,
                    FirstName = recruitmentMonitor.FirstName,
                    LastName = recruitmentMonitor.LastName,
                    Sex = recruitmentMonitor.Sex,
                    Status = recruitmentMonitor.Status,
                    Qualification = recruitmentMonitor.Qualification,
                    Experience = recruitmentMonitor.Experience,
                    Region = recruitmentMonitor.Region,
                    Phone = recruitmentMonitor.Phone,
                    Age = recruitmentMonitor.Age,
                    Centre = recruitmentMonitor.Centre,
                    Station = recruitmentMonitor.Station,
                    District = recruitmentMonitor.District,
                    Email = recruitmentMonitor.Email,
                    AcceptStatus = recruitmentMonitor.AcceptStatus,
                    AcceptBy = recruitmentMonitor.AcceptBy,
                    AcceptDate = recruitmentMonitor.AcceptDate,
                    Comment = recruitmentMonitor.Comment,
             
                   
                    EmailInvitations = recruitmentMonitor.EmailInvitations,
                    SourceTable = "Recruitment"
                };

                if (attachments.Any())
                {
                    viewModel.Attachments = attachments;
                }

                if (qualifications.Any())
                {
                    viewModel.ProfessionalQualifications = qualifications;
                }

                return View(viewModel);
            }

            // If not found in recruitment, check accepted monitors
            var acceptedMonitor = await _context.ExamMonitors
                .FirstOrDefaultAsync(m => m.NationalId == id);

            if (acceptedMonitor != null)
            {
                var viewModel = new MonitorDetailsViewModel
                {
                    NationalId = acceptedMonitor.NationalId,
                    MonitorId = acceptedMonitor.MonitorId,
                    FirstName = acceptedMonitor.FirstName,
                    LastName = acceptedMonitor.LastName,
                    Sex = acceptedMonitor.Sex,
                    Status = acceptedMonitor.Status,
                    Qualification = acceptedMonitor.Qualification,
                    Experience = acceptedMonitor.Experience,
                    Region = acceptedMonitor.Region,
                    Phone = acceptedMonitor.Phone,
                    Age = acceptedMonitor.Age,
                    Centre = acceptedMonitor.Centre,
                    Station = acceptedMonitor.Station,
                    District = acceptedMonitor.District,
                    Email = acceptedMonitor.Email,
                    AcceptStatus = acceptedMonitor.AcceptStatus,
                    AcceptBy = acceptedMonitor.AcceptBy,
                    AcceptDate = acceptedMonitor.AcceptDate,
                    Comment = acceptedMonitor.Comment,
                    ProfessionalQualifications = acceptedMonitor.ProfessionalQualifications,
                  
                    SourceTable = "Monitor"
                };

                if (attachments.Any())
                {
                    viewModel.Attachments = attachments;
                }

                if (qualifications.Any())
                {
                    viewModel.ProfessionalQualifications = qualifications;
                }

                return View(viewModel);
            }

            return NotFound();
        }
    }





    // DTO classes for DataTables and API requests

    public class MonitorUpdateDto
    {
        public string NationalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string Status { get; set; }
        public string Region { get; set; }
        public string Phone { get; set; }
        public string Centre { get; set; }
        public string Station { get; set; }
   
    }

    public class UploadError
    {
        public int RowNumber { get; set; }
        public string NationalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class DtParameters
    {
        public int Draw { get; set; }
        public DtColumn[] Columns { get; set; }
        public DtOrder[] Order { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public DtSearch Search { get; set; }
        public string RegionFilter { get; set; }
        public string StatusFilter { get; set; }
        public string GenderFilter { get; set; }
        public string ClusterFilter { get; set; }
        public string CentreFilter { get; set; }
        public string DistrictFilter { get; set; }
        public string PhaseFilter { get; set; }
        public string SessionFilter { get; set; }
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
        public DtOrderDir Dir { get; set; }
    }

    public enum DtOrderDir
    {
        Asc,
        Desc
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
        public IEnumerable<T> Data { get; set; }
        public string Error { get; set; }
    }
    public class AssignmentRequest
    {
        public string[] MonitorIds { get; set; }
        public string Session { get; set; }
        public string Phase { get; set; }
        public string HrStatus { get; set; } = "Selected";
    }
    public class ExamMonitorDto
    {
        public string NationalId { get; set; }
        public Guid MonitorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string Status { get; set; }
        public string Qualification { get; set; }
        public string Experience { get; set; }
        public string Region { get; set; }
        public string Phone { get; set; }
        public string Age { get; set; }
        public string Centre { get; set; }
        public string Station { get; set; }
        public string District { get; set; }
        public string BankNameZwg { get; set; }
        public string BranchZwg { get; set; }
        public string AccountNumberZwg { get; set; }
        public string BankNameUsd { get; set; }
        public string BranchUsd { get; set; }
        public string AccountNumberUsd { get; set; }
    }

    public class ExamMonitorDetailDto
    {
        public string NationalId { get; set; }
        public Guid MonitorId { get; set; }
        public string FullName { get; set; }
        public string Sex { get; set; }
        public string Status { get; set; }
        public string Qualification { get; set; }
        public string Experience { get; set; }
        public string Region { get; set; }
        public string Phone { get; set; }
        public string Age { get; set; }
        public string Centre { get; set; }
        public string Station { get; set; }
        public string District { get; set; }
        public string BankDetailsZwg { get; set; }
        public string BankDetailsUsd { get; set; }
    }
}
