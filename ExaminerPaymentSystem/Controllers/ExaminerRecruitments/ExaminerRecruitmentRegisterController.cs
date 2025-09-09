
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using iText.Kernel.Geom;
using System.Diagnostics.Metrics;
using ClosedXML.Excel;


namespace ExaminerPaymentSystem.Controllers.ExaminerRecruitments
{
    public class ExaminerRecruitmentRegisterController : Controller
    {
        private readonly IExaminerRecruitmentRegisterRepository _repository;
        private readonly IExaminerRecruitmentAssessmentRepository _assessmentRepository;
        private readonly IExaminerRecruitmentTrainingSelectionRepository _trainingSelectionRepository;

        public ExaminerRecruitmentRegisterController(IExaminerRecruitmentRegisterRepository repository
            , IExaminerRecruitmentAssessmentRepository assessmentRepository,
             IExaminerRecruitmentTrainingSelectionRepository trainingSelectionRepository)
        {
            _repository = repository;
            _assessmentRepository = assessmentRepository;
            _trainingSelectionRepository = trainingSelectionRepository;
        }

        public IActionResult RegisterIndex(SessionData model) {

            // Store the filter values in session

            HttpContext.Session.SetString("PaperCode", model.PaperCode ?? string.Empty);
            HttpContext.Session.SetString("Subject", model.Subject ?? string.Empty);
            HttpContext.Session.SetString("Experience", model.Experience ?? string.Empty);


            // Set the session values to ViewBag to use them in the JavaScript
            ViewBag.Subject = HttpContext.Session.GetString("Subject");
            ViewBag.PaperCode = HttpContext.Session.GetString("PaperCode");
            ViewBag.Experience = HttpContext.Session.GetString("Experience");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadRegisterBasedOnParametersExcel(string subject, string paperCode, string experience)
        {

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(subject) && string.IsNullOrWhiteSpace(paperCode) && string.IsNullOrWhiteSpace(experience))
                {
                    return BadRequest(new { success = false, message = "At least one parameter must be provided." });
                }

                var data = await _repository.LoadByParametersRegister(subject, paperCode, experience);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("ExaminerRegister");

                    // Add main heading
                    worksheet.Cell(1, 1).Value = "Trainee Examiner Register";
                    worksheet.Range(1, 1, 1, 11).Merge();
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Add subheadings
                    worksheet.Cell(2, 1).Value = $"Level: {experience ?? "Not specified"}";
                    worksheet.Range(2, 1, 2, 3).Merge();
                    worksheet.Cell(2, 1).Style.Font.Bold = true;

                    worksheet.Cell(2, 4).Value = $"Subject: {subject ?? "Not specified"}";
                    worksheet.Range(2, 4, 2, 6).Merge();
                    worksheet.Cell(2, 4).Style.Font.Bold = true;

                    worksheet.Cell(2, 7).Value = $"Paper Code: {paperCode ?? "Not specified"}";
                    worksheet.Range(2, 7, 2, 11).Merge();
                    worksheet.Cell(2, 7).Style.Font.Bold = true;

                    // Add column headers (starting from row 4)
                    worksheet.Cell(4, 1).Value = "Examiner Name";
                    worksheet.Cell(4, 2).Value = "Last Name";
                    worksheet.Cell(4, 3).Value = "National ID";
                    worksheet.Cell(4, 4).Value = "Level";
                    worksheet.Cell(4, 5).Value = "Subject";
                    worksheet.Cell(4, 6).Value = "Paper Code";
                    worksheet.Cell(4, 7).Value = "Phone";
                    worksheet.Cell(4, 8).Value = "Email";
                    worksheet.Cell(4, 9).Value = "Gender";
                    worksheet.Cell(4, 10).Value = "Status";
                    worksheet.Cell(4, 11).Value = "Signature";

                    // Style headers
                    var headerRange = worksheet.Range(4, 1, 4, 11);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Add borders to table
                    var tableRange = worksheet.Range(4, 1, data.Count + 4, 11);
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Add data (starting from row 5)
                    for (int i = 0; i < data.Count; i++)
                    {
                        var row = i + 5;
                        worksheet.Cell(row, 1).Value = data[i].ExaminerName;
                        worksheet.Cell(row, 2).Value = data[i].LastName;
                        worksheet.Cell(row, 3).Value = data[i].CemId;
                        worksheet.Cell(row, 4).Value = data[i].Experience;
                        worksheet.Cell(row, 5).Value = data[i].Subject;
                        worksheet.Cell(row, 6).Value = data[i].PaperCode;
                        worksheet.Cell(row, 7).Value = data[i].PhoneHome;
                        worksheet.Cell(row, 8).Value = data[i].EmailAddress;
                        worksheet.Cell(row, 9).Value = data[i].Gender;
                        // Convert boolean to Present/Absent
                        worksheet.Cell(row, 10).Value = "Present";
                        worksheet.Cell(row, 11).Value = "";
                    }

                    // Auto-fit columns and set minimum width
                    worksheet.Columns(1, 11).AdjustToContents();
                    for (int col = 1; col <= 11; col++)
                    {
                        var column = worksheet.Column(col);
                        if (column.Width < 11)
                            column.Width = 11;
                    }

                    // Set specific widths for Email and Gender
                    worksheet.Column(8).Width = 25; // Email
                    worksheet.Column(9).Width = 8;  // Gender

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        string formattedDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string safeSubject = string.IsNullOrEmpty(subject) ? "AllSubjects" : subject;
                        string safeExperience = string.IsNullOrEmpty(experience) ? "AllLevels" : experience;
                        string fileName = $"TraineeExaminerRegister_{safeSubject}_{safeExperience}_{formattedDate}.xlsx";

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }

            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "An error occurred while generating the Excel file: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadRegisterBasedOnParametersPDF(string subject, string paperCode, string experience)
        {
            try
            {
                var data = await _repository.LoadByParametersRegister(subject, paperCode, experience);

                using (var memoryStream = new MemoryStream())
                {
                    PdfWriter writer = new PdfWriter(memoryStream);
                    PdfDocument pdf = new PdfDocument(writer);
                    Document document = new Document(pdf, PageSize.A4);
                    document.SetMargins(20, 20, 20, 20); // Set consistent margins

                    // Add title
                    document.Add(new Paragraph("Trainee Examiner Register")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetBold());

                    // Add subheadings
                    document.Add(new Paragraph($"Subject: {subject ?? "Not specified"}").SetFontSize(12));
                    document.Add(new Paragraph($"Level: {experience ?? "Not specified"}").SetFontSize(12));
                    document.Add(new Paragraph($"PaperCode: {paperCode ?? "Not specified"}").SetFontSize(12));
                    document.Add(new Paragraph("").SetMarginBottom(10)); // Spacer

                    // Adjusted column widths to avoid Signature column overlap
                    float[] columnWidths = { 4f, 14f, 14f, 10f, 4f, 10f, 4f, 12f, 4f, 6f, 18f };
                    Table table = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();

                    // Add headers
                    table.AddHeaderCell(new Cell().Add(new Paragraph("No").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Name").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Sur").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Lev").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Sub").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("P.C").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Cell").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("G").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Status").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Signature").SetBold()));

                    // Add data rows
                    int counter = 1;
                    foreach (var item in data)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(counter.ToString())));
                        table.AddCell(new Cell().Add(new Paragraph(item.ExaminerName ?? "")));
                        table.AddCell(new Cell().Add(new Paragraph(item.LastName ?? "")));
                        table.AddCell(new Cell().Add(new Paragraph(item.CemId ?? "")));
                        table.AddCell(new Cell().Add(new Paragraph(item.Experience ?? "")));
                        table.AddCell(new Cell().Add(new Paragraph(item.Subject ?? "")));
                        table.AddCell(new Cell().Add(new Paragraph(item.PaperCode ?? "")));
                        table.AddCell(new Cell().Add(new Paragraph(item.PhoneHome ?? "")));
                        table.AddCell(new Cell().Add(new Paragraph(item.Gender ?? "")));
                        table.AddCell(new Cell().Add(new Paragraph("P"))); // Placeholder for status
                        table.AddCell(new Cell().Add(new Paragraph("")));   // Signature cell left empty
                        counter++;
                    }

