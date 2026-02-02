# Implementation Plan 004: Lottery Engine

**Phase:** 4 of 5  
**Estimated Effort:** Single session  
**Prerequisites:** Phase 3 complete (Validation service)  
**Status:** ✅ COMPLETE

---

## Implementation Completion Status

| Feature | Status | Tests |
|---------|--------|-------|
| **ILotteryEngine Interface** | ✅ Complete | ✅ |
| RunLottery method signature | ✅ | ✅ |
| **WeightedCandidate Model** | ✅ Complete | ✅ |
| Registration, Workshop, Weight, Score properties | ✅ | ✅ |
| **LotteryEngine Implementation** | ✅ Complete | ✅ 22 tests |
| Efraimidis-Spirakis algorithm | ✅ | ✅ |
| score = log(u) / weight formula | ✅ | ✅ |
| BuildWorkshopPools (weighted ordering) | ✅ | ✅ |
| **Wave-Based Assignment** | ✅ Complete | ✅ |
| Wave 1: Unique participants only | ✅ | ✅ |
| Wave 2: Fill remaining seats | ✅ | ✅ |
| Waitlist ordering preservation | ✅ | ✅ |
| **Determinism** | ✅ Complete | ✅ |
| Same seed → same results | ✅ | ✅ |
| Different seeds → different results | ✅ | ✅ |
| **Integration Tests** | ✅ Complete | ✅ 6 tests |
| Full workflow validation | ✅ | ✅ |
| Wave 1 uniqueness verification | ✅ | ✅ |
| Capacity limit enforcement | ✅ | ✅ |
| Waitlist ordering verification | ✅ | ✅ |
| Duplicate filtering before lottery | ✅ | ✅ |
| Determinism across runs | ✅ | ✅ |
| **Statistical Verification** | ✅ Complete | ✅ 2 tests |
| Higher weight = higher probability | ✅ | ✅ |
| Equal weights ≈ uniform distribution | ✅ | ✅ |

**Total Tests Added:** 28 new tests (Plan 004)  
**Total Tests in Project:** 284 passing tests ✅

---

## Objective

Implement the lottery engine that performs weighted random selection using the Efraimidis-Spirakis algorithm and wave-based assignment to maximize unique participant access.

---

## Tasks

### 1. Create Lottery Engine Interface

Create `src/WorkshopLottery/Services/ILotteryEngine.cs`:

```csharp
namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

public interface ILotteryEngine
{
    LotteryResult RunLottery(
        IReadOnlyList<Registration> eligibleRegistrations,
        LotteryConfiguration config);
}
```

### 2. Create Weighted Candidate Model

Create `src/WorkshopLottery/Models/WeightedCandidate.cs`:

```csharp
namespace WorkshopLottery.Models;

/// <summary>
/// Represents a candidate with their lottery score for a specific workshop.
/// </summary>
internal record WeightedCandidate
{
    public required Registration Registration { get; init; }
    public required WorkshopId Workshop { get; init; }
    public required int Weight { get; init; }
    public required double Score { get; init; }
}
```

### 3. Implement Lottery Engine

Create `src/WorkshopLottery/Services/LotteryEngine.cs`:

