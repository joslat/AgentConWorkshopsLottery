using FluentAssertions;
using WorkshopLottery.Models;

namespace WorkshopLottery.Tests.Models;

/// <summary>
/// Unit tests for the RawRegistration model.
/// </summary>
public class RawRegistrationTests
{
    [Fact]
    public void RawRegistration_ShouldStoreAllProperties()
    {
        // Arrange & Act
        var registration = new RawRegistration
        {
            RowNumber = 5,
            FullName = "John Doe",
            Email = "john@example.com",
            LaptopResponse = "Yes",
            Commit10MinResponse = "Yes, I commit",
            RequestedW1Response = "Yes",
            RequestedW2Response = "No",
            RequestedW3Response = null,
            RankingsResponse = "Workshop 1;Workshop 2"
        };

        // Assert
        registration.RowNumber.Should().Be(5);
        registration.FullName.Should().Be("John Doe");
        registration.Email.Should().Be("john@example.com");
        registration.LaptopResponse.Should().Be("Yes");
        registration.Commit10MinResponse.Should().Be("Yes, I commit");
        registration.RequestedW1Response.Should().Be("Yes");
        registration.RequestedW2Response.Should().Be("No");
        registration.RequestedW3Response.Should().BeNull();
        registration.RankingsResponse.Should().Be("Workshop 1;Workshop 2");
    }

    [Fact]
    public void RawRegistration_ShouldDefaultToNullableStrings()
    {
        // Arrange & Act
        var registration = new RawRegistration { RowNumber = 1 };

        // Assert
        registration.FullName.Should().BeNull();
        registration.Email.Should().BeNull();
        registration.LaptopResponse.Should().BeNull();
        registration.Commit10MinResponse.Should().BeNull();
        registration.RequestedW1Response.Should().BeNull();
        registration.RequestedW2Response.Should().BeNull();
        registration.RequestedW3Response.Should().BeNull();
        registration.RankingsResponse.Should().BeNull();
    }

    [Fact]
    public void RawRegistration_ShouldBeRecordType_WithValueEquality()
    {
        // Arrange
        var reg1 = new RawRegistration
        {
            RowNumber = 1,
            FullName = "John",
            Email = "john@test.com"
        };
        var reg2 = new RawRegistration
        {
            RowNumber = 1,
            FullName = "John",
            Email = "john@test.com"
        };
        var reg3 = new RawRegistration
        {
            RowNumber = 2,
            FullName = "John",
            Email = "john@test.com"
        };

        // Assert
        reg1.Should().Be(reg2);
        reg1.Should().NotBe(reg3);
    }
}
