using ClosedXML.Excel;
using WorkshopLottery.Models;

namespace WorkshopLottery.Services;

/// <summary>
/// Service for writing lottery results to Excel with one sheet per workshop.
/// </summary>
public class ExcelWriterService : IExcelWriterService
{
    private static readonly string[] Headers = new[]
    {
        "Order", "Status", "Wave", "Name", "Email",
        "Laptop", "Commit10Min", "Requested", "Rank", "Weight", "Seed"
    };

    /// <summary>
    /// Writes the lottery results to an Excel file.
    /// </summary>
    public void WriteResults(string filePath, LotteryResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(result);
        
        using var workbook = new XLWorkbook();

        // Add summary sheet first
        WriteSummarySheet(workbook, result);

        foreach (var (workshopId, workshopResult) in result.Results.OrderBy(kvp => kvp.Key))
        {
            var sheetName = workshopId.ToString();  // W1, W2, W3
            var worksheet = workbook.Worksheets.Add(sheetName);

            WriteHeader(worksheet);
            WriteAssignments(worksheet, workshopResult, workshopId, result.Seed);
            FormatWorksheet(worksheet, workshopResult.Assignments.Count);
        }

        workbook.SaveAs(filePath);
    }

    /// <summary>
    /// Writes the header row with column names.
    /// </summary>
    private static void WriteHeader(IXLWorksheet worksheet)
    {
        for (int i = 0; i < Headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = Headers[i];
        }
    }

    /// <summary>
    /// Writes all assignments for a workshop.
    /// </summary>
    private static void WriteAssignments(
        IXLWorksheet worksheet,
        WorkshopResult workshopResult,
        WorkshopId workshopId,
        int seed)
    {
        var row = 2;

        // Write accepted: Wave 1 first, then Wave 2 (in order within each wave)
        var accepted = workshopResult.Accepted
            .OrderBy(a => a.Wave)
            .ThenBy(a => a.Order);

        foreach (var assignment in accepted)
        {
            WriteAssignmentRow(worksheet, row++, assignment, workshopId, seed);
        }

        // Write waitlisted (in order)
        var waitlisted = workshopResult.Waitlisted.OrderBy(a => a.Order);

        foreach (var assignment in waitlisted)
        {
            WriteAssignmentRow(worksheet, row++, assignment, workshopId, seed);
        }
    }

    /// <summary>
    /// Writes a single assignment row.
    /// </summary>
    private static void WriteAssignmentRow(
        IXLWorksheet worksheet,
        int row,
        WorkshopAssignment assignment,
        WorkshopId workshopId,
        int seed)
    {
        var reg = assignment.Registration;
        var pref = reg.WorkshopPreferences.GetValueOrDefault(workshopId);

        worksheet.Cell(row, 1).Value = assignment.Order;
        worksheet.Cell(row, 2).Value = assignment.Status.ToString();
        worksheet.Cell(row, 3).Value = assignment.Wave?.ToString() ?? "";
        worksheet.Cell(row, 4).Value = reg.FullName;
        worksheet.Cell(row, 5).Value = reg.Email;
        worksheet.Cell(row, 6).Value = reg.HasLaptop ? "Yes" : "No";
        worksheet.Cell(row, 7).Value = reg.WillCommit10Min ? "Yes" : "No";
        worksheet.Cell(row, 8).Value = pref?.Requested == true ? "Yes" : "No";
        worksheet.Cell(row, 9).Value = pref?.Rank?.ToString() ?? "";
        worksheet.Cell(row, 10).Value = pref?.Weight ?? 0;
        worksheet.Cell(row, 11).Value = seed;

        // Color-code by status
        var rowRange = worksheet.Range(row, 1, row, Headers.Length);
        rowRange.Style.Fill.BackgroundColor = assignment.Status switch
        {
            AssignmentStatus.Accepted when assignment.Wave == 1 => XLColor.LightGreen,
            AssignmentStatus.Accepted when assignment.Wave == 2 => XLColor.LightYellow,
            AssignmentStatus.Waitlisted => XLColor.LightGray,
            _ => XLColor.NoColor
        };
    }

    /// <summary>
    /// Applies formatting to the worksheet.
    /// </summary>
    private static void FormatWorksheet(IXLWorksheet worksheet, int dataRowCount)
    {
        // Bold header row
        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Freeze header row
        worksheet.SheetView.FreezeRows(1);

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Minimum column widths for readability
        foreach (var col in worksheet.ColumnsUsed())
        {
            if (col.Width < 10)
                col.Width = 10;
        }

        // Add borders
        if (dataRowCount > 0)
        {
            var dataRange = worksheet.Range(1, 1, dataRowCount + 1, Headers.Length);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }
    }

    /// <summary>
    /// Writes a summary sheet with overall statistics.
    /// </summary>
    private static void WriteSummarySheet(XLWorkbook workbook, LotteryResult result)
    {
        var worksheet = workbook.Worksheets.Add("Summary");

        var row = 1;

        // Title
        worksheet.Cell(row, 1).Value = "Workshop Lottery Results";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Font.FontSize = 16;
        row += 2;

        // Metadata
        worksheet.Cell(row, 1).Value = "Random Seed:";
        worksheet.Cell(row, 2).Value = result.Seed;
        row++;

        worksheet.Cell(row, 1).Value = "Capacity per Workshop:";
        worksheet.Cell(row, 2).Value = result.Capacity;
        row++;

        worksheet.Cell(row, 1).Value = "Total Registrations:";
        worksheet.Cell(row, 2).Value = result.TotalRegistrations;
        row++;

        worksheet.Cell(row, 1).Value = "Eligible:";
        worksheet.Cell(row, 2).Value = result.EligibleCount;
        row++;

        worksheet.Cell(row, 1).Value = "Disqualified:";
        worksheet.Cell(row, 2).Value = result.DisqualifiedCount;
        row += 2;

        // Disqualification reasons
        if (result.DisqualificationReasons != null && result.DisqualificationReasons.Count > 0)
        {
            worksheet.Cell(row, 1).Value = "Disqualification Reasons:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            foreach (var (reason, count) in result.DisqualificationReasons.OrderByDescending(r => r.Value))
            {
                worksheet.Cell(row, 1).Value = $"  {reason}:";
                worksheet.Cell(row, 2).Value = count;
                row++;
            }
            row++;
        }

        // Per-workshop results
        worksheet.Cell(row, 1).Value = "Results by Workshop:";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row++;

        worksheet.Cell(row, 1).Value = "Workshop";
        worksheet.Cell(row, 2).Value = "Accepted";
        worksheet.Cell(row, 3).Value = "Wave 1";
        worksheet.Cell(row, 4).Value = "Wave 2";
        worksheet.Cell(row, 5).Value = "Waitlist";
        worksheet.Range(row, 1, row, 5).Style.Font.Bold = true;
        row++;

        foreach (var (workshopId, workshopResult) in result.Results.OrderBy(kvp => kvp.Key))
        {
            worksheet.Cell(row, 1).Value = workshopId.ToString();
            worksheet.Cell(row, 2).Value = workshopResult.AcceptedCount;
            worksheet.Cell(row, 3).Value = workshopResult.Wave1Count;
            worksheet.Cell(row, 4).Value = workshopResult.Wave2Count;
            worksheet.Cell(row, 5).Value = workshopResult.WaitlistCount;
            row++;
        }

        // Format column A
        worksheet.Column(1).Style.Font.Bold = true;
        worksheet.Columns().AdjustToContents();
    }
}
