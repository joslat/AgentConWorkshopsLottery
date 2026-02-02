namespace WorkshopLottery.Tests.Integration;

using ClosedXML.Excel;
using FluentAssertions;
using WorkshopLottery.Models;
using WorkshopLottery.Services;
using Xunit;

/// <summary>
/// End-to-end tests using the sample-workshop-small-50.xlsx file.
/// This file contains:
/// - 50 fake entries total
/// - 35 people who want all 3 workshops
/// - 10 people who want only 1 workshop
/// - 5 mixed cases (2 workshops)
/// - 10 people without laptop / won't commit (disqualified → low-priority)
/// </summary>
public class SmallSampleEndToEndTests : IDisposable
{
    private const string SampleFileName = "sample-workshop-small-50.xlsx";
    private const int CapacityPerWorkshop = 34;
    private const int FixedSeed = 42;
    
    private readonly string _inputPath;
    private readonly string _outputDir;
    private readonly string _outputPath;
    private readonly bool _sampleFileExists;

    public SmallSampleEndToEndTests()
    {
        var repoRoot = FindRepoRoot();
        _inputPath = Path.Combine(repoRoot, "input", SampleFileName);
        _sampleFileExists = File.Exists(_inputPath);
        
        _outputDir = Path.Combine(Path.GetTempPath(), $"SmallSampleTests_{Guid.NewGuid():N}");
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
    public void SmallSample_AllWorkshopsFilled()
    {
        if (!_sampleFileExists) return;

        var result = RunLottery();

        // All workshops should be filled to capacity (34 slots, 50 people)
        foreach (var workshopId in new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 })
        {
            var workshopResult = result.Results[workshopId];
            workshopResult.AcceptedCount.Should().Be(CapacityPerWorkshop,
                $"{workshopId} should fill all {CapacityPerWorkshop} seats");
        }
    }

    [Fact]
    public void SmallSample_TotalRegistrationsIs50()
    {
        if (!_sampleFileExists) return;

        var result = RunLottery();

        result.TotalRegistrations.Should().Be(50, "Sample has 50 registrations");
    }

