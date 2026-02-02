# Implementation Plan 003: Validation Service & Ranking Parser

**Phase:** 3 of 5  
**Estimated Effort:** Single session  
**Prerequisites:** Phase 2 complete (Excel parser service)  
**Status:** ✅ COMPLETE

---

## Implementation Completion Status

| Feature | Status | Tests |
|---------|--------|-------|
| **ParsingExtensions** | ✅ Complete | ✅ 22 tests |
| ParseYesNo (yes/ja/oui/sí/si/y/true/1) | ✅ | ✅ |
| NormalizeEmail (trim + lowercase) | ✅ | ✅ |
| TrimOrEmpty helper | ✅ | ✅ |
| IsNullOrWhiteSpace helper | ✅ | ✅ |
| **RankingParser** | ✅ Complete | ✅ 26 tests |
| Semicolon-delimited parsing | ✅ | ✅ |
| Case-insensitive workshop detection | ✅ | ✅ |
| Position-based ranking (1-indexed) | ✅ | ✅ |
| Unknown workshop handling | ✅ | ✅ |
| Full workshop names (MS Forms format) | ✅ | ✅ |
| **ValidationService** | ✅ Complete | ✅ 24 tests |
| IValidationService interface | ✅ | ✅ |
| ValidationResult class | ✅ | ✅ |
| RawRegistration → Registration conversion | ✅ | ✅ |
| Eligibility: HasLaptop check | ✅ | ✅ |
| Eligibility: WillCommit10Min check | ✅ | ✅ |
| Eligibility: Name required | ✅ | ✅ |
| Eligibility: Email required | ✅ | ✅ |
| Duplicate detection (case-insensitive) | ✅ | ✅ |
| All duplicate instances disqualified | ✅ | ✅ |
| Disqualification reasons tracking | ✅ | ✅ |
| Workshop preference building | ✅ | ✅ |
| Default rank 3 for requested-but-unranked | ✅ | ✅ |

**Total Tests Added:** 72 new tests (Plan 003)  
**Total Tests in Project:** 256 passing tests ✅

---

## Objective

Implement the validation service that transforms raw registrations into validated Registration objects, handles duplicate detection, eligibility checks, and ranking parsing.

---

## Tasks

### 1. Create Parsing Extensions

Create `src/WorkshopLottery/Extensions/ParsingExtensions.cs`:

```csharp
namespace WorkshopLottery.Extensions;

public static class ParsingExtensions
{
    /// <summary>
    /// Parses Yes/No responses (case-insensitive).
    /// </summary>
    public static bool ParseYesNo(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        
        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "yes" => true,
            "ja" => true,   // German
            "oui" => true,  // French
            _ => false
        };
    }
    
    /// <summary>
    /// Normalizes email for comparison (trim + lowercase).
    /// </summary>
    public static string NormalizeEmail(this string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}
```

### 2. Create Ranking Parser

Create `src/WorkshopLottery/Services/RankingParser.cs`:

```csharp
namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

public static class RankingParser
{
    /// <summary>
    /// Parses the ranking field into per-workshop ranks.
    /// Example input: "Workshop 2 – AI Architecture;Workshop 1 – Secure;Workshop 3 – Pizza"
    /// Returns: { W2: 1, W1: 2, W3: 3 }
    /// </summary>
    public static Dictionary<WorkshopId, int> ParseRankings(string? rankingsField)
    {
        var result = new Dictionary<WorkshopId, int>();
        
        if (string.IsNullOrWhiteSpace(rankingsField))
            return result;
        
        var segments = rankingsField.Split(';', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i].Trim();
            var rank = i + 1; // 1-indexed position
            
            // Detect which workshop this segment refers to
            if (ContainsWorkshop(segment, "1"))
            {
                result.TryAdd(WorkshopId.W1, rank);
            }
            else if (ContainsWorkshop(segment, "2"))
            {
                result.TryAdd(WorkshopId.W2, rank);
            }
            else if (ContainsWorkshop(segment, "3"))
            {
                result.TryAdd(WorkshopId.W3, rank);
            }
            // Unknown workshop format - skip
        }
        
        return result;
    }
    
    private static bool ContainsWorkshop(string segment, string number)
    {
        // Match "Workshop 1", "Workshop1", "workshop 1", etc.
        return segment.Contains($"Workshop {number}", StringComparison.OrdinalIgnoreCase) ||
               segment.Contains($"Workshop{number}", StringComparison.OrdinalIgnoreCase);
    }
}
```

