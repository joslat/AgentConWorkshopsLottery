namespace WorkshopLottery.Extensions;

/// <summary>
/// Extension methods for parsing form responses.
/// </summary>
public static class ParsingExtensions
{
    /// <summary>
    /// Parses Yes/No responses (case-insensitive).
    /// Also supports common variations and localized versions.
    /// </summary>
    /// <param name="value">The response value to parse.</param>
    /// <returns>True if the response indicates yes, false otherwise.</returns>
    public static bool ParseYesNo(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim().ToLowerInvariant();
        
        // Check for explicit yes values
        return normalized switch
        {
            "yes" => true,
            "y" => true,
            "ja" => true,      // German
            "oui" => true,     // French
            "sí" => true,      // Spanish
            "si" => true,      // Spanish (no accent)
            "true" => true,
            "1" => true,
            _ => normalized.StartsWith("yes", StringComparison.OrdinalIgnoreCase) // Handles "Yes, I will bring..."
        };
    }

    /// <summary>
    /// Normalizes email for comparison (trim + lowercase).
    /// </summary>
    /// <param name="email">The email to normalize.</param>
    /// <returns>Normalized email string.</returns>
    public static string NormalizeEmail(this string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Checks if a string is null, empty, or whitespace.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Returns a trimmed version of the string, or empty if null.
    /// </summary>
    public static string TrimOrEmpty(this string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
