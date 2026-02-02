namespace WorkshopLottery.Models;

/// <summary>
/// Represents a candidate with their lottery score for a specific workshop.
/// Used internally by the lottery engine for Efraimidis-Spirakis weighted selection.
/// </summary>
internal record WeightedCandidate
{
    /// <summary>
    /// The registration for this candidate.
    /// </summary>
    public required Registration Registration { get; init; }
    
    /// <summary>
    /// The workshop this candidate is competing for.
    /// </summary>
    public required WorkshopId Workshop { get; init; }
    
    /// <summary>
    /// The weight derived from the candidate's rank for this workshop.
    /// </summary>
    public required int Weight { get; init; }
    
    /// <summary>
    /// The Efraimidis-Spirakis score: log(u) / weight where u ∈ (0,1).
    /// Higher scores have better ranking (selected first).
    /// </summary>
    public required double Score { get; init; }
}
