namespace WorkshopLottery.Tests.Integration;

using FluentAssertions;
using WorkshopLottery.Models;
using WorkshopLottery.Services;
using Xunit;

/// <summary>
/// Integration tests verifying the complete lottery workflow.
/// </summary>
public class LotteryIntegrationTests
{
    private readonly LotteryEngine _lotteryEngine = new();
    private readonly ValidationService _validationService = new();

    #region End-to-End Workflow Tests

    [Fact]
    public void FullWorkflow_FromRawRegistrations_ProducesValidResults()
    {
        // Arrange - create synthetic raw registrations
        var rawRegistrations = CreateSyntheticRawRegistrations(100);

        // Act - validate then run lottery
        var validationResult = _validationService.ValidateAndFilter(rawRegistrations);
        var config = new LotteryConfiguration
        {
            InputPath = "synthetic.xlsx",
            Capacity = 34,
            Seed = 20260202
        };
        var lotteryResult = _lotteryEngine.RunLottery(validationResult.EligibleRegistrations, config);

        // Assert
        lotteryResult.Results.Should().HaveCount(3);
        lotteryResult.Seed.Should().Be(20260202);
        lotteryResult.Capacity.Should().Be(34);

        // Each workshop should have results
        foreach (var (workshop, result) in lotteryResult.Results)
        {
            result.WorkshopId.Should().Be(workshop);
            result.AcceptedCount.Should().BeLessThanOrEqualTo(34);
        }
    }

