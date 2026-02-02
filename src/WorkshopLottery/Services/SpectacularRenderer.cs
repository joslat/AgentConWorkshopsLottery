namespace WorkshopLottery.Services;

using Spectre.Console;
using WorkshopLottery.Models;

/// <summary>
/// Spectacular console renderer with dramatic animations, spinners, and colorful output.
/// Activated with the --spectacular flag for a fun, engaging experience!
/// </summary>
public class SpectacularRenderer : IConsoleRenderer
{
    private readonly double _speedMultiplier;

    /// <summary>
    /// Creates a new SpectacularRenderer with the specified speed multiplier.
    /// </summary>
    /// <param name="speedMultiplier">Delay multiplier: 1.0 = normal, 2.0 = slow, 3.0 = slower, 5.0 = slowest</param>
    public SpectacularRenderer(double speedMultiplier = 1.0)
    {
        _speedMultiplier = speedMultiplier;
    }

    private int Delay(int baseMs) => (int)(baseMs * _speedMultiplier);

    private static readonly string[] BootMessages =
    [
        "⚡ Powering up quantum randomizer...",
        "🔌 Connecting to the fairness grid...",
        "💾 Loading probability matrices...",
        "🔋 Charging entropy capacitors...",
        "📡 Calibrating selection antennas...",
        "🧬 Sequencing random DNA...",
        "🌌 Aligning cosmic constants...",
        "⚙️ Warming up the algorithm cores..."
    ];

    private static readonly string[] ProcessingMessages =
    [
        "🎲 Rolling the dice of destiny...",
        "🔮 Consulting the probability oracle...",
        "⚡ Charging up the randomizer...",
        "🌟 Aligning the fairness crystals...",
        "🎰 Spinning the wheel of fortune...",
        "🧮 Crunching the quantum numbers...",
        "🎪 Preparing the grand reveal...",
        "🎭 Shuffling the deck of fate...",
        "🌀 Stirring the entropy pool...",
        "🎯 Calibrating the selection matrix...",
        "🚀 Launching weighted probability engine...",
        "💫 Sprinkling statistical fairy dust...",
        "🔥 Heating up the lottery furnace...",
        "❄️ Cooling down the bias detectors...",
        "🎵 Harmonizing the selection frequencies..."
    ];

    private static readonly Dictionary<WorkshopId, (string Name, string Emoji, Color Color)> WorkshopInfo = new()
    {
        [WorkshopId.W1] = ("Secure Coding Literacy", "🔐", Color.Green),
        [WorkshopId.W2] = ("AI Architecture Critic", "🤖", Color.Blue),
        [WorkshopId.W3] = ("Pizza Ordering Agent", "🍕", Color.Red)
    };