```csharp
namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

public class LotteryEngine : ILotteryEngine
{
    public LotteryResult RunLottery(
        IReadOnlyList<Registration> eligibleRegistrations,
        LotteryConfiguration config)
    {
        var seed = config.GetEffectiveSeed();
        var random = new Random(seed);
        
        // Build per-workshop candidate pools with weighted ordering
        var workshopPools = BuildWorkshopPools(eligibleRegistrations, config.WorkshopOrder, random);
        
        // Execute wave-based assignment
        var workshopResults = ExecuteWaveAssignment(workshopPools, config.Capacity, config.WorkshopOrder);
        
        return new LotteryResult
        {
            Seed = seed,
            Capacity = config.Capacity,
            Results = workshopResults,
            TotalRegistrations = eligibleRegistrations.Count,
            EligibleCount = eligibleRegistrations.Count,
            DisqualifiedCount = 0 // Will be set by orchestrator
        };
    }
    
    /// <summary>
    /// Builds weighted-ordered candidate lists for each workshop using Efraimidis-Spirakis.
    /// </summary>
    private Dictionary<WorkshopId, List<WeightedCandidate>> BuildWorkshopPools(
        IReadOnlyList<Registration> registrations,
        List<WorkshopId> workshops,
        Random random)
    {
        var pools = new Dictionary<WorkshopId, List<WeightedCandidate>>();
        
        foreach (var workshop in workshops)
        {
            var candidates = registrations
                .Where(r => r.WorkshopPreferences.TryGetValue(workshop, out var pref) && pref.Requested)
                .Select(r => CreateWeightedCandidate(r, workshop, random))
                .OrderByDescending(c => c.Score) // Higher score = better position
                .ToList();
            
            pools[workshop] = candidates;
        }
        
        return pools;
    }
    
    /// <summary>
    /// Creates a weighted candidate using the Efraimidis-Spirakis algorithm.
    /// score = log(u) / weight, where u ∈ (0,1)
    /// </summary>
    private WeightedCandidate CreateWeightedCandidate(
        Registration registration,
        WorkshopId workshop,
        Random random)
    {
        var pref = registration.WorkshopPreferences[workshop];
        var weight = pref.Weight;
        
        // Generate u ∈ (0,1) - must exclude 0 to avoid log(0)
        double u;
        do { u = random.NextDouble(); } while (u == 0.0);
        
        // Efraimidis-Spirakis score: log(u) / weight
        // Higher weight → score closer to 0 → better rank after descending sort
        var score = Math.Log(u) / weight;
        
        return new WeightedCandidate
        {
            Registration = registration,
            Workshop = workshop,
            Weight = weight,
            Score = score
        };
    }
    
    /// <summary>
    /// Executes two-wave assignment:
    /// Wave 1: Maximize unique participants
    /// Wave 2: Fill remaining seats
    /// </summary>
    private Dictionary<WorkshopId, WorkshopResult> ExecuteWaveAssignment(
        Dictionary<WorkshopId, List<WeightedCandidate>> pools,
        int capacity,
        List<WorkshopId> workshopOrder)
    {
        var results = new Dictionary<WorkshopId, WorkshopResult>();
        var globallyAssigned = new HashSet<string>(); // Track by normalized email
        var workshopAccepted = new Dictionary<WorkshopId, HashSet<string>>();
        
        // Initialize results and tracking
        foreach (var workshop in workshopOrder)
        {
            results[workshop] = new WorkshopResult { WorkshopId = workshop };
            workshopAccepted[workshop] = new HashSet<string>();
        }
        
        // WAVE 1: Assign seats to people without any assignment yet
        foreach (var workshop in workshopOrder)
        {
            var pool = pools[workshop];
            var result = results[workshop];
            var accepted = workshopAccepted[workshop];
            var orderCounter = 1;
            
            foreach (var candidate in pool)
            {
                if (accepted.Count >= capacity)
                    break;
                
                var email = candidate.Registration.NormalizedEmail;
                
                if (!globallyAssigned.Contains(email))
                {
                    result.Assignments.Add(new WorkshopAssignment
                    {
                        Registration = candidate.Registration,
                        Status = AssignmentStatus.Accepted,
                        Wave = 1,
                        Order = orderCounter++
                    });
                    
                    accepted.Add(email);
                    globallyAssigned.Add(email);
                }
            }
        }
        
        // WAVE 2: Fill remaining seats with anyone (may already be assigned elsewhere)
        foreach (var workshop in workshopOrder)
        {
            var pool = pools[workshop];
            var result = results[workshop];
            var accepted = workshopAccepted[workshop];
            var orderCounter = accepted.Count + 1;
            var spotsRemaining = capacity - accepted.Count;
            
            if (spotsRemaining <= 0)
                continue;
            
            foreach (var candidate in pool)
            {
                if (spotsRemaining <= 0)
                    break;
                
                var email = candidate.Registration.NormalizedEmail;
                
                // Only add if not already accepted in THIS workshop
                if (!accepted.Contains(email))
                {
                    result.Assignments.Add(new WorkshopAssignment
                    {
                        Registration = candidate.Registration,
                        Status = AssignmentStatus.Accepted,
                        Wave = 2,
                        Order = orderCounter++
                    });
                    
                    accepted.Add(email);
                    spotsRemaining--;
                }
            }
        }
        
        // WAITLIST: Add remaining candidates not accepted
        foreach (var workshop in workshopOrder)
        {
            var pool = pools[workshop];
            var result = results[workshop];
            var accepted = workshopAccepted[workshop];
            var orderCounter = accepted.Count + 1;
            
            foreach (var candidate in pool)
            {
                var email = candidate.Registration.NormalizedEmail;
                
                if (!accepted.Contains(email))
                {
                    result.Assignments.Add(new WorkshopAssignment
                    {
                        Registration = candidate.Registration,
                        Status = AssignmentStatus.Waitlisted,
                        Wave = null,
                        Order = orderCounter++
                    });
                }
            }
        }
        
        return results;
    }
}
```

