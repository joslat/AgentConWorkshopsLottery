namespace WorkshopLottery.Tests.Services;

using FluentAssertions;
using WorkshopLottery.Models;
using WorkshopLottery.Services;
using Xunit;

/// <summary>
/// Unit tests for the ValidationService.
/// </summary>
public class ValidationServiceTests
{
    private readonly ValidationService _service = new();

    #region ValidateAndFilter Basic Tests

    [Fact]
    public void ValidateAndFilter_WithEmptyList_ReturnsEmptyResult()
    {
        // Act
        var result = _service.ValidateAndFilter(new List<RawRegistration>());

        // Assert
        result.AllRegistrations.Should().BeEmpty();
        result.EligibleRegistrations.Should().BeEmpty();
        result.DisqualifiedRegistrations.Should().BeEmpty();
    }

    [Fact]
    public void ValidateAndFilter_WithValidRegistration_ReturnsEligible()
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com");
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.AllRegistrations.Should().HaveCount(1);
        result.EligibleRegistrations.Should().HaveCount(1);
        result.DisqualifiedRegistrations.Should().BeEmpty();
    }

    #endregion

    #region Eligibility - Name Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ValidateAndFilter_WithMissingName_IsNotEligible(string? name)
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com");
        raw = raw with { FullName = name };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.AllRegistrations.Should().HaveCount(1);
        result.EligibleRegistrations.Should().BeEmpty();
        result.DisqualifiedRegistrations.Should().HaveCount(1);
    }

    #endregion

    #region Eligibility - Email Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ValidateAndFilter_WithMissingEmail_IsNotEligible(string? email)
    {
        // Arrange
        var raw = CreateValidRawRegistration("valid@example.com");
        raw = raw with { Email = email };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.AllRegistrations.Should().HaveCount(1);
        result.EligibleRegistrations.Should().BeEmpty();
        result.DisqualifiedRegistrations.Should().HaveCount(1);
    }

    #endregion

    #region Eligibility - Laptop Tests

    [Theory]
    [InlineData("No")]
    [InlineData("Nein")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateAndFilter_WithNoLaptop_IsNotEligible(string? laptopResponse)
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com");
        raw = raw with { LaptopResponse = laptopResponse };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.AllRegistrations.Should().HaveCount(1);
        result.EligibleRegistrations.Should().BeEmpty();
        result.DisqualifiedRegistrations.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("Yes")]
    [InlineData("yes")]
    [InlineData("Ja")]
    [InlineData("ja")]
    [InlineData("Yes, I have a laptop")]
    public void ValidateAndFilter_WithLaptop_IsEligible(string laptopResponse)
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com");
        raw = raw with { LaptopResponse = laptopResponse };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.EligibleRegistrations.Should().HaveCount(1);
    }

    #endregion

    #region Eligibility - Commitment Tests

    [Theory]
    [InlineData("No")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateAndFilter_WithNoCommitment_IsNotEligible(string? commitResponse)
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com");
        raw = raw with { Commit10MinResponse = commitResponse };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.AllRegistrations.Should().HaveCount(1);
        result.EligibleRegistrations.Should().BeEmpty();
        result.DisqualifiedRegistrations.Should().HaveCount(1);
    }

    #endregion

    #region Duplicate Detection Tests

    [Fact]
    public void ValidateAndFilter_WithDuplicateEmails_DisqualifiesBoth()
    {
        // Arrange
        var raw1 = CreateValidRawRegistration("duplicate@example.com") with { FullName = "Person One" };
        var raw2 = CreateValidRawRegistration("duplicate@example.com") with { FullName = "Person Two" };
        var rawList = new List<RawRegistration> { raw1, raw2 };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.AllRegistrations.Should().HaveCount(2);
        result.EligibleRegistrations.Should().BeEmpty();
        result.DisqualifiedRegistrations.Should().HaveCount(2);
    }

    [Fact]
    public void ValidateAndFilter_WithDuplicateEmails_CaseInsensitive()
    {
        // Arrange
        var raw1 = CreateValidRawRegistration("Test@Example.com") with { FullName = "Person One" };
        var raw2 = CreateValidRawRegistration("test@example.com") with { FullName = "Person Two" };
        var rawList = new List<RawRegistration> { raw1, raw2 };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.AllRegistrations.Should().HaveCount(2);
        result.EligibleRegistrations.Should().BeEmpty();
        result.DisqualifiedRegistrations.Should().HaveCount(2);
    }

    [Fact]
    public void ValidateAndFilter_WithTriplicateEmails_DisqualifiesAll()
    {
        // Arrange
        var raw1 = CreateValidRawRegistration("multi@example.com") with { FullName = "Person One" };
        var raw2 = CreateValidRawRegistration("MULTI@example.com") with { FullName = "Person Two" };
        var raw3 = CreateValidRawRegistration("multi@EXAMPLE.com") with { FullName = "Person Three" };
        var rawList = new List<RawRegistration> { raw1, raw2, raw3 };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.AllRegistrations.Should().HaveCount(3);
        result.EligibleRegistrations.Should().BeEmpty();
        result.DisqualifiedRegistrations.Should().HaveCount(3);
    }

    [Fact]
    public void ValidateAndFilter_WithUniqueEmails_AllEligible()
    {
        // Arrange
        var raw1 = CreateValidRawRegistration("person1@example.com");
        var raw2 = CreateValidRawRegistration("person2@example.com");
        var raw3 = CreateValidRawRegistration("person3@example.com");
        var rawList = new List<RawRegistration> { raw1, raw2, raw3 };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.AllRegistrations.Should().HaveCount(3);
        result.EligibleRegistrations.Should().HaveCount(3);
        result.DisqualifiedRegistrations.Should().BeEmpty();
    }

    #endregion

    #region Workshop Preference Parsing Tests

    [Fact]
    public void ValidateAndFilter_WithRankedWorkshops_SetsCorrectWeights()
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com") with
        {
            RequestedW1Response = "Yes",
            RequestedW2Response = "Yes",
            RequestedW3Response = "",
            RankingsResponse = "Workshop 1;Workshop 2"
        };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.EligibleRegistrations.Should().HaveCount(1);
        var registration = result.EligibleRegistrations[0];

        registration.WorkshopPreferences[WorkshopId.W1].Requested.Should().BeTrue();
        registration.WorkshopPreferences[WorkshopId.W1].Rank.Should().Be(1);
        registration.WorkshopPreferences[WorkshopId.W1].Weight.Should().Be(5); // Rank 1 = Weight 5

        registration.WorkshopPreferences[WorkshopId.W2].Requested.Should().BeTrue();
        registration.WorkshopPreferences[WorkshopId.W2].Rank.Should().Be(2);
        registration.WorkshopPreferences[WorkshopId.W2].Weight.Should().Be(2); // Rank 2 = Weight 2

        registration.WorkshopPreferences[WorkshopId.W3].Requested.Should().BeFalse();
    }

    [Fact]
    public void ValidateAndFilter_WithRequestedButNotRanked_GetsDefaultRank3()
    {
        // Arrange - W3 requested but rankings only list W1 and W2
        var raw = CreateValidRawRegistration("test@example.com") with
        {
            RequestedW1Response = "Yes",
            RequestedW2Response = "Yes",
            RequestedW3Response = "Yes",
            RankingsResponse = "Workshop 1;Workshop 2"
        };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        var registration = result.EligibleRegistrations[0];

        registration.WorkshopPreferences[WorkshopId.W3].Requested.Should().BeTrue();
        registration.WorkshopPreferences[WorkshopId.W3].Rank.Should().Be(3); // Default rank for requested but unranked
        registration.WorkshopPreferences[WorkshopId.W3].Weight.Should().Be(1); // Rank 3 = Weight 1
    }

    [Fact]
    public void ValidateAndFilter_WithAllThreeRanked_SetsCorrectWeights()
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com") with
        {
            RequestedW1Response = "Yes",
            RequestedW2Response = "Yes",
            RequestedW3Response = "Yes",
            RankingsResponse = "Workshop 3;Workshop 1;Workshop 2"
        };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        var registration = result.EligibleRegistrations[0];

        registration.WorkshopPreferences[WorkshopId.W1].Rank.Should().Be(2);
        registration.WorkshopPreferences[WorkshopId.W1].Weight.Should().Be(2);

        registration.WorkshopPreferences[WorkshopId.W2].Rank.Should().Be(3);
        registration.WorkshopPreferences[WorkshopId.W2].Weight.Should().Be(1);

        registration.WorkshopPreferences[WorkshopId.W3].Rank.Should().Be(1);
        registration.WorkshopPreferences[WorkshopId.W3].Weight.Should().Be(5);
    }

    #endregion

    #region Name Trimming Tests

    [Fact]
    public void ValidateAndFilter_TrimsNameWhitespace()
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com") with
        {
            FullName = "  John Doe  "
        };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.EligibleRegistrations[0].FullName.Should().Be("John Doe");
    }

    #endregion

    #region IsEligible Property Tests

    [Fact]
    public void ValidateAndFilter_EligibleRegistration_HasIsEligibleTrue()
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com");
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.EligibleRegistrations[0].IsEligible.Should().BeTrue();
    }

    [Fact]
    public void ValidateAndFilter_DisqualifiedRegistration_HasIsEligibleFalse()
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com") with { LaptopResponse = "No" };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.DisqualifiedRegistrations[0].IsEligible.Should().BeFalse();
    }

    #endregion

    #region Disqualification Reasons Tests

    [Fact]
    public void ValidateAndFilter_DisqualifiedRegistration_HasDisqualificationReason()
    {
        // Arrange
        var raw = CreateValidRawRegistration("test@example.com") with { LaptopResponse = "No" };
        var rawList = new List<RawRegistration> { raw };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.DisqualifiedRegistrations[0].DisqualificationReason.Should().Be("No laptop");
        result.DisqualificationReasons.Should().ContainKey("No laptop");
        result.DisqualificationReasons["No laptop"].Should().Be(1);
    }

    [Fact]
    public void ValidateAndFilter_DuplicatesDisqualified_HasDuplicateReason()
    {
        // Arrange
        var raw1 = CreateValidRawRegistration("dup@example.com") with { FullName = "Person One" };
        var raw2 = CreateValidRawRegistration("dup@example.com") with { FullName = "Person Two" };
        var rawList = new List<RawRegistration> { raw1, raw2 };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.DisqualifiedRegistrations.Should().AllSatisfy(r => 
            r.DisqualificationReason.Should().Be("Duplicate email"));
        result.DisqualificationReasons["Duplicate email"].Should().Be(2);
    }

    #endregion

    #region Combined Scenarios

    [Fact]
    public void ValidateAndFilter_MixedEligibilityAndDuplicates_CategorizesCorrectly()
    {
        // Arrange
        var eligible1 = CreateValidRawRegistration("eligible1@example.com");
        var eligible2 = CreateValidRawRegistration("eligible2@example.com");
        var noLaptop = CreateValidRawRegistration("nolaptop@example.com") with { LaptopResponse = "No" };
        var duplicate1 = CreateValidRawRegistration("dup@example.com") with { FullName = "Dup One" };
        var duplicate2 = CreateValidRawRegistration("dup@example.com") with { FullName = "Dup Two" };

        var rawList = new List<RawRegistration> { eligible1, eligible2, noLaptop, duplicate1, duplicate2 };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.AllRegistrations.Should().HaveCount(5);
        result.EligibleRegistrations.Should().HaveCount(2);
        result.DisqualifiedRegistrations.Should().HaveCount(3); // noLaptop + 2 duplicates
    }

    #endregion

    #region Count Properties Tests

    [Fact]
    public void ValidateAndFilter_CountProperties_AreCorrect()
    {
        // Arrange
        var valid1 = CreateValidRawRegistration("valid1@example.com");
        var valid2 = CreateValidRawRegistration("valid2@example.com");
        var invalid = CreateValidRawRegistration("invalid@example.com") with { LaptopResponse = "No" };
        var rawList = new List<RawRegistration> { valid1, valid2, invalid };

        // Act
        var result = _service.ValidateAndFilter(rawList);

        // Assert
        result.TotalCount.Should().Be(3);
        result.EligibleCount.Should().Be(2);
        result.DisqualifiedCount.Should().Be(1);
    }

    #endregion

    #region Helper Methods

    private static RawRegistration CreateValidRawRegistration(string email, string name = "Test Person")
    {
        return new RawRegistration
        {
            RowNumber = 1,
            Email = email,
            FullName = name,
            LaptopResponse = "Yes",
            Commit10MinResponse = "Yes",
            RequestedW1Response = "Yes",
            RequestedW2Response = "",
            RequestedW3Response = "",
            RankingsResponse = "Workshop 1"
        };
    }

    #endregion
}