    public async Task ShowBannerAsync()
    {
        AnsiConsole.Clear();
        var random = new Random();
        
        // ═══════════════════════════════════════════════════════════════
        // PHASE 1: Boot Sequence - Building suspense
        // ═══════════════════════════════════════════════════════════════
        
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold grey]▓▓▓ WORKSHOP LOTTERY SYSTEM ▓▓▓[/]").RuleStyle("grey").Centered());
        AnsiConsole.WriteLine();
        
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots12)
            .SpinnerStyle(Style.Parse("cyan bold"))
            .StartAsync("[cyan]Initializing...[/]", async ctx =>
            {
                // Show random boot messages
                var selectedMessages = BootMessages.OrderBy(_ => random.Next()).Take(4).ToArray();
                foreach (var msg in selectedMessages)
                {
                    ctx.Status(msg);
                    ctx.Spinner(GetRandomSpinner(random));
                    await Task.Delay(Delay(300) + random.Next(Delay(200)));
                }
            });
        
        // ═══════════════════════════════════════════════════════════════
        // PHASE 2: Progress Bar - Charging up 
        // ═══════════════════════════════════════════════════════════════
        
        AnsiConsole.Clear();
        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn { CompletedStyle = new Style(Color.Gold1), RemainingStyle = new Style(Color.Grey23) },
                new PercentageColumn(),
                new SpinnerColumn(Spinner.Known.Star))
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[yellow]🔋 Charging lottery engine[/]", maxValue: 100);
                
                while (!task.IsFinished)
                {
                    var increment = random.Next(3, 12);
                    task.Increment(increment);
                    
                    // Update description at milestones
                    if (task.Value > 30 && task.Value < 35)
                        task.Description = "[yellow]⚡ Amplifying randomness...[/]";
                    else if (task.Value > 60 && task.Value < 65)
                        task.Description = "[yellow]🎯 Locking probability vectors...[/]";
                    else if (task.Value > 85)
                        task.Description = "[yellow]🚀 Final power surge...[/]";
                    
                    await Task.Delay(Delay(60));
                }
            });
        
        await Task.Delay(Delay(200));
        AnsiConsole.Clear();
        
        // ═══════════════════════════════════════════════════════════════
        // PHASE 3: Dramatic Title Reveal - Letter by letter
        // ═══════════════════════════════════════════════════════════════
        
        var garbledChars = "█▓▒░╬╫╪┼†‡§¶•◘○◙♦♣♠♥▲►▼◄■□●○★☆✦✧";
        var workshopText = "WORKSHOP";
        var lotteryText = "LOTTERY";
        
        // Build garbled versions
        var workshopGarbled = new string(workshopText.Select(_ => garbledChars[random.Next(garbledChars.Length)]).ToArray());
        var lotteryGarbled = new string(lotteryText.Select(_ => garbledChars[random.Next(garbledChars.Length)]).ToArray());
        
        // Colors for heat effect: grey → red → orange → yellow → gold
        var colors = new[] { Color.Grey, Color.Red, Color.Orange1, Color.Yellow, Color.Gold1 };
        
        // Animate "WORKSHOP" reveal
        for (int phase = 0; phase < 5; phase++)
        {
            AnsiConsole.Clear();
            
            // Build partially revealed text
            var revealedCount = (int)((phase + 1) / 5.0 * workshopText.Length);
            var displayText = new string(Enumerable.Range(0, workshopText.Length)
                .Select(i => i < revealedCount ? workshopText[i] : garbledChars[random.Next(garbledChars.Length)])
                .ToArray());
            
            AnsiConsole.Write(
                new FigletText(displayText)
                    .Centered()
                    .Color(colors[phase]));
            
            await Task.Delay(Delay(150));
        }
        
        await Task.Delay(Delay(100));
        
        // Animate "LOTTERY" reveal below
        for (int phase = 0; phase < 5; phase++)
        {
            AnsiConsole.Clear();
            
            // WORKSHOP is now fully revealed in gold
            AnsiConsole.Write(
                new FigletText(workshopText)
                    .Centered()
                    .Color(Color.Gold1));
            
            // Build partially revealed LOTTERY text
            var revealedCount = (int)((phase + 1) / 5.0 * lotteryText.Length);
            var displayText = new string(Enumerable.Range(0, lotteryText.Length)
                .Select(i => i < revealedCount ? lotteryText[i] : garbledChars[random.Next(garbledChars.Length)])
                .ToArray());
            
            // LOTTERY goes through red shades
            var lotteryColors = new[] { Color.Grey, Color.Maroon, Color.Red, Color.Red1, Color.Red };
            
            AnsiConsole.Write(
                new FigletText(displayText)
                    .Centered()
                    .Color(lotteryColors[phase]));
            
            await Task.Delay(Delay(150));
        }
        
        // ═══════════════════════════════════════════════════════════════
        // PHASE 4: Flash Effect - Brief brightness pulse
        // ═══════════════════════════════════════════════════════════════
        
        // Quick white flash
        AnsiConsole.Clear();
        await Task.Delay(Delay(50));
        
        // Final reveal with celebration
        AnsiConsole.Clear();
        
        // Top emoji border
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]" + new string('✨', 30).PadLeft(50) + "[/]");
        
        AnsiConsole.Write(
            new FigletText("WORKSHOP")
                .Centered()
                .Color(Color.Gold1));
        
        AnsiConsole.Write(
            new FigletText("LOTTERY")
                .Centered()
                .Color(Color.Red));
        
        // Bottom emoji border
        AnsiConsole.MarkupLine("[yellow]" + new string('✨', 30).PadLeft(50) + "[/]");
        AnsiConsole.WriteLine();
        
        await Task.Delay(Delay(300));

        // ═══════════════════════════════════════════════════════════════
        // PHASE 5: Subtitle with style
        // ═══════════════════════════════════════════════════════════════
        
        var rule = new Rule("[bold yellow]🎰 Fair Seat Assignment Using Weighted Lottery 🎰[/]")
            .RuleStyle("yellow")
            .Centered();
        AnsiConsole.Write(rule);
        
        AnsiConsole.WriteLine();
        
        // Dramatic pause before continuing
        await Task.Delay(Delay(500));
    }

    public async Task ShowStartAsync(string inputPath, string outputPath, int? seed)
    {
        var panel = new Panel(
            new Markup(
                $"[bold cyan]Input:[/]  [white]{EscapeMarkup(inputPath)}[/]\n" +
                $"[bold cyan]Output:[/] [white]{EscapeMarkup(outputPath)}[/]\n" +
                (seed.HasValue 
                    ? $"[bold cyan]Seed:[/]   [yellow]{seed.Value}[/] [dim](reproducible)[/]"
                    : "[bold cyan]Seed:[/]   [dim]Random[/]")))
            .Header("[bold green]📋 Configuration[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Style.Parse("green"));

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        
        await Task.Delay(Delay(500));
    }

    public async Task ShowParsingAsync(int registrationCount)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots12)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync("📖 Reading Excel file...", async ctx =>
            {
                await Task.Delay(Delay(600));
                ctx.Status("📊 Extracting registrations...");
                ctx.Spinner(Spinner.Known.Star);
                await Task.Delay(Delay(400));
            });
        
        AnsiConsole.MarkupLine($"[green]✓[/] Found [bold yellow]{registrationCount}[/] registrations!");
        AnsiConsole.WriteLine();
    }

    public async Task ShowValidationAsync(ValidationResult result)
    {
        var total = result.EligibleRegistrations.Count + result.DisqualifiedRegistrations.Count;
        
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.BouncingBar)
            .SpinnerStyle(Style.Parse("cyan bold"))
            .StartAsync("🔍 Validating registrations...", async ctx =>
            {
                await Task.Delay(Delay(400));
                ctx.Status("🔎 Checking eligibility criteria...");
                ctx.Spinner(Spinner.Known.Dots);
                await Task.Delay(Delay(400));
                ctx.Status("📧 Detecting duplicate emails...");
                await Task.Delay(Delay(300));
                ctx.Status("💻 Verifying laptop requirements...");
                await Task.Delay(Delay(300));
                ctx.Status("⏰ Confirming commitment pledges...");
                await Task.Delay(Delay(300));
            });

        // Validation summary table
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(Style.Parse("cyan"))
            .AddColumn(new TableColumn("[bold]Metric[/]").Centered())
            .AddColumn(new TableColumn("[bold]Count[/]").Centered());

        table.AddRow("[white]Total Registrations[/]", $"[bold]{total}[/]");
        table.AddRow("[green]✓ Eligible[/]", $"[bold green]{result.EligibleRegistrations.Count}[/]");
        table.AddRow("[red]✗ Disqualified[/]", $"[bold red]{result.DisqualifiedRegistrations.Count}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        // Show disqualification reasons if any
        if (result.DisqualificationReasons.Count > 0)
        {
            AnsiConsole.MarkupLine("[dim]Disqualification breakdown:[/]");
            foreach (var (reason, count) in result.DisqualificationReasons.OrderByDescending(kvp => kvp.Value))
            {
                AnsiConsole.MarkupLine($"  [red]•[/] {reason}: [bold]{count}[/]");
            }
            AnsiConsole.WriteLine();
        }

        await Task.Delay(Delay(500));
    }

    public async Task ShowLotteryStartAsync(int eligibleCount, int capacity)
    {
        AnsiConsole.WriteLine();
        
        var rule = new Rule("[bold yellow]🎲 THE LOTTERY BEGINS 🎲[/]")
            .RuleStyle("yellow")
            .DoubleBorder();
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Dramatic spinning section
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Aesthetic)
            .SpinnerStyle(Style.Parse("yellow bold"))
            .StartAsync("Initializing lottery engine...", async ctx =>
            {
                var random = new Random();
                
                ctx.Status("⚙️ Initializing Efraimidis-Spirakis algorithm...");
                await Task.Delay(Delay(500));
                
                // Show fun processing messages
                for (int i = 0; i < 5; i++)
                {
                    var message = ProcessingMessages[random.Next(ProcessingMessages.Length)];
                    ctx.Status(message);
                    ctx.Spinner(GetRandomSpinner(random));
                    await Task.Delay(Delay(400) + random.Next(Delay(400)));
                }

                ctx.Spinner(Spinner.Known.Grenade);
                ctx.Status("🎰 [bold]SPINNING THE WHEEL OF DESTINY...[/]");
                await Task.Delay(Delay(800));
            });

        AnsiConsole.MarkupLine("[green]✓[/] Lottery engine ready!");
        AnsiConsole.MarkupLine($"  [dim]Capacity per workshop:[/] [bold cyan]{capacity}[/]");
        AnsiConsole.MarkupLine($"  [dim]Eligible participants:[/] [bold cyan]{eligibleCount}[/]");
        AnsiConsole.WriteLine();
        
        await Task.Delay(Delay(300));
    }

    public async Task ShowWorkshopResultAsync(WorkshopId workshopId, WorkshopResult result)
    {
        var info = WorkshopInfo[workshopId];
        
        // Workshop header
        AnsiConsole.WriteLine();
        var rule = new Rule($"[bold]{info.Emoji} {workshopId} - {info.Name} {info.Emoji}[/]")
            .RuleStyle(new Style(info.Color))
            .DoubleBorder();
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Countdown!
        AnsiConsole.MarkupLine("[bold yellow]Results in...[/]");
        for (int i = 3; i > 0; i--)
        {
            AnsiConsole.MarkupLine($"[bold red]{i}[/]");
            await Task.Delay(Delay(400));
        }
        AnsiConsole.MarkupLine("[bold green]🎉 REVEALING! 🎉[/]");
        AnsiConsole.WriteLine();
        await Task.Delay(Delay(300));

        // Results table with animated population
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(info.Color))
            .AddColumn(new TableColumn("[bold]#[/]").Centered())
            .AddColumn(new TableColumn("[bold]Participant[/]"))
            .AddColumn(new TableColumn("[bold]Status[/]").Centered());

        // Show accepted participants with animation
        var accepted = result.Accepted.Take(10).ToList(); // Show first 10 with animation
        var remaining = result.Accepted.Skip(10).ToList();

        await AnsiConsole.Live(table)
            .StartAsync(async ctx =>
            {
                int order = 1;
                foreach (var assignment in accepted)
                {
                    var statusMarkup = GetStatusMarkup(assignment);
                    table.AddRow(
                        $"[white]{order}[/]",
                        $"[cyan]{EscapeMarkup(assignment.Registration.FullName)}[/]",
                        statusMarkup);
                    ctx.Refresh();
                    await Task.Delay(Delay(100));
                    order++;
                }

                // Add remaining without animation
                foreach (var assignment in remaining)
                {
                    var statusMarkup = GetStatusMarkup(assignment);
                    table.AddRow(
                        $"[white]{order}[/]",
                        $"[cyan]{EscapeMarkup(assignment.Registration.FullName)}[/]",
                        statusMarkup);
                    order++;
                }

                // Add waitlisted
                foreach (var assignment in result.Waitlisted.Take(5))
                {
                    table.AddRow(
                        $"[dim]{order}[/]",
                        $"[dim]{EscapeMarkup(assignment.Registration.FullName)}[/]",
                        "[grey]⚪ Waitlist[/]");
                    order++;
                }

                ctx.Refresh();
            });

        // Summary stats
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Summary:[/] [green]{result.AcceptedCount} accepted[/] " +
            $"([cyan]W1:{result.Wave1Count}[/] [yellow]W2:{result.Wave2Count}[/] [orange1]LP:{result.LowPriorityCount}[/]) " +
            $"| [grey]{result.WaitlistCount} waitlisted[/]");

        if (result.WaitlistCount > 5)
        {
            AnsiConsole.MarkupLine($"[dim]  ...and {result.WaitlistCount - 5} more on waitlist[/]");
        }

        await Task.Delay(Delay(300));
    }

    public async Task ShowFinalSummaryAsync(LotteryResult result)
    {
        AnsiConsole.WriteLine();
        
        // Final celebration
        var celebrationRule = new Rule("[bold green]🎉🎊 LOTTERY COMPLETE! 🎊🎉[/]")
            .RuleStyle("green")
            .DoubleBorder();
        AnsiConsole.Write(celebrationRule);
        AnsiConsole.WriteLine();

        await Task.Delay(Delay(300));

        // Stats panel
        var totalAccepted = result.Results.Values.Sum(r => r.AcceptedCount);
        var totalWaitlisted = result.Results.Values.Sum(r => r.WaitlistCount);
        var uniqueParticipants = result.Results.Values
            .SelectMany(r => r.Accepted)
            .Select(a => a.Registration.Email.ToLowerInvariant())
            .Distinct()
            .Count();

        var statsContent = new Markup(
            $"[bold cyan]Total Registrations:[/]    [white]{result.TotalRegistrations}[/]\n" +
            $"[bold green]Eligible:[/]               [green]{result.EligibleCount}[/]\n" +
            $"[bold red]Disqualified:[/]           [red]{result.DisqualifiedCount}[/]\n" +
            $"[bold yellow]Seats Assigned:[/]         [yellow]{totalAccepted}[/]\n" +
            $"[bold magenta]Unique Winners:[/]         [magenta]{uniqueParticipants}[/]\n" +
            $"[bold grey]Total Waitlisted:[/]       [grey]{totalWaitlisted}[/]\n" +
            $"[bold blue]Random Seed:[/]            [blue]{result.Seed}[/]\n" +
            $"[bold green]Fairness Level:[/]         [bold green]MAXIMUM[/] ✨");

        var panel = new Panel(Align.Center(statsContent))
            .Header("[bold]📊 FINAL STATISTICS[/]")
            .Border(BoxBorder.Double)
            .BorderStyle(Style.Parse("gold1"));

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        // Output file reminder
        AnsiConsole.MarkupLine($"[dim]Results written to Excel file[/]");
        AnsiConsole.WriteLine();

        await Task.Delay(Delay(300));
    }

    public void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[bold green]✓[/] {EscapeMarkup(message)}");
    }

    public void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[bold red]✗ ERROR:[/] {EscapeMarkup(message)}");
    }

    public void ShowInfo(string message)
    {
        AnsiConsole.MarkupLine($"[cyan]ℹ[/] {EscapeMarkup(message)}");
    }

    public void ShowWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠[/] {EscapeMarkup(message)}");
    }

    private static string GetStatusMarkup(WorkshopAssignment assignment)
    {
        if (assignment.IsLowPriority)
            return "[orange1]🟠 Low Priority[/]";
        
        return assignment.Wave switch
        {
            1 => "[green]🟢 Wave 1[/]",
            2 => "[yellow]🟡 Wave 2[/]",
            _ => "[grey]⚪ Waitlist[/]"
        };
    }

    private static Spinner GetRandomSpinner(Random random)
    {
        var spinners = new[]
        {
            Spinner.Known.Dots,
            Spinner.Known.Dots2,
            Spinner.Known.Star,
            Spinner.Known.Star2,
            Spinner.Known.Bounce,
            Spinner.Known.Arc,
            Spinner.Known.Circle,
            Spinner.Known.BouncingBar,
            Spinner.Known.Christmas,
            Spinner.Known.Earth,
            Spinner.Known.Hearts,
            Spinner.Known.Moon
        };
        return spinners[random.Next(spinners.Length)];
    }

    private static string EscapeMarkup(string text)
    {
        return Markup.Escape(text);
    }
}