                    // Add table to document
                    table.SetKeepTogether(false);
                    document.Add(table);

                    // Finalize PDF
                    document.Close();

                    string formattedDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string fileName = $"ExaminerRegister_{subject}_{experience}_{formattedDate}.pdf";

                    return File(memoryStream.ToArray(), "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to generate PDF file: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> LoadExaminerRecruitmentsRegister(string? status)
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
           // var registerStatus = Request.Form["registerStatus"].FirstOrDefault();
            var subject = Request.Form["subject"].FirstOrDefault();
            var paperCode = Request.Form["paperCode"].FirstOrDefault();
            var experience = Request.Form["experience"].FirstOrDefault();

            var result = await _trainingSelectionRepository.GetPaginatedRegisterAsync(
                start,
                length,
                searchValue,
                sortColumn,
                sortDirection,
                subject,
                paperCode,
                experience);

            if (!string.IsNullOrEmpty(status))
            {
                result.Data = status switch
                {
                    "Total" => result.Data.ToList(),
                    "Present" => result.Data.Where(x => x.RegisterStatus == true).ToList(),
                    "Pending" => result.Data.Where(x => x.RegisterStatus == null).ToList(),
                    "Absent" => result.Data.Where(x => x.RegisterStatus == false).ToList(),
                    _ => result.Data.ToList()
                };
            }

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = result.TotalCount,
                recordsTotal = result.TotalCount,
                data = result.Data,
                totalExaminersWithRegisterTrue = result.TotalExaminersWithRegisterTrue,
                totalExaminersWithRegisterFalse = result.TotalExaminersWithRegisterFalse,
                totalSelectedNotRegistered = result.TotalSelectedNotRegistered,
                totalSelectedInTrainerTab = result.TotalSelectedInTrainerTab
            };

