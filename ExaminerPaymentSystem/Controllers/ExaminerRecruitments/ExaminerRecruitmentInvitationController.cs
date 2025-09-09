
using ExaminerPaymentSystem.Extensions;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using MimeKit;
using MailKit.Net.Smtp;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using System.Security.Claims;
using ExaminerPaymentSystem.Data;
using Microsoft.EntityFrameworkCore;






namespace ExaminerPaymentSystem.Controllers.ExaminerRecruitments
{
    public class ExaminerRecruitmentInvitationController : Controller
    {
        private readonly IExaminerRecruitmentTrainingSelectionRepository _traineeSelectionRepository;
        private readonly IExaminerRecruitmentRepository _recruitmentRepository;
        private readonly IExaminerRecruitmentVenueDetailsRepository _venueDetailsRepository;
        private readonly ILogger<ExaminerRecruitmentInvitationController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IExaminerRecruitmentEmailInvitationRepository _emailInvitationRepository;
        private readonly ApplicationDbContext _emailInvitedContext;

        public ExaminerRecruitmentInvitationController(IExaminerRecruitmentTrainingSelectionRepository traineeSelectionRepository,
                                                        IExaminerRecruitmentRepository recruitmentRepository,
                                                        IExaminerRecruitmentVenueDetailsRepository venueDetailsRepository,
                                                        ILogger<ExaminerRecruitmentInvitationController> logger,
                                                        IWebHostEnvironment hostingEnvironment,
                                                        IExaminerRecruitmentEmailInvitationRepository emailInvitationRepository,
                                                        ApplicationDbContext contextByEmail   )
        {
            _traineeSelectionRepository = traineeSelectionRepository;
            _recruitmentRepository = recruitmentRepository;
            _venueDetailsRepository = venueDetailsRepository;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _emailInvitationRepository = emailInvitationRepository;
            _emailInvitedContext = contextByEmail;
        }


        [HttpGet]
        public async Task<IActionResult> InviteIndex(SessionData model)
        {
            // Store the filter values in session
            HttpContext.Session.SetString("Region", model.Region ?? string.Empty);
            HttpContext.Session.SetString("PaperCode", model.PaperCode ?? string.Empty);
            HttpContext.Session.SetString("Subject", model.Subject ?? string.Empty);
            HttpContext.Session.SetString("Experience", model.Experience ?? string.Empty);
            HttpContext.Session.SetString("ExaminerRecruitsVenue", model.ExaminerRecruitsVenue ?? string.Empty);

            // Set the session values to ViewBag to use them in the JavaScript
            ViewBag.Region = HttpContext.Session.GetString("Region");
            ViewBag.Subject = HttpContext.Session.GetString("Subject");
            ViewBag.PaperCode = HttpContext.Session.GetString("PaperCode");
            ViewBag.Experience = HttpContext.Session.GetString("Experience");

            return View();
        }






        [HttpGet]
        [Route("ExaminerRecruitmentInvitation/GetTrainingSelectionDataTable")]
        public async Task<IActionResult> GetTrainingSelectionDataTable(
        int start,
        int length,
        string searchValue,
        string sortColumn,
        string sortDirection,
        string Experience,
        string Subject,
        string PaperCode,
        string Region)
{
    // Retrieve session values if parameters are empty
    var userSession = HttpContext.Session.GetObjectFromJson<SessionData>("Session") ?? new SessionData();

    // Fallback to session values if query parameters are not provided
    Experience = !string.IsNullOrEmpty(Experience) ? Experience : userSession.Experience;
    Subject = !string.IsNullOrEmpty(Subject) ? Subject : userSession.Subject;
    PaperCode = !string.IsNullOrEmpty(PaperCode) ? PaperCode : userSession.PaperCode;
    Region = !string.IsNullOrEmpty(Region) ? Region : userSession.Region;


           
    // Retrieve filtered records from the repository
    var (data, totalRecords, filteredRecords) = await _traineeSelectionRepository.GetTrainingSelectionDataTableAsync(
        start,
        length,
        searchValue,
        sortColumn,
        sortDirection,
        Subject,
        Experience,
        PaperCode,
        Region
    );

    // Return the result as JSON for DataTables
    return Json(new
    {
        draw = HttpContext.Request.Query["draw"],
        recordsTotal = totalRecords,
        recordsFiltered = filteredRecords,
        data = data,
    });
}


