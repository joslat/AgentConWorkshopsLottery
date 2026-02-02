using ClosedXML.Excel;
using System.Text.Json;

namespace ExcelAnalyzer;

class Program
{
    static void Main(string[] args)
    {
        var excelPath = @"c:\git\joslat\AgentConWorkshopsLottery\input\AgentCon Zurich – Workshop Signup (Lottery + Standby)(1-7).xlsx";
        
        if (!File.Exists(excelPath))
        {
            Console.WriteLine($"Excel file not found: {excelPath}");
            return;
        }

        try
        {
            AnalyzeExcelFile(excelPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing Excel file: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    static void AnalyzeExcelFile(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        
        Console.WriteLine("=== EXCEL FILE ANALYSIS ===");
        Console.WriteLine($"File: {filePath}");
        Console.WriteLine($"Number of worksheets: {workbook.Worksheets.Count}");
        Console.WriteLine();

        foreach (var worksheet in workbook.Worksheets)
        {
            Console.WriteLine($"=== WORKSHEET: {worksheet.Name} ===");
            
            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
            {
                Console.WriteLine("No data found in worksheet");
                Console.WriteLine();
                continue;
            }

            Console.WriteLine($"Used range: {usedRange.RangeAddress}");
            Console.WriteLine($"Number of rows (including header): {usedRange.RowCount()}");
            Console.WriteLine($"Number of columns: {usedRange.ColumnCount()}");
            Console.WriteLine();

            // Get column headers from first row
            var headerRow = worksheet.Row(1);
            var headers = new List<string>();
            
            Console.WriteLine("=== COLUMN HEADERS ===");
            for (int col = 1; col <= usedRange.ColumnCount(); col++)
            {
                var headerCell = headerRow.Cell(col);
                var headerText = headerCell.GetString().Trim();
                headers.Add(headerText);
                Console.WriteLine($"{col:D2}: {headerText}");
            }
            Console.WriteLine();

            // Analyze fuzzy matching patterns
            Console.WriteLine("=== FUZZY MATCHING ANALYSIS ===");
            AnalyzeColumnMatchingPatterns(headers);
            Console.WriteLine();

            // Sample a few data rows (without exposing personal info)
            Console.WriteLine("=== DATA SAMPLE ANALYSIS ===");
            AnalyzeDataSamples(worksheet, headers, Math.Min(5, usedRange.RowCount() - 1));
            Console.WriteLine();

            // Only analyze first worksheet for now
            break;
        }
    }

    static void AnalyzeColumnMatchingPatterns(List<string> headers)
    {
        // Based on ADR-005 fuzzy matching rules
        var patterns = new Dictionary<string, Func<string, bool>>
        {
            ["Email"] = h => h.Contains("email", StringComparison.OrdinalIgnoreCase),
            ["FullName"] = h => h.Contains("name", StringComparison.OrdinalIgnoreCase) 
                              && !h.Contains("email", StringComparison.OrdinalIgnoreCase),
            ["HasLaptop"] = h => h.Contains("laptop", StringComparison.OrdinalIgnoreCase),
            ["WillCommit10Min"] = h => h.Contains("commit", StringComparison.OrdinalIgnoreCase) 
                                    || h.Contains("10 min", StringComparison.OrdinalIgnoreCase)
                                    || h.Contains("early", StringComparison.OrdinalIgnoreCase),
            ["RequestedW1"] = h => h.Contains("workshop 1", StringComparison.OrdinalIgnoreCase),
            ["RequestedW2"] = h => h.Contains("workshop 2", StringComparison.OrdinalIgnoreCase),
            ["RequestedW3"] = h => h.Contains("workshop 3", StringComparison.OrdinalIgnoreCase),
            ["Rankings"] = h => h.Contains("rank", StringComparison.OrdinalIgnoreCase)
        };

        foreach (var (expectedField, matcher) in patterns)
        {
            var matchedHeaders = headers.Where((h, i) => matcher(h)).Select((h, i) => $"Col {headers.IndexOf(h)+1}: \"{h}\"");
            
            if (matchedHeaders.Any())
            {
                Console.WriteLine($"{expectedField} matches: {string.Join(", ", matchedHeaders)}");
            }
            else
            {
                Console.WriteLine($"{expectedField}: NO MATCH FOUND");
            }
        }

        // Check for unmatched columns
        var unmatchedHeaders = headers.Where(h => !patterns.Values.Any(matcher => matcher(h))).ToList();
        if (unmatchedHeaders.Any())
        {
            Console.WriteLine($"Unmatched columns: {string.Join(", ", unmatchedHeaders.Select((h, i) => $"Col {headers.IndexOf(h)+1}: \"{h}\""))}");
        }
    }

    static void AnalyzeDataSamples(IXLWorksheet worksheet, List<string> headers, int sampleRows)
    {
        Console.WriteLine($"Analyzing {sampleRows} sample rows...");
        
        for (int row = 2; row <= sampleRows + 1; row++) // Start from row 2 (skip header)
        {
            Console.WriteLine($"--- Sample Row {row - 1} ---");
            
            for (int col = 1; col <= headers.Count; col++)
            {
                var cell = worksheet.Cell(row, col);
                var value = cell.GetString().Trim();
                var header = headers[col - 1];
                
                // Anonymize potentially sensitive data
                var displayValue = AnonymizeValue(header, value);
                
                Console.WriteLine($"{header}: {displayValue}");
            }
            Console.WriteLine();
        }
    }

    static string AnonymizeValue(string header, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "[Empty]";
            
        var lowerHeader = header.ToLowerInvariant();
        
        if (lowerHeader.Contains("name"))
        {
            return $"[Name: {value.Length} chars]";
        }
        
        if (lowerHeader.Contains("email"))
        {
            return value.Contains("@") ? "[Email: valid format]" : "[Email: invalid format]";
        }
        
        // For other fields, show first 50 chars
        return value.Length > 50 ? $"{value[..50]}..." : value;
    }
}