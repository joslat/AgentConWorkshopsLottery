namespace WorkshopLottery.Models;

/// <summary>
/// Configuration options for running the lottery.
/// </summary>
public record LotteryConfiguration
{
    /// <summary>
    /// Path to the input Excel file (MS Forms export).
    /// </summary>
    public required string InputPath { get; init; }
    
    /// <summary>
    /// Path for the output Excel file. Defaults to "WorkshopAssignments.xlsx".
    /// </summary>
    public string OutputPath { get; init; } = "WorkshopAssignments.xlsx";
    
    /// <summary>
    /// Number of seats available per workshop. Defaults to 34.
    /// </summary>
    public int Capacity { get; init; } = 34;
    
    /// <summary>
    /// Random seed for reproducible lottery results.
    /// If null, uses the current date in YYYYMMDD format.
    /// </summary>
    public int? Seed { get; init; }
    
    /// <summary>
    /// Order in which workshops are processed for assignment.
    /// Earlier workshops get priority in Wave 1.
    /// </summary>
    public List<WorkshopId> WorkshopOrder { get; init; } = [WorkshopId.W1, WorkshopId.W2, WorkshopId.W3];
    
    /// <summary>
    /// Gets the effective seed (provided seed or date-based default).
    /// </summary>
    public int GetEffectiveSeed() => Seed ?? int.Parse(DateTime.Now.ToString("yyyyMMdd"));
}
