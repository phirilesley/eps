using ExaminerPaymentSystem.Models;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Org.BouncyCastle.Crypto.Engines;


namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class PDFService
    {
        //public byte[] GeneratePDF(TandSReport tandSReport)
        //{
        //    //Define your memory stream which will temporarily hold the PDF
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        //Initialize PDF writer with landscape orientation and A4 page size
        //        PdfWriter writer = new PdfWriter(stream, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0));
        //        PdfDocument pdf = new PdfDocument(writer);
        //        // Set page size to A4 landscape
        //        PageSize pageSize = PageSize.A4.Rotate();
        //        pdf.SetDefaultPageSize(pageSize);

        //        // Initialize document
        //        Document document = new Document(pdf);

        //        // Add the date in the top left corner
        //        Paragraph dateInfo = new Paragraph(DateTime.Now.ToString("MMMM dd, yyyy"))
        //            .SetFontSize(6)
        //            .SetFixedPosition(40, pageSize.GetHeight() - 40, pageSize.GetWidth() - 40);
        //        document.Add(dateInfo);
        //        // Add content to the document
        //        // Header
        //        document.Add(new Paragraph("ZIMBABWE SCHOOL EXAMINATIONS COUNCIL")
        //            .SetTextAlignment(TextAlignment.CENTER)
        //            .SetFontSize(20));

        //        document.Add(new Paragraph("Travelling And Subsistence Claim Report")
        //            .SetTextAlignment(TextAlignment.CENTER)
        //            .SetBold());

        //        // tandSReport data
        //        Div leftContent = new Div()
        //            .SetTextAlignment(TextAlignment.LEFT)
        //            .Add(new Paragraph("Claim Details").SetBold())
        //            .Add(new Paragraph($"Full Name: {tandSReport.EMS_BANK_NAME_ZWL}"))
        //            .Add(new Paragraph($"National ID: {tandSReport.EMS_NATIONAL_ID}"))
        //            .Add(new Paragraph($"Address: {tandSReport.EMS_ADDRESS}"))
        //            .Add(new Paragraph($"Phone: {tandSReport.EMS_PHONE_HOME}"))
        //            .Add(new Paragraph($"Bank (NOSTRO): {tandSReport.EMS_BANK_NAME_FCA}"))
        //            .Add(new Paragraph($"Account Number (NOSTRO): {tandSReport.EMS_ACCOUNT_NO_FCA}"));

        //        Div rightContent = new Div()
        //            .SetTextAlignment(TextAlignment.RIGHT)
        //            .Add(new Paragraph($"Subject: {tandSReport.EMS_SUB_SUB_ID}"))
        //            .Add(new Paragraph($"Level Of Examination: {tandSReport.EMS_LEVEL_OF_EXAM_MARKED}"))
        //            .Add(new Paragraph($"Purpose Of Journey: {tandSReport.EMS_PURPOSEOFJOURNEY}"))
        //            .Add(new Paragraph($"Bank (ZiG): {tandSReport.EMS_BANK_NAME_ZWL}"))
        //            .Add(new Paragraph($"Account Number (ZiG): {tandSReport.EMS_ACCOUNT_NO_ZWL}"));

        //        // Add left and right content to the document
        //        document.Add(new Paragraph().Add(leftContent).Add(rightContent));

        //        document.Add(new Paragraph("Actual Claim")
        //            .SetTextAlignment(TextAlignment.CENTER)
        //            .SetBold());

        //        // Table for tandSReport items
        //        Table table = new Table(new float[] { 3, 1, 1, 1,1,1,1,1 });
        //        table.SetWidth(UnitValue.CreatePercentValue(100));
        //        table.AddHeaderCell("Date");
        //        table.AddHeaderCell("Departure");
        //        table.AddHeaderCell("Arrival");
        //        table.AddHeaderCell("Place(From/To)");
        //        table.AddHeaderCell("Busfare");
        //        table.AddHeaderCell("Accommodation");
        //        table.AddHeaderCell("Lunch");
        //        table.AddHeaderCell("Total");
        //        foreach (var item in tandSReport.TANDSDETAILS)
        //        {
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_DATE)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_DEPARTURE)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_ARRIVAL)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_PLACE)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_BUSFARE)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_ACCOMMODATION)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_LUNCH)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_TOTAL)));
        //        }
        //        // Add the Table to the PDF Document
        //        document.Add(table);
        //        // Total Amount
        //        document.Add(new Paragraph($"Total Amount Due: {tandSReport.EMS_TOTAL}")
        //            .SetTextAlignment(TextAlignment.RIGHT));

        //        document.Add(new Paragraph("Adjusted Accounting")
        //        .SetTextAlignment(TextAlignment.CENTER)
        //        .SetBold());


        //        // Table for tandSReport items
        //        Table table2 = new Table(new float[] { 3, 1, 1, 1, 1, 1, 1, 1 });
        //        table2.SetWidth(UnitValue.CreatePercentValue(100));
        //        table2.AddHeaderCell("Date");
        //        table2.AddHeaderCell("Departure");
        //        table2.AddHeaderCell("Arrival");
        //        table2.AddHeaderCell("Place(From/To)");
        //        table2.AddHeaderCell("Busfare");
        //        table2.AddHeaderCell("Accommodation");
        //        table2.AddHeaderCell("Lunch");
        //        table2.AddHeaderCell("Total");
        //        foreach (var item in tandSReport.TANDSDETAILS)
        //        {
        //            table2.AddCell(new Cell().Add(new Paragraph(item.EMS_DATE)));
        //            table2.AddCell(new Cell().Add(new Paragraph(item.EMS_DEPARTURE)));
        //            table2.AddCell(new Cell().Add(new Paragraph(item.EMS_ARRIVAL)));
        //            table2.AddCell(new Cell().Add(new Paragraph(item.EMS_PLACE)));
        //            table2.AddCell(new Cell().Add(new Paragraph(item.ADJ_BUSFARE)));
        //            table2.AddCell(new Cell().Add(new Paragraph(item.ADJ_ACCOMMODATION)));
        //            table2.AddCell(new Cell().Add(new Paragraph(item.ADJ_LUNCH)));
        //            table2.AddCell(new Cell().Add(new Paragraph(item.ADJ_TOTAL)));
        //        }
        //        // Add the Table to the PDF Document
        //        document.Add(table2);
        //        // Total Amount
        //        document.Add(new Paragraph($"Total Amount Due: {tandSReport.ADJ_TOTAL}")
        //            .SetTextAlignment(TextAlignment.RIGHT));


        //        // Close the Document
        //        document.Close();
        //        return stream.ToArray();
        //    }
        //}


        //public byte[] GenerateActualClaimPDF(TandSReport tandSReport)
        //{
        //    //Define your memory stream which will temporarily hold the PDF
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        //Initialize PDF writer with landscape orientation and A4 page size
        //        PdfWriter writer = new PdfWriter(stream, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0));
        //        PdfDocument pdf = new PdfDocument(writer);
        //        // Set page size to A4 landscape
        //        PageSize pageSize = PageSize.A4.Rotate();
        //        pdf.SetDefaultPageSize(pageSize);

        //        // Initialize document
        //        Document document = new Document(pdf);

        //        // Add the date in the top left corner
        //        Paragraph dateInfo = new Paragraph(DateTime.Now.ToString("MMMM dd, yyyy"))
        //            .SetFontSize(6)
        //            .SetFixedPosition(40, pageSize.GetHeight() - 40, pageSize.GetWidth() - 40);
        //        document.Add(dateInfo);

        //        // Add content to the document
        //        // Header
        //        document.Add(new Paragraph("ZIMBABWE SCHOOL EXAMINATIONS COUNCIL")
        //            .SetTextAlignment(TextAlignment.CENTER)
        //            .SetFontSize(20));

        //        document.Add(new Paragraph("Travelling And Subsistence Claim Report")
        //            .SetTextAlignment(TextAlignment.CENTER)
        //            .SetBold());

        //        // tandSReport data
        //        Div leftContent = new Div()
        //            .SetTextAlignment(TextAlignment.LEFT)
        //            .Add(new Paragraph("Claim Details").SetBold())
        //            .Add(new Paragraph($"Full Name: {tandSReport.EMS_BANK_NAME_ZWL}"))
        //            .Add(new Paragraph($"National ID: {tandSReport.EMS_NATIONAL_ID}"))
        //            .Add(new Paragraph($"Address: {tandSReport.EMS_ADDRESS}"))
        //            .Add(new Paragraph($"Phone: {tandSReport.EMS_PHONE_HOME}"))
        //            .Add(new Paragraph($"Bank (NOSTRO): {tandSReport.EMS_BANK_NAME_FCA}"))
        //            .Add(new Paragraph($"Account Number (NOSTRO): {tandSReport.EMS_ACCOUNT_NO_FCA}"));

        //        Div rightContent = new Div()
        //            .SetTextAlignment(TextAlignment.RIGHT)
        //            .Add(new Paragraph($"Subject: {tandSReport.EMS_SUB_SUB_ID}"))
        //            .Add(new Paragraph($"Level Of Examination: {tandSReport.EMS_LEVEL_OF_EXAM_MARKED}"))
        //            .Add(new Paragraph($"Purpose Of Journey: {tandSReport.EMS_PURPOSEOFJOURNEY}"))
        //            .Add(new Paragraph($"Bank (ZiG): {tandSReport.EMS_BANK_NAME_ZWL}"))
        //            .Add(new Paragraph($"Account Number (ZiG): {tandSReport.EMS_ACCOUNT_NO_ZWL}"));

        //        // Add left and right content to the document
        //        document.Add(new Paragraph().Add(leftContent).Add(rightContent));

        //        document.Add(new Paragraph("Actual Claim")
        //            .SetTextAlignment(TextAlignment.CENTER)
        //            .SetBold());

        //        // Table for tandSReport items
        //        Table table = new Table(new float[] { 3, 1, 1, 1, 1, 1, 1, 1 });
        //        table.SetWidth(UnitValue.CreatePercentValue(100));
        //        table.AddHeaderCell("Date");
        //        table.AddHeaderCell("Departure");
        //        table.AddHeaderCell("Arrival");
        //        table.AddHeaderCell("Place(From/To)");
        //        table.AddHeaderCell("Busfare");
        //        table.AddHeaderCell("Accommodation");
        //        table.AddHeaderCell("Lunch");
        //        table.AddHeaderCell("Total");
        //        foreach (var item in tandSReport.TANDSDETAILS)
        //        {
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_DATE)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_DEPARTURE)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_ARRIVAL)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_PLACE)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_BUSFARE)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_ACCOMMODATION)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_LUNCH)));
        //            table.AddCell(new Cell().Add(new Paragraph(item.EMS_TOTAL)));
        //        }
        //        // Add the Table to the PDF Document
        //        document.Add(table);
        //        // Total Amount
        //        document.Add(new Paragraph($"Total Amount: {tandSReport.EMS_TOTAL}")
        //            .SetTextAlignment(TextAlignment.RIGHT));

               

        //        // Close the Document
        //        document.Close();
        //        return stream.ToArray();
        //    }

            



        //    }

        //public byte[] GenerateAccountsTanSReport(List<TravelAdvanceReport> tandsReports, TandSAdvanceFees advanceFees)
        //{
        //    // Define your memory stream which will temporarily hold the PDF
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        // Initialize PDF writer with landscape orientation and A4 page size
        //        PdfWriter writer = new PdfWriter(stream, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0));
        //        PdfDocument pdf = new PdfDocument(writer);

        //        // Set page size to A4 landscape
        //        PageSize pageSize = PageSize.A4.Rotate();
        //        pdf.SetDefaultPageSize(pageSize);

        //        // Initialize document
        //        Document document = new Document(pdf);

        //        // Add the key information at the top of the report, with smaller font size
        //        Paragraph keyInfo = new Paragraph()
        //            .Add("Harare Poly KEY: LO=LOCAL OUT OF RES DEC 2023 MARKING\n")
        //            .Add("T&SNLO:NON LOCAL OUT OF RE 4004/01\n")
        //            .Add("RES : IN RESIDENCE")
        //            .SetFontSize(7)
        //        .SetMarginBottom(20); // Set the font size to 10pt
        //        document.Add(keyInfo);

        //        // Add the date in the top left corner
        //        Paragraph dateInfo = new Paragraph(DateTime.Now.ToString("MMMM dd, yyyy"))
        //            .SetFontSize(6)
        //            .SetFixedPosition(40, pageSize.GetHeight() - 40, pageSize.GetWidth() - 40);
        //        document.Add(dateInfo);

        //        // Add rest of the content of the report
      

        //        // Define relative column widths (percentages)
        //        float[] colWidths = { 3, 7, 5,5,3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 5, 5, 3,5 };

        //        // Create the table with relative column widths
        //        Table table = new Table(UnitValue.CreatePercentArray(colWidths));
        //        table.SetWidth(UnitValue.CreatePercentValue(100)); // Set table width to 100% of page width
        //        table.SetFixedLayout();

        //      string[] headers = {
        //        "STATUS", "NAME", "BANK ZIG","ACCOUNT ZIG", "BANK USD","BANK USD","WORK ADDRESS","DAYS", "TRANSIT BUSFARE", "LOCAL BUSFARE", "TRANSIT LUNCH",
        //        "CHECK INN ACC", "ACCOMMODATION", "B/FAST", "LUNCH AND DINNER", "CHECK IN DINNER", "SUPP",
        //        "TOTAL", "LESS ADV", "USD BAL", "RATE", "ZIG BALANCE", "ZIG TICKET", "ZIG PAYMENT"
        //    };

        //        foreach (string header in headers)
        //        {
        //            Cell cell = new Cell().Add(new Paragraph(header).SetFontSize(7).SetHeight(10));
        //            table.AddHeaderCell(cell);
        //        }
                
               
        //        table.StartNewRow();
        //        table.AddCell(CreateCell("-"));
        //        table.AddCell(CreateCell("-"));
        //        table.AddCell(CreateCell("-"));
        //        table.AddCell(CreateCell("-")); 
        //        table.AddCell(CreateCell("-"));
        //        table.AddCell(CreateCell("-"));
        //        table.AddCell(CreateCell("-"));
        //        table.AddCell(CreateCell("-"));
        //        table.AddCell(CreateCell("-"));
        //        table.AddCell(CreateCell(advanceFees.FEE_TRANSPORT));
        //        table.AddCell(CreateCell("-")); 
        //        table.AddCell(CreateCell("-")); 
        //        table.AddCell(CreateCell(advanceFees.FEE_ACCOMMODATION_NONRES));
                
        //        table.AddCell(CreateCell(advanceFees.FEE_BREAKFAST)); 
        //        table.AddCell(CreateCell(advanceFees.FEE_DINNER + advanceFees.FEE_LUNCH));
        //        table.AddCell(CreateCell("-")); 
        //        table.AddCell(CreateCell(advanceFees.FEE_OVERNIGHTALLOWANCE));
        //        table.AddCell(CreateCell("-")); 
        //        table.AddCell(CreateCell("-")); // For TOTAL
        //        table.AddCell(CreateCell("-")); // For LESS ADV
        //        table.AddCell(CreateCell("-")); // For USD BAL
        //        table.AddCell(CreateCell("-")); // For RATE
        //        table.AddCell(CreateCell("-")); // For ZIG BALANCE
        //        table.AddCell(CreateCell("-")); // For ZIG TICKET
        //        table.AddCell(CreateCell("-")); // For ZIG PAYMENT




        //        // Add data rows
        //        foreach (var item in tandsReports)
        //        {
        //            table.StartNewRow(); // Start a new row for each report item

        //            // Add cell data for each property in the TravelAdvanceReport class
        //            table.AddCell(CreateCell(item.Status));
        //            table.AddCell(CreateCell(item.Name));
        //            table.AddCell(CreateCell(item.BankZIG));
        //            table.AddCell(CreateCell(item.BankAccountZIG));
        //            table.AddCell(CreateCell(item.BankUSD));
        //            table.AddCell(CreateCell(item.BankAccountUSD));
        //            table.AddCell(CreateCell(item.WorkAddress));
        //            table.AddCell(CreateCell(item.Days));
        //            table.AddCell(CreateCell(item.TransitBusFare));
        //            table.AddCell(CreateCell(item.LocalBusFare));
        //            table.AddCell(CreateCell(item.TransitLunch));
        //            table.AddCell(CreateCell(item.CheckInnAccommodation));
        //            table.AddCell(CreateCell(item.Accommodation));
        //            table.AddCell(CreateCell(item.Breakfast));
        //            table.AddCell(CreateCell(item.LunchAndDinner));
        //            table.AddCell(CreateCell(item.CheckInDinner));
        //            table.AddCell(CreateCell(item.Supp));
        //            table.AddCell(CreateCell(item.Total));
        //            table.AddCell(CreateCell(item.LessAdvance));
        //            table.AddCell(CreateCell(item.UsdBalance));
        //            table.AddCell(CreateCell(item.Rate));
        //            table.AddCell(CreateCell(item.ZigBalance));
        //            table.AddCell(CreateCell(item.ZigTicket));
        //            table.AddCell(CreateCell(item.ZigPayment));
        //        }

         
       


        //    // Add the table to the document
        //    document.Add(table);


        //        // Close the document
        //        document.Close();

        //        return stream.ToArray();
        //    }

        //    // Helper method to create a cell with specified data
          

        //}


        //public byte[] GenerateAccountsExaminerReport(List<TravelExaminerMarkingReport> tandsReports)
        //{
        //    // Define your memory stream which will temporarily hold the PDF
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        // Initialize PDF writer with landscape orientation and A4 page size
        //        PdfWriter writer = new PdfWriter(stream, new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0));
        //        PdfDocument pdf = new PdfDocument(writer);

        //        // Set page size to A4 landscape
        //        PageSize pageSize = PageSize.A4.Rotate();
        //        pdf.SetDefaultPageSize(pageSize);

        //        // Initialize document
        //        Document document = new Document(pdf);

           
          

        //        // Add the date in the top left corner
        //        Paragraph dateInfo = new Paragraph(DateTime.Now.ToString("MMMM dd, yyyy"))
        //            .SetFontSize(6)
        //            .SetFixedPosition(40, pageSize.GetHeight() - 40, pageSize.GetWidth() - 40)
        //         .SetMarginBottom(20);
        //        document.Add(dateInfo);

        //        // Add rest of the content of the report


        //        // Define relative column widths (percentages)
        //        float[] colWidths = { 4, 5, 4, 7, 7, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4 };

        //        // Create the table with relative column widths
        //        Table table = new Table(UnitValue.CreatePercentArray(colWidths));
        //        table.SetWidth(UnitValue.CreatePercentValue(100)); // Set table width to 100% of page width
        //        table.SetFixedLayout();

        //        // Add column headers
        //        string[] headers = {
        //        "DATE", "BANK", "SHORT CODE","BENEFICIARY ACCOUNT NUMBER","BENEFICIARY NAME", "SUBJECT", "STATUS", "SCRIPT MARKED",
        //        "SCRIPT RATE", "TOTAL", "RESP", "COORD", "TOTAL M/FEES", "CAPTURING",
        //        "TOTAL", "RATE", "TOTAL AMT ZIG", "WHT", "AMOUNT PAYABLE"
        //    };

        //        foreach (string header in headers)
        //        {
        //            Cell cell = new Cell().Add(new Paragraph(header).SetFontSize(7).SetHeight(10));
        //            table.AddHeaderCell(cell);
        //        }
              
        //        foreach (var item in tandsReports)
        //        {
        //            table.StartNewRow();
        //            table.AddCell(CreateCell(item.Date));
        //            table.AddCell(CreateCell(item.BankName));
        //            table.AddCell(CreateCell(item.ShortCode));
        //            table.AddCell(CreateCell(item.BankAccount));
        //            table.AddCell(CreateCell(item.Fullname));
        //            table.AddCell(CreateCell(item.Subject));
        //            table.AddCell(CreateCell(item.Status));
        //            table.AddCell(CreateCell(item.ScriptMarked));
        //            table.AddCell(CreateCell(item.ScriptRate));
        //            table.AddCell(CreateCell(item.TotalAfterScriptRate));
        //            table.AddCell(CreateCell(item.Resp));
        //            table.AddCell(CreateCell(item.Coord));
        //            table.AddCell(CreateCell(item.GrandTotal));
        //            table.AddCell(CreateCell(item.Capturing));
        //            table.AddCell(CreateCell(item.Total));
        //            table.AddCell(CreateCell(item.Rate));
        //            table.AddCell(CreateCell(item.ZIGAmount));
        //            table.AddCell(CreateCell(item.WHT));
        //            table.AddCell(CreateCell(item.AmountPayable));
                  
        //        }





        //        // Add the table to the document
        //        document.Add(table);


        //        // Close the document
        //        document.Close();

        //        return stream.ToArray();
        //    }

        //    // Helper method to create a cell with specified data


        //}


        //private Cell CreateCell(string? data)
        //{
        //    return new Cell().Add(new Paragraph(data ?? "").SetFontSize(6).SetHeight(10));
        //}




    }
}
