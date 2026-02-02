namespace WorkshopLottery.Tests.Services;

using ClosedXML.Excel;
using FluentAssertions;
using WorkshopLottery.Models;
using WorkshopLottery.Services;
using Xunit;

public class ExcelWriterServiceTests : IDisposable
{
    private readonly string _testOutputDir;
    private readonly ExcelWriterService _sut;

    public ExcelWriterServiceTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), "ExcelWriterTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testOutputDir);
        _sut = new ExcelWriterService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
        {
            Directory.Delete(_testOutputDir, recursive: true);
        }
    }

    [Fact]
    public void WriteResults_CreatesOutputFile()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");
        var result = CreateSampleLotteryResult();

        // Act
        _sut.WriteResults(outputPath, result);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
    }

    [Fact]
    public void WriteResults_CreatesSummarySheet()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");
        var result = CreateSampleLotteryResult();

        // Act
        _sut.WriteResults(outputPath, result);

        // Assert
        using var workbook = new XLWorkbook(outputPath);
        workbook.Worksheets.Contains("Summary").Should().BeTrue();
    }

    [Fact]
    public void WriteResults_CreatesWorkshopSheets()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");
        var result = CreateSampleLotteryResult();

        // Act
        _sut.WriteResults(outputPath, result);

        // Assert
        using var workbook = new XLWorkbook(outputPath);
        workbook.Worksheets.Contains("W1").Should().BeTrue();
        workbook.Worksheets.Contains("W2").Should().BeTrue();
        workbook.Worksheets.Contains("W3").Should().BeTrue();
    }

    [Fact]
    public void WriteResults_SummarySheet_ContainsStatistics()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");
        var result = CreateSampleLotteryResult();

        // Act
        _sut.WriteResults(outputPath, result);

        // Assert
        using var workbook = new XLWorkbook(outputPath);
        var summary = workbook.Worksheet("Summary");
        
        // Check key statistics are present
        var cellValues = summary.RangeUsed()?.Cells()
            .Select(c => c.GetString())
            .ToList() ?? new List<string>();

        cellValues.Should().Contain(v => v.Contains("Total Registrations"));
        cellValues.Should().Contain(v => v.Contains("Eligible"));
        cellValues.Should().Contain(v => v.Contains("Random Seed"));
    }

    [Fact]
    public void WriteResults_WorkshopSheet_HasCorrectHeaders()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");
        var result = CreateSampleLotteryResult();

        // Act
        _sut.WriteResults(outputPath, result);

        // Assert
        using var workbook = new XLWorkbook(outputPath);
        var w1Sheet = workbook.Worksheet("W1");
        
        w1Sheet.Cell(1, 1).GetString().Should().Be("Order");
        w1Sheet.Cell(1, 2).GetString().Should().Be("Status");
        w1Sheet.Cell(1, 3).GetString().Should().Be("Wave");
        w1Sheet.Cell(1, 4).GetString().Should().Be("Name");
        w1Sheet.Cell(1, 5).GetString().Should().Be("Email");
    }

    [Fact]
    public void WriteResults_WorkshopSheet_ContainsAssignments()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");
        var result = CreateSampleLotteryResult();

        // Act
        _sut.WriteResults(outputPath, result);

        // Assert
        using var workbook = new XLWorkbook(outputPath);
        var w1Sheet = workbook.Worksheet("W1");
        
        // Row 2 should have first assignment
        w1Sheet.Cell(2, 4).GetString().Should().Be("Alice Test"); // Name
        w1Sheet.Cell(2, 5).GetString().Should().Be("alice@test.com"); // Email
    }

    [Fact]
    public void WriteResults_AcceptedParticipants_HaveCorrectStatus()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");
        var result = CreateSampleLotteryResult();

        // Act
        _sut.WriteResults(outputPath, result);

        // Assert
        using var workbook = new XLWorkbook(outputPath);
        var w1Sheet = workbook.Worksheet("W1");
        
        w1Sheet.Cell(2, 2).GetString().Should().Be("Accepted");
    }

    [Fact]
    public void WriteResults_WaitlistedParticipants_HaveCorrectStatus()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");
        var result = CreateResultWithWaitlist();

        // Act
        _sut.WriteResults(outputPath, result);

        // Assert
        using var workbook = new XLWorkbook(outputPath);
        var w1Sheet = workbook.Worksheet("W1");
        
        // Find the waitlisted row
        var rows = w1Sheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? new List<IXLRangeRow>();
        var waitlistedRow = rows.FirstOrDefault(r => r.Cell(2).GetString() == "Waitlisted");
        waitlistedRow.Should().NotBeNull();
    }

    [Fact]
    public void WriteResults_ThrowsOnNullPath()
    {
        // Arrange
        var result = CreateSampleLotteryResult();

        // Act & Assert
        var act = () => _sut.WriteResults(null!, result);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WriteResults_ThrowsOnEmptyPath()
    {
        // Arrange
        var result = CreateSampleLotteryResult();

        // Act & Assert
        var act = () => _sut.WriteResults("", result);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WriteResults_ThrowsOnNullResult()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");

        // Act & Assert
        var act = () => _sut.WriteResults(outputPath, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WriteResults_HandlesEmptyResult()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");
        var result = new LotteryResult
        {
            TotalRegistrations = 0,
            EligibleCount = 0,
            DisqualifiedCount = 0,
            Seed = 42,
            Capacity = 20,
            Results = new Dictionary<WorkshopId, WorkshopResult>
            {
                [WorkshopId.W1] = new() { WorkshopId = WorkshopId.W1, Assignments = new List<WorkshopAssignment>() },
                [WorkshopId.W2] = new() { WorkshopId = WorkshopId.W2, Assignments = new List<WorkshopAssignment>() },
                [WorkshopId.W3] = new() { WorkshopId = WorkshopId.W3, Assignments = new List<WorkshopAssignment>() }
            }
        };

        // Act
        _sut.WriteResults(outputPath, result);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        using var workbook = new XLWorkbook(outputPath);
        workbook.Worksheets.Count.Should().Be(4); // Summary + 3 workshops
    }

    [Fact]
    public void WriteResults_PreservesWaveInformation()
    {
        // Arrange
        var outputPath = Path.Combine(_testOutputDir, "output.xlsx");
        var result = CreateResultWithBothWaves();

        // Act
        _sut.WriteResults(outputPath, result);

        // Assert
        using var workbook = new XLWorkbook(outputPath);
        var w1Sheet = workbook.Worksheet("W1");
        
        var rows = w1Sheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? new List<IXLRangeRow>();
        var waves = rows.Select(r => r.Cell(3).GetString()).ToList();
        
        waves.Should().Contain("1");
        waves.Should().Contain("2");
    }

    private static LotteryResult CreateSampleLotteryResult()
    {
        var registration = CreateRegistration("Alice Test", "alice@test.com");
        
        return new LotteryResult
        {
            TotalRegistrations = 1,
            EligibleCount = 1,
            DisqualifiedCount = 0,
            Seed = 42,
            Capacity = 20,
            Results = new Dictionary<WorkshopId, WorkshopResult>
            {
                [WorkshopId.W1] = new()
                {
                    WorkshopId = WorkshopId.W1,
                    Assignments = new List<WorkshopAssignment>
                    {
                        new()
                        {
                            Registration = registration,
                            Status = AssignmentStatus.Accepted,
                            Wave = 1,
                            Order = 1
                        }
                    }
                },
                [WorkshopId.W2] = new() { WorkshopId = WorkshopId.W2, Assignments = new List<WorkshopAssignment>() },
                [WorkshopId.W3] = new() { WorkshopId = WorkshopId.W3, Assignments = new List<WorkshopAssignment>() }
            }
        };
    }

    private static LotteryResult CreateResultWithWaitlist()
    {
        var accepted = CreateRegistration("Alice Test", "alice@test.com");
        var waitlisted = CreateRegistration("Bob Test", "bob@test.com");
        
        return new LotteryResult
        {
            TotalRegistrations = 2,
            EligibleCount = 2,
            DisqualifiedCount = 0,
            Seed = 42,
            Capacity = 1, // Only 1 seat
            Results = new Dictionary<WorkshopId, WorkshopResult>
            {
                [WorkshopId.W1] = new()
                {
                    WorkshopId = WorkshopId.W1,
                    Assignments = new List<WorkshopAssignment>
                    {
                        new()
                        {
                            Registration = accepted,
                            Status = AssignmentStatus.Accepted,
                            Wave = 1,
                            Order = 1
                        },
                        new()
                        {
                            Registration = waitlisted,
                            Status = AssignmentStatus.Waitlisted,
                            Wave = null,
                            Order = 2
                        }
                    }
                },
                [WorkshopId.W2] = new() { WorkshopId = WorkshopId.W2, Assignments = new List<WorkshopAssignment>() },
                [WorkshopId.W3] = new() { WorkshopId = WorkshopId.W3, Assignments = new List<WorkshopAssignment>() }
            }
        };
    }

    private static LotteryResult CreateResultWithBothWaves()
    {
        var wave1 = CreateRegistration("Alice Wave1", "alice@test.com");
        var wave2 = CreateRegistration("Bob Wave2", "bob@test.com");
        
        return new LotteryResult
        {
            TotalRegistrations = 2,
            EligibleCount = 2,
            DisqualifiedCount = 0,
            Seed = 42,
            Capacity = 20,
            Results = new Dictionary<WorkshopId, WorkshopResult>
            {
                [WorkshopId.W1] = new()
                {
                    WorkshopId = WorkshopId.W1,
                    Assignments = new List<WorkshopAssignment>
                    {
                        new()
                        {
                            Registration = wave1,
                            Status = AssignmentStatus.Accepted,
                            Wave = 1,
                            Order = 1
                        },
                        new()
                        {
                            Registration = wave2,
                            Status = AssignmentStatus.Accepted,
                            Wave = 2,
                            Order = 2
                        }
                    }
                },
                [WorkshopId.W2] = new() { WorkshopId = WorkshopId.W2, Assignments = new List<WorkshopAssignment>() },
                [WorkshopId.W3] = new() { WorkshopId = WorkshopId.W3, Assignments = new List<WorkshopAssignment>() }
            }
        };
    }

    private static Registration CreateRegistration(string name, string email)
    {
        return new Registration
        {
            FullName = name,
            Email = email,
            HasLaptop = true,
            WillCommit10Min = true,
            WorkshopPreferences = new Dictionary<WorkshopId, WorkshopPreference>
            {
                [WorkshopId.W1] = new() { Requested = true, Rank = 1 },
                [WorkshopId.W2] = new() { Requested = false, Rank = null },
                [WorkshopId.W3] = new() { Requested = false, Rank = null }
            }
        };
    }
}
