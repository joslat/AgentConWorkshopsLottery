using FluentAssertions;
using WorkshopLottery.Models;

namespace WorkshopLottery.Tests.Models;

/// <summary>
/// Unit tests for the LotteryResult model.
/// </summary>
public class LotteryResultTests
{
    [Fact]
    public void NewLotteryResult_ShouldHaveEmptyResults()
    {
        // Arrange & Act
        var result = new LotteryResult
        {
            Seed = 42,
            Capacity = 34
        };

        // Assert
        result.Results.Should().NotBeNull();
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public void LotteryResult_ShouldStoreSeed()
    {
        // Arrange & Act
        var result = new LotteryResult
        {
            Seed = 20260202,
            Capacity = 34
        };

        // Assert
        result.Seed.Should().Be(20260202);
    }

    [Fact]
    public void LotteryResult_ShouldStoreCapacity()
    {
        // Arrange & Act
        var result = new LotteryResult
        {
            Seed = 42,
            Capacity = 50
        };

        // Assert
        result.Capacity.Should().Be(50);
    }

    [Fact]
    public void LotteryResult_ShouldStoreWorkshopResults()
    {
        // Arrange
        var workshopResult = new WorkshopResult { WorkshopId = WorkshopId.W1 };

        // Act
        var result = new LotteryResult
        {
            Seed = 42,
            Capacity = 34,
            Results = new Dictionary<WorkshopId, WorkshopResult>
            {
                { WorkshopId.W1, workshopResult }
            }
        };

        // Assert
        result.Results.Should().ContainKey(WorkshopId.W1);
        result.Results[WorkshopId.W1].Should().Be(workshopResult);
    }

    [Fact]
    public void LotteryResult_ShouldStoreStatistics()
    {
        // Arrange & Act
        var result = new LotteryResult
        {
            Seed = 42,
            Capacity = 34,
            TotalRegistrations = 150,
            EligibleCount = 120,
            DisqualifiedCount = 30,
            DisqualificationReasons = new Dictionary<string, int>
            {
                { "Missing laptop", 10 },
                { "Won't commit", 8 },
                { "Duplicate email", 12 }
            }
        };

        // Assert
        result.TotalRegistrations.Should().Be(150);
        result.EligibleCount.Should().Be(120);
        result.DisqualifiedCount.Should().Be(30);
        result.DisqualificationReasons.Should().HaveCount(3);
        result.DisqualificationReasons["Missing laptop"].Should().Be(10);
    }

    [Fact]
    public void DisqualificationReasons_ShouldDefaultToEmptyDictionary()
    {
        // Arrange & Act
        var result = new LotteryResult
        {
            Seed = 42,
            Capacity = 34
        };

        // Assert
        result.DisqualificationReasons.Should().NotBeNull();
        result.DisqualificationReasons.Should().BeEmpty();
    }

    [Fact]
    public void LotteryResult_ShouldStoreAllThreeWorkshops()
    {
        // Arrange & Act
        var result = new LotteryResult
        {
            Seed = 42,
            Capacity = 34,
            Results = new Dictionary<WorkshopId, WorkshopResult>
            {
                { WorkshopId.W1, new WorkshopResult { WorkshopId = WorkshopId.W1 } },
                { WorkshopId.W2, new WorkshopResult { WorkshopId = WorkshopId.W2 } },
                { WorkshopId.W3, new WorkshopResult { WorkshopId = WorkshopId.W3 } }
            }
        };

        // Assert
        result.Results.Should().HaveCount(3);
        result.Results.Should().ContainKeys(WorkshopId.W1, WorkshopId.W2, WorkshopId.W3);
    }
}
