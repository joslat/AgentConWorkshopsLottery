using WorkshopLottery.Extensions;
using WorkshopLottery.Models;

namespace WorkshopLottery.Services;

/// <summary>
/// Implementation of validation service.
/// Transforms raw registrations into validated Registration objects,
/// handles duplicate detection, eligibility checks, and ranking parsing.
/// </summary>
public class ValidationService : IValidationService
{
    /// <summary>
    /// Validates raw registrations and returns categorized results.
    /// </summary>
    public ValidationResult ValidateAndFilter(IReadOnlyList<RawRegistration> rawRegistrations)
    {
        var result = new ValidationResult();

        // Step 1: Convert raw registrations to Registration objects
        var registrations = rawRegistrations
            .Select(ConvertToRegistration)
            .ToList();

        // Step 2: Apply basic eligibility checks
        ApplyBasicEligibilityChecks(registrations, result.DisqualificationReasons);

        // Step 3: Detect and disqualify duplicates (only among still-eligible)
        DisqualifyDuplicates(registrations, result.DisqualificationReasons);

        // Populate result
        result.AllRegistrations.AddRange(registrations);
        result.EligibleRegistrations.AddRange(registrations.Where(r => r.IsEligible));
        result.DisqualifiedRegistrations.AddRange(registrations.Where(r => !r.IsEligible));

        // Log summary
        LogValidationSummary(result);

        return result;
    }

    /// <summary>
    /// Converts a raw registration to a Registration object with parsed preferences.
    /// </summary>
    private Registration ConvertToRegistration(RawRegistration raw)
    {
        var rankings = RankingParser.ParseRankings(raw.RankingsResponse);

        var registration = new Registration
        {
            FullName = raw.FullName.TrimOrEmpty(),
            Email = raw.Email.TrimOrEmpty(),
            HasLaptop = raw.LaptopResponse.ParseYesNo(),
            WillCommit10Min = raw.Commit10MinResponse.ParseYesNo(),
            WorkshopPreferences = BuildWorkshopPreferences(raw, rankings)
        };

        return registration;
    }

    /// <summary>
    /// Builds workshop preferences from raw responses and parsed rankings.
    /// </summary>
    private Dictionary<WorkshopId, WorkshopPreference> BuildWorkshopPreferences(
        RawRegistration raw,
        Dictionary<WorkshopId, int> rankings)
    {
        var preferences = new Dictionary<WorkshopId, WorkshopPreference>
        {
            // Workshop 1
            [WorkshopId.W1] = CreatePreference(raw.RequestedW1Response, WorkshopId.W1, rankings),
            
            // Workshop 2
            [WorkshopId.W2] = CreatePreference(raw.RequestedW2Response, WorkshopId.W2, rankings),
            
            // Workshop 3
            [WorkshopId.W3] = CreatePreference(raw.RequestedW3Response, WorkshopId.W3, rankings)
        };

        return preferences;
    }

    /// <summary>
    /// Creates a workshop preference based on the request response and rankings.
    /// </summary>
    private WorkshopPreference CreatePreference(
        string? requestedResponse, 
        WorkshopId workshopId, 
        Dictionary<WorkshopId, int> rankings)
    {
        var requested = requestedResponse.ParseYesNo();
        
        return new WorkshopPreference
        {
            Requested = requested,
            Rank = requested ? GetRankOrDefault(rankings, workshopId) : null
        };
    }

    /// <summary>
    /// Gets the rank for a workshop, defaulting to 3 if not specified.
    /// </summary>
    private int GetRankOrDefault(Dictionary<WorkshopId, int> rankings, WorkshopId workshop)
    {
        // If workshop was requested but not in rankings, default to rank 3 (lowest priority)
        return rankings.TryGetValue(workshop, out var rank) ? rank : 3;
    }

    /// <summary>
    /// Applies basic eligibility checks to all registrations.
    /// </summary>
    private void ApplyBasicEligibilityChecks(
        List<Registration> registrations,
        Dictionary<string, int> reasons)
    {
        foreach (var reg in registrations)
        {
            // Check in priority order - only first failure reason is recorded
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

    /// <summary>
    /// Detects and disqualifies duplicate email registrations.
    /// All instances of duplicate emails are disqualified (not just the later ones).
    /// </summary>
    private void DisqualifyDuplicates(
        List<Registration> registrations,
        Dictionary<string, int> reasons)
    {
        // Group by normalized email among still-eligible registrations
        var duplicates = registrations
            .Where(r => r.IsEligible) // Only check among still-eligible
            .GroupBy(r => r.NormalizedEmail)
            .Where(g => g.Count() > 1) // Only groups with duplicates
            .SelectMany(g => g)
            .ToList();

        foreach (var reg in duplicates)
        {
            reg.Disqualify("Duplicate email");
            IncrementReason(reasons, "Duplicate email");
        }
    }

    /// <summary>
    /// Increments the count for a disqualification reason.
    /// </summary>
    private static void IncrementReason(Dictionary<string, int> reasons, string reason)
    {
        reasons.TryGetValue(reason, out var count);
        reasons[reason] = count + 1;
    }

    /// <summary>
    /// Logs a summary of the validation results.
    /// </summary>
    private static void LogValidationSummary(ValidationResult result)
    {
        Console.WriteLine();
        Console.WriteLine("✅ Validation Summary:");
        Console.WriteLine($"   Total registrations: {result.TotalCount}");
        Console.WriteLine($"   Eligible: {result.EligibleCount}");
        Console.WriteLine($"   Disqualified: {result.DisqualifiedCount}");
        
        if (result.DisqualificationReasons.Count > 0)
        {
            Console.WriteLine("   Disqualification reasons:");
            foreach (var (reason, count) in result.DisqualificationReasons.OrderByDescending(r => r.Value))
            {
                Console.WriteLine($"      - {reason}: {count}");
            }
        }
    }
}
