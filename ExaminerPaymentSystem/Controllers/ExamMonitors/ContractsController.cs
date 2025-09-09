
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Services.ExamMonitors;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;



// Services/PdfContractService.cs
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO.Compression;
using Text = iText.Layout.Element.Text;

public class PdfContractService
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    public PdfContractService(IWebHostEnvironment hostingEnvironment)
    {
       _hostingEnvironment = hostingEnvironment;
    }

    public byte[] GenerateClusterManagerContract(ContractData data)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var pdfWriter = new PdfWriter(memoryStream))
            {
                using (var pdfDocument = new PdfDocument(pdfWriter))
                {
                    var document = new Document(pdfDocument);

                    // Add ZIMSEC Header with contract type
                    AddZimsecHeader(document, data, "ClusterManager");

                    // Add Contract Content
                    AddManagerContractContent(document, data);

                    // Add Signature Section
                    AddSignatureSection(document, data);

                    document.Close();
                }
            }
            return memoryStream.ToArray();
        }
    }

    public byte[] GenerateAssistantClusterManagerContract(ContractData data)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var pdfWriter = new PdfWriter(memoryStream))
            {
                using (var pdfDocument = new PdfDocument(pdfWriter))
                {
                    var document = new Document(pdfDocument);

                    // Add ZIMSEC Header with contract type
                    AddZimsecHeader(document, data, "AssistantClusterManager");

                    // Add Contract Content
                    AddAssistantContractContent(document, data);

                    // Add Signature Section
                    AddSignatureSection(document, data);

                    document.Close();
                }
            }
            return memoryStream.ToArray();
        }
    }

    public byte[] GenerateResidentMonitorContract(ContractData data)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var pdfWriter = new PdfWriter(memoryStream))
            {
                using (var pdfDocument = new PdfDocument(pdfWriter))
                {
                    var document = new Document(pdfDocument);

                    // Add ZIMSEC Header with contract type
                    AddZimsecHeader(document, data, "ResidentMonitor");

                    // Add Contract Content
                    AddResidentMonitorContractContent(document, data);

                    // Add Signature Section
                    AddSignatureSection(document, data);

                    document.Close();
                }
            }
            return memoryStream.ToArray();
        }
    }

    private void AddZimsecHeader(Document document, ContractData data, string contractType)
    {
        // Create a table with 3 columns for the header layout
        iText.Layout.Element.Table headerTable = new iText.Layout.Element.Table(new float[] { 2, 3, 2 });
        headerTable.SetWidth(UnitValue.CreatePercentValue(100));

        // Left cell - Logo
        iText.Layout.Element.Cell logoCell = new iText.Layout.Element.Cell();
        logoCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
        logoCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);

        string imagePath1 = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "Images", "logo.PNG");
        Image img1 = new Image(ImageDataFactory.Create(imagePath1));
        img1.SetMaxWidth(100); // Adjust as needed
        logoCell.Add(img1);
        headerTable.AddCell(logoCell);

        // Center cell - Address information
        iText.Layout.Element.Cell addressCell = new iText.Layout.Element.Cell();
        addressCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
        addressCell.SetTextAlignment(TextAlignment.CENTER);
        addressCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);

        addressCell.Add(new Paragraph("Zimbabwe School Examinations Council")
            .SetBold()
            .SetFontSize(12));
        addressCell.Add(new Paragraph("Examinations Centre, Upper East Road, Mount Pleasant")
            .SetFontSize(10));
        addressCell.Add(new Paragraph("P.O. Box CY 1464, Causeway, Harare, Zimbabwe")
            .SetFontSize(10));
      

        headerTable.AddCell(addressCell);

        // Right cell - Contact information
        iText.Layout.Element.Cell contactCell = new iText.Layout.Element.Cell();
        contactCell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
        contactCell.SetTextAlignment(TextAlignment.RIGHT);
        contactCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);
        contactCell.Add(new Paragraph("All communications should be addressed to:")
          .SetFontSize(9));
        contactCell.Add(new Paragraph("The Director, Zimbabwe School Examinations Council")
          .SetFontSize(9));
        contactCell.Add(new Paragraph("Telephone: +263 (0)242302623/4, 304552")
            .SetFontSize(9));
        contactCell.Add(new Paragraph("Telegraphic address: \"ZIMSEC\"")
            .SetFontSize(9));
        contactCell.Add(new Paragraph("Facsimile: +263 (0)242 302288; 339080; 333889")
            .SetFontSize(9));
    


        headerTable.AddCell(contactCell);

        // Add the header table to the document
        document.Add(headerTable);

        // Add the date and other elements
        string currentDate = DateTime.Now.ToString("d MMMM yyyy", new CultureInfo("en-US"));
        document.Add(new Paragraph(currentDate).SetTextAlignment(TextAlignment.LEFT));

        document.Add(new Paragraph($"Dear {data.FullName}")
            .SetMarginTop(20)
            .SetTextAlignment(TextAlignment.LEFT));

        document.Add(new Paragraph("CONTRACT OF EMPLOYMENT")
            .SetTextAlignment(TextAlignment.LEFT)
            .SetBold()
            .SetFontSize(14));

        string contractTitle = contractType switch
        {
            "ClusterManager" => $"ZIMSEC CLUSTER MANAGER ({data.Session}) {DateTime.Now.Year} EXAMINATIONS",
            "AssistantClusterManager" => $"ZIMSEC ASSISTANT CLUSTER MANAGER ({data.Session}) {DateTime.Now.Year} EXAMINATIONS",
            "ResidentMonitor" => $"ZIMSEC RESIDENT MONITOR ({data.Session}) {DateTime.Now.Year} EXAMINATIONS",
            _ => $"ZIMSEC EXAMINATIONS CONTRACT ({data.Session}) {DateTime.Now.Year}"
        };



        // Create a paragraph with border
        Paragraph borderedTitle = new Paragraph(contractTitle)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetBold()
            .SetFontSize(12)
            .SetBorder(new SolidBorder(1)) // Add border
            .SetPadding(6); // Add padding

        document.Add(borderedTitle);
    }

    private void AddManagerContractContent(Document document, ContractData data)
    {
        // 1. APPOINTMENT
        document.Add(new Paragraph("1. APPOINTMENT").SetBold().SetMarginTop(10));

        document.Add(new Paragraph("1.1 The Zimbabwe School Examinations Council (ZIMSEC) hereby commissions you to participate in the administration and supervision of national examinations as  Cluster Manager. Your role shall be to assist the Cluster Manager in executing the responsibilities outlined in this contract."));

        PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        Paragraph p = new Paragraph()
            .Add(new Text("1.2 The contract for the Cluster Manager shall commence on ").SetFont(normalFont))
            .Add(new Text(data.StartDate.ToString("dd MMMM yyyy", new CultureInfo("en-US"))).SetFont(boldFont))
            .Add(new Text(" and end on ").SetFont(normalFont))
            .Add(new Text(data.EndDate.ToString("dd MMMM yyyy", new CultureInfo("en-US"))).SetFont(boldFont))
            .Add(new Text(".").SetFont(normalFont));

        document.Add(p);

        document.Add(new Paragraph("1.3 You shall work under the guidance and supervision of the ZIMSEC Regional Manager, who shall provide instructions regarding your duties ."));

        document.Add(new Paragraph("1.4 The Council may require you to use your own vehicle or provide a Council-assigned vehicle for duty execution. If using a personal vehicle, ZIMSEC vehicle usage policies and reimbursement conditions shall apply."));

        // 2. DUTIES AND RESPONSIBILITIES
        document.Add(new Paragraph("2. DUTIES AND RESPONSIBILITIES").SetBold().SetMarginTop(10));
        document.Add(new Paragraph("You are expected to assist in the administration of examination activities within the cluster and perform the following duties:"));

        var duties = new List<string>
        {
            "Mantain a register of all Centres serviced by the cluster.",
            "Ensure that the Cluster Centre remains secure and that all examination materials are protected from tampering, damage, or loss.",
            "Receive and verify question papers for all centres within the cluster.",
            "Receive question paper verification reports from each Centre,ensuring accuracy, sufficient quantities and the integrity of the received envelopes.",
            "Retain the official question paper dispatch list from each Centre for proper script collection.",
            "Report any material shortages immediately to the ZIMSEC Regional Office.",
            "Manage and maintain the cluster question paper dispatch control sheet for each Centre.",
            "Oversee the question paper integrity control sheet for all centres.",
            "Operationalise the three-tier key holding arrangement throughout the examination period.",
            "Manage the script envelope control sheet to ensure proper usage and accountability.",
            "Log and account for all answer scripts as received from centres.",
            "Ensure adherence to script turn-in deadlines and immediately report any irregularities.",
            "Verify that all scripts correspond with their respective question paper dispatch lists for accuracy.",
            "Confirm that all scripts for registered subject components are submitted as required.",
            "Ensure that scripts are properly packed by subject before collection by ZIMSEC.",
            "Perform any other duties assigned by the ZIMSEC Regional Manager, ensuring full compliance with ZIMSEC policies and guidelines."
        };

        for (int i = 0; i < duties.Count; i++)
        {
            document.Add(new Paragraph($"{i + 1}. {duties[i]}"));
        }

        // 3. PAYMENTS AND ALLOWANCES
        document.Add(new Paragraph("3. PAYMENTS AND ALLOWANCES").SetBold().SetMarginTop(10));

        // Stipend paragraph with bold rate
        Paragraph stipendParagraph = new Paragraph()
            .Add(new Text("3.1 The Council shall pay a daily stipend of USD ").SetFont(normalFont))
            .Add(new Text(data.StipendRate.ToString()).SetFont(boldFont))
            .Add(new Text(" per working day for the duration of the contract. Additionally, you shall receive the following allowances:").SetFont(normalFont));

        document.Add(stipendParagraph);

        // Lunch allowance with bold rate
        Paragraph lunchParagraph = new Paragraph()
            .Add(new Text("Lunch: USD ").SetFont(normalFont))
            .Add(new Text(data.LunchRate.ToString()).SetFont(boldFont))
            .Add(new Text(" per day").SetFont(normalFont));

        document.Add(lunchParagraph);

        document.Add(new Paragraph("3.2 The Finance Department shall process advance payments equivalent to 50% of the total allowances at the start of the contract. The balance shall be paid upon completion of the assignment, subject to submission of a signed Regional attendance register, verifying full participation in daily duties."));

        document.Add(new Paragraph("3.3 Payment Method: All payments shall be processed through the bank, and bank details must be submitted for seamless transactions."));

        document.Add(new Paragraph("3.4 Work-Based Payment Rule: The Council shall only pay the agreed sum for actual workdays where the employee reports to work and performs assigned duties for the full day. Any partial work or unauthorized absenteeism may result in payment deductions."));

        document.Add(new Paragraph("3.5 Tax Deductions: ZIMSEC shall deduct applicable income tax in accordance with the provisions of the Finance (No. 2) Act, 2024 and any other tax regulations in effect."));

        // 4-6. Other sections...
        AddStandardContractSections(document);
    }

    private void AddAssistantContractContent(Document document, ContractData data)
    {
        // 1. APPOINTMENT
        document.Add(new Paragraph("1. APPOINTMENT").SetBold().SetMarginTop(10));

        document.Add(new Paragraph("1.1 The Zimbabwe School Examinations Council (ZIMSEC) hereby commissions you to participate in the administration and supervision of national examinations as Assistant Cluster Manager. Your role shall be to assist the Cluster Manager in executing the responsibilities outlined in this contract."));

        // Create fonts
        PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        // Build paragraph
        Paragraph p = new Paragraph()
            .Add(new Text("1.2 The contract for the Assistant Cluster Manager shall commence on ").SetFont(normalFont))
            .Add(new Text(data.StartDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)).SetFont(boldFont))
            .Add(new Text(" and end on ").SetFont(normalFont))
            .Add(new Text(data.EndDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)).SetFont(boldFont))
            .Add(new Text(".").SetFont(normalFont));

        // Add to document
        document.Add(p);




        document.Add(new Paragraph("1.3 You shall work under the guidance and supervision of the ZIMSEC Regional Manager through the Cluster Manager of the Centre to which you are assigned."));

        document.Add(new Paragraph("1.4 The Council may require you to use your own vehicle or provide a Council-assigned vehicle for duty execution. If using a personal vehicle, ZIMSEC vehicle usage policies and reimbursement conditions shall apply."));

        // 2. DUTIES AND RESPONSIBILITIES
        document.Add(new Paragraph("2. DUTIES AND RESPONSIBILITIES").SetBold().SetMarginTop(10));
        document.Add(new Paragraph("You are expected to assist in the administration of examination activities within the cluster and perform the following duties:"));

        var duties = new List<string>
        {
            "Assist in maintaining a register of all Centres serviced by the cluster.",
            "Ensure that the Cluster Centre remains secure and that all examination materials are protected from tampering, damage, or loss.",
            "Assist in receiving and verifying question papers for all centres within the cluster.",
            "Assist in receiving question paper verification reports from each Centre, confirming quantities and the integrity of the received envelopes.",
            "Assist in retaining the official question paper dispatch list from each Centre for proper script collection.",
            "Report any material shortages immediately to the ZIMSEC Regional Office.",
            "Assist in managing the cluster question paper dispatch control sheet for each Centre.",
            "Assist in overseeing the question paper integrity control sheet for all centres.",
            "Assist in operationalizing the three-tier key holding arrangement throughout the examination period.",
            "Manage the script envelope control sheet to ensure proper usage and accountability.",
            "Log and account for all answer scripts as received from centres.",
            "Ensure adherence to script turn-in deadlines and immediately report any irregularities.",
            "Verify that all scripts correspond with their respective question paper dispatch lists for accuracy.",
            "Confirm that all scripts for registered subject components are submitted as required.",
            "Ensure that scripts are properly packed by subject before collection by ZIMSEC.",
            "Perform any other duties assigned by the Cluster Manager, ensuring full compliance with ZIMSEC policies and guidelines."
        };

        for (int i = 0; i < duties.Count; i++)
        {
            document.Add(new Paragraph($"{i + 1}. {duties[i]}"));
        }

        // 3. PAYMENTS AND ALLOWANCES
        document.Add(new Paragraph("3. PAYMENTS AND ALLOWANCES").SetBold().SetMarginTop(10));

        // Stipend paragraph with bold rate
        Paragraph stipendParagraph = new Paragraph()
            .Add(new Text("3.1 The Council shall pay a daily stipend of USD ").SetFont(normalFont))
            .Add(new Text(data.StipendRate.ToString()).SetFont(boldFont))
            .Add(new Text(" per working day for the duration of the contract. Additionally, you shall receive the following allowances:").SetFont(normalFont));

        document.Add(stipendParagraph);

        // Lunch allowance with bold rate
        Paragraph lunchParagraph = new Paragraph()
            .Add(new Text("Lunch: USD ").SetFont(normalFont))
            .Add(new Text(data.LunchRate.ToString()).SetFont(boldFont))
            .Add(new Text(" per day").SetFont(normalFont));

        document.Add(lunchParagraph);

        document.Add(new Paragraph("3.2 The Finance Department shall process advance payments equivalent to 50% of the total allowances at the start of the contract. The balance shall be paid upon completion of the assignment, subject to submission of a signed Regional attendance register, verifying full participation in daily duties."));

        document.Add(new Paragraph("3.3 Payment Method: All payments shall be processed through the bank, and bank details must be submitted for seamless transactions."));

        document.Add(new Paragraph("3.4 Work-Based Payment Rule: The Council shall only pay the agreed sum for actual workdays where the employee reports to work and performs assigned duties for the full day. Any partial work or unauthorized absenteeism may result in payment deductions."));

        document.Add(new Paragraph("3.5 Tax Deductions: ZIMSEC shall deduct applicable income tax in accordance with the provisions of the Finance (No. 2) Act, 2024 and any other tax regulations in effect."));

        // 4-6. Other sections...
        AddStandardContractSections(document);
    }

    private void AddResidentMonitorContractContent(Document document, ContractData data)
    {
        // 1. APPOINTMENT
        document.Add(new Paragraph("1. APPOINTMENT").SetBold().SetMarginTop(10));

        document.Add(new Paragraph("1.1 The Zimbabwe School Examinations Council (ZIMSEC) hereby commissions you to participate in the administration and supervision of national examinations as Resident Monitor."));

        // Create fonts
        PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        // Build paragraph
        Paragraph p = new Paragraph()
            .Add(new Text($"1.2 The contract for the Resident Monitor shall have a duration of {data.DurationDays} days, including weekends, commencing on ").SetFont(normalFont))
            .Add(new Text(data.StartDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)).SetFont(boldFont))
            .Add(new Text(" and ending on ").SetFont(normalFont))
            .Add(new Text(data.EndDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)).SetFont(boldFont))
            .Add(new Text(".").SetFont(normalFont));

        // Add to document
        document.Add(p);

        document.Add(new Paragraph("1.3 You shall report to and work under the guidance and supervision of the ZIMSEC Regional Manager, who shall oversee issues relating to your duties."));

        document.Add(new Paragraph("1.4 The Council may require you to use your own vehicle or provide a Council-assigned vehicle for duty execution. If using a personal vehicle, ZIMSEC vehicle usage policies and reimbursement conditions shall apply."));

        // 2. DUTIES AND RESPONSIBILITIES
        document.Add(new Paragraph("2. DUTIES AND RESPONSIBILITIES").SetBold().SetMarginTop(10));
        document.Add(new Paragraph("The Resident Monitor is responsible for ensuring the integrity and security of the examination administration process and shall perform the following duties:"));

        var duties = new List<string>
    {
        "Inspect the cluster storage facility for compliance with security protocols and specifications.",
        "Maintain custody of the ZIMSEC key (3rd Key) as part of the three-tier key holding arrangement for access to the storage facility.",
        "Compile a list of Examination Committee members authorized to collect and deliver examination materials for each Centre serviced by the cluster.",
        "Monitor and supervise the delivery, verification, and storage of question papers and stationery received at the cluster Centre.",
        "Record any material shortages or surpluses identified during the question paper verification exercise.",
        "Witness the completion of question paper shortage request forms for submission to the Regional Manager.",
        "Monitor and verify the distribution of question papers, ensuring centres collect the correct papers to prevent premature opening.",
        "Monitor the use of script envelopes to prevent misuse or unauthorized distribution.",
        "Oversee the activities of supervisors and invigilators, ensuring compliance with regulations governing the conduct of examinations.",
        "Cross-check with the examinations timetable to ensure all centres submit scripts for scheduled and written components.",
        "Record and report any cases of malpractice occurring at the Centre and immediately alert the ZIMSEC Regional Manager.",
        "Maintain strict confidentiality and diligence in handling examination materials and enforcing correct processes and procedures.",
        "Monitor transactions and processes taking place at every stage within the cluster.",
        "Prepare and submit a detailed Resident Monitor report at the end of the examination program.",
        "Perform any other duties delegated by the Regional Manager, ensuring full compliance with ZIMSEC policies and guidelines."
    };

        for (int i = 0; i < duties.Count; i++)
        {
            document.Add(new Paragraph($"{i + 1}. {duties[i]}"));
        }

        // 3. PAYMENTS AND ALLOWANCES
        document.Add(new Paragraph("3. PAYMENTS AND ALLOWANCES").SetBold().SetMarginTop(10));

      

        // Stipend paragraph
        Paragraph stipendParagraph = new Paragraph()
            .Add(new Text("3.1 The Council shall pay a daily stipend of USD ").SetFont(normalFont))
            .Add(new Text(data.StipendRate.ToString()).SetFont(boldFont))
            .Add(new Text(" per working day for the duration of the contract. Additionally, you shall receive the following allowances:").SetFont(normalFont));

        document.Add(stipendParagraph);

        // Allowances
        var allowances = new List<(string Label, decimal Rate)>
{
    ("Accommodation", data.AccomodationRate),
    ("Breakfast", data.BreakFastRate),
    ("Lunch", data.LunchRate),
    ("Dinner", data.DinnerRate)
};

        foreach (var allowance in allowances)
        {
            Paragraph pp = new Paragraph()
                .Add(new Text($"{allowance.Label}: USD ").SetFont(normalFont))
                .Add(new Text(allowance.Rate.ToString()).SetFont(boldFont))
                .Add(new Text(" per day" + (allowance.Label == "Accommodation" ? " (if applicable)" : "")).SetFont(normalFont));

            document.Add(pp);
        }

        document.Add(new Paragraph("3.2 Resident Monitors working at another school within their locality shall not claim accommodation but will be entitled to claim transport costs, breakfast, lunch, and dinner at rates stipulated in the contract."));

        document.Add(new Paragraph("3.3 Resident Monitors deployed outside a 40km radius from town shall be entitled to claim transport costs, breakfast, lunch, dinner, and accommodation at rates stipulated in the contract."));

        document.Add(new Paragraph("3.4 The Finance Department shall process advance payments equivalent to 50% of total allowances at the start of the contract. The balance shall be paid upon completion of the assignment, subject to submission of a signed Regional attendance register, verifying full participation in daily duties."));

        document.Add(new Paragraph("3.5 Payment Method: All payments shall be processed through the bank, and bank details must be submitted for seamless transactions."));

        document.Add(new Paragraph("3.6 Work-Based Payment Rule: The Council shall only pay the agreed sum for actual workdays where the employee reports to work and performs assigned duties for the full day. Any partial work or unauthorized absenteeism may result in payment deductions."));

        document.Add(new Paragraph("3.7 Tax Deductions: ZIMSEC shall deduct applicable income tax in accordance with the provisions of the Finance (No.2) Act, 2024, and any other relevant tax regulations."));

        // Add standard sections (DISPUTE RESOLUTION, CONFIDENTIALITY, TERMINATION CLAUSE)
        AddStandardContractSections(document);
    }

    private void AddStandardContractSections(Document document)
    {
        // Add DISPUTE RESOLUTION, CONFIDENTIALITY, TERMINATION CLAUSE
        document.Add(new Paragraph("4. DISPUTE RESOLUTION").SetBold().SetMarginTop(10));
        document.Add(new Paragraph("4.1 Any disputes or grievances arising from this contract shall first be resolved internally through ZIMSEC's dispute resolution mechanisms, including engagement with the Regional Manager."));
        document.Add(new Paragraph("4.2 If unresolved, disputes may be escalated to ZIMSEC Human Capital Department for mediation before seeking external legal intervention."));

        document.Add(new Paragraph("5. CONFIDENTIALITY AND CODE OF CONDUCT").SetBold().SetMarginTop(10));
        document.Add(new Paragraph("5.1 The Assistant Cluster Manager shall maintain strict confidentiality regarding all examination materials, scripts, and related processes."));
        document.Add(new Paragraph("5.2 Any breach of confidentiality or failure to comply with the ZIMSEC Code of Conduct shall be grounds for disciplinary action, termination, and legal liability where applicable."));

        document.Add(new Paragraph("6. TERMINATION CLAUSE").SetBold().SetMarginTop(10));
        document.Add(new Paragraph("6.1 In terms Section 12(4) of the labour act 28:01), either party may terminate this contract within 24hours (One day) with a written notice."));
        document.Add(new Paragraph("6.2 If terminated due to misconduct or policy violations, payments may be withheld pending investigation."));
        document.Add(new Paragraph("The Council reserves the right to terminate this contract with immediate effect in cases of serious misconduct, fraud, negligence, or security breaches affecting national examinations."));
    }

    private void AddSignatureSection(Document document, ContractData data)
    {

        string imagePath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "Images", "hrsign.PNG");

        Image img = new Image(ImageDataFactory.Create(imagePath));
        img.SetWidth(80); // Set width of the image (optional)
        img.SetHeight(40); // Set height of the image (optional)


        document.Add(img);
        document.Add(new Paragraph("S. Malamba").SetMarginTop(25));
        document.Add(new Paragraph("Director- Human Capital"));
        document.Add(new Paragraph("ZIMBABWE SCHOOL EXAMINATIONS COUNCIL"));

        document.Add(new Paragraph(" ").SetMarginTop(25));
        document.Add(new Paragraph($"I, {data.FullName} ID No. {data.IdNumber}"));
        document.Add(new Paragraph("have read and understood the contents of this contract of employment and"));
        document.Add(new Paragraph("Accept these conditions."));

        document.Add(new Paragraph(" ").SetMarginTop(10));
        document.Add(new Paragraph($"Signature: _________________________ Date: {DateTime.Now.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}"));
        document.Add(new Paragraph($"Witness: {data.Witness} Next of Kin Cell: {data.NextOfKinCell}"));
        document.Add(new Paragraph($"Signature: _________________________ Date: {DateTime.Now.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}"));

        // Banking Details
        document.Add(new Paragraph("ZWG BANKING DETAILS").SetBold().SetMarginTop(20));
        document.Add(new Paragraph($"Name: {data.FullName}"));
        document.Add(new Paragraph($"Name of Bank: {data.BankName}"));
        document.Add(new Paragraph($"Branch: {data.Branch}"));
        document.Add(new Paragraph($"Account Number: {data.AccountNumber}"));
        document.Add(new Paragraph("Signature: _________________________"));

        document.Add(new Paragraph(" ").SetMarginTop(10));
        document.Add(new Paragraph("USD BANKING DETAILS").SetBold().SetMarginTop(20));
        document.Add(new Paragraph($"Name: {data.FullName}"));
        document.Add(new Paragraph($"Name of Bank: {data.USDBankName}"));
        document.Add(new Paragraph($"Branch: {data.USDBranch}"));
        document.Add(new Paragraph($"Account Number: {data.USDAccountNumber}"));
        document.Add(new Paragraph("Signature: _________________________"));
    }
}
public class ContractController : Controller
{
    private readonly PdfContractService _pdfService;
    private readonly ApplicationDbContext _context;
    private readonly IExamMonitorService _service;
    private readonly IMapper _mapper;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ContractController(IExamMonitorService service, IMapper mapper, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, PdfContractService pdfService)
    {
        _service = service;
        _mapper = mapper;
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
    _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> DownloadManagerContract()
    {

        ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
        var userRoles = await _userManager.GetRolesAsync(currentUser);
        var transaction = await _context.ExamMonitorTransactions.Include(a => a.ExamMonitor)
            .FirstOrDefaultAsync(a => a.SubKey == currentUser.EMS_SUBKEY);
        var phase = await _context.Phases.FirstOrDefaultAsync(a => a.PhaseCode == transaction.Phase);


        var clusterCode = await _context.Centres
              .Where(c => c.CentreNumber == transaction.CentreAttached)
              .Select(c => c.ClusterCode)
              .FirstOrDefaultAsync();


        IQueryable<DateTime> examDatesQuery;

        var centresInCluster = await _context.Centres
            .Where(c => c.ClusterCode == clusterCode)
            .Select(c => c.CentreNumber)
            .ToListAsync();

        examDatesQuery = _context.Exm_Timetable
            .Where(t => centresInCluster.Contains(t.CentreCode)
                       && t.Exam_date >= phase.StartTime.Date
                       && t.Exam_date <= phase.EndTime.Date)
            .Select(t => t.Exam_date);

        var actualExamDates = await examDatesQuery
          .Distinct()
          .OrderBy(d => d)
          .ToListAsync();

        // Add travel days: 2 days before first exam and 1 day after last exam
        var allDates = new List<DateTime>();

        if (actualExamDates.Any())
        {
            var firstExamDate = actualExamDates.Min();
            var lastExamDate = actualExamDates.Max();

            // Add 2 days before first exam
            allDates.Add(firstExamDate.AddDays(-2));
            allDates.Add(firstExamDate.AddDays(-1));

            // Add all actual exam dates
            allDates.AddRange(actualExamDates);

            // Add 1 day after last exam
            allDates.Add(lastExamDate.AddDays(1));
        }

        var session = await _context.ExamSessions.FirstOrDefaultAsync(a => a.SessionCode == transaction.Session);

        if (session == null)
        {
            return NotFound();
        }

        // Get data from user input or database - you can modify this to get from form
        var contractData = new ContractData
        {
            FullName = transaction.ExamMonitor.FirstName + " " + transaction.ExamMonitor.LastName,
            IdNumber = transaction.NationalId,
            Status = transaction.Status,
            Session = session.SessionName,
            StartDate = DateTime.Parse(phase.StartTime.ToString()),
            EndDate = DateTime.Parse(phase.EndTime.ToString()),
            DurationDays = allDates.Count(),
            BankName = transaction.ExamMonitor.BankNameZwg,
            Branch = transaction.ExamMonitor.BranchZwg,
            AccountNumber = transaction.ExamMonitor.AccountNumberZwg,
            Witness = "",
            NextOfKinCell = "",
            StipendRate = phase.ClusterManagerRate,
            LunchRate = phase.LunchRate,
            BreakFastRate = phase.BreakFastRate,
            AccomodationRate = phase.AccomodationRate,
            DinnerRate = phase.DinnerRate,
            USDBankName = transaction.ExamMonitor.BankNameUsd,
            USDBranch = transaction.ExamMonitor.BranchUsd,
            USDAccountNumber = transaction.ExamMonitor.AccountNumberUsd
        };

        if (transaction.ExamMonitor.Sex == "M")
        {
            contractData.Title = "Sir";
        }
        else
        {
            contractData.Title = "Madam";
        }

        var pdfBytes = _pdfService.GenerateClusterManagerContract(contractData);
        return File(pdfBytes, "application/pdf", "Cluster_Manager_Contract.pdf");
    }

    

    [HttpPost]
    public async Task<IActionResult> DownloadAssistantContract()
    {
        ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
        var userRoles = await _userManager.GetRolesAsync(currentUser);
        var transaction = await _context.ExamMonitorTransactions.Include(a => a.ExamMonitor)
            .FirstOrDefaultAsync(a => a.SubKey == currentUser.EMS_SUBKEY);
        var phase = await _context.Phases.FirstOrDefaultAsync(a => a.PhaseCode == transaction.Phase);


        var clusterCode = await _context.Centres
              .Where(c => c.CentreNumber == transaction.CentreAttached)
              .Select(c => c.ClusterCode)
              .FirstOrDefaultAsync();

  
        IQueryable<DateTime> examDatesQuery;

        var centresInCluster = await _context.Centres
            .Where(c => c.ClusterCode == clusterCode)
            .Select(c => c.CentreNumber)
            .ToListAsync();

        examDatesQuery = _context.Exm_Timetable
            .Where(t => centresInCluster.Contains(t.CentreCode)
                       && t.Exam_date >= phase.StartTime.Date
                       && t.Exam_date <= phase.EndTime.Date)
            .Select(t => t.Exam_date);

        var actualExamDates = await examDatesQuery
          .Distinct()
          .OrderBy(d => d)
          .ToListAsync();

        // Add travel days: 2 days before first exam and 1 day after last exam
        var allDates = new List<DateTime>();

        if (actualExamDates.Any())
        {
            var firstExamDate = actualExamDates.Min();
            var lastExamDate = actualExamDates.Max();

            // Add 2 days before first exam
            allDates.Add(firstExamDate.AddDays(-2));
            allDates.Add(firstExamDate.AddDays(-1));

            // Add all actual exam dates
            allDates.AddRange(actualExamDates);

            // Add 1 day after last exam
            allDates.Add(lastExamDate.AddDays(1));
        }

        var session = await _context.ExamSessions.FirstOrDefaultAsync(a => a.SessionCode == transaction.Session);

        if (session == null)
        {
            return NotFound();
        }
        // Get data from user input or database - you can modify this to get from form
        var contractData = new ContractData
        {
            FullName = transaction.ExamMonitor.FirstName + " " + transaction.ExamMonitor.LastName,
            IdNumber = transaction.NationalId,
            Status = transaction.Status,
            Session = session.SessionName,
            StartDate = DateTime.Parse(phase.StartTime.ToString()),
            EndDate = DateTime.Parse(phase.EndTime.ToString()),
            DurationDays = allDates.Count(),
            BankName = transaction.ExamMonitor.BankNameZwg,
            Branch = transaction.ExamMonitor.BranchZwg,
            AccountNumber = transaction.ExamMonitor.AccountNumberZwg,
            Witness = "",
            NextOfKinCell = "",
            StipendRate = phase.AssistantClusterManagerRate,
            LunchRate = phase.LunchRate,
            BreakFastRate = phase.BreakFastRate,
            AccomodationRate = phase.AccomodationRate,
            DinnerRate = phase.DinnerRate,
            USDBankName = transaction.ExamMonitor.BankNameUsd,
            USDBranch = transaction.ExamMonitor.BranchUsd,
            USDAccountNumber = transaction.ExamMonitor.AccountNumberUsd
        };

        if (transaction.ExamMonitor.Sex == "M")
        {
            contractData.Title = "Sir";
        }
        else
        {
            contractData.Title = "Madam";
        }

        var pdfBytes = _pdfService.GenerateAssistantClusterManagerContract(contractData);
        return File(pdfBytes, "application/pdf", "Assistant_Cluster_Manager_Contract.pdf");
    }

    [HttpPost]
    public async Task<IActionResult> DownloadResidentMonitorContract()
    {

        ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
        var userRoles = await _userManager.GetRolesAsync(currentUser);
        var transaction = await _context.ExamMonitorTransactions.Include(a => a.ExamMonitor)
            .FirstOrDefaultAsync(a => a.SubKey == currentUser.EMS_SUBKEY);
        var phase = await _context.Phases.FirstOrDefaultAsync(a => a.PhaseCode == transaction.Phase);

        IQueryable<DateTime> examDatesQuery;

    
            examDatesQuery = _context.Exm_Timetable
                .Where(t => t.CentreCode == transaction.CentreAttached
                           && t.Exam_date >= phase.StartTime.Date
                           && t.Exam_date <= phase.EndTime.Date)
                .Select(t => t.Exam_date);


        var actualExamDates = await examDatesQuery
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        // Add travel days: 2 days before first exam and 1 day after last exam
        var allDates = new List<DateTime>();

        if (actualExamDates.Any())
        {
            var firstExamDate = actualExamDates.Min();
            var lastExamDate = actualExamDates.Max();

            // Add 2 days before first exam
            allDates.Add(firstExamDate.AddDays(-2));
            allDates.Add(firstExamDate.AddDays(-1));

            // Add all actual exam dates
            allDates.AddRange(actualExamDates);

            // Add 1 day after last exam
            allDates.Add(lastExamDate.AddDays(1));
        }

        var session = await _context.ExamSessions.FirstOrDefaultAsync(a => a.SessionCode == transaction.Session);

        if(session == null)
        {
            return NotFound();
        }

        var contractData = new ContractData
        {
            FullName = transaction.ExamMonitor.FirstName + " " + transaction.ExamMonitor.LastName,
            IdNumber = transaction.NationalId,
            Session = session.SessionName,
            Status = transaction.Status,
            StartDate = DateTime.Parse(phase.StartTime.ToString()),
            EndDate = DateTime.Parse(phase.EndTime.ToString()),
            DurationDays = allDates.Count(),
            BankName = transaction.ExamMonitor.BankNameZwg,
            Branch = transaction.ExamMonitor.BranchZwg,
            AccountNumber = transaction.ExamMonitor.AccountNumberZwg,
            Witness = "",
            NextOfKinCell = "",
            StipendRate = phase.ResidentMonitorRate,
            LunchRate = phase.LunchRate,
            BreakFastRate = phase.BreakFastRate,
            AccomodationRate = phase.AccomodationRate,
            DinnerRate = phase.DinnerRate,
            USDBankName = transaction.ExamMonitor.BankNameUsd,
            USDBranch = transaction.ExamMonitor.BranchUsd,
            USDAccountNumber = transaction.ExamMonitor.AccountNumberUsd
        };

        if(transaction.ExamMonitor.Sex == "M")
        {
            contractData.Title = "Sir";
        }
        else
        {
            contractData.Title = "Madam";
        }

            var pdfBytes = _pdfService.GenerateResidentMonitorContract(contractData);
        return File(pdfBytes, "application/pdf", "Resident_Monitor_Contract.pdf");
    }


    [HttpGet]
    public async Task<IActionResult> DownloadContract(string nationalId, string category)
    {
        // Fetch examiner transaction by National ID
        var transaction = await _context.ExamMonitorTransactions
            .Include(a => a.ExamMonitor)
            .FirstOrDefaultAsync(a => a.NationalId == nationalId);

        if (transaction == null)
        {
            return NotFound($"No transaction found for National ID: {nationalId}");
        }

        if(transaction.CentreAttached == "XXXXXX")
        {
            return NotFound();
        }

        var phase = await _context.Phases.FirstOrDefaultAsync(a => a.PhaseCode == transaction.Phase);
        if (phase == null)
        {
            return NotFound($"No phase found for {transaction.Phase}");
        }


        var session = await _context.ExamSessions.FirstOrDefaultAsync(a => a.SessionCode == transaction.Session);

        if (session == null)
        {
            return NotFound();
        }

        // Build exam date list
        IQueryable<DateTime> examDatesQuery;
        if (category == "Cluster Manager" || category == "Assistant Cluster Manager")
        {
            var clusterCode = await _context.Centres
                .Where(c => c.CentreNumber == transaction.CentreAttached)
                .Select(c => c.ClusterCode)
                .FirstOrDefaultAsync();

            if (clusterCode == null)
                return NotFound($"Cluster code not found for centre: {transaction.CentreAttached}");

            var centresInCluster = await _context.Centres
                .Where(c => c.ClusterCode == clusterCode)
                .Select(c => c.CentreNumber)
                .ToListAsync();

            examDatesQuery = _context.Exm_Timetable
                .Where(t => centresInCluster.Contains(t.CentreCode)
                    && t.Exam_date >= phase.StartTime.Date
                    && t.Exam_date <= phase.EndTime.Date)
                .Select(t => t.Exam_date);
        }
        else
        {
            examDatesQuery = _context.Exm_Timetable
                .Where(t => t.CentreCode == transaction.CentreAttached
                    && t.Exam_date >= phase.StartTime.Date
                    && t.Exam_date <= phase.EndTime.Date)
                .Select(t => t.Exam_date);
        }

        var actualExamDates = await examDatesQuery
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        // Travel days
        var allDates = new List<DateTime>();
        if (actualExamDates.Any())
        {
            var firstExamDate = actualExamDates.Min();
            var lastExamDate = actualExamDates.Max();

            allDates.Add(firstExamDate.AddDays(-2));
            allDates.Add(firstExamDate.AddDays(-1));
            allDates.AddRange(actualExamDates);
            allDates.Add(lastExamDate.AddDays(1));
        }

        // Build contract data
        var contractData = new ContractData
        {
            FullName = transaction.ExamMonitor.FirstName + " " + transaction.ExamMonitor.LastName,
            IdNumber = transaction.NationalId,
            Session = session.SessionName,
            Status = transaction.Status,
            StartDate = phase.StartTime,
            EndDate = phase.EndTime,
            DurationDays = allDates.Count,
            BankName = transaction.ExamMonitor.BankNameZwg,
            Branch = transaction.ExamMonitor.BranchZwg,
            AccountNumber = transaction.ExamMonitor.AccountNumberZwg,
            Witness = "",
            NextOfKinCell = "",
            LunchRate = phase.LunchRate,
            BreakFastRate = phase.BreakFastRate,
            AccomodationRate = phase.AccomodationRate,
            DinnerRate = phase.DinnerRate,
            USDBankName = transaction.ExamMonitor.BankNameUsd,
            USDBranch = transaction.ExamMonitor.BranchUsd,
            USDAccountNumber = transaction.ExamMonitor.AccountNumberUsd,
            Title = transaction.ExamMonitor.Sex == "M" ? "Sir" : "Madam"
        };

        // Assign rates and PDF template based on category
        byte[] pdfBytes;
        string fileName;

        switch (category)
        {
            case "Cluster Manager":
                contractData.StipendRate = phase.ClusterManagerRate;
                pdfBytes = _pdfService.GenerateClusterManagerContract(contractData);
                fileName = "Cluster_Manager_Contract.pdf";
                break;

            case "Assistant Cluster Manager":
                contractData.StipendRate = phase.AssistantClusterManagerRate;
                pdfBytes = _pdfService.GenerateAssistantClusterManagerContract(contractData);
                fileName = "Assistant_Cluster_Manager_Contract.pdf";
                break;

            case "Resident Monitor":
                contractData.StipendRate = phase.ResidentMonitorRate;
                pdfBytes = _pdfService.GenerateResidentMonitorContract(contractData);
                fileName = "Resident_Monitor_Contract.pdf";
                break;

            default:
                return BadRequest("Unknown category.");
        }

        return File(pdfBytes, "application/pdf", fileName);
    }


    [HttpGet]
    public async Task<IActionResult> BulkDownloadContracts(string region, string session, string phase)
    {
        // 1. Fetch all exam monitor transactions matching filters
        var transactions = await _context.ExamMonitorTransactions
            .Include(t => t.ExamMonitor)
            .Where(t => t.Region == region && t.Session == session && t.Phase == phase && t.CentreAttached != "XXXXXX")
            .ToListAsync();

        if (!transactions.Any())
            return NotFound("No transactions found for the selected filters.");


        var sessionData = await _context.ExamSessions.FirstOrDefaultAsync(a => a.SessionCode == session);

        if (session == null)
        {
            return NotFound();
        }

        using var memoryStream = new MemoryStream();

        using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var t in transactions)
            {
                var phaseEntity = await _context.Phases
                    .FirstOrDefaultAsync(p => p.PhaseCode == t.Phase);
                if (phaseEntity == null) continue;

                // Determine exam dates
                IQueryable<DateTime> examDatesQuery;

                if (t.Status == "Cluster Manager" || t.Status == "Assistant Cluster Manager")
                {
                    var clusterCode = await _context.Centres
                        .Where(c => c.CentreNumber == t.CentreAttached)
                        .Select(c => c.ClusterCode)
                        .FirstOrDefaultAsync();

                    if (clusterCode == null) continue;

                    var centresInCluster = await _context.Centres
                        .Where(c => c.ClusterCode == clusterCode)
                        .Select(c => c.CentreNumber)
                        .ToListAsync();

                    examDatesQuery = _context.Exm_Timetable
                        .Where(d => centresInCluster.Contains(d.CentreCode)
                                    && d.Exam_date >= phaseEntity.StartTime.Date
                                    && d.Exam_date <= phaseEntity.EndTime.Date)
                        .Select(d => d.Exam_date);
                }
                else
                {
                    examDatesQuery = _context.Exm_Timetable
                        .Where(d => d.CentreCode == t.CentreAttached
                                    && d.Exam_date >= phaseEntity.StartTime.Date
                                    && d.Exam_date <= phaseEntity.EndTime.Date)
                        .Select(d => d.Exam_date);
                }

                var actualExamDates = await examDatesQuery
                    .Distinct()
                    .OrderBy(d => d)
                    .ToListAsync();

                // Travel days
                var allDates = new List<DateTime>();
                if (actualExamDates.Any())
                {
                    var firstExamDate = actualExamDates.Min();
                    var lastExamDate = actualExamDates.Max();

                    allDates.Add(firstExamDate.AddDays(-2));
                    allDates.Add(firstExamDate.AddDays(-1));
                    allDates.AddRange(actualExamDates);
                    allDates.Add(lastExamDate.AddDays(1));
                }

                // Build contract data
                var contractData = new ContractData
                {
                    FullName = $"{t.ExamMonitor.FirstName} {t.ExamMonitor.LastName}",
                    IdNumber = t.NationalId,
                    Status = t.Status,
                    Session = sessionData.SessionName?? "",
                    StartDate = phaseEntity.StartTime,
                    EndDate = phaseEntity.EndTime,
                    DurationDays = allDates.Count,
                    BankName = t.ExamMonitor.BankNameZwg,
                    Branch = t.ExamMonitor.BranchZwg,
                    AccountNumber = t.ExamMonitor.AccountNumberZwg,
                    Witness = "",
                    NextOfKinCell = "",
                    LunchRate = phaseEntity.LunchRate,
                    BreakFastRate = phaseEntity.BreakFastRate,
                    AccomodationRate = phaseEntity.AccomodationRate,
                    DinnerRate = phaseEntity.DinnerRate,
                    USDBankName = t.ExamMonitor.BankNameUsd,
                    USDBranch = t.ExamMonitor.BranchUsd,
                    USDAccountNumber = t.ExamMonitor.AccountNumberUsd,
                    Title = t.ExamMonitor.Sex == "M" ? "Sir" : "Madam"
                };

                // Assign category-specific stipend rate & PDF template
                byte[] pdfBytes;
                string fileName;

                switch (t.Status)
                {
                    case "Cluster Manager":
                        contractData.StipendRate = phaseEntity.ClusterManagerRate;
                        pdfBytes = _pdfService.GenerateClusterManagerContract(contractData);
                        fileName = $"{contractData.FullName}_ClusterManager.pdf";
                        break;

                    case "Assistant Cluster Manager":
                        contractData.StipendRate = phaseEntity.AssistantClusterManagerRate;
                        pdfBytes = _pdfService.GenerateAssistantClusterManagerContract(contractData);
                        fileName = $"{contractData.FullName}_AssistantClusterManager.pdf";
                        break;

                    case "Resident Monitor":
                        contractData.StipendRate = phaseEntity.ResidentMonitorRate;
                        pdfBytes = _pdfService.GenerateResidentMonitorContract(contractData);
                        fileName = $"{contractData.FullName}_ResidentMonitor.pdf";
                        break;

                    default:
                        continue;
                }

                // Add PDF to ZIP
                if (pdfBytes != null)
                {
                    var entry = zip.CreateEntry(fileName, CompressionLevel.Fastest);
                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                }
            }
        }

        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "application/zip",
            $"Contracts_{region}_{session}_{phase}.zip");
    }



    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}


