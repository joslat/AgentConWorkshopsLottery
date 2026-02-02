namespace WorkshopLottery.Tests.Services;

using FluentAssertions;
using WorkshopLottery.Models;
using WorkshopLottery.Services;
using Xunit;

/// <summary>
/// Unit tests for the RankingParser static class.
/// </summary>
public class RankingParserTests
{
    #region Basic Parsing Tests

    [Fact]
    public void ParseRankings_WithEmptyString_ReturnsEmptyDictionary()
    {
        // Act
        var result = RankingParser.ParseRankings("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseRankings_WithNull_ReturnsEmptyDictionary()
    {
        // Act
        var result = RankingParser.ParseRankings(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseRankings_WithWhitespaceOnly_ReturnsEmptyDictionary()
    {
        // Act
        var result = RankingParser.ParseRankings("   ");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Single Workshop Tests

    [Theory]
    [InlineData("Workshop 1", WorkshopId.W1)]
    [InlineData("workshop 1", WorkshopId.W1)]
    [InlineData("WORKSHOP 1", WorkshopId.W1)]
    public void ParseRankings_WithSingleWorkshop1_ReturnsRank1(string input, WorkshopId expectedId)
    {
        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().ContainKey(expectedId);
        result[expectedId].Should().Be(1);
    }

    [Theory]
    [InlineData("Workshop 2", WorkshopId.W2)]
    [InlineData("workshop 2", WorkshopId.W2)]
    public void ParseRankings_WithSingleWorkshop2_ReturnsRank1(string input, WorkshopId expectedId)
    {
        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().ContainKey(expectedId);
        result[expectedId].Should().Be(1);
    }

    [Theory]
    [InlineData("Workshop 3", WorkshopId.W3)]
    [InlineData("workshop 3", WorkshopId.W3)]
    public void ParseRankings_WithSingleWorkshop3_ReturnsRank1(string input, WorkshopId expectedId)
    {
        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().ContainKey(expectedId);
        result[expectedId].Should().Be(1);
    }

    #endregion

    #region Ordered Rankings Tests

    [Fact]
    public void ParseRankings_WithTwoWorkshops_ReturnsCorrectRanks()
    {
        // Arrange
        var input = "Workshop 1;Workshop 2";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().HaveCount(2);
        result[WorkshopId.W1].Should().Be(1); // First = Rank 1
        result[WorkshopId.W2].Should().Be(2); // Second = Rank 2
    }

    [Fact]
    public void ParseRankings_WithThreeWorkshops_ReturnsCorrectRanks()
    {
        // Arrange
        var input = "Workshop 3;Workshop 1;Workshop 2";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().HaveCount(3);
        result[WorkshopId.W3].Should().Be(1); // First = Rank 1
        result[WorkshopId.W1].Should().Be(2); // Second = Rank 2
        result[WorkshopId.W2].Should().Be(3); // Third = Rank 3
    }

    [Fact]
    public void ParseRankings_WithAllWorkshopsReversed_ReturnsCorrectRanks()
    {
        // Arrange
        var input = "Workshop 2;Workshop 3;Workshop 1";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().HaveCount(3);
        result[WorkshopId.W2].Should().Be(1);
        result[WorkshopId.W3].Should().Be(2);
        result[WorkshopId.W1].Should().Be(3);
    }

    #endregion

    #region Delimiter Tests

    [Fact]
    public void ParseRankings_WithSemicolonDelimiter_ParsesCorrectly()
    {
        // Arrange
        var input = "Workshop 1;Workshop 2";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void ParseRankings_WithSpacesAroundDelimiter_ParsesCorrectly()
    {
        // Arrange
        var input = "Workshop 1 ; Workshop 2 ; Workshop 3";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().HaveCount(3);
        result[WorkshopId.W1].Should().Be(1);
        result[WorkshopId.W2].Should().Be(2);
        result[WorkshopId.W3].Should().Be(3);
    }

    #endregion

    #region Real-World Format Tests

    [Fact]
    public void ParseRankings_WithFullWorkshopNames_ParsesCorrectly()
    {
        // Arrange - realistic MS Forms format
        var input = "Workshop 1 – Secure Coding Literacy for Vibe Coders;Workshop 2 – AI Architecture Critic";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().HaveCount(2);
        result[WorkshopId.W1].Should().Be(1);
        result[WorkshopId.W2].Should().Be(2);
    }

    [Fact]
    public void ParseRankings_WithFullThreeWorkshops_ParsesCorrectly()
    {
        // Arrange
        var input = "Workshop 2 – AI Architecture Critic;Workshop 3 – Build a Pizza Ordering Agent;Workshop 1 – Secure Coding";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().HaveCount(3);
        result[WorkshopId.W2].Should().Be(1);
        result[WorkshopId.W3].Should().Be(2);
        result[WorkshopId.W1].Should().Be(3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ParseRankings_WithDuplicateWorkshops_KeepsFirstOccurrence()
    {
        // Arrange - Workshop 1 appears twice
        var input = "Workshop 1;Workshop 2;Workshop 1";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().HaveCount(2);
        result[WorkshopId.W1].Should().Be(1); // First occurrence = Rank 1
        result[WorkshopId.W2].Should().Be(2);
    }

    [Fact]
    public void ParseRankings_WithUnknownWorkshop_IgnoresIt()
    {
        // Arrange - Workshop 99 is position 2, so W2 ends up at position 3
        var input = "Workshop 1;Workshop 99;Workshop 2";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert (unknown workshop is skipped but position is preserved)
        result.Should().HaveCount(2);
        result[WorkshopId.W1].Should().Be(1); // Position 1
        result[WorkshopId.W2].Should().Be(3); // Position 3 (not 2, because 99 is at position 2)
    }

    [Fact]
    public void ParseRankings_WithEmptySegments_IgnoresThem()
    {
        // Arrange
        var input = "Workshop 1;;;Workshop 2";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().HaveCount(2);
        result[WorkshopId.W1].Should().Be(1);
        result[WorkshopId.W2].Should().Be(2);
    }

    [Fact]
    public void ParseRankings_WithOnlyInvalidContent_ReturnsEmptyDictionary()
    {
        // Arrange
        var input = "Some random text;More random text";

        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Case Insensitivity Tests

    [Theory]
    [InlineData("workshop 1")]
    [InlineData("Workshop 1")]
    [InlineData("WORKSHOP 1")]
    [InlineData("WoRkShOp 1")]
    public void ParseRankings_WithMixedCase_ParsesCorrectly(string input)
    {
        // Act
        var result = RankingParser.ParseRankings(input);

        // Assert
        result.Should().ContainKey(WorkshopId.W1);
    }

    #endregion
}
