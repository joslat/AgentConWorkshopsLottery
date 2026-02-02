# Implementation Plan 002: Excel Parser Service

**Phase:** 2 of 5  
**Estimated Effort:** Single session  
**Prerequisites:** Phase 1 complete (project setup, domain models)  
**Status:** ✅ COMPLETE

---

## 🎸 Completion Summary 🎸

| Feature | Status | Verified |
|---------|--------|----------|
| RawRegistration model | ✅ Implemented | ✅ |
| ColumnMapping record | ✅ Implemented | ✅ |
| ColumnMatcher class | ✅ Implemented | ✅ |
| ColumnMatchers (all 8) | ✅ Implemented | ✅ |
| IExcelParserService interface | ✅ Implemented | ✅ |
| ExcelParserService with ClosedXML | ✅ Implemented | ✅ |
| Fuzzy header matching | ✅ Implemented | ✅ |
| Required column validation | ✅ Implemented | ✅ |
| Optional column support | ✅ Implemented | ✅ |
| Empty row skipping | ✅ Implemented | ✅ |
| Whitespace trimming | ✅ Implemented | ✅ |
| Console logging of mappings | ✅ Implemented | ✅ |
| RawRegistrationTests | ✅ Implemented (3 tests) | ✅ |
| ColumnMatchersTests | ✅ Implemented (34 tests) | ✅ |
| ExcelParserServiceTests | ✅ Implemented (10 tests) | ✅ |
| Real sample file test | ✅ Verified (7 registrations parsed) | ✅ |

**Total Tests:** 142 passing ✅  
**Build Status:** ✅ Success  
**Real Data Test:** ✅ All 8 columns matched with sample file!

---

## Objective

Implement the Excel parsing service that reads Microsoft Forms exports, matches columns using fuzzy logic, and produces raw registration data.

---

## Tasks

### 1. Create Raw Registration Model

Create `src/WorkshopLottery/Models/RawRegistration.cs`:

```csharp
namespace WorkshopLottery.Models;

/// <summary>
/// Represents raw data extracted from Excel before validation.
/// </summary>
public record RawRegistration
{
    public int RowNumber { get; init; }
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? LaptopResponse { get; init; }
    public string? Commit10MinResponse { get; init; }
    public string? RequestedW1Response { get; init; }
    public string? RequestedW2Response { get; init; }
    public string? RequestedW3Response { get; init; }
    public string? RankingsResponse { get; init; }
}
```

### 2. Create Column Mapping Infrastructure

Create `src/WorkshopLottery/Infrastructure/ColumnMatcher.cs`:

```csharp
namespace WorkshopLottery.Infrastructure;

public record ColumnMapping
{
    public required string FieldName { get; init; }
    public int? ColumnIndex { get; init; }
    public string? MatchedHeader { get; init; }
}

public class ColumnMatcher
{
    public string FieldName { get; }
    public Func<string, bool> Matcher { get; }
    public bool IsRequired { get; }
    
    public ColumnMatcher(string fieldName, Func<string, bool> matcher, bool isRequired = true)
    {
        FieldName = fieldName;
        Matcher = matcher;
        IsRequired = isRequired;
    }
}
```

### 3. Define Column Matchers

Create `src/WorkshopLottery/Infrastructure/ColumnMatchers.cs`:

```csharp
namespace WorkshopLottery.Infrastructure;

public static class ColumnMatchers
{
    private static bool ContainsIgnoreCase(string header, string value) =>
        header.Contains(value, StringComparison.OrdinalIgnoreCase);
    
    public static readonly ColumnMatcher[] All = new[]
    {
        // Email must be checked before Name (both might contain "address")
        new ColumnMatcher("Email", 
            h => ContainsIgnoreCase(h, "email"), 
            isRequired: true),
        
        // Name - exclude email to avoid false match
        new ColumnMatcher("FullName", 
            h => ContainsIgnoreCase(h, "name") && !ContainsIgnoreCase(h, "email"), 
            isRequired: true),
        
        // Laptop requirement
        new ColumnMatcher("Laptop", 
            h => ContainsIgnoreCase(h, "laptop"), 
            isRequired: true),
        
        // Commit 10 minutes early
        new ColumnMatcher("Commit10Min", 
            h => ContainsIgnoreCase(h, "commit") || 
                 ContainsIgnoreCase(h, "10 min") || 
                 ContainsIgnoreCase(h, "before"), 
            isRequired: true),
        
        // Workshop 1 request
        new ColumnMatcher("RequestedW1", 
            h => ContainsIgnoreCase(h, "workshop 1"), 
            isRequired: false),
        
        // Workshop 2 request
        new ColumnMatcher("RequestedW2", 
            h => ContainsIgnoreCase(h, "workshop 2"), 
            isRequired: false),
        
        // Workshop 3 request
        new ColumnMatcher("RequestedW3", 
            h => ContainsIgnoreCase(h, "workshop 3"), 
            isRequired: false),
        
        // Rankings
        new ColumnMatcher("Rankings", 
            h => ContainsIgnoreCase(h, "rank"), 
            isRequired: false),
    };
}
```

### 4. Create Excel Parser Service Interface

Create `src/WorkshopLottery/Services/IExcelParserService.cs`:

```csharp
namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

public interface IExcelParserService
{
    IReadOnlyList<RawRegistration> ParseRegistrations(string filePath);
}
```

### 5. Implement Excel Parser Service

Create `src/WorkshopLottery/Services/ExcelParserService.cs`:

