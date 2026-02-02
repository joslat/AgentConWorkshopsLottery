namespace WorkshopLottery.Tests.Integration;

using ClosedXML.Excel;
using FluentAssertions;
using WorkshopLottery.Models;
using WorkshopLottery.Services;
using Xunit;

/// <summary>
/// End-to-end tests using the sample-workshop-registrations-120.xlsx file.
/// This file contains:
/// - 120 fake entries total
/// - 10 people who only want one workshop
/// - Mix of eligible and ineligible (no laptop / won't commit) participants
/// </summary>
public class SampleDataEndToEndTests : IDisposable
{
    private const string SampleFileName = "sample-workshop-registrations-120.xlsx";
    private const int CapacityPerWorkshop = 34;
    private const int FixedSeed = 42;
    
    private readonly string _inputPath;
    private readonly string _outputDir;
    private readonly string _outputPath;
    private readonly bool _sampleFileExists;

    public SampleDataEndToEndTests()
    {
        // Find the sample file - it should be in the input folder at the repo root
        var repoRoot = FindRepoRoot();
        _inputPath = Path.Combine(repoRoot, "input", SampleFileName);
        _sampleFileExists = File.Exists(_inputPath);
        
        // Create temp output directory
        _outputDir = Path.Combine(Path.GetTempPath(), $"SampleDataTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_outputDir);
        _outputPath = Path.Combine(_outputDir, "output.xlsx");
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }
    }

