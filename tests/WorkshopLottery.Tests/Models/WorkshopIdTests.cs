using FluentAssertions;
using WorkshopLottery.Models;

namespace WorkshopLottery.Tests.Models;

/// <summary>
/// Unit tests for the WorkshopId enum.
/// </summary>
public class WorkshopIdTests
{
    [Fact]
    public void WorkshopId_ShouldHaveCorrectIntValues()
    {
        // Assert
        ((int)WorkshopId.W1).Should().Be(1);
        ((int)WorkshopId.W2).Should().Be(2);
        ((int)WorkshopId.W3).Should().Be(3);
    }

    [Fact]
    public void WorkshopId_ShouldHaveThreeValues()
    {
        // Arrange
        var values = Enum.GetValues<WorkshopId>();

        // Assert
        values.Should().HaveCount(3);
    }

    [Fact]
    public void WorkshopId_ShouldBeConvertibleFromInt()
    {
        // Act
        var w1 = (WorkshopId)1;
        var w2 = (WorkshopId)2;
        var w3 = (WorkshopId)3;

        // Assert
        w1.Should().Be(WorkshopId.W1);
        w2.Should().Be(WorkshopId.W2);
        w3.Should().Be(WorkshopId.W3);
    }

    [Fact]
    public void WorkshopId_ShouldHaveCorrectStringRepresentation()
    {
        // Assert
        WorkshopId.W1.ToString().Should().Be("W1");
        WorkshopId.W2.ToString().Should().Be("W2");
        WorkshopId.W3.ToString().Should().Be("W3");
    }

    [Theory]
    [InlineData(WorkshopId.W1, "W1")]
    [InlineData(WorkshopId.W2, "W2")]
    [InlineData(WorkshopId.W3, "W3")]
    public void WorkshopId_ToString_ShouldMatchExpected(WorkshopId id, string expected)
    {
        // Act & Assert
        id.ToString().Should().Be(expected);
    }
}
