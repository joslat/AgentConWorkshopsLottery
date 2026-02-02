namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

/// <summary>
/// Simple console renderer that outputs text without animations.
/// This is the default mode for fast, non-interactive output.
/// </summary>
public class SimpleConsoleRenderer : IConsoleRenderer
{
    public Task ShowBannerAsync()
    {
        Console.WriteLine();
        Console.WriteLine("🎰 ═══════════════════════════════════════════════════════════ 🎰");
        Console.WriteLine("              WORKSHOP LOTTERY SYSTEM v1.0                       ");
        Console.WriteLine("        Fair seat assignment using weighted lottery              ");
        Console.WriteLine("🎰 ═══════════════════════════════════════════════════════════ 🎰");
        Console.WriteLine();
        return Task.CompletedTask;
    }

    public Task ShowStartAsync(string inputPath, string outputPath, int? seed)
    {
        ShowInfo("Starting lottery process...");
        ShowInfo($"Input file: {inputPath}");
        ShowInfo($"Output file: {outputPath}");
        if (seed.HasValue)
        {
            ShowInfo($"Random seed: {seed.Value}");
        }
        return Task.CompletedTask;
    }

    public Task ShowParsingAsync(int registrationCount)
    {
        ShowInfo("Parsing Excel file...");
        return Task.CompletedTask;
    }

    public Task ShowValidationAsync(ValidationResult result)
    {
        ShowInfo("Validating registrations...");
        Console.WriteLine();
        Console.WriteLine("✅ Validation Summary:");
        Console.WriteLine($"   Total registrations: {result.EligibleRegistrations.Count + result.DisqualifiedRegistrations.Count}");
        Console.WriteLine($"   Eligible: {result.EligibleRegistrations.Count}");
        Console.WriteLine($"   Disqualified: {result.DisqualifiedRegistrations.Count}");
        
        if (result.DisqualificationReasons.Count > 0)
        {
            Console.WriteLine("   Disqualification reasons:");
            foreach (var (reason, count) in result.DisqualificationReasons.OrderByDescending(kvp => kvp.Value))
            {
                Console.WriteLine($"      - {reason}: {count}");
            }
        }
        
        return Task.CompletedTask;
    }

    public Task ShowLotteryStartAsync(int eligibleCount, int capacity)
    {
        ShowInfo("Running weighted lottery...");
        Console.WriteLine();
        Console.WriteLine($"🎲 Running lottery");
        Console.WriteLine($"   Capacity per workshop: {capacity}");
        Console.WriteLine($"   Eligible participants: {eligibleCount}");
        return Task.CompletedTask;
    }

    public Task ShowWorkshopResultAsync(WorkshopId workshopId, WorkshopResult result)
    {
        Console.WriteLine();
        Console.WriteLine($"🎯 {workshopId}");
        Console.WriteLine($"   Accepted: {result.AcceptedCount} (Wave 1: {result.Wave1Count}, Wave 2: {result.Wave2Count}, Low Priority: {result.LowPriorityCount})");
        Console.WriteLine($"   Waitlist: {result.WaitlistCount}");
        return Task.CompletedTask;
    }

    public Task ShowFinalSummaryAsync(LotteryResult result)
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    🎲 LOTTERY COMPLETE 🎲                      ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine($"   Total Registrations: {result.TotalRegistrations}");
        Console.WriteLine($"   Eligible: {result.EligibleCount}");
        Console.WriteLine($"   Disqualified: {result.DisqualifiedCount}");
        Console.WriteLine($"   Random Seed: {result.Seed}");
        Console.WriteLine();
        return Task.CompletedTask;
    }

    public void ShowSuccess(string message)
    {
        Console.WriteLine($"[OK] {message}");
    }

    public void ShowError(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }

    public void ShowInfo(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public void ShowWarning(string message)
    {
        Console.WriteLine($"[WARN] {message}");
    }
}
