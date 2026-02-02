namespace WorkshopLottery.Models;

/// <summary>
/// Represents a participant's preference for a specific workshop.
/// </summary>
public record WorkshopPreference
{
    /// <summary>
    /// Whether the participant requested to attend this workshop.
    /// </summary>
    public bool Requested { get; init; }
    
    /// <summary>
    /// The participant's ranking of this workshop (1, 2, or 3).
    /// Null if the workshop was not ranked.
    /// </summary>
    public int? Rank { get; init; }
    
    /// <summary>
    /// Calculated weight for lottery selection.
    /// Rank 1 = 5, Rank 2 = 2, Rank 3 or unranked = 1.
    /// </summary>
    public int Weight => Rank switch
    {
        1 => 5,
        2 => 2,
        _ => 1  // Rank 3 or unranked
    };
}
