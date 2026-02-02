using ClosedXML.Excel;
using WorkshopLottery.Infrastructure;
using WorkshopLottery.Models;

namespace WorkshopLottery.Services;

/// <summary>
/// Implementation of Excel parser service using ClosedXML.
/// Implements ADR-002: ClosedXML for Excel and ADR-005: Fuzzy Column Matching.
/// </summary>
public class ExcelParserService : IExcelParserService
{
    /// <summary>
    /// Parses an Excel file and extracts raw registration data.
    /// </summary>
    public IReadOnlyList<RawRegistration> ParseRegistrations(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Input file not found: {filePath}", filePath);

        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();

        var columnMappings = MapColumns(worksheet);
        ValidateRequiredColumns(columnMappings);
        LogColumnMappings(columnMappings);

        return ExtractRegistrations(worksheet, columnMappings);
    }

    /// <summary>
    /// Maps column headers to logical field names using fuzzy matching.
    /// </summary>
    private Dictionary<string, ColumnMapping> MapColumns(IXLWorksheet worksheet)
    {
        var headerRow = worksheet.Row(1);
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        var mappings = new Dictionary<string, ColumnMapping>();

        // Initialize all mappings as not found
        foreach (var matcher in ColumnMatchers.All)
        {
            mappings[matcher.FieldName] = new ColumnMapping
            {
                FieldName = matcher.FieldName,
                ColumnIndex = null,
                MatchedHeader = null
            };
        }

        // Scan header row and match columns
        for (int col = 1; col <= lastColumn; col++)
        {
            var header = headerRow.Cell(col).GetString().Trim();
            if (string.IsNullOrEmpty(header)) continue;

            foreach (var matcher in ColumnMatchers.All)
            {
                // Skip if already mapped
                if (mappings[matcher.FieldName].ColumnIndex.HasValue)
                    continue;

                if (matcher.Matcher(header))
                {
                    mappings[matcher.FieldName] = new ColumnMapping
                    {
                        FieldName = matcher.FieldName,
                        ColumnIndex = col,
                        MatchedHeader = header
                    };
                    break; // Each column maps to at most one field
                }
            }
        }

        return mappings;
    }

    /// <summary>
    /// Validates that all required columns were found.
    /// </summary>
    private void ValidateRequiredColumns(Dictionary<string, ColumnMapping> mappings)
    {
        var missing = ColumnMatchers.All
            .Where(m => m.IsRequired && !mappings[m.FieldName].ColumnIndex.HasValue)
            .Select(m => m.FieldName)
            .ToList();

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Required columns not found: {string.Join(", ", missing)}. " +
                "Please verify the Excel file has the expected column headers.");
        }
    }

    /// <summary>
    /// Logs the column mappings for debugging and verification.
    /// </summary>
    private static void LogColumnMappings(Dictionary<string, ColumnMapping> mappings)
    {
        Console.WriteLine("📊 Column mappings:");
        foreach (var mapping in mappings.Values.OrderBy(m => m.ColumnIndex ?? int.MaxValue))
        {
            if (mapping.ColumnIndex.HasValue)
            {
                Console.WriteLine($"   ✅ {mapping.FieldName} → Column {mapping.ColumnIndex} \"{mapping.MatchedHeader}\"");
            }
            else
            {
                Console.WriteLine($"   ⚠️ {mapping.FieldName} → Not found");
            }
        }
    }

    /// <summary>
    /// Extracts registration data from the worksheet using the column mappings.
    /// </summary>
    private List<RawRegistration> ExtractRegistrations(
        IXLWorksheet worksheet,
        Dictionary<string, ColumnMapping> mappings)
    {
        var registrations = new List<RawRegistration>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (int row = 2; row <= lastRow; row++) // Start from row 2 (skip header)
        {
            var registration = new RawRegistration
            {
                RowNumber = row,
                FullName = GetCellValue(worksheet, row, mappings["FullName"]),
                Email = GetCellValue(worksheet, row, mappings["Email"]),
                LaptopResponse = GetCellValue(worksheet, row, mappings["Laptop"]),
                Commit10MinResponse = GetCellValue(worksheet, row, mappings["Commit10Min"]),
                RequestedW1Response = GetCellValue(worksheet, row, mappings["RequestedW1"]),
                RequestedW2Response = GetCellValue(worksheet, row, mappings["RequestedW2"]),
                RequestedW3Response = GetCellValue(worksheet, row, mappings["RequestedW3"]),
                RankingsResponse = GetCellValue(worksheet, row, mappings["Rankings"]),
            };

            // Skip completely empty rows
            if (!string.IsNullOrWhiteSpace(registration.Email) ||
                !string.IsNullOrWhiteSpace(registration.FullName))
            {
                registrations.Add(registration);
            }
        }

        Console.WriteLine($"📋 Extracted {registrations.Count} registrations from {lastRow - 1} data rows");
        return registrations;
    }

    /// <summary>
    /// Gets a cell value from the worksheet, trimmed and normalized.
    /// </summary>
    private static string? GetCellValue(IXLWorksheet worksheet, int row, ColumnMapping mapping)
    {
        if (!mapping.ColumnIndex.HasValue)
            return null;

        var value = worksheet.Cell(row, mapping.ColumnIndex.Value).GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
