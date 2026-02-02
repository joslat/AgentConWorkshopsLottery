namespace WorkshopLottery.Tests.Extensions;

using FluentAssertions;
using WorkshopLottery.Extensions;
using Xunit;

/// <summary>
/// Unit tests for ParsingExtensions helper methods.
/// </summary>
public class ParsingExtensionsTests
{
    #region ParseYesNo Tests

    [Theory]
    [InlineData("Yes", true)]
    [InlineData("yes", true)]
    [InlineData("YES", true)]
    [InlineData("YeS", true)]
    [InlineData("Ja", true)]
    [InlineData("ja", true)]
    [InlineData("JA", true)]
    [InlineData("Oui", true)]
    [InlineData("oui", true)]
    [InlineData("OUI", true)]
    [InlineData("Sí", true)]
    [InlineData("sí", true)]
    [InlineData("SÍ", true)]
    [InlineData("y", true)]
    [InlineData("Y", true)]
    [InlineData("si", true)]
    [InlineData("true", true)]
    [InlineData("1", true)]
    public void ParseYesNo_WithExactYesVariants_ReturnsTrue(string input, bool expected)
    {
        // Act
        var result = input.ParseYesNo();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Yes, I have a laptop")]
    [InlineData("Yes, absolutely")]
    [InlineData("yes i do")]
    [InlineData("Yes!")]
    public void ParseYesNo_WithYesPrefix_ReturnsTrue(string input)
    {
        // Act
        var result = input.ParseYesNo();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("No")]
    [InlineData("no")]
    [InlineData("NO")]
    [InlineData("Nein")]
    [InlineData("Non")]
    [InlineData("Maybe")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("n")]
    [InlineData("false")]
    public void ParseYesNo_WithNonYesValues_ReturnsFalse(string input)
    {
        // Act
        var result = input.ParseYesNo();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ParseYesNo_WithNull_ReturnsFalse()
    {
        // Act
        var result = ((string?)null).ParseYesNo();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("  Yes  ", true)]
    [InlineData("  Ja  ", true)]
    [InlineData("  No  ", false)]
    public void ParseYesNo_WithWhitespace_HandlesCorrectly(string input, bool expected)
    {
        // Act
        var result = input.ParseYesNo();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region NormalizeEmail Tests

    [Fact]
    public void NormalizeEmail_WithValidEmail_ReturnsLowerCaseTrimmed()
    {
        // Arrange
        var email = "  John.Doe@Example.COM  ";

        // Act
        var result = email.NormalizeEmail();

        // Assert
        result.Should().Be("john.doe@example.com");
    }

    [Theory]
    [InlineData("test@example.com", "test@example.com")]
    [InlineData("TEST@EXAMPLE.COM", "test@example.com")]
    [InlineData("Test@Example.Com", "test@example.com")]
    [InlineData("  test@example.com  ", "test@example.com")]
    public void NormalizeEmail_WithVariousCases_NormalizesCorrectly(string input, string expected)
    {
        // Act
        var result = input.NormalizeEmail();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NormalizeEmail_WithNull_ReturnsEmptyString()
    {
        // Act
        var result = ((string?)null).NormalizeEmail();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void NormalizeEmail_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var result = "".NormalizeEmail();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void NormalizeEmail_WithWhitespaceOnly_ReturnsEmptyString()
    {
        // Act
        var result = "   ".NormalizeEmail();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region TrimOrEmpty Tests

    [Theory]
    [InlineData("Hello", "Hello")]
    [InlineData("  Hello  ", "Hello")]
    [InlineData("Hello World", "Hello World")]
    [InlineData("  Hello World  ", "Hello World")]
    public void TrimOrEmpty_WithValidString_ReturnsTrimmed(string input, string expected)
    {
        // Act
        var result = input.TrimOrEmpty();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void TrimOrEmpty_WithNull_ReturnsEmptyString()
    {
        // Act
        var result = ((string?)null).TrimOrEmpty();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void TrimOrEmpty_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var result = "".TrimOrEmpty();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void TrimOrEmpty_WithWhitespaceOnly_ReturnsEmptyString()
    {
        // Act
        var result = "   ".TrimOrEmpty();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
