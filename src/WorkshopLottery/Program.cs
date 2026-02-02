using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using WorkshopLottery.Models;
using WorkshopLottery.Services;

// Set console encoding to UTF-8 for proper emoji display
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Define CLI options
var inputOption = new Option<FileInfo>(
    name: "--input",
    description: "Path to the input Excel file (MS Forms export)")
{
    IsRequired = true
};
inputOption.AddAlias("-i");

var outputOption = new Option<FileInfo?>(
    name: "--output",
    description: "Path to the output Excel file (defaults to input filename with _results suffix)")
{
    IsRequired = false
};
outputOption.AddAlias("-o");

var seedOption = new Option<int?>(
    name: "--seed",
    description: "Random seed for reproducible results (optional)")
{
    IsRequired = false
};
seedOption.AddAlias("-s");

var capacityOption = new Option<int>(
    name: "--capacity",
    description: "Capacity per workshop (default: 34)",
    getDefaultValue: () => 34)
{
    IsRequired = false
};
capacityOption.AddAlias("-c");

var verboseOption = new Option<bool>(
    name: "--verbose",
    description: "Enable verbose output",
    getDefaultValue: () => false);
verboseOption.AddAlias("-v");

var spectacularOption = new Option<bool>(
    name: "--spectacular",
    description: "Enable dramatic animations and visual effects 🎪",
    getDefaultValue: () => false);
spectacularOption.AddAlias("-S");
spectacularOption.AddAlias("--show");

// Speed options as separate boolean flags (mutually exclusive, higher wins)
var slowOption = new Option<bool>(
    name: "--slow",
    description: "Slow animations (2x slower) - auto-enables spectacular mode",
    getDefaultValue: () => false);

var slowerOption = new Option<bool>(
    name: "--slower",
    description: "Slower animations (3x slower) - auto-enables spectacular mode",
    getDefaultValue: () => false);

var slowestOption = new Option<bool>(
    name: "--slowest",
    description: "Slowest animations (5x slower) - auto-enables spectacular mode",
    getDefaultValue: () => false);

// Build root command
var rootCommand = new RootCommand("🎰 Workshop Lottery - Fair workshop seat assignment using weighted lottery")
{
    inputOption,
    outputOption,
    seedOption,
    capacityOption,
    verboseOption,
    spectacularOption,
    slowOption,
    slowerOption,
    slowestOption
};

rootCommand.SetHandler(async (InvocationContext context) =>
{
    var input = context.ParseResult.GetValueForOption(inputOption)!;
    var output = context.ParseResult.GetValueForOption(outputOption);
    var seed = context.ParseResult.GetValueForOption(seedOption);
    var capacity = context.ParseResult.GetValueForOption(capacityOption);
    var verbose = context.ParseResult.GetValueForOption(verboseOption);
    var spectacular = context.ParseResult.GetValueForOption(spectacularOption);
    var slow = context.ParseResult.GetValueForOption(slowOption);
    var slower = context.ParseResult.GetValueForOption(slowerOption);
    var slowest = context.ParseResult.GetValueForOption(slowestOption);

    try
    {
        // Validate input file exists
        if (!input.Exists)
        {
            Console.WriteLine($"[ERROR] Input file not found: {input.FullName}");
            context.ExitCode = 1;
            return;
        }

        // Determine output path
        var outputPath = output?.FullName 
            ?? Path.Combine(
                input.DirectoryName ?? ".",
                Path.GetFileNameWithoutExtension(input.Name) + "_results.xlsx");

        // Calculate speed multiplier (highest wins if multiple specified)
        var speedMultiplier = slowest ? 5.0 : slower ? 3.0 : slow ? 2.0 : 1.0;

        // Any speed flag auto-enables spectacular mode
        var useSpectacular = spectacular || slow || slower || slowest;

        LotteryResult result;

        if (useSpectacular)
        {
            // 🎪 SPECTACULAR MODE - With animations and dramatic reveals!
            result = await RunSpectacularLotteryAsync(input.FullName, outputPath, seed, capacity, speedMultiplier);
        }
        else
        {
            // Standard mode - fast, no animations
            result = RunStandardLottery(input.FullName, outputPath, seed, capacity);
        }

        if (useSpectacular)
        {
            Console.WriteLine();
            Console.WriteLine("🎸 Lottery completed successfully! 🎸");
        }
        else
        {
            SummaryLogger.LogSuccess("🎸 Lottery completed successfully! 🎸");
        }
        
        context.ExitCode = 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Lottery failed: {ex.Message}");
        if (verbose)
        {
            Console.WriteLine(ex.StackTrace);
        }
        context.ExitCode = 1;
    }
});

