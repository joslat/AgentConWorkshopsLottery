# ADR-002: ClosedXML for Excel Operations

**Status:** Accepted  
**Date:** 2026-02-02  
**Context:** Excel library selection

---

## Context

The application must read Microsoft Forms Excel exports and write formatted assignment results. We need a reliable .NET library for Excel operations.

## Decision Drivers

- Must support .xlsx format (Office Open XML)
- Read existing files and create new files
- Formatting support (bold headers, freeze panes, column widths)
- Cross-platform compatibility (.NET 10)
- Open source / permissive license
- Active maintenance
- No Excel installation required (server-side capable)

## Options Considered

### Option 1: ClosedXML
- Open source (MIT)
- Pure .NET, no COM dependencies
- Rich formatting API
- Active community
- NuGet: ~50M downloads

### Option 2: EPPlus
- Open source (Polyform Noncommercial License for v5+)
- Very mature
- License changed: free only for noncommercial use
- Would require commercial license for business use

### Option 3: NPOI
- Open source (Apache 2.0)
- Port of Apache POI
- Supports both .xls and .xlsx
- API less intuitive than ClosedXML

### Option 4: Office Interop
- Requires Excel installation
- Windows-only
- COM complexity
- Not suitable for server/CLI

## Decision

**Option 1: ClosedXML**

## Rationale

1. **Licensing:** MIT license allows unrestricted commercial use
2. **API Quality:** Fluent, intuitive API for common operations
3. **Feature Set:** Meets all requirements (read, write, formatting, freeze panes)
4. **Cross-Platform:** Pure .NET, works on Windows/Linux/macOS
5. **Ecosystem:** User requirement explicitly specified ClosedXML
6. **Maturity:** 10+ years of development, stable API

```csharp
// Example: Clean, readable API
using var workbook = new XLWorkbook("input.xlsx");
var sheet = workbook.Worksheet(1);
var cell = sheet.Cell("A1");

// Writing with formatting
sheet.Row(1).Style.Font.Bold = true;
sheet.SheetView.FreezeRows(1);
workbook.SaveAs("output.xlsx");
```

## Consequences

### Positive
- Simple integration via NuGet
- Excellent documentation
- Strong typing for cells
- Memory-efficient streaming for large files

### Negative
- Limited chart support (not needed)
- Some advanced Excel features not supported (macros, VBA)

### Implementation Notes
- Package: `ClosedXML` (latest stable)
- Target: .NET 10
- No additional dependencies required

## Related Documents
- [ARCHITECTURE.md](../ARCHITECTURE.md) - Overall architecture specification
