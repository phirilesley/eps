using ClosedXML.Excel;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ExaminerPaymentSystem.Controllers.ExaminerRecruitments
{
  
  
    public class ExaminerRecruitmentEmailInvitationController : Controller
    {
        private readonly IExaminerRecruitmentEmailInvitationRepository _repository;

        public ExaminerRecruitmentEmailInvitationController(
            IExaminerRecruitmentEmailInvitationRepository repository)
        {
            _repository = repository;
        }

        // GET: /ExaminerRecruitmentEmailInvitations
        public IActionResult EmailInviteIndex(SessionData model)
        {
     
            // Store the filter values in session
            HttpContext.Session.SetString("Region", model.Region ?? string.Empty);
            HttpContext.Session.SetString("PaperCode", model.PaperCode ?? string.Empty);
            HttpContext.Session.SetString("Subject", model.Subject ?? string.Empty);
            HttpContext.Session.SetString("Experience", model.Experience ?? string.Empty);

            // Set the session values to ViewBag to use them in the JavaScript
            ViewBag.Region = HttpContext.Session.GetString("Region");
            ViewBag.Subject = HttpContext.Session.GetString("Subject");
            ViewBag.PaperCode = HttpContext.Session.GetString("PaperCode");
            ViewBag.Experience = HttpContext.Session.GetString("Experience");

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetData(
               string region, string subject, string paperCode, string experience)
        {
            try
            {
                // Extract DataTable parameters
                var draw = Request.Query["draw"].FirstOrDefault();
                var start = Request.Query["start"].FirstOrDefault();
                var length = Request.Query["length"].FirstOrDefault();
                var sortColumnIndex = Request.Query["order[0][column]"].FirstOrDefault();
                var sortDirection = Request.Query["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Query["search[value]"].FirstOrDefault();

                int skip = string.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start);
                int take = string.IsNullOrEmpty(length) ? 10 : Convert.ToInt32(length);

                var sortColumn = !string.IsNullOrEmpty(sortColumnIndex)
                    ? Request.Query[$"columns[{sortColumnIndex}][data]"].FirstOrDefault()
                    : "ExaminerName";

                sortDirection = string.IsNullOrEmpty(sortDirection) ? "asc" : sortDirection;

                // Get data
                var allData = (await _repository.GetAllAsync()).ToList();

                // Apply explicit session-based filters
                if (!string.IsNullOrEmpty(region))
                {
                    allData = allData
                        .Where(x => x.ExaminerRecruitment?.RegionCode != null &&
                                    x.ExaminerRecruitment.RegionCode.Equals(region, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(subject))
                {
                    allData = allData
                        .Where(x => x.ExaminerRecruitment?.Subject != null &&
                                    x.ExaminerRecruitment.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(paperCode))
                {
                    allData = allData
                        .Where(x => x.ExaminerRecruitment?.PaperCode != null &&
                                    x.ExaminerRecruitment.PaperCode.Equals(paperCode, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(experience))
                {
                    allData = allData
                        .Where(x => x.ExaminerRecruitment?.Experience != null &&
                                    x.ExaminerRecruitment.Experience.Equals(experience, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // Apply search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    allData = allData.Where(x =>
                        (!string.IsNullOrEmpty(x.ExaminerRecruitment?.ExaminerName) &&
                         x.ExaminerRecruitment.ExaminerName.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.ExaminerRecruitment?.LastName) &&
                         x.ExaminerRecruitment.LastName.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                          (!string.IsNullOrEmpty(x.ExaminerRecruitment?.Experience) &&
                         x.ExaminerRecruitment.Experience.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                          (!string.IsNullOrEmpty(x.ExaminerRecruitment?.Subject) &&
                         x.ExaminerRecruitment.Subject.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                          (!string.IsNullOrEmpty(x.ExaminerRecruitment?.PaperCode) &&
                         x.ExaminerRecruitment.PaperCode.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.InvitedByUser?.UserName) &&
                         x.InvitedByUser.UserName.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        x.DateInvited.ToString("dd MMM yyyy HH:mm").Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Apply sorting
                allData = sortDirection == "asc"
                    ? allData.OrderBy(x => GetSortValue(x, sortColumn)).ToList()
                    : allData.OrderByDescending(x => GetSortValue(x, sortColumn)).ToList();

                var recordsTotal = allData.Count;

                // Apply paging
                var data = allData
                    .Skip(skip)
                    .Take(take)
                    .Select(x => new
                    {
                        Id = x.Id,
                        ExaminerName = x.ExaminerRecruitment?.ExaminerName ?? "N/A",
                        LastName = x.ExaminerRecruitment?.LastName ?? "N/A",
                        Level = x.ExaminerRecruitment?.Experience ?? "N/A",
                        Subject = x.ExaminerRecruitment?.Subject ?? "N/A",
                        PaperCode = x.ExaminerRecruitment?.PaperCode ?? "N/A",
                        Region = x.ExaminerRecruitment?.RegionCode ?? "N/A",
                        InvitedBy = x.InvitedByUser?.UserName ?? "N/A",
                        DateInvited = x.DateInvited.ToString("dd MMM yyyy HH:mm")
                    })
                    .ToList();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsTotal,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }




        [HttpGet]
        public async Task<IActionResult> Download(string region, string subject, string paperCode, string experience, string searchValue, string format)
        {
            try
            {
                var allData = await _repository.GetAllAsync();

                if (!string.IsNullOrEmpty(region))
                {
                    allData = allData.Where(x => x.ExaminerRecruitment?.RegionCode == region);
                }
                if (!string.IsNullOrEmpty(subject))
                {
                    allData = allData.Where(x => x.ExaminerRecruitment?.Subject == subject);
                }
                if (!string.IsNullOrEmpty(paperCode))
                {
                    allData = allData.Where(x => x.ExaminerRecruitment?.PaperCode == paperCode);
                }
                if (!string.IsNullOrEmpty(experience))
                {
                    allData = allData.Where(x => x.ExaminerRecruitment?.Experience == experience);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    allData = allData.Where(x =>
                        (x.ExaminerRecruitment?.ExaminerName?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (x.ExaminerRecruitment?.LastName?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (x.ExaminerRecruitment?.Experience?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (x.ExaminerRecruitment?.Subject?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (x.ExaminerRecruitment?.PaperCode?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (x.ExaminerRecruitment?.RegionCode?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (x.InvitedByUser?.UserName?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        x.DateInvited.ToString("dd MMM yyyy HH:mm").Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    );
                }

                var data = allData
                    .OrderBy(x => x.ExaminerRecruitment?.ExaminerName ?? "")
                    .Select(x => new
                    {
                        ExaminerName = x.ExaminerRecruitment?.ExaminerName ?? "N/A",
                        LastName = x.ExaminerRecruitment?.LastName ?? "N/A",
                        Level = x.ExaminerRecruitment?.Experience ?? "N/A",
                        Subject = x.ExaminerRecruitment?.Subject ?? "N/A",
                        PaperCode = x.ExaminerRecruitment?.PaperCode ?? "N/A",
                        Region = x.ExaminerRecruitment?.RegionCode ?? "N/A",
                        InvitedBy = x.InvitedByUser?.UserName ?? "N/A",
                        DateInvited = x.DateInvited.ToString("dd MMM yyyy HH:mm")
                    })
                    .ToList();

                // Generate file name: level_subject_papercode_currentdatetime
                var level = string.IsNullOrEmpty(experience) ? experience : Regex.Replace(experience, "[^a-zA-Z0-9_-]", "_");
                var subjectName = string.IsNullOrEmpty(subject) ? subject : Regex.Replace(subject, "[^a-zA-Z0-9_-]", "_");
                var paper = string.IsNullOrEmpty(paperCode) ? paperCode : Regex.Replace(paperCode, "[^a-zA-Z0-9_-]", "_");
                var dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileNameBase = $"{level}_{subjectName}_{paper}_{dateTime}";

                if (format?.ToLower() == "excel")
                {
                    // Generate Excel
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Invitations");

                    // Add main heading
                    worksheet.Cell(1, 1).Value = "Invited Trainee Examiners";
                    worksheet.Range("A1:H1").Merge().Style
                        .Font.SetBold()
                        .Font.SetFontSize(16)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // Add filter information on second row
                    worksheet.Cell(2, 1).Value = $"Level: {experience ?? "N/A"}";
                    worksheet.Cell(2, 2).Value = $"Subject: {subject ?? "N/A"}";
                    worksheet.Cell(2, 3).Value = $"Paper Code: {paperCode ?? "N/A"}";
                    worksheet.Range("A2:H2").Style
                        .Font.SetItalic()
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                    // Add headers on third row
                    worksheet.Cell(3, 1).Value = "First Name";
                    worksheet.Cell(3, 2).Value = "Last Name";
                    worksheet.Cell(3, 3).Value = "Level";
                    worksheet.Cell(3, 4).Value = "Subject";
                    worksheet.Cell(3, 5).Value = "Paper Code";
                    worksheet.Cell(3, 6).Value = "Region";
                    worksheet.Cell(3, 7).Value = "Invited By";
                    worksheet.Cell(3, 8).Value = "Date Invited";

                    // Style headers
                    var headerRange = worksheet.Range("A3:H3");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Add data starting from fourth row
                    for (int i = 0; i < data.Count; i++)
                    {
                        worksheet.Cell(i + 4, 1).Value = data[i].ExaminerName;
                        worksheet.Cell(i + 4, 2).Value = data[i].LastName;
                        worksheet.Cell(i + 4, 3).Value = data[i].Level;
                        worksheet.Cell(i + 4, 4).Value = data[i].Subject;
                        worksheet.Cell(i + 4, 5).Value = data[i].PaperCode;
                        worksheet.Cell(i + 4, 6).Value = data[i].Region;
                        worksheet.Cell(i + 4, 7).Value = data[i].InvitedBy;
                        worksheet.Cell(i + 4, 8).Value = data[i].DateInvited;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Save to stream
                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileNameBase}.xlsx");
                }
                else if (format?.ToLower() == "pdf")
                {
                    // Generate PDF
                    using var stream = new MemoryStream();
                    using var writer = new PdfWriter(stream);
                    using var pdf = new PdfDocument(writer);
                    var document = new Document(pdf);

                    // Add main heading
                    document.Add(new Paragraph("Invited Trainee Examiners")
                        .SetFontSize(16)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(10));

                    // Add filter information
                    document.Add(new Paragraph($"Level: {experience ?? "N/A"}, Subject: {subject ?? "N/A"}, Paper Code: {paperCode ?? "N/A"}")
                        .SetFontSize(12)
                        .SetItalic()
                        .SetMarginBottom(10));

                    // Create table with 8 columns
                    var table = new Table(UnitValue.CreatePercentArray(new float[] { 15, 15, 10, 10, 10, 10, 15, 15 })).UseAllAvailableWidth();

                    // Add headers
                    table.AddHeaderCell(new Cell().Add(new Paragraph("FirstName").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("LastName").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Level").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Subject").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("PaperCode").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Region").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("InvitedBy").SetBold()));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("DateInvited").SetBold()));

                    // Add data
                    foreach (var item in data)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(item.ExaminerName)));
                        table.AddCell(new Cell().Add(new Paragraph(item.LastName)));
                        table.AddCell(new Cell().Add(new Paragraph(item.Level)));
                        table.AddCell(new Cell().Add(new Paragraph(item.Subject)));
                        table.AddCell(new Cell().Add(new Paragraph(item.PaperCode)));
                        table.AddCell(new Cell().Add(new Paragraph(item.Region)));
                        table.AddCell(new Cell().Add(new Paragraph(item.InvitedBy)));
                        table.AddCell(new Cell().Add(new Paragraph(item.DateInvited)));
                    }

                    document.Add(table);
                    document.Close();

                    return File(stream.ToArray(), "application/pdf", $"{fileNameBase}.pdf");
                }
                else
                {
                    return BadRequest(new { error = "Invalid format specified. Use 'excel' or 'pdf'." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while generating the file." });
            }
        }

        private object GetSortValue(ExaminerRecruitmentEmailInvitation x, string sortColumn)
        {
            return sortColumn switch
            {
                "ExaminerName" => x.ExaminerRecruitment?.ExaminerName ?? "",
                "InvitedBy" => x.InvitedByUser?.UserName ?? "",
                "DateInvited" => x.DateInvited,
                _ => x.Id
            };
        }

        private IQueryable<ExaminerRecruitmentEmailInvitation> ApplySorting(
            IQueryable<ExaminerRecruitmentEmailInvitation> query,
            string sortColumn,
            bool ascending)
        {
            return sortColumn switch
            {
                "ExaminerName" => ascending ?
                    query.OrderBy(x => x.ExaminerRecruitment.ExaminerName) :
                    query.OrderByDescending(x => x.ExaminerRecruitment.ExaminerName),
                "InvitedBy" => ascending ?
                    query.OrderBy(x => x.InvitedByUser.UserName) :
                    query.OrderByDescending(x => x.InvitedByUser.UserName),
                "DateInvited" => ascending ?
                    query.OrderBy(x => x.DateInvited) :
                    query.OrderByDescending(x => x.DateInvited),
                _ => ascending ?
                    query.OrderBy(x => x.DateInvited) :
                    query.OrderByDescending(x => x.DateInvited)
            };
        }



        // POST: /ExaminerRecruitmentEmailInvitations/Delete/5
        [HttpPost]
 
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return Json(new { success = true, message = "Invitation deleted successfully." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to delete invitation." });
            }
        }

        // POST: /ExaminerRecruitmentEmailInvitations/BulkDelete
        [HttpPost]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "No invitations selected for deletion." });
            }

            try
            {
                await _repository.BulkDeleteAsync(ids);
                return Json(new { success = true, message = $"Successfully deleted {ids.Count} invitation(s)." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to delete invitations." });
            }
        }
    }

}
