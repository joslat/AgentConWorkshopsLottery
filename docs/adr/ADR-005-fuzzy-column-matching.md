# ADR-005: Fuzzy Column Header Matching Strategy

**Status:** Accepted  
**Date:** 2026-02-02  
**Context:** Handling variable column headers in MS Forms exports

---

## Context

Microsoft Forms exports have column headers that may vary:
- Exact wording depends on form question text
- May include ellipsis for long questions
- User might edit form questions over time
- Need robust matching without exact string comparison

Example variations:
- "Full name" vs "Your full name" vs "Name"
- "Will you bring a laptop to the workshop?" vs "laptop (required)"
- "Workshop 1 – It's Giving Insecure..." vs "Do you want to attend Workshop 1..."

## Decision Drivers

- Robustness across form variations
- Avoid false positives (matching wrong columns)
- Maintainability
- Clear matching logic for debugging

## Options Considered

### Option 1: Contains-Based Matching with Priority Keywords
- Check if header contains specific keywords
- Use priority/exclusion rules for disambiguation

### Option 2: Regex Patterns
- Define regex for each column type
- More precise but harder to maintain

### Option 3: Exact Match with Configuration
- User provides column mappings in config file
- Most flexible but adds user burden

### Option 4: Machine Learning / Fuzzy Matching
- Levenshtein distance or similar
- Over-engineered for this use case

## Decision

**Option 1: Contains-Based Matching with Priority Keywords**

## Rationale

1. **Simple and Effective:** Most MS Forms columns have distinctive keywords
2. **Easy to Debug:** Log which pattern matched which column
3. **Maintainable:** Add new patterns without regex complexity
4. **Good Enough:** Perfect matching not required; validate at runtime

## Matching Rules

```csharp
// Priority-ordered matchers
private static readonly ColumnMatcher[] Matchers = new[]
{
    // Email (must check before Name since "email address" contains "address")
    new ColumnMatcher("Email", 
        h => h.Contains("email", OrdinalIgnoreCase)),
    
    // Name (exclude email to avoid false match)
    new ColumnMatcher("FullName", 
        h => h.Contains("name", OrdinalIgnoreCase) 
          && !h.Contains("email", OrdinalIgnoreCase)),
    
    // Laptop
    new ColumnMatcher("HasLaptop", 
        h => h.Contains("laptop", OrdinalIgnoreCase)),
    
    // Commit 10 min
    new ColumnMatcher("WillCommit10Min", 
        h => h.Contains("commit", OrdinalIgnoreCase) 
          || h.Contains("10 min", OrdinalIgnoreCase)
          || h.Contains("early", OrdinalIgnoreCase)),
    
    // Workshop requests (order matters: check specific workshop IDs)
    new ColumnMatcher("RequestedW1", 
        h => h.Contains("workshop 1", OrdinalIgnoreCase)),
    new ColumnMatcher("RequestedW2", 
        h => h.Contains("workshop 2", OrdinalIgnoreCase)),
    new ColumnMatcher("RequestedW3", 
        h => h.Contains("workshop 3", OrdinalIgnoreCase)),
    
    // Rankings
    new ColumnMatcher("Rankings", 
        h => h.Contains("rank", OrdinalIgnoreCase)),
};
```

## Validation

After matching:
1. Ensure all required columns found (Name, Email, Laptop, Commit)
2. Log warnings for missing optional columns
3. Fail fast with clear error message if critical column missing

```
Found columns:
  Name → Column B "Full name"
  Email → Column C "Email address"  
  Laptop → Column D "Will you bring a laptop..."
  ...
Missing columns: Rankings (will treat unranked as rank 3)
```

## Consequences

### Positive
- Works for common MS Forms variations
- Easy to add new matchers
- Clear logging for troubleshooting
- Graceful handling of missing optional columns

### Negative
- May fail on unusual form designs
- User cannot override without code change

### Mitigations
- Document expected form structure
- Provide clear error messages
- Consider future config option for explicit column mapping

### Validation
- Sample Excel file available: `input/AgentCon Zurich – Workshop Signup (Lottery + Standby)(1-7).xlsx`
- Analysis tool provided: `tools/quick-excel-check.csx` to verify matching patterns
- Real-world testing of fuzzy matching logic against actual MS Forms export

## Related Documents
- [ARCHITECTURE.md](../ARCHITECTURE.md) - Column mapping in Section 8
