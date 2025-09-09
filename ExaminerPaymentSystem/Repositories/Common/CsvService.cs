using CsvHelper;
using ClosedXML.Excel;
using System.Globalization;
using System.Text;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.ViewModels.Examiners;

namespace ExaminerPaymentSystem.Repositories.Common
{
    public class CsvService
    {

        public string GenerateCSV(TandSReport tandSReport)
        {
            StringBuilder csv = new StringBuilder();

            // Add header for actual claim
            csv.AppendLine("Actual Claim Details");
            csv.AppendLine("Date,Departure,Arrival,Place(From/To),Busfare,Accommodation,Lunch,Total");

            // Add rows for actual claim
            foreach (var item in tandSReport.TANDSDETAILS)
            {
                csv.AppendLine($"{item.EMS_DATE},{item.EMS_DEPARTURE},{item.EMS_ARRIVAL},{item.EMS_PLACE},{item.EMS_BUSFARE},{item.EMS_ACCOMMODATION},{item.EMS_LUNCH},{item.EMS_TOTAL}");
            }

            // Add empty line between sections
            csv.AppendLine();

            // Add header for adjusted accounting
            csv.AppendLine("Adjusted Accounting Details");
            csv.AppendLine("Date,Departure,Arrival,Place(From/To),Busfare,Accommodation,Lunch,Total");

            // Add rows for adjusted accounting
            foreach (var item in tandSReport.TANDSDETAILS)
            {
                csv.AppendLine($"{item.EMS_DATE},{item.EMS_DEPARTURE},{item.EMS_ARRIVAL},{item.EMS_PLACE},{item.ADJ_BUSFARE},{item.ADJ_ACCOMMODATION},{item.ADJ_LUNCH},{item.ADJ_TOTAL}");
            }

            // Add empty line between sections
            csv.AppendLine();

            // Add Claim Details section
            csv.AppendLine("Claim Details");
            csv.AppendLine($"Full Name: {tandSReport.EMS_BANK_NAME_ZWL}");
            csv.AppendLine($"National ID: {tandSReport.EMS_NATIONAL_ID}");
            csv.AppendLine($"Address: {tandSReport.EMS_ADDRESS}");
            csv.AppendLine($"Phone: {tandSReport.EMS_PHONE_HOME}");
            csv.AppendLine($"Bank (NOSTRO): {tandSReport.EMS_BANK_NAME_FCA}");
            csv.AppendLine($"Account Number (NOSTRO): {tandSReport.EMS_ACCOUNT_NO_FCA}");
            csv.AppendLine($"Subject: {tandSReport.EMS_SUB_SUB_ID}");
            csv.AppendLine($"Level Of Examination: {tandSReport.EMS_LEVEL_OF_EXAM_MARKED}");
            csv.AppendLine($"Purpose Of Journey: {tandSReport.EMS_PURPOSEOFJOURNEY}");
            csv.AppendLine($"Bank (ZiG): {tandSReport.EMS_BANK_NAME_ZWL}");
            csv.AppendLine($"Account Number (ZiG): {tandSReport.EMS_ACCOUNT_NO_ZWL}");

            return csv.ToString();
        }


