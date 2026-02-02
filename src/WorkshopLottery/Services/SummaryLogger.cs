namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

/// <summary>
/// Provides console logging for lottery results and progress.
/// </summary>
public static class SummaryLogger
{
    /// <summary>
    /// Logs the complete lottery result summary to the console.
    /// </summary>
    public static void LogResults(LotteryResult result)
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    🎲 LOTTERY RESULTS 🎲                       ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();

        LogOverallStats(result);
        Console.WriteLine();

        foreach (var workshop in result.Results.Values)
        {
            LogWorkshopResult(workshop, result.Capacity);
        }

        LogDisqualified(result);

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
    }

    /// <summary>
    /// Logs progress message during lottery execution.
    /// </summary>
    public static void LogProgress(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public static void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN] {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Logs a success message.
    /// </summary>
    public static void LogSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[OK] {message}");
        Console.ResetColor();
    }

    private static void LogOverallStats(LotteryResult result)
    {
        var totalAssigned = result.Results.Values.Sum(w => w.AcceptedCount);
        var totalWaitlisted = result.Results.Values.Sum(w => w.WaitlistCount);
        var uniqueParticipants = result.Results.Values
            .SelectMany(w => w.Accepted)
            .Select(a => a.Registration.Email.ToLowerInvariant())
            .Distinct()
            .Count();

        Console.WriteLine("📊 OVERALL STATISTICS");
        Console.WriteLine("───────────────────────────────────────────────────────────────");
        Console.WriteLine($"   Total Registrations:     {result.TotalRegistrations}");
        Console.WriteLine($"   Eligible:                {result.EligibleCount}");
        Console.WriteLine($"   Disqualified:            {result.DisqualifiedCount}");
        Console.WriteLine($"   Total Seat Assignments:  {totalAssigned}");
        Console.WriteLine($"   Unique Participants:     {uniqueParticipants}");
        Console.WriteLine($"   Total Waitlisted:        {totalWaitlisted}");
        Console.WriteLine($"   Random Seed:             {result.Seed}");
    }

    private static void LogWorkshopResult(WorkshopResult workshop, int capacity)
    {
        var assigned = workshop.Accepted.ToList();
        var waitlisted = workshop.Waitlisted.ToList();
        var wave1 = workshop.Wave1Count;
        var wave2 = workshop.Wave2Count;
        var lowPriority = workshop.LowPriorityCount;

        Console.WriteLine($"🎯 {GetWorkshopDisplayName(workshop.WorkshopId)}");
        Console.WriteLine("───────────────────────────────────────────────────────────────");
        Console.WriteLine($"   Capacity:    {capacity}");
        Console.WriteLine($"   Assigned:    {assigned.Count} (Wave 1: {wave1}, Wave 2: {wave2}, Low Priority: {lowPriority})");
        Console.WriteLine($"   Waitlisted:  {waitlisted.Count}");
        Console.WriteLine($"   Fill Rate:   {(capacity > 0 ? assigned.Count * 100 / capacity : 0)}%");
        Console.WriteLine();

        if (assigned.Count > 0)
        {
            Console.WriteLine("   Assigned Participants:");
            int order = 1;
            foreach (var assignment in assigned.OrderBy(a => a.Wave).ThenBy(a => a.Order))
            {
                var waveIndicator = assignment.IsLowPriority ? "🟠" : (assignment.Wave == 1 ? "🟢" : "🟡");
                Console.WriteLine($"      {order,2}. {waveIndicator} {assignment.Registration.FullName} ({assignment.Registration.Email})");
                order++;
            }
            Console.WriteLine();
        }

        if (waitlisted.Count > 0)
        {
            Console.WriteLine("   Waitlist:");
            int order = 1;
            foreach (var assignment in waitlisted.OrderBy(a => a.Order))
            {
                Console.WriteLine($"      {order,2}. ⚪ {assignment.Registration.FullName} ({assignment.Registration.Email})");
                order++;
            }
            Console.WriteLine();
        }
    }

    private static void LogDisqualified(LotteryResult result)
    {
        if (result.DisqualifiedCount == 0)
            return;

        Console.WriteLine("⛔ DISQUALIFIED REGISTRATIONS");
        Console.WriteLine("───────────────────────────────────────────────────────────────");

        // Log by reason if available
        if (result.DisqualificationReasons.Count > 0)
        {
            foreach (var (reason, count) in result.DisqualificationReasons)
            {
                Console.WriteLine($"   {count}x {reason}");
            }
        }
        else
        {
            Console.WriteLine($"   Total: {result.DisqualifiedCount} registrations did not meet eligibility criteria.");
        }
        Console.WriteLine();
    }

    private static string GetWorkshopDisplayName(WorkshopId workshopId)
    {
        return workshopId switch
        {
            WorkshopId.W1 => "Workshop 1 – Secure Coding Literacy for Vibe Coders",
            WorkshopId.W2 => "Workshop 2 – AI Architecture Critic",
            WorkshopId.W3 => "Workshop 3 – Build a Pizza Ordering Agent with Microsoft Foundry and MCP",
            _ => workshopId.ToString()
        };
    }
}