```csharp
namespace WorkshopLottery.Services;

using ClosedXML.Excel;
using WorkshopLottery.Infrastructure;
using WorkshopLottery.Models;

public class ExcelParserService : IExcelParserService
{
    public IReadOnlyList<RawRegistration> ParseRegistrations(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Input file not found: {filePath}");
        
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();
        
        var columnMappings = MapColumns(worksheet);
        ValidateRequiredColumns(columnMappings);
        LogColumnMappings(columnMappings);
        
        return ExtractRegistrations(worksheet, columnMappings);
    }
    
    private Dictionary<string, ColumnMapping> MapColumns(IXLWorksheet worksheet)
    {
        var headerRow = worksheet.Row(1);
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        var mappings = new Dictionary<string, ColumnMapping>();
        
        foreach (var matcher in ColumnMatchers.All)
        {
            mappings[matcher.FieldName] = new ColumnMapping
            {
                FieldName = matcher.FieldName,
                ColumnIndex = null,
                MatchedHeader = null
            };
        }
        
        for (int col = 1; col <= lastColumn; col++)
        {
            var header = headerRow.Cell(col).GetString().Trim();
            if (string.IsNullOrEmpty(header)) continue;
            
            foreach (var matcher in ColumnMatchers.All)
            {
                if (mappings[matcher.FieldName].ColumnIndex.HasValue)
                    continue; // Already mapped
                
                if (matcher.Matcher(header))
                {
                    mappings[matcher.FieldName] = new ColumnMapping
                    {
                        FieldName = matcher.FieldName,
                        ColumnIndex = col,
                        MatchedHeader = header
                    };
                    break;
                }
            }
        }
        
        return mappings;
    }
    
    private void ValidateRequiredColumns(Dictionary<string, ColumnMapping> mappings)
    {
        var missing = ColumnMatchers.All
            .Where(m => m.IsRequired && !mappings[m.FieldName].ColumnIndex.HasValue)
            .Select(m => m.FieldName)
            .ToList();
        
        if (missing.Any())
        {
            throw new InvalidOperationException(
                $"Required columns not found: {string.Join(", ", missing)}");
        }
    }
    
    private void LogColumnMappings(Dictionary<string, ColumnMapping> mappings)
    {
        Console.WriteLine("Column mappings:");
        foreach (var mapping in mappings.Values)
        {
            if (mapping.ColumnIndex.HasValue)
                Console.WriteLine($"  {mapping.FieldName} → Column {mapping.ColumnIndex} \"{mapping.MatchedHeader}\"");
            else
                Console.WriteLine($"  {mapping.FieldName} → Not found");
        }
    }
    
    private List<RawRegistration> ExtractRegistrations(
        IXLWorksheet worksheet, 
        Dictionary<string, ColumnMapping> mappings)
    {
        var registrations = new List<RawRegistration>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        
        for (int row = 2; row <= lastRow; row++)
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
        
        return registrations;
    }
    
    private string? GetCellValue(IXLWorksheet worksheet, int row, ColumnMapping mapping)
    {
        if (!mapping.ColumnIndex.HasValue)
            return null;
        
        var value = worksheet.Cell(row, mapping.ColumnIndex.Value).GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
```

### 6. Add Unit Tests

Create `tests/WorkshopLottery.Tests/Services/ExcelParserServiceTests.cs`:

**Test Cases:**
- `ParseRegistrations_WithValidFile_ReturnsRegistrations`
- `ParseRegistrations_WithMissingFile_ThrowsFileNotFoundException`
- `ParseRegistrations_MissingRequiredColumn_ThrowsInvalidOperationException`
- `ParseRegistrations_SkipsEmptyRows`
- `ParseRegistrations_TrimsWhitespace`

Create `tests/WorkshopLottery.Tests/Infrastructure/ColumnMatchersTests.cs`:

**Test Cases:**
- `EmailMatcher_MatchesVariousHeaders` (test "Email", "Email address", etc.)
- `NameMatcher_MatchesNameNotEmail` (test "Full name" matches, "Email address" doesn't)
- `LaptopMatcher_MatchesVariousHeaders`
- `Commit10MinMatcher_MatchesVariousHeaders`
- `WorkshopMatchers_MatchCorrectWorkshops`
- `RankingsMatcher_MatchesVariousHeaders`

### 7. Create Test Excel File

Create a test fixture Excel file for unit tests:
`tests/WorkshopLottery.Tests/TestData/valid-registrations.xlsx`

With sample data covering:
- Normal registrations
- Missing optional fields
- Various header variations

---

## Verification

1. **Build succeeds:**
   ```bash
   dotnet build
   ```

2. **Tests pass:**
   ```bash
   dotnet test --filter "ExcelParser|ColumnMatcher"
   ```

3. **Manual test with real sample file:**
   ```csharp
   var parser = new ExcelParserService();
   var registrations = parser.ParseRegistrations("input/AgentCon Zurich – Workshop Signup (Lottery + Standby)(1-7).xlsx");
   Console.WriteLine($"Parsed {registrations.Count} registrations");
   ```

4. **Validate column matching with analysis tool:**
   ```bash
   dotnet run --project tools/ExcelAnalyzer
   ```

---

## Architecture Alignment Check

| Requirement | Status |
|-------------|--------|
| Fuzzy column matching (ADR-005) | ✅ Aligned |
| ClosedXML usage (ADR-002) | ✅ Aligned |
| IExcelParserService interface (ARCHITECTURE.md §4.3) | ✅ Aligned |
| Column mapping strategy (ARCHITECTURE.md §8) | ✅ Aligned |

---

## Handoff to Next Phase

After completing this phase, the following will be ready:
- Excel files can be parsed into raw registrations
- Column matching is robust and tested
- Foundation for Phase 3 (Validation service)
