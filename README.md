# 🎰 Workshop Lottery System

A .NET 10 console application for fair workshop seat assignment using weighted lottery algorithms. Built for AgentCon Zurich workshop signup management.

## Overview

This application reads MS Forms Excel exports containing workshop registrations and uses a weighted lottery (Efraimidis-Spirakis algorithm) to fairly assign seats while respecting participant preferences.

### Key Features

- **Weighted Lottery**: Higher preference ranks get higher lottery weights
- **Two-Wave Assignment**: Maximizes unique participants before allowing second workshops
- **Low-Priority Fill**: Participants without laptop/commitment can fill remaining empty seats
- **Fuzzy Column Matching**: Handles MS Forms Excel export variations
- **Reproducible Results**: Optional seed for deterministic outcomes
- **Rich Output**: Color-coded Excel reports and console summaries
- **🎪 Spectacular Mode**: Dramatic animations, spinners, and colorful reveals!

## Quick Start

```bash
# Build the application
dotnet build

# Run with a sample file (34 seats per workshop, random seed)
dotnet run --project src/WorkshopLottery -- --input "input/sample-workshop-small-50.xlsx"

# Run with fixed seed for reproducible results
dotnet run --project src/WorkshopLottery -- --input "input/sample-workshop-small-50.xlsx" --seed 42

# Run with custom capacity and output path
dotnet run --project src/WorkshopLottery -- \
    --input "input/sample-workshop-registrations-120.xlsx" \
    --output "output/lottery-results.xlsx" \
    --seed 42 \
    --capacity 34
```

## CLI Options

| Option | Alias | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--input` | `-i` | Path to input Excel file | Yes | - |
| `--output` | `-o` | Path to output Excel file | No | `<input>_results.xlsx` |
| `--seed` | `-s` | Random seed for reproducibility | No | Random |
| `--capacity` | `-c` | Capacity per workshop | No | 34 |
| `--verbose` | `-v` | Enable verbose output | No | false |
| `--spectacular` | `-S`, `--show` | Enable dramatic animations 🎪 | No | false |
| `--slow` | | Slow animations (2x) - auto-enables spectacular | No | false |
| `--slower` | | Slower animations (3x) - auto-enables spectacular | No | false |
| `--slowest` | | Slowest animations (5x) - auto-enables spectacular | No | false |

## 🎪 Spectacular Mode

For a fun, dramatic lottery experience with animations:

```bash
# Run with spectacular mode - dramatic reveals, spinners, and colorful output!
dotnet run --project src/WorkshopLottery -- -i "input/sample-workshop-small-50.xlsx" -s 42 --spectacular

# Or use the short flag
dotnet run --project src/WorkshopLottery -- -i "input/sample-workshop-small-50.xlsx" -s 42 -S

# Run slower for more emphasis (2x slower)
dotnet run --project src/WorkshopLottery -- -i "input/sample-workshop-small-50.xlsx" -s 42 -S --slow

# Even slower for presentations (3x slower)
dotnet run --project src/WorkshopLottery -- -i "input/sample-workshop-small-50.xlsx" -s 42 -S --slower

# Maximum drama! (5x slower)
dotnet run --project src/WorkshopLottery -- -i "input/sample-workshop-small-50.xlsx" -s 42 -S --slowest
```

**Speed Options:**
| Flag | Multiplier | Use Case |
|------|------------|----------|
| (default) | 1x | Quick demo |
| `--slow` | 2x | Standard presentation |
| `--slower` | 3x | Live event with audience |
| `--slowest` | 5x | Maximum dramatic effect |

**Features in Spectacular Mode:**
- 🎨 Large FIGlet ASCII art banner
- ⏳ Animated spinners with fun messages like "🔮 Consulting the probability oracle..."
- ⏱️ Countdown reveals: "3... 2... 1... 🎉 REVEALING!"
- 📊 Animated table population (names appear one by one)
- 🎉 Final celebration with stats panel
- 🌈 Full color output using Spectre.Console

## Sample Files

Two sample files are included in the `input/` folder:

| File | Description |
|------|-------------|
| `sample-workshop-small-50.xlsx` | 50 fake registrations, 10 disqualified (no laptop/won't commit) |
| `sample-workshop-registrations-120.xlsx` | 120 fake registrations, 23 disqualified |

### Try it yourself

```bash
# Small sample (50 people, 10 disqualified → triggers low-priority feature)
dotnet run --project src/WorkshopLottery -- -i "input/sample-workshop-small-50.xlsx" -s 42

