namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

/// <summary>
/// Interface for rendering lottery output to the console.
/// Allows switching between normal and spectacular modes.
/// </summary>
public interface IConsoleRenderer
{
    /// <summary>
    /// Display the application banner/header.
    /// </summary>
    Task ShowBannerAsync();

    /// <summary>
    /// Show the start of the lottery process.
    /// </summary>
    Task ShowStartAsync(string inputPath, string outputPath, int? seed);

    /// <summary>
    /// Show parsing progress.
    /// </summary>
    Task ShowParsingAsync(int registrationCount);

    /// <summary>
    /// Show validation results.
    /// </summary>
    Task ShowValidationAsync(ValidationResult result);

    /// <summary>
    /// Show lottery execution progress.
    /// </summary>
    Task ShowLotteryStartAsync(int eligibleCount, int capacity);

    /// <summary>
    /// Show results for a single workshop.
    /// </summary>
    Task ShowWorkshopResultAsync(WorkshopId workshopId, WorkshopResult result);

    /// <summary>
    /// Show the final summary of all results.
    /// </summary>
    Task ShowFinalSummaryAsync(LotteryResult result);

    /// <summary>
    /// Show success message.
    /// </summary>
    void ShowSuccess(string message);

    /// <summary>
    /// Show error message.
    /// </summary>
    void ShowError(string message);

    /// <summary>
    /// Show info message.
    /// </summary>
    void ShowInfo(string message);

    /// <summary>
    /// Show warning message.
    /// </summary>
    void ShowWarning(string message);
}
