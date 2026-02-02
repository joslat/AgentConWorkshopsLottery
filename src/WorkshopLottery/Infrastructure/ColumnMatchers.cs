namespace WorkshopLottery.Infrastructure;

/// <summary>
/// Defines the fuzzy column matchers for MS Forms Excel exports.
/// Implements ADR-005: Fuzzy Column Matching Strategy.
/// </summary>
public static class ColumnMatchers
{
    /// <summary>
    /// Case-insensitive contains check for header matching.
    /// </summary>
    private static bool ContainsIgnoreCase(string header, string value) =>
        header.Contains(value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// All column matchers in priority order.
    /// Order matters: Email must be checked before Name since "email address" contains "address".
    /// </summary>
    public static readonly ColumnMatcher[] All =
    [
        // Email must be checked before Name (both might contain "address")
        new ColumnMatcher(
            "Email",
            h => ContainsIgnoreCase(h, "email"),
            isRequired: true),

        // Name - exclude email to avoid false match on "email address"
        new ColumnMatcher(
            "FullName",
            h => ContainsIgnoreCase(h, "name") && !ContainsIgnoreCase(h, "email"),
            isRequired: true),

        // Laptop requirement - required for eligibility
        new ColumnMatcher(
            "Laptop",
            h => ContainsIgnoreCase(h, "laptop"),
            isRequired: true),

        // Commit 10 minutes early - required for eligibility
        new ColumnMatcher(
            "Commit10Min",
            h => ContainsIgnoreCase(h, "commit") ||
                 ContainsIgnoreCase(h, "10 min") ||
                 ContainsIgnoreCase(h, "before") ||
                 ContainsIgnoreCase(h, "early"),
            isRequired: true),

        // Workshop 1 request - optional (might use only rankings)
        new ColumnMatcher(
            "RequestedW1",
            h => ContainsIgnoreCase(h, "workshop 1"),
            isRequired: false),

        // Workshop 2 request - optional
        new ColumnMatcher(
            "RequestedW2",
            h => ContainsIgnoreCase(h, "workshop 2"),
            isRequired: false),

        // Workshop 3 request - optional
        new ColumnMatcher(
            "RequestedW3",
            h => ContainsIgnoreCase(h, "workshop 3"),
            isRequired: false),

        // Rankings - optional but important for weighted lottery
        new ColumnMatcher(
            "Rankings",
            h => ContainsIgnoreCase(h, "rank"),
            isRequired: false),
    ];
    
    /// <summary>
    /// Gets a matcher by field name.
    /// </summary>
    public static ColumnMatcher? GetByFieldName(string fieldName) =>
        All.FirstOrDefault(m => m.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
    
    /// <summary>
    /// Gets all required matchers.
    /// </summary>
    public static IEnumerable<ColumnMatcher> Required => All.Where(m => m.IsRequired);
    
    /// <summary>
    /// Gets all optional matchers.
    /// </summary>
    public static IEnumerable<ColumnMatcher> Optional => All.Where(m => !m.IsRequired);
}
