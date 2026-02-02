namespace WorkshopLottery.Models;

/// <summary>
/// Represents a participant's assignment to a workshop.
/// </summary>
public record WorkshopAssignment
{
    /// <summary>
    /// The registration this assignment belongs to.
    /// </summary>
    public required Registration Registration { get; init; }
    
    /// <summary>
    /// Whether the participant is accepted or waitlisted.
    /// </summary>
    public required AssignmentStatus Status { get; init; }
    
    /// <summary>
    /// The wave in which the participant was assigned (1 or 2).
    /// Null for waitlisted participants.
    /// </summary>
    public int? Wave { get; init; }
    
    /// <summary>
    /// Order position in the assignment (1-based).
    /// </summary>
    public int Order { get; init; }
}
