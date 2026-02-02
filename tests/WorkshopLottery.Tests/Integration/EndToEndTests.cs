namespace WorkshopLottery.Tests.Integration;

using ClosedXML.Excel;
using FluentAssertions;
using WorkshopLottery.Models;
using WorkshopLottery.Services;
using Xunit;

/// <summary>
/// End-to-end tests for the complete lottery workflow.
/// </summary>
public class EndToEndTests : IDisposable
{
    private readonly string _testDir;

    public EndToEndTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "E2ETests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    [Fact]
    public void FullWorkflow_WithValidInput_ProducesValidOutput()
    {
        // Arrange
        var inputPath = CreateTestInputFile();
        var outputPath = Path.Combine(_testDir, "output.xlsx");
        var orchestrator = LotteryOrchestrator.CreateDefault();

        // Act
        var result = orchestrator.Run(inputPath, outputPath, seed: 42);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        result.Should().NotBeNull();
        result.TotalRegistrations.Should().BeGreaterThan(0);
    }

    [Fact]
    public void FullWorkflow_ProducesReproducibleResults_WithSameSeed()
    {
        // Arrange
        var inputPath = CreateTestInputFile();
        var outputPath1 = Path.Combine(_testDir, "output1.xlsx");
        var outputPath2 = Path.Combine(_testDir, "output2.xlsx");
        var orchestrator = LotteryOrchestrator.CreateDefault();
        var seed = 12345;

        // Act
        var result1 = orchestrator.Run(inputPath, outputPath1, seed: seed);
        var result2 = orchestrator.Run(inputPath, outputPath2, seed: seed);

        // Assert
        result1.Seed.Should().Be(result2.Seed);
        
        // Compare assignments for each workshop
        foreach (var workshopId in new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 })
        {
            var assignments1 = result1.Results[workshopId]
                .Assignments
                .OrderBy(a => a.Order)
                .Select(a => a.Registration.Email)
                .ToList();

            var assignments2 = result2.Results[workshopId]
                .Assignments
                .OrderBy(a => a.Order)
                .Select(a => a.Registration.Email)
                .ToList();

            assignments1.Should().BeEquivalentTo(assignments2);
        }
    }

    [Fact]
    public void FullWorkflow_DifferentSeeds_ProduceDifferentResults()
    {
        // Arrange - Create input with more participants to ensure different outcomes
        var inputPath = CreateLargeTestInputFile();
        var outputPath1 = Path.Combine(_testDir, "output1.xlsx");
        var outputPath2 = Path.Combine(_testDir, "output2.xlsx");
        var orchestrator = LotteryOrchestrator.CreateDefault();

        // Act
        var result1 = orchestrator.Run(inputPath, outputPath1, seed: 111);
        var result2 = orchestrator.Run(inputPath, outputPath2, seed: 999);

        // Assert
        result1.Seed.Should().NotBe(result2.Seed);
        
        // At least one workshop should have different order
        var anyDifference = false;
        foreach (var workshopId in new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 })
        {
            var order1 = result1.Results[workshopId]
                .Assignments
                .OrderBy(a => a.Order)
                .Select(a => a.Registration.Email)
                .ToList();

            var order2 = result2.Results[workshopId]
                .Assignments
                .OrderBy(a => a.Order)
                .Select(a => a.Registration.Email)
                .ToList();

            if (!order1.SequenceEqual(order2))
            {
                anyDifference = true;
                break;
            }
        }
        
        anyDifference.Should().BeTrue("different seeds should produce different orderings");
    }

    [Fact]
    public void FullWorkflow_ValidatesEligibility_DisqualifiesIneligible()
    {
        // Arrange
        var inputPath = CreateInputWithIneligible();
        var outputPath = Path.Combine(_testDir, "output.xlsx");
        var orchestrator = LotteryOrchestrator.CreateDefault();

        // Act
        var result = orchestrator.Run(inputPath, outputPath, seed: 42);

        // Assert
        result.DisqualifiedCount.Should().BeGreaterThan(0);
        result.EligibleCount.Should().BeLessThan(result.TotalRegistrations);
    }

    [Fact]
    public void FullWorkflow_OutputExcel_HasAllRequiredSheets()
    {
        // Arrange
        var inputPath = CreateTestInputFile();
        var outputPath = Path.Combine(_testDir, "output.xlsx");
        var orchestrator = LotteryOrchestrator.CreateDefault();

        // Act
        orchestrator.Run(inputPath, outputPath, seed: 42);

        // Assert
        using var workbook = new XLWorkbook(outputPath);
        workbook.Worksheets.Should().HaveCount(4); // Summary + W1 + W2 + W3
        workbook.Worksheets.Contains("Summary").Should().BeTrue();
        workbook.Worksheets.Contains("W1").Should().BeTrue();
        workbook.Worksheets.Contains("W2").Should().BeTrue();
        workbook.Worksheets.Contains("W3").Should().BeTrue();
    }

    [Fact]
    public void FullWorkflow_AssignmentsRespectCapacities()
    {
        // Arrange
        var inputPath = CreateLargeTestInputFile();
        var outputPath = Path.Combine(_testDir, "output.xlsx");
        var orchestrator = LotteryOrchestrator.CreateDefault();
        var capacity = 5;

        // Act
        var result = orchestrator.Run(inputPath, outputPath, seed: 42, capacity: capacity);

        // Assert
        foreach (var workshop in result.Results.Values)
        {
            workshop.AcceptedCount.Should().BeLessThanOrEqualTo(capacity);
        }
    }

    [Fact]
    public void FullWorkflow_Wave1_HasUniqueParticipants()
    {
        // Arrange
        var inputPath = CreateLargeTestInputFile();
        var outputPath = Path.Combine(_testDir, "output.xlsx");
        var orchestrator = LotteryOrchestrator.CreateDefault();

        // Act
        var result = orchestrator.Run(inputPath, outputPath, seed: 42);

        // Assert
        var wave1Emails = result.Results.Values
            .SelectMany(w => w.Accepted)
            .Where(a => a.Wave == 1)
            .Select(a => a.Registration.Email.ToLowerInvariant())
            .ToList();

        // Each email should appear at most once in Wave 1
        wave1Emails.Should().OnlyHaveUniqueItems();
    }

    private string CreateTestInputFile()
    {
        var inputPath = Path.Combine(_testDir, "input.xlsx");
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Form Responses");

        // Headers - using fuzzy-matching column names
        ws.Cell(1, 1).Value = "Full Name";
        ws.Cell(1, 2).Value = "Email";
        ws.Cell(1, 3).Value = "Do you have a laptop?";
        ws.Cell(1, 4).Value = "Will you commit to arrive 10 min early?";
        ws.Cell(1, 5).Value = "Workshop 1 Request";
        ws.Cell(1, 6).Value = "Workshop 2 Request";
        ws.Cell(1, 7).Value = "Workshop 3 Request";
        ws.Cell(1, 8).Value = "Rank your preferences";

        // Data rows - Yes/No for workshop selection, semicolon-delimited for rankings
        ws.Cell(2, 1).Value = "Alice Test";
        ws.Cell(2, 2).Value = "alice@test.com";
        ws.Cell(2, 3).Value = "Yes";
        ws.Cell(2, 4).Value = "Yes";
        ws.Cell(2, 5).Value = "Yes";
        ws.Cell(2, 6).Value = "Yes";
        ws.Cell(2, 7).Value = "Yes";
        ws.Cell(2, 8).Value = "Workshop 1;Workshop 2;Workshop 3";

        ws.Cell(3, 1).Value = "Bob Test";
        ws.Cell(3, 2).Value = "bob@test.com";
        ws.Cell(3, 3).Value = "Yes";
        ws.Cell(3, 4).Value = "Yes";
        ws.Cell(3, 5).Value = "Yes";
        ws.Cell(3, 6).Value = "Yes";
        ws.Cell(3, 7).Value = "Yes";
        ws.Cell(3, 8).Value = "Workshop 2;Workshop 3;Workshop 1";

        ws.Cell(4, 1).Value = "Carol Test";
        ws.Cell(4, 2).Value = "carol@test.com";
        ws.Cell(4, 3).Value = "Yes";
        ws.Cell(4, 4).Value = "Yes";
        ws.Cell(4, 5).Value = "Yes";
        ws.Cell(4, 6).Value = "Yes";
        ws.Cell(4, 7).Value = "Yes";
        ws.Cell(4, 8).Value = "Workshop 3;Workshop 1;Workshop 2";

        workbook.SaveAs(inputPath);
        return inputPath;
    }

    private string CreateLargeTestInputFile()
    {
        var inputPath = Path.Combine(_testDir, "input_large.xlsx");
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Form Responses");

        // Headers - using fuzzy-matching column names
        ws.Cell(1, 1).Value = "Full Name";
        ws.Cell(1, 2).Value = "Email";
        ws.Cell(1, 3).Value = "Laptop";
        ws.Cell(1, 4).Value = "Commit early";
        ws.Cell(1, 5).Value = "Workshop 1 Request";
        ws.Cell(1, 6).Value = "Workshop 2 Request";
        ws.Cell(1, 7).Value = "Workshop 3 Request";
        ws.Cell(1, 8).Value = "Rank your preferences";

        // Preference patterns - different selection patterns to ensure variety
        var patterns = new (string w1, string w2, string w3, string ranks)[]
        {
            ("Yes", "No", "No", "Workshop 1"),
            ("No", "Yes", "No", "Workshop 2"),
            ("No", "No", "Yes", "Workshop 3"),
            ("Yes", "Yes", "No", "Workshop 1;Workshop 2"),
            ("Yes", "No", "Yes", "Workshop 1;Workshop 3"),
            ("No", "Yes", "Yes", "Workshop 2;Workshop 3"),
            ("Yes", "Yes", "Yes", "Workshop 1;Workshop 2;Workshop 3"),
            ("Yes", "Yes", "Yes", "Workshop 2;Workshop 1;Workshop 3"),
            ("Yes", "Yes", "Yes", "Workshop 3;Workshop 2;Workshop 1"),
            ("Yes", "Yes", "Yes", "Workshop 3;Workshop 1;Workshop 2")
        };

        // Create 50 participants with varied preferences
        for (int i = 0; i < 50; i++)
        {
            var row = i + 2;
            var pattern = patterns[i % patterns.Length];
            
            ws.Cell(row, 1).Value = $"Participant {i + 1}";
            ws.Cell(row, 2).Value = $"participant{i + 1}@test.com";
            ws.Cell(row, 3).Value = "Yes";
            ws.Cell(row, 4).Value = "Yes";
            ws.Cell(row, 5).Value = pattern.w1;
            ws.Cell(row, 6).Value = pattern.w2;
            ws.Cell(row, 7).Value = pattern.w3;
            ws.Cell(row, 8).Value = pattern.ranks;
        }

        workbook.SaveAs(inputPath);
        return inputPath;
    }

    private string CreateInputWithIneligible()
    {
        var inputPath = Path.Combine(_testDir, "input_mixed.xlsx");
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Form Responses");

        // Headers - using fuzzy-matching column names
        ws.Cell(1, 1).Value = "Name";
        ws.Cell(1, 2).Value = "Email";
        ws.Cell(1, 3).Value = "Laptop";
        ws.Cell(1, 4).Value = "Commit early";
        ws.Cell(1, 5).Value = "Workshop 1 Request";
        ws.Cell(1, 6).Value = "Workshop 2 Request";
        ws.Cell(1, 7).Value = "Workshop 3 Request";
        ws.Cell(1, 8).Value = "Rank your preferences";

        // Eligible participant
        ws.Cell(2, 1).Value = "Good User";
        ws.Cell(2, 2).Value = "good@test.com";
        ws.Cell(2, 3).Value = "Yes";
        ws.Cell(2, 4).Value = "Yes";
        ws.Cell(2, 5).Value = "Yes";
        ws.Cell(2, 6).Value = "Yes";
        ws.Cell(2, 7).Value = "No";
        ws.Cell(2, 8).Value = "Workshop 1;Workshop 2";

        // No laptop - INELIGIBLE
        ws.Cell(3, 1).Value = "No Laptop User";
        ws.Cell(3, 2).Value = "nolaptop@test.com";
        ws.Cell(3, 3).Value = "No";
        ws.Cell(3, 4).Value = "Yes";
        ws.Cell(3, 5).Value = "Yes";
        ws.Cell(3, 6).Value = "No";
        ws.Cell(3, 7).Value = "No";
        ws.Cell(3, 8).Value = "Workshop 1";

        // Won't commit - INELIGIBLE
        ws.Cell(4, 1).Value = "No Commit User";
        ws.Cell(4, 2).Value = "nocommit@test.com";
        ws.Cell(4, 3).Value = "Yes";
        ws.Cell(4, 4).Value = "No";
        ws.Cell(4, 5).Value = "No";
        ws.Cell(4, 6).Value = "Yes";
        ws.Cell(4, 7).Value = "No";
        ws.Cell(4, 8).Value = "Workshop 2";

        // Duplicate email (both disqualified) - INELIGIBLE x2
        ws.Cell(5, 1).Value = "Duplicate 1";
        ws.Cell(5, 2).Value = "dupe@test.com";
        ws.Cell(5, 3).Value = "Yes";
        ws.Cell(5, 4).Value = "Yes";
        ws.Cell(5, 5).Value = "No";
        ws.Cell(5, 6).Value = "No";
        ws.Cell(5, 7).Value = "Yes";
        ws.Cell(5, 8).Value = "Workshop 3";

        ws.Cell(6, 1).Value = "Duplicate 2";
        ws.Cell(6, 2).Value = "dupe@test.com";
        ws.Cell(6, 3).Value = "Yes";
        ws.Cell(6, 4).Value = "Yes";
        ws.Cell(6, 5).Value = "Yes";
        ws.Cell(6, 6).Value = "No";
        ws.Cell(6, 7).Value = "No";
        ws.Cell(6, 8).Value = "Workshop 1";

        workbook.SaveAs(inputPath);
        return inputPath;
    }
}
