using WorkshopLottery.Models;

namespace WorkshopLottery.Services;

/// <summary>
/// Service for validating and filtering registrations.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates raw registrations and returns categorized results.
    /// </summary>
    /// <param name="rawRegistrations">Raw registrations from Excel parser.</param>
    /// <returns>Validation result with eligible, disqualified, and statistics.</returns>
    ValidationResult ValidateAndFilter(IReadOnlyList<RawRegistration> rawRegistrations);
}

/// <summary>
/// Contains the result of validation including categorized registrations and statistics.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// All registrations (both eligible and disqualified).
    /// </summary>
    public List<Registration> AllRegistrations { get; init; } = new();
    
    /// <summary>
    /// Registrations that passed all eligibility checks.
    /// </summary>
    public List<Registration> EligibleRegistrations { get; init; } = new();
    
    /// <summary>
    /// Registrations that failed one or more eligibility checks.
    /// </summary>
    public List<Registration> DisqualifiedRegistrations { get; init; } = new();
    
    /// <summary>
    /// Count of disqualifications by reason.
    /// </summary>
    public Dictionary<string, int> DisqualificationReasons { get; init; } = new();
    
    /// <summary>
    /// Total number of registrations processed.
    /// </summary>
    public int TotalCount => AllRegistrations.Count;
    
    /// <summary>
    /// Number of eligible registrations.
    /// </summary>
    public int EligibleCount => EligibleRegistrations.Count;
    
    /// <summary>
    /// Number of disqualified registrations.
    /// </summary>
    public int DisqualifiedCount => DisqualifiedRegistrations.Count;
}
