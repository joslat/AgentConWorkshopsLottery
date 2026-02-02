namespace WorkshopLottery.Models;

/// <summary>
/// Status of a workshop assignment.
/// </summary>
public enum AssignmentStatus
{
    /// <summary>Participant has been accepted into the workshop.</summary>
    Accepted,
    
    /// <summary>Participant is on the waitlist for the workshop.</summary>
    Waitlisted
}