//using Microsoft.AspNetCore.Mvc;

//namespace ExaminerPaymentSystem.Controllers.ExamMonitors
//{
//    using ExaminerPaymentSystem.Data;
//    using ExaminerPaymentSystem.Models.ExamMonitors;
//    // Controllers/ContractsController.cs
//    using iText.Html2pdf;
//    using iText.Kernel.Pdf;
//    using Microsoft.AspNetCore.Mvc;
//    using Microsoft.EntityFrameworkCore;
//    using System.IO;
//    using System.Threading.Tasks;

//    public class ContractsController : Controller
//    {
//        private readonly IContractService _contractService;

//        public ContractsController(IContractService contractService)
//        {
//            _contractService = contractService;
//        }

//        // HR: Edit contract template
//        [HttpGet]
//        public async Task<IActionResult> EditTemplate(int id)
//        {
//            var template = await _contractService.GetTemplateByIdAsync(id);
//            if (template == null)
//            {
//                return NotFound();
//            }
//            return View(template);
//        }

//        [HttpPost]
//        public async Task<IActionResult> EditTemplate(ContractTemplate template)
//        {
//            if (ModelState.IsValid)
//            {
//                await _contractService.UpdateTemplateAsync(template);
//                return RedirectToAction(nameof(TemplateList));
//            }
//            return View(template);
//        }

