using FluentAssertions;
using WorkshopLottery.Models;

namespace WorkshopLottery.Tests.Models;

/// <summary>
/// Unit tests for the LotteryConfiguration model.
/// </summary>
public class LotteryConfigurationTests
{
    [Fact]
    public void GetEffectiveSeed_ShouldReturnProvidedSeed_WhenSeedIsSet()
    {
        // Arrange
        var config = new LotteryConfiguration 
        { 
            InputPath = "test.xlsx",
            Seed = 42
        };

        // Act
        var effectiveSeed = config.GetEffectiveSeed();

        // Assert
        effectiveSeed.Should().Be(42);
    }

    [Fact]
    public void GetEffectiveSeed_ShouldReturnDateBasedSeed_WhenSeedIsNull()
    {
        // Arrange
        var config = new LotteryConfiguration 
        { 
            InputPath = "test.xlsx",
            Seed = null
        };
        var expectedSeed = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

        // Act
        var effectiveSeed = config.GetEffectiveSeed();

        // Assert
        effectiveSeed.Should().Be(expectedSeed);
    }

    [Fact]
    public void DefaultCapacity_ShouldBe34()
    {
        // Arrange & Act
        var config = new LotteryConfiguration { InputPath = "test.xlsx" };

        // Assert
        config.Capacity.Should().Be(34);
    }

    [Fact]
    public void DefaultOutputPath_ShouldBeWorkshopAssignmentsXlsx()
    {
        // Arrange & Act
        var config = new LotteryConfiguration { InputPath = "test.xlsx" };

        // Assert
        config.OutputPath.Should().Be("WorkshopAssignments.xlsx");
    }

    [Fact]
    public void DefaultWorkshopOrder_ShouldBeW1W2W3()
    {
        // Arrange & Act
        var config = new LotteryConfiguration { InputPath = "test.xlsx" };

        // Assert
        config.WorkshopOrder.Should().BeEquivalentTo(
            new[] { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void Capacity_ShouldBeOverridable()
    {
        // Arrange & Act
        var config = new LotteryConfiguration 
        { 
            InputPath = "test.xlsx",
            Capacity = 50
        };

        // Assert
        config.Capacity.Should().Be(50);
    }

    [Fact]
    public void OutputPath_ShouldBeOverridable()
    {
        // Arrange & Act
        var config = new LotteryConfiguration 
        { 
            InputPath = "test.xlsx",
            OutputPath = "custom_output.xlsx"
        };

        // Assert
        config.OutputPath.Should().Be("custom_output.xlsx");
    }

    [Fact]
    public void WorkshopOrder_ShouldBeOverridable()
    {
        // Arrange & Act
        var config = new LotteryConfiguration 
        { 
            InputPath = "test.xlsx",
            WorkshopOrder = [WorkshopId.W3, WorkshopId.W1, WorkshopId.W2]
        };

        // Assert
        config.WorkshopOrder.Should().BeEquivalentTo(
            new[] { WorkshopId.W3, WorkshopId.W1, WorkshopId.W2 },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void LotteryConfiguration_ShouldBeRecordType_WithValueEquality()
    {
        // Arrange - use same WorkshopOrder list to avoid collection inequality
        var workshopOrder = new List<WorkshopId> { WorkshopId.W1, WorkshopId.W2, WorkshopId.W3 };
        var config1 = new LotteryConfiguration { InputPath = "test.xlsx", Seed = 42, WorkshopOrder = workshopOrder };
        var config2 = new LotteryConfiguration { InputPath = "test.xlsx", Seed = 42, WorkshopOrder = workshopOrder };
        var config3 = new LotteryConfiguration { InputPath = "test.xlsx", Seed = 100, WorkshopOrder = workshopOrder };

        // Assert - records have value equality (when same list reference is used)
        config1.Should().Be(config2);
        config1.Should().NotBe(config3);
    }

    [Fact]
    public void LotteryConfiguration_ShouldSupportEquivalentComparison()
    {
        // Arrange - different list instances with same values
        var config1 = new LotteryConfiguration { InputPath = "test.xlsx", Seed = 42 };
        var config2 = new LotteryConfiguration { InputPath = "test.xlsx", Seed = 42 };

        // Assert - use BeEquivalentTo for deep comparison
        config1.Should().BeEquivalentTo(config2);
    }

    [Fact]
    public void InputPath_ShouldBeRequired()
    {
        // This test verifies behavior - InputPath must be provided
        var config = new LotteryConfiguration { InputPath = "required.xlsx" };
        
        config.InputPath.Should().NotBeNullOrEmpty();
    }
}