// Run the CLI
return await rootCommand.InvokeAsync(args);

// ============================================================================
// Standard Mode - Fast execution without animations
// ============================================================================
static LotteryResult RunStandardLottery(string inputPath, string outputPath, int? seed, int capacity)
{
    // Print banner
    Console.WriteLine();
    Console.WriteLine("🎰 ═══════════════════════════════════════════════════════════ 🎰");
    Console.WriteLine("              WORKSHOP LOTTERY SYSTEM v1.0                       ");
    Console.WriteLine("        Fair seat assignment using weighted lottery              ");
    Console.WriteLine("🎰 ═══════════════════════════════════════════════════════════ 🎰");
    Console.WriteLine();

    // Run the lottery
    var orchestrator = LotteryOrchestrator.CreateDefault();
    return orchestrator.Run(inputPath, outputPath, seed, capacity);
}

// ============================================================================
// Spectacular Mode - With dramatic animations and colorful output! 🎪
// ============================================================================
static async Task<LotteryResult> RunSpectacularLotteryAsync(
    string inputPath, 
    string outputPath, 
    int? seed, 
    int capacity,
    double speedMultiplier = 1.0)
{
    var renderer = new SpectacularRenderer(speedMultiplier);
    
    // Services
    var parser = new ExcelParserService();
    var validator = new ValidationService();
    var lotteryEngine = new LotteryEngine();
    var writer = new ExcelWriterService();

    // Show banner
    await renderer.ShowBannerAsync();
    
    // Show configuration
    await renderer.ShowStartAsync(inputPath, outputPath, seed);

    // Step 1: Parse Excel
    var rawRegistrations = parser.ParseRegistrations(inputPath);
    await renderer.ShowParsingAsync(rawRegistrations.Count);

    // Step 2: Validate
    var validationResult = validator.ValidateAndFilter(rawRegistrations);
    await renderer.ShowValidationAsync(validationResult);

    // Step 3: Build configuration
    var config = new LotteryConfiguration
    {
        InputPath = inputPath,
        OutputPath = outputPath,
        Seed = seed,
        Capacity = capacity
    };

    // Step 4: Run lottery (with dramatic buildup!)
    await renderer.ShowLotteryStartAsync(validationResult.EligibleRegistrations.Count, capacity);
    
    var result = lotteryEngine.RunLottery(validationResult.EligibleRegistrations, config);

    // Step 4b: Fill remaining seats with disqualified participants
    FillRemainingSeatsWithDisqualified(result, validationResult.DisqualifiedRegistrations, capacity);

    // Update result with validation counts
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

    // Step 5: Show results for each workshop (with countdown reveals!)
    foreach (var workshopId in new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 })
    {
        if (finalResult.Results.TryGetValue(workshopId, out var workshopResult))
        {
            await renderer.ShowWorkshopResultAsync(workshopId, workshopResult);
        }
    }

    // Step 6: Write output
    writer.WriteResults(outputPath, finalResult);
    renderer.ShowSuccess($"Results written to: {outputPath}");

    // Step 7: Final summary
    await renderer.ShowFinalSummaryAsync(finalResult);

    return finalResult;
}

// Helper method for filling remaining seats (duplicated from orchestrator for spectacular mode)
static void FillRemainingSeatsWithDisqualified(
    LotteryResult result,
    IReadOnlyList<Registration> disqualifiedRegistrations,
    int capacity)
{
    if (disqualifiedRegistrations.Count == 0)
        return;

    foreach (var workshopId in new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 })
    {
        if (!result.Results.TryGetValue(workshopId, out var workshopResult))
            continue;

        var emptySeats = capacity - workshopResult.AcceptedCount;
        if (emptySeats <= 0)
            continue;

        var candidates = disqualifiedRegistrations
            .Where(r => r.WorkshopPreferences.TryGetValue(workshopId, out var pref) && pref.Requested)
            .ToList();

        if (candidates.Count == 0)
            continue;

        var nextOrder = workshopResult.Assignments.Count + 1;
        var seatsToFill = Math.Min(emptySeats, candidates.Count);

        for (int i = 0; i < seatsToFill; i++)
        {
            var candidate = candidates[i];
            workshopResult.Assignments.Add(new WorkshopAssignment
            {
                Registration = candidate,
                Status = AssignmentStatus.Accepted,
                Wave = 3,
                Order = nextOrder++,
                IsLowPriority = true
            });
        }
    }
}
