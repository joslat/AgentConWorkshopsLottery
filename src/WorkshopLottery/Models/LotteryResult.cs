namespace WorkshopLottery.Models;

/// <summary>
/// Contains the complete results of a lottery run.
/// </summary>
public class LotteryResult
{
    /// <summary>
    /// The random seed used for this lottery run.
    /// </summary>
    public required int Seed { get; init; }
    
    /// <summary>
    /// The capacity (seats per workshop) used for this lottery.
    /// </summary>
    public required int Capacity { get; init; }
    
    /// <summary>
    /// Results for each workshop, keyed by workshop ID.
    /// </summary>
    public Dictionary<WorkshopId, WorkshopResult> Results { get; init; } = new();
    
    /// <summary>
    /// Total number of registrations processed.
    /// </summary>
    public int TotalRegistrations { get; init; }
    
    /// <summary>
    /// Number of eligible registrations (after validation).
    /// </summary>
    public int EligibleCount { get; init; }
    
    /// <summary>
    /// Number of disqualified registrations.
    /// </summary>
    public int DisqualifiedCount { get; init; }
    
    /// <summary>
    /// Count of disqualifications by reason.
    /// </summary>
    public Dictionary<string, int> DisqualificationReasons { get; init; } = new();
}
