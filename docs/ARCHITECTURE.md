# Workshop Lottery System - Architecture Document

**Version:** 1.0  
**Date:** 2026-02-02  
**Status:** Draft

---

## 1. Overview

The Workshop Lottery System is a .NET 10 console application that fairly assigns workshop seats to participants using a weighted lottery algorithm with wave-based assignment. It reads Microsoft Forms Excel exports and produces assignment results in Excel format.

### 1.1 Core Problem Statement

Given a registration Excel file from Microsoft Forms with workshop preferences and rankings, the system must:
1. Parse and validate registrations
2. Disqualify ineligible or duplicate entries
3. Assign seats fairly using weighted randomization
4. Maximize unique participant access in Wave 1
5. Fill remaining seats in Wave 2
6. Export results to Excel with per-workshop sheets

### 1.2 Design Principles

- **DRY (Don't Repeat Yourself):** Shared parsing, validation, and weight calculation logic centralized
- **KISS (Keep It Simple, Stupid):** Single responsibility per component, minimal abstraction layers
- **SOLID:** Interface-based design for testability; single-purpose classes
- **CLEAN Architecture:** Clear separation between domain logic, infrastructure, and presentation
- **PRAGMATIC:** Avoid over-engineering; optimize for maintainability and clarity

---

## 2. Solution Structure

```
AgentConWorkshopsLottery/
├── docs/
│   ├── ARCHITECTURE.md          # This document
│   ├── adr/                      # Architecture Decision Records
│   └── plans/                    # Implementation plans
├── src/
│   └── WorkshopLottery/          # Main console application
│       ├── WorkshopLottery.csproj
│       ├── Program.cs
│       ├── Models/               # Domain models
│       ├── Services/             # Business logic
│       ├── Infrastructure/       # Excel I/O, CLI
│       └── Extensions/           # Helper extensions
├── tests/
│   └── WorkshopLottery.Tests/    # Unit tests
│       ├── WorkshopLottery.Tests.csproj
│       └── ...
├── input/                        # Sample input files
├── WorkshopLottery.sln
└── README.md
```

### 2.1 Project Structure Rationale

**Single Console Project:** Given the focused scope (CLI tool), a single project with clear folder separation provides sufficient modularity without the overhead of multiple assemblies. This follows KISS and Pragmatic principles.

**Tests Separated:** Unit tests in a dedicated project for proper isolation and test framework dependencies.

---

## 3. Domain Model

### 3.1 Core Entities

```
Registration
├── Id (Guid)
├── FullName (string)
├── Email (string, normalized)
├── HasLaptop (bool)
├── WillCommit10Min (bool)
├── WorkshopPreferences (Dictionary<WorkshopId, WorkshopPreference>)
├── IsEligible (bool, computed)
├── DisqualificationReason (string?, computed)

WorkshopPreference
├── Requested (bool)
├── Rank (int?, 1-3)
├── Weight (int, computed: Rank1=5, Rank2=2, Rank3=1)

WorkshopId (enum)
├── W1  # Workshop 1 – Secure Coding Literacy for Vibe Coders
├── W2  # Workshop 2 – AI Architecture Critic  
├── W3  # Workshop 3 – Build a Pizza Ordering Agent with Microsoft Foundry and MCP

WorkshopAssignment
├── WorkshopId
├── Registrations (ordered list)
├── Accepted (list with Wave indicator)
├── Waitlist (list)

AssignmentResult
├── Registration
├── Status (Accepted | Waitlisted)
├── Wave (1 | 2 | null for waitlist)
├── Order (int)

WorkshopResult
├── WorkshopId
├── Assignments (list of WorkshopAssignment)
├── Accepted (computed: assignments with status=Accepted)
├── Waitlisted (computed: assignments with status=Waitlisted)
├── AcceptedCount (computed)
├── Wave1Count (computed)
├── Wave2Count (computed)
├── WaitlistCount (computed)

LotteryResult
├── Seed (int)
├── Capacity (int)
├── Results (Dictionary<WorkshopId, WorkshopResult>)
├── TotalRegistrations (int)
├── EligibleCount (int)
├── DisqualifiedCount (int)
├── DisqualificationReasons (Dictionary<string, int>)
```

### 3.2 Value Objects & Constants

```
LotteryConfiguration
├── Capacity (int, default: 34)
├── Seed (int, default: YYYYMMDD)
├── WorkshopOrder (list, default: [W1, W2, W3])
├── Weights (Rank1=5, Rank2=2, Rank3=1)
```

---

## 4. Component Architecture

### 4.1 Layer Diagram

```
┌─────────────────────────────────────────────────┐
│                  Program.cs                      │
│              (Entry Point / CLI)                 │
└────────────────────┬────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────┐
│              LotteryOrchestrator                 │
│         (Coordinates the workflow)               │
└────────────────────┬────────────────────────────┘
                     │
    ┌────────────────┼────────────────┐
    │                │                │
┌───▼────┐     ┌─────▼─────┐    ┌─────▼─────┐
│ Parser │     │ Validator │    │  Lottery  │
│Service │     │  Service  │    │  Engine   │
└───┬────┘     └─────┬─────┘    └─────┬─────┘
    │                │                │
┌───▼────────────────▼────────────────▼───┐
│              Domain Models               │
└─────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────┐
│              Infrastructure                      │
│    ExcelReader │ ExcelWriter │ ConsoleLogger    │
└─────────────────────────────────────────────────┘
```

### 4.2 Component Responsibilities

| Component | Responsibility |
|-----------|----------------|
| **Program.cs** | CLI argument parsing, DI setup, orchestration invocation |
| **LotteryOrchestrator** | Coordinates read → validate → assign → write workflow |
| **ExcelParserService** | Reads Excel, maps columns by fuzzy matching, produces raw registrations |
| **ValidationService** | Applies eligibility rules, detects duplicates, marks disqualifications |
| **LotteryEngine** | Efraimidis-Spirakis weighted permutation, wave-based assignment |
| **ExcelWriterService** | Produces output workbook with formatted sheets |
| **ConsoleLogger** | Outputs summary statistics to console |

### 4.3 Service Interfaces

```csharp
interface IExcelParserService
{
    IReadOnlyList<RawRegistration> ParseRegistrations(string filePath);
}

interface IValidationService
{
    IReadOnlyList<Registration> ValidateAndFilter(
        IReadOnlyList<RawRegistration> raw);
}

interface ILotteryEngine
{
    LotteryResult RunLottery(
        IReadOnlyList<Registration> eligible,
        LotteryConfiguration config);
}

interface IExcelWriterService
{
    void WriteResults(string filePath, LotteryResult result);
}
```

---

## 5. Algorithm Specification

### 5.1 Efraimidis-Spirakis Weighted Random Permutation

For each candidate in a workshop pool with weight `w`:

```
u = Random.NextDouble()  // (0, 1) exclusive
score = Math.Log(u) / w  // Higher weight → score closer to 0
```

Sort candidates by `score` descending (closest to 0 first).

**Properties:**
- Deterministic with fixed seed
- Fair probability proportional to weight
- Single-pass generation

### 5.2 Wave Assignment Algorithm

```
Wave 1: Maximize Unique Participants
----------------------------------------
assignedGlobally = {}  // People who have ANY seat

for each workshop in order:
    candidates = weightedPermutation[workshop]
    accepted = []
    
    for person in candidates:
        if accepted.Count >= capacity: break
        if person NOT in assignedGlobally:
            accepted.Add(person, Wave=1)
            assignedGlobally.Add(person)
    
    workshop.Accepted = accepted
    workshop.Remaining = candidates - accepted

Wave 2: Fill Remaining Seats
----------------------------------------
for each workshop in order:
    remaining = workshop.Remaining
    spotsLeft = capacity - workshop.Accepted.Count
    
    for person in remaining:
        if spotsLeft == 0: break
        if person NOT already accepted in THIS workshop:
            workshop.Accepted.Add(person, Wave=2)
            spotsLeft--
```

### 5.3 Duplicate Detection

```
emails = registrations.GroupBy(r => r.Email.ToLower().Trim())
duplicates = emails.Where(g => g.Count() > 1).SelectMany(g => g)

// Mark ALL instances as disqualified
foreach (registration in duplicates):
    registration.Disqualify("Duplicate email")
```

---

## 6. Data Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Input.xlsx  │────►│ ExcelParser  │────►│    Raw       │
│  (MS Forms)  │     │   Service    │     │ Registrations│
└──────────────┘     └──────────────┘     └──────┬───────┘
                                                  │
┌──────────────┐     ┌──────────────┐     ┌──────▼───────┐
│  Validated   │◄────│ Validation   │◄────│   Fuzzy      │
│ Registrations│     │   Service    │     │   Mapped     │
└──────┬───────┘     └──────────────┘     └──────────────┘
       │
       │ (Eligible only)
       │
┌──────▼───────┐     ┌──────────────┐     ┌──────────────┐
│   Lottery    │────►│   Lottery    │────►│   Output     │
│    Engine    │     │    Result    │     │    .xlsx     │
└──────────────┘     └──────────────┘     └──────────────┘
```

---

## 7. Configuration & CLI

### 7.1 Command Line Interface

```bash
workshop-lottery \
  --input <path>              # Required: Input Excel file
  --output <path>             # Optional: Output file (default: WorkshopAssignments.xlsx)
  --capacity <int>            # Optional: Seats per workshop (default: 34)
  --seed <int>                # Optional: Random seed (default: YYYYMMDD)
  --order <W1,W2,W3>          # Optional: Workshop processing order
```

### 7.2 Implementation

Use `System.CommandLine` for modern, robust CLI parsing with:
- Automatic help generation
- Validation
- Type conversion

---

## 8. Excel Column Mapping

### 8.0 Sample Input File

A real Microsoft Forms export is available for testing and validation:
- **Location:** `input/AgentCon Zurich – Workshop Signup (Lottery + Standby)(1-7).xlsx`
- **Purpose:** Verify fuzzy matching patterns, test parsing logic, validate data structure
- **Analysis Tool:** `tools/ExcelAnalyzer` - C# console app to analyze column structure (run with `dotnet run --project tools/ExcelAnalyzer`)

### 8.1 Fuzzy Header Matching Strategy

Headers in MS Forms exports may vary. Match using case-insensitive contains/prefix:

| Target Field | Match Pattern |
|--------------|---------------|
| FullName | Contains "name" AND NOT contains "email" |
| Email | Contains "email" |
| HasLaptop | Contains "laptop" |
| WillCommit10Min | Contains "commit" OR contains "10 min" |
| RequestedW1 | Contains "Workshop 1" |
| RequestedW2 | Contains "Workshop 2" |
| RequestedW3 | Contains "Workshop 3" |
| Rankings | Contains "rank" |

### 8.2 Ranking Field Parsing

Input: `"Workshop 2 – AI Architecture;Workshop 1 – Secure;Workshop 3 – Pizza"`

Algorithm:
1. Split by `;`
2. For each segment, detect `Workshop 1`, `Workshop 2`, `Workshop 3`
3. Position in array (1-indexed) = Rank

---

## 9. Output Format

### 9.1 Excel Structure

**Workbook:** 3 sheets named "Workshop 1", "Workshop 2", "Workshop 3"

**Columns per sheet:**

| Column | Description |
|--------|-------------|
| Order | Sequential number (1, 2, 3...) |
| Status | "Accepted" or "Waitlist" |
| Wave | 1, 2, or empty for waitlist |
| Name | Full name |
| Email | Email address |
| Laptop | "Yes" or "No" |
| Commit10Min | "Yes" or "No" |
| Requested | "Yes" (always for this sheet) |
| Rank | 1, 2, or 3 |
| Weight | Weight used (5, 2, or 1) |
| Seed | Random seed used |

**Formatting:**
- Header row: Bold
- Freeze top row
- Auto-fit column widths

### 9.2 Row Ordering

1. Accepted (Wave 1) - in lottery order
2. Accepted (Wave 2) - in lottery order
3. Waitlist - in lottery order

---

## 10. Error Handling

| Scenario | Behavior |
|----------|----------|
| Input file not found | Exit with error message |
| Invalid Excel format | Exit with descriptive error |
| No eligible registrations | Output empty sheets, warn |
| Missing required columns | Exit with error listing missing |

---

## 11. Logging & Summary

Console output at completion:

```
=== Workshop Lottery Summary ===
Input file: registrations.xlsx
Random seed: 20260202

Total rows: 150
Eligible: 120
Disqualified: 30
  - Missing laptop: 10
  - Won't commit: 8
  - Duplicate emails: 12

Workshop 1: 34 accepted (Wave1: 30, Wave2: 4), 45 waitlisted
Workshop 2: 34 accepted (Wave1: 28, Wave2: 6), 38 waitlisted
Workshop 3: 34 accepted (Wave1: 32, Wave2: 2), 52 waitlisted

Output: WorkshopAssignments.xlsx
```

---

## 12. Testing Strategy

### 12.1 Unit Test Coverage

| Component | Test Cases |
|-----------|------------|
| Ranking Parser | Valid rankings, partial rankings, empty, malformed |
| Duplicate Detection | No duplicates, all duplicates, partial duplicates |
| Weight Calculation | Rank 1/2/3, missing rank defaults to 3 |
| Weighted Permutation | Determinism with seed, distribution fairness |
| Wave 1 Logic | Uniqueness constraint, capacity limits |
| Wave 2 Logic | Fills remaining seats, respects existing assignments |

### 12.2 Test Data

Generate synthetic test data for edge cases:
- Exactly 34 eligible per workshop
- More than 34 eligible
- Fewer than 34 eligible
- All duplicates
- No eligible registrations

---

## 13. Dependencies

| Package | Purpose | Version |
|---------|---------|---------|
| ClosedXML | Excel read/write | Latest stable |
| System.CommandLine | CLI parsing | Latest stable |
| xUnit | Unit testing | Latest stable |
| FluentAssertions | Test assertions | Latest stable |

---

## 14. Future Considerations (Out of Scope)

- Web UI for running lottery
- Database persistence
- Historical tracking
- Email notifications
- Multiple lottery runs with undo

---

## Appendix A: Glossary

| Term | Definition |
|------|------------|
| Wave 1 | First assignment pass prioritizing unique participants |
| Wave 2 | Second pass filling remaining seats with any eligible |
| Weighted Permutation | Random ordering where probability is proportional to weight |
| Efraimidis-Spirakis | Algorithm for weighted sampling without replacement |
