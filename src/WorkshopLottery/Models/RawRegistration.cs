namespace WorkshopLottery.Models;

/// <summary>
/// Represents raw data extracted from Excel before validation.
/// This is the intermediate representation before converting to fully validated Registration.
/// </summary>
public record RawRegistration
{
    /// <summary>
    /// The row number in the Excel file (for error reporting).
    /// </summary>
    public int RowNumber { get; init; }
    
    /// <summary>
    /// Raw full name value from the Excel file.
    /// </summary>
    public string? FullName { get; init; }
    
    /// <summary>
    /// Raw email value from the Excel file.
    /// </summary>
    public string? Email { get; init; }
    
    /// <summary>
    /// Raw response for laptop question (e.g., "Yes", "No", "Yes, I will bring a laptop").
    /// </summary>
    public string? LaptopResponse { get; init; }
    
    /// <summary>
    /// Raw response for 10-minute commitment question.
    /// </summary>
    public string? Commit10MinResponse { get; init; }
    
    /// <summary>
    /// Raw response for Workshop 1 attendance request.
    /// </summary>
    public string? RequestedW1Response { get; init; }
    
    /// <summary>
    /// Raw response for Workshop 2 attendance request.
    /// </summary>
    public string? RequestedW2Response { get; init; }
    
    /// <summary>
    /// Raw response for Workshop 3 attendance request.
    /// </summary>
    public string? RequestedW3Response { get; init; }
    
    /// <summary>
    /// Raw rankings response (semicolon-separated workshop preferences).
    /// Example: "Workshop 2 – AI Architecture;Workshop 1 – Secure;Workshop 3 – Pizza"
    /// </summary>
    public string? RankingsResponse { get; init; }
}
