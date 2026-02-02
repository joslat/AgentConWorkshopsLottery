using FluentAssertions;
using WorkshopLottery.Models;

namespace WorkshopLottery.Tests.Models;

/// <summary>
/// Unit tests for the Registration model.
/// </summary>
public class RegistrationTests
{
    [Fact]
    public void NormalizedEmail_ShouldTrimAndLowercase()
    {
        // Arrange
        var registration = new Registration
        {
            FullName = "Test User",
            Email = "  TEST@Example.COM  "
        };

        // Act
        var normalizedEmail = registration.NormalizedEmail;

        // Assert
        normalizedEmail.Should().Be("test@example.com");
    }

    [Fact]
    public void NormalizedEmail_ShouldHandleAlreadyNormalized()
    {
        // Arrange
        var registration = new Registration
        {
            FullName = "Test User",
            Email = "test@example.com"
        };

        // Act & Assert
        registration.NormalizedEmail.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("John Doe", "john@test.com", true, true, true)]
    [InlineData("John Doe", "john@test.com", true, false, false)]  // No commit
    [InlineData("John Doe", "john@test.com", false, true, false)]  // No laptop
    [InlineData("John Doe", "", true, true, false)]                 // Empty email
    [InlineData("", "john@test.com", true, true, false)]            // Empty name
    [InlineData("  ", "john@test.com", true, true, false)]          // Whitespace name
    [InlineData("John Doe", "  ", true, true, false)]               // Whitespace email
    public void MeetsBasicRequirements_ShouldEvaluateCorrectly(
        string fullName, 
        string email, 
        bool hasLaptop, 
        bool willCommit, 
        bool expectedResult)
    {
        // Arrange
        var registration = new Registration
        {
            FullName = fullName,
            Email = email,
            HasLaptop = hasLaptop,
            WillCommit10Min = willCommit
        };

        // Act
        var result = registration.MeetsBasicRequirements();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void Disqualify_ShouldSetIsEligibleToFalse()
    {
        // Arrange
        var registration = new Registration
        {
            FullName = "Test User",
            Email = "test@test.com"
        };

        // Act
        registration.Disqualify("Test reason");

        // Assert
        registration.IsEligible.Should().BeFalse();
    }

    [Fact]
    public void Disqualify_ShouldSetDisqualificationReason()
    {
        // Arrange
        var registration = new Registration
        {
            FullName = "Test User",
            Email = "test@test.com"
        };

        // Act
        registration.Disqualify("Duplicate email");

        // Assert
        registration.DisqualificationReason.Should().Be("Duplicate email");
    }

    [Fact]
    public void NewRegistration_ShouldBeEligibleByDefault()
    {
        // Arrange & Act
        var registration = new Registration
        {
            FullName = "Test User",
            Email = "test@test.com"
        };

        // Assert
        registration.IsEligible.Should().BeTrue();
        registration.DisqualificationReason.Should().BeNull();
    }

    [Fact]
    public void NewRegistration_ShouldHaveUniqueId()
    {
        // Arrange & Act
        var reg1 = new Registration { FullName = "User 1", Email = "user1@test.com" };
        var reg2 = new Registration { FullName = "User 2", Email = "user2@test.com" };

        // Assert
        reg1.Id.Should().NotBe(Guid.Empty);
        reg2.Id.Should().NotBe(Guid.Empty);
        reg1.Id.Should().NotBe(reg2.Id);
    }

    [Fact]
    public void WorkshopPreferences_ShouldDefaultToEmptyDictionary()
    {
        // Arrange & Act
        var registration = new Registration
        {
            FullName = "Test User",
            Email = "test@test.com"
        };

        // Assert
        registration.WorkshopPreferences.Should().NotBeNull();
        registration.WorkshopPreferences.Should().BeEmpty();
    }
}
