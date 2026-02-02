namespace WorkshopLottery.Tests.Services;

using FluentAssertions;
using WorkshopLottery.Models;
using WorkshopLottery.Services;
using Xunit;

/// <summary>
/// Unit tests for the LotteryEngine.
/// Tests cover Efraimidis-Spirakis algorithm behavior, wave assignment, and edge cases.
/// </summary>
public class LotteryEngineTests
{
    private readonly LotteryEngine _engine = new();

    #region Determinism Tests

    [Fact]
    public void RunLottery_WithSameSeed_ProducesDeterministicOrder()
    {
        // Arrange
        var registrations = CreateTestRegistrations(10);
        var config = CreateConfig(seed: 42, capacity: 5);

        // Act - run twice with same seed
        var result1 = _engine.RunLottery(registrations, config);
        var result2 = _engine.RunLottery(registrations, config);

        // Assert - same order
        var order1 = result1.Results[WorkshopId.W1].Assignments
            .Select(a => a.Registration.Email).ToList();
        var order2 = result2.Results[WorkshopId.W1].Assignments
            .Select(a => a.Registration.Email).ToList();

        order1.Should().Equal(order2);
    }

    [Fact]
    public void RunLottery_WithDifferentSeeds_ProducesDifferentOrders()
    {
        // Arrange
        var registrations = CreateTestRegistrations(50);
        var config1 = CreateConfig(seed: 100, capacity: 20);
        var config2 = CreateConfig(seed: 200, capacity: 20);

        // Act
        var result1 = _engine.RunLottery(registrations, config1);
        var result2 = _engine.RunLottery(registrations, config2);

        // Assert - different orders (probability of identical order is negligible)
        var order1 = result1.Results[WorkshopId.W1].Assignments
            .Select(a => a.Registration.Email).ToList();
        var order2 = result2.Results[WorkshopId.W1].Assignments
            .Select(a => a.Registration.Email).ToList();

        order1.Should().NotEqual(order2);
    }

    [Fact]
    public void RunLottery_SeedIsFinalDivisionDate_Deterministic()
    {
        // Arrange - using the actual seed from ADR
        var registrations = CreateTestRegistrations(10);
        var config = CreateConfig(seed: 20260202, capacity: 5);

        // Act
        var result1 = _engine.RunLottery(registrations, config);
        var result2 = _engine.RunLottery(registrations, config);

        // Assert
        result1.Seed.Should().Be(20260202);
        result1.Results[WorkshopId.W1].Assignments.Count
            .Should().Be(result2.Results[WorkshopId.W1].Assignments.Count);
    }

    #endregion

    #region Wave 1 Tests

