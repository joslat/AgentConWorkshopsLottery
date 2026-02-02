using WorkshopLottery.Models;

namespace WorkshopLottery.Services;

/// <summary>
/// Implementation of the lottery engine using the Efraimidis-Spirakis algorithm
/// for weighted random selection and two-wave assignment for fair seat distribution.
/// </summary>
public class LotteryEngine : ILotteryEngine
{
    /// <summary>
    /// Runs the lottery to assign workshop seats using weighted random selection.
    /// </summary>
    public LotteryResult RunLottery(
        IReadOnlyList<Registration> eligibleRegistrations,
        LotteryConfiguration config)
    {
        var seed = config.GetEffectiveSeed();
        var random = new Random(seed);

        Console.WriteLine();
        Console.WriteLine($"🎲 Running lottery with seed: {seed}");
        Console.WriteLine($"   Capacity per workshop: {config.Capacity}");
        Console.WriteLine($"   Eligible participants: {eligibleRegistrations.Count}");

        // Build per-workshop candidate pools with weighted ordering
        var workshopPools = BuildWorkshopPools(eligibleRegistrations, config.WorkshopOrder, random);

        // Execute wave-based assignment
        var workshopResults = ExecuteWaveAssignment(workshopPools, config.Capacity, config.WorkshopOrder);

        // Log results summary
        LogResultsSummary(workshopResults, config.WorkshopOrder);

        return new LotteryResult
        {
            Seed = seed,
            Capacity = config.Capacity,
            Results = workshopResults,
            TotalRegistrations = eligibleRegistrations.Count,
            EligibleCount = eligibleRegistrations.Count,
            DisqualifiedCount = 0 // Set by orchestrator if needed
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

            Console.WriteLine($"   {workshop}: {candidates.Count} candidates");
        }

        return pools;
    }

    /// <summary>
    /// Creates a weighted candidate using the Efraimidis-Spirakis algorithm.
    /// Score = log(u) / weight, where u ∈ (0, 1)
    /// </summary>
    /// <remarks>
    /// The Efraimidis-Spirakis algorithm generates a random permutation where
    /// the probability of each item appearing in a given position is proportional
    /// to its weight. This is achieved by computing score = log(u) / weight for
    /// each item, where u is a uniform random number in (0,1).
    /// 
    /// Higher weights result in scores closer to 0 (since log(u) is negative),
    /// so items with higher weights have better (higher) scores after the
    /// log transformation.
    /// </remarks>
    private WeightedCandidate CreateWeightedCandidate(
        Registration registration,
        WorkshopId workshop,
        Random random)
    {
        var pref = registration.WorkshopPreferences[workshop];
        var weight = pref.Weight;

        // Generate u ∈ (0, 1) - must exclude 0 to avoid log(0) = -∞
        double u;
        do { u = random.NextDouble(); } while (u == 0.0);

        // Efraimidis-Spirakis score: log(u) / weight
        // Since log(u) < 0 for u < 1, and we divide by weight:
        // - Higher weight → score closer to 0 → better (higher) score
        // - This gives correct probability weighting when sorted descending
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
    /// - Wave 1: Maximize unique participants (each person gets at most one workshop)
    /// - Wave 2: Fill remaining seats (allows multiple workshops per person)
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
        // This maximizes the number of unique participants across all workshops
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

                // Only assign if this person hasn't been assigned to ANY workshop yet
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
        // This allows popular choices to fill their remaining seats
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

        // WAITLIST: Add remaining candidates not accepted to the workshop
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

    /// <summary>
    /// Logs a summary of the lottery results.
    /// </summary>
    private static void LogResultsSummary(
        Dictionary<WorkshopId, WorkshopResult> results,
        List<WorkshopId> workshopOrder)
    {
        Console.WriteLine();
        Console.WriteLine("✅ Lottery Results:");

        foreach (var workshop in workshopOrder)
        {
            var result = results[workshop];
            Console.WriteLine($"   {workshop}:");
            Console.WriteLine($"      Accepted: {result.AcceptedCount} (Wave 1: {result.Wave1Count}, Wave 2: {result.Wave2Count})");
            Console.WriteLine($"      Waitlist: {result.WaitlistCount}");
        }
    }
}