### 4. Add Comprehensive Unit Tests

Create `tests/WorkshopLottery.Tests/Services/LotteryEngineTests.cs`:

```csharp
// Test cases for Efraimidis-Spirakis:
// - RunLottery_WithSameSeed_ProducesDeterministicOrder
// - RunLottery_WithDifferentSeeds_ProducesDifferentOrders
// - RunLottery_HigherWeightHasHigherSelectionProbability (statistical test)

// Test cases for Wave 1:
// - RunLottery_Wave1_DoesNotAssignSamePersonTwice
// - RunLottery_Wave1_MaximizesUniqueParticipants
// - RunLottery_Wave1_RespectsCapacity

// Test cases for Wave 2:
// - RunLottery_Wave2_FillsRemainingSeats
// - RunLottery_Wave2_AllowsMultipleWorkshops
// - RunLottery_Wave2_OnlyRunsIfSeatsAvailable

// Test cases for Waitlist:
// - RunLottery_Waitlist_PreservesWeightedOrder
// - RunLottery_Waitlist_IncludesAllRemainingCandidates

// Edge cases:
// - RunLottery_WithFewerCandidatesThanCapacity_AcceptsAll
// - RunLottery_WithNoEligibleRegistrations_ReturnsEmptyResults
// - RunLottery_WithWorkshopOrderConfigured_RespectsOrder
```

Create `tests/WorkshopLottery.Tests/Services/WeightedRandomTests.cs`:

```csharp
// Statistical validation tests:
// - Weight5_SelectedMoreOftenThanWeight1 (run many trials, verify ratio)
// - EqualWeights_UniformDistribution
// - DeterministicWithSeed_SameResultsAcrossRuns
```

### 5. Add Integration Test

Create `tests/WorkshopLottery.Tests/Integration/LotteryIntegrationTests.cs`:

```csharp
// End-to-end test with synthetic data:
// - Create 100 registrations with various ranks
// - Run lottery
// - Verify Wave 1 uniqueness
// - Verify capacity limits
// - Verify waitlist ordering
```

---

## Verification

1. **Build succeeds:**
   ```bash
   dotnet build
   ```

2. **Tests pass:**
   ```bash
   dotnet test --filter "Lottery|WeightedRandom"
   ```

3. **Manual verification with sample data:**
   ```csharp
   var parser = new ExcelParserService();
   var validator = new ValidationService();
   var engine = new LotteryEngine();
   
   var raw = parser.ParseRegistrations("input/AgentCon...xlsx");
   var validated = validator.ValidateAndFilter(raw);
   
   var config = new LotteryConfiguration
   {
       InputPath = "input/...",
       Capacity = 34,
       Seed = 20260202
   };
   
   var result = engine.RunLottery(validated.EligibleRegistrations, config);
   
   foreach (var (workshop, workshopResult) in result.Results)
   {
       Console.WriteLine($"{workshop}: {workshopResult.AcceptedCount} accepted, {workshopResult.WaitlistCount} waitlisted");
       Console.WriteLine($"  Wave 1: {workshopResult.Wave1Count}, Wave 2: {workshopResult.Wave2Count}");
   }
   ```

4. **Determinism verification:**
   Run twice with same seed, verify identical results.

---

## Algorithm Verification

### Efraimidis-Spirakis Properties

| Property | Verification Method |
|----------|---------------------|
| Deterministic with seed | Run twice, compare results |
| Probability ∝ weight | Statistical test over many runs |
| No replacement | Verify no duplicates in output |

### Wave Assignment Properties

| Property | Verification Method |
|----------|---------------------|
| Wave 1 uniqueness | Assert no person in multiple Wave 1 slots |
| Wave 2 only after Wave 1 | Assert Wave 2 people had opportunity in Wave 1 |
| Capacity respected | Assert accepted ≤ capacity |
| All non-accepted on waitlist | Assert pool size = accepted + waitlisted |

---

## Architecture Alignment Check

| Requirement | Status |
|-------------|--------|
| Efraimidis-Spirakis algorithm (ADR-003) | ✅ Aligned |
| Wave-based assignment (ADR-006) | ✅ Aligned |
| Deterministic with seed | ✅ Aligned |
| ILotteryEngine interface | ✅ Aligned |
| Weight 5/2/1 for rank 1/2/3 | ✅ Aligned |

---

## Handoff to Next Phase

After completing this phase, the following will be ready:
- Lottery can be run with weighted random selection
- Wave-based assignment maximizes unique participants
- Results are deterministic with a given seed
- Foundation for Phase 5 (Excel writer, CLI, orchestrator)
