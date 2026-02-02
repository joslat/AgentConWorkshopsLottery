namespace WorkshopLottery.Models;

/// <summary>
/// Represents a participant's registration for workshops.
/// </summary>
public class Registration
{
    /// <summary>
    /// Unique identifier for this registration.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// The participant's full name.
    /// </summary>
    public required string FullName { get; init; }
    
    /// <summary>
    /// The participant's email address.
    /// </summary>
    public required string Email { get; init; }
    
    /// <summary>
    /// Email address normalized for duplicate detection (trimmed and lowercased).
    /// </summary>
    public string NormalizedEmail => Email.Trim().ToLowerInvariant();
    
    /// <summary>
    /// Whether the participant will bring a laptop (required for workshops).
    /// </summary>
    public bool HasLaptop { get; init; }
    
    /// <summary>
    /// Whether the participant commits to arriving 10 minutes early.
    /// </summary>
    public bool WillCommit10Min { get; init; }
    
    /// <summary>
    /// Dictionary of workshop preferences keyed by workshop ID.
    /// </summary>
    public Dictionary<WorkshopId, WorkshopPreference> WorkshopPreferences { get; init; } = new();
    
    /// <summary>
    /// Whether the participant is eligible for the lottery.
    /// Starts as true and can be set to false via Disqualify().
    /// </summary>
    public bool IsEligible { get; private set; } = true;
    
    /// <summary>
    /// Reason for disqualification, if applicable.
    /// </summary>
    public string? DisqualificationReason { get; private set; }
    
    /// <summary>
    /// Marks this registration as disqualified with the given reason.
    /// </summary>
    /// <param name="reason">The reason for disqualification.</param>
    public void Disqualify(string reason)
    {
        IsEligible = false;
        DisqualificationReason = reason;
    }
    
    /// <summary>
    /// Checks if the registration meets basic eligibility requirements.
    /// Does not check for duplicates (handled by ValidationService).
    /// </summary>
    /// <returns>True if basic requirements are met.</returns>
    public bool MeetsBasicRequirements() =>
        !string.IsNullOrWhiteSpace(FullName) &&
        !string.IsNullOrWhiteSpace(Email) &&
        HasLaptop &&
        WillCommit10Min;
}
