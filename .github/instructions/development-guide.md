# Custom Instructions for Workshop Lottery Development

These instructions supplement the agent definition for implementing the Workshop Lottery System.

## Phase-Specific Guidance

### Phase 1: Project Setup & Domain Models
- Create solution structure per `docs/plans/001-Implementation-plan-project-setup-domain-models.md`
- All models in `src/WorkshopLottery/Models/`
- Use records for immutable value types
- Include computed properties (e.g., `Weight` from `Rank`)

### Phase 2: Excel Parser Service
- Implement fuzzy column matching per ADR-005
- Handle MS Forms export variations
- Log column mappings for debugging
- Test with real sample file: `input/AgentCon Zurich...xlsx`

### Phase 3: Validation Service
- Duplicate detection: normalize email (trim + lowercase)
- **Disqualify ALL instances** of duplicate emails (not just extras)
- Track disqualification reasons for summary output
- Default unranked workshops to Rank 3

### Phase 4: Lottery Engine
- Efraimidis-Spirakis formula: `score = log(u) / weight`
- **Important**: Ensure `u > 0` to avoid `log(0)`
- Sort descending by score (higher = better position)
- Deterministic with same seed

### Phase 5: Excel Writer, CLI & Orchestrator
- Three sheets: "Workshop 1", "Workshop 2", "Workshop 3"
- Output order: Accepted (Wave 1), Accepted (Wave 2), Waitlisted
- Header row bold, freeze top row
- Auto-fit column widths

## Common Patterns

### Service Interface Pattern
```csharp
// In Services/
public interface IExcelParserService
{
    IReadOnlyList<RawRegistration> ParseRegistrations(string filePath);
}

public class ExcelParserService : IExcelParserService
{
    public IReadOnlyList<RawRegistration> ParseRegistrations(string filePath)
    {
        // Implementation
    }
}
```

### Weighted Random Selection
```csharp
// Efraimidis-Spirakis
double u;
do { u = random.NextDouble(); } while (u == 0.0);
double score = Math.Log(u) / weight;
// Sort descending by score
```

### Yes/No Parsing
```csharp
public static bool ParseYesNo(this string? value)
{
    var normalized = (value ?? "").Trim().ToLowerInvariant();
    return normalized is "yes" or "ja" or "oui";
}
```

## Testing Patterns

### Determinism Test
```csharp
[Fact]
public void RunLottery_WithSameSeed_ProducesSameResults()
{
    var config = new LotteryConfiguration { Seed = 42, ... };
    
    var result1 = engine.RunLottery(registrations, config);
    var result2 = engine.RunLottery(registrations, config);
    
    result1.Should().BeEquivalentTo(result2);
}
```

### Wave 1 Uniqueness Test
```csharp
[Fact]
public void Wave1_DoesNotAssignSamePersonTwice()
{
    var result = engine.RunLottery(registrations, config);
    
    var wave1People = result.Results.Values
        .SelectMany(r => r.Accepted.Where(a => a.Wave == 1))
        .Select(a => a.Registration.NormalizedEmail);
    
    wave1People.Should().OnlyHaveUniqueItems();
}
```

## Error Messages

Use clear, actionable error messages:

```csharp
// Good
throw new FileNotFoundException($"Input file not found: {filePath}");
throw new InvalidOperationException($"Required columns not found: {string.Join(", ", missing)}");

// Bad
throw new Exception("File error");
throw new Exception("Missing columns");
```

## Console Output Format

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

## File Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Model | PascalCase.cs | `Registration.cs` |
| Service | IPascalCaseService.cs | `ILotteryEngine.cs` |
| Test | PascalCaseTests.cs | `LotteryEngineTests.cs` |
| Prompt | NNN-description.md | `003-developer-agent-creation.md` |
