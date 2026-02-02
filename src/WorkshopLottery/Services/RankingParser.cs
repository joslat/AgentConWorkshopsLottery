using WorkshopLottery.Models;

namespace WorkshopLottery.Services;

/// <summary>
/// Parses workshop ranking responses from MS Forms exports.
/// Rankings are semicolon-separated workshop names in preference order.
/// </summary>
public static class RankingParser
{
    /// <summary>
    /// Parses the ranking field into per-workshop ranks.
    /// </summary>
    /// <param name="rankingsField">
    /// The raw rankings field from Excel.
    /// Example: "Workshop 2 – AI Architecture;Workshop 1 – Secure;Workshop 3 – Pizza"
    /// </param>
    /// <returns>Dictionary mapping WorkshopId to rank (1-indexed position in the input).</returns>
    /// <example>
    /// Input: "Workshop 2 – AI Architecture;Workshop 1 – Secure;Workshop 3 – Pizza"
    /// Returns: { W2: 1, W1: 2, W3: 3 }
    /// </example>
    public static Dictionary<WorkshopId, int> ParseRankings(string? rankingsField)
    {
        var result = new Dictionary<WorkshopId, int>();

        if (string.IsNullOrWhiteSpace(rankingsField))
            return result;

        var segments = rankingsField.Split(';', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i].Trim();
            var rank = i + 1; // 1-indexed position

            // Detect which workshop this segment refers to
            var workshopId = DetectWorkshop(segment);
            if (workshopId.HasValue)
            {
                // Only add if not already present (first occurrence wins)
                result.TryAdd(workshopId.Value, rank);
            }
        }

        return result;
    }

    /// <summary>
    /// Detects which workshop a ranking segment refers to.
    /// </summary>
    private static WorkshopId? DetectWorkshop(string segment)
    {
        // Check for workshop identifiers (case-insensitive)
        // Support various formats: "Workshop 1", "Workshop1", "workshop 1", etc.
        
        if (ContainsWorkshop(segment, "1"))
            return WorkshopId.W1;
        
        if (ContainsWorkshop(segment, "2"))
            return WorkshopId.W2;
        
        if (ContainsWorkshop(segment, "3"))
            return WorkshopId.W3;

        // Unknown workshop format
        return null;
    }

    /// <summary>
    /// Checks if a segment contains a workshop reference with the given number.
    /// </summary>
    private static bool ContainsWorkshop(string segment, string number)
    {
        // Match "Workshop 1", "Workshop1", "workshop 1", etc.
        return segment.Contains($"Workshop {number}", StringComparison.OrdinalIgnoreCase) ||
               segment.Contains($"Workshop{number}", StringComparison.OrdinalIgnoreCase);
    }
}