### 3. Create Validation Service Interface

Create `src/WorkshopLottery/Services/IValidationService.cs`:

```csharp
namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

public interface IValidationService
{
    ValidationResult ValidateAndFilter(IReadOnlyList<RawRegistration> rawRegistrations);
}

public class ValidationResult
{
    public List<Registration> AllRegistrations { get; init; } = new();
    public List<Registration> EligibleRegistrations { get; init; } = new();
    public List<Registration> DisqualifiedRegistrations { get; init; } = new();
    
    public Dictionary<string, int> DisqualificationReasons { get; init; } = new();
}
```

### 4. Implement Validation Service

Create `src/WorkshopLottery/Services/ValidationService.cs`:

```csharp
namespace WorkshopLottery.Services;

using WorkshopLottery.Extensions;
using WorkshopLottery.Models;

public class ValidationService : IValidationService
{
    public ValidationResult ValidateAndFilter(IReadOnlyList<RawRegistration> rawRegistrations)
    {
        var result = new ValidationResult();
        
        // Step 1: Convert raw registrations to Registration objects
        var registrations = rawRegistrations
            .Select(ConvertToRegistration)
            .ToList();
        
        // Step 2: Apply basic eligibility checks
        ApplyBasicEligibilityChecks(registrations, result.DisqualificationReasons);
        
        // Step 3: Detect and disqualify duplicates
        DisqualifyDuplicates(registrations, result.DisqualificationReasons);
        
        // Populate result
        result.AllRegistrations.AddRange(registrations);
        result.EligibleRegistrations.AddRange(registrations.Where(r => r.IsEligible));
        result.DisqualifiedRegistrations.AddRange(registrations.Where(r => !r.IsEligible));
        
        return result;
    }
    
    private Registration ConvertToRegistration(RawRegistration raw)
    {
        var rankings = RankingParser.ParseRankings(raw.RankingsResponse);
        
        var registration = new Registration
        {
            FullName = raw.FullName ?? string.Empty,
            Email = raw.Email ?? string.Empty,
            HasLaptop = raw.LaptopResponse.ParseYesNo(),
            WillCommit10Min = raw.Commit10MinResponse.ParseYesNo(),
            WorkshopPreferences = BuildWorkshopPreferences(raw, rankings)
        };
        
        return registration;
    }
    
    private Dictionary<WorkshopId, WorkshopPreference> BuildWorkshopPreferences(
        RawRegistration raw, 
        Dictionary<WorkshopId, int> rankings)
    {
        var preferences = new Dictionary<WorkshopId, WorkshopPreference>();
        
        // Workshop 1
        var requestedW1 = raw.RequestedW1Response.ParseYesNo();
        preferences[WorkshopId.W1] = new WorkshopPreference
        {
            Requested = requestedW1,
            Rank = requestedW1 ? GetRankOrDefault(rankings, WorkshopId.W1) : null
        };
        
        // Workshop 2
        var requestedW2 = raw.RequestedW2Response.ParseYesNo();
        preferences[WorkshopId.W2] = new WorkshopPreference
        {
            Requested = requestedW2,
            Rank = requestedW2 ? GetRankOrDefault(rankings, WorkshopId.W2) : null
        };
        
        // Workshop 3
        var requestedW3 = raw.RequestedW3Response.ParseYesNo();
        preferences[WorkshopId.W3] = new WorkshopPreference
        {
            Requested = requestedW3,
            Rank = requestedW3 ? GetRankOrDefault(rankings, WorkshopId.W3) : null
        };
        
        return preferences;
    }
    
    private int GetRankOrDefault(Dictionary<WorkshopId, int> rankings, WorkshopId workshop)
    {
        // If workshop was requested but not in rankings, default to rank 3
        return rankings.TryGetValue(workshop, out var rank) ? rank : 3;
    }
    
    private void ApplyBasicEligibilityChecks(
        List<Registration> registrations, 
        Dictionary<string, int> reasons)
    {
        foreach (var reg in registrations)
        {
            if (string.IsNullOrWhiteSpace(reg.FullName))
            {
                reg.Disqualify("Missing name");
                IncrementReason(reasons, "Missing name");
            }
            else if (string.IsNullOrWhiteSpace(reg.Email))
            {
                reg.Disqualify("Missing email");
                IncrementReason(reasons, "Missing email");
            }
            else if (!reg.HasLaptop)
            {
                reg.Disqualify("No laptop");
                IncrementReason(reasons, "No laptop");
            }
            else if (!reg.WillCommit10Min)
            {
                reg.Disqualify("Won't commit to arrive early");
                IncrementReason(reasons, "Won't commit to arrive early");
            }
        }
    }
    
    private void DisqualifyDuplicates(
        List<Registration> registrations, 
        Dictionary<string, int> reasons)
    {
        // Group by normalized email
        var byEmail = registrations
            .Where(r => r.IsEligible) // Only check among still-eligible
            .GroupBy(r => r.NormalizedEmail)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToList();
        
        foreach (var reg in byEmail)
        {
            reg.Disqualify("Duplicate email");
            IncrementReason(reasons, "Duplicate email");
        }
    }
    
    private void IncrementReason(Dictionary<string, int> reasons, string reason)
    {
        reasons.TryGetValue(reason, out var count);
        reasons[reason] = count + 1;
    }
}
```

