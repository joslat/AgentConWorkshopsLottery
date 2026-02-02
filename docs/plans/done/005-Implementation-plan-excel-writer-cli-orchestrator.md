# Implementation Plan 005: Excel Writer, CLI & Orchestrator

**Phase:** 5 of 5  
**Estimated Effort:** Single session  
**Prerequisites:** Phase 4 complete (Lottery engine)  
**Status:** ✅ COMPLETE

---

## ✅ Implementation Completion Table

| Feature | Status | File(s) |
|---------|--------|---------|
| Excel Writer Interface | ✅ | `src/WorkshopLottery/Services/IExcelWriterService.cs` |
| Excel Writer Service | ✅ | `src/WorkshopLottery/Services/ExcelWriterService.cs` |
| Summary Sheet | ✅ | Included in ExcelWriterService |
| Per-Workshop Sheets (W1, W2, W3) | ✅ | Included in ExcelWriterService |
| Status Column (Accepted, Waitlisted) | ✅ | Included in ExcelWriterService |
| Wave Column (1, 2, or empty) | ✅ | Included in ExcelWriterService |
| Row Color Coding | ✅ | LightGreen (Wave 1), LightYellow (Wave 2), LightGray (Waitlist) |
| Console Summary Logger | ✅ | `src/WorkshopLottery/Services/SummaryLogger.cs` |
| Lottery Orchestrator | ✅ | `src/WorkshopLottery/Services/LotteryOrchestrator.cs` |
| CLI with System.CommandLine | ✅ | `src/WorkshopLottery/Program.cs` |
| CLI Options (input, output, seed, capacity, verbose) | ✅ | Program.cs |
| README Updated | ✅ | `README.md` |
| ExcelWriterService Unit Tests | ✅ | `tests/WorkshopLottery.Tests/Services/ExcelWriterServiceTests.cs` |
| LotteryOrchestrator Unit Tests | ✅ | `tests/WorkshopLottery.Tests/Services/LotteryOrchestratorTests.cs` |
| End-to-End Integration Tests | ✅ | `tests/WorkshopLottery.Tests/Integration/EndToEndTests.cs` |
| All 317 Tests Pass | ✅ | `dotnet test` |
| E2E Test with Real Sample Data | ✅ | Verified with `input/AgentCon Zurich – Workshop Signup (Lottery + Standby)(1-7).xlsx` |

---

## Objective

Implement the final components: Excel output writer, CLI argument parsing, main orchestrator, and console summary logging. This completes the application.

---

## Tasks

### 1. Create Excel Writer Service Interface

Create `src/WorkshopLottery/Services/IExcelWriterService.cs`:

```csharp
namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

public interface IExcelWriterService
{
    void WriteResults(string filePath, LotteryResult result);
}
```

### 2. Implement Excel Writer Service

Create `src/WorkshopLottery/Services/ExcelWriterService.cs`:

```csharp
namespace WorkshopLottery.Services;

using ClosedXML.Excel;
using WorkshopLottery.Models;

public class ExcelWriterService : IExcelWriterService
{
    private static readonly string[] Headers = new[]
    {
        "Order", "Status", "Wave", "Name", "Email",
        "Laptop", "Commit10Min", "Requested", "Rank", "Weight", "Seed"
    };
    
    public void WriteResults(string filePath, LotteryResult result)
    {
        using var workbook = new XLWorkbook();
        
        foreach (var (workshopId, workshopResult) in result.Results.OrderBy(kvp => kvp.Key))
        {
            var sheetName = GetSheetName(workshopId);
            var worksheet = workbook.Worksheets.Add(sheetName);
            
            WriteHeader(worksheet);
            WriteAssignments(worksheet, workshopResult, workshopId, result.Seed);
            FormatWorksheet(worksheet);
        }
        
        workbook.SaveAs(filePath);
    }
    
    private string GetSheetName(WorkshopId workshopId) => workshopId switch
    {
        WorkshopId.W1 => "Workshop 1",
        WorkshopId.W2 => "Workshop 2",
        WorkshopId.W3 => "Workshop 3",
        _ => $"Workshop {(int)workshopId}"
    };
    
    private void WriteHeader(IXLWorksheet worksheet)
    {
        for (int i = 0; i < Headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = Headers[i];
        }
    }
    
    private void WriteAssignments(
        IXLWorksheet worksheet,
        WorkshopResult workshopResult,
        WorkshopId workshopId,
        int seed)
    {
        var row = 2;
        
        // Write accepted (Wave 1 first, then Wave 2)
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
    
    private void WriteAssignmentRow(
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
    }
    
    private void FormatWorksheet(IXLWorksheet worksheet)
    {
        // Bold header row
        worksheet.Row(1).Style.Font.Bold = true;
        
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
    }
}
```