//        // HR: List all templates
//        public async Task<IActionResult> TemplateList()
//        {
//            var templates = await _contractService.GetAllTemplatesAsync();
//            return View(templates);
//        }

//        // User: Generate contract form
//        [HttpGet]
//        public async Task<IActionResult> GenerateContract(string contractType)
//        {
//            var template = await _contractService.GetActiveTemplateAsync(contractType);
//            if (template == null)
//            {
//                return NotFound();
//            }

//            // Get user data from database (you'll need to implement this)
//            var userData = await _contractService.GetUserContractDataAsync(User.Identity.Name);

//            var viewModel = new ContractViewModel
//            {
//                Template = template,
//                Data = userData,
//                ContractType = contractType
//            };

//            return View(viewModel);
//        }

//        // User: Download PDF contract
//        [HttpPost]
//        public async Task<IActionResult> DownloadContract(ContractViewModel model)
//        {
//            var template = await _contractService.GetActiveTemplateAsync(model.ContractType);
//            if (template == null)
//            {
//                return NotFound();
//            }

//            // Replace placeholders with actual data
//            string filledContent = ReplacePlaceholders(template.Content, model.Data);

//            // Generate PDF using iText7
//            byte[] pdfBytes;
//            using (var memoryStream = new MemoryStream())
//            {
//                using (var pdfWriter = new PdfWriter(memoryStream))
//                {
//                    using (var pdfDocument = new PdfDocument(pdfWriter))
//                    {
//                        var converter = new HtmlConverter(pdfDocument);
//                        converter.ConvertToPdf(filledContent, pdfDocument);
//                    }
//                }
//                pdfBytes = memoryStream.ToArray();
//            }