            public async Task<IActionResult> GenarateInvitationLetter(int examinerRecruitmentId)
        {
            var examiner = await _recruitmentRepository.GetByIdAsync(examinerRecruitmentId);
            if (examiner == null)
            {
                return NotFound("Examiner not found.");
            }

            var venueDetailIdString = HttpContext.Session.GetString("ExaminerRecruitsVenue");
            if (!int.TryParse(venueDetailIdString, out int venueDetailId))
            {
                return BadRequest("Invalid Venue ID.");
            }

            var venueDetails = await _venueDetailsRepository.GetByIdAsync(venueDetailId);
            if (venueDetails == null)
            {
                return NotFound("Venue details not found.");
            }

  
            // Create a MemoryStream to hold the PDF content
            using (MemoryStream stream = new MemoryStream())
            {
                // Create a PdfWriter and PdfDocument
                using (PdfWriter writer = new PdfWriter(stream))
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    // Create a Document and add content
                    using (Document document = new Document(pdf))
                    {

                        string imagePath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "Images", "hrsign.PNG");
                        string imagePath1 = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "Images", "header.PNG");

                        // Create the image object
                        Image img = new Image(ImageDataFactory.Create(imagePath));
                        Image img1 = new Image(ImageDataFactory.Create(imagePath1));
                        img.SetWidth(80); // Set width of the image (optional)
                        img.SetHeight(40); // Set height of the image (optional)

                        // Add the image for ZIMSEC logo
                        document.Add(img1);


                        // Create the bold date text
                        Text dateText = new Text($"{DateTime.Now.ToString("dd MMM yyyy")}\n\n").SetFontSize(10); // Make the text bold

                        // Create a Paragraph with right alignment
                        Paragraph dateParagraph = new Paragraph(dateText)
                            .SetTextAlignment(TextAlignment.RIGHT).SetFontSize(10); // Align text to the right

                        // Add the date paragraph to the document at the top
                        document.Add(dateParagraph);

                        // Add address with line breaks
                        document.Add(new Paragraph($"{examiner.WorkAddress1?.ToUpper()}").SetFontSize(10));
                        document.Add(new Paragraph($"{examiner.WorkAddress2?.ToUpper()}").SetFontSize(10));
                        document.Add(new Paragraph($"{examiner.WorkAddress3?.ToUpper()}\n\n").SetFontSize(10));


                        // Create the salutation text
                        Text salutationText = new Text($"Dear {(examiner.Sex.ToLower() == "m" ? "Sir" : "Madam")}\n\n")
                            .SetBold().SetFontSize(10); // Make the text bold
                                                        // Add the salutation to the document
                        document.Add(new Paragraph(salutationText));

                        // Create a Div with a border
                        Div borderDiv = new Div()
                            .SetBorder(new SolidBorder(2)); // 2px solid border; // Add some padding inside the border

                        // Create the bold text
                        Text boldText = new Text("INVITATION TO TRAIN AS AN EXAMINER\n").SetFontSize(10)
                            .SetBold(); // Make the text bold

                        // Add the bold text to a Paragraph and then to the Div
                        borderDiv.Add(new Paragraph(boldText));

                        // Add the bordered Div to the document
                        document.Add(borderDiv);

                        document.Add(new Paragraph("1.0 I am pleased to inform you that you have been selected to train as an examiner.\n").SetFontSize(10));



                        // Create a new Text object for the bold part
                        Text subjectText = new Text(examiner.Subject).SetBold().SetFontSize(10);
                        Text paperCodeText = new Text($"  Paper Code: {examiner.PaperCode}").SetBold().SetFontSize(10);

                        // Combine them into one Paragraph
                        document.Add(new Paragraph("2.0 Subject: ").Add(subjectText).Add(paperCodeText).SetFontSize(10));



                        // Create Text objects for the dates and make them bold
                        Text startDateText = new Text($"{venueDetails.TrainingStartDate:dd-MMM-yyyy} to {venueDetails.TrainingEndDate:dd-MMM-yyyy}").SetBold().SetFontSize(10);

                        // Create the Paragraph and add the non-bold text followed by the bold text
                        document.Add(new Paragraph($"2.1 Training Dates: ").Add(startDateText).SetFontSize(10));


                        // Create Text objects for the check-in and check-out dates and make them bold
                        Text checkInDateText = new Text($"{venueDetails.CheckInDate:dd-MMM-yyyy}").SetBold().SetFontSize(10);
                        Text checkOutDateText = new Text($"{venueDetails.CheckOutDate:dd-MMM-yyyy}").SetBold().SetFontSize(10);

                        // Create the Paragraph and add the regular text followed by the bold dates
                        document.Add(new Paragraph($"2.2 Check in date: ").Add(checkInDateText)
                            .Add(" Check out date ").Add(checkOutDateText).SetFontSize(10));



                        // Create Text objects for the start and end dates and make them bold
                        Text trainingStartDateText = new Text($"{venueDetails.TrainingTime}").SetBold().SetFontSize(10);


                        // Create the Paragraph and add the regular text followed by the bold dates
                        document.Add(new Paragraph($"2.3 Training times: ").Add(trainingStartDateText).SetFontSize(10));


                        // Create a Text object for the VenueName and make it bold
                        Text venueNameText = new Text($"{venueDetails.VenueName}").SetBold().SetFontSize(10);

                        // Create the Paragraph and add the regular text followed by the bold VenueName
                        document.Add(new Paragraph($"2.4 Training Venue: ").Add(venueNameText).SetFontSize(10));


                        document.Add(new Paragraph("3.0  ZIMSEC  will provide bed, breakfast, lunch and dinner for the duration of your stay.\n").SetFontSize(10));
                        document.Add(new Paragraph("4.0  Please ensure that you have enough bus fare to travel to and from your station. ZIMSEC will not provide bus fare \n").SetFontSize(10));
                        Text instructionText = new Text($"5.0 Please fill in the tear-off slip below and send it back to ZIMSEC by {venueDetails.TrainingStartDate:dd MMM yyyy}\n")
                        .SetBold()
                        .SetFontSize(10);

                        document.Add(new Paragraph(instructionText).SetFontSize(10));
                        document.Add(new Paragraph("Yours Faithfully").SetFontSize(10));

                        //signature
                        document.Add(img);
                        // Adding the name and title in bold
                        document.Add(new Paragraph("S.MALAMBA\n").SetBold().SetFontSize(10));
                        document.Add(new Paragraph("DIRECTOR - HUMAN CAPITAL\n\n").SetBold().SetFontSize(10));

                        // Adding the title of the organization in bold
                        document.Add(new Paragraph("FOR DIRECTOR - ZIMBABWE SCHOOL EXAMINATIONS COUNCIL\n").SetBold().SetFontSize(10));
                        document.Add(new Paragraph("---------------------------------------------------------------------------------------------------------------------------------\n").SetBold());

                        // Adding the subject and paper code in bold
                        document.Add(new Paragraph($"SUBJECT / CODE: {examiner.Subject} {examiner.PaperCode}\n\n").SetBold().SetFontSize(10));


                        document.Add(new Paragraph("I __________________________________* accept/do not accept the invitation on all conditions outlined in the letter. (*delete inapplicable) \n").SetFontSize(10));

                        document.Add(new Paragraph("DATE____________________________________SIGNATURE___________________________").SetFontSize(8));


                    } // Document is closed here, but the stream remains open
                } // PdfDocument and PdfWriter are closed here, but the stream remains open