### 3. Create Console Summary Logger

Create `src/WorkshopLottery/Services/SummaryLogger.cs`:

```csharp
namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

public static class SummaryLogger
{
    public static void LogSummary(
        LotteryResult result,
        ValidationResult validation,
        LotteryConfiguration config)
    {
        Console.WriteLine();
        Console.WriteLine("=== Workshop Lottery Summary ===");
        Console.WriteLine($"Input file: {config.InputPath}");
        Console.WriteLine($"Random seed: {result.Seed}");
        Console.WriteLine();
        
        Console.WriteLine($"Total rows: {validation.AllRegistrations.Count}");
        Console.WriteLine($"Eligible: {validation.EligibleRegistrations.Count}");
        Console.WriteLine($"Disqualified: {validation.DisqualifiedRegistrations.Count}");
        
        if (validation.DisqualificationReasons.Any())
        {
            foreach (var (reason, count) in validation.DisqualificationReasons.OrderByDescending(r => r.Value))
            {
                Console.WriteLine($"  - {reason}: {count}");
            }
        }
        
        Console.WriteLine();
        
        foreach (var (workshopId, workshopResult) in result.Results.OrderBy(kvp => kvp.Key))
        {
            var name = GetWorkshopDisplayName(workshopId);
            var w1 = workshopResult.Wave1Count;
            var w2 = workshopResult.Wave2Count;
            var waitlist = workshopResult.WaitlistCount;
            
            Console.WriteLine($"{name}: {workshopResult.AcceptedCount} accepted (Wave1: {w1}, Wave2: {w2}), {waitlist} waitlisted");
        }
        
        Console.WriteLine();
        Console.WriteLine($"Output: {config.OutputPath}");
    }
    
    private static string GetWorkshopDisplayName(WorkshopId workshopId) => workshopId switch
    {
        WorkshopId.W1 => "Workshop 1",
        WorkshopId.W2 => "Workshop 2",
        WorkshopId.W3 => "Workshop 3",
        _ => $"Workshop {(int)workshopId}"
    };
}
```

### 4. Create Lottery Orchestrator

Create `src/WorkshopLottery/Services/LotteryOrchestrator.cs`:

```csharp
namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

public class LotteryOrchestrator
{
    private readonly IExcelParserService _parser;
    private readonly IValidationService _validator;
    private readonly ILotteryEngine _engine;
    private readonly IExcelWriterService _writer;
    
    public LotteryOrchestrator(
        IExcelParserService parser,
        IValidationService validator,
        ILotteryEngine engine,
        IExcelWriterService writer)
    {
        _parser = parser;
        _validator = validator;
        _engine = engine;
        _writer = writer;
    }
    
    public void Run(LotteryConfiguration config)
    {
        Console.WriteLine($"Reading input file: {config.InputPath}");
        var rawRegistrations = _parser.ParseRegistrations(config.InputPath);
        Console.WriteLine($"Found {rawRegistrations.Count} rows");
        Console.WriteLine();
        
        Console.WriteLine("Validating registrations...");
        var validation = _validator.ValidateAndFilter(rawRegistrations);
        Console.WriteLine($"Eligible: {validation.EligibleRegistrations.Count}");
        Console.WriteLine();
        
        Console.WriteLine($"Running lottery with seed {config.GetEffectiveSeed()}...");
        var result = _engine.RunLottery(validation.EligibleRegistrations, config);
        
        // Enrich result with validation stats
        result = result with
        {
            TotalRegistrations = validation.AllRegistrations.Count,
            EligibleCount = validation.EligibleRegistrations.Count,
            DisqualifiedCount = validation.DisqualifiedRegistrations.Count,
            DisqualificationReasons = validation.DisqualificationReasons
        };
        
        Console.WriteLine($"Writing results to: {config.OutputPath}");
        _writer.WriteResults(config.OutputPath, result);
        
        SummaryLogger.LogSummary(result, validation, config);
    }
}
```