            return Json(jsonData);
        }



        [HttpGet]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null) return NotFound($"Entity with ID {id} not found.");
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


     

        [HttpPost]
        public async Task<IActionResult> CreateAsync(ExaminerRecruitmentRegister entity)
        {
            try
            {
                if (entity == null) return BadRequest("Entity cannot be null.");

                await _repository.AddAsync(entity);
                await _repository.SaveAsync();

                return CreatedAtAction(nameof(GetByIdAsync), new { id = entity.Id }, entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAsync(int id, ExaminerRecruitmentRegister entity)
        {
            try
            {
                if (entity == null || entity.Id != id) return BadRequest("Entity ID mismatch.");

                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null) return NotFound($"Entity with ID {id} not found.");

                await _repository.UpdateAsync(entity);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null) return NotFound($"Entity with ID {id} not found.");

                await _repository.DeleteAsync(id);
                await _repository.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAttendance([FromBody] ExaminerRecruitmentRegisterViewModel request)
        {
            try
            {
                // Check if the examiner has a grade
                var checkGrade = await _assessmentRepository.GetByExaminerIdAsync(request.ExaminerRecruitmentId);

                if (checkGrade != null && request.RegisterStatus == false)
                {
                    return BadRequest(new { message = "You cannot mark absent the trainee examiner who already has a grade." });
                }

                if (request == null) return BadRequest("Invalid request.");

                var existingRecord = await _repository.GetByIdAsync(request.ExaminerRecruitmentId);

                if (existingRecord != null)
                {
                    // Toggle the status
                    existingRecord.Status = request.RegisterStatus;
                    await _repository.UpdateAsync(existingRecord);
                }
                else
                {
                    // Add a new record
                    var newRecord = new ExaminerRecruitmentRegister
                    {
                        ExaminerRecruitmentId = request.ExaminerRecruitmentId,
                        Status = request.RegisterStatus,
                        Date = DateTime.Now
                    };
                    await _repository.AddAsync(newRecord);
                }

                await _repository.SaveAsync();
                return Ok(new { message = "Status toggled successfully." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Database update error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        public IActionResult PresentExaminerTraineesWithoutGrades()
        {

            return View();

        }


        [HttpPost("GetPresentExaminersWithoutGrades")]
        public async Task<IActionResult> GetPresentExaminersWithoutGrades()
        {
            try
            {
                // Retrieve DataTable parameters
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
                var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
                var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();

                // Retrieve custom filter parameters
                var sessionLevelFilter = Request.Form["sessionLevel"].FirstOrDefault();
                var subjectFilter = Request.Form["subject"].FirstOrDefault();
                var paperCodeFilter = Request.Form["paperCode"].FirstOrDefault();
                var regionCodeFilter = Request.Form["regionCodeFilter"].FirstOrDefault();

                // Fetch data from repository with filters
                var assessments = await _repository.GetPresentExaminersWithoutGradesAsync(
                    sessionLevelFilter, subjectFilter, paperCodeFilter, regionCodeFilter);

                // Store total records before search
                var recordsTotal = assessments.Count;

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchValue))
                {
                    assessments = assessments.Where(a =>
                        (a.ExaminerName?.ToLower().Contains(searchValue) ?? false) ||
                        (a.LastName?.ToLower().Contains(searchValue) ?? false) ||
                        (a.CemId?.ToLower().Contains(searchValue) ?? false) ||
                        (a.Subject?.ToLower().Contains(searchValue) ?? false) ||
                        (a.PaperCode?.ToLower().Contains(searchValue) ?? false)
                    ).ToList();
                }

                // Store filtered records after search
                var recordsFiltered = assessments.Count;

                // Apply pagination
                var data = assessments.Skip(start).Take(length).ToList();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = data.Select(a => new
                    {
                        examinerName = a.ExaminerName, // Consistent naming
                        lastName = a.LastName,
                        cemId = a.CemId,
                        subject = a.Subject,
                        paperCode = a.PaperCode,
                        Capturer = a.CapturerUserName,
                        Verifier = a.VerifierUserName,
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Error retrieving data: {ex.Message}" });
            }
        }


    }
}
