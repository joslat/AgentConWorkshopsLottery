namespace WorkshopLottery.Models;

/// <summary>
/// Contains the lottery results for a single workshop.
/// </summary>
public class WorkshopResult
{
    /// <summary>
    /// The workshop this result is for.
    /// </summary>
    public required WorkshopId WorkshopId { get; init; }
    
    /// <summary>
    /// All assignments for this workshop (accepted and waitlisted).
    /// </summary>
    public List<WorkshopAssignment> Assignments { get; init; } = new();
    
    /// <summary>
    /// Participants who have been accepted into this workshop.
    /// </summary>
    public IEnumerable<WorkshopAssignment> Accepted => 
        Assignments.Where(a => a.Status == AssignmentStatus.Accepted);
    
    /// <summary>
    /// Participants who are on the waitlist for this workshop.
    /// </summary>
    public IEnumerable<WorkshopAssignment> Waitlisted => 
        Assignments.Where(a => a.Status == AssignmentStatus.Waitlisted);
    
    /// <summary>
    /// Total number of accepted participants.
    /// </summary>
    public int AcceptedCount => Accepted.Count();
    
    /// <summary>
    /// Number of participants accepted in Wave 1 (unique assignment).
    /// </summary>
    public int Wave1Count => Accepted.Count(a => a.Wave == 1);
    
    /// <summary>
    /// Number of participants accepted in Wave 2 (fill remaining).
    /// </summary>
    public int Wave2Count => Accepted.Count(a => a.Wave == 2);
    
    /// <summary>
    /// Number of participants on the waitlist.
    /// </summary>
    public int WaitlistCount => Waitlisted.Count();
}