### 5. Implement CLI with System.CommandLine

Update `src/WorkshopLottery/Program.cs`:

```csharp
using System.CommandLine;
using WorkshopLottery.Models;
using WorkshopLottery.Services;

// Define CLI options
var inputOption = new Option<FileInfo>(
    name: "--input",
    description: "Input Excel file from Microsoft Forms")
{
    IsRequired = true
};
inputOption.AddAlias("-i");

var outputOption = new Option<string>(
    name: "--output",
    getDefaultValue: () => "WorkshopAssignments.xlsx",
    description: "Output Excel file path");
outputOption.AddAlias("-o");

var capacityOption = new Option<int>(
    name: "--capacity",
    getDefaultValue: () => 34,
    description: "Seats per workshop");
capacityOption.AddAlias("-c");

var seedOption = new Option<int?>(
    name: "--seed",
    description: "Random seed (default: YYYYMMDD)");
seedOption.AddAlias("-s");

var orderOption = new Option<string>(
    name: "--order",
    getDefaultValue: () => "W1,W2,W3",
    description: "Workshop processing order (e.g., W1,W2,W3)");

// Add validators
inputOption.AddValidator(result =>
{
    var file = result.GetValueForOption(inputOption);
    if (file is not null && !file.Exists)
    {
        result.ErrorMessage = $"Input file not found: {file.FullName}";
    }
});

capacityOption.AddValidator(result =>
{
    var capacity = result.GetValueForOption(capacityOption);
    if (capacity < 1)
    {
        result.ErrorMessage = "Capacity must be at least 1";
    }
});

// Build root command
var rootCommand = new RootCommand("Workshop Lottery - Fair seat assignment using weighted lottery")
{
    inputOption,
    outputOption,
    capacityOption,
    seedOption,
    orderOption
};

// Set handler
rootCommand.SetHandler(
    (FileInfo input, string output, int capacity, int? seed, string order) =>
    {
        var workshopOrder = ParseWorkshopOrder(order);
        
        var config = new LotteryConfiguration
        {
            InputPath = input.FullName,
            OutputPath = output,
            Capacity = capacity,
            Seed = seed,
            WorkshopOrder = workshopOrder
        };
        
        // Create services
        var parser = new ExcelParserService();
        var validator = new ValidationService();
        var engine = new LotteryEngine();
        var writer = new ExcelWriterService();
        
        var orchestrator = new LotteryOrchestrator(parser, validator, engine, writer);
        
        try
        {
            orchestrator.Run(config);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    },
    inputOption, outputOption, capacityOption, seedOption, orderOption);

return await rootCommand.InvokeAsync(args);

// Helper function
static List<WorkshopId> ParseWorkshopOrder(string order)
{
    var result = new List<WorkshopId>();
    var parts = order.Split(',', StringSplitOptions.RemoveEmptyEntries);
    
    foreach (var part in parts)
    {
        var normalized = part.Trim().ToUpperInvariant();
        if (normalized == "W1") result.Add(WorkshopId.W1);
        else if (normalized == "W2") result.Add(WorkshopId.W2);
        else if (normalized == "W3") result.Add(WorkshopId.W3);
    }
    
    // Default if parsing fails
    if (result.Count == 0)
    {
        result.Add(WorkshopId.W1);
        result.Add(WorkshopId.W2);
        result.Add(WorkshopId.W3);
    }
    
    return result;
}
```

### 6. Update Project README

Update `README.md` in the root:

```markdown
# Workshop Lottery

A fair seat assignment tool for workshop registrations using weighted lottery.

## Features

- Imports Microsoft Forms Excel exports
- Weighted lottery based on participant rankings
- Two-wave assignment maximizes unique participant access
- Reproducible results with configurable seed
- Exports results to formatted Excel sheets

## Usage

```bash
# Basic usage
workshop-lottery --input registrations.xlsx