    [Fact]
    public void SmallSample_SingleWorkshopRequestersOnlyInThatWorkshop()
    {
        if (!_sampleFileExists) return;

        var result = RunLottery();

        // Find single-workshop requesters
        var singleRequesters = GetSingleWorkshopParticipants(result);

        // They should NOT appear in workshops they didn't request
        foreach (var (email, requestedWorkshop) in singleRequesters)
        {
            foreach (var (workshopId, workshopResult) in result.Results)
            {
                var appearances = workshopResult.Assignments
                    .Where(a => a.Registration.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (workshopId != requestedWorkshop)
                {
                    appearances.Should().BeEmpty(
                        $"{email} only requested {requestedWorkshop} but appears in {workshopId}");
                }
            }
        }
    }

    [Fact]
    public void SmallSample_ParticipantsOnlyAssignedToRequestedWorkshops()
    {
        if (!_sampleFileExists) return;

        var result = RunLottery();

        foreach (var (workshopId, workshopResult) in result.Results)
        {
            foreach (var assignment in workshopResult.Accepted)
            {
                var prefs = assignment.Registration.WorkshopPreferences;
                prefs.Should().ContainKey(workshopId);
                prefs[workshopId].Requested.Should().BeTrue(
                    $"{assignment.Registration.FullName} in {workshopId} should have requested it");
            }
        }
    }

    [Fact]
    public void SmallSample_LowPriorityParticipantsFillEmptySeats()
    {
        if (!_sampleFileExists) return;

        var result = RunLottery();

        // File has 10 disqualified (6 no laptop, 4 won't commit)
        result.DisqualifiedCount.Should().Be(10, "Sample has 10 disqualified entries");
        result.EligibleCount.Should().Be(40, "Sample has 40 eligible entries");

        // Low-priority should fill some empty seats
        var totalLowPriority = result.Results.Values.Sum(w => w.LowPriorityCount);
        totalLowPriority.Should().BeGreaterThan(0, "Low-priority participants should fill empty seats");

        // Verify low-priority assignments are correctly marked
        foreach (var (workshopId, workshopResult) in result.Results)
        {
            var lowPriorityAssignments = workshopResult.Accepted.Where(a => a.IsLowPriority);
            
            foreach (var assignment in lowPriorityAssignments)
            {
                var reg = assignment.Registration;
                
                // Low-priority must be truly ineligible (missing laptop OR commit)
                var isIneligible = !reg.HasLaptop || !reg.WillCommit10Min;
                isIneligible.Should().BeTrue(
                    $"{reg.FullName} is low-priority but has laptop={reg.HasLaptop} commit={reg.WillCommit10Min}");
                
                // Must have requested this workshop
                reg.WorkshopPreferences[workshopId].Requested.Should().BeTrue(
                    $"Low-priority {reg.FullName} in {workshopId} should have requested it");
            }
        }
    }

    [Fact]
    public void SmallSample_OnlyEligibleInMainAssignments()
    {
        if (!_sampleFileExists) return;

        var result = RunLottery();

        foreach (var (workshopId, workshopResult) in result.Results)
        {
            var mainAccepted = workshopResult.Accepted.Where(a => !a.IsLowPriority);
            
            foreach (var assignment in mainAccepted)
            {
                var reg = assignment.Registration;
                reg.HasLaptop.Should().BeTrue($"{reg.FullName} in {workshopId} main should have laptop");
                reg.WillCommit10Min.Should().BeTrue($"{reg.FullName} in {workshopId} main should commit");
            }
        }
    }

    [Fact]
    public void SmallSample_OutputExcelHasLowPriorityColumn()
    {
        if (!_sampleFileExists) return;

        RunLottery();

        using var workbook = new XLWorkbook(_outputPath);
        var summary = workbook.Worksheet("Summary");
        
        // Find "Low Priority" anywhere in the Summary sheet (it's in the per-workshop results section)
        var lowPriorityColumnExists = summary.CellsUsed()
            .Any(c => c.GetString().Equals("Low Priority", StringComparison.OrdinalIgnoreCase));
        
        lowPriorityColumnExists.Should().BeTrue("Summary should have Low Priority column");
    }

    [Fact]
    public void SmallSample_DeterministicResults()
    {
        if (!_sampleFileExists) return;

        var result1 = RunLottery();
        var result2 = RunLottery();

        result1.Seed.Should().Be(result2.Seed);

        foreach (var workshopId in new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 })
        {
            var emails1 = result1.Results[workshopId].Assignments
                .OrderBy(a => a.Order)
                .Select(a => a.Registration.Email)
                .ToList();
                
            var emails2 = result2.Results[workshopId].Assignments
                .OrderBy(a => a.Order)
                .Select(a => a.Registration.Email)
                .ToList();
                
            emails1.Should().Equal(emails2, $"{workshopId} should be deterministic");
        }
    }

    private LotteryResult RunLottery()
    {
        var orchestrator = LotteryOrchestrator.CreateDefault();
        return orchestrator.Run(_inputPath, _outputPath, seed: FixedSeed, capacity: CapacityPerWorkshop);
    }

    private List<(string Email, WorkshopId Workshop)> GetSingleWorkshopParticipants(LotteryResult result)
    {
        var allRegistrations = result.Results.Values
            .SelectMany(w => w.Assignments)
            .Select(a => a.Registration)
            .DistinctBy(r => r.Email.ToLowerInvariant())
            .ToList();

        var singleRequesters = new List<(string Email, WorkshopId Workshop)>();

        foreach (var reg in allRegistrations)
        {
            var requested = reg.WorkshopPreferences
                .Where(kvp => kvp.Value.Requested)
                .Select(kvp => kvp.Key)
                .ToList();

            if (requested.Count == 1)
            {
                singleRequesters.Add((reg.Email, requested[0]));
            }
        }

        return singleRequesters;
    }

    private static string FindRepoRoot()
    {
        var current = Directory.GetCurrentDirectory();
        
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current, "input")))
            {
                return current;
            }
            current = Directory.GetParent(current)?.FullName;
        }
        
        return Directory.GetCurrentDirectory();
    }
}