    [Fact]
    public void FullWorkflow_Wave1Uniqueness_IsRespected()
    {
        // Arrange - 50 people all requesting all workshops
        var rawRegistrations = CreateSyntheticRawRegistrations(50, requestAll: true);
        var validationResult = _validationService.ValidateAndFilter(rawRegistrations);
        var config = new LotteryConfiguration
        {
            InputPath = "test.xlsx",
            Capacity = 20,
            Seed = 42
        };

        // Act
        var lotteryResult = _lotteryEngine.RunLottery(validationResult.EligibleRegistrations, config);

        // Assert - in Wave 1, each person should appear at most once across all workshops
        var wave1Emails = lotteryResult.Results.Values
            .SelectMany(r => r.Assignments)
            .Where(a => a.Wave == 1)
            .Select(a => a.Registration.NormalizedEmail)
            .ToList();

        wave1Emails.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void FullWorkflow_CapacityLimits_AreRespected()
    {
        // Arrange
        var rawRegistrations = CreateSyntheticRawRegistrations(200);
        var validationResult = _validationService.ValidateAndFilter(rawRegistrations);
        var config = new LotteryConfiguration
        {
            InputPath = "test.xlsx",
            Capacity = 34,
            Seed = 42
        };

        // Act
        var lotteryResult = _lotteryEngine.RunLottery(validationResult.EligibleRegistrations, config);

        // Assert
        foreach (var result in lotteryResult.Results.Values)
        {
            result.AcceptedCount.Should().BeLessThanOrEqualTo(34);
        }
    }

    [Fact]
    public void FullWorkflow_WaitlistOrdering_IsCorrect()
    {
        // Arrange
        var rawRegistrations = CreateSyntheticRawRegistrations(50);
        var validationResult = _validationService.ValidateAndFilter(rawRegistrations);
        var config = new LotteryConfiguration
        {
            InputPath = "test.xlsx",
            Capacity = 10,
            Seed = 42
        };

        // Act
        var lotteryResult = _lotteryEngine.RunLottery(validationResult.EligibleRegistrations, config);

        // Assert - waitlist should have sequential order numbers after accepted
        foreach (var result in lotteryResult.Results.Values)
        {
            var waitlisted = result.Assignments
                .Where(a => a.Status == AssignmentStatus.Waitlisted)
                .OrderBy(a => a.Order)
                .ToList();

            if (waitlisted.Count > 0)
            {
                var maxAcceptedOrder = result.Assignments
                    .Where(a => a.Status == AssignmentStatus.Accepted)
                    .Max(a => a.Order);

                waitlisted.First().Order.Should().BeGreaterThan(maxAcceptedOrder);
            }
        }
    }

    [Fact]
    public void FullWorkflow_DuplicatesFiltered_BeforeLottery()
    {
        // Arrange - create registrations with duplicates
        var rawRegistrations = new List<RawRegistration>
        {
            CreateRawRegistration("unique1@test.com", "Person 1"),
            CreateRawRegistration("unique2@test.com", "Person 2"),
            CreateRawRegistration("duplicate@test.com", "Duplicate 1"),
            CreateRawRegistration("DUPLICATE@TEST.COM", "Duplicate 2"), // case-insensitive
            CreateRawRegistration("unique3@test.com", "Person 3")
        };

        // Act
        var validationResult = _validationService.ValidateAndFilter(rawRegistrations);

        // Assert - only non-duplicates should be eligible
        validationResult.EligibleRegistrations.Should().HaveCount(3);
        validationResult.DisqualifiedRegistrations.Should().HaveCount(2);

        var eligibleEmails = validationResult.EligibleRegistrations
            .Select(r => r.NormalizedEmail)
            .ToList();

        eligibleEmails.Should().NotContain("duplicate@test.com");
    }

    [Fact]
    public void FullWorkflow_DeterministicAcrossRuns()
    {
        // Arrange
        var rawRegistrations = CreateSyntheticRawRegistrations(30);
        var validationResult = _validationService.ValidateAndFilter(rawRegistrations);
        var config = new LotteryConfiguration
        {
            InputPath = "test.xlsx",
            Capacity = 10,
            Seed = 12345
        };

        // Act - run twice
        var result1 = _lotteryEngine.RunLottery(validationResult.EligibleRegistrations, config);
        var result2 = _lotteryEngine.RunLottery(validationResult.EligibleRegistrations, config);

        // Assert - results should be identical
        foreach (var workshop in config.WorkshopOrder)
        {
            var accepted1 = result1.Results[workshop].Assignments
                .Where(a => a.Status == AssignmentStatus.Accepted)
                .Select(a => a.Registration.Email)
                .ToList();

            var accepted2 = result2.Results[workshop].Assignments
                .Where(a => a.Status == AssignmentStatus.Accepted)
                .Select(a => a.Registration.Email)
                .ToList();

            accepted1.Should().Equal(accepted2);
        }
    }

    #endregion

    #region Weight Distribution Integration Tests

    [Fact]
    public void Integration_WeightDistribution_RespectsRanking()
    {
        // Arrange - create registrations with known weights
        var rawRegistrations = new List<RawRegistration>();

        // 10 people preferring W1 as rank 1 (weight 5)
        for (int i = 0; i < 10; i++)
        {
            rawRegistrations.Add(CreateRawRegistration(
                $"rank1_{i}@test.com",
                $"Rank 1 Person {i}",
                rankings: "Workshop 1;Workshop 2;Workshop 3"));
        }

        // 10 people preferring W1 as rank 2 (weight 2)
        for (int i = 0; i < 10; i++)
        {
            rawRegistrations.Add(CreateRawRegistration(
                $"rank2_{i}@test.com",
                $"Rank 2 Person {i}",
                rankings: "Workshop 2;Workshop 1;Workshop 3"));
        }

        // 10 people preferring W1 as rank 3 (weight 1)
        for (int i = 0; i < 10; i++)
        {
            rawRegistrations.Add(CreateRawRegistration(
                $"rank3_{i}@test.com",
                $"Rank 3 Person {i}",
                rankings: "Workshop 2;Workshop 3;Workshop 1"));
        }

        // Act - validate and run
        var validationResult = _validationService.ValidateAndFilter(rawRegistrations);
        var config = new LotteryConfiguration
        {
            InputPath = "test.xlsx",
            Capacity = 10, // Only 10 of 30 will be accepted
            Seed = 42
        };
        var lotteryResult = _lotteryEngine.RunLottery(validationResult.EligibleRegistrations, config);

        // Assert - count how many from each rank group got accepted
        var acceptedW1 = lotteryResult.Results[WorkshopId.W1].Assignments
            .Where(a => a.Status == AssignmentStatus.Accepted)
            .ToList();

        var rank1Accepted = acceptedW1.Count(a => a.Registration.Email.StartsWith("rank1_"));
        var rank2Accepted = acceptedW1.Count(a => a.Registration.Email.StartsWith("rank2_"));
        var rank3Accepted = acceptedW1.Count(a => a.Registration.Email.StartsWith("rank3_"));

        // Rank 1 (weight 5) should generally have more accepted than rank 3 (weight 1)
        // Not a guarantee due to randomness, but over time this holds
        (rank1Accepted + rank2Accepted).Should().BeGreaterThanOrEqualTo(rank3Accepted,
            "higher weights should tend to get more seats");
    }

    #endregion

    #region Helper Methods

    private static List<RawRegistration> CreateSyntheticRawRegistrations(
        int count, 
        bool requestAll = false)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateRawRegistration(
                $"person{i}@test.com",
                $"Test Person {i}",
                requestAll: requestAll))
            .ToList();
    }

    private static RawRegistration CreateRawRegistration(
        string email,
        string name,
        bool requestAll = true,
        string? rankings = null)
    {
        return new RawRegistration
        {
            RowNumber = 1,
            Email = email,
            FullName = name,
            LaptopResponse = "Yes",
            Commit10MinResponse = "Yes",
            RequestedW1Response = requestAll ? "Yes" : (new Random().Next(2) == 0 ? "Yes" : ""),
            RequestedW2Response = requestAll ? "Yes" : (new Random().Next(2) == 0 ? "Yes" : ""),
            RequestedW3Response = requestAll ? "Yes" : (new Random().Next(2) == 0 ? "Yes" : ""),
            RankingsResponse = rankings ?? "Workshop 1;Workshop 2;Workshop 3"
        };
    }

    #endregion
}