        public void ExportToCsv(List<TravelAdvanceReport> tandsReports, TandSAdvanceFees advanceFees, string filePath)
        {
            // Prepare the data in a format that CsvHelper can process
            List<dynamic> records = new List<dynamic>();

            // Add header row
            records.Add(new
            {
                STATUS = "STATUS",
                NAME = "NAME",
                BANK = "BANK",
                DAYS = "DAYS",
                TRANSIT_BUSFARE = "TRANSIT BUSFARE",
                LOCAL_BUSFARE = "LOCAL BUSFARE",
                TRANSIT_LUNCH = "TRANSIT LUNCH",
                CHECK_INN_ACC = "CHECK INN ACC",
                ACCOMMODATION = "ACCOMMODATION",
                B_FAST = "B/FAST",
                LUNCH_AND_DINNER = "LUNCH AND DINNER",
                CHECK_IN_DINNER = "CHECK IN DINNER",
                SUPP = "SUPP",
                TOTAL = "TOTAL",
                LESS_ADV = "LESS ADV",
                USD_BAL = "USD BAL",
                RATE = "RATE",
                ZIG_BALANCE = "ZIG BALANCE",
                ZIG_TICKET = "ZIG TICKET",
                ZIG_PAYMENT = "ZIG PAYMENT"
            });

            // Add data rows
            foreach (var item in tandsReports)
            {
                records.Add(new
                {
                    STATUS = item.Status,
                    NAME = item.Name,
                    BANK = item.BankZIG,
                    DAYS = item.Days,
                    TRANSIT_BUSFARE = item.TransitBusFare,
                    LOCAL_BUSFARE = item.LocalBusFare,
                    TRANSIT_LUNCH = item.TransitLunch,
                    CHECK_INN_ACC = item.CheckInnAccommodation,
                    ACCOMMODATION = item.Accommodation,
                    B_FAST = item.Breakfast,
                    LUNCH_AND_DINNER = item.LunchAndDinner,
                    CHECK_IN_DINNER = item.CheckInDinner,
                    SUPP = item.Supp,
                    TOTAL = item.Total,
                    LESS_ADV = item.LessAdvance,
                    USD_BAL = item.UsdBalance,
                    RATE = item.Rate,
                    ZIG_BALANCE = item.ZigBalance,
                    ZIG_TICKET = item.ZigTicket,
                    ZIG_PAYMENT = item.ZigPayment
                });
            }

            // Write data to CSV file
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
        }





        public Task ExportToExcel(List<TravelAdvanceReport> tandsReports, string filePath)
        {
            var tcs = new TaskCompletionSource<object>();

            Task.Run(() =>
            {
                // Create a new workbook
                using (var workbook = new XLWorkbook())
                {
                    // Add a worksheet
                    var worksheet = workbook.Worksheets.Add("Report");

                    // Add headers
                    string[] headers = {
                "STATUS", "NAME", "SUBJECT","BANK NAME ZWG","BRANCH CODE ZWG","BANK ACCOUNT NO ZWG", "BANK NAME USD","BRANCH CODE USD","BANK ACCOUNT NO USD","WORK ADDRESS","VENUE","DAYS", "TRANSIT BUSFARE", "LOCAL BUSFARE", "TRANSIT LUNCH",
                "CHECK INN ACC", "ACCOMMODATION", "B/FAST", "LUNCH AND DINNER", "CHECK IN DINNER", "SUPP",
                "TOTAL", "LESS ADV", "USD BAL", "RATE", "ZWG BALANCE", "ZWG TICKET", "ZWG PAYMENT"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                    }

                    // Add data rows
                    int row = 2;
                    foreach (var item in tandsReports)
                    {
                        worksheet.Cell(row, 1).Value = item.Status;
                        worksheet.Cell(row, 2).Value = item.Name;
                        worksheet.Cell(row,3).Value = item.Subject;
                        worksheet.Cell(row, 4).Value = item.BankZIG;
                        worksheet.Cell(row, 5).Value = item.BankBranchZIG;
                        worksheet.Cell(row, 6).Value = item.BankAccountZIG;
                        worksheet.Cell(row, 7).Value = item.BankUSD;
                        worksheet.Cell(row, 8).Value = item.BankBranchUSD;
                        worksheet.Cell(row, 9).Value = item.BankAccountUSD;
                        worksheet.Cell(row, 10).Value = item.WorkAddress;
                        worksheet.Cell(row, 11).Value = item.Venue;
                        worksheet.Cell(row, 12).Value = item.Days;
                        worksheet.Cell(row, 13).Value = item.TransitBusFare;
                        worksheet.Cell(row, 14).Value = item.LocalBusFare;
                        worksheet.Cell(row, 15).Value = item.TransitLunch;
                        worksheet.Cell(row, 16).Value = item.CheckInnAccommodation;
                        worksheet.Cell(row, 17).Value = item.Accommodation;
                        worksheet.Cell(row, 18).Value = item.Breakfast;
                        worksheet.Cell(row, 19).Value = item.LunchAndDinner;
                        worksheet.Cell(row, 20).Value = item.CheckInDinner;
                        worksheet.Cell(row, 21).Value = item.Supp;
                        worksheet.Cell(row, 22).Value = item.Total;
                        worksheet.Cell(row, 23).Value = item.LessAdvance;
                        worksheet.Cell(row, 24).Value = item.UsdBalance;
                        worksheet.Cell(row, 25).Value = item.Rate;
                        worksheet.Cell(row, 26).Value = item.ZigBalance;
                        worksheet.Cell(row, 27).Value = item.ZigTicket;
                        worksheet.Cell(row, 28).Value = item.ZigPayment;

                        row++;
                    }

                    // Save the workbook
                    workbook.SaveAs(filePath);
                }

                tcs.SetResult(null);
            });

            return tcs.Task;
        }



