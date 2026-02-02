# 🎰 Workshop Lottery System

A .NET 10 console application for fair workshop seat assignment using weighted lottery algorithms. Built for AgentCon Zurich workshop signup management.

## Overview

This application reads MS Forms Excel exports containing workshop registrations and uses a weighted lottery (Efraimidis-Spirakis algorithm) to fairly assign seats while respecting participant preferences.

### Key Features

- **Weighted Lottery**: Higher preference ranks get higher lottery weights
- **Two-Wave Assignment**: Maximizes unique participants before allowing second workshops
- **Fuzzy Column Matching**: Handles MS Forms Excel export variations
- **Reproducible Results**: Optional seed for deterministic outcomes
- **Rich Output**: Color-coded Excel reports and console summaries

## Quick Start

```bash
# Build the application
dotnet build

# Run with minimum required options
dotnet run --project src/WorkshopLottery -- --input "path/to/registrations.xlsx"

# Run with all options
dotnet run --project src/WorkshopLottery -- \
    --input "input/registrations.xlsx" \
    --output "output/results.xlsx" \
    --seed 42 \
    --w1-capacity 25 \
    --w2-capacity 20 \
    --w3-capacity 15
```

## CLI Options

| Option | Alias | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--input` | `-i` | Path to input Excel file | Yes | - |
| `--output` | `-o` | Path to output Excel file | No | `<input>_results.xlsx` |
| `--seed` | `-s` | Random seed for reproducibility | No | Random |
| `--w1-capacity` | - | Workshop 1 capacity | No | 20 |
| `--w2-capacity` | - | Workshop 2 capacity | No | 20 |
| `--w3-capacity` | - | Workshop 3 capacity | No | 20 |
| `--verbose` | `-v` | Enable verbose output | No | false |

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
3. **Wave 2**: Fill remaining seats with anyone who requested the workshop
4. **Waitlist**: Remaining participants ordered by lottery position

### Efraimidis-Spirakis Algorithm

For weighted random selection without replacement:
```
score = log(random(0,1)) / weight
```
Higher weights produce higher expected scores, giving proportionally higher selection probability.

## Input Format

The application expects an MS Forms Excel export with these columns (fuzzy matching supported):

- **Name** / Full Name
- **Email** / Email Address
- **Laptop** / "Do you have a laptop?"
- **Commit** / "Will you commit to arrive 10 min early?"
- **Rankings** / Workshop preferences (semicolon-separated)

Example rankings format: `Workshop 1;Workshop 3;Workshop 2`

## Output Format

### Excel File

The output Excel file contains:

1. **Summary Sheet**: Overall statistics, seed, configuration
2. **Per-Workshop Sheets** (W1, W2, W3): Participants with assignment details

Color coding:
- 🟢 Light Green: Wave 1 assignments
- 🟡 Light Yellow: Wave 2 assignments  
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
│   └── WorkshopLottery.Tests/     # Unit tests (280+ tests)
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
