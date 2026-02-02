using FluentAssertions;
using WorkshopLottery.Infrastructure;

namespace WorkshopLottery.Tests.Infrastructure;

/// <summary>
/// Unit tests for the ColumnMatchers fuzzy matching logic.
/// </summary>
public class ColumnMatchersTests
{
    #region Email Matcher Tests

    [Theory]
    [InlineData("Email")]
    [InlineData("email")]
    [InlineData("EMAIL")]
    [InlineData("Email address")]
    [InlineData("Your email")]
    [InlineData("email_address")]
    [InlineData("What is your email?")]
    public void EmailMatcher_ShouldMatchVariousEmailHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("Email");

        // Act & Assert
        matcher.Should().NotBeNull();
        matcher!.Matcher(header).Should().BeTrue($"'{header}' should match Email");
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Full name")]
    [InlineData("Phone")]
    public void EmailMatcher_ShouldNotMatchNonEmailHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("Email");

        // Act & Assert
        matcher!.Matcher(header).Should().BeFalse($"'{header}' should NOT match Email");
    }

    #endregion

    #region Name Matcher Tests

    [Theory]
    [InlineData("Name")]
    [InlineData("Full name")]
    [InlineData("FULL NAME")]
    [InlineData("Your name")]
    [InlineData("What is your name?")]
    [InlineData("Participant name")]
    public void NameMatcher_ShouldMatchVariousNameHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("FullName");

        // Act & Assert
        matcher.Should().NotBeNull();
        matcher!.Matcher(header).Should().BeTrue($"'{header}' should match FullName");
    }

    [Theory]
    [InlineData("Email")]
    [InlineData("Email address")]
    [InlineData("email_name")] // Has both but email takes priority due to contains
    public void NameMatcher_ShouldNotMatchEmailHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("FullName");

        // Act & Assert
        matcher!.Matcher(header).Should().BeFalse($"'{header}' should NOT match FullName (contains 'email')");
    }

    #endregion

    #region Laptop Matcher Tests

    [Theory]
    [InlineData("Laptop")]
    [InlineData("laptop")]
    [InlineData("Will you bring a laptop?")]
    [InlineData("Laptop (required for workshop)")]
    [InlineData("Do you have a laptop?")]
    public void LaptopMatcher_ShouldMatchVariousLaptopHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("Laptop");

        // Act & Assert
        matcher.Should().NotBeNull();
        matcher!.Matcher(header).Should().BeTrue($"'{header}' should match Laptop");
    }

    [Theory]
    [InlineData("Computer")]
    [InlineData("Device")]
    public void LaptopMatcher_ShouldNotMatchNonLaptopHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("Laptop");

        // Act & Assert
        matcher!.Matcher(header).Should().BeFalse($"'{header}' should NOT match Laptop");
    }

    #endregion

    #region Commit10Min Matcher Tests

    [Theory]
    [InlineData("Commit")]
    [InlineData("Will you commit to arrive early?")]
    [InlineData("10 min early")]
    [InlineData("arrive 10 minutes before")]
    [InlineData("Please commit")]
    [InlineData("Before the workshop")]
    [InlineData("Arrive early")]
    public void Commit10MinMatcher_ShouldMatchVariousHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("Commit10Min");

        // Act & Assert
        matcher.Should().NotBeNull();
        matcher!.Matcher(header).Should().BeTrue($"'{header}' should match Commit10Min");
    }

    [Theory]
    [InlineData("Workshop time")]
    [InlineData("Schedule")]
    public void Commit10MinMatcher_ShouldNotMatchUnrelatedHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("Commit10Min");

        // Act & Assert
        matcher!.Matcher(header).Should().BeFalse($"'{header}' should NOT match Commit10Min");
    }

    #endregion

    #region Workshop Matchers Tests

    [Theory]
    [InlineData("Workshop 1")]
    [InlineData("workshop 1")]
    [InlineData("Workshop 1 – Secure Coding")]
    [InlineData("Do you want Workshop 1?")]
    public void Workshop1Matcher_ShouldMatchVariousHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("RequestedW1");

        // Act & Assert
        matcher.Should().NotBeNull();
        matcher!.Matcher(header).Should().BeTrue($"'{header}' should match RequestedW1");
    }

    [Theory]
    [InlineData("Workshop 2")]
    [InlineData("workshop 2")]
    [InlineData("Workshop 2 – AI Architecture")]
    public void Workshop2Matcher_ShouldMatchVariousHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("RequestedW2");

        // Act & Assert
        matcher.Should().NotBeNull();
        matcher!.Matcher(header).Should().BeTrue($"'{header}' should match RequestedW2");
    }

    [Theory]
    [InlineData("Workshop 3")]
    [InlineData("workshop 3")]
    [InlineData("Workshop 3 – Pizza Agent")]
    public void Workshop3Matcher_ShouldMatchVariousHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("RequestedW3");

        // Act & Assert
        matcher.Should().NotBeNull();
        matcher!.Matcher(header).Should().BeTrue($"'{header}' should match RequestedW3");
    }

    [Fact]
    public void WorkshopMatchers_ShouldNotCrossMatch()
    {
        // Arrange
        var w1Matcher = ColumnMatchers.GetByFieldName("RequestedW1");
        var w2Matcher = ColumnMatchers.GetByFieldName("RequestedW2");
        var w3Matcher = ColumnMatchers.GetByFieldName("RequestedW3");

        // Act & Assert - W1 matcher should not match W2 or W3
        w1Matcher!.Matcher("Workshop 2").Should().BeFalse();
        w1Matcher.Matcher("Workshop 3").Should().BeFalse();

        // W2 matcher should not match W1 or W3
        w2Matcher!.Matcher("Workshop 1").Should().BeFalse();
        w2Matcher.Matcher("Workshop 3").Should().BeFalse();

        // W3 matcher should not match W1 or W2
        w3Matcher!.Matcher("Workshop 1").Should().BeFalse();
        w3Matcher.Matcher("Workshop 2").Should().BeFalse();
    }

    #endregion

    #region Rankings Matcher Tests

    [Theory]
    [InlineData("Rank")]
    [InlineData("Rankings")]
    [InlineData("rank your choices")]
    [InlineData("Please rank the workshops")]
    public void RankingsMatcher_ShouldMatchVariousHeaders(string header)
    {
        // Arrange
        var matcher = ColumnMatchers.GetByFieldName("Rankings");

        // Act & Assert
        matcher.Should().NotBeNull();
        matcher!.Matcher(header).Should().BeTrue($"'{header}' should match Rankings");
    }

    #endregion

    #region Required/Optional Tests

    [Fact]
    public void RequiredMatchers_ShouldBeFour()
    {
        // Act
        var required = ColumnMatchers.Required.ToList();

        // Assert
        required.Should().HaveCount(4);
        required.Select(m => m.FieldName).Should().BeEquivalentTo(
            ["Email", "FullName", "Laptop", "Commit10Min"]);
    }

    [Fact]
    public void OptionalMatchers_ShouldBeFour()
    {
        // Act
        var optional = ColumnMatchers.Optional.ToList();

        // Assert
        optional.Should().HaveCount(4);
        optional.Select(m => m.FieldName).Should().BeEquivalentTo(
            ["RequestedW1", "RequestedW2", "RequestedW3", "Rankings"]);
    }

    [Fact]
    public void AllMatchers_ShouldHaveEight()
    {
        // Assert
        ColumnMatchers.All.Should().HaveCount(8);
    }

    #endregion

    #region GetByFieldName Tests

    [Theory]
    [InlineData("Email")]
    [InlineData("FullName")]
    [InlineData("Laptop")]
    [InlineData("Commit10Min")]
    [InlineData("RequestedW1")]
    [InlineData("RequestedW2")]
    [InlineData("RequestedW3")]
    [InlineData("Rankings")]
    public void GetByFieldName_ShouldReturnMatcherForValidNames(string fieldName)
    {
        // Act
        var matcher = ColumnMatchers.GetByFieldName(fieldName);

        // Assert
        matcher.Should().NotBeNull();
        matcher!.FieldName.Should().Be(fieldName);
    }

    [Fact]
    public void GetByFieldName_ShouldReturnNullForInvalidName()
    {
        // Act
        var matcher = ColumnMatchers.GetByFieldName("InvalidFieldName");

        // Assert
        matcher.Should().BeNull();
    }

    [Fact]
    public void GetByFieldName_ShouldBeCaseInsensitive()
    {
        // Act
        var matcher1 = ColumnMatchers.GetByFieldName("email");
        var matcher2 = ColumnMatchers.GetByFieldName("EMAIL");
        var matcher3 = ColumnMatchers.GetByFieldName("Email");

        // Assert
        matcher1.Should().NotBeNull();
        matcher2.Should().NotBeNull();
        matcher3.Should().NotBeNull();
        matcher1!.FieldName.Should().Be(matcher2!.FieldName).And.Be(matcher3!.FieldName);
    }

    #endregion
}
