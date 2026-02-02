namespace WorkshopLottery.Services;

using WorkshopLottery.Models;

/// <summary>
/// Orchestrates the complete lottery workflow from input to output.
/// </summary>
public interface ILotteryOrchestrator
{
    /// <summary>
    /// Runs the complete lottery workflow.
    /// </summary>
    /// <param name="inputPath">Path to the input Excel file.</param>
    /// <param name="outputPath">Path to write the output Excel file.</param>
    /// <param name="seed">Optional random seed for reproducibility.</param>
    /// <param name="capacity">Workshop capacity (default: 34).</param>
    /// <returns>The lottery result.</returns>
    LotteryResult Run(
        string inputPath,
        string outputPath,
        int? seed = null,
        int? capacity = null);
}

/// <summary>
/// Default implementation of the lottery orchestrator.
/// </summary>
public class LotteryOrchestrator : ILotteryOrchestrator
{
    private readonly IExcelParserService _parser;
    private readonly IValidationService _validator;
    private readonly ILotteryEngine _lotteryEngine;
    private readonly IExcelWriterService _writer;

    public LotteryOrchestrator(
        IExcelParserService parser,
        IValidationService validator,
        ILotteryEngine lotteryEngine,
        IExcelWriterService writer)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _lotteryEngine = lotteryEngine ?? throw new ArgumentNullException(nameof(lotteryEngine));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <inheritdoc />
    public LotteryResult Run(
        string inputPath,
        string outputPath,
        int? seed = null,
        int? capacity = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        SummaryLogger.LogProgress($"Starting lottery process...");
        SummaryLogger.LogProgress($"Input file: {inputPath}");
        SummaryLogger.LogProgress($"Output file: {outputPath}");
        if (seed.HasValue)
        {
            SummaryLogger.LogProgress($"Random seed: {seed.Value}");
        }

        // Step 1: Parse Excel
        SummaryLogger.LogProgress("Parsing Excel file...");
        var rawRegistrations = _parser.ParseRegistrations(inputPath);
        SummaryLogger.LogSuccess($"Parsed {rawRegistrations.Count} raw registrations.");

        // Step 2: Validate and convert
        SummaryLogger.LogProgress("Validating registrations...");
        var validationResult = _validator.ValidateAndFilter(rawRegistrations);
        SummaryLogger.LogSuccess($"Eligible: {validationResult.EligibleRegistrations.Count}, Disqualified: {validationResult.DisqualifiedRegistrations.Count}");

        if (validationResult.DisqualifiedRegistrations.Count > 0)
        {
            SummaryLogger.LogWarning($"{validationResult.DisqualifiedRegistrations.Count} registrations were disqualified.");
        }

        // Step 3: Build configuration
        var config = new LotteryConfiguration
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            Seed = seed,
            Capacity = capacity ?? 34
        };

        // Step 4: Run lottery
        SummaryLogger.LogProgress("Running weighted lottery...");
        var result = _lotteryEngine.RunLottery(validationResult.EligibleRegistrations, config);
        SummaryLogger.LogSuccess("Lottery complete!");

        // Update result with validation counts (LotteryEngine only sees eligible registrations)
        var finalResult = new LotteryResult
        {
            Seed = result.Seed,
            Capacity = result.Capacity,
            Results = result.Results,
            TotalRegistrations = rawRegistrations.Count,
            EligibleCount = validationResult.EligibleRegistrations.Count,
            DisqualifiedCount = validationResult.DisqualifiedRegistrations.Count,
            DisqualificationReasons = validationResult.DisqualificationReasons
        };

        // Step 5: Write output
        SummaryLogger.LogProgress("Writing output Excel file...");
        _writer.WriteResults(outputPath, finalResult);
        SummaryLogger.LogSuccess($"Results written to: {outputPath}");

        // Step 6: Log summary
        SummaryLogger.LogResults(finalResult);

        return finalResult;
    }

    /// <summary>
    /// Creates a default orchestrator with all standard services.
    /// </summary>
    public static LotteryOrchestrator CreateDefault()
    {
        return new LotteryOrchestrator(
            new ExcelParserService(),
            new ValidationService(),
            new LotteryEngine(),
            new ExcelWriterService());
    }
}
