using FluentAssertions;
using WorkshopLottery.Models;

namespace WorkshopLottery.Tests.Models;

/// <summary>
/// Unit tests for the WorkshopPreference model.
/// </summary>
public class WorkshopPreferenceTests
{
    [Fact]
    public void Weight_ShouldReturn5_WhenRankIs1()
    {
        // Arrange
        var preference = new WorkshopPreference { Requested = true, Rank = 1 };

        // Act & Assert
        preference.Weight.Should().Be(5);
    }

    [Fact]
    public void Weight_ShouldReturn2_WhenRankIs2()
    {
        // Arrange
        var preference = new WorkshopPreference { Requested = true, Rank = 2 };

        // Act & Assert
        preference.Weight.Should().Be(2);
    }

    [Fact]
    public void Weight_ShouldReturn1_WhenRankIs3()
    {
        // Arrange
        var preference = new WorkshopPreference { Requested = true, Rank = 3 };

        // Act & Assert
        preference.Weight.Should().Be(1);
    }

    [Fact]
    public void Weight_ShouldReturn1_WhenRankIsNull()
    {
        // Arrange
        var preference = new WorkshopPreference { Requested = true, Rank = null };

        // Act & Assert
        preference.Weight.Should().Be(1);
    }

    [Fact]
    public void Weight_ShouldReturn1_WhenRankIsGreaterThan3()
    {
        // Arrange - edge case: rank 4 should still be treated as 1
        var preference = new WorkshopPreference { Requested = true, Rank = 4 };

        // Act & Assert
        preference.Weight.Should().Be(1);
    }

    [Fact]
    public void Weight_ShouldReturn1_WhenRankIsZero()
    {
        // Arrange - edge case: rank 0 should be treated as unranked
        var preference = new WorkshopPreference { Requested = true, Rank = 0 };

        // Act & Assert
        preference.Weight.Should().Be(1);
    }

    [Fact]
    public void Weight_ShouldReturn1_WhenRankIsNegative()
    {
        // Arrange - edge case: negative ranks should be treated as unranked
        var preference = new WorkshopPreference { Requested = true, Rank = -1 };

        // Act & Assert
        preference.Weight.Should().Be(1);
    }

    [Fact]
    public void DefaultPreference_ShouldNotBeRequested()
    {
        // Arrange & Act
        var preference = new WorkshopPreference();

        // Assert
        preference.Requested.Should().BeFalse();
        preference.Rank.Should().BeNull();
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    [InlineData(null, 1)]
    public void Weight_ShouldMatchExpected(int? rank, int expectedWeight)
    {
        // Arrange
        var preference = new WorkshopPreference { Rank = rank };

        // Act & Assert
        preference.Weight.Should().Be(expectedWeight);
    }

    [Fact]
    public void WorkshopPreference_ShouldBeRecordType_WithValueEquality()
    {
        // Arrange
        var pref1 = new WorkshopPreference { Requested = true, Rank = 1 };
        var pref2 = new WorkshopPreference { Requested = true, Rank = 1 };
        var pref3 = new WorkshopPreference { Requested = true, Rank = 2 };

        // Assert - records have value equality
        pref1.Should().Be(pref2);
        pref1.Should().NotBe(pref3);
    }
}
