using System.CommandLine;
using WorkshopLottery.Models;
using WorkshopLottery.Services;

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

// Build root command
var rootCommand = new RootCommand("🎰 Workshop Lottery - Fair workshop seat assignment using weighted lottery")
{
    inputOption,
    outputOption,
    seedOption,
    capacityOption,
    verboseOption
};

rootCommand.SetHandler(
    (FileInfo input, FileInfo? output, int? seed, int capacity, bool verbose) =>
    {
        try
        {
            // Validate input file exists
            if (!input.Exists)
            {
                SummaryLogger.LogError($"Input file not found: {input.FullName}");
                Environment.Exit(1);
                return;
            }

            // Determine output path
            var outputPath = output?.FullName 
                ?? Path.Combine(
                    input.DirectoryName ?? ".",
                    Path.GetFileNameWithoutExtension(input.Name) + "_results.xlsx");

            // Print banner
            Console.WriteLine();
            Console.WriteLine("🎰 ═══════════════════════════════════════════════════════════ 🎰");
            Console.WriteLine("              WORKSHOP LOTTERY SYSTEM v1.0                       ");
            Console.WriteLine("        Fair seat assignment using weighted lottery              ");
            Console.WriteLine("🎰 ═══════════════════════════════════════════════════════════ 🎰");
            Console.WriteLine();

            // Run the lottery
            var orchestrator = LotteryOrchestrator.CreateDefault();
            var result = orchestrator.Run(
                input.FullName,
                outputPath,
                seed,
                capacity);

            SummaryLogger.LogSuccess("🎸 Lottery completed successfully! 🎸");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            SummaryLogger.LogError($"Lottery failed: {ex.Message}");
            if (verbose)
            {
                Console.WriteLine(ex.StackTrace);
            }
            Environment.Exit(1);
        }
    },
    inputOption, outputOption, seedOption, capacityOption, verboseOption);

// Run the CLI
return await rootCommand.InvokeAsync(args);