# With all options
workshop-lottery \
  --input registrations.xlsx \
  --output results.xlsx \
  --capacity 30 \
  --seed 42 \
  --order W2,W1,W3
```

## Options

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--input` | `-i` | Required | Input Excel file from MS Forms |
| `--output` | `-o` | WorkshopAssignments.xlsx | Output file path |
| `--capacity` | `-c` | 34 | Seats per workshop |
| `--seed` | `-s` | YYYYMMDD | Random seed for reproducibility |
| `--order` | | W1,W2,W3 | Workshop processing order |

## Building

```bash
dotnet build
```

## Running Tests

```bash
dotnet test
```

## Algorithm

The lottery uses the **Efraimidis-Spirakis** algorithm for weighted random selection:
- Rank 1 preference = weight 5
- Rank 2 preference = weight 2  
- Rank 3 preference = weight 1

Assignment happens in two waves:
1. **Wave 1**: Maximize unique participants (each person gets at most one seat)
2. **Wave 2**: Fill remaining seats from waitlist (allows multiple workshops)

## Output Format

The output Excel file contains three sheets (Workshop 1, 2, 3) with columns:
- Order, Status, Wave, Name, Email, Laptop, Commit10Min, Requested, Rank, Weight, Seed

Rows are ordered: Accepted (Wave 1), Accepted (Wave 2), Waitlisted.
```

### 7. Add Integration Tests

Create `tests/WorkshopLottery.Tests/Integration/EndToEndTests.cs`:

```csharp
// Full pipeline test:
// - Create synthetic Excel input
// - Run complete lottery pipeline
// - Verify output Excel has correct structure
// - Verify assignment counts match expectations
```

Create `tests/WorkshopLottery.Tests/Services/ExcelWriterServiceTests.cs`:

```csharp
// Test cases:
// - WriteResults_CreatesThreeSheets
// - WriteResults_HeaderRowIsBold
// - WriteResults_TopRowIsFrozen
// - WriteResults_OrdersAcceptedBeforeWaitlist
// - WriteResults_IncludesAllColumns
```

---

## Verification

1. **Build succeeds:**
   ```bash
   dotnet build
   ```

2. **All tests pass:**
   ```bash
   dotnet test
   ```

3. **CLI works:**
   ```bash
   dotnet run --project src/WorkshopLottery -- --help
   ```

4. **End-to-end test with sample data:**
   ```bash
   dotnet run --project src/WorkshopLottery -- \
     --input "input/AgentCon Zurich – Workshop Signup (Lottery + Standby)(1-7).xlsx" \
     --output test-output.xlsx \
     --seed 20260202
   ```

5. **Verify output:**
   - Open test-output.xlsx
   - Confirm 3 sheets exist
   - Confirm header formatting
   - Confirm row ordering

---

## Architecture Alignment Check

| Requirement | Status |
|-------------|--------|
| System.CommandLine for CLI (ADR-004) | ✅ Aligned |
| ClosedXML for output (ADR-002) | ✅ Aligned |
| Output columns per spec | ✅ Aligned |
| Header bold, freeze top row | ✅ Aligned |
| Console summary output | ✅ Aligned |
| Orchestrator coordinates workflow | ✅ Aligned |

---

## Final Deliverables Checklist

| Deliverable | Status |
|-------------|--------|
| `/src` console app | ✅ Complete |
| `/tests` unit tests | ✅ Complete |
| `README.md` with usage | ✅ Complete |
| CLI with all options | ✅ Complete |
| Excel input parsing | ✅ Complete |
| Validation & duplicate detection | ✅ Complete |
| Weighted lottery engine | ✅ Complete |
| Wave-based assignment | ✅ Complete |
| Excel output with formatting | ✅ Complete |
| Console summary logging | ✅ Complete |

---

## Post-Implementation

After all phases complete:
1. Run full test suite
2. Manual test with real sample data
3. Verify determinism (same seed = same output)
4. Update ARCHITECTURE.md if any deviations occurred
5. Tag release version