    [Fact]
    public void FullWorkflow_EachWorkshopHas34SpotsFilledOrAll()
    {
        if (!_sampleFileExists) return; // Skip if file not present

        // Arrange & Act
        var result = RunLottery();

        // Assert - each workshop should have exactly 34 accepted 
        // (or fewer if not enough candidates)
        foreach (var workshopId in new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 })
        {
            var workshopResult = result.Results[workshopId];
            workshopResult.AcceptedCount.Should().Be(CapacityPerWorkshop,
                $"{workshopId} should have exactly {CapacityPerWorkshop} accepted participants");
        }
    }

    [Fact]
    public void FullWorkflow_ParticipantsOnlyAssignedToRequestedWorkshops()
    {
        if (!_sampleFileExists) return; // Skip if file not present

        // Arrange & Act
        var result = RunLottery();

        // Assert - every assigned person should have requested that workshop
        foreach (var (workshopId, workshopResult) in result.Results)
        {
            foreach (var assignment in workshopResult.Accepted)
            {
                var prefs = assignment.Registration.WorkshopPreferences;
                prefs.Should().ContainKey(workshopId,
                    $"{assignment.Registration.FullName} was assigned to {workshopId} but has no preferences for it");
                    
                prefs[workshopId].Requested.Should().BeTrue(
                    $"{assignment.Registration.FullName} was assigned to {workshopId} but did not request it");
            }
        }
    }

    [Fact]
    public void FullWorkflow_SingleWorkshopRequestersOnlyInThatWorkshop()
    {
        if (!_sampleFileExists) return; // Skip if file not present

        // Arrange & Act
        var result = RunLottery();

        // Find all people who only requested one workshop
        var singleWorkshopParticipants = GetSingleWorkshopParticipants(result);

        // Assert - each single-workshop requester should:
        // 1. Appear in their requested workshop (accepted or waitlisted)
        // 2. NOT appear in any other workshop
        foreach (var (email, requestedWorkshop) in singleWorkshopParticipants)
        {
            foreach (var (workshopId, workshopResult) in result.Results)
            {
                var appearancesInThisWorkshop = workshopResult.Assignments
                    .Where(a => a.Registration.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (workshopId == requestedWorkshop)
                {
                    // Should appear in their requested workshop (accepted or waitlisted)
                    appearancesInThisWorkshop.Should().NotBeEmpty(
                        $"{email} requested only {requestedWorkshop} but is not in that workshop at all");
                }
                else
                {
                    // Should NOT appear in workshops they didn't request
                    appearancesInThisWorkshop.Should().BeEmpty(
                        $"{email} only requested {requestedWorkshop} but appears in {workshopId}");
                }
            }
        }
    }

    [Fact]
    public void FullWorkflow_OnlyEligibleParticipantsInMainAssignments()
    {
        if (!_sampleFileExists) return; // Skip if file not present

        // Arrange & Act
        var result = RunLottery();

        // Assert - non-low-priority accepted participants should all be eligible
        foreach (var (workshopId, workshopResult) in result.Results)
        {
            var mainAccepted = workshopResult.Accepted.Where(a => !a.IsLowPriority);
            
            foreach (var assignment in mainAccepted)
            {
                var reg = assignment.Registration;
                
                reg.HasLaptop.Should().BeTrue(
                    $"{reg.FullName} in {workshopId} (non-low-priority) should have laptop");
                    
                reg.WillCommit10Min.Should().BeTrue(
                    $"{reg.FullName} in {workshopId} (non-low-priority) should commit to 10 min early");
            }
        }
    }

    [Fact]
    public void FullWorkflow_LowPriorityParticipantsAreTrulyIneligible()
    {
        if (!_sampleFileExists) return; // Skip if file not present

        // Arrange & Act
        var result = RunLottery();

        // Assert - low-priority participants should NOT have both laptop AND commit
        foreach (var (workshopId, workshopResult) in result.Results)
        {
            var lowPriority = workshopResult.Accepted.Where(a => a.IsLowPriority);
            
            foreach (var assignment in lowPriority)
            {
                var reg = assignment.Registration;
                var isIneligible = !reg.HasLaptop || !reg.WillCommit10Min;
                
                isIneligible.Should().BeTrue(
                    $"{reg.FullName} is marked as low-priority but has both laptop={reg.HasLaptop} and commit={reg.WillCommit10Min}");
            }
        }
    }

    [Fact]
    public void FullWorkflow_OutputExcel_HasCorrectStructure()
    {
        if (!_sampleFileExists) return; // Skip if file not present

        // Arrange & Act
        RunLottery();

        // Assert - output Excel has correct structure
        using var workbook = new XLWorkbook(_outputPath);
        
        workbook.Worksheets.Should().HaveCount(4, "should have Summary + 3 workshop sheets");
        workbook.Worksheets.Contains("Summary").Should().BeTrue();
        workbook.Worksheets.Contains("W1").Should().BeTrue();
        workbook.Worksheets.Contains("W2").Should().BeTrue();
        workbook.Worksheets.Contains("W3").Should().BeTrue();
    }

    [Fact]
    public void FullWorkflow_OutputExcel_WorksheetHasExpectedRowCount()
    {
        if (!_sampleFileExists) return; // Skip if file not present

        // Arrange & Act
        var result = RunLottery();

        // Assert - each worksheet should have header + all assignments
        using var workbook = new XLWorkbook(_outputPath);
        
        foreach (var workshopId in new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 })
        {
            var sheetName = workshopId.ToString();
            var worksheet = workbook.Worksheet(sheetName);
            var workshopResult = result.Results[workshopId];
            
            var expectedRows = 1 + workshopResult.Assignments.Count; // header + data
            var actualRows = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            
            actualRows.Should().Be(expectedRows,
                $"{sheetName} should have {expectedRows} rows (1 header + {workshopResult.Assignments.Count} assignments)");
        }
    }

    [Fact]
    public void FullWorkflow_DeterministicWithSameSeed()
    {
        if (!_sampleFileExists) return; // Skip if file not present

        // Arrange & Act
        var result1 = RunLottery();
        var result2 = RunLottery();

        // Assert - same seed produces identical results
        result1.Seed.Should().Be(result2.Seed);
        
        foreach (var workshopId in new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 })
        {
            var order1 = result1.Results[workshopId].Assignments
                .OrderBy(a => a.Order)
                .Select(a => a.Registration.Email)
                .ToList();
                
            var order2 = result2.Results[workshopId].Assignments
                .OrderBy(a => a.Order)
                .Select(a => a.Registration.Email)
                .ToList();
                
            order1.Should().Equal(order2,
                $"{workshopId} assignments should be identical with same seed");
        }
    }

    [Fact]
    public void FullWorkflow_DisqualifiedCountMatchesExpected()
    {
        if (!_sampleFileExists) return; // Skip if file not present

        // Arrange & Act
        var result = RunLottery();

        // Assert - sample file has 23 disqualified (15 no commit + 8 no laptop)
        result.DisqualifiedCount.Should().Be(23,
            "Sample file should have 23 disqualified registrations");
        
        result.EligibleCount.Should().Be(97,
            "Sample file should have 97 eligible registrations");
        
        result.TotalRegistrations.Should().Be(120,
            "Sample file should have 120 total registrations");
    }

    private LotteryResult RunLottery()
    {
        var orchestrator = LotteryOrchestrator.CreateDefault();
        return orchestrator.Run(_inputPath, _outputPath, seed: FixedSeed, capacity: CapacityPerWorkshop);
    }

    /// <summary>
    /// Finds participants who requested exactly one workshop.
    /// </summary>
    private List<(string Email, WorkshopId Workshop)> GetSingleWorkshopParticipants(LotteryResult result)
    {
        // Get all unique registrations from all assignments
        var allRegistrations = result.Results.Values
            .SelectMany(w => w.Assignments)
            .Select(a => a.Registration)
            .DistinctBy(r => r.Email.ToLowerInvariant())
            .ToList();

        var singleRequesters = new List<(string Email, WorkshopId Workshop)>();

        foreach (var reg in allRegistrations)
        {
            var requestedWorkshops = reg.WorkshopPreferences
                .Where(kvp => kvp.Value.Requested)
                .Select(kvp => kvp.Key)
                .ToList();

            if (requestedWorkshops.Count == 1)
            {
                singleRequesters.Add((reg.Email, requestedWorkshops[0]));
            }
        }

        return singleRequesters;
    }

    private static string FindRepoRoot()
    {
        var current = Directory.GetCurrentDirectory();
        
        // Walk up until we find a directory with "input" folder
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current, "input")))
            {
                return current;
            }
            current = Directory.GetParent(current)?.FullName;
        }
        
        // Fallback to current directory
        return Directory.GetCurrentDirectory();
    }
}