//            string fileName = $"{model.ContractType}_Contract_{DateTime.Now:yyyyMMddHHmmss}.pdf";
//            return File(pdfBytes, "application/pdf", fileName);
//        }

//        private string ReplacePlaceholders(string content, ContractData data)
//        {
//            // Replace all placeholders with actual data
//            return content
//                .Replace("....................................................................................", data.FullName)
//                .Replace("Mr./Mrs./Miss/Dr./Prof....................................................................................", $"{data.Title} {data.FullName}")
//                .Replace(".......", data.StartDate.ToString("dd MMMM yyyy"))
//                .Replace(".........", data.EndDate.ToString("dd MMMM yyyy"))
//                .Replace("....................................", data.FullName)
//                .Replace("...........................................", data.IdNumber)
//                .Replace(".....................................", DateTime.Now.ToString("dd MMMM yyyy"))
//                .Replace(".......................................", data.Witness)
//                .Replace(".....................................", data.NextOfKinCell)
//                .Replace("\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_", data.FullName)
//                .Replace("\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_", data.BankName)
//                .Replace("\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_", data.Branch)
//                .Replace("\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_", data.AccountNumber);
//        }
//    }


//    // Services/IContractService.cs
//    public interface IContractService
//    {
//        Task<ContractTemplate> GetTemplateByIdAsync(int id);
//        Task<List<ContractTemplate>> GetAllTemplatesAsync();
//        Task<ContractTemplate> GetActiveTemplateAsync(string contractType);
//        Task UpdateTemplateAsync(ContractTemplate template);
//        Task<ContractData> GetUserContractDataAsync(string username);
//    }