# Larger sample (120 people)
dotnet run --project src/WorkshopLottery -- -i "input/sample-workshop-registrations-120.xlsx" -s 42
```

## Workshops

| ID | Name |
|----|------|
| W1 | Secure Coding Literacy for Vibe Coders |
| W2 | AI Architecture Critic |
| W3 | Build a Pizza Ordering Agent with Microsoft Foundry and MCP |

## Algorithm

### Weight Calculation

Participant preferences are converted to lottery weights:

| Preference Rank | Weight |
|-----------------|--------|
| 1st Choice | 5 |
| 2nd Choice | 2 |
| 3rd Choice / No Rank | 1 |

### Selection Process

1. **Eligibility Check**: Must have laptop, commit to arriving 10 min early, valid name/email, no duplicate emails
2. **Wave 1**: Each eligible person can win at most one workshop (maximize unique participants)
3. **Wave 2**: Fill remaining seats with anyone eligible who requested the workshop
4. **Low-Priority**: Fill any remaining empty seats with disqualified participants (no laptop/won't commit)
5. **Waitlist**: Remaining participants ordered by lottery position

### Efraimidis-Spirakis Algorithm

For weighted random selection without replacement:
```
score = log(random(0,1)) / weight
```
Higher weights produce higher expected scores, giving proportionally higher selection probability.

## Input Format

The application expects an MS Forms Excel export with these columns (fuzzy matching supported):

| Column | Example Headers | Values |
|--------|-----------------|--------|
| Name | "Full name", "Name" | Text |
| Email | "Email address", "Email" | Valid email |
| Laptop | "Will you bring a laptop?" | "Yes" / "No" |
| Commit 10min | "Do you commit to be there 10 min before?" | "Yes" / "No" |
| Workshop 1 | "Do you want to attend Workshop 1?" | "Yes" / empty |
| Workshop 2 | "Do you want to attend Workshop 2?" | "Yes" / empty |
| Workshop 3 | "Do you want to attend Workshop 3?" | "Yes" / empty |
| Rankings | "Please rank the workshops" | Semicolon-separated |

### Rankings Format

Rankings can be in various formats (the parser handles MS Forms variations):
- `Workshop 1;Workshop 3;Workshop 2` (order = rank)
- `Workshop 1 – Name;Workshop 2 – Name` (workshops with titles)

### Creating Your Own Input File

1. Export your MS Forms responses to Excel
2. Ensure the columns match the expected format above
3. The parser uses fuzzy matching, so exact column names aren't required

Example workflow:
```bash
# 1. Place your Excel file in the input folder
cp "Downloads/My Workshop Signup.xlsx" input/

# 2. Run the lottery
dotnet run --project src/WorkshopLottery -- -i "input/My Workshop Signup.xlsx" -s 42

# 3. Check the output (created in same folder as input by default)
# Opens: input/My Workshop Signup_results.xlsx
```

## Output Format

### Excel File

The output Excel file contains:

1. **Summary Sheet**: Overall statistics, seed, configuration
2. **Per-Workshop Sheets** (W1, W2, W3): Participants with assignment details

Color coding:
- 🟢 Light Green: Wave 1 assignments (first workshop for this participant)
- 🟡 Light Yellow: Wave 2 assignments (additional workshop for this participant)
- 🟠 Light Salmon: Low-priority assignments (disqualified participant filling empty seat)
- ⚪ Light Gray: Waitlisted

### Console Output

Rich summary with:
- Overall statistics (total, eligible, assigned, waitlisted)
- Per-workshop breakdowns with participant lists
- Disqualification summary

## Development

### Prerequisites

- .NET 10 SDK
- VS Code or Visual Studio 2022+

### Project Structure

```
AgentConWorkshopsLottery/
├── src/
│   └── WorkshopLottery/           # Main console application
│       ├── Models/                 # Domain models
│       ├── Services/               # Business logic
│       ├── Infrastructure/         # Excel I/O, CLI
│       └── Extensions/             # Helper extensions
├── tests/
│   └── WorkshopLottery.Tests/     # Unit tests (330+ tests)
├── docs/
│   ├── ARCHITECTURE.md            # System specification
│   ├── adr/                        # Architecture Decision Records
│   └── plans/                      # Implementation plans
└── input/                          # Sample Excel files
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Key Design Decisions

See `docs/adr/` for Architecture Decision Records:

- **ADR-001**: Single project structure
- **ADR-002**: ClosedXML for Excel
- **ADR-003**: Efraimidis-Spirakis algorithm
- **ADR-004**: System.CommandLine for CLI
- **ADR-005**: Fuzzy column matching
- **ADR-006**: Two-wave assignment strategy

## License

MIT License - See [LICENSE](LICENSE)

## Contributing

1. Check existing ADRs in `docs/adr/`
2. Review `docs/ARCHITECTURE.md` for system design
3. Document prompts/requests in `docs/prompts/`
4. Write tests alongside implementation
5. Follow C# conventions (file-scoped namespaces, records for data)