        public Task ExportToExcelExaminer(List<TravelExaminerMarkingReport> examinerReports, string filePath)
        {
            var tcs = new TaskCompletionSource<object>();

            Task.Run(() =>
            {
                // Create a new workbook
                using (var workbook = new XLWorkbook())
                {
                    // Add a worksheet
                    var worksheet = workbook.Worksheets.Add("Report");

                    // Add headers
                    string[] headers = {
                "DATE", "BANK NAME", "SORT CODE","BENEFICIARY ACCOUNT NUMBER", "BENEFICIARY NAME", "SUBJECT","VENUE", "STATUS", "SCRIPT MARKED",
                "SCRIPT RATE", "TOTAL", "RESP", "COORD", "TOTAL M/FEES", "CAPTURING",
                "TOTAL", "RATE", "TOTAL AMT ZWG", "WHT", "AMOUNT PAYABLE"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                    }

                    // Add data rows
                    int row = 2;
                    foreach (var item in examinerReports)
                    {
                        worksheet.Cell(row, 1).Value = item.Date;
                        worksheet.Cell(row, 2).Value = item.BankName;
                        worksheet.Cell(row, 3).Value = item.ShortCode;
                        worksheet.Cell(row,4).Value = item.BankAccount;
                        worksheet.Cell(row, 5).Value = item.Fullname;
                        worksheet.Cell(row, 6).Value = "'" + item.Subject;
                        worksheet.Cell(row, 7).Value = item.Venue;
                        worksheet.Cell(row, 8).Value = item.Status;
                        worksheet.Cell(row, 9).Value = item.ScriptMarked;
                        worksheet.Cell(row, 10).Value = item.ScriptRate;
                        worksheet.Cell(row, 11).Value = item.TotalAfterScriptRate;
                        worksheet.Cell(row, 12).Value = item.Resp;
                        worksheet.Cell(row, 13).Value = item.Coord;
                        worksheet.Cell(row, 14).Value = item.GrandTotal;
                        worksheet.Cell(row, 15).Value = item.Capturing;
                        worksheet.Cell(row, 16).Value = item.Total;
                        worksheet.Cell(row, 17).Value = item.Rate;
                        worksheet.Cell(row, 18).Value = item.ZIGAmount;
                        worksheet.Cell(row, 19).Value = item.WHT;
                        worksheet.Cell(row, 20).Value = item.AmountPayable;


                        row++;
                    }

                    // Save the workbook
                    workbook.SaveAs(filePath);
                }

                tcs.SetResult(null);
            });

            return tcs.Task;
        }

        public Task ExportBanksToExcel(List<MissingPeopleReportViewModel> tandsReports, string filePath)
        {
            var tcs = new TaskCompletionSource<object>();

            Task.Run(() =>
            {
                // Create a new workbook
                using (var workbook = new XLWorkbook())
                {
                    // Add a worksheet
                    var worksheet = workbook.Worksheets.Add("Report");

                    // Add headers
                    string[] headers = {
                "NAME", "NATIONALID","BANK NAME ZWG","BRANCH CODE ZWG","BANK ACCOUNT NO ZWG", "BANK NAME USD","BRANCH CODE USD","BANK ACCOUNT NO USD"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                    }

                    // Add data rows
                    int row = 2;
                    foreach (var item in tandsReports)
                    {
                      
                        worksheet.Cell(row, 1).Value = item.EMS_FullName;
                         worksheet.Cell(row, 2).Value = item.EMS_NATIONAL_ID;
                        worksheet.Cell(row, 3).Value = item.BankNamezig;
                        worksheet.Cell(row, 4).Value = item.Branchzig;
                        worksheet.Cell(row, 5).Value = item.Bankzig;


                        worksheet.Cell(row, 6).Value = item.BankNamefca;
                        worksheet.Cell(row, 7).Value = item.Branchfca;
                        worksheet.Cell(row, 8).Value = item.Bankfca;
                        

                        row++;
                    }

                    // Save the workbook
                    workbook.SaveAs(filePath);
                }

                tcs.SetResult(null);
            });

            return tcs.Task;
        }

    }
}