### 5. Add Comprehensive Unit Tests

Create `tests/WorkshopLottery.Tests/Services/RankingParserTests.cs`:

```csharp
// Test cases:
// - ParseRankings_WithValidInput_ReturnsCorrectRanks
// - ParseRankings_WithEmptyString_ReturnsEmptyDictionary
// - ParseRankings_WithNull_ReturnsEmptyDictionary
// - ParseRankings_WithPartialRankings_ReturnsOnlyPresentWorkshops
// - ParseRankings_WithDifferentSeparators_ParsesCorrectly
// - ParseRankings_IgnoresCaseForWorkshopDetection
```

Create `tests/WorkshopLottery.Tests/Services/ValidationServiceTests.cs`:

```csharp
// Test cases:
// - ValidateAndFilter_WithEligibleRegistrations_ReturnsAllEligible
// - ValidateAndFilter_MissingName_DisqualifiesRegistration
// - ValidateAndFilter_MissingEmail_DisqualifiesRegistration
// - ValidateAndFilter_NoLaptop_DisqualifiesRegistration
// - ValidateAndFilter_WontCommit_DisqualifiesRegistration
// - ValidateAndFilter_DuplicateEmails_DisqualifiesAllInstances
// - ValidateAndFilter_DuplicateEmails_CaseInsensitive
// - ValidateAndFilter_RequestedWithoutRanking_DefaultsToRank3
// - ValidateAndFilter_TracksDisqualificationReasons
```

Create `tests/WorkshopLottery.Tests/Extensions/ParsingExtensionsTests.cs`:

```csharp
// Test cases:
// - ParseYesNo_Yes_ReturnsTrue
// - ParseYesNo_No_ReturnsFalse
// - ParseYesNo_CaseInsensitive
// - ParseYesNo_NullOrEmpty_ReturnsFalse
// - ParseYesNo_Ja_ReturnsTrue (German)
// - ParseYesNo_Oui_ReturnsTrue (French)
// - NormalizeEmail_TrimsAndLowercases
```

---

## Verification

1. **Build succeeds:**
   ```bash
   dotnet build
   ```

2. **Tests pass:**
   ```bash
   dotnet test --filter "Validation|Ranking|Parsing"
   ```

3. **Integration test:**
   ```csharp
   var parser = new ExcelParserService();
   var validator = new ValidationService();
   
   var raw = parser.ParseRegistrations("input/AgentCon...xlsx");
   var result = validator.ValidateAndFilter(raw);
   
   Console.WriteLine($"Total: {result.AllRegistrations.Count}");
   Console.WriteLine($"Eligible: {result.EligibleRegistrations.Count}");
   Console.WriteLine($"Disqualified: {result.DisqualifiedRegistrations.Count}");
   foreach (var reason in result.DisqualificationReasons)
   {
       Console.WriteLine($"  - {reason.Key}: {reason.Value}");
   }
   ```

---

## Architecture Alignment Check

| Requirement | Status |
|-------------|--------|
| Duplicate detection disqualifies ALL instances | ✅ Aligned |
| Yes/No parsing case-insensitive | ✅ Aligned |
| Ranking parser splits on ';' | ✅ Aligned |
| Missing rank defaults to 3 | ✅ Aligned |
| Eligibility: laptop + commit + name + email | ✅ Aligned |
| IValidationService interface | ✅ Aligned |

---

## Handoff to Next Phase

After completing this phase, the following will be ready:
- Raw registrations can be validated and filtered
- Duplicates are properly detected and disqualified
- Rankings are parsed into per-workshop preferences
- Weights are calculated from ranks
- Foundation for Phase 4 (Lottery engine)