//    // Services/ContractService.cs
//    public class ContractService : IContractService
//    {
//        private readonly ApplicationDbContext _context;

//        public ContractService(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<ContractTemplate> GetTemplateByIdAsync(int id)
//        {
//            return await _context.ContractTemplates.FindAsync(id);
//        }

//        public async Task<List<ContractTemplate>> GetAllTemplatesAsync()
//        {
//            return await _context.ContractTemplates.ToListAsync();
//        }

//        public async Task<ContractTemplate> GetActiveTemplateAsync(string contractType)
//        {
//            return await _context.ContractTemplates
//                .FirstOrDefaultAsync(t => t.TemplateType == contractType && t.IsActive);
//        }

//        public async Task UpdateTemplateAsync(ContractTemplate template)
//        {
//            template.LastModifiedDate = DateTime.Now;
//            _context.ContractTemplates.Update(template);
//            await _context.SaveChangesAsync();
//        }

//        public async Task<ContractData> GetUserContractDataAsync(string username)
//        {
//            // Implement logic to get user data from your database
//            // This is a simplified example
//            var user = await _context.Users
//                .FirstOrDefaultAsync(u => u.UserName == username);

//            return new ContractData
//            {
//                FullName = user?.FullName,
//                IdNumber = user?.IdNumber,
//                Title = user?.Title,
//                // Add other properties as needed
//            };
//        }
//    }
//}
