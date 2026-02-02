namespace WorkshopLottery.Infrastructure;

/// <summary>
/// Represents a mapping between a logical field name and an Excel column.
/// </summary>
public record ColumnMapping
{
    /// <summary>
    /// The logical field name (e.g., "Email", "FullName").
    /// </summary>
    public required string FieldName { get; init; }
    
    /// <summary>
    /// The 1-based column index in the Excel file, or null if not found.
    /// </summary>
    public int? ColumnIndex { get; init; }
    
    /// <summary>
    /// The actual header text that was matched, or null if not found.
    /// </summary>
    public string? MatchedHeader { get; init; }
    
    /// <summary>
    /// Indicates whether this column was found in the Excel file.
    /// </summary>
    public bool IsFound => ColumnIndex.HasValue;
}

/// <summary>
/// Defines a column matching rule for fuzzy header matching.
/// </summary>
public class ColumnMatcher
{
    /// <summary>
    /// The logical field name this matcher is for.
    /// </summary>
    public string FieldName { get; }
    
    /// <summary>
    /// The function that determines if a header matches this field.
    /// </summary>
    public Func<string, bool> Matcher { get; }
    
    /// <summary>
    /// Whether this column is required for parsing to succeed.
    /// </summary>
    public bool IsRequired { get; }
    
    /// <summary>
    /// Creates a new column matcher.
    /// </summary>
    /// <param name="fieldName">The logical field name.</param>
    /// <param name="matcher">The matching function.</param>
    /// <param name="isRequired">Whether this column is required.</param>
    public ColumnMatcher(string fieldName, Func<string, bool> matcher, bool isRequired = true)
    {
        FieldName = fieldName;
        Matcher = matcher;
        IsRequired = isRequired;
    }
}