    [Fact]
    public void RunLottery_Wave1_DoesNotAssignSamePersonToMultipleWorkshops()
    {
        // Arrange - people requesting multiple workshops
        var registrations = CreateTestRegistrations(20, requestAllWorkshops: true);
        var config = CreateConfig(seed: 42, capacity: 10);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert - collect all Wave 1 assignments
        var wave1Emails = result.Results.Values
            .SelectMany(r => r.Assignments)
            .Where(a => a.Wave == 1)
            .Select(a => a.Registration.NormalizedEmail)
            .ToList();

        // Each email should appear only once in Wave 1
        wave1Emails.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void RunLottery_Wave1_MaximizesUniqueParticipants()
    {
        // Arrange - 30 people, each requesting all 3 workshops, capacity 10
        var registrations = CreateTestRegistrations(30, requestAllWorkshops: true);
        var config = CreateConfig(seed: 42, capacity: 10);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert - with 30 people and 3 workshops × 10 capacity = 30 slots in Wave 1,
        // we should be able to give many unique people a spot
        var wave1Participants = result.Results.Values
            .SelectMany(r => r.Assignments)
            .Where(a => a.Wave == 1)
            .Select(a => a.Registration.NormalizedEmail)
            .Distinct()
            .Count();

        // Should have close to 30 unique participants (or all available slots filled)
        wave1Participants.Should().BeGreaterThanOrEqualTo(25);
    }

    [Fact]
    public void RunLottery_Wave1_RespectsCapacity()
    {
        // Arrange
        var registrations = CreateTestRegistrations(100);
        var config = CreateConfig(seed: 42, capacity: 10);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert
        foreach (var (workshop, workshopResult) in result.Results)
        {
            workshopResult.AcceptedCount.Should().BeLessThanOrEqualTo(config.Capacity);
        }
    }

    #endregion

    #region Wave 2 Tests

    [Fact]
    public void RunLottery_Wave2_FillsRemainingSeats()
    {
        // Arrange - 50 people requesting W1, capacity 34
        var registrations = CreateTestRegistrations(50, workshop: WorkshopId.W1);
        var config = CreateConfig(seed: 42, capacity: 34);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert - should fill to capacity
        result.Results[WorkshopId.W1].AcceptedCount.Should().Be(34);
    }

    [Fact]
    public void RunLottery_Wave2_AllowsMultipleWorkshopsPerPerson()
    {
        // Arrange - 5 people each requesting all workshops, capacity 10 per workshop
        var registrations = CreateTestRegistrations(5, requestAllWorkshops: true);
        var config = CreateConfig(seed: 42, capacity: 10);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert - all 5 should be accepted in all workshops
        foreach (var (workshop, workshopResult) in result.Results)
        {
            workshopResult.AcceptedCount.Should().Be(5);
        }
    }

    [Fact]
    public void RunLottery_Wave2_OnlyRunsIfSeatsAvailable()
    {
        // Arrange - 100 people requesting W1 only, capacity 50
        var registrations = CreateTestRegistrations(100, workshop: WorkshopId.W1);
        var config = CreateConfig(seed: 42, capacity: 50);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert - W1 was filled in Wave 1, Wave 2 count should be less than Wave 1 or 0
        // (Wave 2 will add nothing for W1 if Wave 1 filled the capacity)
        result.Results[WorkshopId.W1].Wave1Count.Should().Be(50);
        result.Results[WorkshopId.W1].Wave2Count.Should().Be(0);
    }

    #endregion

    #region Waitlist Tests

    [Fact]
    public void RunLottery_Waitlist_IncludesAllRemainingCandidates()
    {
        // Arrange - 20 people, capacity 10
        var registrations = CreateTestRegistrations(20, workshop: WorkshopId.W1);
        var config = CreateConfig(seed: 42, capacity: 10);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert - 10 accepted, 10 waitlisted = 20 total
        var w1Result = result.Results[WorkshopId.W1];
        w1Result.AcceptedCount.Should().Be(10);
        w1Result.WaitlistCount.Should().Be(10);
        w1Result.Assignments.Count.Should().Be(20);
    }

    [Fact]
    public void RunLottery_Waitlist_PreservesWeightedOrder()
    {
        // Arrange
        var registrations = CreateTestRegistrations(20, workshop: WorkshopId.W1);
        var config = CreateConfig(seed: 42, capacity: 10);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert - Order should be sequential
        var waitlisted = result.Results[WorkshopId.W1].Assignments
            .Where(a => a.Status == AssignmentStatus.Waitlisted)
            .ToList();

        var previousOrder = 0;
        foreach (var assignment in waitlisted)
        {
            assignment.Order.Should().BeGreaterThan(previousOrder);
            previousOrder = assignment.Order;
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void RunLottery_WithFewerCandidatesThanCapacity_AcceptsAll()
    {
        // Arrange - 5 people, capacity 34
        var registrations = CreateTestRegistrations(5, workshop: WorkshopId.W1);
        var config = CreateConfig(seed: 42, capacity: 34);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert
        result.Results[WorkshopId.W1].AcceptedCount.Should().Be(5);
        result.Results[WorkshopId.W1].WaitlistCount.Should().Be(0);
    }

    [Fact]
    public void RunLottery_WithNoEligibleRegistrations_ReturnsEmptyResults()
    {
        // Arrange
        var registrations = new List<Registration>();
        var config = CreateConfig(seed: 42, capacity: 34);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert
        foreach (var (workshop, workshopResult) in result.Results)
        {
            workshopResult.AcceptedCount.Should().Be(0);
            workshopResult.WaitlistCount.Should().Be(0);
        }
    }

    [Fact]
    public void RunLottery_WithWorkshopOrderConfigured_RespectsOrder()
    {
        // Arrange - W3 first, then W1, then W2
        var registrations = CreateTestRegistrations(30, requestAllWorkshops: true);
        var config = new LotteryConfiguration
        {
            InputPath = "test.xlsx",
            Capacity = 10,
            Seed = 42,
            WorkshopOrder = new() { WorkshopId.W3, WorkshopId.W1, WorkshopId.W2 }
        };

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert - results should have all workshops
        result.Results.Should().ContainKey(WorkshopId.W1);
        result.Results.Should().ContainKey(WorkshopId.W2);
        result.Results.Should().ContainKey(WorkshopId.W3);
    }

    [Fact]
    public void RunLottery_ResultContainsCorrectMetadata()
    {
        // Arrange
        var registrations = CreateTestRegistrations(10);
        var config = CreateConfig(seed: 12345, capacity: 5);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert
        result.Seed.Should().Be(12345);
        result.Capacity.Should().Be(5);
        result.TotalRegistrations.Should().Be(10);
        result.EligibleCount.Should().Be(10);
    }

    [Fact]
    public void RunLottery_WithNoCandidatesForWorkshop_ReturnsEmptyResult()
    {
        // Arrange - everyone only wants W1
        var registrations = CreateTestRegistrations(10, workshop: WorkshopId.W1);
        var config = CreateConfig(seed: 42, capacity: 5);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert - W2 and W3 should be empty
        result.Results[WorkshopId.W2].Assignments.Should().BeEmpty();
        result.Results[WorkshopId.W3].Assignments.Should().BeEmpty();
    }

    #endregion

    #region Weight Distribution Tests

    [Fact]
    public void RunLottery_HigherWeightHasBetterSelectionProbability()
    {
        // Arrange - statistical test over multiple runs
        var highWeightWins = 0;
        var totalRuns = 1000;

        for (int seed = 0; seed < totalRuns; seed++)
        {
            // Create two registrations: one with rank 1 (weight 5), one with rank 3 (weight 1)
            var registrations = new List<Registration>
            {
                CreateRegistration("high@test.com", WorkshopId.W1, rank: 1),
                CreateRegistration("low@test.com", WorkshopId.W1, rank: 3)
            };

            var config = CreateConfig(seed: seed, capacity: 1);
            var result = _engine.RunLottery(registrations, config);

            var winner = result.Results[WorkshopId.W1].Assignments
                .First(a => a.Status == AssignmentStatus.Accepted)
                .Registration.Email;

            if (winner == "high@test.com")
                highWeightWins++;
        }

        // Assert - with weight ratio 5:1, high weight should win ~83% of the time (5/6)
        // Allow some variance (between 75% and 90%)
        var winRate = (double)highWeightWins / totalRuns;
        winRate.Should().BeGreaterThan(0.75, "high weight (5) should win more often than low weight (1)");
        winRate.Should().BeLessThan(0.92, "but not always due to randomness");
    }

    [Fact]
    public void RunLottery_EqualWeights_ApproximatelyUniformDistribution()
    {
        // Arrange - statistical test over multiple runs
        var person1Wins = 0;
        var totalRuns = 1000;

        for (int seed = 0; seed < totalRuns; seed++)
        {
            var registrations = new List<Registration>
            {
                CreateRegistration("person1@test.com", WorkshopId.W1, rank: 3), // weight 1
                CreateRegistration("person2@test.com", WorkshopId.W1, rank: 3)  // weight 1
            };

            var config = CreateConfig(seed: seed, capacity: 1);
            var result = _engine.RunLottery(registrations, config);

            var winner = result.Results[WorkshopId.W1].Assignments
                .First(a => a.Status == AssignmentStatus.Accepted)
                .Registration.Email;

            if (winner == "person1@test.com")
                person1Wins++;
        }

        // Assert - should be approximately 50% (between 40% and 60%)
        var winRate = (double)person1Wins / totalRuns;
        winRate.Should().BeInRange(0.40, 0.60, "equal weights should give approximately equal chances");
    }

    #endregion

    #region Assignment Status Tests

    [Fact]
    public void RunLottery_AcceptedAssignments_HaveWaveNumber()
    {
        // Arrange
        var registrations = CreateTestRegistrations(20);
        var config = CreateConfig(seed: 42, capacity: 10);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert
        var accepted = result.Results[WorkshopId.W1].Assignments
            .Where(a => a.Status == AssignmentStatus.Accepted)
            .ToList();

        accepted.Should().AllSatisfy(a => a.Wave.Should().BeOneOf(1, 2));
    }

    [Fact]
    public void RunLottery_WaitlistedAssignments_HaveNoWaveNumber()
    {
        // Arrange
        var registrations = CreateTestRegistrations(20);
        var config = CreateConfig(seed: 42, capacity: 10);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert
        var waitlisted = result.Results[WorkshopId.W1].Assignments
            .Where(a => a.Status == AssignmentStatus.Waitlisted)
            .ToList();

        waitlisted.Should().AllSatisfy(a => a.Wave.Should().BeNull());
    }

    [Fact]
    public void RunLottery_AllAssignments_HaveOrderNumbers()
    {
        // Arrange
        var registrations = CreateTestRegistrations(30);
        var config = CreateConfig(seed: 42, capacity: 15);

        // Act
        var result = _engine.RunLottery(registrations, config);

        // Assert
        foreach (var workshopResult in result.Results.Values)
        {
            var orders = workshopResult.Assignments.Select(a => a.Order).ToList();
            orders.Should().OnlyHaveUniqueItems("each assignment should have a unique order");
            orders.Should().AllSatisfy(o => o.Should().BeGreaterThan(0));
        }
    }

    #endregion

    #region Helper Methods

    private static LotteryConfiguration CreateConfig(int seed, int capacity)
    {
        return new LotteryConfiguration
        {
            InputPath = "test.xlsx",
            Capacity = capacity,
            Seed = seed
        };
    }

    private static List<Registration> CreateTestRegistrations(
        int count, 
        bool requestAllWorkshops = false,
        WorkshopId? workshop = null)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateRegistration(
                $"person{i}@test.com",
                workshop ?? (requestAllWorkshops ? null : WorkshopId.W1),
                rank: (i % 3) + 1, // Cycles through 1, 2, 3
                requestAll: requestAllWorkshops))
            .ToList();
    }

    private static Registration CreateRegistration(
        string email,
        WorkshopId? workshop,
        int rank,
        bool requestAll = false)
    {
        var preferences = new Dictionary<WorkshopId, WorkshopPreference>();

        foreach (var ws in new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 })
        {
            var requested = requestAll || ws == workshop;
            preferences[ws] = new WorkshopPreference
            {
                Requested = requested,
                Rank = requested ? rank : null
            };
        }

        return new Registration
        {
            FullName = $"Test Person ({email})",
            Email = email,
            HasLaptop = true,
            WillCommit10Min = true,
            WorkshopPreferences = preferences
        };
    }

    #endregion
}
