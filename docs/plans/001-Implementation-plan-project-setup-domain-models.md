# Implementation Plan 001: Project Setup & Domain Models

**Phase:** 1 of 5  
**Estimated Effort:** Single session  
**Prerequisites:** None  
**Status:** Pending

---

## Objective

Set up the .NET 10 solution structure, configure dependencies, and implement the core domain models that will be used throughout the application.

---

## Tasks

### 1. Create Solution Structure

Create the following directory and file structure:

```
AgentConWorkshopsLottery/
├── src/
│   └── WorkshopLottery/
│       ├── WorkshopLottery.csproj
│       ├── Program.cs (minimal placeholder)
│       ├── Models/
│       ├── Services/
│       ├── Infrastructure/
│       └── Extensions/
├── tests/
│   └── WorkshopLottery.Tests/
│       └── WorkshopLottery.Tests.csproj
└── WorkshopLottery.sln
```

### 2. Configure Project Files

**WorkshopLottery.csproj:**
- Target: `net10.0`
- Output type: `Exe`
- Nullable: `enable`
- ImplicitUsings: `enable`
- Package references:
  - `ClosedXML` (latest stable)
  - `System.CommandLine` (latest stable)

**WorkshopLottery.Tests.csproj:**
- Target: `net10.0`
- Package references:
  - `Microsoft.NET.Test.Sdk`
  - `xunit`
  - `xunit.runner.visualstudio`
  - `FluentAssertions`
  - `coverlet.collector`
- Project reference to `WorkshopLottery.csproj`

### 3. Implement Domain Models

Create the following models in `src/WorkshopLottery/Models/`:

**WorkshopId.cs:**
```csharp
namespace WorkshopLottery.Models;

public enum WorkshopId
{
    W1 = 1,
    W2 = 2,
    W3 = 3
}
```

**WorkshopPreference.cs:**
```csharp
namespace WorkshopLottery.Models;

public record WorkshopPreference
{
    public bool Requested { get; init; }
    public int? Rank { get; init; }  // 1, 2, or 3
    public int Weight => Rank switch
    {
        1 => 5,
        2 => 2,
        _ => 1  // Rank 3 or unranked
    };
}
```

**Registration.cs:**
```csharp
namespace WorkshopLottery.Models;

public class Registration
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public string NormalizedEmail => Email.Trim().ToLowerInvariant();
    
    public bool HasLaptop { get; init; }
    public bool WillCommit10Min { get; init; }
    
    public Dictionary<WorkshopId, WorkshopPreference> WorkshopPreferences { get; init; } = new();
    
    public bool IsEligible { get; private set; } = true;
    public string? DisqualificationReason { get; private set; }
    
    public void Disqualify(string reason)
    {
        IsEligible = false;
        DisqualificationReason = reason;
    }
    
    public bool MeetsBasicRequirements() =>
        !string.IsNullOrWhiteSpace(FullName) &&
        !string.IsNullOrWhiteSpace(Email) &&
        HasLaptop &&
        WillCommit10Min;
}
```

**AssignmentStatus.cs:**
```csharp
namespace WorkshopLottery.Models;

public enum AssignmentStatus
{
    Accepted,
    Waitlisted
}
```

**WorkshopAssignment.cs:**
```csharp
namespace WorkshopLottery.Models;

public record WorkshopAssignment
{
    public required Registration Registration { get; init; }
    public required AssignmentStatus Status { get; init; }
    public int? Wave { get; init; }  // 1, 2, or null for waitlist
    public int Order { get; init; }
}
```

**WorkshopResult.cs:**
```csharp
namespace WorkshopLottery.Models;

public class WorkshopResult
{
    public required WorkshopId WorkshopId { get; init; }
    public List<WorkshopAssignment> Assignments { get; init; } = new();
    
    public IEnumerable<WorkshopAssignment> Accepted => 
        Assignments.Where(a => a.Status == AssignmentStatus.Accepted);
    
    public IEnumerable<WorkshopAssignment> Waitlisted => 
        Assignments.Where(a => a.Status == AssignmentStatus.Waitlisted);
    
    public int AcceptedCount => Accepted.Count();
    public int Wave1Count => Accepted.Count(a => a.Wave == 1);
    public int Wave2Count => Accepted.Count(a => a.Wave == 2);
    public int WaitlistCount => Waitlisted.Count();
}
```

**LotteryResult.cs:**
```csharp
namespace WorkshopLottery.Models;

public class LotteryResult
{
    public required int Seed { get; init; }
    public required int Capacity { get; init; }
    public Dictionary<WorkshopId, WorkshopResult> Results { get; init; } = new();
    
    // Statistics
    public int TotalRegistrations { get; init; }
    public int EligibleCount { get; init; }
    public int DisqualifiedCount { get; init; }
    public Dictionary<string, int> DisqualificationReasons { get; init; } = new();
}
```

**LotteryConfiguration.cs:**
```csharp
namespace WorkshopLottery.Models;

public record LotteryConfiguration
{
    public required string InputPath { get; init; }
    public string OutputPath { get; init; } = "WorkshopAssignments.xlsx";
    public int Capacity { get; init; } = 34;
    public int? Seed { get; init; }
    public List<WorkshopId> WorkshopOrder { get; init; } = [WorkshopId.W1, WorkshopId.W2, WorkshopId.W3];
    
    public int GetEffectiveSeed() => Seed ?? int.Parse(DateTime.Now.ToString("yyyyMMdd"));
}
```

### 4. Create Minimal Program.cs

```csharp
using WorkshopLottery.Models;

Console.WriteLine("Workshop Lottery - Phase 1 Complete");
Console.WriteLine($"Default capacity: {new LotteryConfiguration { InputPath = "test.xlsx" }.Capacity}");
```

### 5. Add Initial Unit Tests

Create `tests/WorkshopLottery.Tests/Models/`:

**RegistrationTests.cs:**
- Test `NormalizedEmail` trims and lowercases
- Test `MeetsBasicRequirements` with various inputs
- Test `Disqualify` sets properties correctly

**WorkshopPreferenceTests.cs:**
- Test weight calculation: Rank1→5, Rank2→2, Rank3→1, null→1

**LotteryConfigurationTests.cs:**
- Test `GetEffectiveSeed` returns provided seed or date-based default

---

## Verification

1. **Build succeeds:**
   ```bash
   dotnet build
   ```

2. **Tests pass:**
   ```bash
   dotnet test
   ```

3. **Application runs:**
   ```bash
   dotnet run --project src/WorkshopLottery
   ```

---

## Architecture Alignment Check

| Requirement | Status |
|-------------|--------|
| Single project structure (ADR-001) | ✅ Aligned |
| Folder separation (Models/, Services/, etc.) | ✅ Aligned |
| .NET 10 target | ✅ Aligned |
| ClosedXML dependency added (ADR-002) | ✅ Aligned |
| Domain models match ARCHITECTURE.md Section 3 | ✅ Aligned |

---

## Handoff to Next Phase

After completing this phase, the following will be ready:
- Solution compiles and runs
- Domain models available for service implementation
- Test project configured for TDD approach
- Foundation for Phase 2 (Excel parsing)