                // Reset the stream position to the beginning before returning
                //stream.Position = 0;

                // Return the PDF as a file
                return File(stream.ToArray(), "application/pdf", $"Examiner_Invitation_{examiner.ExaminerName}_{examiner.LastName}_{examiner.Subject}{examiner.PaperCode}.pdf");
            } // MemoryStream is disposed here


        }


        [HttpGet]
        public async Task<IActionResult> GenerateBulkInvitationLetters(
          string? Experience,
          string? Subject,
          string? PaperCode,
          string? Region)
        {


           string  region  = !string.IsNullOrEmpty(Region) ? Region : HttpContext.Session.GetString("Region");
           string  subject = !string.IsNullOrEmpty(Subject) ? Subject : HttpContext.Session.GetString("Subject");
           string  paperCode = !string.IsNullOrEmpty(PaperCode) ? PaperCode : HttpContext.Session.GetString("PaperCode");
           string  experience = !string.IsNullOrEmpty(Experience) ? Experience : HttpContext.Session.GetString("Experience");


            var request = await _traineeSelectionRepository.GetIdsForBulkDownload(
                Subject: subject,
                Experience: experience,
                PaperCode: paperCode,
                Region: region
            );


            if (request == null || !request.Any())
            {
                return Json(new { success = false, message = "No Trainee Examiners found for generating invitations." });
            }

            try
            {
                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var id in request)
                        {
                            var examiner = await _recruitmentRepository.GetByIdAsync(id);
                            if (examiner == null)
                            {
                                _logger.LogWarning("Examiner with ID {Id} not found.", id);
                                continue;
                            }

                            var venueDetailIdString = HttpContext.Session.GetString("ExaminerRecruitsVenue");
                            if (!int.TryParse(venueDetailIdString, out int venueDetailId))
                            {
                                _logger.LogWarning("Invalid venue ID in session.");
                                continue;
                            }

                            var venueDetails = await _venueDetailsRepository.GetByIdAsync(venueDetailId);
                            if (venueDetails == null)
                            {
                                _logger.LogWarning("Venue details for VenueID {VenueId} not found.", venueDetailId);
                                continue;
                            }

                            // Create PDF content and store in pdfStream
                            MemoryStream pdfStream = new MemoryStream();

                            // Use a separate block for PdfWriter and PdfDocument to ensure they don't close the stream
                            using (PdfWriter writer = new PdfWriter(pdfStream))
                            {
                                writer.SetCloseStream(false); // Ensure PdfWriter does not close the stream
                                using (PdfDocument pdf = new PdfDocument(writer))
                                using (Document document = new Document(pdf))
                                {

                                    string imagePath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "Images", "hrsign.PNG");
                                    string imagePath1 = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "Images", "header.PNG");

                                    // Create the image object
                                    Image img = new Image(ImageDataFactory.Create(imagePath));
                                    Image img1 = new Image(ImageDataFactory.Create(imagePath1));
                                    img.SetWidth(80); // Set width of the image (optional)
                                    img.SetHeight(40); // Set height of the image (optional)

                                    // Add the image for ZIMSEC logo
                                    document.Add(img1);


                                    // Create the bold date text
                                    Text dateText = new Text($"{DateTime.Now.ToString("dd MMM yyyy")}\n\n").SetFontSize(10); // Make the text bold

                                    // Create a Paragraph with right alignment
                                    Paragraph dateParagraph = new Paragraph(dateText)
                                        .SetTextAlignment(TextAlignment.RIGHT).SetFontSize(10); // Align text to the right

                                    // Add the date paragraph to the document at the top
                                    document.Add(dateParagraph);

                                    // Add address with line breaks
                                    document.Add(new Paragraph($"{examiner.WorkAddress1?.ToUpper()}").SetFontSize(10));
                                    document.Add(new Paragraph($"{examiner.WorkAddress2?.ToUpper()}").SetFontSize(10));
                                    document.Add(new Paragraph($"{examiner.WorkAddress3?.ToUpper()}\n\n").SetFontSize(10));


                                    // Create the salutation text
                                    Text salutationText = new Text($"Dear {(examiner.Sex.ToLower() == "m" ? "Sir" : "Madam")}\n\n")
                                        .SetBold().SetFontSize(10); // Make the text bold
                                                                    // Add the salutation to the document
                                    document.Add(new Paragraph(salutationText));

                                    // Create a Div with a border
                                    Div borderDiv = new Div()
                                        .SetBorder(new SolidBorder(2)); // 2px solid border; // Add some padding inside the border

                                    // Create the bold text
                                    Text boldText = new Text("INVITATION TO TRAIN AS AN EXAMINER\n").SetFontSize(10)
                                        .SetBold(); // Make the text bold

                                    // Add the bold text to a Paragraph and then to the Div
                                    borderDiv.Add(new Paragraph(boldText));

                                    // Add the bordered Div to the document
                                    document.Add(borderDiv);

                                    document.Add(new Paragraph("1.0 I am pleased to inform you that you have been selected to train as an examiner.\n").SetFontSize(10));



                                    // Create a new Text object for the bold part
                                    Text subjectText = new Text(examiner.Subject).SetBold().SetFontSize(10);
                                    Text paperCodeText = new Text($"  Paper Code: {examiner.PaperCode}").SetBold().SetFontSize(10);

                                    // Combine them into one Paragraph
                                    document.Add(new Paragraph("2.0 Subject: ").Add(subjectText).Add(paperCodeText).SetFontSize(10));



                                    // Create Text objects for the dates and make them bold
                                    Text startDateText = new Text($"{venueDetails.TrainingStartDate:dd-MMM-yyyy} to {venueDetails.TrainingEndDate:dd-MMM-yyyy}").SetBold().SetFontSize(10);

                                    // Create the Paragraph and add the non-bold text followed by the bold text
                                    document.Add(new Paragraph($"2.1 Training Dates: ").Add(startDateText).SetFontSize(10));


                                    // Create Text objects for the check-in and check-out dates and make them bold
                                    Text checkInDateText = new Text($"{venueDetails.CheckInDate:dd-MMM-yyyy}").SetBold().SetFontSize(10);
                                    Text checkOutDateText = new Text($"{venueDetails.CheckOutDate:dd-MMM-yyyy}").SetBold().SetFontSize(10);

                                    // Create the Paragraph and add the regular text followed by the bold dates
                                    document.Add(new Paragraph($"2.2 Check in date: ").Add(checkInDateText)
                                        .Add(" Check out date ").Add(checkOutDateText).SetFontSize(10));



                                    // Create Text objects for the start and end dates and make them bold
                                    Text trainingStartDateText = new Text($"{venueDetails.TrainingTime}").SetBold().SetFontSize(10);


                                    // Create the Paragraph and add the regular text followed by the bold dates
                                    document.Add(new Paragraph($"2.3 Training times: ").Add(trainingStartDateText).SetFontSize(10));


                                    // Create a Text object for the VenueName and make it bold
                                    Text venueNameText = new Text($"{venueDetails.VenueName}").SetBold().SetFontSize(10);

                                    // Create the Paragraph and add the regular text followed by the bold VenueName
                                    document.Add(new Paragraph($"2.4 Training Venue: ").Add(venueNameText).SetFontSize(10));


                                    document.Add(new Paragraph("3.0  ZIMSEC  will provide bed, breakfast, lunch and dinner for the duration of your stay.\n").SetFontSize(10));
                                    document.Add(new Paragraph("4.0  Please ensure that you have enough bus fare to travel to and from your station. ZIMSEC will not provide bus fare \n").SetFontSize(10));
                                    Text instructionText = new Text($"5.0 Please fill in the tear-off slip below and send it back to ZIMSEC by {venueDetails.TrainingStartDate:dd MMM yyyy}\n")
                                    .SetBold()
                                    .SetFontSize(10);

                                    document.Add(new Paragraph(instructionText).SetFontSize(10));
                                    document.Add(new Paragraph("Yours Faithfully").SetFontSize(10));

                                    //signature
                                    document.Add(img);
                                    // Adding the name and title in bold
                                    document.Add(new Paragraph("S.MALAMBA\n").SetBold().SetFontSize(10));
                                    document.Add(new Paragraph("DIRECTOR - HUMAN CAPITAL\n\n").SetBold().SetFontSize(10));

                                    // Adding the title of the organization in bold
                                    document.Add(new Paragraph("FOR DIRECTOR - ZIMBABWE SCHOOL EXAMINATIONS COUNCIL\n").SetBold().SetFontSize(10));
                                    document.Add(new Paragraph("---------------------------------------------------------------------------------------------------------------------------------\n").SetBold());

                                    // Adding the subject and paper code in bold
                                    document.Add(new Paragraph($"SUBJECT / CODE: {examiner.Subject} {examiner.PaperCode}\n\n").SetBold().SetFontSize(10));


                                    document.Add(new Paragraph("I __________________________________* accept/do not accept the invitation on all conditions outlined in the letter. (*delete inapplicable) \n").SetFontSize(10));

                                    document.Add(new Paragraph("DATE____________________________________SIGNATURE___________________________").SetFontSize(8));

                                }
                            }

                            // Reset the position of pdfStream to the beginning before copying
                            pdfStream.Position = 0;

                            // Create a new entry in the ZIP file for this PDF
                            var zipEntry = archive.CreateEntry($"Invitation_{examiner.ExaminerName}_{examiner.LastName}_{examiner.Subject}{examiner.PaperCode}.pdf");

                            // Copy the PDF stream into the ZIP archive entry
                            using (var entryStream = zipEntry.Open())
                            {
                                await pdfStream.CopyToAsync(entryStream);
                            }

                            // Dispose pdfStream manually after copying its content
                            pdfStream.Dispose();
                        }
                    }

                    // Reset position of zipStream before returning the ZIP file
                    zipStream.Position = 0;
                    // Generate the formatted date separately
                    var formattedDate = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");

                    // Concatenate the date with the filename
                    var fileName = $"Examiner_Invitations_{formattedDate}.zip";

                    // Return the file
                    return File(zipStream.ToArray(), "application/zip", fileName);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating bulk invitations.");
                return Json(new { success = false, message = $"An error occurred while generating invitations. Details: {ex.Message}" });
            }
        }


        public async Task<IActionResult> InviteByEmail(int examinerRecruitmentId)
        {

            try
            {

                var examinerEmailInvite = await _emailInvitationRepository.GetByExaminerRecruitmentIdAsync(examinerRecruitmentId);
                if (examinerEmailInvite != null) // Check if data exists
                {
                    return Json(new { success = false, message = "Examiner already invited." }); // Error message
                }

            // Continue with the process if examinerEmailInvite is null (does not exist)
            var examiner = await _recruitmentRepository.GetByIdAsync(examinerRecruitmentId);
            if (examiner == null)
            {
                return Json(new { success = false, message = "Examiner not found." });
            }

            var venueDetailIdString = HttpContext.Session.GetString("ExaminerRecruitsVenue");
            if (!int.TryParse(venueDetailIdString, out int venueDetailId))
            {
                return Json(new { success = false, message = "Invalid Venue ID." });
            }

            var venueDetails = await _venueDetailsRepository.GetByIdAsync(venueDetailId);
            if (venueDetails == null)
            {
                return Json(new { success = false, message = "Venue details not found." });
            }

            byte[] pdfBytes;

            // Generate PDF in memory
            using (MemoryStream stream = new MemoryStream())
            {
                using (PdfWriter writer = new PdfWriter(stream))
                using (PdfDocument pdf = new PdfDocument(writer))
                using (Document document = new Document(pdf))
                {
                    string imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "hrsign.PNG");
                    string imagePath1 = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "header.PNG");

                    Image img = new Image(ImageDataFactory.Create(imagePath)).SetWidth(80).SetHeight(40);
                    Image img1 = new Image(ImageDataFactory.Create(imagePath1));

                    document.Add(img1);
                    document.Add(new Paragraph($"{DateTime.Now:dd MMM yyyy}").SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetFontSize(10));
                    document.Add(new Paragraph($"{examiner.WorkAddress1?.ToUpper()}").SetFontSize(10));
                    document.Add(new Paragraph($"{examiner.WorkAddress2?.ToUpper()}").SetFontSize(10));
                    document.Add(new Paragraph($"{examiner.WorkAddress3?.ToUpper()}\n\n").SetFontSize(10));
                    document.Add(new Paragraph($"Dear {(examiner.Sex.ToLower() == "m" ? "Sir" : "Madam")}\n\n").SetBold().SetFontSize(10));

                    Div borderDiv = new Div().SetBorder(new SolidBorder(2));
                    borderDiv.Add(new Paragraph("INVITATION TO TRAIN AS AN EXAMINER\n").SetFontSize(10).SetBold());
                    document.Add(borderDiv);
                    document.Add(new Paragraph("1.0 I am pleased to inform you that you have been selected to train as an examiner.\n").SetFontSize(10));

                    document.Add(new Paragraph("2.0 Subject: ")
                        .Add(new Text(examiner.Subject).SetBold().SetFontSize(10))
                        .Add(new Text($"  Paper Code: {examiner.PaperCode}").SetBold().SetFontSize(10)));

                    document.Add(new Paragraph("2.1 Training Dates: ")
                        .Add(new Text($"{venueDetails.TrainingStartDate:dd-MMM-yyyy} to {venueDetails.TrainingEndDate:dd-MMM-yyyy}").SetBold().SetFontSize(10)));

                    document.Add(new Paragraph("2.2 Check in date: ")
                        .Add(new Text($"{venueDetails.CheckInDate:dd-MMM-yyyy}").SetBold().SetFontSize(10))
                        .Add(" Check out date ")
                        .Add(new Text($"{venueDetails.CheckOutDate:dd-MMM-yyyy}").SetBold().SetFontSize(10)));

                    document.Add(new Paragraph("2.3 Training times: ")
                        .Add(new Text($"{venueDetails.TrainingTime}").SetBold().SetFontSize(10)));

                    document.Add(new Paragraph("2.4 Training Venue: ")
                        .Add(new Text($"{venueDetails.VenueName}").SetBold().SetFontSize(10)));

                    document.Add(new Paragraph("3.0  ZIMSEC will provide bed, breakfast, lunch and dinner for the duration of your stay.\n").SetFontSize(10));
                    document.Add(new Paragraph("4.0  Please ensure that you have enough bus fare to travel to and from your station. ZIMSEC will not provide bus fare \n").SetFontSize(10));

                    document.Add(new Paragraph("5.0 Please fill in the tear-off slip below and send it back to ZIMSEC by ")
                        .Add(new Text($"{venueDetails.TrainingStartDate:dd MMM yyyy}").SetBold().SetFontSize(10)));

                    document.Add(new Paragraph("Yours Faithfully").SetFontSize(10));
                    document.Add(img);
                    document.Add(new Paragraph("S.MALAMBA\n").SetBold().SetFontSize(10));
                    document.Add(new Paragraph("DIRECTOR - HUMAN CAPITAL\n\n").SetBold().SetFontSize(10));
                    document.Add(new Paragraph("FOR DIRECTOR - ZIMBABWE SCHOOL EXAMINATIONS COUNCIL\n").SetBold().SetFontSize(10));
                    document.Add(new Paragraph("---------------------------------------------------------------------------------------------------------------------------------\n").SetBold());
                    document.Add(new Paragraph($"SUBJECT / CODE: {examiner.Subject} {examiner.PaperCode}\n\n").SetBold().SetFontSize(10));
                    document.Add(new Paragraph("I __________________________________* accept/do not accept the invitation on all conditions outlined in the letter. (*delete inapplicable) \n").SetFontSize(10));
                    document.Add(new Paragraph("DATE____________________________________SIGNATURE___________________________").SetFontSize(8));
                }

                pdfBytes = stream.ToArray();
            }

        
                // Send Email with Attachment using MailerSend SMTP
                await SendEmailWithAttachment(examiner.EmailAddress, pdfBytes, $"Examiner_Invitation_{examiner.ExaminerName}_{examiner.LastName}_{examiner.Subject}{examiner.PaperCode}.pdf");

                // Get the current user ID (may be null if user is not authenticated)
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Create the invitation object, allowing InvitedBy to be null
                ExaminerRecruitmentEmailInvitation examinerRecruitmentEmailInvitation = new ExaminerRecruitmentEmailInvitation
                {
                    ExaminerRecruitmentId = examinerRecruitmentId,
                    InvitedBy = currentUserId,  // Will be null if the user is not authenticated
                    DateInvited = DateTime.UtcNow // Set the current date as the invitation date
                };

                // Add the invitation to the repository
                await _emailInvitationRepository.AddAsync(examinerRecruitmentEmailInvitation);


                return Json(new { success = true, message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                // If an error occurs, set the error message in TempData
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }


        }

        private async Task SendEmailWithAttachment(string recipientEmail, byte[] pdfBytes, string fileName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ekasu", "info@trial-68zxl27rvo94j905.mlsender.net"));
            message.To.Add(new MailboxAddress("Recipient Name", recipientEmail));
            message.Subject = "Invitation to Train as a ZIMSEC Examiner";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = "Please find attached your invitation letter for ZIMSEC examiner training.",
            };

            using (var memoryStream = new MemoryStream(pdfBytes))
            {
                bodyBuilder.Attachments.Add(fileName, memoryStream, MimeKit.ContentType.Parse("application/pdf"));
            }

            message.Body = bodyBuilder.ToMessageBody();

            // Use MailKit to send the email through MailerSend SMTP
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.mailersend.net", 587, SecureSocketOptions.StartTls); // MailerSend SMTP server
                await client.AuthenticateAsync("MS_wTYHBh@trial-68zxl27rvo94j905.mlsender.net", "mssp.JvIPtda.o65qngkvnq8lwr12.OyBVOZf"); // MailerSend SMTP username and password
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }





        public async Task<IActionResult> InviteBulkByEmail(
         string? Experience,
         string? Subject,
         string? PaperCode,
         string? Region)
        {
            string region = !string.IsNullOrEmpty(Region) ? Region : HttpContext.Session.GetString("Region");
            string subject = !string.IsNullOrEmpty(Subject) ? Subject : HttpContext.Session.GetString("Subject");
            string paperCode = !string.IsNullOrEmpty(PaperCode) ? PaperCode : HttpContext.Session.GetString("PaperCode");
            string experience = !string.IsNullOrEmpty(Experience) ? Experience : HttpContext.Session.GetString("Experience");

            var request = await _traineeSelectionRepository.GetIdsForBulkDownload(
                Subject: subject,
                Experience: experience,
                PaperCode: paperCode,
                Region: region
            );

            if (request == null || !request.Any())
            {
                return Json(new { success = true, message = "No IDs Users provided.!" });
            }

            try
            {
                int skippedCount = 0;
                int processedCount = 0;

                // Optimize: Check for existing invitations in bulk
                var existingIds = await _emailInvitedContext.ExaminerRecruitmentEmailInvitations
                    .Where(i => request.Contains(i.ExaminerRecruitmentId))
                    .Select(i => i.ExaminerRecruitmentId)
                    .ToListAsync();

                foreach (var id in request)
                {
                    // Check if an invitation record already exists for this ExaminerRecruitmentId
                    if (existingIds.Contains(id))
                    {
                        _logger.LogInformation("Skipping invitation for ExaminerRecruitmentId {Id} as it already exists.", id);
                        skippedCount++;
                        continue;
                    }

                    var examiner = await _recruitmentRepository.GetByIdAsync(id);
                    if (examiner == null)
                    {
                        _logger.LogWarning("Examiner with ID {Id} not found.", id);
                        continue;
                    }

                    var venueDetailIdString = HttpContext.Session.GetString("ExaminerRecruitsVenue");
                    if (!int.TryParse(venueDetailIdString, out int venueDetailId))
                    {
                        _logger.LogWarning("Invalid venue ID in session.");
                        continue;
                    }

                    var venueDetails = await _venueDetailsRepository.GetByIdAsync(venueDetailId);
                    if (venueDetails == null)
                    {
                        _logger.LogWarning("Venue details for VenueID {VenueId} not found.", venueDetailId);
                        continue;
                    }

                    // Validate email address
                    if (string.IsNullOrEmpty(examiner.EmailAddress))
                    {
                        _logger.LogWarning("Email address missing for Examiner ID {Id}.", id);
                        continue;
                    }

                    // Create PDF content
                    using var pdfStream = new MemoryStream();
                    using (var writer = new PdfWriter(pdfStream))
                    using (var pdf = new PdfDocument(writer))
                    using (var document = new Document(pdf))
                    {
                        string imagePath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "Images", "hrsign.PNG");
                        string imagePath1 = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "Images", "header.PNG");

                        Image img = new Image(ImageDataFactory.Create(imagePath));
                        Image img1 = new Image(ImageDataFactory.Create(imagePath1));
                        img.SetWidth(80);
                        img.SetHeight(40);

                        document.Add(img1);

                        Text dateText = new Text($"{DateTime.Now.ToString("dd MMM yyyy")}\n\n").SetFontSize(10);
                        Paragraph dateParagraph = new Paragraph(dateText).SetTextAlignment(TextAlignment.RIGHT).SetFontSize(10);
                        document.Add(dateParagraph);

                        document.Add(new Paragraph($"{examiner.WorkAddress1?.ToUpper()}").SetFontSize(10));
                        document.Add(new Paragraph($"{examiner.WorkAddress2?.ToUpper()}").SetFontSize(10));
                        document.Add(new Paragraph($"{examiner.WorkAddress3?.ToUpper()}\n\n").SetFontSize(10));

                        Text salutationText = new Text($"Dear {(examiner.Sex.ToLower() == "m" ? "Sir" : "Madam")}\n\n")
                            .SetBold().SetFontSize(10);
                        document.Add(new Paragraph(salutationText));

                        Div borderDiv = new Div().SetBorder(new SolidBorder(2));
                        Text boldText = new Text("INVITATION TO TRAIN AS AN EXAMINER\n").SetFontSize(10).SetBold();
                        borderDiv.Add(new Paragraph(boldText));
                        document.Add(borderDiv);

                        document.Add(new Paragraph("1.0 I am pleased to inform you that you have been selected to train as an examiner.\n").SetFontSize(10));

                        Text subjectText = new Text(examiner.Subject).SetBold().SetFontSize(10);
                        Text paperCodeText = new Text($"  Paper Code: {examiner.PaperCode}").SetBold().SetFontSize(10);
                        document.Add(new Paragraph("2.0 Subject: ").Add(subjectText).Add(paperCodeText).SetFontSize(10));

                        Text startDateText = new Text($"{venueDetails.TrainingStartDate:dd-MMM-yyyy} to {venueDetails.TrainingEndDate:dd-MMM-yyyy}").SetBold().SetFontSize(10);
                        document.Add(new Paragraph($"2.1 Training Dates: ").Add(startDateText).SetFontSize(10));

                        Text checkInDateText = new Text($"{venueDetails.CheckInDate:dd-MMM-yyyy}").SetBold().SetFontSize(10);
                        Text checkOutDateText = new Text($"{venueDetails.CheckOutDate:dd-MMM-yyyy}").SetBold().SetFontSize(10);
                        document.Add(new Paragraph($"2.2 Check in date: ").Add(checkInDateText)
                            .Add(" Check out date ").Add(checkOutDateText).SetFontSize(10));

                        Text trainingStartDateText = new Text($"{venueDetails.TrainingTime}").SetBold().SetFontSize(10);
                        document.Add(new Paragraph($"2.3 Training times: ").Add(trainingStartDateText).SetFontSize(10));

                        Text venueNameText = new Text($"{venueDetails.VenueName}").SetBold().SetFontSize(10);
                        document.Add(new Paragraph($"2.4 Training Venue: ").Add(venueNameText).SetFontSize(10));

                        document.Add(new Paragraph("3.0 ZIMSEC will provide bed, breakfast, lunch and dinner for the duration of your stay.\n").SetFontSize(10));
                        document.Add(new Paragraph("4.0 Please ensure that you have enough bus fare to travel to and from your station. ZIMSEC will not provide bus fare \n").SetFontSize(10));
                        Text instructionText = new Text($"5.0 Please fill in the tear-off slip below and send it back to ZIMSEC by {venueDetails.TrainingStartDate:dd MMM yyyy}\n")
                            .SetBold().SetFontSize(10);
                        document.Add(new Paragraph(instructionText).SetFontSize(10));
                        document.Add(new Paragraph("Yours Faithfully").SetFontSize(10));

                        document.Add(img);
                        document.Add(new Paragraph("S.MALAMBA\n").SetBold().SetFontSize(10));
                        document.Add(new Paragraph("DIRECTOR - HUMAN CAPITAL\n\n").SetBold().SetFontSize(10));
                        document.Add(new Paragraph("FOR DIRECTOR - ZIMBABWE SCHOOL EXAMINATIONS COUNCIL\n").SetBold().SetFontSize(10));
                        document.Add(new Paragraph("---------------------------------------------------------------------------------------------------------------------------------\n").SetBold());
                        document.Add(new Paragraph($"SUBJECT / CODE: {examiner.Subject} {examiner.PaperCode}\n\n").SetBold().SetFontSize(10));
                        document.Add(new Paragraph("I __________________________________* accept/do not accept the invitation on all conditions outlined in the letter. (*delete inapplicable) \n").SetFontSize(10));
                        document.Add(new Paragraph("DATE____________________________________SIGNATURE___________________________").SetFontSize(8));
                    }

                    byte[] pdfBytes = pdfStream.ToArray();
                    string fileName = $"Invitation_{examiner.ExaminerName}_{examiner.LastName}_{examiner.Subject}{examiner.PaperCode}.pdf";

                    // Send the email
                    await SendEmailWithAttachment(examiner.EmailAddress, pdfBytes, fileName);

                    // Record the invitation in the database after successful email send
                    ExaminerRecruitmentEmailInvitation examinerRecruitmentEmailInvitation = new ExaminerRecruitmentEmailInvitation
                    {
                        ExaminerRecruitmentId = id,
                        InvitedBy = HttpContext.User.Identity.IsAuthenticated ? HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null,
                        DateInvited = DateTime.UtcNow
                    };

                    await _emailInvitationRepository.AddAsync(examinerRecruitmentEmailInvitation);
                    processedCount++;
                }

                string message = $"Bulk emails sent successfully! Processed: {processedCount}, Skipped (already invited): {skippedCount}";
                return Json(new { success = true, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating and sending bulk invitations.");
                return StatusCode(500, $"An error occurred while sending bulk invitations. Details: {ex.Message}");
            }
        }


    }


}

