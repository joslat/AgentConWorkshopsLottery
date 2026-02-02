# ADR-003: Efraimidis-Spirakis Weighted Sampling Algorithm

**Status:** Accepted  
**Date:** 2026-02-02  
**Context:** Weighted lottery algorithm selection

---

## Context

The lottery must assign workshop seats fairly based on participant rankings. Higher-ranked workshops should have proportionally higher selection probability. The selection must be:
- Reproducible (deterministic with seed)
- Fair (probability proportional to weight)
- Without replacement (each person selected at most once per wave)

## Decision Drivers

- Mathematical fairness
- Reproducibility with seed
- Computational efficiency
- Implementation simplicity
- Weighted sampling without replacement

## Options Considered

### Option 1: Efraimidis-Spirakis Algorithm
For each item with weight w:
```
u = random(0,1)
key = log(u) / w
```
Sort by key descending. Items with higher weights have proportionally higher probability of ranking first.

**Complexity:** O(n log n) for sorting

### Option 2: Repeated Weighted Random Selection
1. Build cumulative weight array
2. Generate random number, binary search to find selected item
3. Remove item, repeat

**Complexity:** O(n²) due to rebuilding weights

### Option 3: Alias Method + Shuffling
1. Build alias table (O(n))
2. Sample without replacement using rejection

**Complexity:** O(n) expected but complex implementation

### Option 4: Naive Approach - Duplicate Items by Weight
- Add 5 copies for rank 1, 2 for rank 2, 1 for rank 3
- Shuffle and deduplicate

**Issues:** Doesn't give true weighted probability for permutation

## Decision

**Option 1: Efraimidis-Spirakis Algorithm**

## Rationale

1. **Mathematical Correctness:**
   - Proven to produce weighted random permutations
   - Each position probability matches weight distribution
   - Paper: "Weighted random sampling with a reservoir" (2006)

2. **Single Pass:**
   - Generate all keys in O(n)
   - Sort once in O(n log n)
   - No need to rebuild data structures

3. **Reproducibility:**
   - Same seed → same random sequence → same keys → same order
   - Fully deterministic

4. **Implementation Simplicity:**
   ```csharp
   var ordered = candidates
       .Select(c => new {
           Candidate = c,
           Key = Math.Log(random.NextDouble()) / c.Weight
       })
       .OrderByDescending(x => x.Key)
       .Select(x => x.Candidate)
       .ToList();
   ```

5. **Edge Case Handling:**
   - Weight = 0: Assign -infinity key (always last)
   - Equal weights: Degenerates to uniform random

## Mathematical Proof Sketch

For weight w, the probability of getting the highest key among n items:

P(item i wins) = wᵢ / Σwⱼ

This holds because log(U)/w has an exponential distribution with rate w, and the minimum of independent exponentials has probability proportional to rates.

## Consequences

### Positive
- Mathematically sound lottery
- Easy to explain and audit
- Fast for expected data sizes (<500 registrations)
- Single, simple implementation

### Negative
- Requires understanding of the algorithm for verification
- Log(0) edge case needs handling (use NextDouble() which excludes 0)

### Implementation Notes
```csharp
// Ensure u ∈ (0,1) exclusive
double u;
do { u = random.NextDouble(); } while (u == 0.0);
double key = Math.Log(u) / weight;
```

## References
- Efraimidis, P.S., Spirakis, P.G. (2006). "Weighted random sampling with a reservoir"
- Information Processing Letters, Volume 97, Issue 5

## Related Documents
- [ARCHITECTURE.md](../ARCHITECTURE.md) - Algorithm specification in Section 5.1
